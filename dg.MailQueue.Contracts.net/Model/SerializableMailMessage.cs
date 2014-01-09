//
//  dg.MailQueue.net
//
//  Created by Daniel Cohen Gindi on 09/01/2014.
//  Copyright (c) 2014 Daniel Cohen Gindi. All rights reserved.
//
//  https://github.com/danielgindi/drunken-danger-zone
//
//  The MIT License (MIT)
//  
//  Copyright (c) 2014 Daniel Cohen Gindi (danielgindi@gmail.com)
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE. 
//  

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;
using System.Xml.Schema;

namespace dg.MailQueue
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
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public bool RequiresSsl { get; set; }
        public bool RequiresAuthentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
            
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

                writer.WriteStartElement("From");
                if (!string.IsNullOrEmpty(this.From.DisplayName))
                {
                    writer.WriteAttributeString("DisplayName", this.From.DisplayName);
                }
                writer.WriteRaw(this.From.Address);
                writer.WriteEndElement();

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

                writer.WriteStartElement("Subject");
                writer.WriteRaw(this.Subject);
                writer.WriteEndElement();

                writer.WriteStartElement("Body");
                writer.WriteCData(Body);
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteStartElement("Smtp");
                if (this.SmtpServer != null) writer.WriteAttributeString("SmtpServer", this.SmtpServer.ToString());
                if (this.SmtpPort > 0) writer.WriteAttributeString("SmtpPort", this.SmtpPort.ToString());
                writer.WriteAttributeString("RequiresSsl", this.RequiresSsl.ToString());
                writer.WriteAttributeString("RequiresAuthentication", this.RequiresAuthentication.ToString());
                if (this.Username != null) writer.WriteAttributeString("Username", this.Username.ToString());
                if (this.Password != null) writer.WriteAttributeString("Password", this.Password.ToString());
                writer.WriteEndElement();
            }
        }

        public void ReadXml(XmlReader reader)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(reader);

            XmlNode rootNode = GetConfigNode(xml, "MailMessage");

            this.IsBodyHtml = Convert.ToBoolean(rootNode.Attributes["IsBodyHtml"].Value);
            this.Priority = (MailPriority)Enum.Parse(typeof(MailPriority), rootNode.Attributes["Priority"].Value);
            if (rootNode.Attributes["BodyEncoding"] != null) this.BodyEncoding = Encoding.GetEncoding(Convert.ToInt32(rootNode.Attributes["BodyEncoding"].Value));
            if (rootNode.Attributes["HeadersEncoding"] != null) this.HeadersEncoding = Encoding.GetEncoding(Convert.ToInt32(rootNode.Attributes["HeadersEncoding"].Value));
            if (rootNode.Attributes["SubjectEncoding"] != null) this.SubjectEncoding = Encoding.GetEncoding(Convert.ToInt32(rootNode.Attributes["SubjectEncoding"].Value));
            this.DeliveryNotificationOptions = (DeliveryNotificationOptions)Enum.Parse(typeof(DeliveryNotificationOptions), rootNode.Attributes["DeliveryNotificationOptions"].Value);

            string displayName;
            MailAddress address;

            XmlNode smtpNode = GetConfigNode(xml, "Smtp");
            if (smtpNode.Attributes["SmtpServer"] != null) this.SmtpServer = smtpNode.Attributes["SmtpServer"].InnerText;
            if (smtpNode.Attributes["SmtpPort"] != null) this.SmtpPort = int.Parse(smtpNode.Attributes["SmtpPort"].InnerText);
            if (smtpNode.Attributes["RequiresSsl"] != null) this.RequiresSsl = bool.Parse(smtpNode.Attributes["RequiresSsl"].InnerText);
            if (smtpNode.Attributes["RequiresAuthentication"] != null) this.RequiresAuthentication = bool.Parse(smtpNode.Attributes["RequiresAuthentication"].InnerText);
            if (smtpNode.Attributes["Username"] != null) this.Username = smtpNode.Attributes["Username"].InnerText;
            if (smtpNode.Attributes["Password"] != null) this.Password = smtpNode.Attributes["Password"].InnerText;

            XmlNode fromNode = GetConfigNode(xml, "MailMessage/From");
            displayName = string.Empty;
            if (fromNode.Attributes["DisplayName"] != null)
            {
                displayName = fromNode.Attributes["DisplayName"].Value;
            }
            address = new MailAddress(fromNode.InnerText, displayName);
            this.From = address;

            XmlNode senderNode = GetConfigNode(xml, "MailMessage/Sender");
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

            XmlNode toNode = GetConfigNode(xml, "MailMessage/To/Addresses");
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

            XmlNode replyToListNode = GetConfigNode(xml, "MailMessage/ReplyToList/Addresses");
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

            XmlNode ccNode = GetConfigNode(xml, "MailMessage/CC/Addresses");
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

            XmlNode bccNode = GetConfigNode(xml, "MailMessage/Bcc/Addresses");
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

            XmlNode headersNode = GetConfigNode(xml, "MailMessage/Headers");
            if (headersNode != null)
            {
                foreach (XmlNode node in headersNode.ChildNodes)
                {
                    this.Headers.Add(node.Attributes["Key"].Value, node.InnerText);
                }
            }

            XmlNode subjectNode = GetConfigNode(xml, "MailMessage/Subject");
            this.Subject = subjectNode.InnerText;

            XmlNode bodyNode = GetConfigNode(xml, "MailMessage/Body");
            this.Body = bodyNode.InnerText;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
