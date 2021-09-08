using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CocApi.Cache.Services
{
    public abstract class PerpetualExecutionBackgroundService<T> : BackgroundService where T : class
    {
        public TimeSpan BeginExecutionAfter { get; }
        public TimeSpan DelayBetweenExecutions { get; }
        public DateTime RanAt { get; private set; }
        public DateTime CompletedAt { get; private set; }
        public int ExecutionsAttempted { get; private set; }

        public int ExecutionsCompleted { get; private set; }

        public PerpetualExecutionBackgroundService(TimeSpan? beginExecutionAfter = null, TimeSpan? delayBetweenExecutions = null)
        {
            BeginExecutionAfter = beginExecutionAfter ?? TimeSpan.Zero;
            DelayBetweenExecutions = delayBetweenExecutions ?? TimeSpan.Zero;
        }

        protected abstract Task DoWorkAsync(CancellationToken cancellationToken);

        private async Task TryDoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                RanAt = DateTime.UtcNow;

                ExecutionsAttempted++;

                await DoWorkAsync(cancellationToken);

                CompletedAt = DateTime.UtcNow;

                ExecutionsCompleted++;
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Library.OnLog((T)(object)this, new LogEventArgs(LogLevel.Error, e));
            }
        }

        private bool _isEnabled = true;

        public void IsEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(BeginExecutionAfter, cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_isEnabled)
                        await TryDoWorkAsync(cancellationToken);

                    await Task.Delay(DelayBetweenExecutions, cancellationToken);
                }
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Library.OnLog((T)(object)this, new LogEventArgs(LogLevel.Error, e));
            }
        }
    }
}
