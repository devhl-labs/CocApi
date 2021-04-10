using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Context.CachedItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    internal class MemberMonitor : MonitorBase
    {
        private readonly PlayersClientBase _playersClientBase;
        private readonly PlayersApi _playersApi;
        private readonly IOptions<ClanClientOptions> _options;

        public MemberMonitor(CacheDbContextFactoryProvider provider, PlayersClientBase playersClient, PlayersApi playersApi, IOptions<ClanClientOptions> options) 
            : base(provider)
        {
            _playersClientBase = playersClient;
            _playersApi = playersApi;
            _options = options;
        }

        protected override async Task PollAsync()
        {
            MonitorOptionsBase options = _options.Value.ClanMembers;

            using var dbContext = _dbContextFactory.CreateDbContext(_dbContextArgs);

            CachedClan cachedClan = await dbContext.Clans
                .FirstOrDefaultAsync(c => c.DownloadMembers && c.Id > _id, _cancellationToken).ConfigureAwait(false);

            _id = cachedClan != null
                ? cachedClan.Id
                : int.MinValue;

            if (cachedClan?.Content == null)
                return;

            HashSet<string> updatingTags = new();

            foreach (var member in cachedClan.Content.Members)
                if (_playersClientBase.UpdatingVillage.TryAdd(member.Tag, null))
                    updatingTags.Add(member.Tag);

            try
            {
                List<CachedPlayer> cachedPlayers = await dbContext.Players
                    .Where(p => updatingTags.Contains(p.Tag) || p.ClanTag == cachedClan.Tag)
                    .ToListAsync(_cancellationToken)
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
                        tasks.Add(MonitorMemberAsync(cachedPlayer));
                }

                foreach (var player in cachedPlayers.Where(p => p.ClanTag == cachedClan.Tag && !cachedClan.Content.Members.Any(m => m.Tag == p.Tag)))
                    player.ClanTag = null;

                await Task.WhenAll(tasks);

                await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                foreach (string tag in updatingTags)
                    _playersClientBase.UpdatingVillage.TryRemove(tag, out _);
            }

            if (_id == int.MinValue)
                await Task.Delay(options.DelayBetweenBatches, _cancellationToken).ConfigureAwait(false);
            else
                await Task.Delay(options.DelayBetweenBatchUpdates, _cancellationToken).ConfigureAwait(false);
        }

        private async Task MonitorMemberAsync(CachedPlayer cachedPlayer)
        {
            CachedPlayer fetched = await CachedPlayer.FromPlayerResponseAsync(cachedPlayer.Tag, _playersClientBase, _playersApi, _cancellationToken).ConfigureAwait(false);

            if (fetched.Content != null && CachedPlayer.HasUpdated(cachedPlayer, fetched))
                await _playersClientBase.OnPlayerUpdatedAsync(new(cachedPlayer.Content, fetched.Content, _cancellationToken));

            cachedPlayer.UpdateFrom(fetched);
        }
    }
}