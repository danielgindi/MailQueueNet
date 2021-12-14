using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MailQueueNet.Senders
{
    public class Mailgun : ISender
    {
        private HttpClient s_HttpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });

        private static string ComposeEmailDisplayAndAddress(string displayName, string address)
        {
            if (string.IsNullOrEmpty(displayName))
                return address;

            displayName = displayName.Replace("\\", "\\\\");

            return string.Format("\"{0}\" <{1}>", displayName, address);
        }

        public async Task<bool> SendMailAsync(MailMessage message, MailQueueNet.Grpc.MailSettings settings)
        {
            var mgSettings = settings.Mailgun;

            if (string.IsNullOrEmpty(mgSettings.Domain))
            {
                return false;
            }

            using (var request = new MultipartFormDataContent("MP/BOUNDARY/" + Guid.NewGuid().ToString()))
            {
                if (message.Subject != null)
                {
                    request.Add(new StringContent(message.Subject), "subject");
                }

                if (message.From != null)
                {
                    request.Add(new StringContent(ComposeEmailDisplayAndAddress(message.From.DisplayName, message.From.Address)), "from");
                }

                if (message.To != null && message.To.Count > 0)
                {
                    foreach (var to in message.To)
                    {
                        request.Add(new StringContent(ComposeEmailDisplayAndAddress(to.DisplayName, to.Address)), "to");
                    }
                }

                if (message.CC != null)
                {
                    foreach (var to in message.CC)
                    {
                        request.Add(new StringContent(ComposeEmailDisplayAndAddress(to.DisplayName, to.Address)), "cc");
                    }
                }

                if (message.Bcc != null)
                {
                    foreach (var to in message.Bcc)
                    {
                        request.Add(new StringContent(ComposeEmailDisplayAndAddress(to.DisplayName, to.Address)), "bcc");
                    }
                }

                if (message.ReplyToList != null && message.ReplyToList.Count > 0)
                {
                    request.Add(new StringContent(string.Join(",",
                        message.ReplyToList.Select(x => ComposeEmailDisplayAndAddress(x.DisplayName, x.Address)))
                    ), "h:Reply-To");
                }

                foreach (var key in message.Headers.AllKeys)
                {
                    if (key.StartsWith("h:") || key.StartsWith("o:") || key.StartsWith("v:"))
                    {
                        var values = message.Headers.GetValues(key);
                        foreach (var value in values)
                        {
                            request.Add(new StringContent(value), key);
                        }
                    }
                    else if (key.StartsWith("Mailgun:")) // Any mailgun header to pass. "Mailgun:" will be stripped from the key.
                    {
                        var values = message.Headers.GetValues(key);
                        var keyPart = key.Remove(0, 8);
                        foreach (var value in values)
                        {
                            request.Add(new StringContent(value), keyPart);
                        }
                    }
                    else if (key == "X-Mailgun-Tag") // Compatibility with SMTP api
                    {
                        var values = message.Headers.GetValues(key);
                        foreach (var value in values)
                        {
                            request.Add(new StringContent(value), "o:tag");
                        }
                    }
                }

                var hasHtmlBody = false;
                var hasTextBody = false;

                if (!string.IsNullOrWhiteSpace(message.Body))
                {
                    request.Add(new StringContent(message.Body), message.IsBodyHtml ? "html" : "text");
                    if (message.IsBodyHtml) hasHtmlBody = true;
                    else hasTextBody = true;
                }

                if (!hasHtmlBody)
                {
                    var content = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == "text/html");
                    if (content != null)
                        request.Add(new StreamContent(content.ContentStream), "html");
                }

                if (!hasTextBody)
                {
                    var content = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == "text/plain");
                    if (content != null)
                        request.Add(new StreamContent(content.ContentStream), "text");
                }

                if (message.Attachments != null)
                {
                    foreach (var attachment in message.Attachments)
                    {
                        var fileStream = attachment.ContentStream as FileStream;
                        if (fileStream == null) continue;

                        if (attachment.ContentDisposition.Inline)
                        {
                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.ContentType.MediaType);
                            request.Add(fileContent, "inline", attachment.ContentId);
                        }
                        else
                        {
                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.ContentType.MediaType);
                            request.Add(fileContent, "attachment", attachment.Name);
                        }
                    }
                }

                var url = $"https://api.mailgun.net/v3/{mgSettings.Domain}/messages";

                request.Headers.Add("Authorization", "basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{mgSettings.ApiKey}")));

                s_HttpClient.Timeout = TimeSpan.FromMilliseconds(mgSettings.ConnectionTimeout);

                using (var resp = await s_HttpClient.PostAsync(url, request).ConfigureAwait(false))
                {
                    if (!resp.IsSuccessStatusCode)
                    {
                        throw new WebException($"Status {resp.StatusCode} returned from MailGun API: {await resp.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                    }

                    if (!resp.Content.Headers.ContentType.MediaType.StartsWith("application/json"))
                    {
                        throw new WebException($"Invalid response returned from MailGun API: {(await resp.Content.ReadAsStringAsync().ConfigureAwait(false))}");
                    }
                }
            }

            return true; // Consumed it
        }
    }
}
