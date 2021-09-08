using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache.Services
{
    public abstract class PerpetualMonitor<T> : PerpetualExecutionBackgroundService<T> where T : class
    {
        protected int _id = int.MinValue;

        public IDesignTimeDbContextFactory<CacheDbContext> DbContextFactory { get; }

        public string[] DbContextArgs { get; }

        protected DateTime expires = DateTime.UtcNow.AddSeconds(-3);

        protected DateTime min = DateTime.MinValue;

        protected DateTime now = DateTime.UtcNow;

        public PerpetualMonitor(CacheDbContextFactoryProvider provider)
        {
            DbContextFactory = provider.Factory;
            DbContextArgs = provider.DbContextArgs;
        }

        protected void SetDateVariables()
        {
            expires = DateTime.UtcNow.AddSeconds(-3);

            now = DateTime.UtcNow;
        }
    }
}
