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
        void QueueMessageWithSmtpSettings(SerializableMailMessage mailMessage, string smtpServer, int port, bool ssl, bool authenticate, string username, string password);
       
        [OperationContract(IsOneWay = true)]
        void QueueMessageNonBlocking(SerializableMailMessage mailMessage);

        [OperationContract(IsOneWay = true)]
        void QueueMessageWithSmtpSettingsNonBlocking(SerializableMailMessage mailMessage, string smtpServer, int port, bool ssl, bool authenticate, string username, string password);
    }
}
