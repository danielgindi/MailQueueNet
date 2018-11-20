using System.Xml.Serialization;

namespace MailQueue
{
    [XmlRoot(ElementName = "Mailgun")]
    public class MailgunMailServerSettings : IMailServerSettings
    {
        public const string TYPE = "Mailgun";

        public MailgunMailServerSettings()
        {
        }

        [XmlIgnore]
        public override string Type { get { return TYPE; } }

        [XmlIgnore]
        public override bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Domain)
                    || string.IsNullOrEmpty(ApiKey);
            }
        }

        public string Domain { get; set; }
        public string ApiKey { get; set; }

        /// <summary>
        /// The number of milliseconds to wait before the request times out.
        /// The default value is 100,000 milliseconds (100 seconds).
        /// </summary>
        public int ConnectionTimeout { get; set; } = 100000;
    }
}
