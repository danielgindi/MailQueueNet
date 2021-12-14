namespace MailQueueNet.Grpc
{
    public partial class MailSettings
    {
        public bool IsEmpty()
        {
            if (SettingsCase == SettingsOneofCase.Mailgun && Mailgun != null)
            {
                return string.IsNullOrEmpty(Mailgun.Domain)
                    || string.IsNullOrEmpty(Mailgun.ApiKey);
            }
            else if (SettingsCase == SettingsOneofCase.Smtp && Smtp != null)
            {
                return string.IsNullOrEmpty(Smtp.Host);
            }

            return true;
        }
    }
}
