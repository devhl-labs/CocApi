using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Client;
using CocApi.Model;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace CocApi.Test
{
    public class ClansClient : ClansClientBase, IHostedService
    {
        private readonly PlayersClient _playersCache;
        private readonly LogService _logService;
        private readonly LeaguesApi _leaguesApi;
        private readonly LocationsApi _locationsApi;
        private readonly ClansApi _clansApi;

        public ClansClient(
            TokenProvider tokenProvider, Cache.ClientConfiguration cacheConfiguration, 
            ClansApi clansApi, PlayersClient playersCache, LogService logService, LeaguesApi leaguesApi, LocationsApi locationsApi) 
            : base(tokenProvider, cacheConfiguration, clansApi, playersCache)
        {
            _playersCache = playersCache;
            _logService = logService;
            _leaguesApi = leaguesApi;
            _locationsApi = locationsApi;
            _clansApi = clansApi;

            ClanUpdated += ClansCache_ClanUpdated;
            ClanWarAdded += ClansCache_ClanWarAdded;
            ClanWarEndingSoon += ClansCache_ClanWarEndingSoon;
            ClanWarEndNotSeen += ClansCache_ClanWarEndNotSeen;
            ClanWarLeagueGroupUpdated += ClansCache_ClanWarLeagueGroupUpdated;
            ClanWarLogUpdated += ClansCache_ClanWarLogUpdated;
            ClanWarStartingSoon += ClansCache_ClanWarStartingSoon;
            ClanWarUpdated += ClansCache_ClanWarUpdated;
            Log += ClansCache_Log;
            clansApi.HttpRequestResult += QueryResult;
        }

        private Task ClansCache_Log(object sender, LogEventArgs log)
        {
            if (log is ExceptionEventArgs exception)
                _logService.Log(LogLevel.Warning, sender.GetType().Name, exception.Method, exception.Message, exception.Exception.Message);
            else            
                _logService.Log(LogLevel.Information, sender.GetType().Name, log.Method, log.Message);            

            return Task.CompletedTask;
        }

        protected override bool HasUpdated(Clan stored, Clan fetched)
        {
            return base.HasUpdated(stored, fetched);
        }

        public new async Task StartAsync(CancellationToken cancellationToken)
        {
            await _playersCache.AddOrUpdateAsync("#29GPU9CUJ"); //squirrel man

            await AddOrUpdateAsync("#8J82PV0C", true, true, true); //fysb unbuckled
            await AddOrUpdateAsync("#22G0JJR8", true, true, true); //fysb
            await AddOrUpdateAsync("#28RUGUYJU", true, true, true); //devhls lab
            await AddOrUpdateAsync("#2C8V29YJ", true, true, true); // russian clan
            await AddOrUpdateAsync("#JYULPG28"); // inphase

            DownloadMembers = true;
            DownloadCurrentWars = true;
            DownloadCwl = true;

            string token = await TokenProvider.GetAsync(_stopRequestedTokenSource.Token).ConfigureAwait(false);

            var playerGlobalRankings = await _locationsApi.GetPlayerRankingAsync(token, "global");
            var playerVersusGlobalRankings = await _locationsApi.GetPlayerVersusRankingAsync(token, "global");
            var clanGlobalRankings = await _locationsApi.GetClanRankingOrDefaultAsync(token, "global");
            var clanGlobalVersusRankings = await _locationsApi.GetClanVersusRankingAsync(token, "global");

            //todo  //await _playersCache.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public new async Task StopAsync(CancellationToken cancellationToken)
        {
            List<Task> tasks = new List<Task>
            {
                base.StopAsync(cancellationToken),
                _playersCache.StopAsync(cancellationToken)
            };

            await Task.WhenAll(tasks);
        }

        private Task ClansCache_ClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "War updated " + ClanWar.NewAttacks(e.Stored, e.Fetched).Count);

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarStartingSoon(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "War starting soon");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarEndNotSeen(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "War war end not seen");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarEndingSoon(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "War ending soon");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarAdded(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "New war");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarLeagueGroupUpdated(object sender, ClanWarLeagueGroupUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "Group updated");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarLogUpdated(object sender, ClanWarLogUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Information, this.GetType().Name, null, "War log updated");

            return Task.CompletedTask;
        }

        private Task QueryResult(object sender, HttpRequestResultEventArgs log)
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

        private async Task ClansCache_ClanUpdated(object sender, ClanUpdatedEventArgs e)
        {
            var donations = Clan.Donations(e.Stored, e.Fetched);

            if (donations.Count > 0)
                _logService.Log(LogLevel.Information, this.GetType().Name, null, "Clan updated" + donations.Count + " " + donations.Sum(d => d.Quanity));
            else
                _logService.Log(LogLevel.Information, this.GetType().Name, null, "Clan updated");

            foreach (ClanMember member in Clan.ClanMembersLeft(e.Stored, e.Fetched))
            {
                Console.WriteLine(member.Name + " left");

                await _playersCache.AddOrUpdateAsync(member.Tag, true);
            }

            foreach (ClanMember member in Clan.ClanMembersJoined(e.Stored, e.Fetched))
            {
                Console.WriteLine(member.Name + " joined");

                await _playersCache.AddOrUpdateAsync(member.Tag, true);
            }
        }
    }
}
