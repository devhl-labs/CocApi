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
        private protected bool _isRunning;

        internal protected TokenProvider TokenProvider { get; }
        internal protected IServiceProvider Services { get; }
        internal protected ClientConfiguration ClientConfiguration { get; }

        protected int _id;

        public ClientBase(TokenProvider tokenProvider, ClientConfiguration clientConfiguration)
        {
            TokenProvider = tokenProvider;
            ClientConfiguration = clientConfiguration;
            Services = ClientConfiguration.BuildServiceProvider();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            while (_isRunning)
            {
                await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            }
        }

        protected CancellationTokenSource _stopRequestedTokenSource = new CancellationTokenSource();
    }

}
