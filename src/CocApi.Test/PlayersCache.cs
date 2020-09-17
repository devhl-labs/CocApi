using System;
using System.Collections.Generic;
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
    public class PlayersCache : PlayersClientBase
    {
        private readonly PlayersApi _playersApi;
        private readonly LogService _logService;

        public PlayersCache(TokenProvider tokenProvider, Cache.ClientConfiguration cacheConfiguration, PlayersApi playersApi, LogService logService) : base(tokenProvider, cacheConfiguration, playersApi)
        {
            _playersApi = playersApi;
            _logService = logService;
            Log += PlayersCache_Log;

            PlayerUpdated += PlayerUpdater_PlayerUpdated;
            _playersApi.HttpRequestResult += HttpRequestResult;
        }

        private Task PlayersCache_Log(object sender, LogEventArgs log)
        {
            if (log is ExceptionEventArgs exception)
                _logService.Log(LogLevel.Warning, sender.GetType().Name, exception.Method, exception.Message, exception.Exception.Message);
            else
                _logService.Log(LogLevel.Information, sender.GetType().Name, log.Method, log.Message);

            return Task.CompletedTask;
        }

        public override TimeSpan TimeToLive(ApiResponse<Player> apiResponse)
        {
            return TimeSpan.FromMinutes(10);
        }

        public override TimeSpan TimeToLive(Exception exception)
        {
            return TimeSpan.FromMinutes(20);
        }

        private Task PlayerUpdater_PlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "Player updated");

            return Task.CompletedTask;
        }

        private Task HttpRequestResult(object sender, HttpRequestResultEventArgs log)
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
