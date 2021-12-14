using System;
using System.IO;
using System.Text;

namespace MailQueueNet.Grpc
{
    public partial class MailMessage
    {
        public static MailMessage FromMessage(System.Net.Mail.MailMessage message)
        {
            var proto = new MailMessage
            {
                Priority = message.Priority.ToString(),
                BodyEncoding = message.BodyEncoding?.WebName,
                HeadersEncoding = message.HeadersEncoding?.WebName,
                SubjectEncoding = message.SubjectEncoding?.WebName,
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsBodyHtml,
                DeliveryNotificationOptions = message.DeliveryNotificationOptions.ToString(),
                From = message.From == null ? null : MailAddress.From(message.From),
                Sender = message.Sender == null ? null : MailAddress.From(message.Sender),
            };

            foreach (var address in message.To)
            {
                proto.To.Add(MailAddress.From(address));
            }

            foreach (var address in message.CC)
            {
                proto.Cc.Add(MailAddress.From(address));
            }

            foreach (var address in message.Bcc)
            {
                proto.Bcc.Add(MailAddress.From(address));
            }

            foreach (var address in message.ReplyToList)
            {
                proto.ReplyTo.Add(MailAddress.From(address));
            }

            if (message.Headers.Count > 0)
            {
                foreach (var key in message.Headers.AllKeys)
                {
                    foreach (var value in message.Headers.GetValues(key))
                    {
                        proto.Headers.Add(new Header { Name = key, Value = value });
                    }
                }
            }

            foreach (var attachment in message.Attachments)
            {
                if (!(attachment.ContentStream is FileStream)) continue;

                proto.Attachments.Add(Attachment.From(attachment));
            }

            return proto;
        }

        public System.Net.Mail.MailMessage ToSystemType()
        {
            var message = new System.Net.Mail.MailMessage
            {
                Priority = (System.Net.Mail.MailPriority)Enum.Parse(typeof(System.Net.Mail.MailPriority), Priority),
                BodyEncoding = BodyEncoding == null ? null : Encoding.GetEncoding(BodyEncoding),
                HeadersEncoding = HeadersEncoding == null ? null : Encoding.GetEncoding(HeadersEncoding),
                SubjectEncoding = SubjectEncoding == null ? null : Encoding.GetEncoding(SubjectEncoding),
                Subject = Subject,
                Body = Body,
                IsBodyHtml = IsBodyHtml,
                DeliveryNotificationOptions = (System.Net.Mail.DeliveryNotificationOptions)Enum.Parse(typeof(System.Net.Mail.DeliveryNotificationOptions), DeliveryNotificationOptions),
                From = From?.ToSystemType(),
                Sender = Sender?.ToSystemType(),
            };

            foreach (var address in To)
            {
                message.To.Add(address.ToSystemType());
            }

            foreach (var address in Cc)
            {
                message.CC.Add(address.ToSystemType());
            }

            foreach (var address in Bcc)
            {
                message.Bcc.Add(address.ToSystemType());
            }

            foreach (var address in ReplyTo)
            {
                message.ReplyToList.Add(address.ToSystemType());
            }

            if (Headers.Count > 0)
            {
                foreach (var header in Headers)
                {
                    message.Headers.Add(header.Name, header.Value);
                }
            }

            foreach (var attachment in Attachments)
            {
                message.Attachments.Add(attachment.ToSystemType());
            }

            return message;
        }

        public static bool Serializable(System.Net.Mail.MailMessage message)
        {
            foreach (var attachment in message.Attachments)
            {
                if (!(attachment.ContentStream is FileStream)) return false;
            }
            return true;
        }
    }
}
