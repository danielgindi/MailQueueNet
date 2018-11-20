using System.IO;
using System.Text;

namespace MailQueue
{
    public static class SettingsController
    {
        public static void SetMailSettings(IMailServerSettings settings)
        {
            if (settings == null)
            {
                Properties.Settings.Default.MailServiceType = "";
            }
            else
            {
                Properties.Settings.Default.MailServiceType = settings.Type;

                if (settings is SmtpMailServerSettings)
                {
                    var smtpSettings = settings as SmtpMailServerSettings;

                    Properties.Settings.Default.SmtpServer = smtpSettings.Host;
                    Properties.Settings.Default.SmtpPort = smtpSettings.Port;
                    Properties.Settings.Default.SmtpSsl = smtpSettings.RequiresSsl;
                    Properties.Settings.Default.SmtpAuthentication = smtpSettings.RequiresAuthentication;
                    Properties.Settings.Default.SmtpUsername = smtpSettings.Username;
                    Properties.Settings.Default.SmtpPassword = smtpSettings.Password;
                }
                else if (settings is MailgunMailServerSettings)
                {
                    var mgSettings = settings as MailgunMailServerSettings;

                    Properties.Settings.Default.MailgunDomain = mgSettings.Domain;
                    Properties.Settings.Default.MailgunApiKey = mgSettings.ApiKey;
                }
                else
                {
                    using (var writer = new StringWriter(new StringBuilder()))
                    {
                        IMailServerSettings.GetSerializer().Serialize(writer, settings);
                        Properties.Settings.Default.MailServiceSettings = writer.GetStringBuilder().ToString();
                    }
                }
            }

            Properties.Settings.Default.Save();
        }

        public static IMailServerSettings GetMailSettings()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.MailServiceType) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.MailServiceSettings))
            {
                try
                {
                    using (var reader = new StringReader(Properties.Settings.Default.MailServiceSettings))
                    {
                        return IMailServerSettings.GetSerializer().Deserialize(reader) as IMailServerSettings;
                    }
                }
                catch
                {
                    // Try other settings stores, like those single values
                }
            }

            switch (Properties.Settings.Default.MailServiceType)
            {
                case SmtpMailServerSettings.TYPE:
                    {
                        var settings = new SmtpMailServerSettings();
                        settings.Host = Properties.Settings.Default.SmtpServer;
                        settings.Port = Properties.Settings.Default.SmtpPort;
                        settings.RequiresSsl = Properties.Settings.Default.SmtpSsl;
                        settings.RequiresAuthentication = Properties.Settings.Default.SmtpAuthentication;
                        settings.Username = Properties.Settings.Default.SmtpUsername;
                        settings.Password = Properties.Settings.Default.SmtpPassword;
                        return settings;
                    }

                case MailgunMailServerSettings.TYPE:
                    {
                        var settings = new MailgunMailServerSettings();
                        settings.Domain = Properties.Settings.Default.MailgunDomain;
                        settings.ApiKey = Properties.Settings.Default.MailgunApiKey;
                        return settings;
                    }
            }

            // Something really empty that won't send anything
            return new SmtpMailServerSettings();
        }
    }
}
