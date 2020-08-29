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

        public PlayersCache(CacheConfiguration cacheConfiguration, PlayersApi playersApi, LogService logService) : base(cacheConfiguration, playersApi)
        {
            _playersApi = playersApi;
            _logService = logService;

            PlayerUpdated += PlayerUpdater_PlayerUpdated;
            _playersApi.QueryResult += QueryResult;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await AddAsync("#29GPU9CUJ"); //squirrel man

            await _playersApi.GetPlayerAsync("#YLY0LPQP");

            await Task.Run(() =>
            {
                _ = RunAsync();
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopAsync();
        }

        private Task PlayerUpdater_PlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "Player updated");

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
