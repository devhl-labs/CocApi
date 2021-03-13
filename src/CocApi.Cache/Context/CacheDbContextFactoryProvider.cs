using System;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Cache
{
    public sealed class CacheDbContextFactoryProvider
    {
        public IDesignTimeDbContextFactory<CacheDbContext> Factory { get; set; }

        public string[] DbContextArgs { get; set; } = Array.Empty<string>();

        internal CacheDbContextFactoryProvider()
        {

        }

        public CacheDbContextFactoryProvider(IDesignTimeDbContextFactory<CacheDbContext> dbContextFactory, string[] dbContextArgs)
        {
            Factory = dbContextFactory;
            DbContextArgs = dbContextArgs;
        }
    }
}
