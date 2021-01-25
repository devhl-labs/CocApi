using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using System.Linq;
using System.Collections.Generic;
using CocApi.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace CocApi.Test
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Any())
                Console.WriteLine(args);

            CocApi.Requests.HttpRequestResult += OnHttpRequestResult;

            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static Task OnHttpRequestResult(object sender, HttpRequestResultEventArgs log)
        {
            string seconds = ((int)log.HttpRequestResult.Elapsed.TotalSeconds).ToString();

            if (log.HttpRequestResult is HttpRequestException exception)
                LogService.Log(LogLevel.Warning, sender.GetType().Name, seconds, exception.Path, exception.Message, exception.InnerException?.Message);
            else if (log.HttpRequestResult is HttpRequestNonSuccess nonSuccess)            
                LogService.Log(LogLevel.Debug, sender.GetType().Name, seconds, nonSuccess.Path, nonSuccess.Reason);            
            else
                LogService.Log(LogLevel.Information, sender.GetType().Name, seconds, log.HttpRequestResult.Path);

            return Task.CompletedTask;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostBuilder, services) =>
            {
                services.AddSingleton(PlayersApiFactory);
                services.AddSingleton(ClansApiFactory);
                services.AddSingleton(LocationsApiFactory);
                services.AddSingleton(LeaguesApiFactory);
                services.AddSingleton(TokenProviderFactory);

                services.AddSingleton<ClientConfiguration>();
                services.AddSingleton<PlayersClient>();
                services.AddHostedService<ClansClient>();
                
                services.AddHttpClient("cocApi", config =>
                {
                    config.BaseAddress = new Uri("https://api.clashofclans.com/v1");
                    config.Timeout = TimeSpan.FromSeconds(10);                    
                })
                .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler() { MaxConnectionsPerServer = 100 });
            })
            .ConfigureLogging(o => o.ClearProviders());

        private static TokenProvider TokenProviderFactory(IServiceProvider arg)
        {
            List<string> tokens = new List<string>();

            for (int i = 0; i < 10; i++)
                tokens.Add(Environment.GetEnvironmentVariable($"TOKEN_{i}", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException($"TOKEN_{i} environment variable not found."));

            TokenProvider tokenProvider = new TokenProvider(tokens, TimeSpan.FromSeconds(3));

            return tokenProvider;
        }

        private static ClansApi ClansApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            TokenProvider tokenProvider = arg.GetRequiredService<TokenProvider>();
            ClansApi clansApi = new ClansApi(httpClient, tokenProvider);

            return clansApi;
        }       

        private static PlayersApi PlayersApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            TokenProvider tokenProvider = arg.GetRequiredService<TokenProvider>();
            PlayersApi playersApi = new PlayersApi(httpClient, tokenProvider);

            return playersApi;
        }

        private static LeaguesApi LeaguesApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            TokenProvider tokenProvider = arg.GetRequiredService<TokenProvider>();
            LeaguesApi leaguesApi = new LeaguesApi(httpClient, tokenProvider);

            return leaguesApi;
        }

        private static LocationsApi LocationsApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            TokenProvider tokenProvider = arg.GetRequiredService<TokenProvider>();
            LocationsApi locationsApi = new LocationsApi(httpClient, tokenProvider);

            return locationsApi;
        }
    }
}