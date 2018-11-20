//
//  MailQueue.net
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
using System.ServiceModel;

namespace MailQueue
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true)]
    public class PipedService : IMailContract, ISettingsContract
    {

        #region IMailContract

        public void QueueMessage(SerializableMailMessage mailMessage)
        {
            if (mailMessage == null) return;

            Coordinator.SharedInstance.AddMail(mailMessage);
        }

        [Obsolete("Use QueueMessageWithMailSettings instead")]
        public void QueueMessageWithSmtpSettings(SerializableMailMessage mailMessage, string smtpServer, int port, bool ssl, bool authenticate, string username, string password)
        {
            if (mailMessage == null) return;

            QueueMessageWithMailSettings(mailMessage, new SmtpMailServerSettings
            {
                Host = smtpServer,
                Port = port,
                RequiresSsl = ssl,
                RequiresAuthentication = authenticate,
                Username = username,
                Password = password
            });
        }

        public void QueueMessageWithMailSettings(SerializableMailMessage mailMessage, IMailServerSettings mailSettings)
        {
            if (mailMessage == null) return;

            mailMessage.MailSettings = mailSettings;

            Coordinator.SharedInstance.AddMail(mailMessage);
        }

        public void QueueMessageNonBlocking(SerializableMailMessage mailMessage)
        {
            if (mailMessage == null) return;

            Coordinator.SharedInstance.AddMail(mailMessage);
        }

        [Obsolete("Use QueueMessageWithMailSettingsNonBlocking instead")]
        public void QueueMessageWithSmtpSettingsNonBlocking(SerializableMailMessage mailMessage, string smtpServer, int port, bool ssl, bool authenticate, string username, string password)
        {
            if (mailMessage == null) return;

            QueueMessageWithMailSettingsNonBlocking(mailMessage, new SmtpMailServerSettings
            {
                Host = smtpServer,
                Port = port,
                RequiresSsl = ssl,
                RequiresAuthentication = authenticate,
                Username = username,
                Password = password
            });
        }

        public void QueueMessageWithMailSettingsNonBlocking(SerializableMailMessage mailMessage, IMailServerSettings mailSettings)
        {
            if (mailMessage == null) return;

            mailMessage.MailSettings = mailSettings;

            Coordinator.SharedInstance.AddMail(mailMessage);
        }

        #endregion

        #region ISettingsContract

        public void SetSmtpSettings(string smtpServer, int port, bool ssl, bool authenticate, string username, string password)
        {
            SetMailSettings(new SmtpMailServerSettings
            {
                Host = smtpServer,
                Port = port,
                RequiresSsl = ssl,
                RequiresAuthentication = authenticate,
                Username = username,
                Password = password
            });
        }

        public void SetMailSettings(IMailServerSettings settings)
        {
            SettingsController.SetMailSettings(settings);
        }

        public IMailServerSettings GetMailSettings()
        {
            return SettingsController.GetMailSettings();
        }

        public void SetQueueFolder(string path)
        {
            Properties.Settings.Default.QueueFolder = path;
            Properties.Settings.Default.Save();
        }

        public string GetQueueFolder()
        {
            return Properties.Settings.Default.QueueFolder;
        }

        public void SetFailedFolder(string path)
        {
            Properties.Settings.Default.FailedFolder = path;
            Properties.Settings.Default.Save();
        }

        public string GetFailedFolder()
        {
            return Properties.Settings.Default.FailedFolder;
        }

        public void SetMaximumFailureRetries(int maximumFailureRetries)
        {
            Properties.Settings.Default.MaximumFailureRetries = maximumFailureRetries;
            Properties.Settings.Default.Save();
        }

        public int GetMaximumFailureRetries()
        {
            return Properties.Settings.Default.MaximumFailureRetries;
        }
        
        public void SetSecondsUntilFolderRefresh(float secondsUntilFolderRefresh)
        {
            Properties.Settings.Default.SecondsUntilFolderRefresh = secondsUntilFolderRefresh;
            Properties.Settings.Default.Save();
        }

        public float GetSecondsUntilFolderRefresh()
        {
            return Properties.Settings.Default.SecondsUntilFolderRefresh;
        }

        public void SetMaximumConcurrentWorkers(int maximumConcurrentWorkers)
        {
            Properties.Settings.Default.MaximumConcurrentWorkers = maximumConcurrentWorkers;
            Properties.Settings.Default.Save();
        }

        public int GetMaximumConcurrentWorkers()
        {
            return Properties.Settings.Default.MaximumConcurrentWorkers;
        }

        #endregion

    }
}
