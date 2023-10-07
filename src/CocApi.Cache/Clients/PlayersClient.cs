using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using CocApi.Rest.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using CocApi.Cache.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CocApi.Cache.Services.Options;
using CocApi.Rest.Apis;

namespace CocApi.Cache;

public class PlayersClient : ClientBase<PlayersClient>
{
    public event AsyncEventHandler<PlayerUpdatedEventArgs>? PlayerUpdated;


    public PlayersClient(
        ILogger<PlayersClient> logger,
        IPlayersApi playersApi, 
        IServiceScopeFactory scopeFactory,
        Synchronizer synchronizer,
        PlayerService playerService,
        MemberService memberService,
        IOptions<CacheOptions> options) 
    : base (logger, scopeFactory, synchronizer, options)
    {
        PlayersApi = playersApi;

        //PlayerService playerService = (PlayerService) perpetualServices.Single(p => p.GetType() == typeof(PlayerService));
        playerService.PlayerUpdated += OnPlayerUpdatedAsync;

        //MemberService memberService = (MemberService)perpetualServices.Single(p => p.GetType() == typeof(MemberService));
        memberService.MemberUpdated += OnMemberUpdatedAsync;

    }


    public IPlayersApi PlayersApi { get; }

    public async Task AddOrUpdateAsync(string tag, bool download = true) => await AddOrUpdateAsync(new string[] { tag }, download);

    public async Task AddOrUpdateAsync(IEnumerable<string> tags, bool download = true)
    {
        HashSet<string> formattedTags = new();

        foreach (string tag in tags)
            formattedTags.Add(Clash.FormatTag(tag));

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        List<CachedPlayer> cachedPlayers = await dbContext.Players
            .Where(c => formattedTags.Contains(c.Tag))
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (string formattedTag in formattedTags)
        {
            CachedPlayer? trackedPlayer = cachedPlayers.FirstOrDefault(c => c.Tag == formattedTag);

            trackedPlayer ??= new CachedPlayer(formattedTag); 

            trackedPlayer.Download = download;

            dbContext.Players.Update(trackedPlayer);
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteAsync(string tag)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        while (!Synchronizer.UpdatingVillage.TryAdd(formattedTag, null))
            await Task.Delay(250);

        try
        {
            CachedPlayer cachedPlayer = await dbContext.Players.FirstOrDefaultAsync(c => c.Tag == formattedTag);

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

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return await dbContext.Players
            .Where(i => i.Tag == formattedTag)
            .FirstAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);
    }

    public async Task<CachedPlayer?> GetCachedPlayerOrDefaultAsync(string tag, CancellationToken? cancellationToken = default)
    {
        string formattedTag = Clash.FormatTag(tag);

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        return await dbContext.Players
            .Where(i => i.Tag == formattedTag)
            .FirstOrDefaultAsync(cancellationToken.GetValueOrDefault())
            .ConfigureAwait(false);
    }

    public async Task<Player> GetOrFetchPlayerAsync(string tag, CancellationToken cancellationToken = default)
    {
        Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

        if (result == null)
            result = (await PlayersApi.FetchPlayerAsync(tag, cancellationToken).ConfigureAwait(false)).Ok();

        return result;
    }

    public async Task<Player?> GetOrFetchPlayerOrDefaultAsync(string tag, CancellationToken cancellationToken = default)
    {
        Player? result = (await GetCachedPlayerOrDefaultAsync(tag, cancellationToken).ConfigureAwait(false))?.Content;

        if (result == null)
            result = (await PlayersApi.FetchPlayerAsync(tag, cancellationToken).ConfigureAwait(false)).Ok();

        return result;
    }

    public async Task<List<CachedPlayer>> GetCachedPlayersAsync(IEnumerable<string> tags, CancellationToken? cancellationToken = default)
    {
        List<string> formattedTags = new();

        foreach (string tag in tags)
            formattedTags.Add(Clash.FormatTag(tag));

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

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

        await Library.SendConcurrentEvent(Logger, nameof(OnPlayerUpdatedAsync), async () =>
        {
            await PlayerUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
        },
        eventArgs.CancellationToken);
    }

    internal async Task OnMemberUpdatedAsync(object sender, MemberUpdatedEventArgs eventArgs)
    {
        if (PlayerUpdated == null)
            return;

        await Library.SendConcurrentEvent(Logger, nameof(OnMemberUpdatedAsync), async () =>
        {
            await PlayerUpdated.Invoke(this, eventArgs).ConfigureAwait(false);
        },
        eventArgs.CancellationToken);
    }
}