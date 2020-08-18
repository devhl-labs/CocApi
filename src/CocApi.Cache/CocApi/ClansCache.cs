using Dapper.SqlWriter;
//using CocApi.Cache.Models;
//using CocApi.Cache.Models.Cache;
//using CocApi.Cache.Models.Clans;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Model;
using CocApi.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using CocApi.Client;
using CocApi.Cache.Models.Cache;
using CocApi.Cache.CocApi;

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

        //public async Task<Clan?> GetAsync(string tag) => await _cocApi.GetAsync<Clan>(Clan.Url(tag));

        //public async Task<CachedItem?> GetWithHttpInfoAsync(string tag) => await _cocApi.GetWithHttpInfoAsync(Clan.Url(tag));

        //public async Task<CachedItem?> GetCurrentWarWithHttpInfoAsync(string tag) => await _cocApi.GetWithHttpInfoAsync(ClanWar.Url(tag));

        //public async Task<ClanWar?> GetCurrentWarAsync(string tag) => await _cocApi.GetAsync<ClanWar>(ClanWar.Url(tag));

        public async Task<Clan> GetAsync(string tag)
        {
            CachedClan result = await GetWithHttpInfoAsync(tag);

            return result.Data;
        }

        public async Task<CachedClan> GetWithHttpInfoAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Clans.Where(i => i.Tag == tag).FirstAsync().ConfigureAwait(false);
        }

        public async Task<Clan?> GetFirstOrDefaultAsync(string tag)
        {
            CachedClan? result = await GetWithHttpInfoFirstOrDefaultAsync(tag);

            return result?.Data;
        }

        public async Task<CachedClan?> GetWithHttpInfoFirstOrDefaultAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Clans.Where(i => i.Tag == tag).FirstOrDefaultAsync().ConfigureAwait(false);
        }



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


        public bool DownloadMembers { get; set; }
        public bool DownloadCurrentWars { get; set; } = true;
        public bool DownloadCwl { get; set; } = true;


        public async Task AddAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            if (downloadClan == false && downloadMembers == true)
                throw new Exception("DownloadClan must be true to download members.");

            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClan cachedClan = await dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedClan != null)
                return;
            
            await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers, dbContext);

            return;

            //cachedClan = new CachedClan
            //{
            //    Tag = formattedTag,
            //    Download = downloadClan,
            //    DownloadCurrentWar = downloadWars,
            //    DownloadCwl = downloadCwl,
            //    DownloadMembers = downloadMembers
            //};

            //cachedContext.Clans.Update(cachedClan);

            //CachedClanWar cachedClanWar = new CachedClanWar
            //{
            //    Tag = formattedTag
            //};

            //cachedContext.ClanWars.Update(cachedClanWar);

            //await cachedContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(string tag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadMembers)
        {
            if (downloadClan == false && downloadMembers == true)
                throw new Exception("DownloadClan must be true to download members.");

            if (Clash.TryGetValidTag(tag, out string formattedTag) == false)
                throw new InvalidTagException(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClan cachedClan = await dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedClan == null)
            {
                await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers, dbContext);

                return;
            }

            cachedClan = new CachedClan
            {
                Tag = formattedTag,
                Download = downloadClan,
                DownloadCurrentWar = downloadWars,
                DownloadCwl = downloadCwl,
                DownloadMembers = downloadMembers
            };

            dbContext.Clans.Update(cachedClan);

            await dbContext.SaveChangesAsync();
        }

        private async Task InsertCachedClanAsync(string formattedTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadMembers, CachedContext dbContext)
        {
            CachedClan cachedClan = new CachedClan
            {
                Tag = formattedTag,
                Download = downloadClan,
                DownloadCurrentWar = downloadWars,
                DownloadCwl = downloadCwl,
                DownloadMembers = downloadMembers
            };

            dbContext.Clans.Update(cachedClan);

            CachedClanWar cachedClanWar = new CachedClanWar
            {
                Tag = formattedTag
            };

            dbContext.ClanWars.Update(cachedClanWar);

            CachedClanWarLeagueGroup group = new CachedClanWarLeagueGroup
            {
                Tag = formattedTag
            };

            dbContext.Groups.Update(group);

            await dbContext.SaveChangesAsync();

            return;
        }















































































        private bool IsRunning { get; set; }

        private bool StopRequested { get; set; }

        private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        private ConcurrentDictionary<int, CachedWar> UpdatingWar { get; set; } = new ConcurrentDictionary<int, CachedWar>();

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

                        CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

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

        private async Task UpdateAsync(CachedClan cachedClan, CachedContext dbContext)
        {
            if (UpdatingClans.TryAdd(cachedClan.Tag, cachedClan) == false)
                return;

            try
            {
                List<Task> tasks = new List<Task>();

                if (cachedClan.Download)
                    tasks.Add(UpdateClanAsync(cachedClan, dbContext));

                //todo uncomment this
                //if (Clash.IsCwlEnabled() && DownloadCwl && cachedClan.DownloadCwl)
                    tasks.Add(UpdateCwl(cachedClan, dbContext));

                await dbContext.SaveChangesAsync();

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                UpdatingClans.TryRemove(cachedClan.Tag, out CachedClan _);
            }
        }

        private async Task UpdateCwl(CachedClan cachedClan, CachedContext dbContext)
        {
            CachedClanWarLeagueGroup group = await dbContext.Groups.Where(g => g.Tag == cachedClan.Tag).FirstAsync();

            if (group.IsLocallyExpired() == false || group.IsServerExpired() == false)
                return;

            ApiResponse<ClanWarLeagueGroup> apiResponse;

            try
            {
                apiResponse = await _cocApi.ClansApi.GetClanWarLeagueGroupWithHttpInfoAsync(cachedClan.Tag);
            }
            catch (ApiException e)
            {
                e.ErrorContent

                var a = e.GetType();

                var b = e.GetType().Name;

                throw;
            }
            

            if (group.Data != null && group.Data.Equals(apiResponse.Data))
                return;

            group.UpdateFrom(apiResponse, _cocApiConfiguration.LeagueGroupTimeToLive);

            //todo insert new wars!
        }

        private async Task UpdateClanAsync(CachedClan cachedClan, CachedContext dbContext)
        {
            if (cachedClan.IsServerExpired() == false || cachedClan.IsLocallyExpired() == false)
                return;

            ApiResponse<Clan> apiResponse = await _cocApi.ClansApi.GetClanWithHttpInfoAsync(cachedClan.Tag);

            if (cachedClan.ServerExpiration >= apiResponse.ServerExpiration)
                return;

            if (cachedClan.Data != null && _cocApi.IsEqual(cachedClan.Data, apiResponse.Data) == false)
                ClanUpdated?.Invoke(this, new ChangedEventArgs<Clan>(cachedClan.Data, apiResponse.Data));

            cachedClan.UpdateFromResponse(apiResponse, _cocApiConfiguration.ClansTimeToLive);

            dbContext.Clans.Update(cachedClan);

            if (DownloadMembers && cachedClan.DownloadMembers)
                await UpdateMembersAsync(cachedClan, dbContext).ConfigureAwait(false);

            if (DownloadCurrentWars && cachedClan.DownloadCurrentWar && apiResponse.Data.IsWarLogPublic)
                await UpdateClanWar(cachedClan, dbContext);
        }

        private async Task UpdateMembersAsync(CachedClan cachedClan, CachedContext dbContext)
        {
            if (cachedClan.Data == null)
                return;

            List<Task> tasks = new List<Task>();

            foreach (ClanMember member in cachedClan.Data.MemberList)
                tasks.Add(UpdateMember(member, dbContext));

            await Task.WhenAll().ConfigureAwait(false);
        }

        private async Task UpdateClanWar(CachedClan cachedClan, CachedContext dbContext)
        {
            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync();

            if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                return;

            ApiResponse<ClanWar> apiResponse = await _cocApi.ClansApi.GetCurrentWarWithHttpInfoAsync(cachedClan.Tag);

            if (IsNewWar(cachedClanWar, apiResponse))
                await InsertNewWarAsync(cachedClan, apiResponse, dbContext);

            cachedClanWar.UpdateFromResponse(apiResponse, _cocApiConfiguration.CurrentWarTimeToLive); //todo better ttl 

            dbContext.ClanWars.Update(cachedClanWar);
        }

        private bool IsNewWar(CachedClanWar cachedClanWar, ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse == null || apiResponse.Data.PreparationStartTime == DateTime.MinValue)
                return false;

            if (cachedClanWar.Data?.PreparationStartTime == apiResponse.Data.PreparationStartTime)
                return false;

            return true;
        }

        private async Task InsertNewWarAsync(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, CachedContext dbContext)
        {
            CachedWar cachedWar = new CachedWar(cachedClan, apiResponse, _cocApiConfiguration.CurrentWarTimeToLive);

            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
                return;

            try
            {
                CachedWar? exists = await dbContext.Wars.Where(
                    w => w.PrepStartTime == apiResponse.Data.PreparationStartTime && 
                        w.ClanTag == apiResponse.Data.Clans.First().Value.Tag).FirstOrDefaultAsync();

                if (exists != null)
                    return;

                dbContext.Wars.Update(cachedWar);

                await dbContext.SaveChangesAsync();
            }
            finally{
                UpdatingWar.TryRemove(cachedWar.GetHashCode(), out var _);
            }
        }

        //private async Task UpdateCwl(string tag, CachedContext dbContext)
        //{
        //    ClanWarLeagueGroup? cachedLeagueGroup = await AddOrUpdateLeagueGroupAsync(tag, dbContext);

        //    if (cachedLeagueGroup == null)
        //        return;

        //    foreach (var round in cachedLeagueGroup.Rounds)
        //        foreach (string warTag in round.WarTags)
        //            await InsertLeagueWar(warTag);

        //    if (!(await CocApi.Wars.GetLeagueGroupAsync(clan.ClanTag, CacheOption.AllowExpiredServerResponse).ConfigureAwait(false) is LeagueGroup leagueGroup))
        //        return;

        //    foreach (Round round in leagueGroup.Rounds.EmptyIfNull())
        //    {
        //        foreach (string warTag in round.WarTags.EmptyIfNull().Where(w => w != "#0"))
        //        {
        //            try
        //            {
        //                if (UpdatingWar.TryAdd(warTag, null) == false)
        //                    continue;

        //                if ((await CocApi.Wars.GetAsync<LeagueWar>(warTag, CacheOption.CacheOnly).ConfigureAwait(false) is LeagueWar leagueWar))
        //                    continue;

        //                if (!(await CocApi.Wars.GetAsync<LeagueWar>(warTag, CacheOption.ServerOnly).ConfigureAwait(false) is LeagueWar fetched))
        //                    continue;

        //                CachedWar cachedWar = new CachedWar(fetched);

        //                await SqlWriter.Insert(cachedWar).ExecuteAsync().ConfigureAwait(false);

        //                CocApi.Wars.OnNewWar(fetched);

        //                if (fetched.WarClans.Any(wc => wc.ClanTag == clan.ClanTag))
        //                    break;
        //            }
        //            finally
        //            {
        //                UpdatingWar.TryRemove(warTag, out _);
        //            }
        //        }
        //    }
        //}

        //private async Task<ClanWarLeagueGroup?> AddOrUpdateLeagueGroupAsync(string tag, CachedContext dbContext)
        //{
        //    CachedItem cachedItem = await dbContext.Items.Where(i => i.Path == ClanWarLeagueGroup.Url(tag)).FirstOrDefaultAsync().ConfigureAwait(false);

        //    if (cachedItem != null && (cachedItem.IsLocallyExpired() == false || cachedItem.IsServerExpired() == false))
        //        return JsonConvert.DeserializeObject<ClanWarLeagueGroup>(cachedItem.Raw);

        //    cachedItem ??= new CachedItem();

        //    ApiResponse<ClanWarLeagueGroup> apiResponse = await _clansApi.GetClanWarLeagueGroupWithHttpInfoAsync(tag).ConfigureAwait(false);

        //    CachedItem cachedResponse = apiResponse.ToCachedItem(_cocApiConfiguration.LeagueGroupTimeToLive, ClanWarLeagueGroup.Url(tag)); //todo league group not foun

        //    cachedItem.UpdateFromResponse(cachedResponse);

        //    dbContext.Items.Update(cachedItem);

        //    return apiResponse.Data;
        //}

        private async Task InsertLeagueWar(string warTag)
        {
            throw new NotImplementedException();
        }

        private async Task UpdateMember(ClanMember member, CachedContext dbContext)
        {
            CachedPlayer cachedPlayer = await _cocApi.PlayersCache.AddAsync(member.Tag, false);

            await _cocApi.PlayersCache.UpdatePlayerAsync(cachedPlayer, dbContext);
        }
    }
}