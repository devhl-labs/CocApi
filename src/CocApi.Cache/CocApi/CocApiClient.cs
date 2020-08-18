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
using CocApi.Cache.CocApi;
using CocApi.Cache.Exceptions;
using CocApi.Cache.Models.Cache;
//using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
//using CocApi.Cache.Updaters;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
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

        public ClansApi ClansApi;
        public ClansCache ClansCache;
        public PlayersCache PlayersCache;
        public PlayersApi PlayersApi;
        public LeaguesApi LeaguesApi;
        public LocationsApi LocationsApi;
        public LabelsApi LabelsApi;

        private readonly List<TokenObject> _tokenObjects = new List<TokenObject>();

        public CocApiClient(CocApiConfiguration cocApiConfiguration)
        {
            _cocApiConfiguration = cocApiConfiguration;

            services = new ServiceCollection().AddDbContext<CachedContext>(o => o.UseSqlite(_cocApiConfiguration.ConnectionString)).BuildServiceProvider();

            CachedContext cacheContext = services.GetRequiredService<CachedContext>();

            cacheContext.Database.Migrate();

            Client.Configuration configuration = new Client.Configuration();

            foreach (string token in _cocApiConfiguration.Tokens)
                _tokenObjects.Add(new TokenObject(this, token, _cocApiConfiguration.TokenTimeOut));
            
            configuration.UserAgent = nameof(CocApiClient);

            configuration.DateTimeFormat = "yyyyMMdd'T'HHmmss.fff'Z'";

            configuration.Timeout = _cocApiConfiguration.TimeToWaitForWebRequests.Milliseconds;

            configuration.GetTokenAsync = GetTokenAsync;

            ClansApi = new ClansApi(configuration);

            ClansCache = new ClansCache(this, cocApiConfiguration, services, ClansApi);

            PlayersApi = new PlayersApi(configuration);

            PlayersCache = new PlayersCache(this, cocApiConfiguration, services, PlayersApi);

            LeaguesApi = new LeaguesApi(configuration);

            LocationsApi = new LocationsApi(configuration);

            LabelsApi = new LabelsApi(configuration);
        }

        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);
        private readonly CocApiConfiguration _cocApiConfiguration;

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

        //public async Task AddClanAsync(string clanTag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadVillages = false)
        //{
        //    if (Clash.TryGetValidTag(clanTag, out string formattedTag) == false)
        //        throw new InvalidTagException(clanTag);

        //    using var scope = services.CreateScope();

        //    CacheContext cacheContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

        //    CachedClan cachedClan = await cacheContext.Clans.Where(c => c.ClanTag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

        //    if (cachedClan == null)
        //    {
        //        cachedClan = new CachedClan
        //        {
        //            ClanTag = formattedTag,
        //            DownloadClan = downloadClan,
        //            DownloadCwl = downloadCwl,
        //            DownloadVillages = downloadVillages,
        //            DownloadCurrentWar = downloadWars
        //        };

        //        cacheContext.Clans.Update(cachedClan);

        //        await cacheContext.SaveChangesAsync().ConfigureAwait(false);
        //    }
        //}

        //public async Task AddOrUpdateClanAsync(string clanTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadVillages)
        //{
        //    if (Clash.TryGetValidTag(clanTag, out string formattedTag) == false)
        //        throw new InvalidTagException(clanTag);

        //    using var scope = services.CreateScope();

        //    CacheContext cacheContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

        //    CachedClan cachedClan = await cacheContext.Clans.Where(c => c.ClanTag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

        //    cachedClan ??= new CachedClan();

        //    cachedClan.ClanTag = formattedTag;

        //    cachedClan.DownloadVillages = downloadVillages;

        //    cachedClan.DownloadCurrentWar = downloadWars;

        //    cachedClan.DownloadCwl = downloadCwl;

        //    cachedClan.DownloadClan = downloadClan;

        //    cacheContext.Clans.Update(cachedClan);

        //    await cacheContext.SaveChangesAsync().ConfigureAwait(false);
        //}

        public virtual bool IsEqual(Player stored, Player fetched) => stored.Equals(fetched);

        public virtual bool IsEqual(Clan stored, Clan fetched) => stored.Equals(fetched);

        //internal async Task<T?> GetAsync<T>(string path) where T : class
        //{
        //    T? result = await GetWithHttpInfoAsync<T>(path);

        //    if (result == null)
        //        return null;

        //    return JsonConvert.DeserializeObject<T>(result.r);
        //}

        //public async Task<CachedItem_newnew<TApi>?> GetWithHttpInfoAsync<TApi>(string tag) /*where TCache : CachedItem_newnew<TApi>*/ where TApi : class
        //{
        //    using var scope = services.CreateScope();

        //    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

        //    //return await dbContext.Items.Where(i => i.Path == path).FirstOrDefaultAsync().ConfigureAwait(false);

        //    if (typeof(TApi) == typeof(Player))
        //        return await dbContext.Players.Where(i => i.Tag == tag).FirstOrDefaultAsync().ConfigureAwait(false);

        //    throw new Exception("Unhandled");
        //}
    }
}
