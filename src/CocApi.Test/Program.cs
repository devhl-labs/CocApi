using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Polly;
using CocApi.Cache;
using Serilog;

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

                .ConfigureServices((hostBuilder, services) =>
                {
                    // configure CocApi by naming your HttpClient, providing tokens, and defining the token timeout
                    services.AddCocApi("cocApi", tokenProvider =>
                    {
                        string[] tokenNames = hostBuilder.Configuration.GetSection("CocApi:Rest:Tokens").Get<string[]>();

                        foreach (string tokenName in tokenNames)
                        {
                            // the real token value is in an environment variable
                            string token = hostBuilder.Configuration.GetValue<string>(tokenName);

                            tokenProvider.Tokens.Add(new TokenBuilder(token, TimeSpan.FromSeconds(1)));
                        }
                    });


                    services.AddCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>(
                        provider =>
                        {
                            // tell the cache library how to query your database
                            provider.Factory = new CacheDbContextFactory();
                        },
                        cacheOptions =>
                        {
                            cacheOptions.ActiveWars.Enabled = true;
                            cacheOptions.ClanMembers.Enabled = true;
                            cacheOptions.Clans.Enabled = true;
                            cacheOptions.NewCwlWars.Enabled = true;
                            cacheOptions.NewWars.Enabled = true;
                            cacheOptions.Wars.Enabled = true;
                            cacheOptions.CwlWars.Enabled = true;
                            cacheOptions.Players.Enabled = true;
                        }
                    );


                    // use appsettings.json like this or configure the CacheOptions as above
                    //services.Configure<CacheOptions>(hostBuilder.Configuration.GetSection("CocApi:Cache"));


                    IConfigurationSection httpConfig = hostBuilder.Configuration.GetSection("CocApi:Rest:HttpClient");

                    // define the HttpClient named "cocApi" that CocApi will request
                    services.AddHttpClient("cocApi", config =>
                    {
                        config.BaseAddress = new Uri(httpConfig.GetValue<string>("BaseAddress"));
                        config.Timeout = TimeSpan.FromSeconds(httpConfig.GetValue<int>("Timeout"));
                    })
                    // optionally configure Polly to handle timeouts and http request error handling
                    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(httpConfig.GetValue<int>("RetryTimeout"))))
                    .AddTransientHttpErrorPolicy(builder => builder.RetryAsync(httpConfig.GetValue<int>("Retry")))
                    .AddTransientHttpErrorPolicy(builder => builder.CircuitBreakerAsync(
                            handledEventsAllowedBeforeBreaking: httpConfig.GetValue<int>("HandledEventsAllowedBeforeBreaking"),
                            durationOfBreak: TimeSpan.FromSeconds(httpConfig.GetValue<int>("DurationOfBreak"))
                    ))

                    // this property is important if you query the api very fast
                    .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler()
                    {
                        MaxConnectionsPerServer = httpConfig.GetValue<int>("MaxConnectionsPerServer")
                    });

                    services.AddHostedService<TestService>();
                });
        }
    }
}