using System;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore.Design;

namespace CocApi.Test
{
    public class PlayersClient : PlayersClientBase
    {
        public PlayersClient(
            IDesignTimeDbContextFactory<CocApiCacheContext> dbContextFactory, PlayersApi playersApi) 
            : base(playersApi, dbContextFactory, Array.Empty<string>())
        {
        }

        public override ValueTask<TimeSpan> TimeToLiveAsync(ApiResponse<Player> apiResponse)
        {
            // store the api results or exception for 10 minutes
            // this controls how frequently the cache queries the api for an update
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(10));
        }

        public override ValueTask<TimeSpan> TimeToLiveAsync(Exception exception)
        {
            // store the api results or exception for 10 minutes
            // this controls how frequently the cache queries the api for an update

            // you can cast exception to TimeOutException or ApiException
            return new ValueTask<TimeSpan>(TimeSpan.FromMinutes(10));
        }

        private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, "Player updated");

            return Task.CompletedTask;
        }
    }
}
