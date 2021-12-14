namespace MailQueueNet.Grpc
{
    public partial class MailMessage
    {
        public string Body
        {
            get { return OptBodyCase == OptBodyOneofCase.None ? null : OptBodyValue; }
            set { if (value == null) ClearOptBody(); else OptBodyValue = value; }
        }

        public string BodyEncoding
        {
            get { return OptBodyEncodingCase == OptBodyEncodingOneofCase.None ? null : OptBodyEncodingValue; }
            set { if (value == null) ClearOptBodyEncoding(); else OptBodyEncodingValue = value; }
        }

        public string Subject
        {
            get { return OptSubjectCase == OptSubjectOneofCase.None ? null : OptSubjectValue; }
            set { if (value == null) ClearOptSubject(); else OptSubjectValue = value; }
        }

        public string SubjectEncoding
        {
            get { return OptSubjectEncodingCase == OptSubjectEncodingOneofCase.None ? null : OptSubjectEncodingValue; }
            set { if (value == null) ClearOptSubjectEncoding(); else OptSubjectEncodingValue = value; }
        }

        public string HeadersEncoding
        {
            get { return OptHeadersEncodingCase == OptHeadersEncodingOneofCase.None ? null : OptHeadersEncodingValue; }
            set { if (value == null) ClearOptHeadersEncoding(); else OptHeadersEncodingValue = value; }
        }

        public string Priority
        {
            get { return OptPriorityCase == OptPriorityOneofCase.None ? null : OptPriorityValue; }
            set { if (value == null) ClearOptPriority(); else OptPriorityValue = value; }
        }

        public string DeliveryNotificationOptions
        {
            get { return OptDeliveryNotificationOptionsCase == OptDeliveryNotificationOptionsOneofCase.None ? null : OptDeliveryNotificationOptionsValue; }
            set { if (value == null) ClearOptDeliveryNotificationOptions(); else OptDeliveryNotificationOptionsValue = value; }
        }
    }
}
