using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services
{
    public sealed class StalePlayerService : ServiceBase
    {
        internal Synchronizer Synchronizer { get; }
        internal static bool Instantiated { get; private set; }
        internal TimeToLiveProvider TimeToLiveProvider { get; }


        public StalePlayerService(
            ILogger<StalePlayerService> logger,
            IServiceScopeFactory scopeFactory,
            TimeToLiveProvider timeToLiveProvider,
            Synchronizer synchronizer,
            IOptions<CacheOptions> options)
        : base(logger, scopeFactory, Microsoft.Extensions.Options.Options.Create(options.Value.DeleteStalePlayers))
        {
            Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
            TimeToLiveProvider = timeToLiveProvider;
            Synchronizer = synchronizer;
        }


        protected override async Task ExecuteScheduledTaskAsync(CancellationToken cancellationToken)
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