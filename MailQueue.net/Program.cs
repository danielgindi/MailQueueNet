using System;
using System.Collections.Generic;
using System.Text;
using MailQueue;
using System.Threading;
using System.Xml.Serialization;
using System.IO;

namespace MailQueue
{
    public class Program
    {
        static MailQueueService service;
        static void Main(string[] args)
        {
            bool IsServiceMode = args.Length == 0 || args[0] != "console";

            // Upgrade settings config file between versions
            SettingsController.Upgrade();

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
