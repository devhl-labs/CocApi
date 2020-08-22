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
using System.Text.RegularExpressions;

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

        public event AsyncEventHandler<ChangedEventArgs<ClanWarLog>>? ClanWarLogUpdated;

        public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        //public event AsyncEventHandler<DonationEventArgs>? ClanDonation;

        //public event AsyncEventHandler<LabelsChangedEventArgs<Clan>>? ClanLabelsChanged;

        //public event AsyncEventHandler<ChildChangedEventArgs<Clan, ClanVillage>>? ClanVillageChanged;

        //public event AsyncEventHandler<ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>>? ClanVillagesJoined;

        //public event AsyncEventHandler<ChangedEventArgs<Clan, ImmutableArray<ClanVillage>>>? ClanVillagesLeft;

        //public event AsyncEventHandler<ChangedEventArgs<Clan>>? ClanBadgeUrlChanged;

        internal void OnClanWarUpdated(Clan clan, ClanWar stored, ClanWar fetched)
            => ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(clan, stored, fetched));

        internal void OnClanWarLogUpdated(ClanWarLog stored, ClanWarLog fetched)
            => ClanWarLogUpdated?.Invoke(this, new ChangedEventArgs<ClanWarLog>(stored, fetched));

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

            CachedClanWarLog log = new CachedClanWarLog
            {
                Tag = formattedTag
            };

            dbContext.WarLogs.Update(log);

            await dbContext.SaveChangesAsync();

            return;
        }

        private bool IsUpdatingClans { get; set; }

        private bool StopUpdatingClansRequested { get; set; }

        private bool IsUpdatingWars { get; set; }

        private bool StopUpdatingWarsRequested { get; set; }

        private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        private ConcurrentDictionary<int, CachedWar> UpdatingWar { get; set; } = new ConcurrentDictionary<int, CachedWar>();

        public void Start()
        {
            UpdateClans();

            UpdateWars();

        }

        private void UpdateClans()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (IsUpdatingClans)
                        return;

                    IsUpdatingClans = true;

                    StopUpdatingClansRequested = false;

                    _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(UpdateClans), LogLevel.Information));

                    int clanId = 0;

                    while (!StopUpdatingClansRequested)
                    {
                        List<Task> tasks = new List<Task>();

                        using var scope = _services.CreateScope();

                        CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                        List<CachedClan> cachedClans = await dbContext.Clans.Where(v =>
                            v.Id > clanId).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedClans.Count; i++)
                            tasks.Add(UpdateEntireClanAsync(cachedClans[i], dbContext));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        await dbContext.SaveChangesAsync();

                        if (cachedClans.Count < _cocApiConfiguration.ConcurrentUpdates)
                            clanId = 0;
                        else
                            clanId = cachedClans.Max(c => c.Id);

                        await Task.Delay(_cocApiConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                    }

                    IsUpdatingClans = false;
                }
                catch (Exception e)
                {
                    _cocApi.OnLog(new ExceptionEventArgs(nameof(ClansCache), nameof(UpdateClans), e));

                    IsUpdatingClans = false;

                    UpdateClans();
                }
            });
        }

        private void UpdateWars()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (IsUpdatingWars)
                        return;

                    IsUpdatingWars = true;

                    StopUpdatingWarsRequested = false;

                    _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(UpdateWars), LogLevel.Information));

                    int clanId = 0;

                    while (!StopUpdatingWarsRequested)
                    {
                        List<Task> tasks = new List<Task>();

                        using var scope = _services.CreateScope();

                        CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                        List<CachedClan> cachedClans = await dbContext.Clans.Where(v =>
                            v.Id > clanId).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedClans.Count; i++)
                            tasks.Add(UpdateWars(cachedClans[i], dbContext));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        await dbContext.SaveChangesAsync();

                        if (cachedClans.Count < _cocApiConfiguration.ConcurrentUpdates)
                            clanId = 0;
                        else
                            clanId = cachedClans.Max(c => c.Id);

                        await Task.Delay(_cocApiConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                    }

                    IsUpdatingWars = false;
                }
                catch (Exception e)
                {
                    _cocApi.OnLog(new ExceptionEventArgs(nameof(ClansCache), nameof(UpdateWars), e));

                    IsUpdatingWars = false;

                    UpdateWars();
                }
            });
        }

        private async Task UpdateWars(CachedClan cachedClan, CachedContext dbContext)
        {
            if (cachedClan.DownloadCurrentWar == false && cachedClan.DownloadCwl == false)
                return;

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync();

            List<CachedWar> cachedWars = await dbContext.Wars.Where(w => 
                (w.ClanTag == cachedClan.Tag || w.OpponentTag == cachedClan.Tag) &&
                w.State != ClanWar.StateEnum.WarEnded &&
                w.IsFinal == false).ToListAsync();

            foreach (CachedWar cachedWar in cachedWars)
                if (cachedWar.WarTag == null)
                    await UpdateNormalWar(cachedClan, cachedClanWar, cachedWar, dbContext);
                else
                    await UpdateLeagueWar(cachedWar, dbContext);
        }

        private Task UpdateNormalWar(CachedClan cachedClan, CachedClanWar cachedClanWar, CachedWar cachedWar, CachedContext dbContext)
        {
            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
                return Task.CompletedTask;

            try
            {
                if (cachedWar.Data!.Equals(cachedClanWar.Data) == false)
                    ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(cachedClan.Data, cachedClanWar.Data, cachedWar.Data));

                cachedWar.UpdateFromCache(cachedClanWar);

                dbContext.Wars.Update(cachedWar);

                return Task.CompletedTask;
            }
            finally
            {
                UpdatingWar.TryRemove(cachedWar.GetHashCode(), out _);
            }
        }

        private Task UpdateLeagueWar(CachedWar cachedWar, CachedContext dbContext)
        {
            throw new NotImplementedException();
        }

        public async Task StopAsync()
        {
            StopUpdatingClansRequested = true;

            _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(StopAsync), LogLevel.Information));

            while (IsUpdatingClans)
                await Task.Delay(500).ConfigureAwait(false);
        }

        private async Task UpdateEntireClanAsync(CachedClan cachedClan, CachedContext dbContext)
        {
            if (UpdatingClans.TryAdd(cachedClan.Tag, cachedClan) == false)
                return;

            try
            {
                List<Task> tasks = new List<Task>();

                if (cachedClan.Download)
                    tasks.Add(UpdateClanAsync(cachedClan, dbContext));

                if (Clash.IsCwlEnabled() && DownloadCwl && cachedClan.DownloadCwl)
                    tasks.Add(UpdateCwl(cachedClan, dbContext));

                tasks.Add(UpdateWarLog(cachedClan));

                await dbContext.SaveChangesAsync();

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                UpdatingClans.TryRemove(cachedClan.Tag, out CachedClan _);
            }
        }

        private async Task UpdateWarLog(CachedClan cachedClan)
        {
            if (cachedClan.Data?.IsWarLogPublic != true)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWarLog log = await dbContext.WarLogs.Where(g => g.Tag == cachedClan.Tag).FirstAsync();

            if (log.IsLocallyExpired() == false || log.IsServerExpired() == false)
                return;

            try
            {
                ApiResponse<ClanWarLog> apiResponse = await _cocApi.ClansApi.GetClanWarLogWithHttpInfoAsync(cachedClan.Tag);

                if (log.Data != null && log.Data.Equals(apiResponse.Data) == false)
                    ClanWarLogUpdated?.Invoke(this, new ChangedEventArgs<ClanWarLog>(log.Data, apiResponse.Data));

                if (log.Data != apiResponse.Data)
                    ClanWarLogUpdated?.Invoke(this, new ChangedEventArgs<ClanWarLog>(log.Data, apiResponse.Data));

                log.UpdateFromResponse(apiResponse, _cocApiConfiguration.WarLogTimeToLive);
            }
            catch (ApiException e)
            {
                log.UpdateFromResponse(e, _cocApiConfiguration.PrivateWarLogTimeToLive);
            }

            dbContext.WarLogs.Update(log);

            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateCwl(CachedClan cachedClan, CachedContext dbContext)
        {
            CachedClanWarLeagueGroup group = await dbContext.Groups.Where(g => g.Tag == cachedClan.Tag).FirstAsync();

            if (group.IsLocallyExpired() == false || group.IsServerExpired() == false)
                return;

            ApiResponse<ClanWarLeagueGroup>? apiResponse = await _cocApi.ClansApi.GetClanWarLeagueGroupWithHttpInfoOrDefaultAsync(cachedClan.Tag);

            if (group.Data != null && group.Data.Equals(apiResponse.Data))
                return;

            group.UpdateFrom(apiResponse, _cocApiConfiguration.LeagueGroupTimeToLive);

            if (apiResponse == null)
                return;

            List<Task> tasks = new List<Task>();

            foreach(var round in apiResponse.Data.Rounds)
                foreach(var warTag in round.WarTags)
                {
                    if (warTag == "#0")
                        break;

                    if (group.Data != null && group.Data.Rounds.Any(r => r.WarTags.Any(w => w == warTag)))
                        break;

                    ApiResponse<ClanWar> clanWar = await _clansApi.GetClanWarLeagueWarWithHttpInfoAsync(warTag);

                    tasks.Add(InsertNewWarAsync(cachedClan, clanWar, dbContext, warTag));
                }

            await Task.WhenAll(tasks).ConfigureAwait(false);
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
                await UpdateClanWar(cachedClan);
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

        private async Task UpdateClanWar(CachedClan cachedClan)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync();

            if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                return;

            ApiResponse<ClanWar> apiResponse = await _cocApi.ClansApi.GetCurrentWarWithHttpInfoAsync(cachedClan.Tag);

            if (IsNewWar(cachedClanWar, apiResponse))
                await InsertNewWarAsync(cachedClan, apiResponse, dbContext);

            cachedClanWar.UpdateFromResponse(apiResponse, _cocApiConfiguration.CurrentWarTimeToLive); //todo better ttl 

            dbContext.ClanWars.Update(cachedClanWar);

            await dbContext.SaveChangesAsync();
        }

        private bool IsNewWar(CachedClanWar cachedClanWar, ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse == null || apiResponse.Data.PreparationStartTime == DateTime.MinValue)
                return false;

            if (cachedClanWar.Data?.PreparationStartTime == apiResponse.Data.PreparationStartTime)
                return false;

            return true;
        }

        private async Task InsertNewWarAsync(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, CachedContext dbContext, string? warTag = null)
        {
            CachedWar cachedWar = new CachedWar(cachedClan, apiResponse, _cocApiConfiguration.CurrentWarTimeToLive, warTag);

            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
                return;

            try
            {
                CachedWar? exists = await dbContext.Wars.Where(
                    w => w.PreparationStartTime == apiResponse.Data.PreparationStartTime && 
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

        private async Task UpdateMember(ClanMember member, CachedContext dbContext)
        {
            CachedPlayer cachedPlayer = await _cocApi.PlayersCache.AddAsync(member.Tag, false);

            await _cocApi.PlayersCache.UpdatePlayerAsync(cachedPlayer, dbContext);
        }
    }
}