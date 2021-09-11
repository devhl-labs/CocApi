using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CocApi.Cache
{
    public sealed class DeleteStalePlayerService : PerpetualService<DeleteStalePlayerService>
    {
        internal Synchronizer Synchronizer { get; }
        internal PlayersApi PlayersApi { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }
        internal TimeToLiveProvider TimeToLiveProvider { get; }


        public DeleteStalePlayerService(
            CacheDbContextFactoryProvider provider, 
            TimeToLiveProvider timeToLiveProvider,
            Synchronizer synchronizer,
            PlayersApi playersApi,
            IOptions<CacheOptions> options) 
        : base(provider, options.Value.DeleteStalePlayers.DelayBeforeExecution, options.Value.DeleteStalePlayers.DelayBetweenExecutions)
        {
            Instantiated = Library.EnsureSingleton(Instantiated);
            TimeToLiveProvider = timeToLiveProvider;
            Synchronizer = synchronizer;
            PlayersApi = playersApi;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            using var dbContext = DbContextFactory.CreateDbContext(DbContextArgs);

            List<Context.CachedItems.CachedPlayer> cachedPlayers = await (
                from p in dbContext.Players
                join c in dbContext.Clans on p.ClanTag equals c.Tag
                into p_c
                from c2 in p_c.DefaultIfEmpty()
                where
                    p.Download == false &&
                    (p.ExpiresAt ?? min) < DateTime.UtcNow.AddMinutes(-10) &&
                    (p.ClanTag == null || (c2 == null || c2.DownloadMembers == false))
                select p
            ).ToListAsync(cancellationToken).ConfigureAwait(false);

            dbContext.RemoveRange(cachedPlayers);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}