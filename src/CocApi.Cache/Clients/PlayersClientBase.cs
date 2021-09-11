using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public class PlayersClientBase : ClientBase
    {
        public PlayersClientBase(
            PlayersApi playersApi, 
            CacheDbContextFactoryProvider provider,
            PlayerService playerMonitor,
            MemberService memberMonitor,
            Synchronizer synchronizer,
            IOptions<CacheOptions> options) 
        : base (provider, synchronizer, options)
        {
            PlayersApi = playersApi;

            playerMonitor.PlayerUpdated += OnPlayerUpdatedAsync;
            memberMonitor.MemberUpdated += OnMemberUpdatedAsync;
        }

        public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;

        public PlayersApi PlayersApi { get; }

        public async Task AddOrUpdateAsync(string tag, bool download = true) => await AddOrUpdateAsync(new string[] { tag }, download);

        public async Task AddOrUpdateAsync(IEnumerable<string> tags, bool download = true)
        {
            HashSet<string> formattedTags = new();

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

            while (!Synchronizer.UpdatingVillage.TryAdd(formattedTag, null))            
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
                Synchronizer.UpdatingVillage.TryRemove(formattedTag, out _);
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
                result = await PlayersApi.FetchPlayerAsync(tag, cancellationToken).ConfigureAwait(false);            

            return result;
        }

        public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
        {
            Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

            if (result == null)
                result = await PlayersApi.FetchPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false);

            return result;
        }

        public async Task<List<CachedPlayer>> GetCachedPlayersAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = default)
        {
            List<string> formattedTags = new();

            foreach (string tag in tags)
                formattedTags.Add(Clash.FormatTag(tag));

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            return await dbContext.Players
                .AsNoTracking()
                .Where(i => formattedTags.Contains(i.Tag))
                .ToListAsync(cancellationToken.GetValueOrDefault())
                .ConfigureAwait(false);
        }

        internal async Task OnPlayerUpdatedAsync(object sender, PlayerUpdatedEventArgs eventArgs)
        {
            if (PlayerUpdated == null)
                return;

            await Library.SendConcurrentEvent(this, async () =>
            {
                await PlayerUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
            },
            eventArgs.CancellationToken);
        }

        internal async Task OnMemberUpdatedAsync(object sender, MemberUpdatedEventArgs eventArgs)
        {
            if (PlayerUpdated == null)
                return;

            await Library.SendConcurrentEvent(this, async () =>
            {
                await PlayerUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
            },
            eventArgs.CancellationToken);
        }
    }
}