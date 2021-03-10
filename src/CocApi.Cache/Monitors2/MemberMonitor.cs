using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    internal class MemberMonitor : MonitorBase
    {
        private readonly PlayersClientBase _playersClientBase;
        private readonly PlayersApi _playersApi;

        public MemberMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, PlayersClientBase playersClientBase, PlayersApi playersApi)
            : base(dbContextFactory, dbContextArgs)
        {
            _playersClientBase = playersClientBase;
            _playersApi = playersApi;
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

        //            Context.CachedItems.CachedClan cachedClan = await dbContext.Clans
        //                .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, _cancellationToken).ConfigureAwait(false);

        //            _id = cachedClan != null
        //                ? cachedClan.Id
        //                : int.MinValue;

        //            if (cachedClan?.Content == null)
        //                continue;

        //            HashSet<string> updatingTags = new();

        //            foreach (var member in cachedClan.Content.Members)
        //                if (_playersClientBase.UpdatingVillage.TryAdd(member.Tag, null))
        //                    updatingTags.Add(member.Tag);

        //            try
        //            {
        //                List<Context.CachedItems.CachedPlayer> cachedPlayers = await dbContext.Players
        //                    .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
        //                    .ToListAsync(_cancellationToken)
        //                    .ConfigureAwait(false);

        //                List<Task> tasks = new();

        //                foreach (var member in cachedClan.Content.Members)
        //                {
        //                    Context.CachedItems.CachedPlayer? cachedPlayer = cachedPlayers.FirstOrDefault(p => p.Tag == member.Tag);

        //                    if (cachedPlayer == null)
        //                    {
        //                        cachedPlayer = new Context.CachedItems.CachedPlayer(member.Tag)
        //                        {
        //                            Download = false
        //                        };

        //                        dbContext.Players.Add(cachedPlayer);
        //                    }

        //                    if (cachedPlayer.IsExpired)
        //                        tasks.Add(MonitorMemberAsync(cachedPlayer));
        //                }

        //                foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
        //                    player.ClanTag = null;

        //                Task updates = Task.WhenAll(tasks);

        //                await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

        //                await dbContext.SaveChangesAsync().ConfigureAwait(false);

        //                _cancellationToken.ThrowIfCancellationRequested();
        //            }
        //            finally
        //            {
        //                foreach (string tag in updatingTags)
        //                    _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
        //            }

        //            if (_id == int.MinValue)
        //                await Task.Delay(Library.Monitors.Members.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
        //            else
        //                await Task.Delay(Library.Monitors.Members.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
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

        //public new async Task StopAsync(CancellationToken cancellationToken)
        //{
        //    await base.BeginShutdown(cancellationToken);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "MemberMonitor stopped"));
        //}

        //public void BeginShutdown()
        //{
        //    _stopRequestedTcs.SetResult(true);

        //    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "stopping"));
        //}

        //protected override async Task PollAsync()
        //{
        //    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //    Context.CachedItems.CachedClan cachedClan = await dbContext.Clans
        //        .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, _cancellationToken).ConfigureAwait(false);

        //    _id = cachedClan != null
        //        ? cachedClan.Id
        //        : int.MinValue;

        //    if (cachedClan?.Content == null)
        //        continue;

        //    HashSet<string> updatingTags = new();

        //    foreach (var member in cachedClan.Content.Members)
        //        if (_playersClientBase.UpdatingVillage.TryAdd(member.Tag, null))
        //            updatingTags.Add(member.Tag);

        //    //try
        //    //{
        //        List<Context.CachedItems.CachedPlayer> cachedPlayers = await dbContext.Players
        //            .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
        //            .ToListAsync(_cancellationToken)
        //            .ConfigureAwait(false);

        //        List<Task> tasks = new();

        //        foreach (var member in cachedClan.Content.Members)
        //        {
        //            Context.CachedItems.CachedPlayer? cachedPlayer = cachedPlayers.FirstOrDefault(p => p.Tag == member.Tag);

        //            if (cachedPlayer == null)
        //            {
        //                cachedPlayer = new Context.CachedItems.CachedPlayer(member.Tag)
        //                {
        //                    Download = false
        //                };

        //                dbContext.Players.Add(cachedPlayer);
        //            }

        //            if (cachedPlayer.IsExpired)
        //                tasks.Add(MonitorMemberAsync(cachedPlayer));
        //        }

        //        foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
        //            player.ClanTag = null;

        //        Task updates = Task.WhenAll(tasks);

        //        await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

        //        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        //        _cancellationToken.ThrowIfCancellationRequested();
        //    }

        protected override async Task PollAsync()
        {
            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            Context.CachedItems.CachedClan cachedClan = await dbContext.Clans
                .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, _cancellationToken).ConfigureAwait(false);

            _id = cachedClan != null
                ? cachedClan.Id
                : int.MinValue;

            if (cachedClan?.Content == null)
                return;

            HashSet<string> updatingTags = new();

            foreach (var member in cachedClan.Content.Members)
                if (_playersClientBase.UpdatingVillage.TryAdd(member.Tag, null))
                    updatingTags.Add(member.Tag);

            try
            {
                List<Context.CachedItems.CachedPlayer> cachedPlayers = await dbContext.Players
                    .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
                    .ToListAsync(_cancellationToken)
                    .ConfigureAwait(false);

                List<Task> tasks = new();

                foreach (var member in cachedClan.Content.Members)
                {
                    Context.CachedItems.CachedPlayer? cachedPlayer = cachedPlayers.FirstOrDefault(p => p.Tag == member.Tag);

                    if (cachedPlayer == null)
                    {
                        cachedPlayer = new Context.CachedItems.CachedPlayer(member.Tag)
                        {
                            Download = false
                        };

                        dbContext.Players.Add(cachedPlayer);
                    }

                    if (cachedPlayer.IsExpired)
                        tasks.Add(MonitorMemberAsync(cachedPlayer));
                }

                foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
                    player.ClanTag = null;

                await Task.WhenAll(tasks);

                //Task updates = Task.WhenAll(tasks);

                //await Task.WhenAny(_stopRequestedTcs.Task, updates).ConfigureAwait(false);

                await dbContext.SaveChangesAsync().ConfigureAwait(false);

                _cancellationToken.ThrowIfCancellationRequested();
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(Library.Monitors.Members.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(Library.Monitors.Members.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task MonitorMemberAsync(Context.CachedItems.CachedPlayer cachedPlayer)
        {
            Context.CachedItems.CachedPlayer fetched = await Context.CachedItems.CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _playersClientBase.HasUpdated(cachedPlayer, fetched))
                await _playersClientBase.OnPlayerUpdatedAsync(new(cachedPlayer.Content, fetched.Content, _cancellationToken));

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}