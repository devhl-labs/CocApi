using System;
using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Rest.Client;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Test;

public class CustomTimeToLiveProvider : TimeToLiveProvider
{
    public CustomTimeToLiveProvider(ILogger<CustomTimeToLiveProvider> logger) : base(logger)
    {
    }

    protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(IOk<T> apiResponse) where T : class
    {
        // in this example if we downloaded a clan, we will keep it for one minutes past the server expiration
        return apiResponse is IOk<Clan>
            ? ValueTask.FromResult(apiResponse.ServerExpiration.AddMinutes(1) - DateTime.UtcNow)
            : base.TimeToLiveAsync(apiResponse);
    }
}
