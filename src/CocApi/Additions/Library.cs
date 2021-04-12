using System;
using System.Net.Http;
using CocApi.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CocApi
{
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

        public static IHostBuilder ConfigureCocApi(this IHostBuilder builder, string namedHttpClient, Action<TokenProviderBuilder> tokenProvider)
        {
            builder.ConfigureServices((context, services) => AddCocApi(services, namedHttpClient, tokenProvider));

            return builder;
        }

        public static void AddCocApi(this IServiceCollection services, string namedHttpClient, Action<TokenProviderBuilder> tokenProvider)
        {
            TokenProviderBuilder tokenProviderBuilder = new();

            tokenProvider(tokenProviderBuilder);

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
    }
}
