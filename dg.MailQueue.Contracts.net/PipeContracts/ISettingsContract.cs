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
    [ServiceContract]
    public interface ISettingsContract
    {
        [OperationContract(IsOneWay = true)]
        void SetSmtpSettings(string smtpServer, int port, bool ssl, bool authenticate, string username, string password);

        [OperationContract(IsOneWay = true)]
        void SetQueueFolder(string path);

        [OperationContract]
        string GetQueueFolder();

        [OperationContract(IsOneWay = true)]
        void SetFailedFolder(string path);

        [OperationContract]
        string GetFailedFolder();

        [OperationContract(IsOneWay = true)]
        void SetMaximumFailureRetries(int maximumFailureRetries);

        [OperationContract]
        int GetMaximumFailureRetries();

        [OperationContract(IsOneWay = true)]
        void SetSecondsUntilFolderRefresh(float secondsUntilFolderRefresh);

        [OperationContract]
        float GetSecondsUntilFolderRefresh();

        [OperationContract(IsOneWay = true)]
        void SetMaximumConcurrentWorkers(int maximumConcurrentWorkers);

        [OperationContract]
        int GetMaximumConcurrentWorkers();
    }
}
