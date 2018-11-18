using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.IO;
using dg.MailQueue.Contracts.Utils;

namespace dg.MailQueue
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
    }
}
