using Grpc.Core;

namespace MailQueueNet.Grpc
{
    public partial class MailGrpcService
    {
        public partial class MailGrpcServiceClient
        {
            public virtual MailMessageReply QueueMail(System.Net.Mail.MailMessage message, Metadata headers = null, System.DateTime? deadline = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                return QueueMail(MailMessage.FromMessage(message), headers, deadline, cancellationToken);
            }

            public virtual MailMessageReply QueueMail(System.Net.Mail.MailMessage message, CallOptions options)
            {
                return QueueMail(MailMessage.FromMessage(message), options);
            }

            public virtual AsyncUnaryCall<MailMessageReply> QueueMailAsync(System.Net.Mail.MailMessage message, Metadata headers = null, System.DateTime? deadline = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                return QueueMailAsync(MailMessage.FromMessage(message), headers, deadline, cancellationToken);
            }

            public virtual AsyncUnaryCall<MailMessageReply> QueueMailAsync(System.Net.Mail.MailMessage message, CallOptions options)
            {
                return QueueMailAsync(MailMessage.FromMessage(message), options);
            }
            public virtual MailMessageReply QueueMailWithSettings(System.Net.Mail.MailMessage message, MailSettings settings, Metadata headers = null, System.DateTime? deadline = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                return QueueMailWithSettings(new MailMessageWithSettings
                {
                    Message = MailMessage.FromMessage(message),
                    Settings = settings,
                }, headers, deadline, cancellationToken);
            }

            public virtual MailMessageReply QueueMailWithSettings(System.Net.Mail.MailMessage message, MailSettings settings, CallOptions options)
            {
                return QueueMailWithSettings(new MailMessageWithSettings
                {
                    Message = MailMessage.FromMessage(message),
                    Settings = settings,
                }, options);
            }

            public virtual AsyncUnaryCall<MailMessageReply> QueueMailAsyncWithSettings(System.Net.Mail.MailMessage message, MailSettings settings, Metadata headers = null, System.DateTime? deadline = null, System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            {
                return QueueMailWithSettingsAsync(new MailMessageWithSettings
                {
                    Message = MailMessage.FromMessage(message),
                    Settings = settings,
                }, headers, deadline, cancellationToken);
            }

            public virtual AsyncUnaryCall<MailMessageReply> QueueMailAsyncWithSettings(System.Net.Mail.MailMessage message, MailSettings settings, CallOptions options)
            {
                return QueueMailWithSettingsAsync(new MailMessageWithSettings
                {
                    Message = MailMessage.FromMessage(message),
                    Settings = settings,
                }, options);
            }
        }
    }
}
