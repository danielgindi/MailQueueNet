using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dg.MailQueue
{
    public class MultipartRequest
    {
        private const string MULTIPART_BOUNDARY_CHARS = "-_1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static byte[] CRLF_BYTES = System.Text.Encoding.ASCII.GetBytes("\r\n");
        private static byte[] MULTIPART_HEADER_CONTENT_DISPOSITION_AND_NAME_BYTES = System.Text.Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"");
        private static byte[] MULTIPART_HEADER_END_NAME_AND_FILENAME = System.Text.Encoding.UTF8.GetBytes("\"; filename=\"");
        private static byte[] MULTIPART_HEADER_END = System.Text.Encoding.UTF8.GetBytes("\"");
        private static byte[] MULTIPART_HEADER_CONTENT_TYPE = System.Text.Encoding.UTF8.GetBytes("Content-Type: ");
        private static byte[] MULTIPART_HEADER_CONTENT_TYPE_END_CHARSET = System.Text.Encoding.UTF8.GetBytes("; charset=");

        public string Boundary { get; set; }

        public string ContentTypeHeader
        {
            get
            {
                return "multipart/form-data; boundary=" + Boundary;
            }
        }

        private Dictionary<string, List<MultipartField>> _Fields = new Dictionary<string, List<MultipartField>>();
        public Dictionary<string, List<MultipartField>> Fields
        {
            get { return _Fields; }
            set { _Fields = value ?? new Dictionary<string, List<MultipartField>>(); }
        }

        public MultipartRequest()
        {
            Boundary = GenerateBoundary();
        }

        public void AddField(string key, object value)
        {
            if (!Fields.ContainsKey(key))
            {
                Fields[key] = new List<MultipartField>();
            }

            Fields[key].Add(new MultipartTextField
            {
                Value = ParamToString(value)
            });
        }

        public void SetField(string key, object value)
        {
            Fields[key] = new List<MultipartField>();
            Fields[key].Add(new MultipartTextField
            {
                Value = ParamToString(value)
            });
        }

        public void AddField(string key, MultipartTextField value)
        {
            if (!Fields.ContainsKey(key))
            {
                Fields[key] = new List<MultipartField>();
            }

            Fields[key].Add(value);
        }

        public void SetField(string key, MultipartTextField value)
        {
            Fields[key] = new List<MultipartField>();
            Fields[key].Add(value);
        }

        public void AddFile(string key, string filePath, string fileName = null, string contentType = null)
        {
            if (!Fields.ContainsKey(key))
            {
                Fields[key] = new List<MultipartField>();
            }

            Fields[key].Add(new MultipartFileField
            {
                FilePath = filePath,
                FileName = fileName,
                ContentType = contentType
            });
        }

        public void SetField(string key, string filePath, string fileName = null, string contentType = null)
        {
            Fields[key] = new List<MultipartField>();
            Fields[key].Add(new MultipartFileField
            {
                FilePath = filePath,
                FileName = fileName,
                ContentType = contentType
            });
        }

        private static void WriteToStream(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        private static Task WriteToStreamAsync(Stream stream, byte[] data)
        {
            return stream.WriteAsync(data, 0, data.Length);
        }

        private void WritePartHeader(Stream stream, string name, MultipartField part)
        {
            var contentType = part.ContentType;

            WriteToStream(stream, MULTIPART_HEADER_CONTENT_DISPOSITION_AND_NAME_BYTES);
            WriteToStream(stream, Encoding.UTF8.GetBytes(Uri.EscapeDataString(name)));

            var isFile = part is MultipartFileField;

            if (isFile)
            {
                contentType = contentType ?? "application/octet-stream";

                var field = part as MultipartFileField;

                if (!string.IsNullOrEmpty(field.FileName))
                {
                    WriteToStream(stream, MULTIPART_HEADER_END_NAME_AND_FILENAME);
                    WriteToStream(stream, Encoding.UTF8.GetBytes(field.FileName.Replace("\"", "")));
                }
            }
            WriteToStream(stream, MULTIPART_HEADER_END);
            WriteToStream(stream, CRLF_BYTES);
            
            if (!string.IsNullOrEmpty(contentType))
            {
                WriteToStream(stream, MULTIPART_HEADER_CONTENT_TYPE);
                WriteToStream(stream, Encoding.UTF8.GetBytes(contentType));
                if (!isFile)
                {
                    WriteToStream(stream, MULTIPART_HEADER_CONTENT_TYPE_END_CHARSET);
                    WriteToStream(stream, Encoding.UTF8.GetBytes("utf-8"));
                }
                WriteToStream(stream, CRLF_BYTES);
            }
        }

        private async Task WritePartHeaderAsync(Stream stream, string name, MultipartField part)
        {
            var contentType = part.ContentType;

            await WriteToStreamAsync(stream, MULTIPART_HEADER_CONTENT_DISPOSITION_AND_NAME_BYTES);
            await WriteToStreamAsync(stream, Encoding.UTF8.GetBytes(Uri.EscapeDataString(name)));

            var isFile = part is MultipartFileField;

            if (isFile)
            {
                contentType = contentType ?? "application/octet-stream";

                var field = part as MultipartFileField;

                if (!string.IsNullOrEmpty(field.FileName))
                {
                    await WriteToStreamAsync(stream, MULTIPART_HEADER_END_NAME_AND_FILENAME);
                    await WriteToStreamAsync(stream, Encoding.UTF8.GetBytes(field.FileName.Replace("\"", "")));
                }
            }
            await WriteToStreamAsync(stream, MULTIPART_HEADER_END);
            await WriteToStreamAsync(stream, CRLF_BYTES);
            
            if (!string.IsNullOrEmpty(contentType))
            {
                await WriteToStreamAsync(stream, MULTIPART_HEADER_CONTENT_TYPE);
                await WriteToStreamAsync(stream, Encoding.UTF8.GetBytes(contentType));
                if (!isFile)
                {
                    await WriteToStreamAsync(stream, MULTIPART_HEADER_CONTENT_TYPE_END_CHARSET);
                    await WriteToStreamAsync(stream, Encoding.UTF8.GetBytes("utf-8"));
                }
                await WriteToStreamAsync(stream, CRLF_BYTES);
            }
        }

        private void WriteDataToStream(Stream stream, MultipartField part)
        {
            if (part is MultipartFileField)
            {
                using (var inputStream = new FileStream(((MultipartFileField)part).FilePath, FileMode.Open))
                {
                    inputStream.CopyTo(stream);
                }
            }
            else if (part is MultipartTextField)
            {
                WriteToStream(stream, Encoding.UTF8.GetBytes(((MultipartTextField)part).Value));
            }
        }

        private async Task WriteDataToStreamAsync(Stream stream, MultipartField part)
        {
            if (part is MultipartFileField)
            {
                using (var inputStream = new FileStream(((MultipartFileField)part).FilePath, FileMode.Open))
                {
                    await inputStream.CopyToAsync(stream);
                }
            }
            else if (part is MultipartTextField)
            {
                await WriteToStreamAsync(stream, Encoding.UTF8.GetBytes(((MultipartTextField)part).Value));
            }
        }

        public void Send(Stream stream)
        {
            var boundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + Boundary);
            var boundaryEndBytes = System.Text.Encoding.UTF8.GetBytes("--" + Boundary + "--");

            foreach (var pair in Fields)
            {
                foreach (var part in pair.Value)
                {
                    WriteToStream(stream, boundaryBytes);
                    WriteToStream(stream, CRLF_BYTES);
                    WritePartHeader(stream, pair.Key, part);
                    WriteToStream(stream, CRLF_BYTES);

                    // Multipart body
                    WriteDataToStream(stream, part);
                    WriteToStream(stream, CRLF_BYTES);
                }
            }

            // Prologue boundary...
            stream.Write(boundaryEndBytes, 0, boundaryEndBytes.Length);
            stream.Write(CRLF_BYTES, 0, CRLF_BYTES.Length);
        }

        public async Task SendAsync(Stream stream)
        {
            var boundaryBytes = System.Text.Encoding.UTF8.GetBytes("--" + Boundary);
            var boundaryEndBytes = System.Text.Encoding.UTF8.GetBytes("--" + Boundary + "--");

            foreach (var pair in Fields)
            {
                foreach (var part in pair.Value)
                {
                    await WriteToStreamAsync(stream, boundaryBytes);
                    await WriteToStreamAsync(stream, CRLF_BYTES);
                    await WritePartHeaderAsync(stream, pair.Key, part);
                    await WriteToStreamAsync(stream, CRLF_BYTES);

                    // Multipart body
                    await WriteDataToStreamAsync(stream, part);
                    await WriteToStreamAsync(stream, CRLF_BYTES);
                }
            }

            // Prologue boundary...
            await stream.WriteAsync(boundaryEndBytes, 0, boundaryEndBytes.Length);
            await stream.WriteAsync(CRLF_BYTES, 0, CRLF_BYTES.Length);
        }
        
        public static string GenerateBoundary()
        {
            var boundary = "------------------------";

            var rnd = new Random();

            for (int i = 0, len = rnd.Next(11) + 30; i < len; i++)
            {
                boundary += MULTIPART_BOUNDARY_CHARS[rnd.Next(MULTIPART_BOUNDARY_CHARS.Length)];
            }

            return boundary;
        }

        private static string ParamToString(object param)
        {
            if (param is double)
            {
                return ((double)param).ToString();
            }

            if (param is float)
            {
                return ((float)param).ToString();
            }

            if (param is decimal)
            {
                return ((decimal)param).ToString();
            }

            if (param is Int64)
            {
                return ((Int64)param).ToString();
            }

            if (param is Int32)
            {
                return ((Int32)param).ToString();
            }

            if (param is bool)
            {
                return ((bool)param) ? "true" : "false";
            }

            if (param is string)
            {
                return (string)param;
            }

            return param == null ? "" : param.ToString();
        }

        public abstract class MultipartField
        {
            public string ContentType;
        }

        public class MultipartFileField : MultipartField
        {
            public string FilePath;
            public string FileName;
        }

        public class MultipartTextField : MultipartField
        {
            public string Value;
        }
    }
}
