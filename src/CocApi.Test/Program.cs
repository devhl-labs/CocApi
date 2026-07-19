using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Microsoft.EntityFrameworkCore;
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

            .ConfigureCocApi()

            .ConfigureCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>((services, dbContextOptions) =>
            {
                IConfiguration configuration = services.GetRequiredService<IConfiguration>();

                string connection = configuration["CocApi.Test:ConnectionString"] ?? throw new InvalidOperationException("'CocApi.Test:ConnectionString' is missing from configuration.");

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
