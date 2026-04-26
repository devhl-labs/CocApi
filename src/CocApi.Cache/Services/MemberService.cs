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

namespace CocApi.Cache.Services;

public sealed class MemberService : ServiceBase
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
        IOptions<CacheOptions> options
        )
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.ClanMembers))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        _logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        SetDateVariables();

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        CachedClan? cachedClan = await dbContext.Clans
            .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClan != null
            ? cachedClan.Id
            : int.MinValue;

        if (cachedClan?.Content == null)
            return;

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

            var activity = CachedClan.GetActivityLevel(cachedClan);
            await foreach (var (cachedPlayer, fetched) in channel.Reader.ReadAllAsync(CancellationToken.None))
            {
                if (fetched != null)
                {
                    if (CachedPlayer.HasUpdated(cachedPlayer, fetched))
                        cachedPlayer.UpdateFrom(fetched);
                    else if (activity != CachedClan.ClanActivityLevel.Active)
                    {
                        var cap = activity == CachedClan.ClanActivityLevel.Dead ? TimeSpan.FromHours(24) : TimeSpan.FromHours(4);
                        cachedPlayer.Backoff(cap);
                    }
                }
            }

            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
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
            _logger.LogError(e, "An exception occured while updating member {tag}", cachedPlayer.Tag);
        }
        finally
        {
            writer.TryWrite((cachedPlayer, fetched));
        }
    }
}