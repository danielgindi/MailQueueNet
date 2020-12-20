using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Net.Mail;

namespace MailQueue
{
    [ServiceContract]
    public interface ISettingsContract
    {
        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(SmtpMailServerSettings))]
        [ServiceKnownType(typeof(MailgunMailServerSettings))]
        void SetMailSettings(IMailServerSettings settings);
         
        [OperationContract]
        IMailServerSettings GetMailSettings();

        [OperationContract(IsOneWay = true)]
        void SetQueueFolder(string path);

        [OperationContract]
        string GetQueueFolder();

        [OperationContract(IsOneWay = true)]
        void SetFailedFolder(string path);

        [OperationContract]
        string GetFailedFolder();

        [OperationContract(IsOneWay = true)]
        void SetMaximumFailureRetries(int maximumFailureRetries);

        [OperationContract]
        int GetMaximumFailureRetries();

        [OperationContract(IsOneWay = true)]
        void SetSecondsUntilFolderRefresh(float secondsUntilFolderRefresh);

        [OperationContract]
        float GetSecondsUntilFolderRefresh();

        [OperationContract(IsOneWay = true)]
        void SetMaximumConcurrentWorkers(int maximumConcurrentWorkers);

        [OperationContract]
        int GetMaximumConcurrentWorkers();
    }
}
