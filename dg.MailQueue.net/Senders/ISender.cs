using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dg.MailQueue.Senders
{
    public interface ISender
    {
        Task<bool> SendMailAsync(SerializableMailMessage message, IMailServerSettings settings);
    }
}
