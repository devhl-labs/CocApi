using Dapper.SqlWriter;
using CocApi.Cache.Exceptions;
//using CocApi.Cache.Models;
using CocApi.Cache.Models.Cache;
//using CocApi.Cache.Models.Clans;
using CocApi.Cache.Models.Villages;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Model;
using CocApi.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CocApi.Client;
using System.Globalization;

namespace CocApi.Cache
{
    public sealed class ClansCache
    {
        private readonly CocApiClient _cocApi;
        private readonly CocApiConfiguration _cocApiConfiguration;
        private readonly ClansApi _clansApi;
        private readonly IServiceProvider _services;

        public ClansCache(CocApiClient cocApi, CocApiConfiguration cocApiConfiguration, IServiceProvider serviceProvider, ClansApi clansApi)
        {
            _cocApi = cocApi;
            _cocApiConfiguration = cocApiConfiguration;
            _services = serviceProvider;
            _clansApi = clansApi;
        }

        public event AsyncEventHandler<ChangedEventArgs<Clan>>? ClanUpdated;

        //public event AsyncEventHandler<DonationEventArgs>? ClanDonation;

        //public event AsyncEventHandler<LabelsChangedEventArgs<Clan>>? ClanLabelsChanged;

        //public event AsyncEventHandler<ChildChangedEventArgs<Clan, ClanVillage>>? ClanVillageChanged;

        //public event AsyncEventHandler<ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>>? ClanVillagesJoined;

        //public event AsyncEventHandler<ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>>? ClanVillagesLeft;

        //public event AsyncEventHandler<ChangedEventArgs<Clan>>? ClanBadgeUrlChanged;




        internal void OnClanUpdated(Clan stored, Clan fetched)
            => ClanUpdated?.Invoke(this, new ChangedEventArgs<Clan>(stored, fetched));

        public async Task<Clan?> GetAsync(string tag) => await _cocApi.GetAsync<Clan>(Clan.Url(tag));

        public async Task<CachedItem?> GetWithHttpInfoAsync(string tag) => await _cocApi.GetWithHttpInfoAsync(Clan.Url(tag));

        public async Task<CachedItem?> GetCurrentWarWithHttpInfoAsync(string tag) => await _cocApi.GetWithHttpInfoAsync(ClanWar.Url(tag));

        public async Task<ClanWar?> GetCurrentWarAsync(string tag) => await _cocApi.GetAsync<ClanWar>(ClanWar.Url(tag));





        //internal void OnBadgeUrlChanged(Clan fetched, Clan queued) => ClanBadgeUrlChanged?.Invoke(this, new ChangedEventArgs<Clan>(fetched, queued));

        //internal void OnClanChanged(Clan fetched, Clan queued) => ClanChanged?.Invoke(this, new ChangedEventArgs<Clan>(fetched, queued));

        //internal void OnClanVillageChanged(Clan fetched, ClanVillage fetchedVillage, ClanVillage queuedVillage) => ClanVillageChanged?.Invoke(this, new ChildChangedEventArgs<Clan, ClanVillage>(fetched, fetchedVillage, queuedVillage));

        //internal void OnClanVillagesJoined(Clan fetched, List<ClanVillage> fetchedClanVillages)
        //{
        //    if (fetchedClanVillages.Count > 0)
        //    {
        //        ClanVillagesJoined?.Invoke(this, new ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
        //    }
        //}

        //internal void OnClanVillagesLeft(Clan fetched, List<ClanVillage> fetchedClanVillages)
        //{
        //    if (fetchedClanVillages.Count > 0)
        //    {
        //        ClanVillagesLeft?.Invoke(this, new ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>(fetched, fetchedClanVillages.ToImmutableArray()));
        //    }
        //}

        //internal void OnDonation(Clan fetched, List<Donation> received, List<Donation> donated)
        //{
        //    if (received.Count > 0 || donated.Count > 0)
        //    {
        //        ClanDonation?.Invoke(this, new DonationEventArgs(fetched, received.ToImmutableArray(), donated.ToImmutableArray()));
        //    }
        //}

        //internal void OnLabelsChanged(Clan fetched, List<Label> added, List<Label> removed)
        //{
        //    if (added.Count == 0 && removed.Count == 0)
        //        return;

        //    ClanLabelsChanged?.Invoke(this, new LabelsChangedEventArgs<Clan>(fetched, added.ToImmutableArray(), removed.ToImmutableArray()));
        //}


        public bool DownloadClanVillages { get; set; }
        public bool DownloadCurrentWars { get; set; }
        public bool DownloadCwl { get; set; }


        public async Task AddAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadVillages = false)
        {
            if (downloadClan == false && downloadVillages == true)
                throw new Exception("DownloadClan must be true to enable village downloads.");

            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            using var scope = _services.CreateScope();

            CacheContext cachedContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedClan cachedClan = await cachedContext.Clans.Where(c => c.ClanTag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedClan != null)
                return;

            cachedClan = new CachedClan
            {
                ClanTag = formattedTag,
                DownloadClan = downloadClan,
                DownloadCurrentWar = downloadWars,
                DownloadCwl = downloadCwl,
                DownloadVillages = downloadVillages
            };

            cachedContext.Clans.Update(cachedClan);

            await cachedContext.SaveChangesAsync();
        }

