using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;

namespace CocApi.Test
{
    public class CacheDbContextFactory : IDesignTimeDbContextFactory<CocApi.Cache.CacheDbContext>
    {
        public CocApi.Cache.CacheDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CocApi.Cache.CacheDbContext>();

            string connection = Environment.GetEnvironmentVariable("POSTGRES_COCAPI_TEST_DEV", EnvironmentVariableTarget.Machine)
                ?? throw new Exception($"Environment variable not found.");

            optionsBuilder.UseNpgsql(connection, b => b.MigrationsAssembly("CocApi.Test"));

            return new CocApi.Cache.CacheDbContext(optionsBuilder.Options);
        }
    }
}