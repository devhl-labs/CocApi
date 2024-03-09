using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.EntityFrameworkCore;
using CocApi.Rest.Client;
using System.Collections.Generic;
using System.Linq;
using CocApi.Rest.Extensions;
using CocApi.Cache.Extensions;
using CocApi.Cache.Services.Options;
using CocApi.Rest.DelegatingHandlers;

namespace CocApi.Test;

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

            .ConfigureCocApi((context, services, options) =>
            {
                List<string> tokenValues = context.Configuration.GetRequiredSection("CocApi.Test:Rest:Tokens").Get<List<string>>();

                ApiKeyToken[] tokens = tokenValues.Select(t => new ApiKeyToken(t, ClientUtils.ApiKeyHeader.Authorization, timeout: TimeSpan.FromSeconds(1))).ToArray();

                options.AddTokens(tokens);

                var section = context.Configuration.GetRequiredSection("CocApi:Rest:HttpClient");

                options.AddCocApiHttpClients(
                    builder: builder =>
                    {
                        builder.ConfigurePrimaryHttpMessageHandler(services => new HttpClientHandler
                        {
                            CookieContainer = services.GetRequiredService<CookieContainer>().Value,
                            // this property is important if you query the api very fast
                            MaxConnectionsPerServer = section.GetValue<int>("MaxConnectionsPerServer")
                        })
                        .AddRetryPolicy(section.GetValue<int>("Retries"))
                        .AddTimeoutPolicy(TimeSpan.FromMilliseconds(section.GetValue<long>("Timeout")))
                        .AddCircuitBreakerPolicy(
                            section.GetValue<int>("DurationOfBreak"),
                            TimeSpan.FromSeconds(section.GetValue<int>("HandledEventsAllowedBeforeBreaking")));

                        // this is important if you use real time responses
                        // when using the real time option, the response headers will still contain max-age header
                        // this message handler will remove the max-age header since it will prevent us from querying in real time
                        if (context.Configuration.GetValue<bool?>("CocApi:Cache:RealTime") == true)
                            builder.AddHttpMessageHandler(() => new PatchRealTimeResponse());
                    }
                );
            })


            .ConfigureCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>((services, dbContextOptions) =>
            {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();

                string connection = configuration.GetConnectionString("CocApi.Test");

                dbContextOptions.UseNpgsql(connection, b => b.MigrationsAssembly("CocApi.Test"));
            })

            .ConfigureServices((context, services) => {
                // configure the library to use your appsettings
                services.Configure<CacheOptions>(context.Configuration.GetRequiredSection("CocApi:Cache"));
                services.Configure<Rest.Models.LoginCredentials>(context.Configuration.GetRequiredSection("CocApi.Test:Rest"));

                services.AddHostedService<TokenService>();
                services.AddHostedService<TestService>();
                services.AddHostedService<CachingServiceTest>();
            });
    }
}
