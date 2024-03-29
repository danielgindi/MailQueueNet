﻿using System;
using System.IO;

namespace MailQueueNet.Service.Utilities
{
    internal static class Folders
    {
        internal static bool VerifyDirectoryExists(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                path = Files.MapPath(path);
            }
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    try
                    {
                        CreateDirectory(path);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static DirectoryInfo CreateDirectory(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetFullPath(path));

            try
            {
                if (!dirInfo.Exists) dirInfo.Create();
                return dirInfo;
            }
            catch
            {
                return new DirectoryInfo(path);
            }
        }

        public static string GetTempDir()
        {
            string path = null;
            try
            {
                path = Environment.GetEnvironmentVariable("TEMP");
                if (path == null || path.Length == 0)
                {
                    path = Environment.GetEnvironmentVariable("TMP");
                }
                if (path == null || path.Length == 0)
                {
                    path = Environment.GetEnvironmentVariable("WINDIR");
                    if (path != null && path.Length > 0)
                    {
                        path = Path.Combine(path, @"TEMP");
                    }
                }
            }
            catch
            {
            }
            if (path == null || path.Length == 0)
            {
                path = Files.MapPath(path);
            }
            path = Path.Combine(path, @"MailQueueNet\");
            if (!path.EndsWith(@"/") && !path.EndsWith(@"\"))
            {
                if (path.IndexOf('/') > -1) path += '/';
                else path += '\\';
            }
            if (VerifyDirectoryExists(path)) return path;
            throw new UnauthorizedAccessException(@"Cannot access TEMP folder!");
        }
    }
}
