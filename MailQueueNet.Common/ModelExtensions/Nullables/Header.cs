namespace MailQueueNet.Grpc
{
    public partial class Header
    {
        public string Name
        {
            get { return OptNameCase == OptNameOneofCase.None ? null : OptNameValue; }
            set { if (value == null) ClearOptName(); else OptNameValue = value; }
        }

        public string Value
        {
            get { return OptValueCase == OptValueOneofCase.None ? null : OptValueValue; }
            set { if (value == null) ClearOptValue(); else OptValueValue = value; }
        }
    }
}