        public async Task AddOrUpdateAsync(string tag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadVillages)
        {
            if (downloadClan == false && downloadVillages == true)
                throw new Exception("DownloadClan must be true to enable village downloads.");

            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            using var scope = _services.CreateScope();

            CacheContext cachedContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedClan cachedClan = await cachedContext.Clans.Where(c => c.ClanTag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            cachedClan ??= new CachedClan();

            cachedClan = new CachedClan
            {
                ClanTag = formattedTag,
                DownloadClan = downloadClan,
                DownloadCurrentWar = downloadWars,
                DownloadCwl = downloadCwl,
                DownloadVillages = downloadVillages
            };

            cachedContext.Clans.Update(cachedClan);

            await cachedContext.SaveChangesAsync();
        }
















































































        private bool IsRunning { get; set; }

        private SqlWriter SqlWriter { get; set; }

        private bool StopRequested { get; set; }

        private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        //private ConcurrentDictionary<string, CurrentWar?> UpdatingWar { get; set; } = new ConcurrentDictionary<string, CurrentWar?>();

        public void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (IsRunning)
                        return;

                    IsRunning = true;

                    StopRequested = false;

                    _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(Start), LogLevel.Information));

                    int id = 0;

                    while (!StopRequested)
                    {
                        List<Task> tasks = new List<Task>();

                        using var scope = _services.CreateScope();

                        CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                        List<CachedClan> cachedClans = await dbContext.Clans.Where(v =>
                            v.Id > id).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedClans.Count; i++)
                            tasks.Add(UpdateAsync(cachedClans[i], dbContext));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        await dbContext.SaveChangesAsync();

                        if (cachedClans.Count < _cocApiConfiguration.ConcurrentUpdates)
                            id = 0;
                        else
                            id = cachedClans.Max(c => c.Id);

                        await Task.Delay(_cocApiConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                    }

                    IsRunning = false;
                }
                catch (Exception e)
                {
                    _cocApi.OnLog(new ExceptionEventArgs(nameof(ClansCache), nameof(Start), e));

                    IsRunning = false;

                    Start();
                }
            });

        }

        public async Task StopAsync()
        {
            StopRequested = true;

            _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(StopAsync), LogLevel.Information));

            while (IsRunning)
                await Task.Delay(500).ConfigureAwait(false);
        }

        private async Task UpdateAsync(CachedClan cachedClan, CacheContext dbContext)
        {
            if (UpdatingClans.TryAdd(cachedClan.ClanTag, cachedClan) == false)
                return;

            try
            {
                List<Task> tasks = new List<Task>();

                if (cachedClan.DownloadClan)
                    tasks.Add(UpdateClanAsync(cachedClan, dbContext));

                if (DownloadCurrentWars && cachedClan.DownloadCurrentWar)
                    tasks.Add(UpdateClanWar(cachedClan.ClanTag));

                if (DownloadCwl && cachedClan.DownloadCwl)
                    tasks.Add(UpdateCwl(cachedClan.ClanTag));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                UpdatingClans.TryRemove(cachedClan.ClanTag, out CachedClan _);
            }
        }

        private async Task UpdateClanAsync(CachedClan cachedClan, CacheContext dbContext)
        {
            CachedItem? cachedItem = await _cocApi.ClansCache.GetWithHttpInfoAsync(cachedClan.ClanTag).ConfigureAwait(false);

            if (cachedItem != null && (cachedItem.IsServerExpired() == false || cachedItem.IsLocallyExpired() == false))
                return;

            cachedItem ??= new CachedItem();

            Clan? storedClan = null;

            if (string.IsNullOrEmpty(cachedItem.Raw) == false)
                storedClan = JsonConvert.DeserializeObject<Clan>(cachedItem.Raw);

            ApiResponse<Clan> apiResponse = await _cocApi.ClansApi.GetClanWithHttpInfoAsync(cachedClan.ClanTag);

            CachedItem responseItem = apiResponse.ToCachedItem(_cocApiConfiguration.VillageTimeToLive, Clan.Url(cachedClan.ClanTag));

            if (cachedItem.ServerExpirationDate == responseItem.ServerExpirationDate)
                return;

            cachedItem.DownloadDate = responseItem.DownloadDate;
            cachedItem.ServerExpirationDate = responseItem.ServerExpirationDate;
            cachedItem.LocalExpirationDate = responseItem.LocalExpirationDate;
            cachedItem.Raw = responseItem.Raw;
            cachedItem.Path = responseItem.Path;

            dbContext.Items.Update(cachedItem);

            Clan fetchedClan = JsonConvert.DeserializeObject<Clan>(apiResponse.RawContent);

            if (storedClan != null && _cocApi.IsEqual(storedClan, fetchedClan) == false)
                ClanUpdated?.Invoke(this, new ChangedEventArgs<Clan>(storedClan, fetchedClan));

            if (DownloadClanVillages && cachedClan.DownloadVillages)
                await UpdatePlayersAsync(fetchedClan, dbContext).ConfigureAwait(false);
        }

        private async Task UpdatePlayersAsync(Clan clan, CacheContext dbContext)
        {
            List<Task> tasks = new List<Task>();

            foreach (ClanMember member in clan.MemberList)            
                tasks.Add(UpdatePlayer(member, dbContext));           

            await Task.WhenAll().ConfigureAwait(false);
        }

        private async Task UpdateClanWar(string tag, CacheContext dbContext)
        {
            CachedItem? cachedItem = await _cocApi.ClansCache.GetCurrentWarWithHttpInfoAsync(tag);

            if (cachedItem != null && (cachedItem.IsLocallyExpired() == false || cachedItem.IsServerExpired() == false))
                return;

            cachedItem ??= new CachedItem();

            ApiResponse<ClanWar> apiResponse = await _cocApi.ClansApi.GetCurrentWarWithHttpInfoAsync(tag);

            CachedItem responseItem = apiResponse.ToCachedItem(_cocApiConfiguration.CurrentWarTimeToLive, ClanWar.Url(tag));

            if (cachedItem.ServerExpirationDate == responseItem.ServerExpirationDate)
                return;

            cachedItem.DownloadDate = responseItem.DownloadDate;
            cachedItem.ServerExpirationDate = responseItem.ServerExpirationDate;
            cachedItem.LocalExpirationDate = responseItem.LocalExpirationDate;
            cachedItem.Raw = responseItem.Raw;
            cachedItem.Path = responseItem.Path;

            dbContext.Items.Update(cachedItem);





            IWar ? war = await CocApi.Wars.GetAsync<CurrentWar>(clan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false);

            if (!(war is CurrentWar currentWar))
                return;

            if (UpdatingWar.TryAdd(currentWar.WarKey(), currentWar) == false)
                return;

            try
            {
                CachedWar? cachedWar = await CocApi.SqlWriter.Select<CachedWar>()
                                                            .Where(w => w.PrepStartTime == currentWar.PreparationStartTimeUtc &&
                                                                        w.ClanTag == currentWar.WarClans[0].ClanTag)
                                                            .QuerySingleOrDefaultAsync()
                                                            .ConfigureAwait(false);

                if (cachedWar != null)
                    return;

                await SqlWriter.Update<CachedWar>()
                                .Where(w => w.ClanTag == clan.ClanTag)
                                .Set(w => w.IsAvailableByClan == false)
                                .ExecuteAsync()
                                .ConfigureAwait(false);

                await SqlWriter.Update<CachedWar>()
                                .Where(w => w.OpponentTag == clan.ClanTag)
                                .Set(w => w.IsAvailableByOpponent == false)
                                .ExecuteAsync()
                                .ConfigureAwait(false);

                cachedWar = new CachedWar
                {
                    ClanTag = currentWar.WarClans[0].ClanTag,
                    EndTime = currentWar.EndTimeUtc,
                    IsFinal = false,
                    Json = currentWar.ToJson(),
                    OpponentTag = currentWar.WarClans[1].ClanTag,
                    PrepStartTime = currentWar.PreparationStartTimeUtc,
                    WarState = currentWar.State,
                    IsAvailableByClan = true,
                    IsAvailableByOpponent = true
                };

                await SqlWriter.Insert(cachedWar).ExecuteAsync().ConfigureAwait(false);

                CocApi.Wars.OnNewWar(currentWar);
            }
            finally
            {
                UpdatingWar.TryRemove(currentWar.WarKey(), out _);
            }
        }

        private async Task UpdateCwl(Clan clan)
        {
            if (!(await CocApi.Wars.GetLeagueGroupAsync(clan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false) is LeagueGroup leagueGroup))
                return;

            foreach (Round round in leagueGroup.Rounds.EmptyIfNull())
            {
                foreach (string warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
                {
                    try
                    {
                        if (UpdatingWar.TryAdd(warTag, null) == false)
                            continue;

                        if ((await CocApi.Wars.GetAsync<LeagueWar>(warTag, CacheOption.CacheOnly).ConfigureAwait(false) is LeagueWar leagueWar))
                            continue;

                        if (!(await CocApi.Wars.GetAsync<LeagueWar>(warTag, CacheOption.ServerOnly).ConfigureAwait(false) is LeagueWar fetched))
                            continue;

                        CachedWar cachedWar = new CachedWar(fetched);

                        await SqlWriter.Insert(cachedWar).ExecuteAsync().ConfigureAwait(false);

                        CocApi.Wars.OnNewWar(fetched);

                        if (fetched.WarClans.Any(wc => wc.ClanTag == clan.ClanTag))
                            break;
                    }
                    finally
                    {
                        UpdatingWar.TryRemove(warTag, out _);
                    }
                }
            }
        }

        private async Task UpdatePlayer(ClanMember member, CacheContext dbContext) 
            => await _cocApi.PlayersCache.UpdatePlayerAsync(member.Tag, dbContext);
    }
}