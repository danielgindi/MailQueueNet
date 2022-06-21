using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MailQueueNet.Service.Utilities;

namespace MailQueueNet.Service.Core
{
    public class Coordinator
    {
        public Coordinator(ILogger<Coordinator> logger, IConfiguration configuration)
        {
            _Logger = logger;
            _Configuration = configuration;
            _Settings = SettingsController.GetSettings(_Configuration);
            _MailSettings = SettingsController.GetMailSettings(_Configuration);
        }

        private ILogger _Logger;
        private IConfiguration _Configuration;
        private Grpc.Settings _Settings;
        private Grpc.MailSettings _MailSettings;

        #region Private threading vars

        private readonly object _actionMonitor = new object();
        private readonly object _failedFnLock = new object();
        private int _concurrentWorkers = 0;

        #endregion

        #region Private vars

        private List<string> _fileNameList;
        private ConcurrentDictionary<string, bool> _sendingFileNames;
        private Dictionary<string, int> _failedFileNameCounter;

        #endregion

        #region Main loop

        public async Task Run(CancellationToken cancellationToken)
        {
            _fileNameList = new List<string>();
            _sendingFileNames = new ConcurrentDictionary<string, bool>();
            _failedFileNameCounter = new Dictionary<string, int>();

            cancellationToken.Register(() =>
            {
                lock (_actionMonitor)
                {
                    Monitor.Pulse(_actionMonitor);
                }
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                while (!ThereIsAFreeWorker && !cancellationToken.IsCancellationRequested)
                {
                    _Logger?.LogTrace("Waiting for a free worker...");

                    lock (_actionMonitor)
                    {
                        // Did the looping condition change by now?
                        if (ThereIsAFreeWorker || cancellationToken.IsCancellationRequested) break;

                        // Lock for an hour. Any mail sent or worker getting freed, will release it.
                        Monitor.Wait(_actionMonitor, 60 * 60 * 1000);
                    }
                }

                _Logger?.LogTrace("A worker is available");

                if (_fileNameList.Count == 0)
                {
                    string queuePath = _Settings.QueueFolder;
                    try { queuePath = Files.MapPath(queuePath); }
                    catch { }

                    try
                    {
                        _fileNameList = new List<string>(Directory.GetFiles(queuePath, "*.mail"));
                        _fileNameList.Sort();
                    }
                    catch
                    {
                        // Your QUEUE folder is inaccessible...
                    }
                }

                if (_fileNameList.Count == 0)
                {
                    _Logger?.LogDebug("There is no mail in queue");

                    lock (_actionMonitor)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        _Logger?.LogDebug("Waiting for mail to get in the queue...");

                        Monitor.Wait(_actionMonitor, (int)(_Settings.SecondsUntilFolderRefresh * 1000f));
                    }
                }

                string nextFileName = null;

                if (_fileNameList.Count > 0)
                {
                    _Logger?.LogDebug("Picking a mail from the queue...");

                    for (var i = 0; i < _fileNameList.Count; i++)
                    {
                        if (!_sendingFileNames.ContainsKey(_fileNameList[i]))
                        {
                            nextFileName = _fileNameList[i];
                            _fileNameList.RemoveAt(i);
                            break;
                        }
                    }

                    if (nextFileName == null)
                    {
                        _Logger?.LogDebug("Picked no mail from the queue");
                    }
                    else
                    {
                        _Logger?.LogDebug($"Picked mail named {nextFileName} from the queue");
                    }
                }

                if (nextFileName != null)
                {
                    _sendingFileNames[nextFileName] = true;

                    await SendMailAsync(nextFileName);
                }
                else
                {
                    lock (_actionMonitor)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        _Logger?.LogTrace("Waiting for changes in the queue or workers...");

                        Monitor.Wait(_actionMonitor, (int)(1 * 1000f));
                    }
                }
            }

            // Wait for all workers to finish
            while (_concurrentWorkers > 0)
            {
                _Logger?.LogDebug("Waiting for workers to finish...");

                lock (_actionMonitor)
                {
                    Monitor.Wait(_actionMonitor, 1000);
                }
            }

            _Logger?.LogInformation("Done.");
        }

        #endregion

        #region Worker task

