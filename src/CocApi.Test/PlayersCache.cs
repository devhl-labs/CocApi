using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using Microsoft.Extensions.Hosting;

namespace CocApi.Test
{
    public class PlayersCache : PlayersCacheBase
    {
        private readonly PlayersApi _playersApi;
        private readonly LogService _logService;

        public PlayersCache(TokenProvider tokenProvider, CacheConfiguration cacheConfiguration, PlayersApi playersApi, LogService logService) : base(tokenProvider, cacheConfiguration, playersApi)
        {
            _playersApi = playersApi;
            _logService = logService;

            PlayerUpdated += PlayerUpdater_PlayerUpdated;
            _playersApi.QueryResult += QueryResult;
        }

        private Task PlayerUpdater_PlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Debug, this.GetType().Name, null, "Player updated");

            return Task.CompletedTask;
        }

        private Task QueryResult(object sender, QueryResultEventArgs log)
        {
            string seconds = ((int)log.QueryResult.Stopwatch.Elapsed.TotalSeconds).ToString();

            if (log.QueryResult is QueryException exception)
            {
                if (exception.Exception is ApiException apiException)
                    _logService.Log(LogLevel.Debug, sender.GetType().Name, seconds, log.QueryResult.EncodedUrl(), apiException.ErrorContent.ToString());
                else
                    _logService.Log(LogLevel.Debug, sender.GetType().Name, seconds, log.QueryResult.EncodedUrl(), exception.Exception.Message);
            }

            if (log.QueryResult is QuerySuccess)
                _logService.Log(LogLevel.Information, sender.GetType().Name, seconds, log.QueryResult.EncodedUrl());

            return Task.CompletedTask;
        }
    }
}
