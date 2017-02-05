using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using dg.MailQueue;
using System.Net.Mail;
using System.Threading;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000); // Wait for the service to start

            ISettingsContract settingsChannel = PipeFactory.NewSettingsChannel();
            IMailContract mailChannel = PipeFactory.NewMailChannel();

            settingsChannel.SetSmtpSettings("smtp.gmail.com", 587, true, true, "[test email account here]", "[test password here]");
            // settingsChannel.SetQueueFolder("C:\\mail\queued");
            // settingsChannel.SetFailedFolder("C:\\mail\failed");

            /*for (int counter = 1; counter < 100; counter++)
            {
                MailMessage message = new MailMessage();
                message.To.Add(new MailAddress(@"to.test@gmail.com", "Test dg.MailQueue.net account"));
                message.From = new MailAddress(@"from.test@gmail.com", "Test dg.MailQueue.net account");
                message.Subject = "Testing dg.MailQueue.net, round " + counter;
                message.Body = @"This is a test!";

                if (counter % 10 == 0)
                {
                    // A failure
                    mailChannel.QueueMessageWithSmtpSettings(new SerializableMailMessage(message), "localhost", 25, false, false, null, null);
                }
                else
                {
                    // A supposed success
                    mailChannel.QueueMessage(new SerializableMailMessage(message));
                }
            }*/
        }
    }
}
