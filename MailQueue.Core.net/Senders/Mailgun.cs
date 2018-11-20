using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MailQueue.Senders
{
    public class Mailgun : ISender
    {
        private static string ComposeEmailDisplayAndAddress(string displayName, string address)
        {
            if (string.IsNullOrEmpty(displayName))
                return address;

            displayName = displayName.Replace("\\", "\\\\");

            return string.Format("\"{0}\" <{1}>", displayName, address);
        }

        public async Task<bool> SendMailAsync(MailMessage message, IMailServerSettings settings)
        {
            var mgSettings = settings as MailgunMailServerSettings;

            if (string.IsNullOrEmpty(mgSettings.Domain))
            {
                return false;
            }

            var request = new MultipartRequest();

            if (message.Subject != null)
            {
                request.AddField("subject", message.Subject);
            }

            if (message.From != null)
            {
                request.AddField("from", ComposeEmailDisplayAndAddress(message.From.DisplayName, message.From.Address));
            }

            if (message.To != null && message.To.Count > 0)
            {
                foreach (var to in message.To)
                {
                    request.AddField("to", ComposeEmailDisplayAndAddress(to.DisplayName, to.Address));
                }
            }

            if (message.CC != null)
            {
                foreach (var to in message.CC)
                {
                    request.AddField("cc", ComposeEmailDisplayAndAddress(to.DisplayName, to.Address));
                }
            }

            if (message.Bcc != null)
            {
                foreach (var to in message.Bcc)
                {
                    request.AddField("bcc", ComposeEmailDisplayAndAddress(to.DisplayName, to.Address));
                }
            }

            if (message.ReplyToList != null && message.ReplyToList.Count > 0)
            {
                request.AddField("h:Reply-To",
                    string.Join(",",
                    message.ReplyToList.Select(x => ComposeEmailDisplayAndAddress(x.DisplayName, x.Address))));
            }

            foreach (var key in message.Headers.AllKeys)
            {
                if (key.StartsWith("h:") || key.StartsWith("o:") || key.StartsWith("v:"))
                {
                    var values = message.Headers.GetValues(key);
                    foreach (var value in values)
                    {
                        request.AddField(key, value);
                    }
                }
                else if (key.StartsWith("Mailgun:")) // Any mailgun header to pass. "Mailgun:" will be stripped from the key.
                {
                    var values = message.Headers.GetValues(key);
                    var keyPart = key.Remove(0, 8);
                    foreach (var value in values)
                    {
                        request.AddField(keyPart, value);
                    }
                }
                else if (key == "X-Mailgun-Tag") // Compatibility with SMTP api
                {
                    var values = message.Headers.GetValues(key);
                    foreach (var value in values)
                    {
                        request.AddField("o:tag", value);
                    }
                }
            }

            var hasHtmlBody = false;
            var hasTextBody = false;

            if (!string.IsNullOrWhiteSpace(message.Body))
            {
                request.AddField(message.IsBodyHtml ? "html" : "text", message.Body);
                if (message.IsBodyHtml) hasHtmlBody = true;
                else hasTextBody = true;
            }

            if (!hasHtmlBody)
            {
                var content = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == "text/html");
                if (content != null)
                    request.AddField("html", content);
            }

            if (!hasTextBody)
            {
                var content = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == "text/plain");
                if (content != null)
                    request.AddField("text", content);
            }

            if (message.Attachments != null)
            {
                foreach (var attachment in message.Attachments)
                {
                    var fileStream = attachment.ContentStream as FileStream;
                    if (fileStream == null) continue;

                    request.AddFile(
                        attachment.ContentId ?? attachment.Name,
                        fileStream.Name,
                        attachment.Name,
                        attachment.ContentType.MediaType);
                }
            }

            var url = string.Format("https://api.mailgun.net/v3/{0}/messages", mgSettings.Domain);

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = request.ContentTypeHeader;
            webRequest.Timeout = mgSettings.ConnectionTimeout;

            webRequest.Headers.Add("Authorization", "basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{mgSettings.ApiKey}")));

            using (var stream = await webRequest.GetRequestStreamAsync())
            {
                await request.SendAsync(stream);
            }

            try
            {
                using (var response = (HttpWebResponse)(await webRequest.GetResponseAsync()))
                {
                    if (response == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new WebException(
                            string.Format("Status {0} returned from MailGun API",
                                response == null ? 0 : (Int32)response.StatusCode));
                    }

                    if (!response.ContentType.StartsWith("application/json"))
                    {
                        throw new WebException(
                            string.Format("Invalid response returned from MailGun API",
                                response == null ? 0 : (Int32)response.StatusCode));
                    }
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse response = null;

                if (ex.Response != null)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                // Figure out more data about the error
                string responseData = "";

                try
                {
                    responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
                }
                catch
                {
                    // Rethrow default
                    throw ex;
                }

                // Rethrow
                throw new WebException(ex.Message + ": " + responseData, ex, ex.Status, ex.Response);
            }

            return true; // Consumed it
        }
    }
}
