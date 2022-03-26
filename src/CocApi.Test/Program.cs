using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Polly;
using CocApi.Cache;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace CocApi.Test
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception e)
            {
                Serilog.Log.Logger.Error(e, "Program crashed!");

                throw;
            }
            finally
            {
                Serilog.Log.CloseAndFlush();
            }
        }


        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, config) =>
                {
                    config.ReadFrom
                        .Configuration(context.Configuration, Serilog.Settings.Configuration.ConfigurationAssemblySource.AlwaysScanDllFiles)
                        .Enrich.FromLogContext()
                        .Enrich.With<UtcTimestampEnricher>();
                })


                .ConfigureCocApi("cocApi", (context, tokenProvider) =>
                {
                    string[] tokens = context.Configuration.GetSection("CocApi.Test:Rest:Tokens").Get<string[]>();

                    foreach (string token in tokens)
                        tokenProvider.Tokens.Add(new TokenBuilder(token, TimeSpan.FromSeconds(1)));
                })


                .ConfigureCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>((services, dbContextOptions) =>
                {
                    IConfiguration configuration = services.GetRequiredService<IConfiguration>();

                    string connection = configuration.GetConnectionString("CocApi.Test");

                    dbContextOptions.UseNpgsql(connection, b => b.MigrationsAssembly("CocApi.Test"));

                }, (cacheOptions, context) =>
                {
                    cacheOptions.ActiveWars.Enabled = true;
                    cacheOptions.ClanMembers.Enabled = true;
                    cacheOptions.Clans.Enabled = true;
                    cacheOptions.NewCwlWars.Enabled = true;
                    cacheOptions.NewWars.Enabled = true;
                    cacheOptions.Wars.Enabled = true;
                    cacheOptions.CwlWars.Enabled = true;
                    cacheOptions.Players.Enabled = true;
                })


                .ConfigureServices((hostBuilder, services) =>
                {
                    // use appsettings.json like this or configure the CacheOptions as above
                    //services.Configure<CacheOptions>(hostBuilder.Configuration.GetSection("CocApi:Cache"));

                    IConfigurationSection httpConfig = hostBuilder.Configuration.GetSection("CocApi:Rest:HttpClient");

                    // define the HttpClient named "cocApi" that CocApi will request
                    services.AddHttpClient("cocApi", config => config.BaseAddress = new Uri(httpConfig.GetValue<string>("BaseAddress")))
                    
                    // optionally configure Polly to handle timeouts and http request error handling
                    .AddPolicyHandler(RetryPolicy(httpConfig))
                    .AddPolicyHandler(TimeoutPolicy(httpConfig))
                    .AddTransientHttpErrorPolicy(builder => CircuitBreakerPolicy(builder, httpConfig))
                    .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler
                    {
                        // this property is important if you query the api very fast
                        MaxConnectionsPerServer = httpConfig.GetValue<int>("MaxConnectionsPerServer")
                    });


                    services.AddHostedService<TestService>();
                });
        }

        private static Polly.Retry.AsyncRetryPolicy<HttpResponseMessage> RetryPolicy(IConfigurationSection config)
            => HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .RetryAsync(config.GetValue<int>("Retries"));

        private static AsyncTimeoutPolicy<HttpResponseMessage> TimeoutPolicy(IConfigurationSection config)
            => Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(config.GetValue<long>("Timeout")));

        private static Polly.CircuitBreaker.AsyncCircuitBreakerPolicy<HttpResponseMessage> CircuitBreakerPolicy(
                PolicyBuilder<HttpResponseMessage> builder, 
                IConfigurationSection config)
            => builder.CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: config.GetValue<int>("HandledEventsAllowedBeforeBreaking"),
                durationOfBreak: TimeSpan.FromSeconds(config.GetValue<int>("DurationOfBreak")));
    }
}