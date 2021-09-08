using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class MemberMonitor : PerpetualMonitor<MemberMonitor>
    {
        private readonly PlayersApi _playersApi;
        private readonly Synchronizer _synchronizer;
        private readonly TimeToLiveProvider _ttl;
        private readonly IOptions<ClanClientOptions> _options;
        public event AsyncEventHandler<MemberUpdatedEventArgs>? MemberUpdated;

        public MemberMonitor(
            CacheDbContextFactoryProvider provider,
            PlayersApi playersApi, 
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<ClanClientOptions> options) 
            : base(provider)
        {
            _playersApi = playersApi;
            _synchronizer = synchronizer;
            _ttl = ttl;
            _options = options;
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            MonitorOptionsBase options = _options.Value.ClanMembers;

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            CachedClan cachedClan = await dbContext.Clans
                .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, cancellationToken).ConfigureAwait(false);

            _id = cachedClan != null
                ? cachedClan.Id
                : int.MinValue;

            if (cachedClan?.Content == null)
                return;

            HashSet<string> updatingTags = new();

            foreach (var member in cachedClan.Content.Members)
                if (_synchronizer.UpdatingVillage.TryAdd(member.Tag, null))
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

                // todo does this swallow exceptions?
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _synchronizer.UpdatingVillage.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        }

        //protected override async Task PollAsync()
        //{
        //    MonitorOptionsBase options = _options.Value.ClanMembers;

        //    using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

        //    CachedClan cachedClan = await dbContext.Clans
        //        .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, cancellationToken).ConfigureAwait(false);

        //    _id = cachedClan != null
        //        ? cachedClan.Id
        //        : int.MinValue;

        //    if (cachedClan?.Content == null)
        //        return;

        //    HashSet<string> updatingTags = new();

        //    foreach (var member in cachedClan.Content.Members)
        //        if (_playersClientBase.UpdatingVillage.TryAdd(member.Tag, null))
        //            updatingTags.Add(member.Tag);

        //    try
        //    {
        //        List<CachedPlayer> cachedPlayers = await dbContext.Players
        //            .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
        //            .ToListAsync(cancellationToken)
        //            .ConfigureAwait(false);

        //        List<Task> tasks = new();

        //        foreach (var member in cachedClan.Content.Members)
        //        {
        //            CachedPlayer? cachedPlayer = cachedPlayers.FirstOrDefault(p => p.Tag == member.Tag);

        //            if (cachedPlayer == null)
        //            {
        //                cachedPlayer = new CachedPlayer(member.Tag)
        //                {
        //                    Download = false
        //                };

        //                dbContext.Players.Add(cachedPlayer);
        //            }

        //            if (cachedPlayer.IsExpired)
        //                tasks.Add(MonitorMemberAsync(cachedPlayer));
        //        }

        //        foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
        //            player.ClanTag = null;

        //        await Task.WhenAll(tasks);

        //        await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
        //    }
        //    finally
        //    {
        //        foreach (string tag in updatingTags)
        //            _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
        //    }

        //    if (_id == int.MinValue)
        //        await Task.Delay(options.DelayBetweenBatches, cancellationToken).ConfigureAwait(false);
        //    else
        //        await Task.Delay(options.DelayBetweenBatchUpdates, cancellationToken).ConfigureAwait(false);
        //}

        private async Task MonitorMemberAsync(CachedPlayer cachedPlayer, CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedPlayer fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _ttl, _playersApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && MemberUpdated != null)
                await MemberUpdated(this, new(cachedClan, cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}