using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Microsoft.EntityFrameworkCore;
using CocApi.Rest.Client;
using System.Collections.Generic;
using System.Linq;
using CocApi.Rest.Extensions;
using CocApi.Cache.Extensions;

namespace CocApi.Test;

class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }


    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, config) =>
            {
                config.ReadFrom
                    .Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(new ExpressionTemplate(
                        "[{@l:u4}] {UtcDateTime(@t):HH:mm} | {@m} <s:{SourceContext}>{#if EventId is not null} [{EventId.Name}]{#end}\n{#if @x is not null}{@x}\n{#end}",
                        theme: TemplateTheme.Literate));
            })

            .ConfigureCocApi((context, services, options) =>
            {
                List<string> tokenValues = context.Configuration.GetRequiredSection("CocApi.Test:Rest:Tokens").Get<List<string>>() ?? throw new InvalidOperationException("CocApi.Test:Rest:Tokens is missing from configuration.");

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
                    }
                );
            })


            .ConfigureCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>((services, dbContextOptions) =>
            {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();

                string connection = configuration.GetConnectionString("CocApi.Test") ?? throw new InvalidOperationException("Connection string 'CocApi.Test' is missing from configuration.");

                dbContextOptions.UseNpgsql(connection, b => b.MigrationsAssembly("CocApi.Test"));
            })

            .ConfigureServices((context, services) => {
                services.Configure<Rest.Models.LoginCredentials>(context.Configuration.GetRequiredSection("CocApi.Test:Rest"));
                services.AddHostedService<TokenService>();
                services.AddHostedService<SanityTestService>();
                services.AddHostedService<StopAndStartTestService>();
            });
    }
}
