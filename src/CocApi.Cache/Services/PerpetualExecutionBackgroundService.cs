using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache.Services
{
    public abstract class PerpetualExecutionBackgroundService<T> : BackgroundService, IPerpetualExecution<T> where T : class
    {
        public TimeSpan BeginExecutionAfter { get; }
        public TimeSpan DelayBeforeExecution { get; }
        public TimeSpan DelayBetweenExecutions { get; }
        public DateTime RanAt { get; private set; }
        public DateTime CompletedAt { get; private set; }
        public int ExecutionsAttempted { get; private set; }
        public int ExecutionsCompleted { get; private set; }
        private protected ILogger<T> Logger { get; }


        public bool IsEnabled { get; set; } = true;


        public PerpetualExecutionBackgroundService(ILogger<T> logger, TimeSpan? delayBeforeExecution, TimeSpan? delayBetweenExecutions)
        {
            Logger = logger;
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
                    Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", ((T)(object)this).GetType().Name, nameof(TryPollAsync));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(BeginExecutionAfter, cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (IsEnabled)
                        await TryPollAsync(cancellationToken);

                    await Task.Delay(DelayBetweenExecutions, cancellationToken);
                }
            }
            catch (Exception e)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", ((T)(object)this).GetType().Name, nameof(ExecuteAsync));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);

            Logger.LogInformation("{status} {service}", "Started", ((T)(object)this).GetType().Name);
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            Logger.LogInformation("{status} {service}", "Stopped", ((T)(object)this).GetType().Name);
        }
    }
}
