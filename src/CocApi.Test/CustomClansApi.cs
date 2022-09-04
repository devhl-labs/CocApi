using CocApi.Rest;
using CocApi.Rest.Client;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace CocApi.Test;

public sealed class CustomClansApi : CocApi.Rest.Apis.ClansApi
{
    public CustomClansApi(
        ILogger<CustomClansApi> logger,
        HttpClient httpClient,
        JsonSerializerOptionsProvider jsonSerializerOptionsProvider,
        TokenProvider<ApiKeyToken> apiKeyProvider)
    : base (logger, httpClient, jsonSerializerOptionsProvider, apiKeyProvider)
    {
    }
}
