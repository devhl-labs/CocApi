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

    public class ClientBase
    {
        internal void OnLog(object sender, LogEventArgs log) => Log?.Invoke(sender, log);

        public event LogEventHandler? Log;

        internal protected bool IsRunning { get; set; }

        internal protected readonly TokenProvider _tokenProvider;
        internal protected readonly IServiceProvider _services;
        internal protected readonly ClientConfigurationBase _cacheConfiguration;

        public ClientBase(TokenProvider tokenProvider, ClientConfigurationBase cacheConfiguration)
        {
            _tokenProvider = tokenProvider;
            _cacheConfiguration = cacheConfiguration;
            _services = _cacheConfiguration.BuildServiceProvider();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));

            while (IsRunning)
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        }

        protected CancellationTokenSource _stopRequestedTokenSource = new CancellationTokenSource();
    }

}
