using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using CocApi.Cache;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Polly;

namespace CocApi.Test
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Any())
                Console.WriteLine(args);

            CocApi.Library.HttpRequestResult += OnHttpRequestResult;

            CocApi.Cache.Library.Log += OnLog;

            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

            // configure CocApi by naming your HttpClient, providing tokens, and defining the token timeout
            .ConfigureCocApi("cocApi", tokenProvider =>
            {
                for (int i = 0; i < 10; i++)
                {
                    string token = GetEnvironmentVariable($"TOKEN_{i}");
                    
                    // you can go much lower than one second
                    tokenProvider.Tokens.Add(new TokenBuilder(token, TimeSpan.FromSeconds(1)));
                }
            })

            .ConfigureCocApiCache<ClansClient, PlayersClient>(                
                // tell the cache library how to query your database
                provider => provider.Factory = new CacheDbContextFactory(),
                    c => {
                        c.ActiveWars.Enabled = false;
                        c.ClanMembers.Enabled = false;
                        c.Clans.Enabled = true;
                        c.NewCwlWars.Enabled = true;
                        c.NewWars.Enabled = false;
                        c.Wars.Enabled = false;
                        c.CwlWars.Enabled = false;
                    },
                    p => p.Enabled = false)


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

        public static string GetEnvironmentVariable(string name)
            => Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine)
            ?? throw new Exception($"No environment variable was found with name {name}");

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

        private static Task OnLog(object sender, LogEventArgs log)
        {
            LogService.Log(LogLevel.Information, sender.GetType().Name, new string?[] { log.Message, log.Exception?.Message, log.Exception?.InnerException?.Message });

            return Task.CompletedTask;
        }
    }
}