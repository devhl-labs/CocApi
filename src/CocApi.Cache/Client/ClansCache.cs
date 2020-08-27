using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public sealed class ClansCache
    {
        private readonly ClansApi _clansApi;
        private readonly CocApiClientBase _cocApi;
        private readonly CocApiConfiguration _cocApiConfiguration;
        private readonly IServiceProvider _services;

        public ClansCache(CocApiClientBase cocApi, CocApiConfiguration cocApiConfiguration, IServiceProvider serviceProvider, ClansApi clansApi/*, CachedContext dbContext*/)
        {
            _cocApi = cocApi;
            _cocApiConfiguration = cocApiConfiguration;
            _services = serviceProvider;
            _clansApi = clansApi;
        }

        public event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;

        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarAdded;

        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEndingSoon;

        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEndNotSeen;

        public event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;

        public event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;

        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarStartingSoon;

        public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        public bool DownloadCurrentWars { get; set; } = true;

        public bool DownloadCwl { get; set; } = true;

        public bool DownloadMembers { get; set; } = true;

        private bool IsUpdatingClans { get; set; }

        private bool StopUpdatingClansRequested { get; set; }

        private ConcurrentDictionary<string, CachedClan> UpdatingClans { get; set; } = new ConcurrentDictionary<string, CachedClan>();

        private ConcurrentDictionary<int, CachedWar> UpdatingWar { get; set; } = new ConcurrentDictionary<int, CachedWar>();

        public async Task AddAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            if (downloadClan == false && downloadMembers == true)
                throw new Exception("DownloadClan must be true to download members.");

            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClan cachedClan = await dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedClan != null)
                return;

            await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers);

            return;
        }

        public async Task<ClanWar?> GetActiveClanWarAsync(string tag)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            var cache = await dbContext.Wars.Where(i =>
                (i.ClanTag == formattedTag ||
                i.OpponentTag == formattedTag))
                .OrderByDescending(w => w.PreparationStartTime)
                .ToListAsync().ConfigureAwait(false);

            if (cache.Count == 0)
                return null;

            foreach (var item in cache)
                item.Data.Type = item.Type;

            return cache.FirstOrDefault(c => c.State == ClanWar.StateEnum.InWar)?.Data
                ?? cache.FirstOrDefault(c => c.State == ClanWar.StateEnum.Preparation)?.Data
                ?? cache.First().Data;
        }

        public async Task<CachedClan> GetCacheAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Clans.Where(i => i.Tag == tag).FirstAsync().ConfigureAwait(false);
        }

        public async Task<CachedClan?> GetCacheOrDefaultAsync(string tag)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Clans.Where(i => i.Tag == tag).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<Clan> GetClanAsync(string tag)
        {
            CachedClan result = await GetCacheAsync(tag).ConfigureAwait(false);

            return result.Data;
        }

        public async Task<Clan?> GetClanOrDefaultAsync(string tag)
        {
            CachedClan? result = await GetCacheOrDefaultAsync(tag).ConfigureAwait(false);

            return result?.Data;
        }

        public async Task<List<ClanWar>> GetClanWarsAsync(string tag)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            var cache = await dbContext.Wars.Where(i =>
                i.ClanTag == formattedTag ||
                i.OpponentTag == formattedTag)
                .ToListAsync().ConfigureAwait(false);

            List<ClanWar> clanWars = new List<ClanWar>();

            foreach (var item in cache)
            {
                item.Data.Type = item.Type;

                clanWars.Add(item.Data);
            }

            return clanWars;
        }

        public async Task<Clan> GetOrFetchClanAsync(string tag)
        {
            return (await GetCacheOrDefaultAsync(tag).ConfigureAwait(false))?.Data
                ?? await _clansApi.GetClanAsync(tag).ConfigureAwait(false);
        }

        public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag)
        {
            return (await GetCacheOrDefaultAsync(tag).ConfigureAwait(false))?.Data
                ?? await _clansApi.GetClanOrDefaultAsync(tag).ConfigureAwait(false);
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (IsUpdatingClans)
                        return;

                    IsUpdatingClans = true;

                    StopUpdatingClansRequested = false;

                    _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(Start), LogLevel.Information));

                    int clanId = 0;

                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    while (!StopUpdatingClansRequested)
                    {
                        List<Task> tasks = new List<Task>();

                        List<CachedClan> cachedClans = await dbContext.Clans.Where(v =>
                            v.Id > clanId).OrderBy(v => v.Id).Take(_cocApiConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedClans.Count; i++)
                            tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

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
                    _cocApi.OnLog(new ExceptionEventArgs(nameof(ClansCache), nameof(Start), e));

                    IsUpdatingClans = false;

                    Start();
                }
            });
        }

        public async Task StopAsync()
        {
            StopUpdatingClansRequested = true;

            _cocApi.OnLog(new LogEventArgs(nameof(ClansCache), nameof(StopAsync), LogLevel.Information));

            while (IsUpdatingClans)
                await Task.Delay(500).ConfigureAwait(false);
        }

        public async Task UpdateAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            if (downloadClan == false && downloadMembers == true)
                throw new ArgumentException("DownloadClan must be true to download members.");

            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClan cachedClan = await dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedClan == null)
            {
                await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers);

                return;
            }

            cachedClan.Tag = formattedTag;
            cachedClan.Download = downloadClan;
            cachedClan.DownloadCurrentWar = downloadWars;
            cachedClan.DownloadCwl = downloadCwl;
            cachedClan.DownloadMembers = downloadMembers;

            dbContext.Clans.Update(cachedClan);

            await dbContext.SaveChangesAsync();
        }

        internal void OnClanUpdated(Clan stored, Clan fetched)
            => ClanUpdated?.Invoke(this, new ClanUpdatedEventArgs(stored, fetched));

        internal void OnClanWarAdded(ClanWar clanWar)
            => ClanWarAdded?.Invoke(this, new ClanWarEventArgs(clanWar));

        internal void OnClanWarEndingSoon(ClanWar clanWar)
            => ClanWarEndingSoon?.Invoke(this, new ClanWarEventArgs(clanWar));

        internal void OnClanWarEndNotSeen(ClanWar clanWar)
                                                                                                                                                                                            => ClanWarEndNotSeen?.Invoke(this, new ClanWarEventArgs(clanWar));

        internal void OnClanWarLeagueGroupUpdated(Clan clan, ClanWarLeagueGroup stored, ClanWarLeagueGroup fetched)
            => ClanWarLeagueGroupUpdated?.Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(clan, stored, fetched));

        internal void OnClanWarLogUpdated(ClanWarLog stored, ClanWarLog fetched)
            => ClanWarLogUpdated?.Invoke(this, new ClanWarLogUpdatedEventArgs(stored, fetched));

        internal void OnClanWarStartingSoon(ClanWar clanWar)
                            => ClanWarStartingSoon?.Invoke(this, new ClanWarEventArgs(clanWar));

        internal void OnClanWarUpdated(Clan clan, ClanWar stored, ClanWar fetched)
            => ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(clan, stored, fetched));

        private async Task InsertCachedClanAsync(string formattedTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadMembers)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

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

        private async Task InsertNewWarAsync(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, string? warTag = null)
        {
            CachedWar cachedWar = new CachedWar(cachedClan, apiResponse, _cocApiConfiguration.CurrentWarTimeToLive, warTag);

            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedWar? exists = await dbContext.Wars.Where(
                    w => w.PreparationStartTime == apiResponse.Data.PreparationStartTime &&
                        w.ClanTag == apiResponse.Data.Clans.First().Value.Tag).FirstOrDefaultAsync();

                if (exists != null)
                    return;

                dbContext.Wars.Update(cachedWar);

                await dbContext.SaveChangesAsync();

                OnClanWarAdded(cachedWar.Data);
            }
            finally
            {
                UpdatingWar.TryRemove(cachedWar.GetHashCode(), out var _);
            }
        }

        private bool IsNewWar(CachedClanWar cachedClanWar, ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse == null || apiResponse.Data.State == ClanWar.StateEnum.NotInWar)
                return false;

            if (cachedClanWar.Data == null)
                return true;

            if (cachedClanWar.Data.PreparationStartTime == apiResponse.Data.PreparationStartTime)
                return false;

            return true;
        }

        private void SendWarAnnouncements(CachedWar cachedWar)
        {
            if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
                DateTime.UtcNow > cachedWar.Data.StartTime.AddHours(-1) &&
                DateTime.UtcNow < cachedWar.Data.StartTime)
            {
                cachedWar.Announcements |= Announcements.WarStartingSoon;
                OnClanWarStartingSoon(cachedWar.Data);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                DateTime.UtcNow > cachedWar.Data.EndTime.AddHours(-1) &&
                DateTime.UtcNow < cachedWar.Data.EndTime)
            {
                cachedWar.Announcements |= Announcements.WarEndingSoon;
                OnClanWarEndingSoon(cachedWar.Data);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                cachedWar.State != ClanWar.StateEnum.WarEnded &&
                cachedWar.IsFinal == true &&
                DateTime.UtcNow > cachedWar.EndTime &&
                DateTime.UtcNow.Day == cachedWar.EndTime.Value.Day &&
                cachedWar.AllAttacksUsed() == false)
            {
                cachedWar.Announcements |= Announcements.WarEndNotSeen;
                OnClanWarEndNotSeen(cachedWar.Data);
            }
        }

        private async Task UpdateClanAsync(CachedClan cachedClan)
        {
            if (cachedClan.Download == false || cachedClan.IsServerExpired() == false || cachedClan.IsLocallyExpired() == false)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            List<Task> tasks = new List<Task>();

            try
            {
                ApiResponse<Clan> apiResponse = await _cocApi.ClansApi.GetClanResponseAsync(cachedClan.Tag);

                if (cachedClan.Data != null && _cocApi.HasUpdated(cachedClan.Data, apiResponse.Data) == false)
                    OnClanUpdated(cachedClan.Data, apiResponse.Data);

                cachedClan.UpdateFrom(apiResponse, _cocApi.TimeToLive(cachedClan, apiResponse));

                tasks.Add(UpdateClanWar(cachedClan));

                tasks.Add(UpdateWarLog(cachedClan));

                tasks.Add(UpdateMembersAsync(cachedClan));
            }
            catch (ApiException e)
            {
                cachedClan.UpdateFrom(e, _cocApi.TimeToLive(cachedClan, e));
            }

            dbContext.Clans.Update(cachedClan);

            await dbContext.SaveChangesAsync();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateClanWar(CachedClan cachedClan)
        {
            if (!DownloadCurrentWars || !cachedClan.DownloadCurrentWar || !cachedClan.Data.IsWarLogPublic)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync();

            if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                return;

            ApiResponse<ClanWar>? apiResponse = null;

            try
            {
                apiResponse = await _cocApi.ClansApi.GetCurrentWarResponseAsync(cachedClan.Tag);

                if (IsNewWar(cachedClanWar, apiResponse))
                {
                    await InsertNewWarAsync(cachedClan, apiResponse);

                    cachedClanWar.Type = apiResponse.Data.Type;
                }

                cachedClanWar.UpdateFrom(apiResponse, _cocApi.TimeToLive(cachedClanWar, apiResponse));
            }
            catch (ApiException e)
            {
                cachedClanWar.UpdateFrom(e, _cocApi.TimeToLive(cachedClanWar, e));
            }

            dbContext.ClanWars.Update(cachedClanWar);

            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateCwl(CachedClan cachedClan)
        {
            if (!Clash.IsCwlEnabled() || !DownloadCwl || !cachedClan.DownloadCwl)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWarLeagueGroup group = await dbContext.Groups.Where(g => g.Tag == cachedClan.Tag).FirstAsync().ConfigureAwait(false);

            if (group.IsLocallyExpired() == false || group.IsServerExpired() == false)
                return;

            List<Task> tasks = new List<Task>();

            ApiResponse<ClanWarLeagueGroup>? apiResponse = null;

            try
            {
                apiResponse = await _cocApi.ClansApi.GetClanWarLeagueGroupResponseAsync(cachedClan.Tag).ConfigureAwait(false);
                group.UpdateFrom(apiResponse, _cocApi.TimeToLive(group, apiResponse));
                dbContext.Groups.Update(group);
                tasks.Add(dbContext.SaveChangesAsync());
            }
            catch (ApiException e)
            {
                group.UpdateFrom(e, _cocApi.TimeToLive(group, e));
                dbContext.Groups.Update(group);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                return;
            }

            if (group.Data != null && group.Data.Equals(apiResponse.Data) == false)
                OnClanWarLeagueGroupUpdated(cachedClan.Data, group.Data, apiResponse.Data);

            foreach (var round in apiResponse.Data.Rounds)
                foreach (var warTag in round.WarTags.Where(w => w != "#0"))
                {
                    if (group.Data != null && group.Data.Rounds.Any(r => r.WarTags.Any(w => w == warTag)))
                        break;

                    ApiResponse<ClanWar>? clanWar = await _clansApi.GetClanWarLeagueWarResponseOrDefaultAsync(warTag).ConfigureAwait(false);

                    if (clanWar == null || clanWar.Data.State == ClanWar.StateEnum.NotInWar)
                        continue;

                    if (clanWar.Data.Clan.Tag == cachedClan.Tag || clanWar.Data.Opponent.Tag == cachedClan.Tag)
                    {
                        tasks.Add(InsertNewWarAsync(cachedClan, clanWar, warTag));

                        break;
                    }
                }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateEntireClanAsync(CachedClan cachedClan)
        {
            if (UpdatingClans.TryAdd(cachedClan.Tag, cachedClan) == false)
                return;

            try
            {
                List<Task> tasks = new List<Task>
                {
                    UpdateClanAsync(cachedClan),
                    UpdateCwl(cachedClan),
                    UpdateWarsAsync(cachedClan)
                };

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            finally
            {
                UpdatingClans.TryRemove(cachedClan.Tag, out CachedClan _);
            }
        }

        private async Task UpdateMember(ClanMember member)
        {
            CachedPlayer cachedPlayer = await _cocApi.PlayersCache.AddAsync(member.Tag, false);

            await _cocApi.PlayersCache.UpdatePlayerAsync(cachedPlayer);
        }

        private async Task UpdateMembersAsync(CachedClan cachedClan)
        {
            if (cachedClan.Data == null || !DownloadMembers || !cachedClan.DownloadMembers)
                return;

            List<Task> tasks = new List<Task>();

            foreach (ClanMember member in cachedClan.Data.MemberList)
                tasks.Add(UpdateMember(member));

            await Task.WhenAll().ConfigureAwait(false);
        }

        private async Task UpdateWarAsync(CachedClan cachedClan, CachedWar cachedWar, CachedClanWar cachedClanWar)
        {
            if (cachedWar.State == ClanWar.StateEnum.WarEnded || cachedWar.IsFinal)
                return;

            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                if (cachedWar.Data.IsSameWar(cachedClanWar.Data) && cachedWar.Data.HasWarUpdated(cachedClanWar.Data))
                {
                    OnClanWarUpdated(cachedClan.Data, cachedWar.Data, cachedClanWar.Data);

                    cachedWar.UpdateFrom(cachedClanWar);
                }
                else if (cachedWar.WarTag != null && (cachedWar.IsLocallyExpired() == false || cachedWar.IsServerExpired() == false))
                {
                    try
                    {
                        ApiResponse<ClanWar> apiResponse = await _clansApi.GetClanWarLeagueWarResponseAsync(cachedWar.WarTag);

                        if (cachedWar.Data.HasWarUpdated(apiResponse.Data))
                            OnClanWarUpdated(cachedClan.Data, cachedWar.Data, cachedClanWar.Data);

                        cachedWar.UpdateFrom(cachedClan, apiResponse, _cocApi.TimeToLive(cachedClanWar, apiResponse));
                    }
                    catch (ApiException e)
                    {
                        cachedWar.UpdateFrom(e, _cocApi.TimeToLive(cachedClanWar, e));
                    }

                }

                SendWarAnnouncements(cachedWar);

                dbContext.Wars.Update(cachedWar);

                await dbContext.SaveChangesAsync();
            }
            finally
            {
                UpdatingWar.TryRemove(cachedWar.GetHashCode(), out var _);
            }
        }

        private async Task UpdateWarLog(CachedClan cachedClan)
        {
            if (!DownloadCurrentWars || !cachedClan.DownloadCurrentWar || !cachedClan.Data.IsWarLogPublic)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWarLog log = await dbContext.WarLogs.Where(g => g.Tag == cachedClan.Tag).FirstAsync();

            if (log.IsLocallyExpired() == false || log.IsServerExpired() == false)
                return;

            try
            {
                ApiResponse<ClanWarLog> apiResponse = await _cocApi.ClansApi.GetClanWarLogResponseAsync(cachedClan.Tag);

                if (log.Data != null && log.Data.Equals(apiResponse.Data) == false)
                    OnClanWarLogUpdated(log.Data, apiResponse.Data);

                log.UpdateFrom(apiResponse, _cocApi.TimeToLive(log, apiResponse));
            }
            catch (ApiException e)
            {
                log.UpdateFrom(e, _cocApi.TimeToLive(log, e));
            }

            dbContext.WarLogs.Update(log);

            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateWarsAsync(CachedClan cachedClan)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            List<CachedWar> cachedWars = await dbContext.Wars.Where(w =>
                w.ClanTag == cachedClan.Tag ||
                w.OpponentTag == cachedClan.Tag &&
                w.State != ClanWar.StateEnum.WarEnded && w.IsFinal == false)
                .ToListAsync().ConfigureAwait(false);

            if (cachedWars.Count == 0)
                return;

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync().ConfigureAwait(false);

            List<Task> tasks = new List<Task>();

            foreach (CachedWar cachedWar in cachedWars)
                tasks.Add(UpdateWarAsync(cachedClan, cachedWar, cachedClanWar));

            await Task.WhenAll(tasks);
        }
    }
}