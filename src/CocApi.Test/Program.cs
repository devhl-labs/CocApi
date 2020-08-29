using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using CocApi.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CocApi.Test
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Count() > 0)
                Console.WriteLine(args);

            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostBuilder, services) =>
            {
                services.AddSingleton(GetTokenProvider);
                services.AddSingleton(GetPlayersApi);
                services.AddSingleton(GetClansApi);
                services.AddSingleton(GetCacheConfiguration);
                services.AddSingleton(new LogService());
                services.AddSingleton<PlayersCache>();
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<ClansCache>();
            })
            .ConfigureLogging(c => c.ClearProviders());

        private static ClansApi GetClansApi(IServiceProvider arg)
        {
            return new ClansApi(arg.GetRequiredService<TokenProvider>());
        }

        private static CacheConfiguration GetCacheConfiguration(IServiceProvider arg)
        {
            return new CacheConfiguration();
        }

        private static PlayersApi GetPlayersApi(IServiceProvider arg)
        {
            return new PlayersApi(arg.GetRequiredService<TokenProvider>());
        }

        private static TokenProvider GetTokenProvider(IServiceProvider arg)
        {
            List<string> tokens = new List<string>
            {
                File.ReadAllText(@"E:\Desktop\token.txt")
            };

            return new TokenProvider(tokens, TimeSpan.FromSeconds(1));
        }
    }
}