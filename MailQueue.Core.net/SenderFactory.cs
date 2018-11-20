using MailQueue.Senders;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueue
{
    public static class SenderFactory
    {
        public static ISender GetSenderForSettings(IMailServerSettings settings)
        {
            if (settings is SmtpMailServerSettings)
            {
                return new Senders.SMTP();
            }
            else if (settings is MailgunMailServerSettings)
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
        public static async Task<bool> SendMail(MailMessage message, IMailServerSettings settings)
        {
            var sender = GetSenderForSettings(settings);
            if (sender == null) return false;

            return await sender.SendMailAsync(message, settings);
        }
    }
}
