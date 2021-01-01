using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public class ClientBase
    {
        private protected bool _isRunning;

        internal protected TokenProvider TokenProvider { get; }
        internal protected IServiceProvider Services { get; }
        internal protected ClientConfiguration Configuration { get; }

        public void Migrate() => MigrationHandler.Migrate(Configuration.ConnectionString);

        public ClientBase(TokenProvider tokenProvider, ClientConfiguration configuration)
        {
            TokenProvider = tokenProvider;
            Configuration = configuration;
            Services = Configuration.BuildServiceProvider();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            while (_isRunning)            
                await Task.Delay(50, cancellationToken).ConfigureAwait(false);            
        }

        internal Task StartAsync()
        {
            CacheContext cachedContext = Services.GetRequiredService<CacheContext>();

            if (cachedContext.Database.GetPendingMigrations().Count() > 0)
                throw new Exception("Please run the migration before starting the client.");

            return Task.CompletedTask;
        }

        protected CancellationTokenSource _stopRequestedTokenSource = new CancellationTokenSource();
    }

}
