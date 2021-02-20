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

namespace CocApi.Cache
{
    public class ClansClientBase : ClientBase
    {
        private readonly ClansApi _clansApi;
        private readonly MemberMonitor? _memberMonitor;

        internal ConcurrentDictionary<string, Context.CachedItems.CachedClan?> UpdatingClan { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingClanWar { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingWar { get; } = new();
        internal ConcurrentDictionary<string, ClanWar?> UpdatingCwlWar { get; } = new();

        public ClansClientBase(ClansApi clansApi, IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clanMonitor = new ClanMonitor(dbContextFactory, dbContextArgs, clansApi, this);
            _newWarMonitor = new NewWarMonitor(dbContextFactory, dbContextArgs, this);
            _newCwlWarMonitor = new NewCwlWarMonitor(dbContextFactory, dbContextArgs, clansApi, this);
            _warMonitor = new WarMonitor(dbContextFactory, dbContextArgs, clansApi, this);
            _activeWarMonitor = new ActiveWarMonitor(dbContextFactory, dbContextArgs, clansApi, this);
        }

        public ClansClientBase(ClansApi clansApi, PlayersClientBase playersClient, PlayersApi playersApi, IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs)
            : this(clansApi, dbContextFactory, dbContextArgs)
        {
            _memberMonitor = new MemberMonitor(dbContextFactory, dbContextArgs, playersClient, playersApi);
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






        private readonly ClanMonitor _clanMonitor;
        private readonly NewWarMonitor _newWarMonitor;
        private readonly NewCwlWarMonitor _newCwlWarMonitor;
        private readonly WarMonitor _warMonitor;
        private readonly ActiveWarMonitor _activeWarMonitor;



        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (!Library.Monitors.Clans.IsDisabled)
                    _ = _clanMonitor.RunAsync();

                if (!Library.Monitors.NewWars.IsDisabled)
                    _ = _newWarMonitor.RunAsync();

                if (!Library.Monitors.NewCwlWars.IsDisabled)
                    _ = _newCwlWarMonitor.RunAsync();

                if (!Library.Monitors.Wars.IsDisabled)
                    _ = _warMonitor.RunAsync();

                if (!Library.Monitors.ActiveWars.IsDisabled)
                    _ = _activeWarMonitor.RunAsync();

                if (!Library.Monitors.Members.IsDisabled && _memberMonitor != null)
                    _ = _memberMonitor.RunAsync();

            }, cancellationToken);
        }

        internal bool HasUpdated(CachedClan stored, CachedClan fetched)
        {
            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (fetched.Content == null)
                return false;

            return !fetched.Content.Equals(stored.Content);

            //return HasUpdated(stored.Data, fetched.Data);
        }

        //internal bool HasUpdated(Clan? stored, Clan fetched)
        //{
        //    if (stored == null)
        //        return true;

        //    return !(stored.BadgeUrls?.Small == fetched.BadgeUrls?.Small
        //        && stored.ClanLevel == fetched.ClanLevel
        //        && stored.ClanPoints == fetched.ClanPoints
        //        && stored.ClanVersusPoints == fetched.ClanVersusPoints
        //        && stored.Description == fetched.Description
        //        && stored.IsWarLogPublic == fetched.IsWarLogPublic
        //        && stored.Location?.Id == fetched.Location?.Id
        //        && stored.Name == fetched.Name
        //        && stored.RequiredTrophies == fetched.RequiredTrophies
        //        && stored.Type == fetched.Type
        //        && stored.WarFrequency == fetched.WarFrequency
        //        && stored.WarLeague?.Id == fetched.WarLeague?.Id
        //        && stored.WarLosses == fetched.WarLosses
        //        && stored.WarTies == fetched.WarTies
        //        && stored.WarWins == fetched.WarWins
        //        && stored.WarWinStreak == fetched.WarWinStreak
        //        && stored.Labels.SequenceEqual(fetched.Labels)
        //        && fetched.Labels.SequenceEqual(stored.Labels)
        //        && Clan.ClanMembersJoined(stored, fetched).Count == 0
        //        && Clan.ClanMembersLeft(stored, fetched).Count == 0
        //        && Clan.Donations(stored, fetched).Count == 0 
        //        && Clan.DonationsReceived(stored, fetched).Count == 0);
        //}

        internal bool HasUpdated(Context.CachedItems.CachedClanWarLeagueGroup stored, Context.CachedItems.CachedClanWarLeagueGroup fetched)
        {
            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (fetched.Content == null)
                return false;

            return !fetched.Content.Equals(stored.Content);

            //return HasUpdated(stored.Data, fetched.Data);
        }

