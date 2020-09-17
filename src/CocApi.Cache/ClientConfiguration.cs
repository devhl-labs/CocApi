using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace CocApi.Cache
{
    public class ClientConfiguration
    {
        public string ConnectionString { get; }

        public int ConcurrentUpdates { get; }

        public TimeSpan DelayBetweenUpdates { get; }

        public ClientConfiguration(string connectionString = "Data Source=cocapi.db", int concurrentUpdates = 1, TimeSpan? delayBetweenUpdates = null)
        {
            ConnectionString = connectionString;

            ConcurrentUpdates = concurrentUpdates;

            DelayBetweenUpdates = delayBetweenUpdates ?? TimeSpan.FromMilliseconds(50);
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
                    .AddDbContext<CachedContext>(o =>
                        o.UseSqlite(ConnectionString))
                    .BuildServiceProvider();

                CachedContext cachedContext = services.GetRequiredService<CachedContext>();

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
                }

                _services = services;

                return _services;
            }
        }
    }
}