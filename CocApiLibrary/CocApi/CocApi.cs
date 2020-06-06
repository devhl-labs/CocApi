using Dapper.SqlWriter;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.War;
using devhl.CocApi.Updaters;

//using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public delegate Task ApiIsAvailableChangedEventHandler(object sender, bool isAvailable);

    public delegate Task AsyncEventHandler(object sender, EventArgs e);

    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public delegate Task SlowQueryEventHandler(object sender, SlowQueryEventArgs e);

    public sealed partial class CocApi : IDisposable
    {
        private volatile bool _isAvailable = true;

        public CocApi(CocApiConfiguration cfg)
        {
            if (cfg != null)
            {
                CocApiConfiguration = cfg;
            }

            if (cfg == null || cfg.Tokens.Count == 0)
            {
                throw new CocApiException("You did not provide any tokens to access the SC Api.");
            }

            Villages = new Villages(this);

            Clans = new Clans(this);

            Wars = new Wars(this);

            Labels = new Labels(this);

            Leagues = new Leagues(this);

            Locations = new Locations(this);

            Test = new Test(this);

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            ConfigureSqlWriter();

            Updater = new Updater(this, SqlWriter);
        }

        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;

        public event LogEventHandler? Log;

        public event SlowQueryEventHandler? SlowQuery;

        public Clans Clans { get; set; }

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
        public Villages Villages { get; }
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

        public async Task AddClanOrUpdateClanAsync(string clanTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadVillages)
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

            CachedVillage cachedVillage = new CachedVillage
            {
                VillageTag = villageTag
            };

            if (await SqlWriter.Select<CachedVillage>()
                               .Where(v => v.VillageTag == formattedTag)
                               .QueryFirstOrDefaultAsync()
                               .ConfigureAwait(false) == null)
                await SqlWriter.Insert(cachedVillage)
                               .ExecuteAsync()
                               .ConfigureAwait(false);
        }

        /// <summary>
        /// Disposes all disposable items.  Pending tasks will be canceled.
        /// </summary>
        public void Dispose()
        {
            foreach (CancellationTokenSource cancellationTokenSource in CancellationTokenSources)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception)
                {
                }

                cancellationTokenSource.Dispose();
            }

            WebResponse.HttpClient.Dispose();

            WebResponse.SemaphoreSlim.Dispose();
        }

        /// <summary>
        /// Use this to get statistics on how long the Api takes to respond for diffent and points.
        /// </summary>
        /// <returns></returns>
        public ImmutableList<WebResponseTimer> GetTimers() => WebResponse.GetTimers().ToImmutableList();

        public string GetTokenStatus() => WebResponse.GetTokenStatus();

        public async Task RemoveVillage(string villageTag)
        {
            await SqlWriter.Delete<CachedVillage>()
                           .Where(v => v.VillageTag == villageTag)
                           .QueryAsync()
                           .ConfigureAwait(false);
        }

        public void Start() => _ = Updater.StartAsync();

        public async Task StopAsync() => await Updater.StopAsync().ConfigureAwait(false);

        //internal async Task<IDownloadable?> FetchAsync<TResult>(string url, CancellationToken? cancellationToken = null) where TResult : class, IDownloadable
        //{
        //    try
        //    {
        //        using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

        //        TokenObject token = await WebResponse.GetTokenAsync().ConfigureAwait(false);

        //        AddCancellationTokenSource(cts);

        //        if (cancellationToken == null)
        //            cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

        //        IDownloadable? result = await WebResponse.GetDownloadableAsync<TResult>(url, token, cts.Token).ConfigureAwait(false);

        //        RemoveCancellationTokenSource(cts);

        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        if (e is CocApiException)
        //            throw;

        //        throw new CocApiException(e.Message, e);
        //    }
        //}

        internal async Task<T?> FetchAsync<T>(string url, CancellationToken? cancellationToken = null) where T : class, IDownloadable
        {
            try
            {
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

                TokenObject token = await WebResponse.GetTokenAsync().ConfigureAwait(false);

                AddCancellationTokenSource(cts);

                if (cancellationToken == null)
                    cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

                T? result = await WebResponse.GetDownloadableAsync<T>(url, token, cts.Token).ConfigureAwait(false) as T;

                RemoveCancellationTokenSource(cts);

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
            SqlWriterConfiguration config = new SqlWriterConfiguration { SlowQueryWarning = TimeSpan.FromSeconds(5) };

            string path = $"Data Source={Path.Combine(CocApiConfiguration.DatabasePath ?? AppDomain.CurrentDomain.BaseDirectory, CocApiConfiguration.DatabaseName)}";

            SQLiteConnection connection = new SQLiteConnection(path);

            SqlWriter = new SqlWriter(connection, config)
            {
                ToTableName = ToTableName,
            };

            SqlWriter.AddPropertyMap(new PropertyMap(typeof(DateTime), DateTimeToDatabase));

            SqlWriter.AddPropertyMap(new PropertyMap(typeof(DateTime?), DateTimeToDatabase));

            SqlWriter.Register<CachedClan>().Key(c => c.Id).AutoIncrement(c => c.Id);

            SqlWriter.Register<CachedWar>().Key(w => w.Id).AutoIncrement(w => w.Id);

            SqlWriter.Register<CachedVillage>().Key(v => v.Id).AutoIncrement(v => v.Id);

            SqlWriter.Register<Cache>().Key(c => c.Path);

            SqlWriter.SlowQuery += OnSlowQuery;
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
            Cache? cache = await SqlWriter.Select<Cache>()
                                          .Where(c => c.Path == path)
                                          .QueryFirstOrDefaultAsync()
                                          .ConfigureAwait(false);

            return cache?.Json.Deserialize<T>();
        }

        internal async Task<T?> GetOrFetchAsync<T>(string path, CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null) where T : class, IDownloadable 
        {
            T? cached = await GetAsync<T>(path).ConfigureAwait(false);

            if (cached == null)
                return await FetchAsync<T>(path, cancellationToken).ConfigureAwait(false);

            if (cacheOption == CacheOption.AllowAny)
                return cached;

            if (cacheOption == CacheOption.AllowExpiredServerResponse && cached.IsLocallyExpired(GetTTL(cached)) == false) //todo fix this time to live
                return cached;

            T? fetched = await FetchAsync<T>(path, cancellationToken).ConfigureAwait(false);

            return fetched ?? cached;
        }

        internal TimeSpan GetTTL<T>(T cached)
        {
            if (typeof(T) == typeof(Clan))
                return CocApiConfiguration.ClanTimeToLive;

            return TimeSpan.FromSeconds(0);
        }
    }
}