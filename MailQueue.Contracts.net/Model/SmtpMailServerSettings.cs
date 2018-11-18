using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.IO;
using MailQueue.Contracts.Utils;

namespace MailQueue
{
    [XmlRoot(ElementName = "Smtp")]
    public class SmtpMailServerSettings : IMailServerSettings
    {
        public const string TYPE = "Smtp";

        public SmtpMailServerSettings()
        {
        }

        [XmlIgnore]
        public override string Type { get { return TYPE; } }

        [XmlIgnore]
        public override bool IsEmpty { get { return string.IsNullOrEmpty(Host); } }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool RequiresSsl { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
