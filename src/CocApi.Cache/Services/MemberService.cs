using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CocApi.Rest.Apis;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CocApi.Rest.Client;
using CocApi.Cache.Services.Options;
using CocApi.Cache.Logging;

namespace CocApi.Cache.Services;

public sealed class MemberService : ServiceBase<MemberServiceOptions>
{
    private readonly ILogger<MemberService> _logger;

    internal event AsyncEventHandler<MemberUpdatedEventArgs>? MemberUpdated;


    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal static bool Instantiated { get; private set; }


    public MemberService(
        ILogger<MemberService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptionsMonitor<MemberServiceOptions> memberOptions,
        ILoggerFactory loggerFactory
        )
        : base(logger, scopeFactory, memberOptions, loggerFactory)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        _logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
    }


    protected override async Task<CycleCounters> ExecuteCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        CachedClan? cachedClan = await dbContext.Clans
            .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClan != null
            ? cachedClan.Id
            : int.MinValue;

        if (cachedClan?.Content == null)
            return new CycleCounters(0, 0, 0, 0);

        HashSet<string> updatingTags = new();

        foreach (var member in cachedClan.Content.Members)
            if (Synchronizer.VillageLock.TryAcquire(member.Tag))
                updatingTags.Add(member.Tag);

        try
        {
            List<CachedPlayer> cachedPlayers = await dbContext.Players
                .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var channel = Channel.CreateUnbounded<(CachedPlayer Player, CachedPlayer? Fetched)>(new UnboundedChannelOptions { SingleReader = true });

            List<Task> allFetchTasks = new();

            foreach (var member in cachedClan.Content.Members)
            {
                CachedPlayer? cachedPlayer = cachedPlayers.FirstOrDefault(p => p.Tag == member.Tag);

                if (cachedPlayer == null)
                {
                    cachedPlayer = new CachedPlayer(member.Tag)
                    {
                        Download = false
                    };

                    dbContext.Players.Add(cachedPlayer);
                }

                if (cachedPlayer.IsExpired)
                {
                    await Synchronizer.UpdateSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                    allFetchTasks.Add(Synchronizer.WithSemaphoreAsync(TryFetchAsync(cachedPlayer, cachedClan, channel.Writer, cancellationToken)));
                }
            }

            foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
                player.ClanTag = null;

            _ = Task.WhenAll(allFetchTasks).ContinueWith(_ => channel.Writer.Complete(), TaskScheduler.Default);

            var noChangeItems = new List<(int Id, CachedPlayer Fetched, DateTime? LastChangedAt)>();

            await foreach (var (cachedPlayer, fetched) in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                if (fetched != null)
                {
                    if (CachedPlayer.HasUpdated(cachedPlayer, fetched))
                        cachedPlayer.UpdateFrom(fetched);
                    else
                        noChangeItems.Add((cachedPlayer.Id, fetched, cachedPlayer.DownloadedAt));
                }
            }

            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);

            if (noChangeItems.Count > 0)
            {
                var groups = new Dictionary<TimeSpan, List<int>>();
                foreach (var (id, fetchedItem, lastChangedAt) in noChangeItems)
                {
                    TimeSpan ttl = await Ttl.NoChangeTimeToLiveOrDefaultAsync<Rest.Models.Player>(fetchedItem, lastChangedAt).ConfigureAwait(false);
                    if (!groups.TryGetValue(ttl, out List<int>? groupIds))
                        groups[ttl] = groupIds = new();
                    groupIds.Add(id);
                }
                foreach (var (ttl, groupIds) in groups)
                {
                    DateTime keepUntil = DateTime.UtcNow.Add(ttl);
                    foreach (int[] chunk in groupIds.Chunk(100))
                    {
                        int[] ids = chunk;
                        await dbContext.Players
                            .Where(p => ids.Contains(p.Id))
                            .ExecuteUpdateAsync(s => s.SetProperty(p => p.KeepUntil, keepUntil), CancellationToken.None)
                            .ConfigureAwait(false);
                    }
                }
            }

            return new CycleCounters(
                cachedClan.Content.Members.Count,
                updatingTags.Count,
                0,
                0);
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.VillageLock.Release(tag);
        }
    }

    private async Task TryFetchAsync(CachedPlayer cachedPlayer, CachedClan cachedClan, ChannelWriter<(CachedPlayer, CachedPlayer?)> writer, CancellationToken cancellationToken)
    {
        CachedPlayer? fetched = null;
        try
        {
            IPlayersApi playersApi = ApiFactory.Create<IPlayersApi>();

            fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, Ttl, playersApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && MemberUpdated != null)
                await MemberUpdated(this, new(cachedClan, cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(CacheLogEvents.MemberUpdateFailed, e, "An exception occured while updating member {tag}", cachedPlayer.Tag);
        }
        finally
        {
            writer.TryWrite((cachedPlayer, fetched));
        }
    }
}