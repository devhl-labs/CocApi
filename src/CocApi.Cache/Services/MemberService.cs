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
    public sealed class MemberService : PerpetualService<MemberService>
    {
        internal event AsyncEventHandler<MemberUpdatedEventArgs>? MemberUpdated;


        internal PlayersApi PlayersApi { get; }
        internal Synchronizer Synchronizer { get; }
        internal TimeToLiveProvider Ttl { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }


        public MemberService(
            CacheDbContextFactoryProvider provider,
            PlayersApi playersApi, 
            Synchronizer synchronizer,
            TimeToLiveProvider ttl,
            IOptions<CacheOptions> options) 
            : base(provider, options.Value.ClanMembers.DelayBeforeExecution, options.Value.ClanMembers.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            PlayersApi = playersApi;
            Synchronizer = synchronizer;
            Ttl = ttl;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            ServiceOptionsBase options = Options.Value.ClanMembers;

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

                // todo does this swallow exceptions?
            }
            finally
            {
                foreach (string tag in updatingTags)
                    Synchronizer.UpdatingVillage.TryRemove(tag, out _);
            }
        }

        private async Task MonitorMemberAsync(CachedPlayer cachedPlayer, CachedClan cachedClan, CancellationToken cancellationToken)
        {
            CachedPlayer fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, Ttl, PlayersApi, cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched) && MemberUpdated != null)
                await MemberUpdated(this, new(cachedClan, cachedPlayer.Content, fetched.Content, cancellationToken)).ConfigureAwait(false);

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}