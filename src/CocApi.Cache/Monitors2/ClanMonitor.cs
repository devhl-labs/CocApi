﻿using System;
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
    internal class ClanMonitor : MonitorBase
    {
        private readonly ClansApi _clansApi;
        private readonly ClansClientBase _clansClient;

        public ClanMonitor(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs, ClansApi clansApi, ClansClientBase clansClientBase)
            : base(dbContextFactory, dbContextArgs)
        {
            _clansApi = clansApi;
            _clansClient = clansClientBase;
        }

        public async Task RunAsync()
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                Library.OnLog(this, new LogEventArgs(LogLevel.Information, "ClanMonitor running"));

                while (_stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

                    DateTime expires = DateTime.UtcNow.AddSeconds(-3);

                    List<Context.CachedItems.CachedClan> cachedClans = await dbContext.Clans
                        .Where(c =>
                            (
(c.ExpiresAt < expires            && c.KeepUntil < DateTime.UtcNow              && c.Download) ||
(c.CurrentWar.ExpiresAt < expires && c.CurrentWar.KeepUntil < DateTime.UtcNow   && c.CurrentWar.Download && c.IsWarLogPublic == true) ||
(c.Group.ExpiresAt < expires      && c.Group.KeepUntil < DateTime.UtcNow        && c.Group.Download) ||
(c.WarLog.ExpiresAt < expires     && c.WarLog.KeepUntil < DateTime.UtcNow       && c.WarLog.Download     && c.IsWarLogPublic == true) 
                            )
                            &&
                            c.Id > _id)
                        .OrderBy(c => c.Id)
                        .Take(Library.ClanMonitorOptions.ConcurrentUpdates)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    _id = cachedClans.Count == Library.ClanMonitorOptions.ConcurrentUpdates
                        ? cachedClans.Max(c => c.Id)
                        : int.MinValue;

                    List<Task> tasks = new();

                    HashSet<string> updatingTags = new();

                    try
                    {
                        foreach (Context.CachedItems.CachedClan cachedClan in cachedClans)
                        {
                            if (!_clansClient.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                                continue;

                            updatingTags.Add(cachedClan.Tag);

                            if (cachedClan.Download && cachedClan.IsExpired)                    
                                tasks.Add(MonitorClanAsync(cachedClan, _stopRequestedTokenSource.Token));

                            if (cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && cachedClan.IsWarLogPublic == true)
                                tasks.Add(MonitorClanWarAsync(cachedClan, _stopRequestedTokenSource.Token));

                            if (cachedClan.WarLog.Download && cachedClan.WarLog.IsExpired && cachedClan.IsWarLogPublic == true)
                                tasks.Add(MonitorWarLogAsync(cachedClan, _stopRequestedTokenSource.Token));

                            if (cachedClan.Group.Download && cachedClan.Group.IsExpired)
                                tasks.Add(MonitorGroupAsync(cachedClan, _stopRequestedTokenSource.Token));
                        }

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
                            _clansClient.UpdatingClan.TryRemove(tag, out _);
                    }

                    await Task.Delay(Library.ClanMonitorOptions.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
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
                    Library.OnLog(this, new LogEventArgs(LogLevel.Information, "ClanMonitor error", e));

                    _ = RunAsync();
                }
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            Library.OnLog(this, new LogEventArgs(LogLevel.Information, "ClanMonitor stopped"));
        }

        private async Task MonitorClanAsync(Context.CachedItems.CachedClan cachedClan, CancellationToken cancellationToken)
        {
            Context.CachedItems.CachedClan fetched = await Context.CachedItems.CachedClan.FromClanResponseAsync(cachedClan.Tag, _clansClient, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _clansClient.HasUpdated(cachedClan, fetched))
                await _clansClient.OnClanUpdatedAsync(new ClanUpdatedEventArgs(cachedClan.Content, fetched.Content));

            cachedClan.UpdateFrom(fetched);
        }

        private async Task MonitorClanWarAsync(Context.CachedItems.CachedClan cachedClan, CancellationToken cancellationToken)
        {
            Context.CachedItems.CachedClanWar? fetched = await Context.CachedItems.CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, _clansClient, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && Context.CachedItems.CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }

        private async Task MonitorWarLogAsync(Context.CachedItems.CachedClan cachedClan, CancellationToken cancellationToken)
        {
            Context.CachedItems.CachedClanWarLog fetched = await Context.CachedItems.CachedClanWarLog.FromClanWarLogResponseAsync(cachedClan.Tag, _clansClient, _clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && _clansClient.HasUpdated(cachedClan.WarLog, fetched))
                await _clansClient.OnClanWarLogUpdatedAsync(new ClanWarLogUpdatedEventArgs(cachedClan.WarLog.Content, fetched.Content, cachedClan.Content));

            cachedClan.WarLog.UpdateFrom(fetched);
        }

        private async Task MonitorGroupAsync(Context.CachedItems.CachedClan cachedClan, CancellationToken cancellationToken)
        {
            Context.CachedItems.CachedClanWarLeagueGroup? fetched = await Context.CachedItems.CachedClanWarLeagueGroup
                .FromClanWarLeagueGroupResponseAsync(cachedClan.Tag, _clansClient, _clansApi, cancellationToken)
                .ConfigureAwait(false);

            if (fetched.Content != null) {
                if (_clansClient.HasUpdated(cachedClan.Group, fetched))
                    await _clansClient.OnClanWarLeagueGroupUpdatedAsync(new ClanWarLeagueGroupUpdatedEventArgs(cachedClan.Group.Content, fetched.Content));

                if (cachedClan.Group.Content == null || cachedClan.Group.Season != fetched.Season || cachedClan.Group.Content.Rounds.Count != fetched.Content.Rounds.Count)
                    cachedClan.Group.Added = false;
            }

            cachedClan.Group.UpdateFrom(fetched);            
        }
    }
}