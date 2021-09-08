using System;
using System.Net.Http;
using System.Threading.Tasks;
using CocApi.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CocApi
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }

    public delegate Task LogEventHandler(object sender, CocApi.LogEventArgs log);

    public static class Library
    {
        public static readonly Version? Version = typeof(Library).Assembly.GetName().Version;

        public const string REPOSITORY_URL = "https://github.com/devhl-labs/CocApi";

        public static event HttpRequestResultEventHandler? HttpRequestResult;

        public delegate System.Threading.Tasks.Task HttpRequestResultEventHandler(object sender, HttpRequestResultEventArgs log);

        internal static void OnHttpRequestResult(object sender, HttpRequestResultEventArgs log)
        {
            HttpRequestResult?.Invoke(sender, log);
        }

        public static IHostBuilder ConfigureCocApi(this IHostBuilder builder, string namedHttpClient, Action<HostBuilderContext, TokenProviderBuilder> tokenProvider)
        {
            builder.ConfigureServices((context, services) => AddCocApi(services, context, namedHttpClient, tokenProvider));

            return builder;
        }

        public static void AddCocApi(this IServiceCollection services, HostBuilderContext host, string namedHttpClient, Action<HostBuilderContext, TokenProviderBuilder> tokenProvider)
        {
            TokenProviderBuilder tokenProviderBuilder = new();

            tokenProvider(host, tokenProviderBuilder);

            services.AddSingleton(tokenProviderBuilder.Build());

            services.AddSingleton<ClansApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(httpClient, tokenProvider);
            });

            services.AddSingleton<LabelsApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(httpClient, tokenProvider);
            });

            services.AddSingleton<LeaguesApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(httpClient, tokenProvider);
            });

            services.AddSingleton<LocationsApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(httpClient, tokenProvider);
            });

            services.AddSingleton<PlayersApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(httpClient, tokenProvider);
            });
        }

        internal static void OnLog(object sender, LogEventArgs log)
        {
            try
            {
                Log?.Invoke(sender, log).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //throw;
            }
        }

        public static event LogEventHandler? Log;
    }
}
