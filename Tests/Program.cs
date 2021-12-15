using Grpc.Net.Client;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(1000); // Wait for the service to start

            var mailChannel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions {
                HttpHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (e, c, ch, errs) => true }
            });
            var mailClient = new MailQueueNet.Grpc.MailGrpcService.MailGrpcServiceClient(mailChannel);

            mailClient.SetMailSettings(new MailQueueNet.Grpc.SetMailSettingsMessage
            {
                Settings = new MailQueueNet.Grpc.MailSettings
                {
                    Smtp = new MailQueueNet.Grpc.SmtpMailSettings
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        RequiresSsl = true,
                        RequiresAuthentication = true,
                        Username = "[test email account here]",
                        Password = "[test password here]"
                    }
                }
            });

            mailClient.SetSettings(new MailQueueNet.Grpc.SetSettingsMessage
            {
                Settings = new MailQueueNet.Grpc.Settings
                {
                    QueueFolder = "C:\\mail\\queue",
                    FailedFolder = "C:\\mail\\failed",
                }
            });

            for (int counter = 1; counter < 100; counter++)
            {
                MailMessage message = new MailMessage();
                message.To.Add(new MailAddress(@"to.test@gmail.com", "Test MailQueueNet account"));
                message.From = new MailAddress(@"from.test@gmail.com", "Test MailQueueNet account");
                message.Subject = "Testing MailQueueNet, round " + counter;
                message.Body = @"This is a test!";

                if (counter % 10 == 0)
                {
                    // A failure
                    mailClient.QueueMailWithSettings(message, new MailQueueNet.Grpc.MailSettings
                    {
                        Smtp = new MailQueueNet.Grpc.SmtpMailSettings
                        {
                            Host = "localhost",
                            Port = 25,
                            RequiresSsl = false,
                            RequiresAuthentication = false,
                            Username = null,
                            Password = null
                        }
                    });
                }
                else
                {
                    // A supposed success
                    mailClient.QueueMail(message);
                }
            }
        }
    }
}
