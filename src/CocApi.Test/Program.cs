using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using System.Linq;
using System.Collections.Generic;
using CocApi.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

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
                services.AddSingleton<LogService>();
                services.AddSingleton<PlayersCache>();
            })
            .ConfigureServices(services =>
            {
                services.AddHostedService<ClansCache>();
            })
            .ConfigureLogging(o => o.ClearProviders());

        private static ClansApi GetClansApi(IServiceProvider arg)
        {
            return new ClansApi(arg.GetRequiredService<TokenProvider>());
        }

        private static Cache.ClientConfiguration GetCacheConfiguration(IServiceProvider arg)
        {
            return new Cache.ClientConfiguration();
        }

        private static PlayersApi GetPlayersApi(IServiceProvider arg)
        {
            return new PlayersApi(arg.GetRequiredService<TokenProvider>());
        }

        private static TokenProvider GetTokenProvider(IServiceProvider arg)
        {
            List<string> tokens = new List<string>
            {
                Environment.GetEnvironmentVariable("TOKEN_0", EnvironmentVariableTarget.Machine) 
                    ?? throw new NullReferenceException("TOKEN_0 environment variable not found.")
            };

            return new TokenProvider(tokens, TimeSpan.FromSeconds(1));
        }
    }
}