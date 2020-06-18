using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Xml.Schema;
using System.IO;
using System.Net.Mime;
using MailQueue.Contracts.Utils;

namespace MailQueue
{
    [XmlRoot(ElementName = "SerializableMailMessage")]
    public class SerializableMailMessage : MailMessage, IXmlSerializable
    {
        public SerializableMailMessage()
        {
        }

        public SerializableMailMessage(MailMessage mailMessage)
        {
            Body = mailMessage.Body;
            BodyEncoding = mailMessage.BodyEncoding;
            DeliveryNotificationOptions = mailMessage.DeliveryNotificationOptions;
            From = mailMessage.From;
            HeadersEncoding = mailMessage.HeadersEncoding;
            IsBodyHtml = mailMessage.IsBodyHtml;
            Priority = mailMessage.Priority;
            Sender = mailMessage.Sender;
            Subject = mailMessage.Subject;
            SubjectEncoding = mailMessage.SubjectEncoding;
            Headers.Add(mailMessage.Headers);

            foreach (var attachment in mailMessage.Attachments)
            {
                Attachments.Add(attachment);
            }

            foreach (MailAddress address in mailMessage.To)
            {
                To.Add(address);
            }
            foreach (MailAddress address in mailMessage.CC)
            {
                CC.Add(address);
            }
            foreach (MailAddress address in mailMessage.Bcc)
            {
                Bcc.Add(address);
            }
            foreach (MailAddress address in mailMessage.ReplyToList)
            {
                ReplyToList.Add(address);
            }
        }

        public static bool Serializable(MailMessage message)
        {
            foreach (var attachment in message.Attachments)
            {
                if (!(attachment.ContentStream is FileStream)) return false;
            }
            return true;
        }

        public IMailServerSettings MailSettings;
            
