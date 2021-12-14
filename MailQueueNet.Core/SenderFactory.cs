using MailQueueNet.Senders;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueueNet
{
    public static class SenderFactory
    {
        public static ISender GetSenderForSettings(Grpc.MailSettings settings)
        {
            if (settings.SettingsCase == Grpc.MailSettings.SettingsOneofCase.Smtp)
            {
                return new Senders.SMTP();
            }
            else if (settings.SettingsCase == Grpc.MailSettings.SettingsOneofCase.Mailgun)
            {
                return new Senders.Mailgun();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sends an email with a MailMessage and a set of settings.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        /// <returns>true if succeeded, false if sending was skipped (i.e missing settings). Could throw exceptions for other reasons.</returns>
        public static async Task<bool> SendMailAsync(MailMessage message, Grpc.MailSettings settings)
        {
            var sender = GetSenderForSettings(settings);
            if (sender == null) return false;

            return await sender.SendMailAsync(message, settings);
        }
    }
}
