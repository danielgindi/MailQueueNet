using System;
using System.IO;
using System.Text;

namespace MailQueueNet.Grpc
{
    public partial class Attachment
    {
        public static Attachment From(System.Net.Mail.Attachment attachment)
        {
            var stream = attachment.ContentStream as FileStream;
            if (stream == null) return null;

            var proto = new Attachment
            {
                FileName = stream.Name,
                Name = attachment.Name,
                NameEncoding = attachment.NameEncoding?.WebName,
                ContentId = attachment.ContentId,
                ContentType = attachment.ContentType.MediaType,
                TransferEncoding = attachment.TransferEncoding.ToString(),
            };

            if (attachment.ContentDisposition != null)
            {
                proto.ContentDisposition = new ContentDisposition
                {
                    DispositionType = attachment.ContentDisposition.DispositionType,
                    Inline = attachment.ContentDisposition.Inline,
                    FileName = attachment.ContentDisposition.FileName,
                    Size = attachment.ContentDisposition.Size
                };

                if (attachment.ContentDisposition.ReadDate != DateTime.MinValue)
                    proto.ContentDisposition.CreationDate = attachment.ContentDisposition.CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

                if (attachment.ContentDisposition.ModificationDate != DateTime.MinValue)
                    proto.ContentDisposition.ModificationDate = attachment.ContentDisposition.ModificationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

                if (attachment.ContentDisposition.ReadDate != DateTime.MinValue)
                    proto.ContentDisposition.ReadDate = attachment.ContentDisposition.ReadDate.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

                foreach (string key in attachment.ContentDisposition.Parameters.Keys)
                {
                    attachment.ContentDisposition.Parameters[key] = attachment.ContentDisposition.Parameters[key];
                }
            }

            return proto;
        }

        public System.Net.Mail.Attachment ToSystemType()
        {
            var attachment = ShouldDelete
                ? new AttachmentEx(FileName) { ShouldDeleteFile = true }
                : new System.Net.Mail.Attachment(FileName);

            if (Name != null)
            {
                attachment.Name = Name;
            }

            if (NameEncoding != null)
            {
                attachment.NameEncoding = Encoding.GetEncoding(NameEncoding);
            }

            if (ContentId != null)
            {
                attachment.ContentId = ContentId;
            }

            if (ContentType != null)
            {
                attachment.ContentType = new System.Net.Mime.ContentType(ContentType);
            }

            if (TransferEncoding != null)
            {
                attachment.TransferEncoding = (System.Net.Mime.TransferEncoding)Enum.Parse(typeof(System.Net.Mime.TransferEncoding), TransferEncoding);
            }

            if (ContentDisposition != null)
            {
                if (ContentDisposition.DispositionType != null)
                {
                    attachment.ContentDisposition.DispositionType = ContentDisposition.DispositionType;
                }

                attachment.ContentDisposition.Inline = ContentDisposition.Inline;

                if (ContentDisposition.FileName != null)
                {
                    attachment.ContentDisposition.FileName = ContentDisposition.FileName;
                }

                if (ContentDisposition.CreationDate != null)
                {
                    attachment.ContentDisposition.CreationDate = DateTime.ParseExact(ContentDisposition.CreationDate, "yyyy-MM-ddTHH:mm:ss.fffzzz", null);
                }

                if (ContentDisposition.ModificationDate != null)
                {
                    attachment.ContentDisposition.ModificationDate = DateTime.ParseExact(ContentDisposition.ModificationDate, "yyyy-MM-ddTHH:mm:ss.fffzzz", null);
                }

                if (ContentDisposition.ReadDate != null)
                {
                    attachment.ContentDisposition.ReadDate = DateTime.ParseExact(ContentDisposition.ReadDate, "yyyy-MM-ddTHH:mm:ss.fffzzz", null);
                }

                attachment.ContentDisposition.Size = Convert.ToInt64(ContentDisposition.Size);

                if (ContentDisposition.Params != null)
                {
                    foreach (var param in ContentDisposition.Params)
                    {
                        try
                        {
                            attachment.ContentDisposition.Parameters[param.Key] = param.Value;
                        }
                        catch { }
                    }
                }
            }

            return attachment;
        }
    }
}
