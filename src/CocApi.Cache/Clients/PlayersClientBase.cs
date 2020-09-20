using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Models;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class PlayersClientBase : ClientBase
    {
        private readonly PlayersApi _playersApi;

        internal readonly PlayerMonitor PlayerMontitor;

        internal void OnLog(object sender, LogEventArgs log) => Task.Run(() => Log?.Invoke(sender, log));


        public event LogEventHandler? Log;

        public PlayersClientBase(TokenProvider tokenProvider, ClientConfiguration cacheConfiguration, PlayersApi playersApi) 
            : base (tokenProvider, cacheConfiguration)
        {
            _playersApi = playersApi;

            PlayerMontitor = new PlayerMonitor(tokenProvider, cacheConfiguration, _playersApi, this);
        }


        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        internal ConcurrentDictionary<string, byte?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, byte?>();

        public async Task<CachedPlayer> AddAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

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



        public async Task<CachedPlayer> GetCachedPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            using var scope = Services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players
                .Where(i => i.Tag == tag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedPlayer?> GetCachedPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            using var scope = Services.CreateScope();

            CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            return await dbContext.Players
                .Where(i => i.Tag == tag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Player> GetOrFetchPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            return (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
                ?? await _playersApi.GetPlayerAsync(tag, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            return (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Data
                ?? await _playersApi.GetPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);
        }



        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                _ = PlayerMontitor.RunAsync(cancellationToken);
                //_ = MonitorPlayers(cancellationToken).ConfigureAwait(false);
            });

            return Task.CompletedTask;
        }

        internal bool HasUpdated(CachedPlayer stored, CachedPlayer fetched)
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

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
            => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(0));

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var scope = Services.CreateScope();

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

        internal void OnPlayerUpdated(Player stored, Player fetched)
        {
            Task.Run(() => PlayerUpdated?.Invoke(this, new PlayerUpdatedEventArgs(stored, fetched)));
        }
    }
}