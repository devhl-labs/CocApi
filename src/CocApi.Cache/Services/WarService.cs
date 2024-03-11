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

public sealed class WarService : ServiceBase
{
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEndingSoon;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEndNotSeen;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarEnded;
    internal event AsyncEventHandler<WarEventArgs>? ClanWarStartingSoon;
    internal event AsyncEventHandler<ClanWarUpdatedEventArgs>? ClanWarUpdated;

    public ILogger<WarService> Logger { get; }

    internal IApiFactory ApiFactory { get; }
    internal static bool Instantiated { get; private set; }
    internal Synchronizer Synchronizer { get; }
    internal TimeToLiveProvider TimeToLiveProvider { get; }
    public IOptions<CacheOptions> Options { get; }

    private readonly HashSet<string> _unmonitoredClans = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);


    public WarService(
        ILogger<WarService> logger,
        IServiceScopeFactory scopeFactory,
        IApiFactory apiFactory,
        Synchronizer synchronizer,
        TimeToLiveProvider timeToLiveProvider,
        IOptions<CacheOptions> options)
    : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.Wars))
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
        Logger = logger;
        ApiFactory = apiFactory;
        Synchronizer = synchronizer;
        TimeToLiveProvider = timeToLiveProvider;
        Options = options;
    }

    protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Executing war service");

        SetDateVariables();

        Logger.LogInformation("Setting variables");

        WarServiceOptions options = Options.Value.Wars;

        using var scope = ScopeFactory.CreateScope();

        CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

        Logger.LogInformation("Querying wars");

        List<CachedWar> cachedWars = await dbContext.Wars
            .Where(w =>
                string.IsNullOrWhiteSpace(w.WarTag) &&
                (w.ExpiresAt ?? min) < expires &&
                (w.KeepUntil ?? min) < now &&
                !w.IsFinal &&
                w.Id > _id)
            .OrderBy(c => c.Id)
            .Take(options.ConcurrentUpdates)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        _id = cachedWars.Count == options.ConcurrentUpdates
            ? cachedWars.Max(c => c.Id)
            : int.MinValue;

        HashSet<string> tags = new();

        foreach (CachedWar cachedWar in cachedWars)
        {
            tags.Add(cachedWar.ClanTag);
            tags.Add(cachedWar.OpponentTag);
        }

        Logger.LogInformation("Querying clans");

        List<CachedClan> allCachedClans = await dbContext.Clans
            .Where(c => tags.Contains(c.Tag))
            .AsNoTracking()
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<Task> tasks = new();

        HashSet<string> updatingWar = new();

        IClansApi clansApi = ApiFactory.Create<IClansApi>();

        Logger.LogInformation("Iterating wars");

        try
        {
            foreach (CachedWar cachedWar in cachedWars)
            {
                List<CachedClan> cachedClans = allCachedClans.Where(c => c.Tag == cachedWar.ClanTag || c.Tag == cachedWar.OpponentTag).ToList();

                updatingWar.Add(cachedWar.Key);

                tasks.Add(
                    UpdateWarAsync(
                        clansApi,
                        dbContext,
                        cachedWar,
                        cachedClans.FirstOrDefault(c => c.Tag == cachedWar.ClanTag),
                        cachedClans.FirstOrDefault(c => c.Tag == cachedWar.OpponentTag),
                        cancellationToken));

                tasks.Add(SendWarAnnouncementsAsync(cachedWar, cachedClans.Select(c => c.CurrentWar).ToArray(), cancellationToken));
            }

            Logger.LogInformation("Waiting for tasks to complete");

            await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);

            Logger.LogInformation("Tasks are complete");

            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            foreach (string tag in updatingWar)
                Synchronizer.UpdatingWar.TryRemove(tag, out _);

            foreach (string tag in _unmonitoredClans)
                Synchronizer.UpdatingClan.TryRemove(tag, out _);
        }
    }

    private async Task UpdateWarAsync(
        IClansApi clansApi,
        CacheDbContext dbContext,
        CachedWar cachedWar,
        CachedClan? cachedClan,
        CachedClan? cachedOpponent,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation("Creating cached clans");

        if (cachedClan == null && cachedOpponent?.CurrentWar.IsExpired == true)
            cachedClan = await CreateCachedClan(clansApi, cachedWar.ClanTag, dbContext, cancellationToken).ConfigureAwait(false);

        if (cachedOpponent == null && cachedClan?.CurrentWar.IsExpired == true)
            cachedOpponent = await CreateCachedClan(clansApi, cachedWar.OpponentTag, dbContext, cancellationToken).ConfigureAwait(false);

        List<CachedClan?> cachedClans = new() { cachedClan, cachedOpponent };

        if (cachedClans.All(c => c?.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime) || cachedWar.EndTime.AddDays(8) < now)
        {
            cachedWar.IsFinal = true;

            return;
        }

        CachedClan? clan = cachedClans
            .OrderByDescending(c => c?.CurrentWar.ExpiresAt)
            .FirstOrDefault(c => c?.CurrentWar.PreparationStartTime == cachedWar.PreparationStartTime);

        if (cachedWar.Content != null &&
            clan?.CurrentWar.Content != null &&
            CachedWar.HasUpdated(cachedWar, clan.CurrentWar) &&
            ClanWarUpdated != null)
        {
            Logger.LogInformation("sending the war updated event");

            await ClanWarUpdated.Invoke(this, new ClanWarUpdatedEventArgs(
                    cachedWar.Content,
                    clan.CurrentWar.Content,
                    cachedClan?.Content,
                    cachedOpponent?.Content,
                    cancellationToken))
                .ConfigureAwait(false);
        }

        if (clan != null)
        {
            cachedWar.UpdateFrom(clan.CurrentWar);

            cachedWar.IsFinal = clan.CurrentWar.State == Rest.Models.WarState.WarEnded;
        }
        else if (cachedClans.All(c => 
                c != null && 
                (
                    c.CurrentWar.PreparationStartTime == DateTime.MinValue || 
                    c.CurrentWar.PreparationStartTime > cachedWar.PreparationStartTime
                )))
            cachedWar.IsFinal = true;


        Logger.LogInformation("Done sending war updated event");
    }

    private async Task<CachedClan?> CreateCachedClan(IClansApi clansApi, string tag, CacheDbContext dbContext, CancellationToken cancellationToken)
    {
        if (!Synchronizer.UpdatingClan.TryAdd(tag, null))
            return null;

        try
        {
            _unmonitoredClans.Add(tag);

            Logger.LogInformation("Creating cached clan war from current war");

            CachedClanWar cachedClanWar = await CachedClanWar
                .FromCurrentWarResponseAsync(tag, Options.Value.RealTime == null ? default : new(Options.Value.RealTime.Value), TimeToLiveProvider, clansApi, cancellationToken)
                .ConfigureAwait(false);

            cachedClanWar.Added = true;

            cachedClanWar.Download = false;

            CachedClan cachedClan = new(tag, false, false, false, false, false)
            {
                CurrentWar = cachedClanWar
            };

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                dbContext.Clans.Add(cachedClan);
            }
            finally
            {
                _semaphore.Release();
            }

            return cachedClan;
        }
        finally
        {
            Synchronizer.UpdatingClan.TryRemove(tag, out _);
        }
    }

    private async Task SendWarAnnouncementsAsync(CachedWar cachedWar, CachedClanWar[] cachedClanWars, CancellationToken cancellationToken)
    {
        try
        {
            if (cachedWar.Content == null)
                return;

            if (cachedWar.Announcements.HasFlag(Announcements.WarStartingSoon) == false &&
                now > cachedWar.Content.StartTime.AddHours(-1) &&
                now < cachedWar.Content.StartTime)
            {
                cachedWar.Announcements |= Announcements.WarStartingSoon;

                if (ClanWarStartingSoon != null)
                {
                    Logger.LogInformation("Clan war starting soon");

                    await ClanWarStartingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndingSoon) == false &&
                now > cachedWar.Content.EndTime.AddHours(-1) &&
                now < cachedWar.Content.EndTime)
            {
                cachedWar.Announcements |= Announcements.WarEndingSoon;

                if (ClanWarEndingSoon != null)
                {
                    Logger.LogInformation("Clan war ending soon");
                    await ClanWarEndingSoon.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEndNotSeen) == false &&
                cachedWar.State != Rest.Models.WarState.WarEnded &&
                now > cachedWar.EndTime &&
                now < cachedWar.EndTime.AddHours(24) &&
                cachedWar.Content.AllAttacksAreUsed() == false &&
                cachedClanWars != null &&
                cachedClanWars.All(w => w.Content != null && w.Content.PreparationStartTime != cachedWar.Content.PreparationStartTime))
            {
                cachedWar.Announcements |= Announcements.WarEndNotSeen;

                if (ClanWarEndNotSeen != null)
                {
                    Logger.LogInformation("Clan war end not seen");

                    await ClanWarEndNotSeen.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }
            }

            if (cachedWar.Announcements.HasFlag(Announcements.WarEnded) == false &&
                cachedWar.EndTime < now &&
                cachedWar.EndTime.Day <= (now.Day + 1))
            {
                cachedWar.Announcements |= Announcements.WarEnded;

                if (ClanWarEnded != null)
                {
                    Logger.LogInformation("Clan war ended");

                    await ClanWarEnded.Invoke(this, new WarEventArgs(cachedWar.Content, cancellationToken)).ConfigureAwait(false);
                }
            }

            Logger.LogInformation("done sending war announcement");
        }
        catch (Exception e)
        {
            if (!cancellationToken.IsCancellationRequested)
                Logger.LogError(e, "An exception occured while executing {typeName}.{methodName}().", GetType().Name, nameof(SendWarAnnouncementsAsync));
        }
    }
}