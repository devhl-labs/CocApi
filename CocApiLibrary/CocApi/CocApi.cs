using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
//using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper.SqlWriter;
using devhl.CocApi.Models.Cache;
using System.IO;
using devhl.CocApi.Models.Clan;
using Newtonsoft.Json;
using System.CodeDom;
using System.Globalization;
using devhl.CocApi.Models.War;
using System.Linq;
using devhl.CocApi.Updaters;

namespace devhl.CocApi
{
    public delegate Task ApiIsAvailableChangedEventHandler(object sender, bool isAvailable);
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;
    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public sealed partial class CocApi : IDisposable
    {
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();
        private readonly object _isAvailableLock = new object();
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

            Clans.CreateClanUpdateServices();

            //DownloadWars = true;

            RegisterClasses();
        }

        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;

        public event LogEventHandler? Log;

#nullable disable

        private SqlWriter SqlWriter { get; set; }

#nullable enable

        private void RegisterClasses()
        {
            SqlWriterConfiguration config = new SqlWriterConfiguration { SlowQueryWarning = TimeSpan.FromSeconds(5) };
            
            SQLiteConnection connection = new SQLiteConnection($"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"CocApiDatabase.sqlite")}");

            SqlWriter = new SqlWriter(connection, config)
            {
                ToTableName = ToTableName,
            };

            SqlWriter.AddPropertyMap(new PropertyMap(typeof(DateTime), DateTimeToDatabase));

            SqlWriter.AddPropertyMap(new PropertyMap(typeof(DateTime?), DateTimeToDatabase));

            SqlWriter.Register<CachedClan>().Key(c => c.ClanTag);

            SqlWriter.Register<CachedWar>().Key(w => w.PrepStartTime).Key(w => w.ClanTag).Key(w => w.OpponentTag);

            SqlWriter.Register<CachedVillage>().Key(v => v.VillageTag);

            SqlWriter.Register<CachedClanWar>().Key(cw => cw.ClanTag).Key(cw => cw.PrepStartTime);

            NewUpdater = new NewUpdater(this, SqlWriter);

            _ = NewUpdater.UpdateAsync();
        }

        private object? DateTimeToDatabase(object arg)
        {
            if (arg == null)
                return null;

            DateTime dte = (DateTime)arg;

            return dte.ToString("s", CultureInfo.InvariantCulture);
        }

        private string ToTableName(string arg) => arg["Cached".Length..];

        public NewUpdater NewUpdater { get; set; }

        //private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        //private ConcurrentDictionary<string, CachedWar> UpdatingWars { get; set; } = new ConcurrentDictionary<string, CachedWar>();

        //private bool IsRunning { get; set; }

        //private bool StopRequested { get; set; }

        //public async Task UpdateAsync()
        //{
        //    if (IsRunning)
        //        return;

        //    IsRunning = true;

        //    StopRequested = false;

        //    while (!StopRequested)
        //    {
        //        for (int i = 0; i < CocApiConfiguration.NumberOfUpdaters; i++)
        //        {
        //            await UpdateClans();
        //        }
        //    }


        //    IsRunning = false;
        //}

        //public bool DownloadWars { get; set; }

        //private async Task UpdateWars(CachedClan cachedClan)
        //{
        //    if (DownloadWars == false || cachedClan.DownloadWars == false)
        //        return;

        //    IEnumerable<CachedWar> cachedWars = await SqlWriter.Select<CachedWar>()
        //                                                 .Where(w => (w.ClanTag == cachedClan.ClanTag ||
        //                                                             w.OpponentTag == cachedClan.ClanTag) &&
        //                                                             w.WarTag == null)
        //                                                 .QueryAsync();

        //    foreach(CachedWar cachedWar in cachedWars)
        //    {
        //        if (StopRequested)
        //            return;

        //        try
        //        {
        //            if (UpdatingWars.TryAdd(cachedWar.WarKey(), cachedWar))
        //                await UpdateWar(cachedWar);
        //        }
        //        finally
        //        {
        //            UpdatingWars.TryRemove(cachedWar.WarKey(), out CachedWar _);
        //        }

        //        await Task.Delay(50);
        //    }

        //    if (cachedWars.Count() == 0 && cachedClan.IsWarLogPublic)
        //    {
        //        IWar? war = await Wars.FetchAsync<CurrentWar>(cachedClan.ClanTag);

