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
using System.ServiceModel;
using System.Net.Mail;

namespace dg.MailQueue
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class PipedService : IMailContract, ISettingsContract
    {

        #region IMailContract

        public void QueueMessage(SerializableMailMessage mailMessage)
        {
            if (mailMessage == null) return;

            Coordinator.AddMail(mailMessage);
        }

        public void QueueMessageWithSmtpSettings(SerializableMailMessage mailMessage, string smtpServer, int port, bool ssl, bool authenticate, string username, string password)
        {
            if (mailMessage == null) return;

            mailMessage.SmtpServer = smtpServer;
            mailMessage.SmtpPort = port;
            mailMessage.RequiresSsl = ssl;
            mailMessage.RequiresAuthentication = authenticate;
            mailMessage.Username = username;
            mailMessage.Password = password;

            Coordinator.AddMail(mailMessage);
        }

        public void QueueMessageNonBlocking(SerializableMailMessage mailMessage)
        {
            if (mailMessage == null) return;

            Coordinator.AddMail(mailMessage);
        }

        public void QueueMessageWithSmtpSettingsNonBlocking(SerializableMailMessage mailMessage, string smtpServer, int port, bool ssl, bool authenticate, string username, string password)
        {
            if (mailMessage == null) return;

            mailMessage.SmtpServer = smtpServer;
            mailMessage.SmtpPort = port;
            mailMessage.RequiresSsl = ssl;
            mailMessage.RequiresAuthentication = authenticate;
            mailMessage.Username = username;
            mailMessage.Password = password;

            Coordinator.AddMail(mailMessage);
        }

        #endregion

        #region ISettingsContract

        public void SetSmtpSettings(string smtpServer, int port, bool ssl, bool authenticate, string username, string password)
        {
            Properties.Settings.Default.SmtpServer = smtpServer;
            Properties.Settings.Default.SmtpPort = port;
            Properties.Settings.Default.SmtpSsl = ssl;
            Properties.Settings.Default.SmtpAuthentication = authenticate;
            Properties.Settings.Default.SmtpUsername = username;
            Properties.Settings.Default.SmtpPassword = password;
            Properties.Settings.Default.Save();
        }

        public void SetQueueFolder(string path)
        {
            Properties.Settings.Default.QueueFolder = path;
            Properties.Settings.Default.Save();
        }

        public void SetFailedFolder(string path)
        {
            Properties.Settings.Default.FailedFolder = path;
            Properties.Settings.Default.Save();
        }

        public void SetMaximumFailureRetries(int maximumFailureRetries)
        {
            Properties.Settings.Default.MaximumFailureRetries = maximumFailureRetries;
            Properties.Settings.Default.Save();
        }

        public void SetSecondsBetweenRetryRounds(int secondsBetweenRetryRounds)
        {
            Properties.Settings.Default.SecondsBetweenRetryRounds = secondsBetweenRetryRounds;
            Properties.Settings.Default.Save();
        }

        public void SetMaximumConcurrentWorkers(int maximumConcurrentWorkers)
        {
            Properties.Settings.Default.MaximumConcurrentWorkers = maximumConcurrentWorkers;
            Properties.Settings.Default.Save();
        }

        #endregion

    }
}
