using System;
using System.Threading;
using System.Threading.Tasks;

namespace MailQueueNet.Service.Internal
{
    internal sealed class Debouncer : IDisposable
    {
        public Debouncer(TimeSpan? delay) => _delay = delay ?? TimeSpan.FromSeconds(2);

        private readonly TimeSpan _delay;
        private CancellationTokenSource previousCancellationToken = null;

        public async Task Debounce(Action action)
        {
            _ = action ?? throw new ArgumentNullException(nameof(action));
            Cancel();
            previousCancellationToken = new CancellationTokenSource();
            try
            {
                await Task.Delay(_delay, previousCancellationToken.Token);
                await Task.Run(action, previousCancellationToken.Token);
            }
            catch (TaskCanceledException) { }    // can swallow exception as nothing more to do if task cancelled
        }

        public void Cancel()
        {
            if (previousCancellationToken != null)
            {
                previousCancellationToken.Cancel();
                previousCancellationToken.Dispose();
                previousCancellationToken = null;
            }
        }

        public void Dispose() => Cancel();

    }
}
