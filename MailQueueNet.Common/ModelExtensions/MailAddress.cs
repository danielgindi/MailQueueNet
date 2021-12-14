namespace MailQueueNet.Grpc
{
    public partial class MailAddress
    {
        public static MailAddress From(System.Net.Mail.MailAddress address)
        {
            return new MailAddress
            {
                Address = address.Address,
                DisplayName = address.DisplayName,
            };
        }

        public System.Net.Mail.MailAddress ToSystemType()
        {
            return new System.Net.Mail.MailAddress(Address, DisplayName);
        }
    }
}
