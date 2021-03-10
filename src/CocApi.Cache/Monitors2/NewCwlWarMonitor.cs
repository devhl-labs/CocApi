using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal class NewCwlWarMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;
        private readonly Dictionary<DateTime, HashSet<string>> _downloadedWars = new();

        public NewCwlWarMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        //public async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    _cancellationToken = cancellationToken;

        //    _cancellationToken.Register(BeginShutdown);

        //    try
        //    {
        //        if (_isRunning)
        //            return;

        //        _isRunning = true;

        //        //_stopRequestedTokenSource = new CancellationTokenSource();

        //        Library.OnLog(this, new LogEventArgs(LogLevel.Information, "running"));

        //        while (_cancellationToken.IsCancellationRequested == false)
        //        {
        //            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //            List<CachedClan> cachedClans = await dbContext.Clans
        //                .Where(c =>
        //                    c.CurrentWar.Download &&
        //                    !c.Group.Added &&
        //                    !string.IsNullOrWhiteSpace(c.Group.RawContent) &&
        //                    c.Id > _id)
        //                .OrderBy(c => c.Id)
        //                .Take(Library.Monitors.NewCwlWars.ConcurrentUpdates)
        //                .ToListAsync(_cancellationToken)
        //                .ConfigureAwait(false);

        //            _id = cachedClans.Count == Library.Monitors.NewCwlWars.ConcurrentUpdates
        //                ? cachedClans.Max(c => c.Id)
        //                : int.MinValue;

        //            List<Task<Client.ApiResponse<CocApi.Model.ClanWar>>> tasks = new();

        //            Dictionary<string, DateTime> updatingTags = new();

        //            try
        //            {
        //                foreach (CachedClan cachedClan in cachedClans)
        //                {
        //                    _downloadedWars.TryAdd(cachedClan.Group.Season.Value, new HashSet<string>());

        //                    HashSet<string> downloadedWars = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season).Value;

        //                    foreach (var round in cachedClan.Group.Content.Rounds)
        //                        foreach (string tag in round.WarTags.Where(t => t != "#0" && !downloadedWars.Contains(t) && _clansClient.UpdatingCwlWar.TryAdd(t, null)))
        //                        {
        //                            updatingTags.Add(tag, cachedClan.Group.Season.Value);

        //                            tasks.Add(_clansApi.FetchClanWarLeagueWarResponseAsync(tag, _cancellationToken));
        //                        }
        //                }

        //                List<CachedWar> cachedWars = await dbContext.Wars
        //                    .Where(w => !string.IsNullOrWhiteSpace(w.WarTag) && updatingTags.Keys.Contains(w.WarTag))
        //                    .ToListAsync(_cancellationToken)
        //                    .ConfigureAwait(false);

        //                Task updates = Task.WhenAll(tasks);

        //                await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

        //                _cancellationToken.ThrowIfCancellationRequested();

        //                List<Client.ApiResponse<CocApi.Model.ClanWar>> allFetchedWars = new();

        //                foreach (var task in tasks.Where(t => t.Result is Client.ApiResponse<CocApi.Model.ClanWar>))
        //                    allFetchedWars.Add(task.Result);

        //                if (!allFetchedWars.Any())
        //                    continue;

        //                List<Task> saveWars = new();

        //                foreach (CachedClan cachedClan in cachedClans)
        //                {
        //                    List<Client.ApiResponse<CocApi.Model.ClanWar>> wars = allFetchedWars
        //                        .Where(w => w.IsSuccessStatusCode && w.Content != null && w.Content.Clans.Any(c => c.Key == cachedClan.Tag)).ToList();

        //                    foreach (Client.ApiResponse<CocApi.Model.ClanWar> warResponse in wars)
        //                    {
        //                        if (cachedWars.Any((c => c.WarTag == warResponse.Content.WarTag && c.Season == cachedClan.Group.Season)))
        //                            continue;

        //                        saveWars.Add(NewWarFoundAsync(cachedClans, cachedClan, warResponse, dbContext));
        //                    }
        //                }

        //                updates = Task.WhenAll(saveWars);

        //                await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

        //                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

        //                foreach (CachedClan cachedClan in cachedClans)
        //                {
        //                    List<Client.ApiResponse<CocApi.Model.ClanWar>> wars = allFetchedWars
        //                        .Where(w => w.IsSuccessStatusCode && w.Content != null && w.Content.Clans.Any(c => c.Key == cachedClan.Tag)).ToList();

        //                    HashSet<string> downloadedWars = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season).Value;

        //                    foreach (Client.ApiResponse<CocApi.Model.ClanWar> warResponse in wars)
        //                        downloadedWars.Add(warResponse.Content.WarTag);
        //                }

        //                _cancellationToken.ThrowIfCancellationRequested();
        //            }
        //            finally
        //            {
        //                foreach (var tag in updatingTags.Keys)
        //                {
        //                    _clansClient.UpdatingCwlWar.TryRemove(tag, out var _);
        //                }
        //            }








        //            //Dictionary<string, DateTime> updatingTags = new();

        //            //foreach (CachedClan cachedClan in cachedClans)
        //            //{
        //            //    cachedClan.Group.Added = true;



        //            //    foreach (var round in cachedClan.Group.Content.Rounds)
        //            //        foreach (string tag in round.WarTags.Where(t => t != "#0"))
        //            //            if (!tags.Contains(tag))
        //            //            //if (_clansClient.UpdatingCwlWar.TryAdd(tag, null))
        //            //                updatingTags.TryAdd(tag, cachedClan.Group.Season.Value);
        //            //}

        //            //try
        //            //{
        //            //    if (!updatingTags.Any())
        //            //        continue;

        //            //    List<CachedWar> cachedWars = await dbContext.Wars
        //            //        .Where(w => !string.IsNullOrWhiteSpace(w.WarTag) && updatingTags.Keys.Contains(w.WarTag))
        //            //        .ToListAsync(_stopRequested)
        //            //        .ConfigureAwait(false);

        //            //    foreach (var kvp in updatingTags.Where(t => !cachedWars.Any(w => w.WarTag == t.Key && w.Season == t.Value)))
        //            //        tasks.Add(InsertNewCwlWarAsync(kvp.Key, kvp.Value, cachedClans, dbContext, _stopRequested));

        //            //    try
        //            //    {
        //            //        await Task.WhenAll(tasks).ConfigureAwait(false);
        //            //    }
        //            //    catch (Exception)
        //            //    {
        //            //    }
        //            //    await dbContext.SaveChangesAsync(_stopRequested).ConfigureAwait(false);
        //            //}
        //            //finally
        //            //{
        //            //    foreach(string tag in updatingTags.Keys)
        //            //        _clansClient.UpdatingCwlWar.TryRemove(tag, out _);
        //            //}

        //            if (_id == int.MinValue)
        //                await Task.Delay(Library.Monitors.NewCwlWars.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
        //            else
        //                await Task.Delay(Library.Monitors.NewCwlWars.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        //        }

        //        _isRunning = false;
        //    }
        //    catch (Exception e)
        //    {
        //        _isRunning = false;

        //        if (_cancellationToken.IsCancellationRequested)
        //            return;

        //        Library.OnLog(this, new LogEventArgs(LogLevel.Error, "errored", e));

        //        _ = Task.Run(() => RunAsync(_cancellationToken));
        //    }
        //}

        protected override async Task PollAsync()
        {
            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            List<CachedClan> cachedClans = await dbContext.Clans
                .Where(c =>
                    c.CurrentWar.Download &&
                    !c.Group.Added &&
                    !string.IsNullOrWhiteSpace(c.Group.RawContent) &&
                    c.Id > _id)
                .OrderBy(c => c.Id)
                .Take(Library.Monitors.NewCwlWars.ConcurrentUpdates)
                .ToListAsync(_cancellationToken)
                .ConfigureAwait(false);

            _id = cachedClans.Count == Library.Monitors.NewCwlWars.ConcurrentUpdates
                ? cachedClans.Max(c => c.Id)
                : int.MinValue;

            List<Task<Client.ApiResponse<CocApi.Model.ClanWar>>> tasks = new();

            Dictionary<string, DateTime> updatingTags = new();

            try
            {
                foreach (CachedClan cachedClan in cachedClans)
                {
                    _downloadedWars.TryAdd(cachedClan.Group.Season.Value, new HashSet<string>());

                    HashSet<string> downloadedWars = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season).Value;

                    foreach (var round in cachedClan.Group.Content.Rounds)
                        foreach (string tag in round.WarTags.Where(t => t != "#0" && !downloadedWars.Contains(t) && _clansClient.UpdatingCwlWar.TryAdd(t, null)))
                        {
                            updatingTags.Add(tag, cachedClan.Group.Season.Value);

                            tasks.Add(_clansApi.FetchClanWarLeagueWarResponseAsync(tag, _cancellationToken));
                        }
                }

                List<CachedWar> cachedWars = await dbContext.Wars
                    .Where(w => !string.IsNullOrWhiteSpace(w.WarTag) && updatingTags.Keys.Contains(w.WarTag))
                    .ToListAsync(_cancellationToken)
                    .ConfigureAwait(false);

                await Task.WhenAll(tasks);

                //Task updates = Task.WhenAll(tasks);

                //await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

                _cancellationToken.ThrowIfCancellationRequested();

                List<Client.ApiResponse<CocApi.Model.ClanWar>> allFetchedWars = new();

                foreach (var task in tasks.Where(t => t.Result is Client.ApiResponse<CocApi.Model.ClanWar>))
                    allFetchedWars.Add(task.Result);

                if (!allFetchedWars.Any())
                    return;

                List<Task> saveWars = new();

                foreach (CachedClan cachedClan in cachedClans)
                {
                    List<Client.ApiResponse<CocApi.Model.ClanWar>> wars = allFetchedWars
                        .Where(w => w.IsSuccessStatusCode && w.Content != null && w.Content.Clans.Any(c => c.Key == cachedClan.Tag)).ToList();

                    foreach (Client.ApiResponse<CocApi.Model.ClanWar> warResponse in wars)
                    {
                        if (cachedWars.Any((c => c.WarTag == warResponse.Content.WarTag && c.Season == cachedClan.Group.Season)))
                            continue;

                        saveWars.Add(NewWarFoundAsync(cachedClans, cachedClan, warResponse, dbContext));
                    }
                }

                await Task.WhenAll(saveWars);

                //updates = Task.WhenAll(saveWars);

                //await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

                foreach (CachedClan cachedClan in cachedClans)
                {
                    List<Client.ApiResponse<CocApi.Model.ClanWar>> wars = allFetchedWars
                        .Where(w => w.IsSuccessStatusCode && w.Content != null && w.Content.Clans.Any(c => c.Key == cachedClan.Tag)).ToList();

                    HashSet<string> downloadedWars = _downloadedWars.Single(w => w.Key == cachedClan.Group.Season).Value;

                    foreach (Client.ApiResponse<CocApi.Model.ClanWar> warResponse in wars)
                        downloadedWars.Add(warResponse.Content.WarTag);
                }

                if (_id == int.MinValue)
                    await Task.Delay(Library.Monitors.NewCwlWars.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
                else
                    await Task.Delay(Library.Monitors.NewCwlWars.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                foreach (var tag in updatingTags.Keys)
                {
                    _clansClient.UpdatingCwlWar.TryRemove(tag, out var _);
                }
            }
        }

        private readonly object _dbContextLock = new();

        private async Task NewWarFoundAsync(List<CachedClan> cachedClans, CachedClan cachedClan, Client.ApiResponse<CocApi.Model.ClanWar> war, CocApiCacheContext dbContext)
        {
            CocApi.Model.Clan? clan = cachedClans.FirstOrDefault(c => c.Tag == war.Content.Clan.Tag)?.Content;

            CocApi.Model.Clan? opponent = cachedClans.FirstOrDefault(c => c.Tag == war.Content.Opponent.Tag)?.Content;

            await _clansClient.OnClanWarAddedAsync(new CwlWarAddedEventArgs(clan, opponent, war.Content, cachedClan.Group.Content, _cancellationToken)).ConfigureAwait(false);

            TimeSpan timeToLive = await _clansClient.TimeToLiveOrDefaultAsync(war).ConfigureAwait(false);

            CachedWar cachedWar = new(war, timeToLive, war.Content.WarTag, cachedClan.Group.Season.Value);

            lock (_dbContextLock)
                dbContext.Wars.Add(cachedWar);
        }

        //private async Task InsertNewCwlWarAsync(string tag, DateTime season, List<CachedClan> cachedClans, CocApiCacheContext dbContext, CancellationToken cancellationToken)
        //{
        //    CachedWar cachedWar = await CachedWar.FromClanWarLeagueWarResponseAsync(tag, season, _clansClient, _clansApi, cancellationToken).ConfigureAwait(false);

        //    if (cachedWar.Content != null)
        //    {
        //        CocApi.Model.Clan? clan = cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag)?.Content;

        //        CocApi.Model.Clan? opponent = cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag)?.Content;

        //        CachedClan cachedClan = cachedClans.First(c => c.Group.Content != null && c.Group.Content.Rounds.Any(r => r.WarTags.Any(t => t == tag)));

        //        await _clansClient.OnClanWarAddedAsync(new CwlWarAddedEventArgs(clan, opponent, cachedWar.Content, cachedClan.Group.Content), _stopRequested);

        //        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        //        try
        //        {
        //            dbContext.Wars.Add(cachedWar);
        //        }
        //        finally
        //        {
        //            _semaphore.Release();
        //        }
        //    }
        //}

        //public new async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    await base.BeginShutdown(cancellationToken);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "NewCwlWarMonitor stopped"));
        //}

        //public void BeginShutdown()
        //{
        //    _stopRequestedTcs.SetResult(true);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "stopping"));
        //}
    }
}