using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace CocApi.Test
{
    public class CocApiDbContext : IDesignTimeDbContextFactory<CocApi.Cache.CocApiCacheContext>
    {
        public CocApi.Cache.CocApiCacheContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CocApi.Cache.CocApiCacheContext>();

            optionsBuilder.UseNpgsql(Program.GetEnvironmentVariable("POSTGRES_COCAPI_TEST_DEV"), b => b.MigrationsAssembly("CocApi.Test"));

            return new CocApi.Cache.CocApiCacheContext(optionsBuilder.Options);
        }
    }
}