namespace MailQueueNet.Grpc
{
    public partial class MailAddress
    {
        public string Address
        {
            get { return OptAddressCase == OptAddressOneofCase.None ? null : OptAddressValue; }
            set { if (value == null) ClearOptAddress(); else OptAddressValue = value; }
        }

        public string DisplayName
        {
            get { return OptDisplayNameCase == OptDisplayNameOneofCase.None ? null : OptDisplayNameValue; }
            set { if (value == null) ClearOptDisplayName(); else OptDisplayNameValue = value; }
        }
    }
}
