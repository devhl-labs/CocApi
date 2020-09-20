﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    internal class PlayerMonitor : ClientBase
    {
        private readonly PlayersApi _playersApi;
        private readonly PlayersClientBase _playersClientBase;

        public PlayerMonitor
            (TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, PlayersApi playersApi, PlayersClientBase playersClientBase)
            : base(tokenProvider, cacheConfiguration)
        {
            _playersApi = playersApi;
            _playersClientBase = playersClientBase;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (_isRunning)
                    return;

                _isRunning = true;

                _stopRequestedTokenSource = new CancellationTokenSource();

                _playersClientBase.OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                while (cancellationToken.IsCancellationRequested == false && _stopRequestedTokenSource.IsCancellationRequested == false)
                {
                    List<Task> tasks = new List<Task>();

                    using var scope = Services.CreateScope();

                    using CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                    List<CachedPlayer> cachedPlayers = await dbContext.Players
                        .Where(v =>
                            v.Id > _id &&
                            v.Download &&
                            v.ServerExpiration < DateTime.UtcNow.AddSeconds(-3) &&
                            v.LocalExpiration < DateTime.UtcNow)
                        .OrderBy(v => v.Id)
                        .Take(ClientConfiguration.ConcurrentUpdates)
                        .ToListAsync(_stopRequestedTokenSource.Token)
                        .ConfigureAwait(false);

                    for (int i = 0; i < cachedPlayers.Count; i++)
                        tasks.Add(UpdatePlayerAsync(cachedPlayers[i]));

                    if (cachedPlayers.Count < ClientConfiguration.ConcurrentUpdates)
                        _id = 0;
                    else
                        _id = cachedPlayers.Max(v => v.Id);

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                    await Task.Delay(ClientConfiguration.DelayBetweenTasks, _stopRequestedTokenSource.Token).ConfigureAwait(false);
                }

                _isRunning = false;
            }
            catch (Exception e)
            {
                _isRunning = false;

                if (_stopRequestedTokenSource.IsCancellationRequested)
                    return;

                _playersClientBase.OnLog(this, new ExceptionEventArgs(nameof(RunAsync), e));

                if (cancellationToken.IsCancellationRequested == false)
                    _ = RunAsync(cancellationToken);
            }
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            _stopRequestedTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            _playersClientBase.OnLog(this, new LogEventArgs(nameof(StopAsync), LogLevel.Information));
        }

        internal async Task UpdatePlayerAsync(CachedPlayer cachedPlayer)
        {
            _stopRequestedTokenSource.Token.ThrowIfCancellationRequested();

            if (_playersClientBase.UpdatingVillage.TryAdd(cachedPlayer.Tag, null) == false)
                return;

            try
            {
                CachedPlayer fetched = await CachedPlayer
                    .FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, _stopRequestedTokenSource.Token);

                if (cachedPlayer.Data != null && fetched.Data != null && _playersClientBase.HasUpdated(cachedPlayer, fetched))
                    _playersClientBase.OnPlayerUpdated(cachedPlayer.Data, fetched.Data);

                cachedPlayer.UpdateFrom(fetched);
            }
            finally
            {
                _playersClientBase.UpdatingVillage.TryRemove(cachedPlayer.Tag, out _);
            }
        }
    }
}