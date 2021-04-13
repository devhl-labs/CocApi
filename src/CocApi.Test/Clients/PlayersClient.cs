using System;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class PlayersClient : PlayersClientBase
    {
        public PlayersClient(CacheDbContextFactoryProvider cacheContextOptions, PlayersApi playersApi, IOptions<MonitorOptions> options) 
            : base(playersApi, cacheContextOptions, options)
        {
            PlayerUpdated += OnPlayerUpdated;
        }

        private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, "Player updated");

            return Task.CompletedTask;
        }

        public override ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
        {
            // store the api results or exception for 10 minutes
            // this controls how frequently the cache queries the api for an update
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(0));
        }

        public override ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
        {
            // store the api results or exception for 10 minutes
            // this controls how frequently the cache queries the api for an update

            // you can cast exception to TimeOutException or ApiException
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(10));
        }
    }
}
