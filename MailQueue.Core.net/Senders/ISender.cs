using System.Net.Mail;
using System.Threading.Tasks;

namespace MailQueue.Senders
{
    public interface ISender
    {
        Task<bool> SendMailAsync(MailMessage message, IMailServerSettings settings);
    }
}
