namespace MailQueueNet.Grpc
{
    public partial class SmtpMailSettings
    {
        public string Host
        {
            get { return OptHostCase == OptHostOneofCase.None ? null : OptHostValue; }
            set { if (value == null) ClearOptHost(); else OptHostValue = value; }
        }

        public string Username
        {
            get { return OptUsernameCase == OptUsernameOneofCase.None ? null : OptUsernameValue; }
            set { if (value == null) ClearOptUsername(); else OptUsernameValue = value; }
        }

        public string Password
        {
            get { return OptPasswordCase == OptPasswordOneofCase.None ? null : OptPasswordValue; }
            set { if (value == null) ClearOptPassword(); else OptPasswordValue = value; }
        }
    }
}
