using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal abstract class MonitorBase
    {
        private bool _isRunning;
        private protected int _id = int.MinValue;
        private protected CancellationToken _cancellationToken;
        private protected IDesignTimeDbContextFactory<CacheDbContext> _dbContextFactory;
        private protected string[] _dbContextArgs;

        public MonitorBase(CacheDbContextFactoryProvider provider)
        {
            _dbContextFactory = provider.Factory;
            _dbContextArgs = provider.DbContextArgs;
        }

        private protected DateTime expires = DateTime.UtcNow.AddSeconds(-3);

        private protected DateTime min = DateTime.MinValue;

        private protected DateTime now = DateTime.UtcNow;

        protected abstract Task PollAsync();

        private readonly object _startLock = new();

        public void Start(CancellationToken cancellationToken)
        {
            lock (_startLock)
            {
                if (_isRunning)
                    throw new InvalidOperationException("Monitor already running.");

                _isRunning = true;

                cancellationToken.ThrowIfCancellationRequested();

                _cancellationToken = cancellationToken;

                RunTask = RunAsync();
            }
        }

        public Task RunTask;

        private async Task RunAsync()
        {
            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "running"));

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    expires = DateTime.UtcNow.AddSeconds(-3);

                    now = DateTime.UtcNow;

                    await PollAsync().ConfigureAwait(false);
                }
                catch(Exception e)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        break;
                    else
                        Library.OnLog(this, new LogEventArgs(LogLevel.Error, "errored", e));
                }
            }

            _isRunning = false;
                                
            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "stopped"));
        }
    }
}
