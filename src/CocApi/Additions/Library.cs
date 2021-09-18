using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CocApi.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        internal static void LogRequestSuccess<T>(ILogger<T> logger, HttpStatusCode httpStatusCode, DateTime start, DateTime end, string path)
            => logger.LogInformation("{0,-9} | {1} | {3}", (end - start).TotalSeconds, httpStatusCode, path);

        internal static void LogRequestFailure<T>(ILogger<T> logger, HttpStatusCode httpStatusCode, DateTime start, DateTime end, string path, string failureReason)
            => logger.LogInformation("{0,-9} | {1} | {3} | {4}", (end - start).TotalSeconds, httpStatusCode, path, failureReason);

        internal static void LogRequestException<T>(ILogger<T> logger, Exception e, DateTime start, DateTime end, string path)
            => logger.LogError(e, "{0,-9} | An exception occured while requesting {1}.", (end - start).TotalSeconds, path);

        public static IHostBuilder ConfigureCocApi(this IHostBuilder builder, string namedHttpClient, Action<HostBuilderContext, TokenContainer> tokenContainer)
        {
            builder.ConfigureServices((context, services) =>
            {
                TokenContainer tokenProviderBuilder = new();

                tokenContainer(context, tokenProviderBuilder);

                AddCocApi(services, namedHttpClient, tokenProviderBuilder);
            });

            return builder;
        }

        public static void AddCocApi(this IServiceCollection services, string namedHttpClient, Action<TokenContainer> tokenContainer)
        {
            TokenContainer tokenProviderBuilder = new();

            tokenContainer(tokenProviderBuilder);

            services.AddSingleton(tokenProviderBuilder);

            AddCocApi(services, namedHttpClient, tokenProviderBuilder);
        }

        private static void AddCocApi(this IServiceCollection services, string namedHttpClient, TokenContainer tokenContainer)
        {
            services.AddSingleton(tokenContainer);

            services.AddSingleton<TokenProvider>();

            services.AddSingleton<ClansApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(serviceProvider.GetRequiredService<ILogger<ClansApi>>(), httpClient, tokenProvider);
            });

            services.AddSingleton<LabelsApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(serviceProvider.GetRequiredService<ILogger<LabelsApi>>(), httpClient, tokenProvider);
            });

            services.AddSingleton<LeaguesApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(serviceProvider.GetRequiredService<ILogger<LeaguesApi>>(), httpClient, tokenProvider);
            });

            services.AddSingleton<LocationsApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(serviceProvider.GetRequiredService<ILogger<LocationsApi>>(), httpClient, tokenProvider);
            });

            services.AddSingleton<PlayersApi>(serviceProvider =>
            {
                IHttpClientFactory factory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = factory.CreateClient(namedHttpClient);
                TokenProvider tokenProvider = serviceProvider.GetRequiredService<TokenProvider>();
                return new(serviceProvider.GetRequiredService<ILogger<PlayersApi>>(), httpClient, tokenProvider);
            });
        }
    }
}
