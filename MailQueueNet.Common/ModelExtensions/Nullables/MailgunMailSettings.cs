namespace MailQueueNet.Grpc
{
    public partial class MailgunMailSettings
    {
        public string Domain
        {
            get { return OptDomainCase == OptDomainOneofCase.None ? null : OptDomainValue; }
            set { if (value == null) ClearOptDomain(); else OptDomainValue = value; }
        }

        public string ApiKey
        {
            get { return OptApiKeyCase == OptApiKeyOneofCase.None ? null : OptApiKeyValue; }
            set { if (value == null) ClearOptApiKey(); else OptApiKeyValue = value; }
        }
    }
}
