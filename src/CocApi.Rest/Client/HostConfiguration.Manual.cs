using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CocApi.Rest.Extensions;

namespace CocApi.Rest.Client;

public partial class HostConfiguration
{
    /// <summary>
    /// Registers a deferred factory for <see cref="TokenContainer{ApiKeyToken}"/> that loads tokens
    /// from <c>CocApi:Rest:Tokens</c> in configuration. This always runs, but if the user calls
    /// <c>options.AddTokens(...)</c> in their <c>ConfigureCocApi</c> callback, that descriptor is
    /// registered last and DI resolves it instead — this factory is never invoked in that case.
    /// </summary>
    partial void OnHostConfigurationCreated()
    {
        _services.AddSingleton<TokenContainer<ApiKeyToken>>(sp =>
        {
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            string[]? tokenValues = config.GetSection("CocApi:Rest:Tokens").Get<string[]>();

            if (tokenValues == null || tokenValues.Length == 0)
                throw new InvalidOperationException(
                    "No CocApi tokens were registered. Either call options.AddTokens(...) in ConfigureCocApi, " +
                    "or populate 'CocApi:Rest:Tokens' in configuration (e.g. appsettings.json or environment variables).");

            int? timeoutMs = config.GetValue<int?>("CocApi:Rest:HttpClient:TokenTimeout");
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMs ?? 33);
            return new TokenContainer<ApiKeyToken>(
                tokenValues.Select(t => new ApiKeyToken(t, ClientUtils.ApiKeyHeader.Authorization, timeout: timeout)));
        });
    }

    /// <summary>
    /// Applies default resilience policies when the user has not provided their own builder.
    /// If the user provided a builder, it is invoked directly and these defaults are skipped.
    /// </summary>
    partial void OnAddCocApiHttpClientBuilder(IHttpClientBuilder builder, Action<IHttpClientBuilder>? userBuilder, ref bool suppressDefault)
    {
        if (userBuilder != null)
            return;

        builder
            .ConfigurePrimaryHttpMessageHandler(services =>
            {
                IConfiguration config = services.GetRequiredService<IConfiguration>();
                return new HttpClientHandler
                {
                    CookieContainer = services.GetRequiredService<CookieContainer>().Value,
                    MaxConnectionsPerServer = config.GetValue("CocApi:Rest:HttpClient:MaxConnectionsPerServer", defaultValue: 100)
                };
            })
            .AddRetryPolicy(GetValue("Retries", 1))
            .AddTimeoutPolicy(TimeSpan.FromMilliseconds(GetValue("Timeout", 1500)))
            .AddCircuitBreakerPolicy(
                GetValue("HandledEventsAllowedBeforeBreaking", 5),
                TimeSpan.FromSeconds(GetValue("DurationOfBreak", 30)));

        int GetValue(string key, int defaultValue)
        {
            IConfiguration? config = builder.Services
                .LastOrDefault(s => s.ServiceType == typeof(IConfiguration) && s.ImplementationInstance != null)
                ?.ImplementationInstance as IConfiguration
                ?? builder.Services.BuildServiceProvider().GetService<IConfiguration>();
            return config?.GetValue($"CocApi:Rest:HttpClient:{key}", defaultValue) ?? defaultValue;
        }
    }
}
