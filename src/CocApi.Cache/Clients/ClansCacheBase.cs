﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class ClansCacheBase : CacheBase
    {
        private readonly ClansApi _clansApi;
        private readonly PlayersCacheBase? _playersCache;

        public ClansCacheBase(TokenProvider tokenProvider, CacheConfiguration cacheConfiguration, ClansApi clansApi)
            : base(tokenProvider, cacheConfiguration)
        {
            _clansApi = clansApi;
        }

        public ClansCacheBase(TokenProvider tokenProvider, CacheConfiguration cacheConfiguration, ClansApi clansApi, PlayersCacheBase playersCache)
            : this(tokenProvider, cacheConfiguration, clansApi)
        {
            _playersCache = playersCache;

            DownloadMembers = true;
        }

        public event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarAdded;
        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEndingSoon;
        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEndNotSeen;
        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEnded;
        public event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
        public event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;
        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarStartingSoon;
        public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        public bool DownloadCurrentWars { get; set; } = true;
        public bool DownloadCwl { get; set; } = true;
        public bool DownloadMembers { get; set; } = false;

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

            return cache.FirstOrDefault(c => c.State == WarState.InWar)?.Data
                ?? cache.FirstOrDefault(c => c.State == WarState.Preparation)?.Data
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

        private async Task<CachedWar?> GetLeagueWarOrDefaultAsync(string warTag, DateTime season)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Wars.Where(w => w.WarTag == warTag && w.Season == season).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<Clan> GetClanAsync(string tag)
        {
            CachedClan result = await GetCacheAsync(tag).ConfigureAwait(false);

            if (result.Data == null)
                throw new NullReferenceException();

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

        private int _clanId = 0;

        public Task RunAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (IsRunning)
                        return;

                    IsRunning = true;

                    StopRequested = false;

                    OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    while (!StopRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        List<Task> tasks = new List<Task>();

                        List<CachedClan> cachedClans = await dbContext.Clans.Where(v =>
                            v.Id > _clanId).OrderBy(v => v.Id).Take(_cacheConfiguration.ConcurrentUpdates).ToListAsync().ConfigureAwait(false);

                        for (int i = 0; i < cachedClans.Count; i++)
                            tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

                        if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
                            _clanId = 0;
                        else
                            _clanId = cachedClans.Max(c => c.Id);

                        await Task.WhenAll(tasks).ConfigureAwait(false);

                        await Task.Delay(_cacheConfiguration.DelayBetweenUpdates).ConfigureAwait(false);
                    }

                    IsRunning = false;
                }
                catch (Exception e)
                {
                    OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                    IsRunning = false;

                    if (cancellationToken.IsCancellationRequested == false)
                        _ = RunAsync(cancellationToken);
                }
            });

            return Task.CompletedTask;
        }

        private bool HasUpdated(CachedClan stored, CachedClan fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (stored.Data == null || fetched.Data == null)
                return false;

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(Clan stored, Clan fetched)
            => !stored.Equals(fetched);

        private bool HasUpdated(CachedClanWarLeagueGroup stored, CachedClanWarLeagueGroup fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (fetched.Data == null)
                return false;

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
            => !fetched.Equals(stored);

        private bool HasUpdated(CachedClanWarLog stored, CachedClanWarLog fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (fetched.Data == null)
                return false;

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(ClanWarLog? stored, ClanWarLog fetched)
        {
            if (stored == null)
                return false;

            return !fetched.Equals(stored);
        }

        private bool HasUpdated(CachedWar stored, CachedClanWar fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (stored.Data == null
                || fetched.Data == null
                || IsSameWar(stored.Data, fetched.Data) == false)
                throw new ArgumentException();

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(ClanWar stored, ClanWar fetched)
        {
            if (stored.Clans.First().Value.Attacks != fetched.Clans.First().Value.Attacks)
                return true;

            if (stored.Clans.Skip(1).First().Value.Attacks != fetched.Clans.Skip(1).First().Value.Attacks)
                return true;

            if (stored.EndTime != fetched.EndTime)
                return true;

            if (stored.StartTime != fetched.StartTime)
                return true;

            if (stored.State != fetched.State)
                return true;

            return false;
        }

        private bool HasUpdated(CachedWar stored, CachedWar fetched)
        {
            if (ReferenceEquals(stored, fetched))
                return false;

            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (stored.Data == null
                || fetched.Data == null
                || IsSameWar(stored.Data, fetched.Data) == false)
                throw new ArgumentException();

            return HasUpdated(stored.Data, fetched.Data);
        }

        private bool IsSameWar(ClanWar? stored, ClanWar fetched)
        {
            if (ReferenceEquals(stored, fetched))
                return true;

            if (stored == null)
                return true;

            if (stored.PreparationStartTime != fetched.PreparationStartTime)
                return false;

            if (stored.Clan.Tag == fetched.Clan.Tag)
                return true;

            if (stored.Clan.Tag == fetched.Opponent.Tag)
                return true;

            return false;
        }

        public virtual TimeSpan ClanTimeToLive(ApiResponse<Clan> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan ClanTimeToLive(Exception exception)
            => TimeSpan.FromMinutes(0);

        public virtual TimeSpan ClanWarLogTimeToLive(ApiResponse<ClanWarLog> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan ClanWarLogTimeToLive(Exception exception)
            => TimeSpan.FromMinutes(2);

        public virtual TimeSpan ClanWarLeagueGroupTimeToLive(ApiResponse<ClanWarLeagueGroup> apiResponse)
        {
            if (apiResponse.Data.State == GroupState.Ended)
                return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)) - DateTime.UtcNow;

            return TimeSpan.FromMinutes(20);
        }

        public virtual TimeSpan ClanWarLeagueGroupTimeToLive(Exception exception)
            => TimeSpan.FromMinutes(20);

        public virtual TimeSpan ClanWarTimeToLive(Exception exception)
        {
            if (exception is ApiException apiException)
                if (apiException.ErrorCode == (int)HttpStatusCode.Forbidden)
                    return TimeSpan.FromMinutes(2);

            return TimeSpan.FromSeconds(0);
        }

        public virtual TimeSpan ClanWarTimeToLive(ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse.Data.State == WarState.Preparation)
                return apiResponse.Data.StartTime - DateTime.UtcNow;

            return TimeSpan.FromSeconds(0);
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_playersCache != null)
                await _playersCache.StopAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
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
        {
            try
            {
                _ = ClanUpdated?.Invoke(this, new ClanUpdatedEventArgs(stored, fetched));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarAdded(ClanWar clanWar)
        {
            try
            {
                _ = ClanWarAdded?.Invoke(this, new ClanWarEventArgs(clanWar));
            }
            catch (Exception)
            {
            }
        }


        internal void OnClanWarEndingSoon(ClanWar clanWar)
        {
            try
            {
                _ = ClanWarEndingSoon?.Invoke(this, new ClanWarEventArgs(clanWar));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarEndNotSeen(ClanWar clanWar)
        {
            try
            {
                _ = (ClanWarEndNotSeen?.Invoke(this, new ClanWarEventArgs(clanWar)));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarEnded(ClanWar clanWar)
        {
            try
            {
                _ = ClanWarEnded?.Invoke(this, new ClanWarEventArgs(clanWar));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarLeagueGroupUpdated(Clan clan, ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        {
            try
            {
                _ = ClanWarLeagueGroupUpdated?.Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(clan, stored, fetched));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarLogUpdated(ClanWarLog? stored, ClanWarLog fetched)
        {
            try
            {
                _ = ClanWarLogUpdated?.Invoke(this, new ClanWarLogUpdatedEventArgs(stored, fetched));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarStartingSoon(ClanWar clanWar)
        {
            try
            {
                _ = ClanWarStartingSoon?.Invoke(this, new ClanWarEventArgs(clanWar));
            }
            catch (Exception)
            {
            }
        }

        internal void OnClanWarUpdated(Clan clan, ClanWar stored, ClanWar fetched)
        {
            try
            {
                _ = ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(clan, stored, fetched));
            }
            catch (Exception)
            {
            }
        }


        private async Task InsertCachedClanAsync(string formattedTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadMembers)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClan cachedClan = new CachedClan(formattedTag)
            {
                Download = downloadClan,
                DownloadCurrentWar = downloadWars,
                DownloadCwl = downloadCwl,
                DownloadMembers = downloadMembers
            };

            dbContext.Clans.Update(cachedClan);

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            cachedClanWar ??= new CachedClanWar(formattedTag);

            dbContext.ClanWars.Update(cachedClanWar);

            CachedClanWarLeagueGroup group = new CachedClanWarLeagueGroup(formattedTag);

            dbContext.Groups.Update(group);

            CachedClanWarLog log = new CachedClanWarLog(formattedTag);

            dbContext.WarLogs.Update(log);

            await dbContext.SaveChangesAsync();

            return;
        }

        private async Task InsertNewWarAsync(CachedWar fetched)
        {
            if (fetched.Data == null)
                throw new ArgumentException("Data should not be null.");

            if (UpdatingWar.TryAdd(fetched.GetHashCode(), fetched) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedWar? exists = await dbContext.Wars.Where(
                    w => w.PreparationStartTime == fetched.Data.PreparationStartTime &&
                        w.ClanTag == fetched.Data.Clans.First().Value.Tag).FirstOrDefaultAsync();

                if (exists != null)
                    return;

                dbContext.Wars.Update(fetched);

                await dbContext.SaveChangesAsync();

                OnClanWarAdded(fetched.Data);
            }
            finally
            {
                UpdatingWar.TryRemove(fetched.GetHashCode(), out var _);
            }
        }

        private bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
        {
            if (fetched.Data == null || fetched.Data.State == WarState.NotInWar)
                return false;

            if (stored.Data == null)
                return true;

            if (stored.Data.PreparationStartTime == fetched.Data.PreparationStartTime)
                return false;

            return true;
        }

        private void SendWarAnnouncements(CachedWar cachedWar)
        {
            if (cachedWar.Data == null)
                return;

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
                cachedWar.State != WarState.WarEnded &&
                cachedWar.IsFinal == true &&
                DateTime.UtcNow > cachedWar.EndTime &&
                DateTime.UtcNow.Day == cachedWar.EndTime.Day &&
                cachedWar.AllAttacksUsed() == false)
            {
                cachedWar.Announcements |= Announcements.WarEndNotSeen;
                OnClanWarEndNotSeen(cachedWar.Data);
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                cachedWar.EndTime < DateTime.UtcNow &&
                cachedWar.EndTime.Day == DateTime.UtcNow.Day)
            {
                cachedWar.Announcements |= Announcements.WarEnded;
                OnClanWarEnded(cachedWar.Data);
            }
        }

        private async Task UpdateClanAsync(CachedClan cachedClan)
        {
            if (cachedClan.Download == false || 
                cachedClan.IsServerExpired() == false || 
                cachedClan.IsLocallyExpired() == false)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            List<Task> tasks = new List<Task>();

            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, this, _clansApi);

            if (cachedClan.Data != null && 
                fetched.Data != null && 
                HasUpdated(cachedClan, fetched))
                OnClanUpdated(cachedClan.Data, fetched.Data);

            cachedClan.UpdateFrom(fetched);

            tasks.Add(UpdateClanWar(cachedClan));

            tasks.Add(UpdateWarLog(cachedClan));

            tasks.Add(UpdateMembersAsync(cachedClan));

            dbContext.Clans.Update(cachedClan);

            await dbContext.SaveChangesAsync();

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateClanWar(CachedClan cachedClan)
        {
            if (!DownloadCurrentWars || 
                !cachedClan.DownloadCurrentWar || 
                cachedClan.Data == null || 
                !cachedClan.Data.IsWarLogPublic)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWar cachedClanWar = await dbContext.ClanWars.Where(w => w.Tag == cachedClan.Tag).FirstAsync();

            if (cachedClanWar.IsLocallyExpired() == false || 
                cachedClanWar.IsServerExpired() == false)
                return;

            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, this, _clansApi);

            if (fetched.Data != null && IsNewWar(cachedClanWar, fetched))
            {
                await InsertNewWarAsync(new CachedWar(fetched));

                cachedClanWar.Type = fetched.Data.Type;
            }

            cachedClanWar.UpdateFrom(fetched);

            dbContext.ClanWars.Update(cachedClanWar);

            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateCwl(CachedClan cachedClan)
        {
            if (!Clash.IsCwlEnabled() || 
                !DownloadCwl || 
                !cachedClan.DownloadCwl || 
                cachedClan.Data == null)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWarLeagueGroup group = await dbContext.Groups.Where(g => g.Tag == cachedClan.Tag).FirstAsync().ConfigureAwait(false);

            if (group.IsLocallyExpired() == false || 
                group.IsServerExpired() == false)
                return;

            List<Task> tasks = new List<Task>();

            CachedClanWarLeagueGroup fetched = await CachedClanWarLeagueGroup.FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, this, _clansApi).ConfigureAwait(false);

            if (fetched.Data != null && HasUpdated(group, fetched))
                OnClanWarLeagueGroupUpdated(cachedClan.Data, group.Data, fetched.Data);

            if (fetched.Data != null && fetched.Data.Season.Month == DateTime.UtcNow.Month)
                foreach (var round in fetched.Data.Rounds)
                    foreach (var warTag in round.WarTags.Where(w => w != "#0"))
                    {
                        if (await GetLeagueWarOrDefaultAsync(warTag, fetched.Season).ConfigureAwait(false) != null)
                            continue;

                        CachedWar fetchedWar = await CachedWar.FromClanWarLeagueWarResponseAsync(warTag, fetched.Season, this, _clansApi).ConfigureAwait(false);

                        if (fetchedWar.Data == null || fetchedWar.Data.State == WarState.NotInWar)
                            continue;

                        if (fetchedWar.Data.Clan.Tag == cachedClan.Tag || fetchedWar.Data.Opponent.Tag == cachedClan.Tag)
                        {
                            tasks.Add(InsertNewWarAsync(fetchedWar));

                            break;
                        }
                    }

            group.UpdateFrom(fetched);

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
            if (cachedClan.Data == null 
                || cachedWar.Data == null 
                || cachedWar.State == WarState.WarEnded 
                || cachedWar.IsFinal)
                return;

            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                if (cachedClanWar.Data != null 
                    && IsSameWar(cachedWar.Data, cachedClanWar.Data) 
                    && HasUpdated(cachedWar, cachedClanWar))
                {
                    OnClanWarUpdated(cachedClan.Data, cachedWar.Data, cachedClanWar.Data);

                    cachedWar.UpdateFrom(cachedClanWar);
                }
                else if (cachedWar.WarTag != null && !(cachedWar.IsLocallyExpired() == false || cachedWar.IsServerExpired() == false))
                {
                    CachedWar fetched = await CachedWar.FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, this, _clansApi).ConfigureAwait(false);

                    if (fetched.Data != null 
                        && IsSameWar(cachedWar.Data, fetched.Data) 
                        && HasUpdated(cachedWar, fetched))
                    {
                        OnClanWarUpdated(cachedClan.Data, cachedWar.Data, fetched.Data);
                    }

                    cachedWar.UpdateFrom(fetched);
                }
                else if(cachedWar.WarTag == null && cachedClanWar.StatusCode == HttpStatusCode.Forbidden && cachedWar.EndTime < DateTime.UtcNow)
                {
                    string enemyTag = cachedWar.ClanTags.First(t => t != cachedClan.Tag);

                    CachedClanWar? enemy = await dbContext.ClanWars.Where(w => w.Tag == enemyTag).FirstOrDefaultAsync();

                    if (enemy == null || enemy.IsServerExpired())
                    {
                        CachedClanWar fetchedEnemy = await CachedClanWar.FromCurrentWarResponseAsync(enemyTag, this, _clansApi);

                        if (fetchedEnemy.Data != null 
                            && (fetchedEnemy.Data.State == WarState.NotInWar
                            || IsSameWar(cachedWar.Data, fetchedEnemy.Data) == false))
                        {
                            cachedWar.IsFinal = true;
                        }

                        if (fetchedEnemy.Data != null 
                            && IsSameWar(cachedWar.Data, fetchedEnemy.Data) 
                            && HasUpdated(cachedWar, fetchedEnemy))
                        {
                            OnClanWarUpdated(cachedClan.Data, cachedWar.Data, fetchedEnemy.Data);

                            cachedWar.UpdateFrom(fetchedEnemy);

                            dbContext.ClanWars.Update(fetchedEnemy);                            
                        }
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
            if (!DownloadCurrentWars || 
                !cachedClan.DownloadCurrentWar || 
                cachedClan.Data == null ||
                !cachedClan.Data.IsWarLogPublic)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWarLog log = await dbContext.WarLogs.Where(g => g.Tag == cachedClan.Tag).FirstAsync();

            if (log.IsLocallyExpired() == false || log.IsServerExpired() == false)
                return;

            CachedClanWarLog fetched = await CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, this, _clansApi);

            if (fetched.Data != null && HasUpdated(log, fetched))
                OnClanWarLogUpdated(log.Data, fetched.Data);

            log.UpdateFrom(fetched);

            dbContext.WarLogs.Update(log);

            await dbContext.SaveChangesAsync();
        }

        private async Task UpdateWarsAsync(CachedClan cachedClan)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            List<CachedWar> cachedWars = await dbContext.Wars.Where(w =>
                (w.ClanTag == cachedClan.Tag || w.OpponentTag == cachedClan.Tag) 
                && w.State != WarState.WarEnded 
                && w.IsFinal == false)
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