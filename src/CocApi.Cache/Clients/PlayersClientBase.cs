﻿using System;
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

        internal readonly PlayerMonitor PlayerMontitor;

        internal void OnLog(object sender, LogEventArgs log) => Task.Run(() => Log?.Invoke(sender, log));


        public event LogEventHandler? Log;

        public PlayersClientBase(ClientConfiguration clientConfiguration, PlayersApi playersApi) : base (clientConfiguration)
        {
            _playersApi = playersApi;

            PlayerMontitor = new PlayerMonitor(clientConfiguration, _playersApi, this);
        }


        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        internal ConcurrentDictionary<string, byte?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, byte?>();

        public async Task<CachedPlayer> AddOrUpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext cacheContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedPlayer cachedPlayer = await cacheContext.Players.Where(v => v.Tag == formattedTag).FirstOrDefaultAsync().ConfigureAwait(false);

            cachedPlayer ??= new CachedPlayer(tag);

            cachedPlayer.Download = download;

            cacheContext.Players.Update(cachedPlayer);

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        public async Task DeleteAsync(string tag)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            while (!UpdatingVillage.TryAdd(formattedTag, new byte()))            
                await Task.Delay(500);            

            try
            {
                CachedPlayer cachedPlayer = await dbContext.Players.FirstOrDefaultAsync(c => c.Tag == formattedTag);

                if (cachedPlayer != null)
                    dbContext.Players.Remove(cachedPlayer);

                await dbContext.SaveChangesAsync();
            }
            finally
            {
                UpdatingVillage.TryRemove(formattedTag, out _);
            }
        }

        public async Task<CachedPlayer> GetCachedPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Players
                .Where(i => i.Tag == formattedTag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedPlayer?> GetCachedPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Players
                .Where(i => i.Tag == formattedTag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Player> GetOrFetchPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data;

            if (result == null)
                result = await _playersApi.FetchPlayerAsync(tag, cancellationToken).ConfigureAwait(false);            

            return result;
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data;

            if (result == null)
                result = await _playersApi.FetchPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<List<CachedPlayer>> GetCachedPlayersAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = default)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var scope = Services.CreateScope();

            CacheContext dbContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            return await dbContext.Players
                .AsNoTracking()
                .Where(i => formattedTags.Contains(i.Tag))
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            base.StartAsync();

            Task.Run(() =>
            {
                _ = PlayerMontitor.RunAsync(cancellationToken);
            }, cancellationToken);

            return Task.CompletedTask;
        }

        internal bool HasUpdated(CachedPlayer stored, CachedPlayer fetched)
        {
            if (stored.Data == null && fetched.Data != null)
                return true;

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
                && stored.Labels.SequenceEqual(fetched.Labels)
                && fetched.Labels.SequenceEqual(stored.Labels)
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
                if (storedHero == null || storedHero.Level != fetchedHero.Level)
                    return true;
            }
               
            foreach(var fetchedSpell in fetched.Spells)
            {
                var storedSpell = stored.Spells.FirstOrDefault(s => s.Name == fetchedSpell.Name && s.Level == fetchedSpell.Level);
                if (storedSpell == null || storedSpell.Level != fetchedSpell.Level)
                    return true;
            }
              
            foreach(var fetchedTroop in fetched.Troops)
            {
                var storedTroop = stored.Troops.FirstOrDefault(t => t.Name == fetchedTroop.Name && t.Level == fetchedTroop.Level);
                if (storedTroop == null || storedTroop.Level != fetchedTroop.Level)
                    return true;
            }

            return false;
        }

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
            => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(0));

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

            CacheContext cacheContext = scope.ServiceProvider.GetRequiredService<CacheContext>();

            CachedPlayer cachedPlayer = await cacheContext.Players
                .FirstOrDefaultAsync(v => v.Tag == formattedTag)
                .ConfigureAwait(false);

            if (cachedPlayer != null && cachedPlayer.Download == download)
                return cachedPlayer;

            if (cachedPlayer == null)
            {
                cachedPlayer = new CachedPlayer(formattedTag);
                cacheContext.Players.Add(cachedPlayer);
            }

            cachedPlayer.Download = download;

            await cacheContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>
            {
                base.StopAsync(cancellationToken),
                PlayerMontitor.StopAsync(cancellationToken)
            };

            await Task.WhenAll(tasks);
        }

        internal async Task<Player?> GetAsync(string tag, CancellationToken? cancellationToken = default)
        {
            CachedPlayer result = await GetCachedPlayerAsync(tag, cancellationToken);

            return result.Data;
        }

        internal void OnPlayerUpdated(Player? stored, Player fetched)
        {
            Task.Run(() => PlayerUpdated?.Invoke(this, new PlayerUpdatedEventArgs(stored, fetched)));
        }
    }
}