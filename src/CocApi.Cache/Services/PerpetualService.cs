using System;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache.Services
{
    public abstract class PerpetualService<T> : PerpetualExecutionBackgroundService<T> where T : class
    {
        private protected int _id = int.MinValue;
        private protected DateTime expires = DateTime.UtcNow.AddSeconds(-3);
        private protected DateTime min = DateTime.MinValue;
        private protected DateTime now = DateTime.UtcNow;


        internal IDesignTimeDbContextFactory<CacheDbContext> DbContextFactory { get; }
        internal string[] DbContextArgs { get; }


        public PerpetualService(
            CacheDbContextFactoryProvider provider, 
            TimeSpan beginExecutionAfter,
            TimeSpan delayBetweenExecutions) : base(beginExecutionAfter, delayBetweenExecutions)
        {
            DbContextFactory = provider.Factory;
            DbContextArgs = provider.DbContextArgs;
        }


        private protected void SetDateVariables()
        {
            expires = DateTime.UtcNow.AddSeconds(-3);

            now = DateTime.UtcNow;
        }
    }
}
