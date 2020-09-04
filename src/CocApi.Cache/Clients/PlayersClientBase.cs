using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class PlayersClientBase : ClientBase
    {
        private readonly PlayersApi _playersApi;

        public PlayersClientBase(TokenProvider tokenProvider, ClientConfigurationBase cacheConfiguration, PlayersApi playersApi) 
            : base (tokenProvider, cacheConfiguration)
        {
            _playersApi = playersApi;
        }

        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        internal ConcurrentDictionary<string, CachedPlayer?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, CachedPlayer?>();

        public async Task<CachedPlayer> AddAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext cacheContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedPlayer cachedPlayer = await cacheContext.Players.Where(v => v.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedPlayer != null)
                return cachedPlayer;

            cachedPlayer = new CachedPlayer(tag)
            {
                Download = download
            };
            cacheContext.Players.Update(cachedPlayer);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        public async Task<CachedPlayer> GetCacheAsync(string tag, CancellationToken? cancellationToken = default)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players.Where(i => i.Tag == tag).FirstAsync(cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
        }

        public async Task<CachedPlayer?> GetCacheOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            using var scope = _services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players.Where(i => i.Tag == tag).FirstOrDefaultAsync(cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
        }

        public async Task<Player> GetOrFetchPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            return (await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
                ?? await _playersApi.GetPlayerAsync(tag, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            return (await GetCacheOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
                ?? await _playersApi.GetPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Player?> GetPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            CachedPlayer? result = await GetCacheOrDefaultAsync(tag, cancellationToken);

            return result?.Data;
        }

        private int _playerId = 0;

        public Task RunAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (IsRunning)
                        return;

                    IsRunning = true;

                    _stopRequestedTokenSource = new CancellationTokenSource();

                    OnLog(this, new LogEventArgs(nameof(RunAsync), LogLevel.Information));

                    while (cancellationToken.IsCancellationRequested == false && _stopRequestedTokenSource.IsCancellationRequested == false)
                    {
                        List<Task> tasks = new List<Task>();

                        using var scope = _services.CreateScope();

                        using CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                        List<CachedPlayer> cachedPlayers = await dbContext.Players.Where(v =>
                            v.Id > _playerId).OrderBy(v => v.Id).Take(_cacheConfiguration.ConcurrentUpdates).ToListAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

                        for (int i = 0; i < cachedPlayers.Count; i++)
                            tasks.Add(UpdatePlayerAsync(cachedPlayers[i]));

                        if (cachedPlayers.Count < _cacheConfiguration.ConcurrentUpdates)
                            _playerId = 0;
                        else
                            _playerId = cachedPlayers.Max(v => v.Id);

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
            });

            return Task.CompletedTask;
        }

        private bool HasUpdated(CachedPlayer stored, CachedPlayer fetched)
        {
            if (stored.ServerExpiration > fetched.ServerExpiration)
                return false;

            if (stored.Data == null || fetched.Data == null)
                return false;

            return HasUpdated(stored.Data, fetched.Data);
        }

        protected virtual bool HasUpdated(Player stored, Player fetched)
        {
            if (!(stored.AttackWins == fetched.AttackWins
                && stored.BestTrophies == fetched.BestTrophies
                && stored.BestVersusTrophies == fetched.BestVersusTrophies
                && stored.BuilderHallLevel == fetched.BuilderHallLevel
                && stored.Clan?.Tag == fetched.Clan?.Tag
                && stored.DefenseWins == fetched.DefenseWins
                && stored.Donations == fetched.Donations
                && stored.DonationsReceived == fetched.DonationsReceived
                && stored.ExpLevel == fetched.ExpLevel
                && stored.League?.Id == fetched.League?.Id
                && stored.Name == fetched.Name
                && stored.Role == fetched.Role
                && stored.TownHallLevel == fetched.TownHallLevel
                && stored.TownHallWeaponLevel == fetched.TownHallWeaponLevel
                && stored.Trophies == fetched.Trophies
                && stored.VersusBattleWinCount == fetched.VersusBattleWinCount
                && stored.VersusBattleWins == fetched.VersusBattleWins
                && stored.VersusTrophies == fetched.VersusTrophies
                && stored.WarStars == fetched.WarStars
                && stored.Labels.Except(fetched.Labels).Count() == 0
                ))
                    return true;

            if (stored.LegendStatistics?.BestSeason?.Trophies != fetched.LegendStatistics?.BestSeason?.Trophies)
                return true;

            if (stored.LegendStatistics?.CurrentSeason.Trophies != fetched.LegendStatistics?.CurrentSeason.Trophies
                || stored.LegendStatistics?.LegendTrophies != fetched.LegendStatistics?.LegendTrophies)
                return true;

            foreach (var fetchAch in fetched.Achievements)
            {
                var storedAch = stored.Achievements.FirstOrDefault(a => 
                    a.Name == fetchAch.Name && a.Info == fetchAch.Info && a.Village == fetchAch.Village);
                    
                if (storedAch == null || storedAch.CompletionInfo != fetchAch.CompletionInfo || storedAch.Stars != fetchAch.Stars)
                    return true;
            }

            foreach(var fetchedHero in fetched.Heroes)
            {
                var storedHero = stored.Heroes.FirstOrDefault(h => h.Name == fetchedHero.Name && h.Level == fetchedHero.Level);
                if (storedHero == null)
                    return true;
            }
               
            foreach(var fetchedSpell in fetched.Spells)
            {
                var storedSpell = stored.Spells.FirstOrDefault(s => s.Name == fetchedSpell.Name && s.Level == fetchedSpell.Level);
                if (storedSpell == null)
                    return true;
            }
              
            foreach(var fetchedTroop in fetched.Troops)
            {
                var storedTroop = stored.Troops.FirstOrDefault(t => t.Name == fetchedTroop.Name && t.Level == fetchedTroop.Level);
                if (storedTroop == null)
                    return true;
            }

            return false;
        }

        public virtual TimeSpan TimeToLive(ApiResponse<Player> apiResponse)
            => TimeSpan.FromSeconds(0);

        public virtual TimeSpan TimeToLive(Exception exception)
            => TimeSpan.FromMinutes(0);

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = _services.CreateScope();

            CachedContext cacheContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            CachedPlayer cachedPlayer = await cacheContext.Players.Where(v => 
                v.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            if (cachedPlayer != null && cachedPlayer.Download == download)
                return cachedPlayer;

            cachedPlayer ??= new CachedPlayer(formattedTag);
            cachedPlayer.Download = download;
            cacheContext.Players.Update(cachedPlayer);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        internal async Task<Player?> GetAsync(string tag, CancellationToken? cancellationToken = default)
        {
            CachedPlayer result = await GetCacheAsync(tag, cancellationToken);

            return result.Data;
        }

        internal void OnPlayerUpdated(Player stored, Player fetched)
        {
            try
            {
                _ = PlayerUpdated?.Invoke(this, new PlayerUpdatedEventArgs(stored, fetched));
            }
            catch (Exception)
            {
            }
        }

        internal async Task UpdatePlayerAsync(CachedPlayer cachedPlayer)
        {
            if (cachedPlayer.IsServerExpired() == false || cachedPlayer.IsLocallyExpired() == false || _stopRequestedTokenSource.IsCancellationRequested)
                return;

            if (UpdatingVillage.TryAdd(cachedPlayer.Tag, cachedPlayer) == false)
                return;

            try
            {
                using var scope = _services.CreateScope();

                CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

                CachedPlayer fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, this, _playersApi, _stopRequestedTokenSource.Token);
                    
                if (cachedPlayer.Data != null && fetched.Data != null && HasUpdated(cachedPlayer, fetched))
                    OnPlayerUpdated(cachedPlayer.Data, fetched.Data);

                cachedPlayer.UpdateFrom(fetched);

                dbContext.Players.Update(cachedPlayer);

                await dbContext.SaveChangesAsync(_stopRequestedTokenSource.Token);
            }
            finally
            {
                UpdatingVillage.TryRemove(cachedPlayer.Tag, out _);
            }
        }
    }
}