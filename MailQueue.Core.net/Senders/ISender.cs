using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueue.Senders
{
    public interface ISender
    {
        /// <summary>
        /// Sends an email with a MailMessage and a set of settings.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="settings"></param>
        /// <returns>true if succeeded, false if sending was skipped (i.e missing settings). Could throw exceptions for other reasons.</returns>
        Task<bool> SendMailAsync(MailMessage message, IMailServerSettings settings);
    }
}
