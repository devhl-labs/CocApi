using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CocApi.Cache.Services
{
    public abstract class PerpetualExecutionBackgroundService<T> : BackgroundService where T : class
    {
        public TimeSpan BeginExecutionAfter { get; }
        public TimeSpan DelayBeforeExecution { get; }
        public TimeSpan DelayBetweenExecutions { get; }
        public DateTime RanAt { get; private set; }
        public DateTime CompletedAt { get; private set; }
        public int ExecutionsAttempted { get; private set; }
        public int ExecutionsCompleted { get; private set; }

     
        private bool _isEnabled = true;


        public PerpetualExecutionBackgroundService(TimeSpan? delayBeforeExecution, TimeSpan? delayBetweenExecutions)
        {
            DelayBeforeExecution = delayBeforeExecution ?? TimeSpan.Zero;
            DelayBetweenExecutions = delayBetweenExecutions ?? TimeSpan.Zero;
        }


        private protected abstract Task PollAsync(CancellationToken cancellationToken);

        private async Task TryPollAsync(CancellationToken cancellationToken)
        {
            try
            {
                RanAt = DateTime.UtcNow;

                ExecutionsAttempted++;

                await PollAsync(cancellationToken);

                CompletedAt = DateTime.UtcNow;

                ExecutionsCompleted++;
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                     await Library.OnLog((T)(object)this, new LogEventArgs(LogLevel.Error, e)).ConfigureAwait(false);
            }
        }

        internal void IsEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(BeginExecutionAfter, cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_isEnabled)
                        await TryPollAsync(cancellationToken);

                    await Task.Delay(DelayBetweenExecutions, cancellationToken);
                }
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    await Library.OnLog((T)(object)this, new LogEventArgs(LogLevel.Error, e)).ConfigureAwait(false);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await Library.OnLog((T)(object)this, new LogEventArgs(LogLevel.Information, null, "Starting"));

            await base.StartAsync(cancellationToken);
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await Library.OnLog((T)(object)this, new LogEventArgs(LogLevel.Information, null, "Stoping"));

            await base.StopAsync(cancellationToken);
        }
    }
}
