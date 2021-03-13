using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Test
{
    public class CacheDbContextFactory : IDesignTimeDbContextFactory<CocApi.Cache.CacheDbContext>
    {
        public CocApi.Cache.CacheDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CocApi.Cache.CacheDbContext>();

            optionsBuilder.UseNpgsql(Program.GetEnvironmentVariable("POSTGRES_COCAPI_TEST_DEV"), b => b.MigrationsAssembly("CocApi.Test"));

            return new CocApi.Cache.CacheDbContext(optionsBuilder.Options);
        }
    }
}