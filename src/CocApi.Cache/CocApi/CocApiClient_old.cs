using Dapper.SqlWriter;
using CocApi.Cache.Exceptions;
using CocApi.Cache.Models;
using CocApi.Cache.Models.Cache;
using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using CocApi.Cache.Models.Wars;
using CocApi.Cache.Updaters;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CocApi.Cache
{
    public delegate Task ApiIsAvailableChangedEventHandler(object sender, bool isAvailable);
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;
    public delegate Task LogEventHandler(object sender, LogEventArgs log);
    public delegate Task SlowQueryEventHandler(object sender, SlowQueryEventArgs e);
    public delegate Task QueryFailureEventHandler(object sender, QueryFailureEventArgs e);

    public sealed partial class CocApiClient_old
    {
        private volatile bool _isAvailable = true;
        private readonly WebResponse _webResponse;

        public CocApiClient_old(CocApiConfiguration config)
        {
            CocApiConfiguration = config;

            if (config != null)
                CocApiConfiguration = config;           

            if (config == null || config.Tokens.Count == 0)
                throw new CocApiException("You did not provide any tokens to access the SC Api.");

            Villages = new Villages_old(this);

            Clans = new ClanCache(this);

            Wars = new Wars(this);

            Labels = new Labels(this);

            Leagues = new Leagues(this);

            Locations = new Locations(this);

            Test = new Test(this);

            _webResponse = new WebResponse(this, CocApiConfiguration, config.Tokens);

            ConfigureSqlWriter();

            Updater = new Updater(this, SqlWriter);
        }

        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;
        public event LogEventHandler? Log;
        public event SlowQueryEventHandler? SlowQuery;
        public event QueryFailureEventHandler? QueryFailure;

        public ClanCache Clans { get; set; }

        public bool IsAvailable
        {
            get
            {
                lock (IsAvailableLock)
                {
                    return _isAvailable;
                }
            }

            internal set
            {
                lock (IsAvailableLock)
                {
                    if (_isAvailable != value)
                    {
                        _isAvailable = value;

                        ApiIsAvailableChanged?.Invoke(this, _isAvailable);
                    }
                }
            }
        }

        public Labels Labels { get; }

        public Leagues Leagues { get; }

        public Locations Locations { get; }

        public Test Test { get; }

        public Villages_old Villages { get; }

        public Wars Wars { get; }

        internal CocApiConfiguration CocApiConfiguration { get; } = new CocApiConfiguration();

#nullable disable

        internal SqlWriter SqlWriter { get; set; }

#nullable enable

        internal Updater Updater { get; }

        private List<CancellationTokenSource> CancellationTokenSources { get; } = new List<CancellationTokenSource>();

        private object IsAvailableLock { get; } = new object();

        public async Task AddClanAsync(string clanTag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadVillages = false)
        {
            if (TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            CachedClan? cachedClan = await SqlWriter.Select<CachedClan>()
                                                   .Where(c => c.ClanTag == formattedTag)
                                                   .QueryFirstOrDefaultAsync()
                                                   .ConfigureAwait(false);

            if (cachedClan == null)
            {
                cachedClan = new CachedClan
                {
                    ClanTag = formattedTag,
                    DownloadClan = downloadClan,
                    DownloadCwl = downloadCwl,
                    DownloadVillages = downloadVillages,
                    DownloadCurrentWar = downloadWars
                };

                await SqlWriter.Insert(cachedClan).ExecuteAsync().ConfigureAwait(false);
            }
        }

        public async Task AddOrUpdateClanAsync(string clanTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadVillages)
        {
            if (TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            CachedClan? cachedClan = await SqlWriter.Select<CachedClan>()
                                                   .Where(c => c.ClanTag == formattedTag)
                                                   .QueryFirstOrDefaultAsync()
                                                   .ConfigureAwait(false);
            if (cachedClan == null)
            {
                await AddClanAsync(formattedTag, downloadClan, downloadVillages, downloadWars, downloadCwl).ConfigureAwait(false);
            }
            else
            {
                cachedClan.DownloadVillages = downloadVillages;

                cachedClan.DownloadCurrentWar = downloadWars;

                cachedClan.DownloadCwl = downloadCwl;

                cachedClan.DownloadClan = downloadClan;

                await SqlWriter.Update(cachedClan).ExecuteAsync().ConfigureAwait(false);
            }
        }

        public async Task AddVillage(string villageTag)
        {
            if (TryGetValidTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            CachedVillage_old cachedVillage = new CachedVillage_old
            {
                VillageTag = villageTag
            };

            if (await SqlWriter.Select<CachedVillage_old>()
                               .Where(v => v.VillageTag == formattedTag)
                               .QueryFirstOrDefaultAsync()
                               .ConfigureAwait(false) == null)
                await SqlWriter.Insert(cachedVillage)
                               .ExecuteAsync()
                               .ConfigureAwait(false);
        }

        /// <summary>
        /// Use this to get statistics on how long the Api takes to respond for diffent and points.
        /// </summary>
        /// <returns></returns>
        public ImmutableList<WebResponseTimer> GetTimers() => _webResponse.GetTimers().ToImmutableList();

        public string GetTokenStatus() => _webResponse.GetTokenStatus();

        public async Task RemoveVillage(string villageTag)
        {
            await SqlWriter.Delete<CachedVillage_old>()
                           .Where(v => v.VillageTag == villageTag)
                           .QueryAsync()
                           .ConfigureAwait(false);
        }

        public void Start()
        {
            OnLog(new LogEventArgs(nameof(CocApiClient_old), nameof(CocApiClient_old), LogLevel.Information, $"Tokens:          {CocApiConfiguration.Tokens.Count} tokens"));
            OnLog(new LogEventArgs(nameof(CocApiClient_old), nameof(CocApiClient_old), LogLevel.Information, $"TokenTimeOut:    {CocApiConfiguration.TokenTimeOut.TotalMilliseconds}ms"));
            OnLog(new LogEventArgs(nameof(CocApiClient_old), nameof(CocApiClient_old), LogLevel.Information, $"Request Timeout: {CocApiConfiguration.TimeToWaitForWebRequests.TotalMilliseconds}ms"));
            OnLog(new LogEventArgs(nameof(CocApiClient_old), nameof(CocApiClient_old), LogLevel.Information, $"DatabasePath:    {Path.Combine(CocApiConfiguration.DatabasePath ?? AppDomain.CurrentDomain.BaseDirectory, CocApiConfiguration.DatabaseName)}"));

            _ = Updater.StartAsync();
        }

        public async Task StopAsync() => await Updater.StopAsync().ConfigureAwait(false);

        internal async Task<T?> FetchAsync<T>(string url, CancellationToken? cancellationToken = null) where T : class, IDownloadable
        {
            try
            {
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

                TokenObject_old token = await _webResponse.GetTokenAsync().ConfigureAwait(false);

                AddCancellationTokenSource(cts);

                if (cancellationToken == null)
                    cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

                T? result = await _webResponse.GetDownloadableAsync<T>(url, token, cts.Token).ConfigureAwait(false) as T;

                RemoveCancellationTokenSource(cts);

                if (typeof(T) == typeof(LeagueWar) && result is NotInWar)
                {
                        await SqlWriter.Update<CachedWar>()
                                       .Where(w => w.WarTag == LeagueWar.WarTagFromUrl(url))
                                       .Set(w => w.IsFinal == true)
                                       .ExecuteAsync();
                }

                return result;
            }
            catch (Exception e)
            {
                if (e is CocApiException)
                    throw;

                throw new CocApiException(e.Message, e);
            }
        }

        internal void OnLog(LogEventArgs log) => Log?.Invoke(this, log);

        internal void UpdateDictionary<T>(ConcurrentDictionary<string, T> dictionary, string key, T downloadable) where T : class?, IDownloadable?
        {
            dictionary.AddOrUpdate(key, downloadable, (_, existingItem) =>
            {
                if (existingItem == null)
                    return downloadable;

                if (downloadable == null)
                    return existingItem;

                if (existingItem.ServerExpirationUtc > downloadable.ServerExpirationUtc)
                    return existingItem;

                return downloadable;
            });
        }

        private void AddCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (CancellationTokenSources)
            {
                CancellationTokenSources.Add(cts);
            }
        }

        private void ConfigureSqlWriter()
        {
            string path = $"Data Source={Path.Combine(CocApiConfiguration.DatabasePath ?? AppDomain.CurrentDomain.BaseDirectory, CocApiConfiguration.DatabaseName)}";

            SqlWriter = new SqlWriter(new SqlWriterConfiguration(path))
            {
                ToTableName = ToTableName,
            };

            SqlWriter.AddPropertyMap(new PropertyMap(typeof(DateTime), DateTimeToDatabase));

            SqlWriter.AddPropertyMap(new PropertyMap(typeof(DateTime?), DateTimeToDatabase));

            SqlWriter.Register<CachedClan>().Key(c => c.Id).AutoIncrement(c => c.Id);

            SqlWriter.Register<CachedWar>().Key(w => w.Id).AutoIncrement(w => w.Id);

            //SqlWriter.Register<CachedVillage>().Key(v => v.Id).AutoIncrement(v => v.Id);

            SqlWriter.Register<CachedItem_old>().Key(c => c.Path);

            SqlWriter.SlowQuery += OnSlowQuery;

            SqlWriter.QueryFailure += OnQueryFailure;
        }

        private object? DateTimeToDatabase(object arg)
        {
            if (arg == null)
                return null;

            DateTime dte = (DateTime)arg;

            return dte.ToString(DATE_FORMAT);
        }

        private Task OnSlowQuery(object sender, SlowQueryEventArgs e)
        {
            SlowQuery?.Invoke(sender, e);

            return Task.CompletedTask;
        }

        private Task OnQueryFailure(object sender, QueryFailureEventArgs e)
        {
            QueryFailure?.Invoke(sender, e);

            return Task.CompletedTask;
        }

        private void RemoveCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (CancellationTokenSources)
            {
                CancellationTokenSources.Remove(cts);
            }
        }

        private string ToTableName(string arg)
                    => (arg.Length > "Cached".Length) ?
                    arg["Cached".Length..] :
                    arg;

        internal async Task<T?> GetAsync<T>(string path) where T : class
        {
            CachedItem_old? cache = await SqlWriter.Select<CachedItem_old>()
                                          .Where(c => c.Path == path)
                                          .QueryFirstOrDefaultAsync()
                                          .ConfigureAwait(false);

            return cache?.Json.Deserialize<T>();
        }

        internal async Task<T?> GetOrFetchAsync<T>(string path, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null) where T : class, IDownloadable 
        {
            if (cacheOption == CacheOption.ServerOnly)
                return await FetchAsync<T>(path, cancellationToken).ConfigureAwait(false);

            T? cached = await GetAsync<T>(path).ConfigureAwait(false);

            if (cacheOption == CacheOption.CacheOnly)
                return cached;

            if (cached == null)
                return await FetchAsync<T>(path, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return cached;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && cached.IsLocallyExpired(GetTTL(cached)) == false)
                return cached;

            T? fetched = await FetchAsync<T>(path, cancellationToken).ConfigureAwait(false);

            return fetched ?? cached;
        }

        internal TimeSpan GetTTL<T>(T cached)
        {
            if (typeof(T) == typeof(Clan))
                return CocApiConfiguration.ClansTimeToLive;

            if (typeof(T) == typeof(Village))
                return CocApiConfiguration.VillageTimeToLive;

            if (typeof(T) == typeof(Paginated<Label>))
                return CocApiConfiguration.Labels;
            if (typeof(T) == typeof(Paginated<League>))
                return CocApiConfiguration.Leagues;
            if (typeof(T) == typeof(Paginated<Models.Clans.Location>))
                return CocApiConfiguration.Locations;
            if (typeof(T) == typeof(Paginated<Models.Clans.TopMainClan>))
                return CocApiConfiguration.TopMainClans;
            if (typeof(T) == typeof(Paginated<Models.Clans.TopBuilderClan>))
                return CocApiConfiguration.TopBuilderClans;
            if (typeof(T) == typeof(Paginated<Clan>))
                return CocApiConfiguration.ClansTimeToLive;
            if (typeof(T) == typeof(Paginated<TopMainVillage>))
                return CocApiConfiguration.TopMainVillages;
            if (typeof(T) == typeof(Paginated<TopBuilderVillage>))
                return CocApiConfiguration.TopBuilderVillages;
            //if (typeof(T) == typeof(Paginated<WarLogEntry>))
            //    throw new NotImplementedException("use WarLog class instead");
            //    //return CocApiConfiguration.WarLogTimeToLive;
            if (typeof(T) == typeof(Paginated<WarLeague>))
                return CocApiConfiguration.WarLeagues;

            if (cached is IWar war)
            {
                if (war is NotInWar)
                    return CocApiConfiguration.NotInWarTimeToLive;

                if (war is PrivateWarLog)
                    return CocApiConfiguration.PrivateWarLogTimeToLive;

                if (war is CurrentWar currentWar && currentWar.State == WarState.Preparation)
                    return currentWar.StartTimeUtc - DateTime.UtcNow;

                if (war is LeagueWar)
                    return CocApiConfiguration.LeagueWarTimeToLive;

                if (war is CurrentWar)
                    return CocApiConfiguration.CurrentWarTimeToLive;

                if (war is WarLog)
                    return CocApiConfiguration.WarLogTimeToLive;
            }

            if (cached is LeagueGroup leagueGroup)
            {
                if (leagueGroup.State == LeagueState.WarsEnded)
                    return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)).AddHours(8) - DateTime.UtcNow;

                return CocApiConfiguration.LeagueGroupTimeToLive;
            }

            if (cached is LeagueGroupNotFound)
            {
                if (DateTime.UtcNow.Day > 9)
                    return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)).AddHours(8) - DateTime.UtcNow;

                return CocApiConfiguration.LeagueGroupNotFoundTimeToLive;
            }


            OnLog(new LogEventArgs(nameof(CocApiClient_old), nameof(GetTTL), LogLevel.Debug, "Class not handled."));

            return TimeSpan.FromSeconds(0);
        }
    }
}