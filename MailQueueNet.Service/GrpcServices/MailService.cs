using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using MailQueueNet.Service.Core;
using Microsoft.Extensions.Configuration;
using System;

namespace MailQueueNet.Service.GrpcServices
{
    internal class MailService : Grpc.MailGrpcService.MailGrpcServiceBase
    {
        private readonly ILogger<MailService> _Logger;
        private readonly IConfiguration _Configuration;
        private readonly Coordinator _Coordinator;

        public MailService(ILogger<MailService> logger, IConfiguration configuration, Coordinator coordinator)
        {
            _Logger = logger;
            _Configuration = configuration;
            _Coordinator = coordinator;
        }

        public override Task<Grpc.MailMessageReply> QueueMail(Grpc.MailMessage request, ServerCallContext context)
        {
            var success = false;

            try
            {
                _Coordinator.AddMail(request);
                success = true;
            }
            catch (Exception ex)
            {
                _Logger?.LogError(ex, $"Exception thrown for QueueMail");
            }

            return Task.FromResult(new Grpc.MailMessageReply
            {
                Success = success,
            });
        }

        public override Task<Grpc.MailMessageReply> QueueMailWithSettings(Grpc.MailMessageWithSettings request, ServerCallContext context)
        {
            var success = false;

            try
            {
                _Coordinator.AddMail(request);
                success = true;
            }
            catch (Exception ex)
            {
                _Logger?.LogError(ex, $"Exception thrown for QueueMail");
            }

            return Task.FromResult(new Grpc.MailMessageReply
            {
                Success = success,
            });
        }

        public override Task<Grpc.SetMailSettingsReply> SetMailSettings(Grpc.SetMailSettingsMessage request, ServerCallContext context)
        {
            SettingsController.SetMailSettings(request.Settings);
            return Task.FromResult(new Grpc.SetMailSettingsReply { });
        }

        public override Task<Grpc.GetMailSettingsReply> GetMailSettings(Grpc.GetMailSettingsMessage request, ServerCallContext context)
        {
            return Task.FromResult(new Grpc.GetMailSettingsReply { Settings = SettingsController.GetMailSettings(_Configuration) });
        }

        public override Task<Grpc.SetSettingsReply> SetSettings(Grpc.SetSettingsMessage request, ServerCallContext context)
        {
            SettingsController.SetSettings(request.Settings);
            return Task.FromResult(new Grpc.SetSettingsReply { });
        }

        public override Task<Grpc.GetSettingsReply> GetSettings(Grpc.GetSettingsMessage request, ServerCallContext context)
        {
            return Task.FromResult(new Grpc.GetSettingsReply { Settings = SettingsController.GetSettings(_Configuration) });
        }
    }
}