        public bool ThereIsAFreeWorker
        {
            get
            {
                return _Settings.MaximumConcurrentWorkers <= 0 ||
                    _concurrentWorkers < _Settings.MaximumConcurrentWorkers;
            }
        }

        private async Task SendMailAsync(string fileName)
        {
            bool workerInUse = false;

            Grpc.MailMessageWithSettings transportMessage = null;
            Grpc.MailSettings mailSettings = null;
            System.Net.Mail.MailMessage message = null;

            try
            {
                _Logger?.LogTrace("Reading mail from " + fileName);

                try
                {
                    transportMessage = ReadMailFromFile(fileName);
                    mailSettings = transportMessage.Settings;
                    message = transportMessage.Message.ToSystemType();
                }
                catch (FileNotFoundException)
                {
                    _Logger?.LogWarning($"Failed reading {fileName}, file not found.");
                    
                    message?.Dispose();
                    MarkFailed(fileName, message);
                    return;
                }
                
                Interlocked.Increment(ref _concurrentWorkers);
                workerInUse = true;

                var logDesc = $"To={message.To}, CC={message.CC}, Bcc={message.Bcc}";

                _Logger?.LogDebug($"Sending {fileName} task to worker ({logDesc})");

                if (mailSettings == null || mailSettings.IsEmpty())
                {
                    mailSettings = _MailSettings;
                }
                
                var success = await SenderFactory.SendMailAsync(message, mailSettings);

                if (!success)
                {
                    _Logger?.LogWarning($"No mail server name, skipping {fileName} ({logDesc})");

                    message?.Dispose();
                    MarkSkipped(fileName);
                }
                else
                {
                    _Logger?.LogInformation($"Sent mail for {fileName} ({logDesc})");

                    MarkSent(fileName, message);
                }
                
                _Logger?.LogTrace($"Releasing worker from {fileName} task ({logDesc})");

                // Task ended, decrement counter and pulse to the Coordinator thread
                Interlocked.Decrement(ref _concurrentWorkers);
                workerInUse = false;

                lock (_actionMonitor)
                {
                    Monitor.Pulse(_actionMonitor);
                }
            }
            catch (Exception ex)
            {
                _Logger?.LogError(ex, $"Exception thrown for {fileName}");

                _Logger?.LogWarning($"Task failed for {fileName}");

                if (workerInUse)
                {
                    // Decrement counter and pulse to the Coordinator thread
                    Interlocked.Decrement(ref _concurrentWorkers);
                }

                try { MarkFailed(fileName, message); }
                catch { }

                lock (_actionMonitor)
                {
                    Monitor.Pulse(_actionMonitor);
                }
            }
        }

