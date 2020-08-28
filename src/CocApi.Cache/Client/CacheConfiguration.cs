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
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public class CacheConfiguration
    {
        //internal IServiceProvider services;

        //private readonly List<Token> _tokenObjects = new List<Token>();

        //private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        public string ConnectionString { get; }

        public int ConcurrentUpdates { get; }

        public TimeSpan DelayBetweenUpdates { get; }

        public CacheConfiguration(string connectionString = "Data Source=cocapi.db", int concurrentUpdates = 1, TimeSpan? delayBetweenUpdates = null)
        {
            ConnectionString = connectionString;

            ConcurrentUpdates = concurrentUpdates;

            DelayBetweenUpdates = delayBetweenUpdates ?? TimeSpan.FromMilliseconds(50);

            new CachedContext(new DbContextOptionsBuilder().UseSqlite(ConnectionString).Options).Database.Migrate();


            //services = BuildServiceProvider(cocApiConfiguration);

            //services.GetRequiredService<CachedContext>().Database.Migrate();









                    //        .AddDbContext<CachedContext>(o =>
                    //o.UseSqlite(cocApiConfiguration.ConnectionString))
        }

        public event LogEventHandler? Log;

        //public ClansApi ClansApi
        //{
        //    get
        //    {
        //        return services.GetRequiredService<ClansApi>();
        //    }
        //}

        //public ClansCache ClansCache
        //{
        //    get
        //    {
        //        return services.GetRequiredService<ClansCache>();
        //    }
        //}

        //public LabelsApi LabelsApi
        //{
        //    get
        //    {
        //        return services.GetRequiredService<LabelsApi>();
        //    }
        //}

        //public LeaguesApi LeaguesApi
        //{
        //    get
        //    {
        //        return services.GetRequiredService<LeaguesApi>();
        //    }
        //}

        //public LocationsApi LocationsApi
        //{
        //    get
        //    {
        //        return services.GetRequiredService<LocationsApi>();
        //    }
        //}

        //public PlayersApi PlayersApi
        //{
        //    get
        //    {
        //        return services.GetRequiredService<PlayersApi>();
        //    }
        //}

        //public PlayersCache PlayersCache
        //{
        //    get
        //    {
        //        return services.GetRequiredService<PlayersCache>();
        //    }
        //}

        //internal async Task<string> GetTokenAsync()
        //{
        //    await _tokenSemaphore.WaitAsync().ConfigureAwait(false);

        //    try
        //    {
        //        return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync().ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        _tokenSemaphore.Release();
        //    }
        //}

        internal void OnLog(object sender, LogEventArgs log) => Log?.Invoke(sender, log);

        //private IServiceProvider BuildServiceProvider(CocApiConfiguration cocApiConfiguration)
        //{
        //    return new ServiceCollection()
        //        .AddDbContext<CachedContext>(o =>
        //            o.UseSqlite(cocApiConfiguration.ConnectionString))
        //        .AddSingleton(cocApiConfiguration)
        //        .AddSingleton(this)
        //        .AddSingleton<ClansApi>()
        //        .AddSingleton<ClansCache>()
        //        .AddSingleton<PlayersApi>()
        //        .AddSingleton<PlayersCache>()
        //        .AddSingleton<LeaguesApi>()
        //        .AddSingleton<LocationsApi>()
        //        .AddSingleton<LabelsApi>()
        //        .AddSingleton(ConfigurationBuilder)
        //        .BuildServiceProvider();
        //}

        //private Client.Configuration ConfigurationBuilder(IServiceProvider services)
        //{
        //    Client.Configuration configuration = new Client.Configuration();

        //    CocApiConfiguration cocApiConfiguration = services.GetRequiredService<CocApiConfiguration>();

        //    foreach (string token in cocApiConfiguration.Tokens)
        //        _tokenObjects.Add(new Token(this, token, cocApiConfiguration.TokenTimeOut));

        //    configuration.UserAgent = nameof(CacheConfiguration);

        //    configuration.Timeout = cocApiConfiguration.TimeToWaitForWebRequests.Milliseconds;

        //    //configuration.GetTokenAsync = GetTokenAsync;

        //    return configuration;
        //}
    }

    //public class TokenProvider
    //{
    //    private readonly List<Token> _tokenObjects = new List<Token>();

    //    private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

    //    public TokenProvider(List<string> tokens, TimeSpan httpRequestTimeOut)
    //    {
    //        foreach (string token in tokens)
    //            _tokenObjects.Add(new Token(this, token, cocApiConfiguration.TokenTimeOut));
    //    }

    //    internal async Task<string> GetTokenAsync()
    //    {
    //        await _tokenSemaphore.WaitAsync().ConfigureAwait(false);

    //        try
    //        {
    //            return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync().ConfigureAwait(false);
    //        }
    //        finally
    //        {
    //            _tokenSemaphore.Release();
    //        }
    //    }
    //}
}