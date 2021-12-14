using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MailQueueNet.Service.Core;
using MailQueueNet.Service.Internal;

namespace MailQueueNet.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> Logger;
        private readonly ILoggerFactory LoggerFactory;
        private readonly IConfiguration Configuration;
        private readonly Coordinator Coordinator;
        private readonly Debouncer _Debouncer = new(TimeSpan.FromSeconds(0.5));
        private IDisposable _RegisteredOnChange = null;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, ILoggerFactory loggerFactory, Coordinator coordinator)
        {
            Logger = logger;
            LoggerFactory = loggerFactory;
            Configuration = configuration;
            Coordinator = coordinator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _RegisteredOnChange?.Dispose();
                _RegisteredOnChange = ChangeToken.OnChange<object>(Configuration.GetReloadToken, async (_) => await _Debouncer.Debounce(() => Coordinator.RefreshSettings()), null);
                Coordinator.RefreshSettings();

                await Coordinator.Run(stoppingToken).ConfigureAwait(false);
            }
            finally
            {
                _RegisteredOnChange?.Dispose();
            }
        }
    }
}
