using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace MailQueueNet.Service.Core
{
    public static class SettingsController
    {
        public static JObject ReadSettingsForUpdate()
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");

            using (var reader = new StringReader(File.ReadAllText(filePath, Encoding.UTF8)))
            using (var jsonReader = new JsonTextReader(reader)
            {
                DateParseHandling = DateParseHandling.None,
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            })
            {
                return JObject.Load(jsonReader);
            }
        }

        public static void CommitSettingsUpdates(JObject jSettings)
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "appSettings.json");

            File.WriteAllText(filePath, jSettings.ToString(Formatting.Indented), Encoding.UTF8);
        }

        public static void AddOrUpdateAppSetting<T>(JObject jSettings, string key, T value)
        {
            JToken jElement = jSettings;

            var keyParts = key.Split(":");
            for (var i = 0; i < keyParts.Length - 1; i++)
            {
                if (!(jElement is JObject))
                    return;

                var keyPart = keyParts[i];
                if (((JObject)jElement).ContainsKey(keyPart))
                {
                    jElement = jElement[keyPart];
                }
                else
                {
                    var jEl = new JObject();
                    jElement[keyPart] = jEl;
                    jElement = jEl;
                }
            }

            var lastKey = keyParts[keyParts.Length - 1];
            jElement[lastKey] = value == null ? null : new JValue(value);
        }

        public static void AddOrUpdateAppSetting<T>(string key, T value)
        {
            var jSettings = ReadSettingsForUpdate();
            AddOrUpdateAppSetting(jSettings, key, value);
            CommitSettingsUpdates(jSettings);
        }

        public static void SetSettings(Grpc.Settings settings)
        {
            var jSettings = ReadSettingsForUpdate();

            AddOrUpdateAppSetting(jSettings, "queue:queue_folder", settings.QueueFolder);
            AddOrUpdateAppSetting(jSettings, "queue:failed_folder", settings.FailedFolder);
            AddOrUpdateAppSetting(jSettings, "queue:seconds_until_folder_refresh", settings.SecondsUntilFolderRefresh);
            AddOrUpdateAppSetting(jSettings, "queue:maximum_concurrent_workers", settings.MaximumConcurrentWorkers);
            AddOrUpdateAppSetting(jSettings, "queue:maximum_failure_retries", settings.MaximumFailureRetries);

            CommitSettingsUpdates(jSettings);
        }

        public static void SetMailSettings(Grpc.MailSettings settings)
        {
            var jSettings = ReadSettingsForUpdate();

            switch (settings?.SettingsCase)
            {
                case Grpc.MailSettings.SettingsOneofCase.Smtp:
                    {
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:server", settings.Smtp.Host);
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:port", settings.Smtp.Port);
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:ssl", settings.Smtp.RequiresSsl);
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:authentication", settings.Smtp.RequiresAuthentication);
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:username", settings.Smtp.Username);
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:password", settings.Smtp.Password);
                        AddOrUpdateAppSetting(jSettings, "queue:smtp:connection_timeout", settings.Smtp.ConnectionTimeout);
                    }
                    break;
                case Grpc.MailSettings.SettingsOneofCase.Mailgun:
                    {
                        AddOrUpdateAppSetting(jSettings, "queue:mailgun:domain", settings.Mailgun.Domain);
                        AddOrUpdateAppSetting(jSettings, "queue:mailgun:api_key", settings.Mailgun.ApiKey);
                    }
                    break;
                default:
                    {
                        AddOrUpdateAppSetting("queue:mail_service_type", "");
                    }
                    break;
            }

            CommitSettingsUpdates(jSettings);
        }

        public static Grpc.Settings GetSettings(IConfiguration configuration)
        {
            return new Grpc.Settings
            {
                QueueFolder = configuration.GetValue("queue:queue_folder", "~/mail/queue"),
                FailedFolder = configuration.GetValue("queue:failed_folder", "~/mail/failed"),
                SecondsUntilFolderRefresh = configuration.GetValue("queue:seconds_until_folder_refresh", 10.0f),
                MaximumConcurrentWorkers = configuration.GetValue("queue:maximum_concurrent_workers", 4),
                MaximumFailureRetries = configuration.GetValue("queue:maximum_failure_retries", 5),
            };
        }

        public static Grpc.MailSettings GetMailSettings(IConfiguration configuration)
        {
            switch (configuration.GetValue("queue:mail_service_type", "smtp"))
            {
                case "smtp":
                    {
                        return new Grpc.MailSettings
                        {
                            Smtp = new Grpc.SmtpMailSettings
                            {
                                Host = configuration.GetValue("queue:smtp:server", ""),
                                Port = configuration.GetValue("queue:smtp:port", 0),
                                RequiresSsl = configuration.GetValue("queue:smtp:ssl", false),
                                RequiresAuthentication = configuration.GetValue("queue:smtp:authentication", false),
                                Username = configuration.GetValue("queue:smtp:username", ""),
                                Password = configuration.GetValue("queue:smtp:password", ""),
                                ConnectionTimeout = configuration.GetValue("queue:smtp:connection_timeout", 100000),
                            }
                        };
                    }

                case "mailgun":
                    {
                        return new Grpc.MailSettings
                        {
                            Mailgun = new Grpc.MailgunMailSettings
                            {
                                Domain = configuration.GetValue("queue:mailgun:domain", ""),
                                ApiKey = configuration.GetValue("queue:mailgun:api_key", ""),
                                ConnectionTimeout = configuration.GetValue("queue:mailgun:connection_timeout", 100000),
                            }
                        };
                    }
            }

            // Something really empty that won't send anything
            return new Grpc.MailSettings();
        }
    }
}
