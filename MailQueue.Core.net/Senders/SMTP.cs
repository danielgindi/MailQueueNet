using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueue.Senders
{
    public class SMTP : ISender
    {
        /// <summary>
        /// The number of milliseconds to wait before the request times out.
        /// The default value is 100,000 milliseconds (100 seconds).
        /// </summary>
        public int ConnectionTimeout { get; set; } = 100000;

        public async Task<bool> SendMailAsync(MailMessage message, IMailServerSettings settings)
        {
            var smtpSettings = settings as SmtpMailServerSettings;

            if (string.IsNullOrEmpty(smtpSettings.Host))
            {
                return false;
            }

            using (var smtp = new SmtpClient())
            {
                smtp.Host = smtpSettings.Host.Trim();

                if (smtpSettings.Port > 0)
                {
                    smtp.Port = smtpSettings.Port;
                }

                if (smtpSettings.RequiresAuthentication)
                {
                    smtp.Credentials = new System.Net.NetworkCredential(smtpSettings.Username, smtpSettings.Password);
                }

                smtp.EnableSsl = smtpSettings.RequiresSsl;

                smtp.Timeout = ConnectionTimeout;

                await smtp.SendMailAsync(message);
            }

            return true;
        }
    }
}
