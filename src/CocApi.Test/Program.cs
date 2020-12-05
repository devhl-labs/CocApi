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
            if (args.Any())
                Console.WriteLine(args);

            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostBuilder, services) =>
            {
                services.AddSingleton(TokenProviderFactory);
                services.AddSingleton(PlayersApiFactory);
                services.AddSingleton(ClansApiFactory);
                services.AddSingleton(ClientConfigurationFactory);
                services.AddSingleton(LocationsApiFactory);
                services.AddSingleton(LeaguesApiFactory);
                services.AddSingleton<LogService>();
                services.AddSingleton<PlayersClient>();
                services.AddHostedService<ClansClient>();
            })
            .ConfigureLogging(o => o.ClearProviders());

        private static ClansApi ClansApiFactory(IServiceProvider arg)
        {
            return new ClansApi(arg.GetRequiredService<TokenProvider>(), TimeSpan.FromSeconds(20));
        }

        private static Cache.ClientConfiguration ClientConfigurationFactory(IServiceProvider arg)
        {
            //this timespan is only used for updating the cache. 
            //you can keep it low so your cache updates fast
            return new Cache.ClientConfiguration(delayBetweenTasks: TimeSpan.FromSeconds(1));
        }

        private static PlayersApi PlayersApiFactory(IServiceProvider arg)
        {
            //this timespan is the max http request time even without a cancellation token
            //consider it a fallback for when you dont provide a cancellation token
            //keep it a bit longer so it doesn't undercut your cancellation tokens
            return new PlayersApi(arg.GetRequiredService<TokenProvider>(), TimeSpan.FromSeconds(20));
        }

        private static TokenProvider TokenProviderFactory(IServiceProvider arg)
        {
            List<string> tokens = new List<string>
            {
                Environment.GetEnvironmentVariable("TOKEN_0", EnvironmentVariableTarget.Machine) 
                    ?? throw new NullReferenceException("TOKEN_0 environment variable not found.")
            };

            return new TokenProvider(tokens, TimeSpan.FromSeconds(1));
        }

        private static LocationsApi LocationsApiFactory(IServiceProvider arg)
        {
            return new LocationsApi(arg.GetRequiredService<TokenProvider>(), TimeSpan.FromSeconds(20));
        }

        private static LeaguesApi LeaguesApiFactory(IServiceProvider arg)
        {
            return new LeaguesApi(arg.GetRequiredService<TokenProvider>(), TimeSpan.FromSeconds(20));
        }
    }
}