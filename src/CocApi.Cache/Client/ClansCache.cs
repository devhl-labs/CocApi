using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        private readonly CocApiClient _cocApi;
        private readonly CocApiConfiguration _cocApiConfiguration;
        private readonly IServiceProvider _services;

        public ClansCache(CocApiClient cocApi, CocApiConfiguration cocApiConfiguration, IServiceProvider serviceProvider, ClansApi clansApi/*, CachedContext dbContext*/)
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
            if (apiResponse == null || apiResponse.Data.PreparationStartTime == DateTime.MinValue)
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

            ApiResponse<Clan> apiResponse = await _cocApi.ClansApi.GetClanWithHttpInfoAsync(cachedClan.Tag);

            if (cachedClan.ServerExpiration >= apiResponse.ServerExpiration)
                return;

            if (cachedClan.Data != null && _cocApi.IsEqual(cachedClan.Data, apiResponse.Data) == false)
                OnClanUpdated(cachedClan.Data, apiResponse.Data);

            cachedClan.UpdateFrom(apiResponse, _cocApiConfiguration.ClansTimeToLive);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            dbContext.Clans.Update(cachedClan);

            await dbContext.SaveChangesAsync();

            List<Task> tasks = new List<Task>();

            if (DownloadCurrentWars && cachedClan.DownloadCurrentWar && apiResponse.Data.IsWarLogPublic)
            {
                tasks.Add(UpdateClanWar(cachedClan));

                tasks.Add(UpdateWarLog(cachedClan));
            }

            if (DownloadMembers && cachedClan.DownloadMembers)
                tasks.Add(UpdateMembersAsync(cachedClan));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateClanWar(CachedClan cachedClan)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync();

            if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                return;

            ApiResponse<ClanWar>? apiResponse = await _cocApi.ClansApi.GetCurrentWarWithHttpInfoOrDefaultAsync(cachedClan.Tag);

            if (IsNewWar(cachedClanWar, apiResponse))
                await InsertNewWarAsync(cachedClan, apiResponse);

            cachedClanWar.UpdateFrom(apiResponse, _cocApiConfiguration.CurrentWarTimeToLive); //todo better ttl

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

            ApiResponse<ClanWarLeagueGroup>? apiResponse = null;

            try
            {
                apiResponse = await _cocApi.ClansApi.GetClanWarLeagueGroupWithHttpInfoAsync(cachedClan.Tag).ConfigureAwait(false);
            }
            catch (ApiException e)
            {
                group.UpdateFrom(e, _cocApiConfiguration.LeagueGroupNotFoundTimeToLive);
            }
            catch (Exception)
            {
                group.UpdateFrom(apiResponse, _cocApiConfiguration.LeagueGroupNotFoundTimeToLive);
            }

            if (apiResponse == null)
            {
                dbContext.Groups.Update(group);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
                return;
            }

            if (group.Data != null && group.Data.Equals(apiResponse.Data) == false)
                OnClanWarLeagueGroupUpdated(cachedClan.Data, group.Data, apiResponse.Data);

            List<Task> tasks = new List<Task>();

            foreach (var round in apiResponse.Data.Rounds)
                foreach (var warTag in round.WarTags)
                {
                    if (warTag == "#0")
                        break;

                    if (group.Data != null && group.Data.Rounds.Any(r => r.WarTags.Any(w => w == warTag)))
                        break;

                    ApiResponse<ClanWar>? clanWar = await _clansApi.GetClanWarLeagueWarWithHttpInfoOrDefaultAsync(warTag).ConfigureAwait(false);

                    if (clanWar == null || clanWar.Data.State == ClanWar.StateEnum.NotInWar)
                        continue;

                    if (clanWar.Data.Clan.Tag == cachedClan.Tag || clanWar.Data.Opponent.Tag == cachedClan.Tag)
                    {
                        tasks.Add(InsertNewWarAsync(cachedClan, clanWar, warTag));

                        break;
                    }
                }

            group.UpdateFrom(apiResponse, _cocApiConfiguration.LeagueGroupTimeToLive);

            dbContext.Groups.Update(group);

            tasks.Add(dbContext.SaveChangesAsync());

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
            if (cachedClan.Data == null)
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
                    ApiResponse<ClanWar> apiResponse = await _clansApi.GetClanWarLeagueWarWithHttpInfoAsync(cachedWar.WarTag);

                    if (cachedWar.Data.HasWarUpdated(apiResponse.Data))
                        OnClanWarUpdated(cachedClan.Data, cachedWar.Data, cachedClanWar.Data);

                    cachedWar.UpdateFrom(cachedClan, apiResponse, _cocApiConfiguration.CurrentWarTimeToLive);
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
                    OnClanWarLogUpdated(log.Data, apiResponse.Data);

                log.UpdateFromResponse(apiResponse, _cocApiConfiguration.WarLogTimeToLive);
            }
            catch (ApiException e)
            {
                log.UpdateFromResponse(e, _cocApiConfiguration.PrivateWarLogTimeToLive);
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