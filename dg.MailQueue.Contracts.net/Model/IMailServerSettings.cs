using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace dg.MailQueue
{
    [XmlRoot(ElementName = "IMailServerSettings")]
    public abstract class IMailServerSettings
    {
        [XmlIgnore]
        public virtual string Type { get; }

        [XmlIgnore]
        public virtual bool IsEmpty { get; }

        public static XmlSerializer GetSerializer()
        {
            return new XmlSerializer(
                typeof(IMailServerSettings),
                new System.Type[] {
                    typeof(SmtpMailServerSettings),
                    typeof(MailgunMailServerSettings)
                }
            );
        }
    }
}
