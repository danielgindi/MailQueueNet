using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueue.Senders
{
    public class SMTP : ISender
    {
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

                smtp.Timeout = Properties.Settings.Default.SmtpConnectionTimeout;

                await smtp.SendMailAsync(message);
            }

            return true;
        }
    }
}
