using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Polly;
using Microsoft.Extensions.Configuration;
using CocApi.Api;
using CocApi.Model;

namespace CocApi.Test
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Any())
                Console.WriteLine(args);

            CocApi.Library.HttpRequestResult += LogService.OnHttpRequestResult;
            CocApi.Library.Log += LogService.OnLog;
            CocApi.Cache.Library.Log += LogService.OnLog;

            IHost host = CreateHostBuilder(args).Build();

            LocationsApi locationsApi = host.Services.GetRequiredService<LocationsApi>();
            LeaguesApi leaguesApi = host.Services.GetRequiredService<LeaguesApi>();
            PlayersApi playersApi = host.Services.GetRequiredService<PlayersApi>();
            await SanityCheck(locationsApi, leaguesApi, playersApi);

            PlayersClient playersClient = host.Services.GetRequiredService<PlayersClient>();
            ClansClient clansClient = host.Services.GetRequiredService<ClansClient>();
            await AddTestItems(playersClient, clansClient);

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

            // configure CocApi by naming your HttpClient, providing tokens, and defining the token timeout
            .ConfigureCocApi("cocApi", (host, tokenProvider) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    string token = host.Configuration.GetValue<string>($"TOKEN_{i}");
                    
                    // you can go much lower than one second, fastest recommended speed is 33 milliseconds
                    tokenProvider.Tokens.Add(new TokenBuilder(token, TimeSpan.FromSeconds(1)));
                }
            })

            .ConfigureCocApiCache<ClansClient, PlayersClient, TimeToLiveProvider>(                
                // tell the cache library how to query your database
                provider => provider.Factory = new CacheDbContextFactory(),
                    c => {
                        c.ActiveWars.Enabled = false;
                        c.ClanMembers.Enabled = true;
                        c.Clans.Enabled = true;
                        c.NewCwlWars.Enabled = true;
                        c.NewWars.Enabled = false;
                        c.Wars.Enabled = false;
                        c.CwlWars.Enabled = false;
                    },
                    p => p.Enabled = false,
                    maxConcurrentEvents: 25)


            .ConfigureServices((hostBuilder, services) =>
            {
                // define the HttpClient named "cocApi" that CocApi will request
                services.AddHttpClient("cocApi", config =>
                {
                    config.BaseAddress = new Uri("https://api.clashofclans.com/v1");
                    config.Timeout = TimeSpan.FromSeconds(10);
                })
                // optionally configure Polly to handle timeouts and http request error handling
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3)))
                .AddTransientHttpErrorPolicy(builder => builder.RetryAsync(2))
                .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: 20,
                        durationOfBreak: TimeSpan.FromSeconds(30)
                ))

                // this property is important if you query the api very fast
                .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler() { MaxConnectionsPerServer = 100 });
            })
            .ConfigureLogging(o => o.ClearProviders());



        private static async Task AddTestItems(PlayersClient playersClient, ClansClient clansClient)
        {
            await playersClient.AddOrUpdateAsync("#29GPU9CUJ"); //squirrel man

            await clansClient.AddOrUpdateAsync("#8J82PV0C", downloadMembers: false); //fysb unbuckled
            await clansClient.AddOrUpdateAsync("#22G0JJR8", downloadMembers: false); //fysb
            await clansClient.AddOrUpdateAsync("#28RUGUYJU", downloadMembers: false); //devhls lab
            await clansClient.AddOrUpdateAsync("#2C8V29YJ", downloadMembers: false); // russian clan
            await clansClient.AddOrUpdateAsync("#JYULPG28", downloadMembers: false); // inphase
            await clansClient.AddOrUpdateAsync("#2P0YUY0L0", downloadMembers: false); // testing closed war log
            await clansClient.AddOrUpdateAsync("#PJYPYG9P", downloadMembers: false); // war heads
            await clansClient.AddOrUpdateAsync("#2900Y0PP2"); // crimine sas
        }

        private static async Task SanityCheck(LocationsApi locationsApi, LeaguesApi leaguesApi, PlayersApi playersApi)
        {
            var playerGlobalRankings = await locationsApi.FetchPlayerRankingAsync("global");
            var playerVersusGlobalRankings = await locationsApi.FetchPlayerVersusRankingAsync("global");
            var clanGlobalRankings = await locationsApi.FetchClanRankingOrDefaultAsync("global");
            var clanGlobalVersusRankings = await locationsApi.FetchClanVersusRankingAsync("global");

            var leagueList = await leaguesApi.FetchWarLeaguesOrDefaultAsync();

            var playerToken = await playersApi.VerifyTokenResponseAsync("#29GPU9CUJ", new VerifyTokenRequest("a"));
        }
    }
}