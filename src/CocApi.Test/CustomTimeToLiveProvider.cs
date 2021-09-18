using System;
using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.Extensions.Logging;

namespace CocApi.Test
{
    public class CustomTimeToLiveProvider : TimeToLiveProvider
    {
        public CustomTimeToLiveProvider(ILogger<CustomTimeToLiveProvider> logger) : base(logger)
        {

        }

        protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(ApiResponse<T> apiResponse)
        {
            // in this example if we downloaded a clan, we will keep it for one minutes past the server expiration
            if (apiResponse is ApiResponse<Clan>)
                return ValueTask.FromResult(apiResponse.ServerExpiration.AddMinutes(1) - DateTime.UtcNow);

            return base.TimeToLiveAsync(apiResponse);
        }

        protected override ValueTask<TimeSpan> TimeToLiveAsync<T>(Exception exception)
        {
            return base.TimeToLiveAsync<T>(exception);
        }
    }
}
