using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class ClansCacheBase
    {
        private readonly ClansApi _clansApi;
        private readonly CacheConfiguration _cacheConfiguration;
        private readonly TokenProvider _tokenProvider;
        private readonly IServiceProvider _services;
        private readonly PlayersCacheBase? _playersCache;

        internal void OnLog(object sender, LogEventArgs log) => Log?.Invoke(sender, log);
        public event LogEventHandler? Log;

        public ClansCacheBase(CacheConfiguration cacheConfiguration, ClansApi clansApi)
        {
            _cacheConfiguration = cacheConfiguration;
            _tokenProvider = clansApi.TokenProvider;
            _services = BuildServiceProvider(cacheConfiguration.ConnectionString);
            _clansApi = clansApi;
        }

        private IServiceProvider BuildServiceProvider(string connectionString)
        {
            return new ServiceCollection()
                .AddDbContext<CachedContext>(o =>
                    o.UseSqlite(connectionString))
                .BuildServiceProvider();
        }

        public ClansCacheBase(CacheConfiguration cacheConfiguration, ClansApi clansApi, PlayersCacheBase playersCache)
            : this(cacheConfiguration, clansApi)
        {
            _playersCache = playersCache;

            DownloadMembers = true;
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

        public bool DownloadMembers { get; set; } = false;

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

        public async Task RunAsync()
        {
            //Task.Run(async () =>
            //{
                try
                {
                    if (IsUpdatingClans)
                        return;

                    IsUpdatingClans = true;

                    StopUpdatingClansRequested = false;

                    OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                    int clanId = 0;

                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    while (!StopUpdatingClansRequested)
                    {
                        List<Task> tasks = new List<Task>();

                        List<CachedClan> cachedClans = await dbContext.Clans.Where(v =>
                            v.Id > clanId).OrderBy(v => v.Id).Take(_cacheConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedClans.Count; i++)
                            tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
                            clanId = 0;
                        else
                            clanId = cachedClans.Max(c => c.Id);

                        await Task.Delay(_cacheConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                    }

                    IsUpdatingClans = false;
                }
                catch (Exception e)
                {
                    OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                    IsUpdatingClans = false;

                    //todo what to do when it breaks;
                    //Start();
                }
            //});
        }

        public virtual bool HasUpdated(Clan stored, Clan fetched) => stored.Equals(fetched);

        public virtual TimeSpan TimeToLive(CachedClan cachedClan, ApiResponse<Clan> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan TimeToLive(CachedClan cachedClan, ApiException apiException)
            => TimeSpan.FromMinutes(0);

        public virtual TimeSpan TimeToLive(CachedClanWarLog cachedClanWarLog, ApiResponse<ClanWarLog> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan TimeToLive(CachedClanWarLog cachedClanWarLog, ApiException apiException)
            => TimeSpan.FromMinutes(2);

        public virtual TimeSpan TimeToLive(CachedClanWarLeagueGroup cachedGroup, ApiResponse<ClanWarLeagueGroup> apiResponse)
        {
            if (apiResponse.Data.State == ClanWarLeagueGroup.StateEnum.Ended)
                return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)) - DateTime.UtcNow;

            return TimeSpan.FromMinutes(20);
        }

        public virtual TimeSpan TimeToLive(CachedClanWarLeagueGroup cachedGroup, ApiException apiException)
            => TimeSpan.FromMinutes(20);

        public virtual TimeSpan TimeToLive(CachedClanWar cachedClanWar, ApiResponse<ClanWar> apiResponse)
            => TimeToLive(apiResponse);

        public virtual TimeSpan TimeToLive(CachedClanWar cachedClanWar, ApiException apiException)
        {
            if (apiException.ErrorCode == (int)HttpStatusCode.Forbidden)
                return TimeSpan.FromMinutes(2);

            return TimeSpan.FromSeconds(0);
        }

        public virtual TimeSpan TimeToLive(ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse.Data.State == ClanWar.StateEnum.Preparation)
                return apiResponse.Data.StartTime - DateTime.UtcNow;

            return TimeSpan.FromSeconds(0);
        }

        public async Task StopAsync()
        {
            StopUpdatingClansRequested = true;

            OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));

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
            CachedWar cachedWar = new CachedWar(cachedClan, apiResponse, TimeToLive(apiResponse), warTag);

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
                ApiResponse<Clan> apiResponse = await _clansApi.GetClanResponseAsync(cachedClan.Tag);

                if (cachedClan.Data != null && HasUpdated(cachedClan.Data, apiResponse.Data) == false)
                    OnClanUpdated(cachedClan.Data, apiResponse.Data);

                cachedClan.UpdateFrom(apiResponse, TimeToLive(cachedClan, apiResponse));

                tasks.Add(UpdateClanWar(cachedClan));

                tasks.Add(UpdateWarLog(cachedClan));

                tasks.Add(UpdateMembersAsync(cachedClan));
            }
            catch (ApiException e)
            {
                cachedClan.UpdateFrom(e, TimeToLive(cachedClan, e));
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
                apiResponse = await _clansApi.GetCurrentWarResponseAsync(cachedClan.Tag);

                if (IsNewWar(cachedClanWar, apiResponse))
                {
                    await InsertNewWarAsync(cachedClan, apiResponse);

                    cachedClanWar.Type = apiResponse.Data.Type;
                }

                cachedClanWar.UpdateFrom(apiResponse, TimeToLive(cachedClanWar, apiResponse));
            }
            catch (ApiException e)
            {
                cachedClanWar.UpdateFrom(e, TimeToLive(cachedClanWar, e));
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
                apiResponse = await _clansApi.GetClanWarLeagueGroupResponseAsync(cachedClan.Tag).ConfigureAwait(false);
                group.UpdateFrom(apiResponse, TimeToLive(group, apiResponse));
                dbContext.Groups.Update(group);
                tasks.Add(dbContext.SaveChangesAsync());
            }
            catch (ApiException e)
            {
                group.UpdateFrom(e, TimeToLive(group, e));
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

        private async Task UpdateMember(ClanMember member, PlayersCacheBase playersCache)
        {
            CachedPlayer cachedPlayer = await playersCache.AddAsync(member.Tag, false);

            await playersCache.UpdatePlayerAsync(cachedPlayer);
        }

        private async Task UpdateMembersAsync(CachedClan cachedClan)
        {
            if (cachedClan.Data == null || !DownloadMembers || !cachedClan.DownloadMembers)
                return;

            if (_playersCache == null)
                throw new ArgumentException("You must provide a PlayersCache object to update members.");

            List<Task> tasks = new List<Task>();

            foreach (ClanMember member in cachedClan.Data.MemberList)
                tasks.Add(UpdateMember(member, _playersCache));

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

                        cachedWar.UpdateFrom(cachedClan, apiResponse, TimeToLive(cachedClanWar, apiResponse));
                    }
                    catch (ApiException e)
                    {
                        cachedWar.UpdateFrom(e, TimeToLive(cachedClanWar, e));
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
                ApiResponse<ClanWarLog> apiResponse = await _clansApi.GetClanWarLogResponseAsync(cachedClan.Tag);

                if (log.Data != null && log.Data.Equals(apiResponse.Data) == false)
                    OnClanWarLogUpdated(log.Data, apiResponse.Data);

                log.UpdateFrom(apiResponse, TimeToLive(log, apiResponse));
            }
            catch (ApiException e)
            {
                log.UpdateFrom(e, TimeToLive(log, e));
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