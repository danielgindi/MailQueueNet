using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueueNet.Senders
{
    public class SMTP : ISender
    {
        public async Task<bool> SendMailAsync(MailMessage message, Grpc.MailSettings settings)
        {
            var smtpSettings = settings.Smtp;

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

                smtp.Timeout = smtpSettings.ConnectionTimeout;

                await smtp.SendMailAsync(message);
            }

            return true;
        }
    }
}