        //        if (war is CurrentWar fetchedWar)
        //        {
        //            CachedWar cachedWar = new CachedWar(fetchedWar, cachedClan.ClanTag);

        //            if (UpdatingWars.TryAdd(cachedWar.WarKey(), cachedWar))
        //            {
        //                try
        //                {
        //                    await SqlWriter.Insert(cachedWar).ExecuteAsync();

        //                    Wars.OnNewWar(fetchedWar);
        //                }
        //                finally
        //                {
        //                    UpdatingWars.TryRemove(cachedWar.WarKey(), out CachedWar _);
        //                }
        //            }
        //        }
        //    }
        //}

        //private async Task UpdateWar(CachedWar cachedWar)
        //{
        //    //todo lookup war log entry if needed
        //    IWar? war = null;

        //    string tagUsed = cachedWar.ClanTag;

        //    if (cachedWar.ClanUpdatesAt == null || DateTime.UtcNow > cachedWar.ClanUpdatesAt)
        //        war = await Wars.FetchAsync<CurrentWar>(cachedWar.ClanTag);

        //    if (war == null && (cachedWar.OpponentUpdatesAt == null || DateTime.UtcNow > cachedWar.OpponentUpdatesAt))
        //    {
        //        war = await Wars.FetchAsync<CurrentWar>(cachedWar.OpponentTag);

        //        tagUsed = cachedWar.OpponentTag;
        //    }

        //    if (cachedWar.Json != null)
        //    {
        //        CurrentWar currentWar = JsonConvert.DeserializeObject<CurrentWar>(cachedWar.Json);

        //        currentWar.Update(this, war);
        //    }

        //    if (war is CurrentWar fetchedWar)
        //    {
        //        if (fetchedWar.WarKey() == cachedWar.WarKey())
        //        {
        //            cachedWar.Json = JsonConvert.SerializeObject(fetchedWar);

        //            await SqlWriter.Update(cachedWar).ExecuteAsync();
        //        }
        //        else
        //        {
        //            CachedWar cachedWar1 = new CachedWar(fetchedWar, tagUsed);

        //            if (UpdatingWars.TryAdd(cachedWar1.WarKey(), cachedWar1))
        //            {
        //                try
        //                {
        //                    await SqlWriter.Insert(cachedWar1).ExecuteAsync();

        //                    Wars.OnNewWar(fetchedWar);
        //                }
        //                finally
        //                {
        //                    UpdatingWars.TryRemove(cachedWar1.WarKey(), out CachedWar _);
        //                }
        //            }
        //        }
        //    }
        //}

        //private bool UpdateIsAvailable(CachedWar cachedWar)
        //{
        //    //todo make this more sophisticated

        //    if (cachedWar.ClanUpdatesAt == null || cachedWar.OpponentUpdatesAt == null)
        //        return true;

        //    if (DateTime.UtcNow > cachedWar.ClanUpdatesAt || DateTime.UtcNow > cachedWar.OpponentUpdatesAt)
        //        return true;

        //    return false;
        //}


        //private async Task UpdateClans()
        //{
        //    //todo dont run this query inside the for loop
        //    IEnumerable<CachedClan> cachedClans = await SqlWriter.Select<CachedClan>()
        //                   .OrderByDesc(c => c.ClanUpdatesAt)
        //                   .Limit(100)
        //                   .QueryAsync();

        //    foreach (CachedClan cachedClan in cachedClans)
        //    {
        //        if (StopRequested)
        //            return;

        //        if (UpdatingClans.TryAdd(cachedClan.ClanTag, cachedClan))
        //        {
        //            try
        //            {
        //                await UpdateClan(cachedClan);

        //                await UpdateWars(cachedClan);
        //            }
        //            finally
        //            {
        //                UpdatingClans.TryRemove(cachedClan.ClanTag, out CachedClan _);
        //            }
        //        }
        //    }
        //}

        //private async Task UpdateClan(CachedClan cachedClan)
        //{
        //    if (cachedClan.Download == false || DateTime.UtcNow < cachedClan.ClanUpdatesAt)
        //        return;

        //    Clan? fetchedClan = await Clans.FetchAsync(cachedClan.ClanTag);

        //    if (cachedClan.Json != null && fetchedClan != null)
        //    {
        //        Clan storedClan = JsonConvert.DeserializeObject<Clan>(cachedClan.Json);