        public XmlNode GetConfigNode(XmlDocument xml, string nodePath)
        {
            if (!string.IsNullOrEmpty(xml.DocumentElement.NamespaceURI))
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xml.NameTable);
                nsmgr.AddNamespace("ms", xml.DocumentElement.NamespaceURI);
                return xml.SelectSingleNode("ms:" + xml.DocumentElement.Name + "/ms:" + nodePath.Replace("/", "/ms:"), nsmgr);
            }
            else
            {
                return xml.SelectSingleNode(xml.DocumentElement.Name + "/" + nodePath);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this != null)
            {
                writer.WriteStartElement("MailMessage");
                writer.WriteAttributeString("Priority", this.Priority.ToString());
                writer.WriteAttributeString("IsBodyHtml", this.IsBodyHtml.ToString());
                if (this.BodyEncoding != null) writer.WriteAttributeString("BodyEncoding", this.BodyEncoding.CodePage.ToString());
                if (this.HeadersEncoding != null) writer.WriteAttributeString("HeadersEncoding", this.HeadersEncoding.CodePage.ToString());
                if (this.SubjectEncoding != null) writer.WriteAttributeString("SubjectEncoding", this.SubjectEncoding.CodePage.ToString());
                writer.WriteAttributeString("DeliveryNotificationOptions", this.DeliveryNotificationOptions.ToString());

                if (this.From != null)
                {
                    writer.WriteStartElement("From");
                    if (!string.IsNullOrEmpty(this.From.DisplayName))
                    {
                        writer.WriteAttributeString("DisplayName", this.From.DisplayName);
                    }
                    writer.WriteRaw(this.From.Address);
                    writer.WriteEndElement();
                }

                if (this.Sender != null)
                {
                    writer.WriteStartElement("Sender");
                    if (!string.IsNullOrEmpty(this.Sender.DisplayName))
                    {
                        writer.WriteAttributeString("DisplayName", this.Sender.DisplayName);
                    }
                    writer.WriteRaw(this.Sender.Address);
                    writer.WriteEndElement();
                }

                if (this.To.Count > 0)
                {
                    writer.WriteStartElement("To");
                    writer.WriteStartElement("Addresses");
                    foreach (MailAddress address in this.To)
                    {
                        writer.WriteStartElement("Address");
                        if (!string.IsNullOrEmpty(address.DisplayName))
                        {
                            writer.WriteAttributeString("DisplayName", address.DisplayName);
                        }
                        writer.WriteRaw(address.Address);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                if (this.ReplyToList.Count > 0)
                {
                    writer.WriteStartElement("ReplyToList");
                    writer.WriteStartElement("Addresses");
                    foreach (MailAddress address in this.ReplyToList)
                    {
                        writer.WriteStartElement("Address");
                        if (!string.IsNullOrEmpty(address.DisplayName))
                        {
                            writer.WriteAttributeString("DisplayName", address.DisplayName);
                        }
                        writer.WriteRaw(address.Address);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                if (this.CC.Count > 0)
                {
                    writer.WriteStartElement("CC");
                    writer.WriteStartElement("Addresses");
                    foreach (MailAddress address in this.CC)
                    {
                        writer.WriteStartElement("Address");
                        if (!string.IsNullOrEmpty(address.DisplayName))
                        {
                            writer.WriteAttributeString("DisplayName", address.DisplayName);
                        }
                        writer.WriteRaw(address.Address);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                if (this.Bcc.Count > 0)
                {
                    writer.WriteStartElement("Bcc");
                    writer.WriteStartElement("Addresses");
                    foreach (MailAddress address in this.Bcc)
                    {
                        writer.WriteStartElement("Address");
                        if (!string.IsNullOrEmpty(address.DisplayName))
                        {
                            writer.WriteAttributeString("DisplayName", address.DisplayName);
                        }
                        writer.WriteRaw(address.Address);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                if (this.Headers.Count > 0)
                {
                    writer.WriteStartElement("Headers");
                    foreach (string key in this.Headers.AllKeys)
                    {
                        writer.WriteStartElement("Header");
                        writer.WriteAttributeString("Key", key);
                        writer.WriteRaw(this.Headers[key]);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (this.Attachments.Count > 0)
                {
                    writer.WriteStartElement("Attachments");
                    foreach (var attachment in this.Attachments)
                    {
                        var stream = attachment.ContentStream as FileStream;
                        if (stream == null) return;

                        writer.WriteStartElement("Attachment");

                        if (stream.Name != null)
                        {
                            writer.WriteAttributeString("FileName", stream.Name);
                        }

                        if (attachment.Name != null)
                        {
                            writer.WriteAttributeString("Name", attachment.Name);
                        }

                        if (attachment.NameEncoding != null)
                        {
                            writer.WriteAttributeString("NameEncoding", attachment.NameEncoding.EncodingName);
                        }

                        if (attachment.ContentId != null)
                        {
                            writer.WriteAttributeString("ContentId", attachment.ContentId);
                        }

                        if (attachment.ContentType != null)
                        {
                            writer.WriteAttributeString("ContentType", attachment.ContentType.MediaType);
                        }

                        writer.WriteAttributeString("TransferEncoding", attachment.TransferEncoding.ToString());

                        if (attachment.ContentDisposition != null)
                        {
                            writer.WriteStartElement("ContentDisposition");
                            {
                                if (attachment.ContentDisposition.DispositionType != null)
                                    writer.WriteAttributeString("DispositionType", attachment.ContentDisposition.DispositionType);
                                writer.WriteAttributeString("Inline", attachment.ContentDisposition.Inline.ToString());
                                if (attachment.ContentDisposition.FileName != null)
                                    writer.WriteAttributeString("FileName", attachment.ContentDisposition.FileName);
                                if (attachment.ContentDisposition.CreationDate != DateTime.MinValue)
                                    writer.WriteAttributeString("CreationDate", attachment.ContentDisposition.CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
                                if (attachment.ContentDisposition.ModificationDate != DateTime.MinValue)
                                    writer.WriteAttributeString("ModificationDate", attachment.ContentDisposition.ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
                                if (attachment.ContentDisposition.ReadDate != DateTime.MinValue)
                                    writer.WriteAttributeString("ReadDate", attachment.ContentDisposition.ReadDate.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"));
                                writer.WriteAttributeString("Size", attachment.ContentDisposition.Size.ToString());

                                writer.WriteStartElement("Parameters");
                                {
                                    foreach (string key in attachment.ContentDisposition.Parameters.Keys)
                                    {
                                        writer.WriteAttributeString(key, attachment.ContentDisposition.Parameters[key]);
                                    }
                                }
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Subject");
                writer.WriteRaw(this.Subject);
                writer.WriteEndElement();

                writer.WriteStartElement("Body");
                writer.WriteCData(Body);
                writer.WriteEndElement();

                writer.WriteEndElement();

                if (MailSettings != null)
                {
                    writer.WriteStartElement("MailSettings");

                    var s = IMailServerSettings.GetSerializer();
                    s.Serialize(writer, MailSettings);

                    writer.WriteEndElement();
                }
            }
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(reader);

            XmlNode rootNode = XmlUtil.GetConfigNode(xml, "MailMessage");

            this.IsBodyHtml = Convert.ToBoolean(rootNode.Attributes["IsBodyHtml"].Value);
            this.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), rootNode.Attributes["Priority"].Value);
            if (rootNode.Attributes["BodyEncoding"] != null) this.BodyEncoding = Encoding.GetEncoding(Convert.ToInt32(rootNode.Attributes["BodyEncoding"].Value));
            if (rootNode.Attributes["HeadersEncoding"] != null) this.HeadersEncoding = Encoding.GetEncoding(Convert.ToInt32(rootNode.Attributes["HeadersEncoding"].Value));
            if (rootNode.Attributes["SubjectEncoding"] != null) this.SubjectEncoding = Encoding.GetEncoding(Convert.ToInt32(rootNode.Attributes["SubjectEncoding"].Value));
            this.DeliveryNotificationOptions = (DeliveryNotificationOptions)Enum.Parse(typeof(DeliveryNotificationOptions), rootNode.Attributes["DeliveryNotificationOptions"].Value);

            string displayName;
            MailAddress address;

            XmlNode smtpNode = XmlUtil.GetConfigNode(xml, "MailSettings");
            if (smtpNode != null)
            {
                using (var strReader = new StringReader(smtpNode.InnerXml))
                {
                    var s = IMailServerSettings.GetSerializer();
                    this.MailSettings = s.Deserialize(strReader) as IMailServerSettings;
                }
            }

            XmlNode fromNode = XmlUtil.GetConfigNode(xml, "MailMessage/From");
            if (fromNode != null)
            {
                displayName = string.Empty;
                if (fromNode.Attributes["DisplayName"] != null)
                {
                    displayName = fromNode.Attributes["DisplayName"].Value;
                }
                address = new MailAddress(fromNode.InnerText, displayName);
                this.From = address;
            }

            XmlNode senderNode = XmlUtil.GetConfigNode(xml, "MailMessage/Sender");
            if (senderNode != null)
            {
                displayName = string.Empty;
                if (senderNode.Attributes["DisplayName"] != null)
                {
                    displayName = senderNode.Attributes["DisplayName"].Value;
                }
                address = new MailAddress(senderNode.InnerText, displayName);
                this.Sender = address;
            }

            XmlNode toNode = XmlUtil.GetConfigNode(xml, "MailMessage/To/Addresses");
            if (toNode != null)
            {
                foreach (XmlNode node in toNode.ChildNodes)
                {
                    displayName = string.Empty;
                    if (node.Attributes["DisplayName"] != null)
                    {
                        displayName = node.Attributes["DisplayName"].Value;
                    }
                    address = new MailAddress(node.InnerText, displayName);
                    this.To.Add(address);
                }
            }

            XmlNode replyToListNode = XmlUtil.GetConfigNode(xml, "MailMessage/ReplyToList/Addresses");
            if (replyToListNode != null)
            {
                foreach (XmlNode node in replyToListNode.ChildNodes)
                {
                    displayName = string.Empty;
                    if (node.Attributes["DisplayName"] != null)
                    {
                        displayName = node.Attributes["DisplayName"].Value;
                    }
                    address = new MailAddress(node.InnerText, displayName);
                    this.ReplyToList.Add(address);
                }
            }

            XmlNode ccNode = XmlUtil.GetConfigNode(xml, "MailMessage/CC/Addresses");
            if (ccNode != null)
            {
                foreach (XmlNode node in ccNode.ChildNodes)
                {
                    displayName = string.Empty;
                    if (node.Attributes["DisplayName"] != null)
                    {
                        displayName = node.Attributes["DisplayName"].Value;
                    }
                    address = new MailAddress(node.InnerText, displayName);
                    this.CC.Add(address);
                }
            }

            XmlNode bccNode = XmlUtil.GetConfigNode(xml, "MailMessage/Bcc/Addresses");
            if (bccNode != null)
            {
                foreach (XmlNode node in bccNode.ChildNodes)
                {
                    displayName = string.Empty;
                    if (node.Attributes["DisplayName"] != null)
                    {
                        displayName = node.Attributes["DisplayName"].Value;
                    }
                    address = new MailAddress(node.InnerText, displayName);
                    this.Bcc.Add(address);
                }
            }

            XmlNode headersNode = XmlUtil.GetConfigNode(xml, "MailMessage/Headers");
            if (headersNode != null)
            {
                foreach (XmlNode node in headersNode.ChildNodes)
                {
                    this.Headers.Add(node.Attributes["Key"].Value, node.InnerText);
                }
            }

            XmlNode attachmentsNode = XmlUtil.GetConfigNode(xml, "MailMessage/Attachments");
            if (attachmentsNode != null)
            {
                foreach (XmlNode node in attachmentsNode.ChildNodes)
                {
                    var attachment = new Attachment(node.Attributes["FileName"].Value);

                    if (node.Attributes["Name"] != null)
                    {
                        attachment.Name = node.Attributes["Name"].Value;
                    }

                    if (node.Attributes["NameEncoding"] != null)
                    {
                        attachment.NameEncoding = Encoding.GetEncoding(node.Attributes["NameEncoding"].Value);
                    }

                    if (node.Attributes["ContentId"] != null)
                    {
                        attachment.ContentId = node.Attributes["ContentId"].Value;
                    }

                    if (node.Attributes["ContentType"] != null)
                    {
                        attachment.ContentType = new ContentType(node.Attributes["ContentType"].Value);
                    }

                    if (node.Attributes["TransferEncoding"] != null)
                    {
                        attachment.TransferEncoding = (TransferEncoding)Enum.Parse(typeof(TransferEncoding), node.Attributes["TransferEncoding"].Value);
                    }

                    var cdNode = node.SelectSingleNode("*[local-name()='ContentDisposition']");
                    if (cdNode != null)
                    {
                        if (cdNode.Attributes["DispositionType"] != null)
                        {
                            attachment.ContentDisposition.DispositionType = cdNode.Attributes["DispositionType"].Value;
                        }

                        if (cdNode.Attributes["Inline"] != null)
                        {
                            attachment.ContentDisposition.Inline = Convert.ToBoolean(cdNode.Attributes["Inline"].Value);
                        }

                        if (cdNode.Attributes["FileName"] != null)
                        {
                            attachment.ContentDisposition.FileName = cdNode.Attributes["FileName"].Value;
                        }

                        if (cdNode.Attributes["CreationDate"] != null)
                        {
                            attachment.ContentDisposition.CreationDate = DateTime.ParseExact(cdNode.Attributes["CreationDate"].Value, "yyyy-MM-ddTHH:mm:ss.fffzzz", null);
                        }

                        if (cdNode.Attributes["ModificationDate"] != null)
                        {
                            attachment.ContentDisposition.ModificationDate = DateTime.ParseExact(cdNode.Attributes["ModificationDate"].Value, "yyyy-MM-ddTHH:mm:ss.fffzzz", null);
                        }

                        if (cdNode.Attributes["ReadDate"] != null)
                        {
                            attachment.ContentDisposition.ReadDate = DateTime.ParseExact(cdNode.Attributes["ReadDate"].Value, "yyyy-MM-ddTHH:mm:ss.fffzzz", null);
                        }

                        if (cdNode.Attributes["Size"] != null)
                        {
                            attachment.ContentDisposition.Size = Convert.ToInt64(cdNode.Attributes["Size"].Value);
                        }

                        var pNode = cdNode.SelectSingleNode("*[local-name()='Parameters']");
                        if (pNode != null)
                        {
                            for (var pi = 0; pi < pNode.Attributes.Count; pi++)
                            {
                                var attr = pNode.Attributes[pi];

                                try
                                {
                                    attachment.ContentDisposition.Parameters[attr.Name] = attr.Value;
                                }
                                catch { }
                            }
                        }
                    }

                    this.Attachments.Add(attachment);
                }
            }

            XmlNode subjectNode = XmlUtil.GetConfigNode(xml, "MailMessage/Subject");
            this.Subject = subjectNode.InnerText;

            XmlNode bodyNode = XmlUtil.GetConfigNode(xml, "MailMessage/Body");
            this.Body = bodyNode.InnerText;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