        //internal bool HasUpdated(ClanWarLeagueGroup? stored, ClanWarLeagueGroup fetched)
        //{
        //    if (stored == null)
        //        return false;

        //    if (stored.State != fetched.State)
        //        return true;

        //    foreach (ClanWarLeagueRound round in fetched.Rounds)
        //        foreach (string warTag in round.WarTags)
        //            if (stored.Rounds.Any(r => r.WarTags.Any(w => w == warTag)) == false)
        //                return true;

        //    return false;
        //}

        internal bool HasUpdated(Context.CachedItems.CachedClanWarLog stored, Context.CachedItems.CachedClanWarLog fetched)
        {
            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (fetched.Content == null)
                return false;

            return !fetched.Content.Equals(stored.Content);

            //return HasUpdated(stored.Data, fetched.Data);
        }

        //internal bool HasUpdated(ClanWarLog? stored, ClanWarLog fetched)
        //{
        //    if (stored == null)
        //        return true;

        //    if (fetched?.Items == null)
        //        return false;

        //    if (stored.Items.Count == 0 && fetched.Items.Count == 0)
        //        return false;

        //    if (stored.Items.Count != fetched.Items.Count)
        //        return true;

        //    return stored.Items.Max(i => i?.EndTime ?? DateTime.MinValue) != fetched.Items.Max(i => i?.EndTime ?? DateTime.MinValue);
        //}

        internal bool HasUpdated(Context.CachedItems.CachedWar stored, Context.CachedItems.CachedClanWar fetched)
        {
            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (stored.Content == null)
                throw new InvalidOperationException($"{nameof(stored)}.Data is null");

            if (fetched.Content == null)
                throw new InvalidOperationException($"{nameof(fetched)}.Data is null");

            return !fetched.Content.Equals(stored.Content);

            //return HasUpdated(stored.Data, fetched.Data);
        }

        //internal bool HasUpdated(ClanWar stored, ClanWar fetched)
        //{
        //    return !(stored.EndTime == fetched.EndTime
        //        && stored.StartTime == fetched.StartTime
        //        && stored.State == fetched.State
        //        && stored.Attacks.Count == fetched.Attacks.Count);
        //}

        internal bool HasUpdated(Context.CachedItems.CachedWar stored, Context.CachedItems.CachedWar fetched)
        {
            if (ReferenceEquals(stored, fetched))
                return false;

            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (stored.Content == null)
                throw new InvalidOperationException($"{nameof(stored)}.Data is null");

            if (fetched.Content == null)
                throw new InvalidOperationException($"{nameof(fetched)}.Data is null");

            if (!ClanWar.IsSameWar(stored.Content, fetched.Content))
                throw new InvalidOperationException("Provided wars are the same war.");

            return !fetched.Content.Equals(stored.Content);

            //return HasUpdated(stored.Data, fetched.Data);
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
            {
                if (!Clash.IsCwlEnabled || group.Content?.State == GroupState.Ended)
                    return new ValueTask<TimeSpan>(
                        new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)
                            .AddMonths(1)
                            .Subtract(DateTime.UtcNow));
            }

            if (apiResponse is ApiResponse<ClanWar> war)
            {
                if (war.StatusCode == HttpStatusCode.Forbidden)
                    return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(2));

                if (war.Content?.State == WarState.Preparation)
                    return new ValueTask<TimeSpan>(war.Content.StartTime.AddHours(-1) - DateTime.UtcNow);
            }

            return new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>
            {
                _clanMonitor.StopAsync(cancellationToken),
                _newWarMonitor.StopAsync(cancellationToken),
                _newCwlWarMonitor.StopAsync(cancellationToken),
                _warMonitor.StopAsync(cancellationToken),
                _activeWarMonitor.StopAsync(cancellationToken)
            };

            if (_memberMonitor != null)
                tasks.Add(_memberMonitor.StopAsync(cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "Stopping clans client"));
        }

        internal async Task OnClanUpdatedAsync(ClanUpdatedEventArgs eventArgs)
        {
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

                ClanUpdated?.Invoke(this, eventArgs).ConfigureAwait(false);
            }
            catch (Exception)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on clan updated."));
            }
            finally
            {
                Library.ConcurrentEventsSemaphore.Release();
            }
        }

        internal async Task OnClanWarAddedAsync(WarAddedEventArgs eventArgs)
        {
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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
            try
            {
                await Library.ConcurrentEventsSemaphore.WaitAsync();

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