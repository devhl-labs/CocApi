using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public class ClansClientBase : ClientBase, IHostedService
    {
        internal ConcurrentDictionary<string, CachedClan?> UpdatingClan { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingWar { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingCwlWar { get; } = new();

        public ClansClientBase(ClansApi clansApi, PlayersClientBase playersClient, PlayersApi playersApi, 
            CacheDbContextFactoryProvider provider,
            IOptions<ClanClientOptions> options)
            : base(provider)
        {
            Library.ConcurrentEventsSemaphore = new SemaphoreSlim(options.Value.MaxConcurrentEvents, options.Value.MaxConcurrentEvents);
            _clansApi = clansApi;
            //_playersClient = playersClient;
            _options = options;
            _clanMonitor = new ClanMonitor(provider, clansApi, this, options);
            _newWarMonitor = new NewWarMonitor(provider, this, options);
            _newCwlWarMonitor = new NewCwlWarMonitor(provider, clansApi, this, options);
            _warMonitor = new WarMonitor(provider, clansApi, this, options);
            _activeWarMonitor = new ActiveWarMonitor(provider, clansApi, this, options);
            _memberMonitor = new MemberMonitor(provider, playersClient, playersApi, options);
            _cwlWarMonitor = new CwlWarMonitor(provider, clansApi, this, options);
        }

        public event AsyncEventHandler<ClanUpdatedEventArgs>? ClanUpdated;
        public event AsyncEventHandler<WarAddedEventArgs>? ClanWarAdded;
        public event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
        public event AsyncEventHandler<WarEventArgs>? ClanWarEndNotSeen;
        public event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
        public event AsyncEventHandler<ClanWarLeagueGroupUpdatedEventArgs>? ClanWarLeagueGroupUpdated;
        public event AsyncEventHandler<ClanWarLogUpdatedEventArgs>? ClanWarLogUpdated;
        public event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
        public event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

        public async Task DeleteAsync(string tag)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            while (!UpdatingClan.TryAdd(formattedTag, null))
                await Task.Delay(250).ConfigureAwait(false);

            try
            {
                Context.CachedItems.CachedClan cachedClan = await dbContext.Clans.FirstOrDefaultAsync(c => c.Tag == formattedTag).ConfigureAwait(false);

                if (cachedClan != null)
                    dbContext.Clans.Remove(cachedClan);

                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            finally
            {
                UpdatingClan.TryRemove(formattedTag, out _);
            }
        }

        public async Task AddAsync(string tag, bool downloadClan = true, bool downloadWar = true, bool downloadLog = false, bool downloadGroup = true, bool downloadMembers = false)
            => await AddAsync(new string[] { tag }, downloadClan, downloadWar, downloadLog, downloadGroup, downloadMembers).ConfigureAwait(false);

        public async Task AddAsync(
            IEnumerable<string> tags, bool downloadClan = true, bool downloadWar = true, bool downloadLog = false, bool downloadGroup = true, bool downloadMembers = false)
        {
            HashSet<string> formattedTags = new HashSet<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<Context.CachedItems.CachedClan> cachedClans = await dbContext.Clans
                .Where(c => formattedTags.Contains(c.Tag))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (string formattedTag in formattedTags.Where(tag => !cachedClans.Any(c => c.Tag == tag)))
                dbContext.Clans.Add(new Context.CachedItems.CachedClan(formattedTag, downloadClan, downloadWar, downloadLog, downloadGroup, downloadMembers));

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddOrUpdateAsync(string tag, bool downloadClan = true, bool downloadWar = true, bool downloadLog = false, bool downloadGroup = true, bool downloadMembers = false)
            => await AddOrUpdateAsync(new string[] { tag }, downloadClan, downloadWar, downloadLog, downloadGroup, downloadMembers).ConfigureAwait(false);

        public async Task AddOrUpdateAsync(
            IEnumerable<string> tags, bool downloadClan = true, bool downloadWar = true, bool downloadLog = false, bool downloadGroup = true, bool downloadMembers = false)
        {
            HashSet<string> formattedTags = new HashSet<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<Context.CachedItems.CachedClan> cachedClans = await dbContext.Clans
                .Where(c => formattedTags.Contains(c.Tag))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (string formattedTag in formattedTags)
            {
                Context.CachedItems.CachedClan? cachedClan = cachedClans.FirstOrDefault(c => c.Tag == formattedTag);

                cachedClan ??= new Context.CachedItems.CachedClan();

                cachedClan.Tag = formattedTag;
                cachedClan.Download = downloadClan;
                cachedClan.WarLog.Download = downloadLog;
                cachedClan.CurrentWar.Download = downloadWar;
                cachedClan.Group.Download = downloadGroup;
                cachedClan.DownloadMembers = downloadMembers;

                dbContext.Clans.Update(cachedClan);
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<CachedWar?> GetActiveClanWarOrDefaultAsync(string tag, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<CachedWar> cache = await dbContext.Wars
                .AsNoTracking()
                .Where(i => (i.ClanTag == formattedTag || i.OpponentTag == formattedTag))
                .OrderByDescending(w => w.PreparationStartTime)
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);

            if (cache.Count == 0)
                return null;

            return cache.FirstOrDefault(c => c.State == WarState.InWar && c.EndTime > DateTime.UtcNow)
                ?? cache.FirstOrDefault(c => c.State == WarState.Preparation && c.EndTime > DateTime.UtcNow)
                ?? cache.First();
        }

        public async Task<ClanWarLeagueGroup> GetOrFetchLeagueGroupAsync(string tag, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return (await dbContext.Clans.FirstOrDefaultAsync(g => g.Tag == formattedTag).ConfigureAwait(false))?.Group.Content
                ?? await _clansApi.FetchClanWarLeagueGroupAsync(formattedTag, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ClanWarLeagueGroup?> GetOrFetchLeagueGroupOrDefaultAsync(string tag, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return (await dbContext.Clans.FirstOrDefaultAsync(g => g.Tag == formattedTag).ConfigureAwait(false))?.Group.Content
                ?? await _clansApi.FetchClanWarLeagueGroupOrDefaultAsync(formattedTag, cancellationToken).ConfigureAwait(false);
        }

        public async Task<CachedWar> GetLeagueWarAsync(string warTag, DateTime season, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(warTag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            CachedWar war = await dbContext.Wars
                .AsNoTracking()
                .FirstAsync(w => w.WarTag == formattedTag && w.Season == season, cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);

            war.Content?.Initialize(war.ExpiresAt.Value, formattedTag);

            return war;
        }

        public async Task<CachedWar?> GetLeagueWarOrDefaultAsync(string warTag, DateTime season, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(warTag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            CachedWar? war = await dbContext.Wars
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WarTag == formattedTag && w.Season == season, cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);

            war?.Content?.Initialize(war.ExpiresAt.Value, formattedTag);

            return war;
        }

        public async Task<List<ClanWar>> GetOrFetchLeagueWarsAsync(ClanWarLeagueGroup group, CancellationToken? cancellationToken = null)
        {
            List<ClanWar> result = new List<ClanWar>();

            foreach (var round in group.Rounds)
                foreach (string warTag in round.WarTags.Where(t => t != "#0"))
                {
                    ClanWar? clanWar = (await GetLeagueWarOrDefaultAsync(warTag, group.Season, cancellationToken).ConfigureAwait(false))?.Content;

                    if (clanWar == null)
                        clanWar = await _clansApi.FetchClanWarLeagueWarAsync(warTag, cancellationToken).ConfigureAwait(false);

                    if (clanWar.PreparationStartTime.Month == group.Season.Month && clanWar.PreparationStartTime.Year == group.Season.Year)
                        result.Add(clanWar);
                }

            return result;
        }

        public async Task<List<CachedWar>> GetClanWarsAsync(string tag, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Wars
                .AsNoTracking()
                .Where(i => i.ClanTag == formattedTag || i.OpponentTag == formattedTag)
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedClan> GetCachedClanAsync(string tag, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => i.Tag == formattedTag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedClan?> GetCachedClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = null)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => i.Tag == formattedTag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<List<CachedClan>> GetCachedClansAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = null)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Clans
                .AsNoTracking()
                .Where(i => formattedTags.Contains(i.Tag))
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Clan> GetOrFetchClanAsync(string tag, CancellationToken? cancellationToken = null)
        {
            Clan? result = (await GetCachedClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await _clansApi.FetchClanAsync(tag, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<Clan?> GetOrFetchClanOrDefaultAsync(string tag, CancellationToken? cancellationToken = null)
        {
            Clan? result = (await GetCachedClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await _clansApi.FetchClanOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

            return result;
        }

        private readonly ClansApi _clansApi;
        //private readonly PlayersClientBase _playersClient;
        private readonly IOptions<ClanClientOptions> _options;
        private readonly ClanMonitor _clanMonitor;
        private readonly NewWarMonitor _newWarMonitor;
        private readonly NewCwlWarMonitor _newCwlWarMonitor;
        private readonly WarMonitor _warMonitor;
        private readonly ActiveWarMonitor _activeWarMonitor;
        private readonly MemberMonitor _memberMonitor;
        private readonly CwlWarMonitor _cwlWarMonitor;

        private bool _isRunning;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task StartAsync(CancellationToken _)
        {
            await _semaphore.WaitAsync(_).ConfigureAwait(false);

            try
            {
                if (_isRunning)
                    throw new InvalidOperationException("Already running.");

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                if (_options.Value.Clans.Enabled)
                    _clanMonitor.Start(_stopRequestedTokenSource.Token);
                if (_options.Value.NewWars.Enabled)
                    _newWarMonitor.Start(_stopRequestedTokenSource.Token);
                if (_options.Value.NewCwlWars.Enabled)
                    _newCwlWarMonitor.Start(_stopRequestedTokenSource.Token);
                if (_options.Value.Wars.Enabled)
                    _warMonitor.Start(_stopRequestedTokenSource.Token);
                if (_options.Value.ActiveWars.Enabled)
                    _activeWarMonitor.Start(_stopRequestedTokenSource.Token);
                if (_options.Value.ClanMembers.Enabled)
                    _memberMonitor.Start(_stopRequestedTokenSource.Token);
                if (_options.Value.CwlWars.Enabled)
                    _cwlWarMonitor.Start(_stopRequestedTokenSource.Token);

                //_playersClient.StartAsync(_stopRequestedTokenSource.Token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task StopAsync(CancellationToken _)
        {
            await _semaphore.WaitAsync(_).ConfigureAwait(false);

            _stopRequestedTokenSource.Cancel();

            try
            {
                List<Task> tasks = new();

                if (_clanMonitor.RunTask != null)
                    tasks.Add(_clanMonitor.RunTask);
                if (_newWarMonitor.RunTask != null)
                    tasks.Add(_newWarMonitor.RunTask);
                if (_newCwlWarMonitor.RunTask != null)
                    tasks.Add(_newCwlWarMonitor.RunTask);
                if (_warMonitor.RunTask != null)
                    tasks.Add(_warMonitor.RunTask);
                if (_activeWarMonitor.RunTask != null)
                    tasks.Add(_activeWarMonitor.RunTask);
                if (_memberMonitor.RunTask != null)
                    tasks.Add(_memberMonitor.RunTask);
                if (_cwlWarMonitor.RunTask != null)
                    tasks.Add(_cwlWarMonitor.RunTask);

                //tasks.Add(_playersClient.StopAsync(_));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                _isRunning = false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync<T>(ApiResponse<T> apiResponse) where T : class
        {
            try
            {
                return await TimeToLiveAsync(apiResponse).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return TimeSpan.FromMinutes(0);
            }
        }

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync<T>(Exception exception) where T : class
        {
            try
            {
                return await TimeToLiveAsync<T>(exception).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return TimeSpan.FromMinutes(0);
            }
        }

        public virtual ValueTask<TimeSpan> TimeToLiveAsync<T>(Exception exception) where T : class
        {
            if (typeof(T) == typeof(ClanWarLeagueGroup))
            {
                if (Clash.IsCwlEnabled)
                    return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(20));
                else
                    return new ValueTask<TimeSpan>(
                        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                            .AddMonths(1)
                            .Subtract(DateTime.UtcNow));
            }

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        public virtual ValueTask<TimeSpan> TimeToLiveAsync<T>(ApiResponse<T> apiResponse) where T : class
        {
            if (apiResponse is ApiResponse<Clan>)
                return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

            if (apiResponse is ApiResponse<ClanWarLog>)
                return apiResponse.StatusCode == HttpStatusCode.Forbidden
                    ? new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2))
                    : new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

            if (apiResponse is ApiResponse<ClanWarLeagueGroup> group)            
                if (!Clash.IsCwlEnabled || 
                    (group.Content?.State == GroupState.Ended && DateTime.UtcNow.Month == group.Content.Season.Month) || 
                    (group.Content == null && DateTime.UtcNow.Day >= 3))
                    return new ValueTask<TimeSpan>(
                        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                            .AddMonths(1)
                            .Subtract(DateTime.UtcNow));            

            if (apiResponse is ApiResponse<ClanWar> war)
            {
                if (war.StatusCode == HttpStatusCode.Forbidden)
                    return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2));

                if (war.Content?.State == WarState.Preparation)
                    return new ValueTask<TimeSpan>(war.Content.StartTime.AddHours(-1) - DateTime.UtcNow);
            }

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        internal async Task OnClanUpdatedAsync(ClanUpdatedEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);

            try
            {
                ClanUpdated?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan updated.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarAddedAsync(WarAddedEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);
            
            try
            {               
                ClanWarAdded?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error adding new clan war.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarEndingSoonAsync(WarEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);

            try
            {
                ClanWarEndingSoon?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war ending soon.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarEndNotSeenAsync(WarEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);
            
            try
            {
                ClanWarEndNotSeen?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war end not seen.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarEndedAsync(WarEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);

            try
            {
                ClanWarEnded?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war ended.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarLeagueGroupUpdatedAsync(ClanWarLeagueGroupUpdatedEventArgs events)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(events.CancellationToken);

            try
            {
                ClanWarLeagueGroupUpdated?.Invoke(this, events).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war league group updated.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarLogUpdatedAsync(ClanWarLogUpdatedEventArgs events)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(events.CancellationToken);

            try
            {
                ClanWarLogUpdated?.Invoke(this, events).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war log updated.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarStartingSoonAsync(WarEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);

            try
            {
                ClanWarStartingSoon?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war starting soon.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarUpdatedAsync(ClanWarUpdatedEventArgs eventArgs)
        {
            await Library.ConcurrentEventsSemaphore.WaitAsync(eventArgs.CancellationToken);

            try
            {
                ClanWarUpdated?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan war updated.", e));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }
    }
}