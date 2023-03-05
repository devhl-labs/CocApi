using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IBaseApis;
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
            .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, cancellationToken).ConfigureAwait(false);

        _id = cachedClan != null
            ? cachedClan.Id
            : int.MinValue;

        if (cachedClan?.Content == null)
            return;

        HashSet<string> updatingTags = new();

        foreach (var member in cachedClan.Content.Members)
            if (Synchronizer.UpdatingVillage.TryAdd(member.Tag, null))
                updatingTags.Add(member.Tag);

        try
        {
            List<CachedPlayer> cachedPlayers = await dbContext.Players
                .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            List<Task> tasks = new();

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
                    tasks.Add(MonitorMemberAsync(cachedPlayer, cachedClan, cancellationToken));
            }

            foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
                player.ClanTag = null;

            await Task.WhenAll(tasks);

            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.UpdatingVillage.TryRemove(tag, out _);
        }
    }

    private async Task MonitorMemberAsync(CachedPlayer cachedPlayer, CachedClan cachedClan, CancellationToken cancellationToken)
    {
        IPlayersApi playersApi = ApiFactory.Create<IPlayersApi>();

        CachedPlayer fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, Ttl, playersApi, cancellationToken).ConfigureAwait(false);

        if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && MemberUpdated != null)
            await MemberUpdated(this, new(cachedClan, cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);

        cachedPlayer.UpdateFrom(fetched);
    }
}