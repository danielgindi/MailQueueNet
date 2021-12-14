namespace MailQueueNet.Grpc
{
    public partial class Attachment
    {
        public string Name
        {
            get { return OptNameCase == OptNameOneofCase.None ? null : OptNameValue; }
            set { if (value == null) ClearOptName(); else OptNameValue = value; }
        }

        public string NameEncoding
        {
            get { return OptNameEncodingCase == OptNameEncodingOneofCase.None ? null : OptNameEncodingValue; }
            set { if (value == null) ClearOptNameEncoding(); else OptNameEncodingValue = value; }
        }

        public string ContentId
        {
            get { return OptContentIdCase == OptContentIdOneofCase.None ? null : OptContentIdValue; }
            set { if (value == null) ClearOptContentId(); else OptContentIdValue = value; }
        }

        public string FileName
        {
            get { return OptFileNameCase == OptFileNameOneofCase.None ? null : OptFileNameValue; }
            set { if (value == null) ClearOptFileName(); else OptFileNameValue = value; }
        }

        public string ContentType
        {
            get { return OptContentTypeCase == OptContentTypeOneofCase.None ? null : OptContentTypeValue; }
            set { if (value == null) ClearOptContentType(); else OptContentTypeValue = value; }
        }

        public string TransferEncoding
        {
            get { return OptTransferEncodingCase == OptTransferEncodingOneofCase.None ? null : OptTransferEncodingValue; }
            set { if (value == null) ClearOptTransferEncoding(); else OptTransferEncodingValue = value; }
        }
    }
}
