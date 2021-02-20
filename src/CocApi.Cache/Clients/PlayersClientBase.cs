using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class PlayersClientBase : ClientBase
    {
        private readonly PlayersApi _playersApi;

        internal readonly PlayerMonitor PlayerMontitor;

        public PlayersClientBase(PlayersApi playersApi, IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, string[] dbContextArgs) 
            : base (dbContextFactory, dbContextArgs)
        {
            _playersApi = playersApi;

            PlayerMontitor = new PlayerMonitor(DbContextFactory, dbContextArgs, _playersApi, this);
        }

        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        internal ConcurrentDictionary<string, Context.CachedItems.CachedPlayer?> UpdatingVillage { get; set; } = new ConcurrentDictionary<string, Context.CachedItems.CachedPlayer?>();

        public async Task AddOrUpdateAsync(string tag, bool download = true)
            => await AddOrUpdateAsync(new string[] { tag }, download);

        public async Task AddOrUpdateAsync(IEnumerable<string> tags, bool download = true)
        {
            HashSet<string> formattedTags = new HashSet<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<Context.CachedItems.CachedPlayer> cachedPlayers = await dbContext.Players
                .Where(c => formattedTags.Contains(c.Tag))
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (string formattedTag in formattedTags)
            {
                Context.CachedItems.CachedPlayer? trackedPlayer = cachedPlayers.FirstOrDefault(c => c.Tag == formattedTag);

                trackedPlayer ??= new Context.CachedItems.CachedPlayer(formattedTag); 

                trackedPlayer.Download = download;

                dbContext.Players.Update(trackedPlayer);
            }

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task DeleteAsync(string tag)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            while (!UpdatingVillage.TryAdd(formattedTag, null))            
                await Task.Delay(250);            

            try
            {
                Context.CachedItems.CachedPlayer cachedPlayer = await dbContext.Players.FirstOrDefaultAsync(c => c.Tag == formattedTag);

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

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .Where(i => i.Tag == formattedTag)
                .FirstAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<CachedPlayer?> GetCachedPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .Where(i => i.Tag == formattedTag)
                .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        public async Task<Player> GetOrFetchPlayerAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await _playersApi.FetchPlayerAsync(tag, cancellationToken).ConfigureAwait(false);            

            return result;
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await _playersApi.FetchPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<List<CachedPlayer>> GetCachedPlayersAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = default)
        {
            List<string> formattedTags = new List<string>();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .AsNoTracking()
                .Where(i => formattedTags.Contains(i.Tag))
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                if (!Library.Monitors.Players.IsDisabled)
                    _ = PlayerMontitor.RunAsync();
            }, cancellationToken);

            return Task.CompletedTask;
        }

        internal bool HasUpdated(Context.CachedItems.CachedPlayer stored, Context.CachedItems.CachedPlayer fetched)
        {
            if (stored.Content == null && fetched.Content != null)
                return true;

            if (stored.ExpiresAt > fetched.ExpiresAt)
                return false;

            if (stored.Content == null || fetched.Content == null)
                return false;

            return !fetched.Content.Equals(stored.Content);

            //return HasUpdated(stored.Data, fetched.Data);
        }

        //protected virtual bool HasUpdated(Player stored, Player fetched)
        //{
        //    if (!(stored.AttackWins == fetched.AttackWins
        //        && stored.BestTrophies == fetched.BestTrophies
        //        && stored.BestVersusTrophies == fetched.BestVersusTrophies
        //        && stored.BuilderHallLevel == fetched.BuilderHallLevel
        //        && stored.Clan?.Tag == fetched.Clan?.Tag
        //        && stored.DefenseWins == fetched.DefenseWins
        //        && stored.Donations == fetched.Donations
        //        && stored.DonationsReceived == fetched.DonationsReceived
        //        && stored.ExpLevel == fetched.ExpLevel
        //        && stored.League?.Id == fetched.League?.Id
        //        && stored.Name == fetched.Name
        //        && stored.Role == fetched.Role
        //        && stored.TownHallLevel == fetched.TownHallLevel
        //        && stored.TownHallWeaponLevel == fetched.TownHallWeaponLevel
        //        && stored.Trophies == fetched.Trophies
        //        && stored.VersusBattleWinCount == fetched.VersusBattleWinCount
        //        && stored.VersusBattleWins == fetched.VersusBattleWins
        //        && stored.VersusTrophies == fetched.VersusTrophies
        //        && stored.WarStars == fetched.WarStars
        //        && stored.Labels.SequenceEqual(fetched.Labels)
        //        && fetched.Labels.SequenceEqual(stored.Labels)
        //        ))
        //            return true;

        //    if (stored.LegendStatistics?.BestSeason?.Trophies != fetched.LegendStatistics?.BestSeason?.Trophies)
        //        return true;

        //    if (stored.LegendStatistics?.CurrentSeason.Trophies != fetched.LegendStatistics?.CurrentSeason.Trophies
        //        || stored.LegendStatistics?.LegendTrophies != fetched.LegendStatistics?.LegendTrophies)
        //        return true;

        //    foreach (var fetchAch in fetched.Achievements)
        //    {
        //        var storedAch = stored.Achievements.FirstOrDefault(a => 
        //            a.Name == fetchAch.Name && a.Info == fetchAch.Info && a.Village == fetchAch.Village);

        //        if (storedAch == null || storedAch.CompletionInfo != fetchAch.CompletionInfo || storedAch.Stars != fetchAch.Stars)
        //            return true;
        //    }

        //    foreach(var fetchedHero in fetched.Heroes)
        //    {
        //        var storedHero = stored.Heroes.FirstOrDefault(h => h.Name == fetchedHero.Name && h.Level == fetchedHero.Level);
        //        if (storedHero == null || storedHero.Level != fetchedHero.Level)
        //            return true;
        //    }

        //    foreach(var fetchedSpell in fetched.Spells)
        //    {
        //        var storedSpell = stored.Spells.FirstOrDefault(s => s.Name == fetchedSpell.Name && s.Level == fetchedSpell.Level);
        //        if (storedSpell == null || storedSpell.Level != fetchedSpell.Level)
        //            return true;
        //    }

        //    foreach(var fetchedTroop in fetched.Troops)
        //    {
        //        var storedTroop = stored.Troops.FirstOrDefault(t => t.Name == fetchedTroop.Name && t.Level == fetchedTroop.Level);
        //        if (storedTroop == null || storedTroop.Level != fetchedTroop.Level)
        //            return true;
        //    }

        //    return false;
        //}

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync(ApiResponse<Player> apiResponse)
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

        internal async ValueTask<TimeSpan> TimeToLiveOrDefaultAsync(Exception exception)
        {
            try
            {
                return await TimeToLiveAsync(exception).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return TimeSpan.FromMinutes(0);
            }
        }

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
            => new ValueTask<TimeSpan>(TimeSpan.FromSeconds(0));

        public virtual ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
            => new ValueTask<TimeSpan>(TimeSpan.FromMinutes(0));

        public async Task<CachedPlayer> UpdateAsync(string tag, bool download = true)
        {
            string formattedTag = Clash.FormatTag(tag);

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            CachedPlayer cachedPlayer = await dbContext.Players
                .FirstOrDefaultAsync(v => v.Tag == formattedTag)
                .ConfigureAwait(false);

            if (cachedPlayer != null && cachedPlayer.Download == download)
                return cachedPlayer;

            if (cachedPlayer == null)
            {
                cachedPlayer = new CachedPlayer(formattedTag);
                dbContext.Players.Add(cachedPlayer);
            }

            cachedPlayer.Download = download;

            await dbContext.SaveChangesAsync().ConfigureAwait(false);

            return cachedPlayer;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await PlayerMontitor.StopAsync(cancellationToken);
        }

        internal void OnPlayerUpdated(PlayerUpdatedEventArgs events)
        {
            try
            {
                PlayerUpdated?.Invoke(this, events).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Library.OnLog(this, new LogEventArgs(LogLevel.Error, "Error on player updated", e));
            }

            
        }
    }
}