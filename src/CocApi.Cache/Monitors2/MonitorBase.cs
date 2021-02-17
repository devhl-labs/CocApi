using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    public class MonitorBase
    {
        private protected bool _isRunning;

        internal protected int _id = int.MinValue;

        internal protected IDesignTimeDbContextFactory<CocApiCacheContext> DbContextFactory { get; }
        internal protected string[] DbContextArgs { get; }

        public MonitorBase(IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs)
        {
            DbContextFactory = dbContextFactory;
            DbContextArgs = dbContextArgs;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            while (_isRunning)            
                await Task.Delay(50, cancellationToken).ConfigureAwait(false);            
        }


        protected CancellationTokenSource _stopRequestedTokenSource = new CancellationTokenSource();
    }

}
