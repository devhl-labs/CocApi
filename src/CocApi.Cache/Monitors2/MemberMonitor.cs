using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context;
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

        public async Task RunAsync()
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "MemberMonitor running"));

                while (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                    DateTime expires = DateTime.UtcNow.AddSeconds(-3);

                    Context.CachedItems.CachedClan cachedClan = await dbContext.Clans
                        .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, _stopRequestedTokenSource.Token).ConfigureAwait(false);

                    _id = cachedClan != null
                        ? cachedClan.Id
                        : int.MinValue;

                    if (cachedClan?.Content == null)
                        continue;

                    HashSet<string> updatingTags = new();

                    foreach (var member in cachedClan.Content.Members)
                        if (_playersClientBase.UpdatingVillage.TryAdd(member.Tag, null))
                            updatingTags.Add(member.Tag);

                    try
                    {
                        List<Context.CachedItems.CachedPlayer> cachedPlayers = await dbContext.Players
                            .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
                            .ToListAsync(_stopRequestedTokenSource.Token)
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
                                tasks.Add(MonitorMemberAsync(cachedPlayer, _stopRequestedTokenSource.Token));
                        }

                        foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
                            player.ClanTag = null;

                        try
                        {
                            await Task.WhenAll(tasks).ConfigureAwait(false);
                        }
                        catch (Exception)
                        {
                        }

                        await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);
                    }
                    finally 
                    { 
                        foreach(string tag in updatingTags)
                            _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
                    }

                    await Task.Delay(Library.MemberMonitorOptions.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                _isRunning = false;
            }
            catch (Exception e)
            {
                _isRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                if (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    Library.OnLog(this, new LogEventArgs(LogLevel.Error, "MemberMonitor error", e));

                    _ = RunAsync();
                }
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "MemberMonitor stopped"));
        }

        private async Task MonitorMemberAsync(Context.CachedItems.CachedPlayer cachedPlayer, CancellationToken cancellationToken)
        {
            Context.CachedItems.CachedPlayer fetched = await Context.CachedItems.CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _playersClientBase.HasUpdated(cachedPlayer, fetched))
                _playersClientBase.OnPlayerUpdated(new(cachedPlayer.Content, fetched.Content));

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}