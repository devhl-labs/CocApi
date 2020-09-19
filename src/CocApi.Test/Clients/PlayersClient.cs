using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.Extensions.Hosting;

namespace CocApi.Test
{
    public class PlayersClient : PlayersClientBase
    {
        private readonly PlayersApi _playersApi;
        private readonly LogService _logService;

        public PlayersClient(TokenProvider tokenProvider, Cache.ClientConfiguration cacheConfiguration, PlayersApi playersApi, LogService logService) : base(tokenProvider, cacheConfiguration, playersApi)
        {
            _playersApi = playersApi;
            _logService = logService;

            Log += PlayersClient_Log;
            PlayerUpdated += PlayerUpdater_PlayerUpdated;

            _playersApi.HttpRequestResult += PlayersApi_HttpRequestResult;
        }

        private Task PlayersClient_Log(object sender, LogEventArgs log)
        {
            if (log is ExceptionEventArgs exception)
                _logService.Log(LogLevel.Warning, sender.GetType().Name, exception.Method, exception.Message, exception.Exception.Message);
            else
                _logService.Log(LogLevel.Information, sender.GetType().Name, log.Method, log.Message);

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

        private Task PlayerUpdater_PlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "Player updated");

            return Task.CompletedTask;
        }

        private Task PlayersApi_HttpRequestResult(object sender, HttpRequestResultEventArgs log)
        {
            string seconds = ((int)log.HttpRequestResult.Elapsed.TotalSeconds).ToString();

            if (log.HttpRequestResult is HttpRequestException exception)
            {
                if (exception.Exception is ApiException apiException)
                    _logService.Log(LogLevel.Debug, sender.GetType().Name, seconds, log.HttpRequestResult.EncodedUrl(), apiException.ErrorContent.ToString());
                else
                    _logService.Log(LogLevel.Debug, sender.GetType().Name, seconds, log.HttpRequestResult.EncodedUrl(), exception.Exception.Message);
            }

            if (log.HttpRequestResult is HttpRequestSuccess)
                _logService.Log(LogLevel.Information, sender.GetType().Name, seconds, log.HttpRequestResult.EncodedUrl());

            return Task.CompletedTask;
        }
    }
}
