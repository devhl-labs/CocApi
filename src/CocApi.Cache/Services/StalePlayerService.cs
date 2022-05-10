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

namespace CocApi.Cache.Services
{
    public sealed class StalePlayerService : PerpetualService<StalePlayerService>
    {
        internal Synchronizer Synchronizer { get; }
        internal IOptions<CacheOptions> Options { get; }
        internal static bool Instantiated { get; private set; }
        internal TimeToLiveProvider TimeToLiveProvider { get; }


        public StalePlayerService(
            ILogger<StalePlayerService> logger,
            IServiceScopeFactory scopeFactory,
            TimeToLiveProvider timeToLiveProvider,
            Synchronizer synchronizer,
            IOptions<CacheOptions> options) 
        : base(logger, scopeFactory, options.Value.DeleteStalePlayers.DelayBeforeExecution, options.Value.DeleteStalePlayers.DelayBetweenExecutions)
        {
            Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
            IsEnabled = options.Value.DeleteStalePlayers.Enabled;
            TimeToLiveProvider = timeToLiveProvider;
            Synchronizer = synchronizer;
            Options = options;
        }


        private protected override async Task PollAsync(CancellationToken cancellationToken)
        {
            SetDateVariables();

            using var scope = ScopeFactory.CreateScope();

            CacheDbContext dbContext = scope.ServiceProvider.GetRequiredService<CacheDbContext>();

            List<CachedPlayer> cachedPlayers = await (
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