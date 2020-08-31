using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public class CacheBase
    {
        internal void OnLog(object sender, LogEventArgs log) => Log?.Invoke(sender, log);

        public event LogEventHandler? Log;

        internal protected bool IsRunning { get; set; }

        internal protected bool StopRequested { get; set; }

        internal protected readonly TokenProvider _tokenProvider;
        internal protected readonly IServiceProvider _services;
        internal protected readonly CacheConfiguration _cacheConfiguration;

        public CacheBase(TokenProvider tokenProvider, CacheConfiguration cacheConfiguration)
        {
            _tokenProvider = tokenProvider;
            _cacheConfiguration = cacheConfiguration;
            _services = BuildServiceProvider(cacheConfiguration.ConnectionString);
        }

        protected virtual IServiceProvider BuildServiceProvider(string connectionString)
        {
            return new ServiceCollection()
                .AddDbContext<CachedContext>(o =>
                    o.UseSqlite(connectionString))
                .BuildServiceProvider();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            StopRequested = true;

            OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));

            while (IsRunning)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(500).ConfigureAwait(false);
            }

        }
    }

}
