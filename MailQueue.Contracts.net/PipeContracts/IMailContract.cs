using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Net.Mail;

namespace MailQueue
{
    [ServiceContract]
    public interface IMailContract
    {
        [OperationContract]
        void QueueMessage(SerializableMailMessage mailMessage);

        [OperationContract]
        void QueueMessageWithMailSettings(SerializableMailMessage mailMessage, IMailServerSettings mailSettings);
    }
}
