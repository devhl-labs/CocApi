using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache
{
    public class ClientConfiguration
    {
        public string ConnectionString { get; }

        public int ConcurrentUpdates { get; }

        public TimeSpan DelayBetweenTasks { get; }

        public ClientConfiguration(string connectionString = "Data Source=CocApi.Cache.sqlite", int concurrentUpdates = 1, TimeSpan? delayBetweenTasks = null)
        {
            ConnectionString = connectionString;

            ConcurrentUpdates = concurrentUpdates;

            DelayBetweenTasks = delayBetweenTasks ?? TimeSpan.FromMilliseconds(250);
        }

        private static IServiceProvider? _services;
        private static readonly object _serviceProviderLock = new object();

        public virtual IServiceProvider BuildServiceProvider()
        {
            lock (_serviceProviderLock)
            {
                if (_services != null)
                    return _services;

                var services = new ServiceCollection()
                    .AddDbContext<CacheContext>(o =>
                        o.UseSqlite(ConnectionString))
                    .BuildServiceProvider();

                CacheContext cachedContext = services.GetRequiredService<CacheContext>();

                if (cachedContext.Database.GetPendingMigrations().Count() > 0)
                {
                    cachedContext.Database.Migrate();

                    cachedContext.Database.ExecuteSqlRaw(@"
drop view if exists WarWithLogStatus;
CREATE VIEW ""WarWithLogStatus"" AS 
select clans.IsWarLogPublic, wars.*
from wars
left join clans on wars.clantag = clans.tag or wars.opponenttag = clans.tag");

                    cachedContext.Database.ExecuteSqlRaw(@"
drop view if exists ClanWarWithLogStatus;
CREATE VIEW ""ClanWarWithLogStatus"" AS 
select clans.IsWarLogPublic, clans.DownloadCurrentWar, clanwars.* 
from clanwars
left join clans on clanwars.tag = clans.tag");


                cachedContext.Database.ExecuteSqlRaw(@"
drop view if exists ClanWarLogWithLogStatus;
CREATE VIEW ""ClanWarLogWithLogStatus"" AS 
select clans.IsWarLogPublic, clans.DownloadCurrentWar, warlogs.* 
from warlogs
left join clans on warlogs.tag = clans.tag");
                };

                cachedContext.Database.ExecuteSqlRaw(@"
drop view if exists ClanWarLeagueGroupWithLogStatus;
CREATE VIEW ""ClanWarLeagueGroupWithLogStatus"" AS 
select clans.DownloadCwl, groups.* 
from groups
left join clans on groups.tag = clans.tag");

                _services = services;

                return _services;
            }
        }
    }
}