        private void MarkFailed(string fileName, System.Net.Mail.MailMessage message)
        {
            var filesToDelete = message?.Attachments
                ?.Where(x => x is AttachmentEx xx && xx.ShouldDeleteFile && x.ContentStream is FileStream fs)
                ?.Select(x => ((FileStream)x.ContentStream).Name)
                ?.ToList();

            message?.Dispose();

            bool shouldRemoveFile = false;
            lock (_failedFnLock)
            {
                if (_failedFileNameCounter.ContainsKey(fileName))
                {
                    _failedFileNameCounter[fileName]++;
                }
                else
                {
                    _failedFileNameCounter[fileName] = 1;
                }

                if (_failedFileNameCounter[fileName] >= _Settings.MaximumFailureRetries)
                {
                    shouldRemoveFile = true;
                }
            }

            if (shouldRemoveFile)
            {
                string failedPath = _Settings.FailedFolder;
                try { failedPath = Files.MapPath(failedPath); }
                catch { }

                string file = Path.Combine(failedPath, Path.GetFileName(fileName));
                try
                {
                    File.Move(fileName, file);
                }
                catch
                {
                    // Try a random file name
                    try
                    {
                        file = Path.Combine(failedPath, DateTime.Now.ToString(@"yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString(@"N") + @".mail");
                        File.Move(fileName, file);
                    }
                    catch
                    {
                        // No choice left, lose it, to get it out of the system
                        try { File.Delete(fileName); }
                        catch { }
                    }
                }

                lock (_failedFnLock)
                {
                    _failedFileNameCounter.Remove(fileName);
                }

                if (filesToDelete != null)
                {
                    foreach (var fn in filesToDelete)
                    {
                        try
                        {
                            File.Delete(fn);
                        }
                        catch { }
                    }
                }
            }

            _sendingFileNames.TryRemove(fileName, out _);
        }

        private void MarkSent(string fileName, System.Net.Mail.MailMessage message)
        {
            var filesToDelete = message?.Attachments
                ?.Where(x => x is AttachmentEx xx && xx.ShouldDeleteFile && x.ContentStream is FileStream fs)
                ?.Select(x => ((FileStream)x.ContentStream).Name)
                ?.ToList();

            message?.Dispose();

            try { File.Delete(fileName); }
            catch { }

            _sendingFileNames.TryRemove(fileName, out _);

            if (filesToDelete != null)
            {
                foreach (var fn in filesToDelete)
                {
                    try
                    {
                        File.Delete(fn);
                    }
                    catch { }
                }
            }
        }

        private void MarkSkipped(string fileName)
        {
            // This is a file that has not failed, but was not sent. This can be when no SMTP server is specified at all, and we are waiting for settings.
            // So do nothing. This file is not in the cached list, and we will only reach it on the next round and try again.

            _sendingFileNames.TryRemove(fileName, out _);
        }

        #endregion

        #region Public methods

        public void RefreshSettings()
        {
            _Settings = SettingsController.GetSettings(_Configuration);
            _MailSettings = SettingsController.GetMailSettings(_Configuration);
        }

        private int mailIdCounter = 0;

        public void AddMail(Grpc.MailMessage message, Grpc.MailSettings settings = null)
        {
            AddMail(new Grpc.MailMessageWithSettings { Message = message, Settings = settings });
        }

        public void AddMail(Grpc.MailMessageWithSettings message)
        {
            string tempPath = Files.CreateEmptyTempFile();
            if (WriteMailToFile(message, tempPath))
            {
                string queuePath = _Settings.QueueFolder;
                try { queuePath = Files.MapPath(queuePath); }
                catch { }

                bool success = false;
                string destPath = Path.Combine(queuePath, DateTime.Now.ToString(@"yyyyMMddHHmmss") + "_" + Interlocked.Increment(ref mailIdCounter).ToString().PadLeft(8, '0') + ".mail");
                while (true)
                {
                    var originalSendingState = _sendingFileNames.ContainsKey(destPath);

                    if (File.Exists(destPath))
                    {
                        destPath = Path.Combine(queuePath, DateTime.Now.ToString(@"yyyyMMddHHmmss") + "_" + Interlocked.Increment(ref mailIdCounter).ToString().PadLeft(8, '0') + ".mail");
                        continue;
                    }
                    
                    try
                    {
                        _sendingFileNames[destPath] = true;
                        File.Move(tempPath, destPath, false);
                        success = true;
                        break;
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        _Logger?.LogError(ex, $"Exception thrown for AddMail");
                        break;
                    }
                    catch (PathTooLongException ex)
                    {
                        _Logger?.LogError(ex, $"Exception thrown for AddMail");
                        break;
                    }
                    catch (FileNotFoundException ex)
                    {
                        _Logger?.LogError(ex, $"Exception thrown for AddMail");
                        break;
                    }
                    catch (IOException ex)
                    {
                        _Logger?.LogError(ex, $"Exception thrown for AddMail");
                        break;
                    }
                    finally
                    {
                        if (!originalSendingState)
                            _sendingFileNames.TryRemove(destPath, out originalSendingState);
                    }
                }

                if (success)
                {
                    ContinueSendingEmails();
                }
            }
        }

        public void ContinueSendingEmails()
        {
            lock (_actionMonitor)
            {
                Monitor.Pulse(_actionMonitor);
            }
        }

        #endregion

        #region Message to file I/O

        public static Grpc.MailMessageWithSettings ReadMailFromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
            using (var streamReader = new Google.Protobuf.CodedInputStream(stream))
            {
                return Grpc.MailMessageWithSettings.Parser.ParseFrom(streamReader);
            }
        }

        public static bool WriteMailToFile(Grpc.MailMessageWithSettings message, string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                using (var streamWriter = new Google.Protobuf.CodedOutputStream(stream))
                {
                    message.WriteTo(streamWriter);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}