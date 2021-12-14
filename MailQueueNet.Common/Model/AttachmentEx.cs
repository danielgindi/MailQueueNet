using System.Net.Mail;
using System.IO;
using System.Net.Mime;

namespace MailQueueNet
{
    public class AttachmentEx : Attachment
    {
        public AttachmentEx(string fileName) : base(fileName)
        {
        }

        public AttachmentEx(string fileName, string mediaType) : base(fileName, mediaType)
        {
        }

        public AttachmentEx(string fileName, ContentType contentType) : base(fileName, contentType)
        {
        }

        public AttachmentEx(Stream contentStream, string name) : base(contentStream, name)
        {
        }

        public AttachmentEx(Stream contentStream, ContentType contentType) : base(contentStream, contentType)
        {
        }

        public AttachmentEx(Stream contentStream, string name, string mediaType) : base(contentStream, name, mediaType)
        {
        }

        public bool ShouldDeleteFile { get; set; } = false;
    }
}
