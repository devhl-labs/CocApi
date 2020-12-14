using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
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
            //this timespan is the max http request time even without a cancellation token
            //consider it a fallback for when you dont provide a cancellation token
            //keep it a bit longer so it doesn't undercut your cancellation tokens
            return new ClansApi(TimeSpan.FromSeconds(20));
        }

        private static Cache.ClientConfiguration ClientConfigurationFactory(IServiceProvider arg)
        {
            //this timespan is only used for updating the cache. 
            //you can keep it low so your cache doesn't halt waiting for a slow api response
            return new Cache.ClientConfiguration(httpRequestTimeOut: TimeSpan.FromSeconds(5));
        }

        private static PlayersApi PlayersApiFactory(IServiceProvider arg)
        {
            return new PlayersApi(TimeSpan.FromSeconds(20));
        }

        private static TokenProvider TokenProviderFactory(IServiceProvider arg)
        {
            List<string> tokens = new List<string>
            {
                Environment.GetEnvironmentVariable("TOKEN_0", EnvironmentVariableTarget.Machine) 
                    ?? throw new NullReferenceException("TOKEN_0 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_1", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_1 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_2", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_2 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_3", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_3 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_4", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_4 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_5", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_5 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_6", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_6 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_7", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_7 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_8", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_8 environment variable not found."),
                Environment.GetEnvironmentVariable("TOKEN_9", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException("TOKEN_9 environment variable not found."),
            };

            //in production make this very fast like TimeSpan.FromMilliseconds(33)
            return new TokenProvider(tokens, TimeSpan.FromSeconds(3));
        }

        private static LocationsApi LocationsApiFactory(IServiceProvider arg)
        {
            return new LocationsApi(TimeSpan.FromSeconds(20));
        }

        private static LeaguesApi LeaguesApiFactory(IServiceProvider arg)
        {
            return new LeaguesApi(TimeSpan.FromSeconds(20));
        }
    }
}