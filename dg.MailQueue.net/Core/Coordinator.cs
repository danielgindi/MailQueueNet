//
//  dg.MailQueue.net
//
//  Created by Daniel Cohen Gindi on 09/01/2014.
//  Copyright (c) 2014 Daniel Cohen Gindi. All rights reserved.
//
//  https://github.com/danielgindi/drunken-danger-zone
//
//  The MIT License (MIT)
//  
//  Copyright (c) 2014 Daniel Cohen Gindi (danielgindi@gmail.com)
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE. 
//  

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace dg.MailQueue
{
    public class Coordinator
    {
        public Coordinator()
        {
            _thread = new Thread(new ThreadStart(Run));
            Start();
        }

        #region Singleton

        private static Coordinator _instance = null;
        public static Coordinator SharedInstance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Coordinator();
                }
                return _instance;
            }
        }

        #endregion

        #region Private threading vars

        private readonly Thread _thread;
        private bool _stopping = false;
        private bool _stopped = true;
        private readonly object _stopLock = new object();
        private readonly object _actionMonitor = new object();
        private readonly object _failedFnLock = new object();
        private int _concurrentWorkers = 0;

        #endregion

        #region Private vars

        private List<string> _fileNameList;
        private ConcurrentDictionary<string, bool> _sendingFileNames;
        private Dictionary<string, int> _failedFileNameCounter;

        #endregion

        #region Thread main loop

        private void Run()
        {
            _fileNameList = new List<string>();
            _sendingFileNames = new ConcurrentDictionary<string, bool>();
            _failedFileNameCounter = new Dictionary<string, int>();
            
            while (!ShouldStop())
            {
                if (ShouldStop()) break;

                while (!ThereIsAFreeWorker && !ShouldStop())
                {
                    lock (_actionMonitor)
                    {
                        // Did the looping condition change by now?
                        if (ThereIsAFreeWorker || ShouldStop()) break;

                        // Lock for an hour. Any mail sent or worker getting freed, will release it.
                        Monitor.Wait(_actionMonitor, 60 * 60 * 1000);
                    }
                }
                
                if (_fileNameList.Count == 0)
                {
                    string queuePath = Properties.Settings.Default.QueueFolder;
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
                    lock (_actionMonitor)
                    {
                        if (ShouldStop()) break;

                        Monitor.Wait(_actionMonitor, (int)(Properties.Settings.Default.SecondsUntilFolderRefresh * 1000f));
                    }
                }

                string nextFileName = null;
                for (var i = 0; i < _fileNameList.Count; i++)
                {
                    if (!_sendingFileNames.ContainsKey(_fileNameList[i]))
                    {
                        nextFileName = _fileNameList[i];
                        _fileNameList.RemoveAt(i);
                        break;
                    }
                }
                
                if (nextFileName != null)
                {
                    _sendingFileNames[nextFileName] = true;

                    var task = SendMailAsync(nextFileName);
                }
            }

            // Wait for all workers to finish
            while (_concurrentWorkers > 0)
            {
                lock (_actionMonitor)
                {
                    Monitor.Wait(_actionMonitor, 1000);
                }
            }

            lock (_stopLock)
            {
                _stopping = false;
                _stopped = true;
            }
        }

        #endregion

        #region Worker task

        public bool ThereIsAFreeWorker
        {
            get
            {
                var settings = Properties.Settings.Default;
                return settings.MaximumConcurrentWorkers <= 0 ||
                    _concurrentWorkers < settings.MaximumConcurrentWorkers;
            }
        }

        private async Task SendMailAsync(string fileName)
        {
            bool workerInUse = false;

            try
            {
                SerializableMailMessage message = ReadMailFromFile(fileName);

                string hostName = message.SmtpServer;
                int port = message.SmtpPort;
                bool ssl = message.RequiresSsl;
                bool auth = message.RequiresAuthentication;
                string username = message.Username;
                string password = message.Password;

                if (string.IsNullOrEmpty(hostName))
                {
                    Properties.Settings settings = Properties.Settings.Default;
                    hostName = settings.SmtpServer;
                    port = settings.SmtpPort;
                    ssl = settings.SmtpSsl;
                    auth = settings.SmtpAuthentication;
                    username = settings.SmtpUsername;
                    password = settings.SmtpPassword;
                }

                if (string.IsNullOrEmpty(hostName))
                {
                    MarkSkipped(fileName);
                }
                else
                {
                    Interlocked.Increment(ref _concurrentWorkers);
                    workerInUse = true;

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.Host = hostName.Trim();
                        if (port > 0)
                        {
                            smtp.Port = port;
                        }

                        if (auth) smtp.Credentials = new System.Net.NetworkCredential(username, password);
                        smtp.EnableSsl = ssl;

                        smtp.Timeout = Properties.Settings.Default.SmtpConnectionTimeout;

                        await smtp.SendMailAsync(message);
                    }

                    // Task ended, decrement counter and pulse to the Coordinator thread
                    Interlocked.Decrement(ref _concurrentWorkers);
                    workerInUse = false;

                    lock (_actionMonitor)
                    {
                        Monitor.Pulse(_actionMonitor);
                    }

                    MarkSent(fileName);
                }
            }
            catch
            {
                if (workerInUse)
                {
                    // Decrement counter and pulse to the Coordinator thread
                    Interlocked.Decrement(ref _concurrentWorkers);
                }

                try { MarkFailed(fileName); }
                catch { }
            }
        }

        private void MarkFailed(string fileName)
        {
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

                if (_failedFileNameCounter[fileName] >= Properties.Settings.Default.MaximumFailureRetries)
                {
                    shouldRemoveFile = true;
                }
            }

            if (shouldRemoveFile)
            {
                string failedPath = Properties.Settings.Default.FailedFolder;
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
                        // No choice left, lost it, to get it out of the system
                        try { File.Delete(fileName); }
                        catch { }
                    }
                }

                lock (_failedFnLock)
                {
                    _failedFileNameCounter.Remove(fileName);
                }
            }

            bool wasSending;
            _sendingFileNames.TryRemove(fileName, out wasSending);
        }

        private void MarkSent(string fileName)
        {
            try { File.Delete(fileName); }
            catch { }

            bool wasSending;
            _sendingFileNames.TryRemove(fileName, out wasSending);
        }

        private void MarkSkipped(string fileName)
        {
            // This is a file that has not failed, but was not sent. This can be when not SMTP server is specified at all, and we are waiting for settings.
            // So do nothing. This file is not in the cached list, and we will only reach it on the next round and try again.

            bool wasSending;
            _sendingFileNames.TryRemove(fileName, out wasSending);
        }

        #endregion

        #region Public methods

        private bool ShouldStop()
        {
            lock (_stopLock)
                return _stopped || _stopping;
        }

        public void Stop()
        {
            lock (_stopLock)
            {
                if (_stopped || _stopping) return;
                _stopping = true;

                lock (_actionMonitor)
                {
                    Monitor.Pulse(_actionMonitor);
                }
            }
        }

        public void Start()
        {
            lock (_stopLock)
            {
                if (_stopping) return;
                if (_stopped)
                {
                    _stopped = false;
                    _thread.Start();
                }
            }
        }

        private static int mailIdCounter = 1;
        public static void AddMail(SerializableMailMessage message)
        {
            string tempPath = Files.CreateEmptyTempFile();
            if (WriteMailToFile(message, tempPath))
            {
                string queuePath = Properties.Settings.Default.QueueFolder;
                try { queuePath = Files.MapPath(queuePath); }
                catch { }

                bool success = false;
                string file = Path.Combine(queuePath, DateTime.Now.ToString(@"yyyyMMddHHmmss") + @"_" + mailIdCounter.ToString().PadLeft(8, '0') + @".mail");
                while (File.Exists(file))
                {
                    mailIdCounter++;
                    file = Path.Combine(queuePath, DateTime.Now.ToString(@"yyyyMMddHHmmss") + @"_" + mailIdCounter.ToString().PadLeft(8, '0') + @".mail");

                    try
                    {
                        File.Move(tempPath, file);
                        success = true;
                        break;
                    }
                    catch (DirectoryNotFoundException)
                    {
                        break;
                    }
                    catch (PathTooLongException)
                    {
                        break;
                    }
                    catch (FileNotFoundException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                }
                if (!success)
                {
                    try
                    {
                        File.Move(tempPath, file);
                        success = true;
                    }
                    catch { }
                }

                if (success)
                {
                    SharedInstance.ContinueSendingEmails();
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

        public static SerializableMailMessage ReadMailFromFile(string path)
        {
            try
            {
                SerializableMailMessage message = new SerializableMailMessage();

                XmlSerializer serializer = new XmlSerializer(typeof(SerializableMailMessage));

                using (TextReader streamReader = new StreamReader(path, Encoding.UTF8))
                {
                    message = serializer.Deserialize(streamReader) as SerializableMailMessage;
                }

                return message;
            }
            catch
            {
                return null;
            }
        }

        public static bool WriteMailToFile(SerializableMailMessage message, string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SerializableMailMessage));

                using (TextWriter streamWriter = new StreamWriter(path, false, Encoding.UTF8))
                {
                    serializer.Serialize(streamWriter, message);
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