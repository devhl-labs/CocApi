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
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;

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
                    // configure CocApi by naming your HttpClient and providing tokens
                    string[] tokenNames = context.Configuration.GetSection("CocApi:Rest:Tokens").Get<string[]>();

                    foreach (string tokenName in tokenNames)
                    {
                        // the real token value is in an environment variable
                        string token = context.Configuration.GetValue<string>(tokenName);

                        tokenProvider.Tokens.Add(new TokenBuilder(token, TimeSpan.FromSeconds(1)));
                    }
                })


                .ConfigureCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>((services, dbContextOptions) =>
                {
                    IConfiguration configuration = services.GetRequiredService<IConfiguration>();

                    string connection = configuration.GetConnectionString("CocApiTest");

                    // convert the variable name to the variable value
                    connection = configuration.GetValue<string>(connection);

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