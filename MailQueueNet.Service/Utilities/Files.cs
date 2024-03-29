﻿using System;
using System.IO;

namespace MailQueueNet.Service.Utilities
{
    internal static class Files
    {
        internal static string MapPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            else if (path.StartsWith("~/"))
            {
                return Path.Combine(AppContext.BaseDirectory, path.Remove(0, 2));
            }
            else
            {
                throw new Exception("Could not resolve non-rooted path.");
            }
        }

        internal static string CreateEmptyTempFile()
        {
            string tempFilePath = Folders.GetTempDir() + Guid.NewGuid().ToString() + @".tmp";
            FileStream fs = null;

            while (true)
            {
                try
                {
                    fs = new FileStream(tempFilePath, FileMode.CreateNew);
                    break;
                }
                catch (IOException ioex)
                {
                    Console.WriteLine(@"Utility.File.CreateEmptyTempFile - Error: {0}", ioex.ToString());
                    if (System.IO.File.Exists(tempFilePath))
                    { // File exists, make up another name
                        tempFilePath = Folders.GetTempDir() + Guid.NewGuid().ToString() + @".tmp";
                    }
                    else
                    { // Another error, throw it back up
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(@"Utility.File.CreateEmptyTempFile - Error: {0}", ex.ToString());
                    break;
                }
            }

            if (fs != null)
            {
                fs.Close();
                fs.Dispose();
                fs = null;
                return tempFilePath;
            }

            return null;
        }
    }
}