        //        storedClan.Update(this, fetchedClan);
        //    }

        //    if (fetchedClan != null)
        //    {
        //        cachedClan.Json = JsonConvert.SerializeObject(fetchedClan);

        //        cachedClan.IsWarLogPublic = fetchedClan.IsWarLogPublic;

        //        if (fetchedClan.ServerExpirationUtc.HasValue)
        //            cachedClan.ClanUpdatesAt = fetchedClan.ServerExpirationUtc.Value;
                
        //        await SqlWriter.Update(cachedClan).ExecuteAsync();

        //        await Task.Delay(50);
        //    }
        //}

        //public async Task AddClan(string clanTag, bool downloadVillages, bool downloadWars, bool downloadCwl, bool download = true)
        //{
        //    CachedClan cachedClan = new CachedClan { ClanTag = clanTag, 
        //                                             Download = download, 
        //                                             DownloadCwl = downloadCwl, 
        //                                             DownloadVillages = downloadVillages, 
        //                                             DownloadWars = downloadWars };

        //    await SqlWriter.Insert(cachedClan).QueryFirstAsync();
        //}

        public Clans Clans { get; set; }

        public bool IsAvailable
        {
            get
            {
                lock (_isAvailableLock)
                {
                    return _isAvailable;
                }
            }

            internal set
            {
                lock (_isAvailableLock)
                {
                    if (_isAvailable != value)
                    {
                        _isAvailable = value;

                        ApiIsAvailableChanged?.Invoke(this, _isAvailable);
                    }
                }
            }
        }

        public Labels Labels { get; private set; }
        public Leagues Leagues { get; private set; }
        public Locations Locations { get; private set; }
        public Villages Villages { get; private set; }
        public Wars Wars { get; private set; }
        public Test Test { get; private set; }

        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Disposes all disposable items.  Pending tasks will be canceled.
        /// </summary>
        public void Dispose()
        {
            foreach (ClanUpdateGroup updateService in Clans.ClanUpdateGroups)
            {
                updateService.StopUpdating();
            }

            foreach (CancellationTokenSource cancellationTokenSource in _cancellationTokenSources)
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
        public ConcurrentBag<WebResponseTimer> GetTimers() => WebResponse.GetTimers();

        public string GetTokenStatus() => WebResponse.GetTokenStatus();

        internal async Task ClanQueueRestartAsync()
        {
            try
            {
                //wait to allow the updater to finish crashing
                await Task.Delay(5000).ConfigureAwait(false);

                Clans.StartQueue();
            }
            catch (Exception)
            {
                OnLog(new LogEventArgs(nameof(CocApi), nameof(ClanQueueRestartAsync), LogLevel.Critical, LoggingEvent.QueueRestartFailed.ToString()));
            }
        }

        internal async Task<IDownloadable?> FetchAsync<TResult>(string url, CancellationToken? cancellationToken = null) where TResult : class, IDownloadable /*, new()*/
        {
            try
            {
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

                TokenObject token = await WebResponse.GetTokenAsync();

                AddCancellationTokenSource(cts);

                if (cancellationToken == null)
                    cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

                IDownloadable? result = await WebResponse.GetDownloadableAsync<TResult>(url, token, cts.Token).ConfigureAwait(false);

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

        internal async Task VillageQueueRestartAsync()
        {
            try
            {
                //wait to allow the updater to finish crashing
                await Task.Delay(5000).ConfigureAwait(false);

                Villages.StartQueue();
            }
            catch (Exception)
            {
                OnLog(new LogEventArgs(nameof(CocApi), nameof(VillageQueueRestartAsync), LogLevel.Critical, LoggingEvent.QueueRestartFailed.ToString()));
            }
        }

        internal async Task WarQueueRestartAsync()
        {
            try
            {
                //wait to allow the updater to finish crashing
                await Task.Delay(5000).ConfigureAwait(false);

                Wars.StartQueue();
            }
            catch (Exception)
            {
                OnLog(new LogEventArgs(nameof(CocApi), nameof(WarQueueRestartAsync), LogLevel.Critical, LoggingEvent.QueueRestartFailed.ToString()));
            }
        }

        private void AddCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Add(cts);
            }
        }

        private void RemoveCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Remove(cts);
            }
        }
    }
}