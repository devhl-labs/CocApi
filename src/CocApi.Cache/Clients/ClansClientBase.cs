using System;
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
    public class ClansClientBase : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly PlayersClientBase? _playersCache;

        public ClansClientBase(TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, ClansApi clansApi)
            : base(tokenProvider, cacheConfiguration)
        {
            _clansApi = clansApi;
        }

        public ClansClientBase(TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, ClansApi clansApi, PlayersClientBase playersCache)
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

        internal ConcurrentDictionary<string, byte> UpdatingClan { get; set; } = new ConcurrentDictionary<string, byte>();

        internal ConcurrentDictionary<string, byte> UpdatingClanWar { get; set; } = new ConcurrentDictionary<string, byte>();

        internal ConcurrentDictionary<int, CachedWar> UpdatingWar { get; set; } = new ConcurrentDictionary<int, CachedWar>();

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

        public async Task<ClanWar?> GetActiveClanWarAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            var cache = await dbContext.Wars
                .AsNoTracking()
                .Where(i => (i.ClanTag == formattedTag || i.OpponentTag == formattedTag))
                .OrderByDescending(w => w.PreparationStartTime)
                .ToListAsync(cancellationToken.GetValueOrDefault(_stopRequestedTokenSource.Token))
                .ConfigureAwait(false);

            if (cache.Count == 0)
                return null;

            foreach (var item in cache)
                item.Data.WarType = item.Type;

            return cache.FirstOrDefault(c => c.State == WarState.InWar)?.Data
                ?? cache.FirstOrDefault(c => c.State == WarState.Preparation)?.Data
                ?? cache.First().Data;
        }

        private async Task<CachedClan> GetCacheAsync(string tag, CancellationToken? cancellationToken = default)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => i.Tag == tag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        internal async Task<CachedClan?> GetCacheOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => i.Tag == tag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        private async Task<CachedWar?> GetLeagueWarOrDefaultAsync(string warTag, DateTime season, CancellationToken? cancellationToken = default)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Wars
                .AsNoTracking()
                .Where(w => w.WarTag == warTag && w.Season == season)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Clan> GetClanAsync(string tag, CancellationToken? cancellationToken = default)
        {
            CachedClan result = await GetCacheAsync(tag, cancellationToken).ConfigureAwait(false);

            if (result.Data == null)
                throw new NullReferenceException();

            return result.Data;
        }

        public async Task<Clan?> GetClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            CachedClan? result = await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

            return result?.Data;
        }

        public async Task<List<ClanWar>> GetClanWarsAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            var cache = await dbContext.Wars
                .AsNoTracking()
                .Where(i => i.ClanTag == formattedTag || i.OpponentTag == formattedTag)
                .ToListAsync(cancellationToken.GetValueOrDefault(_stopRequestedTokenSource.Token))
                .ConfigureAwait(false);

            List<ClanWar> clanWars = new List<ClanWar>();

            foreach (var item in cache)
            {
                item.Data.WarType = item.Type;

                clanWars.Add(item.Data);
            }

            return clanWars;
        }

        public async Task<Clan> GetOrFetchClanAsync(string tag, CancellationToken? cancellationToken = default)
        {
            return (await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
                ?? await _clansApi.GetClanAsync(tag, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            return (await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
                ?? await _clansApi.GetClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);
        }

        private int _clanId = 0;

        private int _warId = 0;

        private ActiveWarMonitor _activeWarMonitor;

        private LogMonitor _clanWarLogMonitor;

        private ClanWarMonitor _clanWarMonitor;

        private ClanMonitor _clanMonitor;

        public Task RunAsync(CancellationToken cancellationToken)
        {
            _activeWarMonitor = new ActiveWarMonitor(_tokenProvider, _cacheConfiguration, _clansApi, this);
            _clanWarLogMonitor = new LogMonitor(_tokenProvider, _cacheConfiguration, _clansApi, this);
            _clanWarMonitor = new ClanWarMonitor(_tokenProvider, _cacheConfiguration, _clansApi, this);
            _clanMonitor = new ClanMonitor(_tokenProvider, _cacheConfiguration, _clansApi, this);

            Task.Run(() =>
            {
                _ = _activeWarMonitor.RunAsync(cancellationToken);
                _ = _clanWarLogMonitor.RunAsync(cancellationToken);
                _ = _clanWarMonitor.RunAsync(cancellationToken);
                _ = _clanMonitor.RunAsync(cancellationToken);


                //_ = FetchClanAsync(cancellationToken);
                //_ = FetchClanWarAsync(cancellationToken);
                //try
                //{
                //    if (IsRunning)
                //        return;

                //    IsRunning = true;

                //    _stopRequestedTokenSource = new CancellationTokenSource();

                //    OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                //    while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                //    {
                //        using var scope = _services.CreateScope();

                //        CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                //        List<Task> tasks = new List<Task>();

                //        List<CachedClan> cachedClans = await dbContext.Clans
                //            .Where(c => c.Id > _clanId)
                //            .OrderBy(c => c.Id)
                //            .Take(_cacheConfiguration.ConcurrentUpdates)
                //            .ToListAsync()
                //            .ConfigureAwait(false);

                //        for (int i = 0; i < cachedClans.Count; i++)
                //            tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

                //        List<CachedWar> cachedWars = await dbContext.Wars
                //            .AsNoTracking()
                //            .Where(w => w.Id > _warId && w.State < WarState.WarEnded && w.IsFinal == false)
                //            .OrderBy(w => w.Id)
                //            .Take(_cacheConfiguration.ConcurrentUpdates)
                //            .ToListAsync()
                //            .ConfigureAwait(false);       

                //        for (int i = 0; i < cachedWars.Count; i++)
                //        {
                //            tasks.Add(UpdateClanWar(cachedWars[i].ClanTag));
                //            tasks.Add(UpdateClanWar(cachedWars[i].OpponentTag));
                //        }

                //        if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
                //            _clanId = 0;
                //        else
                //            _clanId = cachedClans.Max(c => c.Id);

                //        if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
                //            _warId = 0;
                //        else
                //            _warId = cachedWars.Max(c => c.Id);

                //        await Task.WhenAll(tasks).ConfigureAwait(false);

                //        await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                //    }

                //    IsRunning = false;
                //}
                //catch (Exception e)
                //{
                //    IsRunning = false;

                //    if (_stopRequestedTokenSource.IsCancellationRequested)
                //        return;

                //    OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                //    if (cancellationToken.IsCancellationRequested == false)
                //        _ = RunAsync(cancellationToken);
                //}
            });

            return Task.CompletedTask;
        }

        private async Task FetchClanAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    List<CachedClan> cachedClans = await dbContext.Clans
                        .Where(c => c.Id > _clanId)
                        .OrderBy(c => c.Id)
                        .Take(_cacheConfiguration.ConcurrentUpdates)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedClans.Count; i++)
                        tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

                    //List<CachedWar> cachedWars = await dbContext.Wars
                    //    .AsNoTracking()
                    //    .Where(w => w.Id > _warId && w.State < WarState.WarEnded && w.IsFinal == false)
                    //    .OrderBy(w => w.Id)
                    //    .Take(_cacheConfiguration.ConcurrentUpdates)
                    //    .ToListAsync()
                    //    .ConfigureAwait(false);

                    //for (int i = 0; i < cachedWars.Count; i++)
                    //{
                    //    tasks.Add(UpdateClanWar(cachedWars[i].ClanTag));
                    //    tasks.Add(UpdateClanWar(cachedWars[i].OpponentTag));
                    //}

                    if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
                        _clanId = 0;
                    else
                        _clanId = cachedClans.Max(c => c.Id);

                    //if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
                    //    _warId = 0;
                    //else
                    //    _warId = cachedWars.Max(c => c.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                IsRunning = false;
            }
            catch (Exception e)
            {
                IsRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        private async Task FetchClanWarAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (IsRunning)
                    return;

                IsRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
                {
                    using var scope = _services.CreateScope();

                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<Task> tasks = new List<Task>();

                    //List<CachedClan> cachedClans = await dbContext.Clans
                    //    .Where(c => c.Id > _clanId)
                    //    .OrderBy(c => c.Id)
                    //    .Take(_cacheConfiguration.ConcurrentUpdates)
                    //    .ToListAsync()
                    //    .ConfigureAwait(false);

                    //for (int i = 0; i < cachedClans.Count; i++)
                    //    tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

                    List<CachedWar> cachedWars = await dbContext.Wars
                        .AsNoTracking()
                        .Where(w => w.Id > _warId && w.State < WarState.WarEnded && w.IsFinal == false)
                        .OrderBy(w => w.Id)
                        .Take(_cacheConfiguration.ConcurrentUpdates)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedWars.Count; i++)
                    {
                        tasks.Add(UpdateClanWar(cachedWars[i].ClanTag));
                        tasks.Add(UpdateClanWar(cachedWars[i].OpponentTag));
                    }

                    //if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
                    //    _clanId = 0;
                    //else
                    //    _clanId = cachedClans.Max(c => c.Id);

                    if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
                        _warId = 0;
                    else
                        _warId = cachedWars.Max(c => c.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                IsRunning = false;
            }
            catch (Exception e)
            {
                IsRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        internal bool HasUpdated(CachedClan stored, CachedClan fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (stored.Data == null || fetched.Data == null)
                return false;

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(Clan stored, Clan fetched)
        {
            return !(stored.BadgeUrls?.Small == fetched.BadgeUrls?.Small
                && stored.ClanLevel == fetched.ClanLevel
                && stored.ClanPoints == fetched.ClanPoints
                && stored.ClanVersusPoints == fetched.ClanVersusPoints
                && stored.Description == fetched.Description
                && stored.IsWarLogPublic == fetched.IsWarLogPublic
                && stored.Location?.Id == fetched.Location?.Id
                && stored.Name == fetched.Name
                && stored.RequiredTrophies == fetched.RequiredTrophies
                && stored.Type == fetched.Type
                && stored.WarFrequency == fetched.WarFrequency
                && stored.WarLeague?.Id == fetched.WarLeague?.Id
                && stored.WarLosses == fetched.WarLosses
                && stored.WarTies == fetched.WarTies
                && stored.WarWins == fetched.WarWins
                && stored.WarWinStreak == fetched.WarWinStreak
                && stored.Labels.Except(fetched.Labels).Count() == 0
                && stored.Members.Except(fetched.Members).Count() == 0
                );
        }

        internal bool HasUpdated(CachedClanWarLeagueGroup stored, CachedClanWarLeagueGroup fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (fetched.Data == null)
                return false;

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        {
            if (stored == null)
                return false;

            if (stored.State != fetched.State)
                return true;

            foreach (ClanWarLeagueRound round in fetched.Rounds)
                foreach (string warTag in round.WarTags)
                    if (stored.Rounds.Any(r => r.WarTags.Any(w => w == warTag)) == false)
                        return true;

            return false;
        }

        internal bool HasUpdated(CachedClanWarLog stored, CachedClanWarLog fetched)
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

            if (stored.Items.Count != fetched.Items.Count)
                return true;

            if (stored.Items.Max(i => i.EndTime) != fetched.Items.Max(i => i.EndTime))
                return true;

            return false;
        }

        internal bool HasUpdated(CachedWar stored, CachedClanWar fetched)
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
            return !(stored.EndTime == fetched.EndTime
                && stored.StartTime == fetched.StartTime
                && stored.State == fetched.State
                && stored.Attacks.Count == fetched.Attacks.Count);
        }

        internal bool HasUpdated(CachedWar stored, CachedWar fetched)
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
            _stopRequestedTokenSource.Cancel();

            List<Task> tasks = new List<Task>
            {
                base.StopAsync(cancellationToken),
                _clanWarLogMonitor.StopAsync(cancellationToken),
                _activeWarMonitor.StopAsync(cancellationToken),
                _clanWarMonitor.StopAsync(cancellationToken),
                _clanMonitor.StopAsync(cancellationToken)
            };

            if (_playersCache != null)
                tasks.Add(_playersCache.StopAsync(cancellationToken));

            await Task.WhenAll(tasks);
        }

        public async Task UpdateAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            if (downloadClan == false && downloadMembers == true)
                throw new ArgumentException("DownloadClan must be true to download members.");

            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClan cachedClan = await dbContext.Clans
                .Where(c => c.Tag == formattedTag)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

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

            await dbContext.SaveChangesAsync();
        }

        internal void OnClanUpdated(Clan stored, Clan fetched)
        {
            Task.Run(() => ClanUpdated?.Invoke(this, new ClanUpdatedEventArgs(stored, fetched)));
        }

        internal void OnClanWarAdded(ClanWar clanWar)
        {
            Task.Run(() => ClanWarAdded?.Invoke(this, new ClanWarEventArgs(clanWar)));
        }

        internal void OnClanWarEndingSoon(ClanWar clanWar)
        {
            Task.Run(() => ClanWarEndingSoon?.Invoke(this, new ClanWarEventArgs(clanWar)));
        }

        internal void OnClanWarEndNotSeen(ClanWar clanWar)
        {
            Task.Run(() => ClanWarEndNotSeen?.Invoke(this, new ClanWarEventArgs(clanWar)));
        }

        internal void OnClanWarEnded(ClanWar clanWar)
        {
            Task.Run(() => ClanWarEnded?.Invoke(this, new ClanWarEventArgs(clanWar)));
        }

        internal void OnClanWarLeagueGroupUpdated(Clan clan, ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        {
            Task.Run(() => ClanWarLeagueGroupUpdated?.Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(clan, stored, fetched)));
        }

        internal void OnClanWarLogUpdated(ClanWarLog? stored, ClanWarLog fetched)
        {
            Task.Run(() => ClanWarLogUpdated?.Invoke(this, new ClanWarLogUpdatedEventArgs(stored, fetched)));
        }

        internal void OnClanWarStartingSoon(ClanWar clanWar)
        {
            Task.Run(() => ClanWarStartingSoon?.Invoke(this, new ClanWarEventArgs(clanWar)));
        }

        internal void OnClanWarUpdated(Clan clan, ClanWar stored, ClanWar fetched)
        {
            Task.Run(() => ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(clan, stored, fetched)));
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

            dbContext.Clans.Add(cachedClan);

            CachedClanWar cachedClanWar = await dbContext.ClanWars
                .Where(w => w.Tag == formattedTag)
                .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            if (cachedClanWar == null)
            {
                cachedClanWar = new CachedClanWar(formattedTag);

                dbContext.ClanWars.Add(cachedClanWar);
            }

            dbContext.Groups.Add(new CachedClanWarLeagueGroup(formattedTag));

            dbContext.WarLogs.Add(new CachedClanWarLog(formattedTag));

            await dbContext.SaveChangesAsync();

            return;
        }

        internal async Task InsertNewWarAsync(CachedWar fetched)
        {
            if (fetched.Data == null)
                throw new ArgumentException("Data should not be null.");

            if (UpdatingWar.TryAdd(fetched.GetHashCode(), fetched) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedWar? exists = await dbContext.Wars
                    .Where(w =>
                        w.PreparationStartTime == fetched.Data.PreparationStartTime &&
                        w.ClanTag == fetched.Data.Clans.First().Value.Tag)
                    .FirstOrDefaultAsync(_stopRequestedTokenSource.Token);

                if (exists != null)
                    return;

                dbContext.Wars.Add(fetched);

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

                OnClanWarAdded(fetched.Data);
            }
            finally
            {
                UpdatingWar.TryRemove(fetched.GetHashCode(), out var _);
            }
        }

        internal bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
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
                cachedWar.Data.AllAttacksAreUsed() == false)
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

            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, this, _clansApi, _stopRequestedTokenSource.Token);

            if (cachedClan.Data != null &&
                fetched.Data != null &&
                HasUpdated(cachedClan, fetched))
                OnClanUpdated(cachedClan.Data, fetched.Data);

            cachedClan.UpdateFrom(fetched);

            tasks.Add(UpdateClanWar(cachedClan));

            tasks.Add(UpdateWarLog(cachedClan));

            tasks.Add(UpdateMembersAsync(cachedClan));

            dbContext.Clans.Update(cachedClan);

            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateClanWar(CachedClan cachedClan)
        {
            if (!DownloadCurrentWars ||
                !cachedClan.DownloadCurrentWar ||
                cachedClan.Data == null ||
                !cachedClan.Data.IsWarLogPublic)
                return;

            await UpdateClanWar(cachedClan.Tag);
        }

        private async Task UpdateClanWar(string clanTag)
        {
            if (UpdatingClanWar.TryAdd(clanTag, new byte()) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedClanWar? cachedClanWar = await dbContext.ClanWars
                    .Where(w => w.Tag == clanTag)
                    .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
                    .ConfigureAwait(false);

                if (cachedClanWar == null)
                {
                    cachedClanWar = new CachedClanWar(clanTag);

                    dbContext.ClanWars.Add(cachedClanWar);
                }

                if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
                    return;

                CachedClanWar fetched = await CachedClanWar
                    .FromCurrentWarResponseAsync(clanTag, this, _clansApi, _stopRequestedTokenSource.Token);

                if (fetched.Data != null && IsNewWar(cachedClanWar, fetched))
                {
                    await InsertNewWarAsync(new CachedWar(fetched));

                    cachedClanWar.Type = fetched.Data.WarType;
                }

                cachedClanWar.UpdateFrom(fetched);

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
            }
            finally
            {
                UpdatingClanWar.TryRemove(clanTag, out _);
            }
        }

        private async Task UpdateCwl(CachedClan cachedClan)
        {
            if (!Clash.IsCwlEnabled() ||
                !DownloadCwl ||
                !cachedClan.DownloadCwl ||
                cachedClan.Data == null ||
                _stopRequestedTokenSource.IsCancellationRequested)
                return;

            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedClanWarLeagueGroup group = await dbContext.Groups
                .Where(g => g.Tag == cachedClan.Tag)
                .FirstAsync(_stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            if (group.IsLocallyExpired() == false || group.IsServerExpired() == false)
                return;

            List<Task> tasks = new List<Task>();

            CachedClanWarLeagueGroup fetched = await CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, this, _clansApi, _stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            if (fetched.Data != null && HasUpdated(group, fetched))
                OnClanWarLeagueGroupUpdated(cachedClan.Data, group.Data, fetched.Data);

            if (fetched.Data != null && fetched.Data.Season.Month == DateTime.UtcNow.Month)
                foreach (var round in fetched.Data.Rounds)
                    foreach (var warTag in round.WarTags.Where(w => w != "#0"))
                    {
                        if (_stopRequestedTokenSource.IsCancellationRequested)
                            return;

                        if (await GetLeagueWarOrDefaultAsync(warTag, fetched.Season, _stopRequestedTokenSource.Token)
                            .ConfigureAwait(false) != null)
                            continue;

                        CachedWar fetchedWar = await CachedWar
                            .FromClanWarLeagueWarResponseAsync(warTag, fetched.Season, this, _clansApi, _stopRequestedTokenSource.Token)
                            .ConfigureAwait(false);

                        if (fetchedWar.Data == null || fetchedWar.Data.State == WarState.NotInWar)
                            continue;

                        if (fetchedWar.Data.Clan.Tag == cachedClan.Tag || fetchedWar.Data.Opponent.Tag == cachedClan.Tag)
                        {
                            tasks.Add(InsertNewWarAsync(fetchedWar));

                            break;
                        }
                    }

            group.UpdateFrom(fetched);

            tasks.Add(dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private async Task UpdateEntireClanAsync(CachedClan cachedClan)
        {
            if (UpdatingClan.TryAdd(cachedClan.Tag, new byte()) == false)
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
                UpdatingClan.TryRemove(cachedClan.Tag, out _);
            }
        }

        private async Task UpdateMember(ClanMember member, PlayersClientBase playersCache)
        {
            CachedPlayer cachedPlayer = await playersCache.AddAsync(member.Tag, false);

            await playersCache.UpdatePlayerAsync(cachedPlayer);
        }

        private async Task UpdateMembersAsync(CachedClan cachedClan)
        {
            if (cachedClan.Data == null || !DownloadMembers || !cachedClan.DownloadMembers || _playersCache == null)
                return;

            if (_stopRequestedTokenSource.IsCancellationRequested)
                return;

            List<Task> tasks = new List<Task>();

            foreach (ClanMember member in cachedClan.Data.Members)
                tasks.Add(UpdateMember(member, _playersCache));

            await Task.WhenAll(tasks);
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
                    CachedWar fetched = await CachedWar.FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, this, _clansApi, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                    if (fetched.Data != null
                        && IsSameWar(cachedWar.Data, fetched.Data)
                        && HasUpdated(cachedWar, fetched))
                        OnClanWarUpdated(cachedClan.Data, cachedWar.Data, fetched.Data);

                    cachedWar.UpdateFrom(fetched);
                }
                else if (cachedWar.WarTag == null && cachedClanWar.StatusCode == HttpStatusCode.Forbidden && cachedWar.EndTime < DateTime.UtcNow)
                {
                    string enemyTag = cachedWar.ClanTags.First(t => t != cachedClan.Tag);

                    CachedClanWar? enemy = await dbContext.ClanWars
                        .Where(w => w.Tag == enemyTag)
                        .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    if (enemy == null || enemy.IsServerExpired())
                    {
                        CachedClanWar fetchedEnemy = await CachedClanWar.FromCurrentWarResponseAsync(enemyTag, this, _clansApi, _stopRequestedTokenSource.Token);

                        if (fetchedEnemy.Data != null
                            && (fetchedEnemy.Data.State == WarState.NotInWar
                                || IsSameWar(cachedWar.Data, fetchedEnemy.Data) == false))
                            cachedWar.IsFinal = true;

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

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
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

            CachedClanWarLog log = await dbContext.WarLogs
                .Where(g => g.Tag == cachedClan.Tag)
                .FirstAsync(_stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            if (log.IsLocallyExpired() == false || log.IsServerExpired() == false)
                return;

            CachedClanWarLog fetched = await CachedClanWarLog
                .FromClanWarLogResponseAsync(cachedClan.Tag, this, _clansApi, _stopRequestedTokenSource.Token);

            if (fetched.Data != null && HasUpdated(log, fetched))
                OnClanWarLogUpdated(log.Data, fetched.Data);

            log.UpdateFrom(fetched);

            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
        }

        private async Task UpdateWarsAsync(CachedClan cachedClan)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            List<CachedWar> cachedWars = await dbContext.Wars
                .Where(w =>
                    (w.ClanTag == cachedClan.Tag || w.OpponentTag == cachedClan.Tag)
                    && w.State != WarState.WarEnded
                    && w.IsFinal == false)
                .ToListAsync(_stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            if (cachedWars.Count == 0)
                return;

            CachedClanWar cachedClanWar = await dbContext.ClanWars
                .Where(w => w.Tag == cachedClan.Tag)
                .FirstAsync(_stopRequestedTokenSource.Token)
                .ConfigureAwait(false);

            List<Task> tasks = new List<Task>();

            foreach (CachedWar cachedWar in cachedWars)
                tasks.Add(UpdateWarAsync(cachedClan, cachedWar, cachedClanWar));

            await Task.WhenAll(tasks);
        }
    }
}

































































































//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Threading;
//using System.Threading.Tasks;
//using CocApi.Api;
//using CocApi.Cache.Models;
//using CocApi.Client;
//using CocApi.Model;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;

//namespace CocApi.Cache
//{
//    public class ClansClientBase : ClientBase
//    {
//        private readonly ClansApi _clansApi;
//        private readonly PlayersClientBase? _playersCache;

//        public ClansClientBase(TokenProvider tokenProvider, ClientConfigurationBase cacheConfiguration, ClansApi clansApi)
//            : base(tokenProvider, cacheConfiguration)
//        {
//            _clansApi = clansApi;
//        }

//        public ClansClientBase(TokenProvider tokenProvider, ClientConfigurationBase cacheConfiguration, ClansApi clansApi, PlayersClientBase playersCache)
//            : this(tokenProvider, cacheConfiguration, clansApi)
//        {
//            _playersCache = playersCache;

//            DownloadMembers = true;
//        }

//        public event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
//        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarAdded;
//        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEndingSoon;
//        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEndNotSeen;
//        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarEnded;
//        public event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
//        public event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;
//        public event AsyncEventHandler<ClanWarEventArgs>? ClanWarStartingSoon;
//        public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

//        public bool DownloadCurrentWars { get; set; } = true;
//        public bool DownloadCwl { get; set; } = true;
//        public bool DownloadMembers { get; set; } = false;

//        private ConcurrentDictionary<string, CachedClan> UpdatingClan { get; set; } = new ConcurrentDictionary<string, CachedClan>();

//        private ConcurrentDictionary<string, CachedClan?> UpdatingClanWar { get; set; } = new ConcurrentDictionary<string, CachedClan?>();

//        private ConcurrentDictionary<int, CachedWar> UpdatingWar { get; set; } = new ConcurrentDictionary<int, CachedWar>();

//        public async Task AddAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
//        {
//            if (downloadClan == false && downloadMembers == true)
//                throw new Exception("DownloadClan must be true to download members.");

//            string formattedTag = Clash.FormatTag(tag);

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            CachedClan cachedClan = await dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

//            if (cachedClan != null)
//                return;

//            await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers);

//            return;
//        }

//        public async Task<ClanWar?> GetActiveClanWarAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            string formattedTag = Clash.FormatTag(tag);

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            var cache = await dbContext.Wars
//                .AsNoTracking()
//                .Where(i => (i.ClanTag == formattedTag || i.OpponentTag == formattedTag))
//                .OrderByDescending(w => w.PreparationStartTime)
//                .ToListAsync(cancellationToken.GetValueOrDefault(_stopRequestedTokenSource.Token))
//                .ConfigureAwait(false);

//            if (cache.Count == 0)
//                return null;

//            foreach (var item in cache)
//                item.Data.WarType = item.Type;

//            return cache.FirstOrDefault(c => c.State == WarState.InWar)?.Data
//                ?? cache.FirstOrDefault(c => c.State == WarState.Preparation)?.Data
//                ?? cache.First().Data;
//        }

//        private async Task<CachedClan> GetCacheAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            return await dbContext.Clans
//                .AsNoTracking()
//                .Where(i => i.Tag == tag)
//                .FirstAsync(cancellationToken.GetValueOrDefault())
//                .ConfigureAwait(false);
//        }

//        public async Task<CachedClan?> GetCacheOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            return await dbContext.Clans
//                .AsNoTracking()
//                .Where(i => i.Tag == tag)
//                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
//                .ConfigureAwait(false);
//        }

//        private async Task<CachedWar?> GetLeagueWarOrDefaultAsync(string warTag, DateTime season, CancellationToken? cancellationToken = default)
//        {
//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            return await dbContext.Wars
//                .AsNoTracking()
//                .Where(w => w.WarTag == warTag && w.Season == season)
//                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
//                .ConfigureAwait(false);
//        }

//        public async Task<Clan> GetClanAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            CachedClan result = await GetCacheAsync(tag, cancellationToken).ConfigureAwait(false);

//            if (result.Data == null)
//                throw new NullReferenceException();

//            return result.Data;
//        }

//        public async Task<Clan?> GetClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            CachedClan? result = await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

//            return result?.Data;
//        }

//        public async Task<List<ClanWar>> GetClanWarsAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            string formattedTag = Clash.FormatTag(tag);

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            var cache = await dbContext.Wars
//                .AsNoTracking()
//                .Where(i => i.ClanTag == formattedTag || i.OpponentTag == formattedTag)
//                .ToListAsync(cancellationToken.GetValueOrDefault(_stopRequestedTokenSource.Token))
//                .ConfigureAwait(false);

//            List<ClanWar> clanWars = new List<ClanWar>();

//            foreach (var item in cache)
//            {
//                item.Data.WarType = item.Type;

//                clanWars.Add(item.Data);
//            }

//            return clanWars;
//        }

//        public async Task<Clan> GetOrFetchClanAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            return (await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
//                ?? await _clansApi.GetClanAsync(tag, cancellationToken).ConfigureAwait(false);
//        }

//        public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
//        {
//            return (await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
//                ?? await _clansApi.GetClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);
//        }

//        private int _clanId = 0;

//        private int _warId = 0;

//        public Task RunAsync(CancellationToken cancellationToken)
//        {
//            Task.Run(() =>
//            {
//                _ = FetchClanAsync(cancellationToken);
//                _ = FetchClanWarAsync(cancellationToken);
//                //try
//                //{
//                //    if (IsRunning)
//                //        return;

//                //    IsRunning = true;

//                //    _stopRequestedTokenSource = new CancellationTokenSource();

//                //    OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

//                //    while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
//                //    {
//                //        using var scope = _services.CreateScope();

//                //        CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//                //        List<Task> tasks = new List<Task>();

//                //        List<CachedClan> cachedClans = await dbContext.Clans
//                //            .Where(c => c.Id > _clanId)
//                //            .OrderBy(c => c.Id)
//                //            .Take(_cacheConfiguration.ConcurrentUpdates)
//                //            .ToListAsync()
//                //            .ConfigureAwait(false);

//                //        for (int i = 0; i < cachedClans.Count; i++)
//                //            tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

//                //        List<CachedWar> cachedWars = await dbContext.Wars
//                //            .AsNoTracking()
//                //            .Where(w => w.Id > _warId && w.State < WarState.WarEnded && w.IsFinal == false)
//                //            .OrderBy(w => w.Id)
//                //            .Take(_cacheConfiguration.ConcurrentUpdates)
//                //            .ToListAsync()
//                //            .ConfigureAwait(false);       

//                //        for (int i = 0; i < cachedWars.Count; i++)
//                //        {
//                //            tasks.Add(UpdateClanWar(cachedWars[i].ClanTag));
//                //            tasks.Add(UpdateClanWar(cachedWars[i].OpponentTag));
//                //        }

//                //        if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
//                //            _clanId = 0;
//                //        else
//                //            _clanId = cachedClans.Max(c => c.Id);

//                //        if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
//                //            _warId = 0;
//                //        else
//                //            _warId = cachedWars.Max(c => c.Id);

//                //        await Task.WhenAll(tasks).ConfigureAwait(false);

//                //        await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
//                //    }

//                //    IsRunning = false;
//                //}
//                //catch (Exception e)
//                //{
//                //    IsRunning = false;

//                //    if (_stopRequestedTokenSource.IsCancellationRequested)
//                //        return;

//                //    OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

//                //    if (cancellationToken.IsCancellationRequested == false)
//                //        _ = RunAsync(cancellationToken);
//                //}
//            });

//            return Task.CompletedTask;
//        }

//        private async Task FetchClanAsync(CancellationToken cancellationToken)
//        {
//            try
//            {
//                if (IsRunning)
//                    return;

//                IsRunning = true;

//                _stopRequestedTokenSource = new CancellationTokenSource();

//                OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

//                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
//                {
//                    using var scope = _services.CreateScope();

//                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//                    List<Task> tasks = new List<Task>();

//                    List<CachedClan> cachedClans = await dbContext.Clans
//                        .Where(c => c.Id > _clanId)
//                        .OrderBy(c => c.Id)
//                        .Take(_cacheConfiguration.ConcurrentUpdates)
//                        .ToListAsync()
//                        .ConfigureAwait(false);

//                    for (int i = 0; i < cachedClans.Count; i++)
//                        tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

//                    //List<CachedWar> cachedWars = await dbContext.Wars
//                    //    .AsNoTracking()
//                    //    .Where(w => w.Id > _warId && w.State < WarState.WarEnded && w.IsFinal == false)
//                    //    .OrderBy(w => w.Id)
//                    //    .Take(_cacheConfiguration.ConcurrentUpdates)
//                    //    .ToListAsync()
//                    //    .ConfigureAwait(false);

//                    //for (int i = 0; i < cachedWars.Count; i++)
//                    //{
//                    //    tasks.Add(UpdateClanWar(cachedWars[i].ClanTag));
//                    //    tasks.Add(UpdateClanWar(cachedWars[i].OpponentTag));
//                    //}

//                    if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
//                        _clanId = 0;
//                    else
//                        _clanId = cachedClans.Max(c => c.Id);

//                    //if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
//                    //    _warId = 0;
//                    //else
//                    //    _warId = cachedWars.Max(c => c.Id);

//                    await Task.WhenAll(tasks).ConfigureAwait(false);

//                    await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
//                }

//                IsRunning = false;
//            }
//            catch (Exception e)
//            {
//                IsRunning = false;

//                if (_stopRequestedTokenSource.IsCancellationRequested)
//                    return;

//                OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

//                if (cancellationToken.IsCancellationRequested == false)
//                    _ = RunAsync(cancellationToken);
//            }
//        }

//        private async Task FetchClanWarAsync(CancellationToken cancellationToken)
//        {
//            try
//            {
//                if (IsRunning)
//                    return;

//                IsRunning = true;

//                _stopRequestedTokenSource = new CancellationTokenSource();

//                OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

//                while (_stopRequestedTokenSource.IsCancellationRequested == false && cancellationToken.IsCancellationRequested == false)
//                {
//                    using var scope = _services.CreateScope();

//                    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//                    List<Task> tasks = new List<Task>();

//                    //List<CachedClan> cachedClans = await dbContext.Clans
//                    //    .Where(c => c.Id > _clanId)
//                    //    .OrderBy(c => c.Id)
//                    //    .Take(_cacheConfiguration.ConcurrentUpdates)
//                    //    .ToListAsync()
//                    //    .ConfigureAwait(false);

//                    //for (int i = 0; i < cachedClans.Count; i++)
//                    //    tasks.Add(UpdateEntireClanAsync(cachedClans[i]));

//                    List<CachedWar> cachedWars = await dbContext.Wars
//                        .AsNoTracking()
//                        .Where(w => w.Id > _warId && w.State < WarState.WarEnded && w.IsFinal == false)
//                        .OrderBy(w => w.Id)
//                        .Take(_cacheConfiguration.ConcurrentUpdates)
//                        .ToListAsync()
//                        .ConfigureAwait(false);

//                    for (int i = 0; i < cachedWars.Count; i++)
//                    {
//                        tasks.Add(UpdateClanWar(cachedWars[i].ClanTag));
//                        tasks.Add(UpdateClanWar(cachedWars[i].OpponentTag));
//                    }

//                    //if (cachedClans.Count < _cacheConfiguration.ConcurrentUpdates)
//                    //    _clanId = 0;
//                    //else
//                    //    _clanId = cachedClans.Max(c => c.Id);

//                    if (cachedWars.Count < _cacheConfiguration.ConcurrentUpdates)
//                        _warId = 0;
//                    else
//                        _warId = cachedWars.Max(c => c.Id);

//                    await Task.WhenAll(tasks).ConfigureAwait(false);

//                    await Task.Delay(_cacheConfiguration.DelayBetweenUpdates, _stopRequestedTokenSource.Token).ConfigureAwait(false);
//                }

//                IsRunning = false;
//            }
//            catch (Exception e)
//            {
//                IsRunning = false;

//                if (_stopRequestedTokenSource.IsCancellationRequested)
//                    return;

//                OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

//                if (cancellationToken.IsCancellationRequested == false)
//                    _ = RunAsync(cancellationToken);
//            }
//        }

//        private bool HasUpdated(CachedClan stored, CachedClan fetched)
//        {
//            if (stored.ServerExpiration > fetched.ServerExpiration)
//                return false;

//            if (stored.Data == null || fetched.Data == null)
//                return false;

//            return HasUpdated(stored.Data, fetched.Data);
//        }

//        protected virtual bool HasUpdated(Clan stored, Clan fetched)
//        {
//            return !(stored.BadgeUrls?.Small == fetched.BadgeUrls?.Small
//                && stored.ClanLevel == fetched.ClanLevel
//                && stored.ClanPoints == fetched.ClanPoints
//                && stored.ClanVersusPoints == fetched.ClanVersusPoints
//                && stored.Description == fetched.Description
//                && stored.IsWarLogPublic == fetched.IsWarLogPublic
//                && stored.Location?.Id == fetched.Location?.Id
//                && stored.Name == fetched.Name
//                && stored.RequiredTrophies == fetched.RequiredTrophies
//                && stored.Type == fetched.Type
//                && stored.WarFrequency == fetched.WarFrequency
//                && stored.WarLeague?.Id == fetched.WarLeague?.Id
//                && stored.WarLosses == fetched.WarLosses
//                && stored.WarTies == fetched.WarTies
//                && stored.WarWins == fetched.WarWins
//                && stored.WarWinStreak == fetched.WarWinStreak
//                && stored.Labels.Except(fetched.Labels).Count() == 0
//                && stored.Members.Except(fetched.Members).Count() == 0
//                );
//        }

//        private bool HasUpdated(CachedClanWarLeagueGroup stored, CachedClanWarLeagueGroup fetched)
//        {
//            if (stored.ServerExpiration > fetched.ServerExpiration)
//                return false;

//            if (fetched.Data == null)
//                return false;

//            return HasUpdated(stored.Data, fetched.Data);
//        }

//        protected virtual bool HasUpdated(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
//        {
//            if (stored == null)
//                return false;

//            if (stored.State != fetched.State)
//                return true;

//            foreach (ClanWarLeagueRound round in fetched.Rounds)
//                foreach (string warTag in round.WarTags)
//                    if (stored.Rounds.Any(r => r.WarTags.Any(w => w == warTag)) == false)
//                        return true;

//            return false;
//        }

//        private bool HasUpdated(CachedClanWarLog stored, CachedClanWarLog fetched)
//        {
//            if (stored.ServerExpiration > fetched.ServerExpiration)
//                return false;

//            if (fetched.Data == null)
//                return false;

//            return HasUpdated(stored.Data, fetched.Data);
//        }

//        protected virtual bool HasUpdated(ClanWarLog? stored, ClanWarLog fetched)
//        {
//            if (stored == null)
//                return false;

//            if (stored.Items.Count != fetched.Items.Count)
//                return true;

//            if (stored.Items.Max(i => i.EndTime) != fetched.Items.Max(i => i.EndTime))
//                return true;

//            return false;
//        }

//        private bool HasUpdated(CachedWar stored, CachedClanWar fetched)
//        {
//            if (stored.ServerExpiration > fetched.ServerExpiration)
//                return false;

//            if (stored.Data == null
//                || fetched.Data == null
//                || IsSameWar(stored.Data, fetched.Data) == false)
//                throw new ArgumentException();

//            return HasUpdated(stored.Data, fetched.Data);
//        }

//        protected virtual bool HasUpdated(ClanWar stored, ClanWar fetched)
//        {
//            return !(stored.EndTime == fetched.EndTime
//                && stored.StartTime == fetched.StartTime
//                && stored.State == fetched.State
//                && stored.Attacks.Count == fetched.Attacks.Count);
//        }

//        private bool HasUpdated(CachedWar stored, CachedWar fetched)
//        {
//            if (ReferenceEquals(stored, fetched))
//                return false;

//            if (stored.ServerExpiration > fetched.ServerExpiration)
//                return false;

//            if (stored.Data == null
//                || fetched.Data == null
//                || IsSameWar(stored.Data, fetched.Data) == false)
//                throw new ArgumentException();

//            return HasUpdated(stored.Data, fetched.Data);
//        }

//        private bool IsSameWar(ClanWar? stored, ClanWar fetched)
//        {
//            if (ReferenceEquals(stored, fetched))
//                return true;

//            if (stored == null)
//                return true;

//            if (stored.PreparationStartTime != fetched.PreparationStartTime)
//                return false;

//            if (stored.Clan.Tag == fetched.Clan.Tag)
//                return true;

//            if (stored.Clan.Tag == fetched.Opponent.Tag)
//                return true;

//            return false;
//        }

//        public virtual TimeSpan ClanTimeToLive(ApiResponse<Clan> apiResponse)
//            => TimeSpan.FromSeconds(0);

//        public virtual TimeSpan ClanTimeToLive(Exception exception)
//            => TimeSpan.FromMinutes(0);

//        public virtual TimeSpan ClanWarLogTimeToLive(ApiResponse<ClanWarLog> apiResponse)
//            => TimeSpan.FromSeconds(0);

//        public virtual TimeSpan ClanWarLogTimeToLive(Exception exception)
//            => TimeSpan.FromMinutes(2);

//        public virtual TimeSpan ClanWarLeagueGroupTimeToLive(ApiResponse<ClanWarLeagueGroup> apiResponse)
//        {
//            if (apiResponse.Data.State == GroupState.Ended)
//                return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).Subtract(new TimeSpan(0, 0, 0, 0, 1)) - DateTime.UtcNow;

//            return TimeSpan.FromMinutes(20);
//        }

//        public virtual TimeSpan ClanWarLeagueGroupTimeToLive(Exception exception)
//            => TimeSpan.FromMinutes(20);

//        public virtual TimeSpan ClanWarTimeToLive(Exception exception)
//        {
//            if (exception is ApiException apiException)
//                if (apiException.ErrorCode == (int)HttpStatusCode.Forbidden)
//                    return TimeSpan.FromMinutes(2);

//            return TimeSpan.FromSeconds(0);
//        }

//        public virtual TimeSpan ClanWarTimeToLive(ApiResponse<ClanWar> apiResponse)
//        {
//            if (apiResponse.Data.State == WarState.Preparation)
//                return apiResponse.Data.StartTime - DateTime.UtcNow;

//            return TimeSpan.FromSeconds(0);
//        }

//        public new async Task StopAsync(CancellationToken cancellationToken)
//        {
//            _stopRequestedTokenSource.Cancel();

//            List<Task> tasks = new List<Task>
//            {
//                base.StopAsync(cancellationToken)
//            };

//            if (_playersCache != null)
//                tasks.Add(_playersCache.StopAsync(cancellationToken));

//            await Task.WhenAll(tasks);
//        }

//        public async Task UpdateAsync(string tag, bool downloadClan = true, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
//        {
//            if (downloadClan == false && downloadMembers == true)
//                throw new ArgumentException("DownloadClan must be true to download members.");

//            string formattedTag = Clash.FormatTag(tag);

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            CachedClan cachedClan = await dbContext.Clans
//                .Where(c => c.Tag == formattedTag)
//                .FirstOrDefaultAsync()
//                .ConfigureAwait(false);

//            if (cachedClan == null)
//            {
//                await InsertCachedClanAsync(formattedTag, downloadClan, downloadWars, downloadCwl, downloadMembers);

//                return;
//            }

//            cachedClan.Tag = formattedTag;
//            cachedClan.Download = downloadClan;
//            cachedClan.DownloadCurrentWar = downloadWars;
//            cachedClan.DownloadCwl = downloadCwl;
//            cachedClan.DownloadMembers = downloadMembers;

//            await dbContext.SaveChangesAsync();
//        }

//        internal void OnClanUpdated(Clan stored, Clan fetched)
//        {
//            Task.Run(() => ClanUpdated?.Invoke(this, new ClanUpdatedEventArgs(stored, fetched)));
//        }

//        internal void OnClanWarAdded(ClanWar clanWar)
//        {
//            Task.Run(() => ClanWarAdded?.Invoke(this, new ClanWarEventArgs(clanWar)));
//        }

//        internal void OnClanWarEndingSoon(ClanWar clanWar)
//        {
//            Task.Run(() => ClanWarEndingSoon?.Invoke(this, new ClanWarEventArgs(clanWar)));
//        }

//        internal void OnClanWarEndNotSeen(ClanWar clanWar)
//        {
//            Task.Run(() => ClanWarEndNotSeen?.Invoke(this, new ClanWarEventArgs(clanWar)));
//        }

//        internal void OnClanWarEnded(ClanWar clanWar)
//        {
//            Task.Run(() => ClanWarEnded?.Invoke(this, new ClanWarEventArgs(clanWar)));
//        }

//        internal void OnClanWarLeagueGroupUpdated(Clan clan, ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
//        {
//            Task.Run(() => ClanWarLeagueGroupUpdated?.Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(clan, stored, fetched)));
//        }

//        internal void OnClanWarLogUpdated(ClanWarLog? stored, ClanWarLog fetched)
//        {
//            Task.Run(() => ClanWarLogUpdated?.Invoke(this, new ClanWarLogUpdatedEventArgs(stored, fetched)));
//        }

//        internal void OnClanWarStartingSoon(ClanWar clanWar)
//        {
//            Task.Run(() => ClanWarStartingSoon?.Invoke(this, new ClanWarEventArgs(clanWar)));
//        }

//        internal void OnClanWarUpdated(Clan clan, ClanWar stored, ClanWar fetched)
//        {
//            Task.Run(() => ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(clan, stored, fetched)));
//        }

//        private async Task InsertCachedClanAsync(string formattedTag, bool downloadClan, bool downloadWars, bool downloadCwl, bool downloadMembers)
//        {
//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            CachedClan cachedClan = new CachedClan(formattedTag)
//            {
//                Download = downloadClan,
//                DownloadCurrentWar = downloadWars,
//                DownloadCwl = downloadCwl,
//                DownloadMembers = downloadMembers
//            };

//            dbContext.Clans.Add(cachedClan);

//            CachedClanWar cachedClanWar = await dbContext.ClanWars
//                .Where(w => w.Tag == formattedTag)
//                .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
//                .ConfigureAwait(false);

//            if (cachedClanWar == null)
//            {
//                cachedClanWar = new CachedClanWar(formattedTag);

//                dbContext.ClanWars.Add(cachedClanWar);
//            }

//            dbContext.Groups.Add(new CachedClanWarLeagueGroup(formattedTag));

//            dbContext.WarLogs.Add(new CachedClanWarLog(formattedTag));

//            await dbContext.SaveChangesAsync();

//            return;
//        }

//        private async Task InsertNewWarAsync(CachedWar fetched)
//        {
//            if (fetched.Data == null)
//                throw new ArgumentException("Data should not be null.");

//            if (UpdatingWar.TryAdd(fetched.GetHashCode(), fetched) == false)
//                return;

//            try
//            {
//                using var scope = _services.CreateScope();

//                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//                CachedWar? exists = await dbContext.Wars
//                    .Where(w => 
//                        w.PreparationStartTime == fetched.Data.PreparationStartTime && 
//                        w.ClanTag == fetched.Data.Clans.First().Value.Tag)
//                    .FirstOrDefaultAsync(_stopRequestedTokenSource.Token);

//                if (exists != null)
//                    return;

//                dbContext.Wars.Add(fetched);

//                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

//                OnClanWarAdded(fetched.Data);
//            }
//            finally
//            {
//                UpdatingWar.TryRemove(fetched.GetHashCode(), out var _);
//            }
//        }

//        private bool IsNewWar(CachedClanWar stored, CachedClanWar fetched)
//        {
//            if (fetched.Data == null || fetched.Data.State == WarState.NotInWar)
//                return false;

//            if (stored.Data == null)
//                return true;

//            if (stored.Data.PreparationStartTime == fetched.Data.PreparationStartTime)
//                return false;

//            return true;
//        }

//        private void SendWarAnnouncements(CachedWar cachedWar)
//        {
//            if (cachedWar.Data == null)
//                return;

//            if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
//                DateTime.UtcNow > cachedWar.Data.StartTime.AddHours(-1) &&
//                DateTime.UtcNow < cachedWar.Data.StartTime)
//            {
//                cachedWar.Announcements |= Announcements.WarStartingSoon;
//                OnClanWarStartingSoon(cachedWar.Data);
//            }

//            if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
//                DateTime.UtcNow > cachedWar.Data.EndTime.AddHours(-1) &&
//                DateTime.UtcNow < cachedWar.Data.EndTime)
//            {
//                cachedWar.Announcements |= Announcements.WarEndingSoon;
//                OnClanWarEndingSoon(cachedWar.Data);
//            }

//            if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
//                cachedWar.State != WarState.WarEnded &&
//                cachedWar.IsFinal == true &&
//                DateTime.UtcNow > cachedWar.EndTime &&
//                DateTime.UtcNow.Day == cachedWar.EndTime.Day &&
//                cachedWar.Data.AllAttacksAreUsed() == false)
//            {
//                cachedWar.Announcements |= Announcements.WarEndNotSeen;
//                OnClanWarEndNotSeen(cachedWar.Data);
//            }

//            if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
//                cachedWar.EndTime < DateTime.UtcNow &&
//                cachedWar.EndTime.Day == DateTime.UtcNow.Day)
//            {
//                cachedWar.Announcements |= Announcements.WarEnded;
//                OnClanWarEnded(cachedWar.Data);
//            }
//        }

//        private async Task UpdateClanAsync(CachedClan cachedClan)
//        {
//            if (cachedClan.Download == false || 
//                cachedClan.IsServerExpired() == false || 
//                cachedClan.IsLocallyExpired() == false)
//                return;

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            List<Task> tasks = new List<Task>();

//            CachedClan fetched = await CachedClan.FromClanResponseAsync(cachedClan.Tag, this, _clansApi, _stopRequestedTokenSource.Token);

//            if (cachedClan.Data != null && 
//                fetched.Data != null && 
//                HasUpdated(cachedClan, fetched))
//                OnClanUpdated(cachedClan.Data, fetched.Data);

//            cachedClan.UpdateFrom(fetched);

//            tasks.Add(UpdateClanWar(cachedClan));

//            tasks.Add(UpdateWarLog(cachedClan));

//            tasks.Add(UpdateMembersAsync(cachedClan));

//            dbContext.Clans.Update(cachedClan);

//            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);

//            await Task.WhenAll(tasks).ConfigureAwait(false);
//        }

//        private async Task UpdateClanWar(CachedClan cachedClan)
//        {
//            if (!DownloadCurrentWars || 
//                !cachedClan.DownloadCurrentWar || 
//                cachedClan.Data == null || 
//                !cachedClan.Data.IsWarLogPublic)
//                return;

//            await UpdateClanWar(cachedClan.Tag);
//        }

//        private async Task UpdateClanWar(string clanTag)
//        {
//            if (UpdatingClanWar.TryAdd(clanTag, null) == false)
//                return;

//            try
//            {
//                using var scope = _services.CreateScope();

//                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//                CachedClanWar? cachedClanWar = await dbContext.ClanWars
//                    .Where(w => w.Tag == clanTag)
//                    .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
//                    .ConfigureAwait(false);

//                if (cachedClanWar == null)
//                {
//                    cachedClanWar = new CachedClanWar(clanTag);

//                    dbContext.ClanWars.Add(cachedClanWar);
//                }

//                if (cachedClanWar.IsLocallyExpired() == false || cachedClanWar.IsServerExpired() == false)
//                    return; 

//                CachedClanWar fetched = await CachedClanWar
//                    .FromCurrentWarResponseAsync(clanTag, this, _clansApi, _stopRequestedTokenSource.Token);

//                if (fetched.Data != null && IsNewWar(cachedClanWar, fetched))
//                {
//                    await InsertNewWarAsync(new CachedWar(fetched));

//                    cachedClanWar.Type = fetched.Data.WarType;
//                }

//                cachedClanWar.UpdateFrom(fetched);

//                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
//            }
//            finally
//            {
//                UpdatingClanWar.TryRemove(clanTag, out _);
//            }
//        }

//        private async Task UpdateCwl(CachedClan cachedClan)
//        {
//            if (!Clash.IsCwlEnabled() ||
//                !DownloadCwl ||
//                !cachedClan.DownloadCwl ||
//                cachedClan.Data == null ||
//                _stopRequestedTokenSource.IsCancellationRequested)
//                return;

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            CachedClanWarLeagueGroup group = await dbContext.Groups
//                .Where(g => g.Tag == cachedClan.Tag)
//                .FirstAsync(_stopRequestedTokenSource.Token)
//                .ConfigureAwait(false);

//            if (group.IsLocallyExpired() == false || group.IsServerExpired() == false)                
//                return;

//            List<Task> tasks = new List<Task>();

//            CachedClanWarLeagueGroup fetched = await CachedClanWarLeagueGroup
//                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, this, _clansApi, _stopRequestedTokenSource.Token)
//                .ConfigureAwait(false);

//            if (fetched.Data != null && HasUpdated(group, fetched))
//                OnClanWarLeagueGroupUpdated(cachedClan.Data, group.Data, fetched.Data);

//            if (fetched.Data != null && fetched.Data.Season.Month == DateTime.UtcNow.Month)                
//                foreach (var round in fetched.Data.Rounds)                    
//                    foreach (var warTag in round.WarTags.Where(w => w != "#0"))
//                    {
//                        if (_stopRequestedTokenSource.IsCancellationRequested)
//                            return;                            

//                        if (await GetLeagueWarOrDefaultAsync(warTag, fetched.Season, _stopRequestedTokenSource.Token)
//                            .ConfigureAwait(false) != null)
//                            continue;

//                        CachedWar fetchedWar = await CachedWar
//                            .FromClanWarLeagueWarResponseAsync(warTag, fetched.Season, this, _clansApi, _stopRequestedTokenSource.Token)
//                            .ConfigureAwait(false);

//                        if (fetchedWar.Data == null || fetchedWar.Data.State == WarState.NotInWar)
//                            continue;

//                        if (fetchedWar.Data.Clan.Tag == cachedClan.Tag || fetchedWar.Data.Opponent.Tag == cachedClan.Tag)
//                        {
//                            tasks.Add(InsertNewWarAsync(fetchedWar));

//                            break;
//                        }
//                    }

//            group.UpdateFrom(fetched);

//            tasks.Add(dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token));

//            await Task.WhenAll(tasks).ConfigureAwait(false);
//        }

//        private async Task UpdateEntireClanAsync(CachedClan cachedClan)
//        {
//            if (UpdatingClan.TryAdd(cachedClan.Tag, cachedClan) == false)
//                return;

//            try
//            {
//                List<Task> tasks = new List<Task>
//                {
//                    UpdateClanAsync(cachedClan),
//                    UpdateCwl(cachedClan),
//                    UpdateWarsAsync(cachedClan)
//                };

//                await Task.WhenAll(tasks).ConfigureAwait(false);
//            }
//            finally
//            {
//                UpdatingClan.TryRemove(cachedClan.Tag, out CachedClan _);
//            }
//        }

//        private async Task UpdateMember(ClanMember member, PlayersClientBase playersCache)
//        {
//            CachedPlayer cachedPlayer = await playersCache.AddAsync(member.Tag, false);

//            await playersCache.UpdatePlayerAsync(cachedPlayer);
//        }

//        private async Task UpdateMembersAsync(CachedClan cachedClan)
//        {
//            if (cachedClan.Data == null || !DownloadMembers || !cachedClan.DownloadMembers || _playersCache == null)
//                return;

//            if (_stopRequestedTokenSource.IsCancellationRequested)
//                return;

//            List<Task> tasks = new List<Task>();

//            foreach (ClanMember member in cachedClan.Data.Members)
//                tasks.Add(UpdateMember(member, _playersCache));

//            await Task.WhenAll(tasks);
//        }

//        private async Task UpdateWarAsync(CachedClan cachedClan, CachedWar cachedWar, CachedClanWar cachedClanWar)
//        {
//            if (cachedClan.Data == null 
//                || cachedWar.Data == null 
//                || cachedWar.State == WarState.WarEnded 
//                || cachedWar.IsFinal)
//                return;

//            if (UpdatingWar.TryAdd(cachedWar.GetHashCode(), cachedWar) == false)
//                return;

//            try
//            {
//                using var scope = _services.CreateScope();

//                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//                if (cachedClanWar.Data != null 
//                    && IsSameWar(cachedWar.Data, cachedClanWar.Data) 
//                    && HasUpdated(cachedWar, cachedClanWar))
//                {
//                    OnClanWarUpdated(cachedClan.Data, cachedWar.Data, cachedClanWar.Data);

//                    cachedWar.UpdateFrom(cachedClanWar);
//                }
//                else if (cachedWar.WarTag != null && !(cachedWar.IsLocallyExpired() == false || cachedWar.IsServerExpired() == false))
//                {
//                    CachedWar fetched = await CachedWar.FromClanWarLeagueWarResponseAsync(cachedWar.WarTag, cachedWar.Season.Value, this, _clansApi, _stopRequestedTokenSource.Token).ConfigureAwait(false);

//                    if (fetched.Data != null 
//                        && IsSameWar(cachedWar.Data, fetched.Data) 
//                        && HasUpdated(cachedWar, fetched))                    
//                        OnClanWarUpdated(cachedClan.Data, cachedWar.Data, fetched.Data);                    

//                    cachedWar.UpdateFrom(fetched);
//                }
//                else if (cachedWar.WarTag == null && cachedClanWar.StatusCode == HttpStatusCode.Forbidden && cachedWar.EndTime < DateTime.UtcNow)
//                {
//                    string enemyTag = cachedWar.ClanTags.First(t => t != cachedClan.Tag);

//                    CachedClanWar? enemy = await dbContext.ClanWars
//                        .Where(w => w.Tag == enemyTag)
//                        .FirstOrDefaultAsync(_stopRequestedTokenSource.Token)
//                        .ConfigureAwait(false);

//                    if (enemy == null || enemy.IsServerExpired())
//                    {
//                        CachedClanWar fetchedEnemy = await CachedClanWar.FromCurrentWarResponseAsync(enemyTag, this, _clansApi, _stopRequestedTokenSource.Token);

//                        if (fetchedEnemy.Data != null
//                            && (fetchedEnemy.Data.State == WarState.NotInWar
//                                || IsSameWar(cachedWar.Data, fetchedEnemy.Data) == false))
//                            cachedWar.IsFinal = true;

//                        if (fetchedEnemy.Data != null
//                            && IsSameWar(cachedWar.Data, fetchedEnemy.Data)
//                            && HasUpdated(cachedWar, fetchedEnemy))
//                        {
//                            OnClanWarUpdated(cachedClan.Data, cachedWar.Data, fetchedEnemy.Data);

//                            cachedWar.UpdateFrom(fetchedEnemy);

//                            dbContext.ClanWars.Update(fetchedEnemy);
//                        }
//                    }
//                }

//                SendWarAnnouncements(cachedWar);

//                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
//            }
//            finally
//            {
//                UpdatingWar.TryRemove(cachedWar.GetHashCode(), out var _);
//            }
//        }

//        private async Task UpdateWarLog(CachedClan cachedClan)
//        {
//            if (!DownloadCurrentWars || 
//                !cachedClan.DownloadCurrentWar || 
//                cachedClan.Data == null ||
//                !cachedClan.Data.IsWarLogPublic)
//                return;

//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            CachedClanWarLog log = await dbContext.WarLogs
//                .Where(g => g.Tag == cachedClan.Tag)
//                .FirstAsync(_stopRequestedTokenSource.Token)
//                .ConfigureAwait(false);

//            if (log.IsLocallyExpired() == false || log.IsServerExpired() == false)
//                return;

//            CachedClanWarLog fetched = await CachedClanWarLog
//                .FromClanWarLogResponseAsync(cachedClan.Tag, this, _clansApi, _stopRequestedTokenSource.Token);

//            if (fetched.Data != null && HasUpdated(log, fetched))
//                OnClanWarLogUpdated(log.Data, fetched.Data);

//            log.UpdateFrom(fetched);

//            await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
//        }

//        private async Task UpdateWarsAsync(CachedClan cachedClan)
//        {
//            using var scope = _services.CreateScope();

//            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

//            List<CachedWar> cachedWars = await dbContext.Wars
//                .Where(w =>
//                    (w.ClanTag == cachedClan.Tag || w.OpponentTag == cachedClan.Tag)
//                    && w.State != WarState.WarEnded
//                    && w.IsFinal == false)
//                .ToListAsync(_stopRequestedTokenSource.Token)
//                .ConfigureAwait(false);

//            if (cachedWars.Count == 0)
//                return;

//            CachedClanWar cachedClanWar = await dbContext.ClanWars
//                .Where(w => w.Tag == cachedClan.Tag)
//                .FirstAsync(_stopRequestedTokenSource.Token)
//                .ConfigureAwait(false);

//            List<Task> tasks = new List<Task>();

//            foreach (CachedWar cachedWar in cachedWars)
//                tasks.Add(UpdateWarAsync(cachedClan, cachedWar, cachedClanWar));

//            await Task.WhenAll(tasks);
//        }
//    }
//}