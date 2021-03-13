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

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            if (_isRunning)
                throw new InvalidOperationException("Monitor already running.");

            _isRunning = true;

            cancellationToken.ThrowIfCancellationRequested();

            _cancellationToken = cancellationToken;

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "running"));

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    expires = DateTime.UtcNow.AddSeconds(-3);

                    now = DateTime.UtcNow;

                    await PollAsync();
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
