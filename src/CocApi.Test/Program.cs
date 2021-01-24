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

            InitializeTokenProvider();

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

        private static void InitializeTokenProvider()
        {
            List<string> tokens = new List<string>();

            for (int i = 0; i < 10; i++)
                tokens.Add(Environment.GetEnvironmentVariable($"TOKEN_{i}", EnvironmentVariableTarget.Machine)
                    ?? throw new NullReferenceException($"TOKEN_{i} environment variable not found."));

            _tokenProvider = new TokenProvider(tokens, TimeSpan.FromSeconds(3));
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostBuilder, services) =>
            {
                services.AddSingleton(PlayersApiFactory);
                services.AddSingleton(ClansApiFactory);
                services.AddSingleton(LocationsApiFactory);
                services.AddSingleton(LeaguesApiFactory);

                services.AddSingleton<ClientConfiguration>();
                services.AddSingleton<PlayersClient>();
                services.AddHostedService<ClansClient>();

                services.AddHttpClient("cocApi", config =>
                {
                    config.BaseAddress = new Uri("https://api.clashofclans.com/v1");
                    config.Timeout = TimeSpan.FromSeconds(10);
                });


            })
            .ConfigureLogging(o => o.ClearProviders());

        private static TokenProvider _tokenProvider;

        private static ClansApi ClansApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");


            ClansApi clansApi = new ClansApi(httpClient)
            {
                GetTokenAsync = GetTokenAsync
            };

            return clansApi;
        }

        private static async ValueTask<string> GetTokenAsync() => await _tokenProvider.GetAsync();        

        private static PlayersApi PlayersApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            PlayersApi playersApi = new PlayersApi(httpClient)
            {
                GetTokenAsync = GetTokenAsync
            };
            return playersApi;
        }

        private static LeaguesApi LeaguesApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            LeaguesApi leaguesApi = new LeaguesApi(httpClient)
            {
                GetTokenAsync = GetTokenAsync
            };
            return leaguesApi;
        }

        private static LocationsApi LocationsApiFactory(IServiceProvider arg)
        {
            IHttpClientFactory factory = arg.GetRequiredService<IHttpClientFactory>();
            HttpClient httpClient = factory.CreateClient("cocApi");
            LocationsApi locationsApi = new LocationsApi(httpClient)
            {
                GetTokenAsync = GetTokenAsync
            };
            return locationsApi;
        }
    }
}