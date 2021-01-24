using System;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Test
{
    public class PlayersClient : PlayersClientBase
    {
        public PlayersClient(Cache.ClientConfiguration cacheConfiguration, PlayersApi playersApi) : base(cacheConfiguration, playersApi)
        {
            Log += OnLog;
            PlayerUpdated += OnPlayerUpdated;
        }

        private Task OnLog(object sender, LogEventArgs log)
        {
            if (log is ExceptionEventArgs exception)
                LogService.Log(LogLevel.Warning, sender.GetType().Name, exception.Method, exception.Message, exception.Exception.Message);
            else
                LogService.Log(LogLevel.Information, sender.GetType().Name, log.Method, log.Message);

            return Task.CompletedTask;
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

        protected override bool HasUpdated(Player stored, Player fetched)
        {
            // in this example we only care if the townhall level changed
            // overriding this will control what changes trigger the PlayerUpdated event
            return stored.TownHallLevel != fetched.TownHallLevel;
        }

        private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, "Player updated");

            return Task.CompletedTask;
        }
    }
}
