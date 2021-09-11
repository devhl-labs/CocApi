using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CocApi.Test
{
    public class CacheDbContextFactory : IDesignTimeDbContextFactory<CocApi.Cache.CacheDbContext>
    {
        public CocApi.Cache.CacheDbContext CreateDbContext(string[] args)
        {
            string environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)               
                .Build();

            // get our environment variable name from appsettings
            string connection = configuration.GetConnectionString("CocApiTest");

            // get the connection string from our environment variables or command line
            connection = configuration.GetValue<string>(connection);

            var optionsBuilder = new DbContextOptionsBuilder<CocApi.Cache.CacheDbContext>();

            optionsBuilder.UseNpgsql(connection, b => b.MigrationsAssembly("CocApi.Test"));

            return new CocApi.Cache.CacheDbContext(optionsBuilder.Options);
        }
    }
}