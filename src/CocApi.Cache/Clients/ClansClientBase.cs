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
        private readonly PlayersClientBase? _playersClient;

        internal void OnLog(object sender, LogEventArgs log) => Task.Run(() => Log?.Invoke(sender, log));

        public event LogEventHandler? Log;

        public ClansClientBase(TokenProvider tokenQueue, ClientConfiguration configuration, ClansApi clansApi)
            : base(tokenQueue, configuration)
        {
            _clansApi = clansApi;

            //_activeWarMonitor = new ActiveWarMonitor(TokenQueue, Configuration, _clansApi, this);
            _clanWarLogMonitor = new WarLogMonitor(TokenProvider, Configuration, _clansApi, this);
            _clanWarMonitor = new ClanWarMonitor(TokenProvider, Configuration, _clansApi, this);
            _cwlMonitor = new CwlMonitor(TokenProvider, Configuration, _clansApi, this);
            _warMonitor = new WarMonitor(TokenProvider, Configuration, _clansApi, this);
        }

        public ClansClientBase(TokenProvider tokenProvider, ClientConfiguration configuration, ClansApi clansApi, PlayersClientBase playersClient)
            : this(tokenProvider, configuration, clansApi)
        {
            _playersClient = playersClient;
            _clanMonitor = new ClanMonitor(_playersClient, TokenProvider, Configuration, _clansApi, this);
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

        public bool DownloadCurrentWars { get; set; }
        public bool DownloadWarLog { get; set; }
        public bool DownloadCwl { get; set; }
        public bool DownloadMembers { get; set; }

        internal ConcurrentDictionary<string, byte> UpdatingClanWar { get; set; } = new ConcurrentDictionary<string, byte>();

        internal ConcurrentDictionary<CachedWar, byte> UpdatingWar { get; set; } = new ConcurrentDictionary<CachedWar, byte>();

        public async Task AddAsync(string tag, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {

            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedClan cachedClan = await dbContext.Clans.Where(c => c.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            cachedClan ??= await PrepareNewCachedClanAsync(formattedTag, downloadWars, downloadCwl, downloadMembers, dbContext).ConfigureAwait(false);

            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return;
        }

        public async Task AddAsync(IEnumerable<string> tags, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c => formattedTags.Contains(c.Tag))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (string formattedTag in formattedTags)
            {
                CachedClan cachedClan = cachedClans.FirstOrDefault(c => c.Tag == formattedTag) ??
                    await PrepareNewCachedClanAsync(formattedTag, downloadWars, downloadCwl, downloadMembers, dbContext).ConfigureAwait(false);
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddOrUpdateAsync(string tag, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedClan cachedClan = await dbContext.Clans
                .Where(c => c.Tag == formattedTag)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            cachedClan ??= await PrepareNewCachedClanAsync(formattedTag, downloadWars, downloadCwl, downloadMembers, dbContext).ConfigureAwait(false);

            cachedClan.Tag = formattedTag;
            cachedClan.DownloadCurrentWar = downloadWars;
            cachedClan.DownloadCwl = downloadCwl;
            cachedClan.DownloadMembers = downloadMembers;

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddOrUpdateAsync(IEnumerable<string> tags, bool downloadWars = true, bool downloadCwl = true, bool downloadMembers = false)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c => formattedTags.Contains(c.Tag))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach(string formattedTag in formattedTags)
            {
                CachedClan cachedClan = cachedClans.FirstOrDefault(c => c.Tag == formattedTag) ??
                    await PrepareNewCachedClanAsync(formattedTag, downloadWars, downloadCwl, downloadMembers, dbContext).ConfigureAwait(false);

                cachedClan.Tag = formattedTag;
                cachedClan.DownloadCurrentWar = downloadWars;
                cachedClan.DownloadCwl = downloadCwl;
                cachedClan.DownloadMembers = downloadMembers;
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }



        public async Task<CachedWar?> GetActiveClanWarOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            List<CachedWar> cache = await dbContext.Wars
                .AsNoTracking()
                .Where(i => (i.ClanTag == formattedTag || i.OpponentTag == formattedTag))
                .OrderByDescending(w => w.PreparationStartTime)
                .ToListAsync(cancellationToken.GetValueOrDefault(_stopRequestedTokenSource.Token))
                .ConfigureAwait(false);

            if (cache.Count == 0)
                return null;

            return cache.FirstOrDefault(c => c.State == WarState.InWar && c.EndTime > DateTime.UtcNow)
                ?? cache.FirstOrDefault(c => c.State == WarState.Preparation && c.EndTime > DateTime.UtcNow)
                ?? cache.First();
        }

        public async Task<CachedWar> GetLeagueWarAsync(string warTag, DateTime season, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(warTag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedWar war = await dbContext.Wars
                .AsNoTracking()
                .FirstAsync(w => w.WarTag == formattedTag && w.Season == season, cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);

            war.Data.Initialize(war.ServerExpiration, formattedTag);

            return war;
        }

        public async Task<CachedWar?> GetLeagueWarOrDefaultAsync(string warTag, DateTime season, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(warTag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedWar? war = await dbContext.Wars
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WarTag == formattedTag && w.Season == season, cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);

            war?.Data?.Initialize(war.ServerExpiration, formattedTag);

            return war;
        }

        public async Task<List<ClanWar>> GetOrFetchLeagueWarsAsync(ClanWarLeagueGroup group, CancellationToken? cancellationToken = default)
        {
            List<ClanWar> result = new List<ClanWar>();

            foreach(var round in group.Rounds)            
                foreach(string warTag in round.WarTags.Where(t => t != "#0"))
                {
                    ClanWar? clanWar = (await GetLeagueWarOrDefaultAsync(warTag, group.Season, cancellationToken))?.Data;

                    if (clanWar == null)
                    {
                        string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                        clanWar = await _clansApi.GetClanWarLeagueWarAsync(token, warTag, cancellationToken);
                    }

                    if (clanWar.PreparationStartTime.Month == group.Season.Month && clanWar.PreparationStartTime.Year == group.Season.Year)
                        result.Add(clanWar);
                }
            
            return result;
        }

        public async Task<List<CachedWar>> GetClanWarsAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Wars
                .AsNoTracking()
                .Where(i => i.ClanTag == formattedTag || i.OpponentTag == formattedTag)
                .ToListAsync(cancellationToken.GetValueOrDefault(_stopRequestedTokenSource.Token))
                .ConfigureAwait(false);
        }

        public async Task<CachedClan> GetCachedClanAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => i.Tag == formattedTag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedClan?> GetCachedClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => i.Tag == formattedTag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<List<CachedClan>> GetCachedClansAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = default)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => formattedTags.Contains(i.Tag))
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Clan> GetOrFetchClanAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Clan? result = (await GetCachedClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data;

            if (result == null)
            {
                string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
 
                result = await _clansApi.GetClanAsync(token, tag, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Clan? result = (await GetCachedClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data;

            if (result == null)
            {
                string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                result = await _clansApi.GetClanOrDefaultAsync(token, tag, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }




        //private readonly ActiveWarMonitor _activeWarMonitor;

        private readonly WarLogMonitor _clanWarLogMonitor;

        private readonly ClanWarMonitor _clanWarMonitor;

        private readonly ClanMonitor _clanMonitor;

        private readonly CwlMonitor _cwlMonitor;

        private readonly WarMonitor _warMonitor;

        public Task StartAsync(CancellationToken cancellationToken)
        {           
            Task.Run(() =>
            {
                //_ = _activeWarMonitor.RunAsync(cancellationToken);


                _ = _clanWarLogMonitor.RunAsync(cancellationToken);
                _ = _clanWarMonitor.RunAsync(cancellationToken);
                _ = _clanMonitor.RunAsync(cancellationToken);
                _ = _cwlMonitor.RunAsync(cancellationToken);
                _ = _warMonitor.RunAsync(cancellationToken);
            });

            return Task.CompletedTask;
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
                && fetched.Labels.Except(stored.Labels).Count() == 0
                && Clan.ClanMembersJoined(stored, fetched).Count == 0
                && Clan.ClanMembersLeft(stored, fetched).Count == 0
                && Clan.Donations(stored, fetched).Count == 0 
                && Clan.DonationsReceived(stored, fetched).Count == 0
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
                || ClanWar.IsSameWar(stored.Data, fetched.Data) == false)
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
                || ClanWar.IsSameWar(stored.Data, fetched.Data) == false)
                throw new ArgumentException();

            return HasUpdated(stored.Data, fetched.Data);
        }

        public virtual ValueTask<TimeSpan> ClanTimeToLiveAsync(ApiResponse<Clan> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> ClanTimeToLiveAsync(Exception exception)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> ClanWarLogTimeToLiveAsync(ApiResponse<ClanWarLog> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> ClanWarLogTimeToLiveAsync(Exception exception)
        {
            if (exception is ApiException apiException)
                if (apiException.ErrorCode == (int)HttpStatusCode.Forbidden)
                    return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2));

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        public virtual ValueTask<TimeSpan> ClanWarLeagueGroupTimeToLiveAsync(ApiResponse<ClanWarLeagueGroup> apiResponse)
        {
            if (apiResponse.Data.State == GroupState.Ended)
                return new ValueTask<TimeSpan>(
                    new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                        .AddMonths(1)
                        .Subtract(new TimeSpan(0, 0, 0, 0, 1))
                        .Subtract(DateTime.UtcNow));

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        public virtual ValueTask<TimeSpan> ClanWarLeagueGroupTimeToLiveAsync(Exception exception)
        {
            if (Clash.IsCwlEnabled())
                return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(20));
            else
                return new ValueTask<TimeSpan>(
                    new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                        .AddMonths(1)
                        .Subtract(new TimeSpan(0, 0, 0, 0, 1))
                        .Subtract(DateTime.UtcNow));
        }

        public virtual ValueTask<TimeSpan> ClanWarTimeToLiveAsync(Exception exception)
        {
            if (exception is ApiException apiException)
                if (apiException.ErrorCode == (int)HttpStatusCode.Forbidden)
                    return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2));

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        public virtual ValueTask<TimeSpan> ClanWarTimeToLiveAsync(ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse.Data.State == WarState.Preparation)
                return new ValueTask<TimeSpan>(apiResponse.Data.StartTime.AddHours(-1) - DateTime.UtcNow);

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            List<Task> tasks = new List<Task>
            {
                base.StopAsync(cancellationToken),
                _clanWarLogMonitor.StopAsync(cancellationToken),
                //_activeWarMonitor.StopAsync(cancellationToken),
                _clanWarMonitor.StopAsync(cancellationToken),
                _clanMonitor.StopAsync(cancellationToken),
                _cwlMonitor.StopAsync(cancellationToken),
                _warMonitor.StopAsync(cancellationToken)
            };

            await Task.WhenAll(tasks);

            OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));
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

        internal void OnClanWarLeagueGroupUpdated(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        {
            Task.Run(() => ClanWarLeagueGroupUpdated?.Invoke(this, new ClanWarLeagueGroupUpdatedEventArgs(stored, fetched)));
        }

        internal void OnClanWarLogUpdated(ClanWarLog? stored, ClanWarLog fetched)
        {
            Task.Run(() => ClanWarLogUpdated?.Invoke(this, new ClanWarLogUpdatedEventArgs(stored, fetched)));
        }

        internal void OnClanWarStartingSoon(ClanWar clanWar)
        {
            Task.Run(() => ClanWarStartingSoon?.Invoke(this, new ClanWarEventArgs(clanWar)));
        }

        internal void OnClanWarUpdated(ClanWar stored, ClanWar fetched)
        {
            Task.Run(() => ClanWarUpdated?.Invoke(this, new ClanWarUpdatedEventArgs(stored, fetched)));
        }

        private async Task<CachedClan> PrepareNewCachedClanAsync(string formattedTag, bool downloadWars, bool downloadCwl, bool downloadMembers, CacheContext dbContext)
        {
            //using var scope = Services.CreateScope();

            //CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedClan cachedClan = new CachedClan(formattedTag)
            {
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

            //await dbContext.SaveChangesAsync();

            return cachedClan;
        }

        internal async Task InsertNewWarAsync(CachedWar fetched)
        {
            if (fetched.Data == null)
                throw new ArgumentException("Data should not be null.");

            if (UpdatingWar.TryAdd(fetched, new byte()) == false)
                return;

            try
            {
                using var scope = Services.CreateScope();

                CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

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
                UpdatingWar.TryRemove(fetched, out var _);
            }
        }

        internal async Task<CachedClanWar?> TryAddCachedClanWar(string tag, DateTime serverExpiration, DateTime localExpiration, CancellationToken? cancellationToken = default)
        {
            try
            {
                using var scope = Services.CreateScope();

                CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

                CachedClanWar? clanWar = await dbContext.ClanWars.FirstOrDefaultAsync(cw => cw.Tag == tag, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

                if (clanWar != null)
                    return clanWar;

                clanWar = new CachedClanWar(tag)
                {
                    LocalExpiration = localExpiration,
                    ServerExpiration = serverExpiration                    
                };

                dbContext.ClanWars.Add(clanWar);

                await dbContext.SaveChangesAsync(cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

                return clanWar;
            }
            catch (Exception)
            {
                return null; //this is a race condition that is hard to handle
            }
        }
    }
}