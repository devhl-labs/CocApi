using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi;
using CocApi.Api;
using CocApi.Cache.Exceptions;
using CocApi.Cache.Models;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace CocApi.Cache
{
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;
    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public class CocApiClient
    {
        internal IServiceProvider services;
        public event LogEventHandler? Log;

        internal void OnLog(LogEventArgs log) => Log?.Invoke(this, log);

        public ClansApi ClansApi {
            get
            {
                return services.GetRequiredService<ClansApi>();
            }
        }

        public ClansCache ClansCache
        {
            get
            {
                return services.GetRequiredService<ClansCache>();
            }
        }
        public PlayersCache PlayersCache
        {
            get
            {
                return services.GetRequiredService<PlayersCache>();
            }
        }
        public PlayersApi PlayersApi
        {
            get
            {
                return services.GetRequiredService<PlayersApi>();
            }
        }
        public LeaguesApi LeaguesApi
        {
            get
            {
                return services.GetRequiredService<LeaguesApi>();
            }
        }
        public LocationsApi LocationsApi
        {
            get
            {
                return services.GetRequiredService<LocationsApi>();
            }
        }
        public LabelsApi LabelsApi
        {
            get
            {
                return services.GetRequiredService<LabelsApi>();
            }
        }

        private readonly List<Token> _tokenObjects = new List<Token>();

        public CocApiClient(CocApiConfiguration cocApiConfiguration)
        {
            services = BuildServiceProvider(cocApiConfiguration);

            services.GetRequiredService<CachedContext>().Database.Migrate();
        }

        private Client.Configuration ConfigurationBuilder(IServiceProvider services)
        {
            Client.Configuration configuration = new Client.Configuration();

            CocApiConfiguration cocApiConfiguration = services.GetRequiredService<CocApiConfiguration>();

            foreach (string token in cocApiConfiguration.Tokens)
                _tokenObjects.Add(new Token(this, token, cocApiConfiguration.TokenTimeOut));

            configuration.UserAgent = nameof(CocApiClient);

            configuration.Timeout = cocApiConfiguration.TimeToWaitForWebRequests.Milliseconds;

            configuration.GetTokenAsync = GetTokenAsync;

            return configuration;
        }

        private IServiceProvider BuildServiceProvider(CocApiConfiguration cocApiConfiguration)
        {
            return new ServiceCollection()
                .AddDbContext<CachedContext>(o =>
                    o.UseSqlite(cocApiConfiguration.ConnectionString))
                .AddSingleton(cocApiConfiguration)
                .AddSingleton(this)
                .AddSingleton<ClansApi>()
                .AddSingleton<ClansCache>()
                .AddSingleton<PlayersApi>()
                .AddSingleton<PlayersCache>()
                .AddSingleton<LeaguesApi>()
                .AddSingleton<LocationsApi>()
                .AddSingleton<LabelsApi>()
                .AddSingleton(ConfigurationBuilder)
                .BuildServiceProvider();
        }

        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        internal async Task<string> GetTokenAsync()
        {
            await _tokenSemaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync().ConfigureAwait(false);
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }

        public virtual bool IsEqual(Player stored, Player fetched) => stored.Equals(fetched);

        public virtual bool IsEqual(Clan stored, Clan fetched) => stored.Equals(fetched);
    }
}
