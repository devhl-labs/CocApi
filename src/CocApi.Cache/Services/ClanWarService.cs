using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

public sealed class ClanWarService : ServiceBase
{
    private readonly ILogger<ClanWarService> _logger;


    internal IApiFactory ApiFactory { get; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider Ttl { get; }
    internal IOptions<CacheOptions> Options { get; }
    internal static bool Instantiated { get; private set; }

    public ClanWarService(
        ILogger<ClanWarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider ttl,
        IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Clans))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        Ttl = ttl;
        Options = options;
        _logger = logger;
    }


    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        SetDateVariables();

        ClanWarServiceOptions options = Options.Value.ClanWars;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        DateTime queryStart = DateTime.UtcNow;

        List<CachedClan> cachedClans = await dbContext.Clans
            .Where(c =>
                c.Id > _id &&
                    options.DownloadCurrentWar && (c.CurrentWar.ExpiresAt ?? min) < expires && (c.CurrentWar.KeepUntil ?? min) < now && c.CurrentWar.Download && c.IsWarLogPublic != false
                )
            .OrderBy(c => c.Id)
            .Take(options.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedClans.Count == options.ConcurrentUpdates
            ? cachedClans.Max(c => c.Id)
            : int.MinValue;

        List<Task> tasks = new();

        HashSet<string> updatingTags = new();

        try
        {
            foreach (CachedClan cachedClan in cachedClans)
            {
                if (!Synchronizer.UpdatingClan.TryAdd(cachedClan.Tag, cachedClan))
                    continue;

                updatingTags.Add(cachedClan.Tag);

                tasks.Add(TryUpdateAsync(cachedClan, cancellationToken));
            }

            try
            {
                await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occured while updating clans.");
            }

            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            foreach (string tag in updatingTags)
                Synchronizer.UpdatingClan.TryRemove(tag, out _);
        }
    }

    private async Task TryUpdateAsync(CachedClan cachedClan, CancellationToken cancellationToken)
    {
        try
        {
            ExtendWarTTLWhileInCwl(cachedClan);

            List<Task> tasks = new();

            IClansApi clansApi = ApiFactory.Create<IClansApi>();

            ClanWarServiceOptions options = Options.Value.ClanWars;

            Option<bool> realTime = Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value);

            if (options.DownloadCurrentWar && cachedClan.CurrentWar.Download && cachedClan.CurrentWar.IsExpired && ((cachedClan.Download && cachedClan.IsWarLogPublic == true) || !cachedClan.Download))
                tasks.Add(MonitorClanWarAsync(clansApi, cachedClan, realTime, cancellationToken));

            await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occured while updating clan {tag}", cachedClan.Tag);
        }
    }

    private async Task MonitorClanWarAsync(IClansApi clansApi, CachedClan cachedClan, Option<bool> realtime, CancellationToken cancellationToken)
    {
        try
        {
            CachedClanWar fetched = await CachedClanWar.FromCurrentWarResponseAsync(cachedClan.Tag, realtime, Ttl, clansApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedClanWar.IsNewWar(cachedClan.CurrentWar, fetched))
            {
                cachedClan.CurrentWar.Type = fetched.Content.GetWarType();

                cachedClan.CurrentWar.Added = false; // flags this war to be added by NewWarMonitor
            }

            cachedClan.CurrentWar.UpdateFrom(fetched);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occured while updating the war for clan {tag}", cachedClan.Tag);
            throw;
        }
    }

    private void ExtendWarTTLWhileInCwl(CachedClan cachedClan)
    {
        if (!Clash.IsCwlEnabled ||
            !Options.Value.CwlWars.Enabled ||
            cachedClan.CurrentWar.Content?.State == Rest.Models.WarState.InWar ||
            cachedClan.CurrentWar.Content?.State == Rest.Models.WarState.Preparation ||
            cachedClan.Group.Content == null ||
            cachedClan.Group.Content.State == Rest.Models.GroupState.Ended ||
            cachedClan.Group.Content.Season.Month < DateTime.UtcNow.Month ||
            cachedClan.Group.Content.Season.Year < DateTime.UtcNow.Year ||
            (cachedClan.Group.KeepUntil.HasValue && cachedClan.Group.KeepUntil.Value.Month > DateTime.UtcNow.Month) ||
            cachedClan.Group.StatusCode != System.Net.HttpStatusCode.OK)
            return;

        // keep currentwar around an arbitrary amount of time since we are in cwl
        cachedClan.CurrentWar.KeepUntil = DateTime.UtcNow.AddMinutes(20);
    }
}