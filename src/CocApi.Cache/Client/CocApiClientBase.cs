using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public delegate Task AsyncEventHandler(object sender, EventArgs e);

    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public class CocApiClientBase
    {
        internal IServiceProvider services;

        private readonly List<Token> _tokenObjects = new List<Token>();

        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        public CocApiClientBase(CocApiConfiguration cocApiConfiguration)
        {
            services = BuildServiceProvider(cocApiConfiguration);

            services.GetRequiredService<CachedContext>().Database.Migrate();
        }

        public event LogEventHandler? Log;

        public ClansApi ClansApi
        {
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

        public LabelsApi LabelsApi
        {
            get
            {
                return services.GetRequiredService<LabelsApi>();
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

        public PlayersApi PlayersApi
        {
            get
            {
                return services.GetRequiredService<PlayersApi>();
            }
        }

        public PlayersCache PlayersCache
        {
            get
            {
                return services.GetRequiredService<PlayersCache>();
            }
        }

        public virtual bool HasUpdated(Player stored, Player fetched) => stored.Equals(fetched);

        public virtual bool HasUpdated(Clan stored, Clan fetched) => stored.Equals(fetched);

        public virtual TimeSpan TimeToLive(CachedPlayer cachedPlayer, ApiResponse<Player> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan TimeToLive(CachedPlayer cachedPlayer, ApiException apiException)
            => TimeSpan.FromMinutes(0);

        public virtual TimeSpan TimeToLive(CachedClan cachedClan, ApiResponse<Clan> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan TimeToLive(CachedClan cachedClan, ApiException apiException)
            => TimeSpan.FromMinutes(0);

        public virtual TimeSpan TimeToLive(CachedClanWarLog cachedClanWarLog, ApiResponse<ClanWarLog> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan TimeToLive(CachedClanWarLog cachedClanWarLog, ApiException apiException)
            => TimeSpan.FromMinutes(2);

        public virtual TimeSpan TimeToLive(CachedClanWarLeagueGroup cachedGroup, ApiResponse<ClanWarLeagueGroup> apiResponse)
        {
            if (apiResponse.Data.State == ClanWarLeagueGroup.StateEnum.Ended)
                return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)) - DateTime.UtcNow;

            return TimeSpan.FromMinutes(20);
        }

        public virtual TimeSpan TimeToLive(CachedClanWarLeagueGroup cachedGroup, ApiException apiException)
            => TimeSpan.FromMinutes(20);

        public virtual TimeSpan TimeToLive(CachedClanWar cachedClanWar, ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse.Data.State == ClanWar.StateEnum.Preparation)
                return apiResponse.Data.StartTime - DateTime.UtcNow;

            return TimeSpan.FromSeconds(0);
        }

        public virtual TimeSpan TimeToLive(CachedClanWar cachedClanWar, ApiException apiException)
        {
            if (apiException.ErrorCode == (int)HttpStatusCode.Forbidden)
                return TimeSpan.FromMinutes(2);

            return TimeSpan.FromSeconds(0);
        }

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

        internal void OnLog(LogEventArgs log) => Log?.Invoke(this, log);

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

        private Client.Configuration ConfigurationBuilder(IServiceProvider services)
        {
            Client.Configuration configuration = new Client.Configuration();

            CocApiConfiguration cocApiConfiguration = services.GetRequiredService<CocApiConfiguration>();

            foreach (string token in cocApiConfiguration.Tokens)
                _tokenObjects.Add(new Token(this, token, cocApiConfiguration.TokenTimeOut));

            configuration.UserAgent = nameof(CocApiClientBase);

            configuration.Timeout = cocApiConfiguration.TimeToWaitForWebRequests.Milliseconds;

            configuration.GetTokenAsync = GetTokenAsync;

            return configuration;
        }
    }
}