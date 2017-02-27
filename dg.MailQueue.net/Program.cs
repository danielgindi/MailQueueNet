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
using dg.MailQueue;
using System.Threading;

namespace dg.MailQueue
{
    public class Program
    {
        static MailQueueService service;
        static void Main(string[] args)
        {
            bool IsServiceMode = args.Length == 0 || args[0] != "console";

            service = new MailQueueService();

            if (IsServiceMode)
            {
                System.ServiceProcess.ServiceBase.Run(service);
            }
            else
            {
                Console.WriteLine(@"** Notice: Running in non-service mode **");

                service.OpenSettingsNamedPipe();

                Coordinator.SharedInstance.ConsoleLogEnabled = true;
                Coordinator.SharedInstance.ConsoleLogExceptions = args[1] == "debug";

                Coordinator.SharedInstance.Start();

                Console.WriteLine("Press ESC to stop.");
                do
                {

                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

                Coordinator.SharedInstance.Stop();
            }
        }
    }
}
