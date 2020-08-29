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
    public class ClansCache : ClansCacheBase, IHostedService
    {
        private readonly LogService _logService;

        public ClansCache(CacheConfiguration cacheConfiguration, ClansApi clansApi, PlayersCache playersCache, LogService logService) 
            : base(cacheConfiguration, clansApi, playersCache)
        {
            _logService = logService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ClanUpdated += ClansCache_ClanUpdated;
            ClanWarAdded += ClansCache_ClanWarAdded;
            ClanWarEndingSoon += ClansCache_ClanWarEndingSoon;
            ClanWarEndNotSeen += ClansCache_ClanWarEndNotSeen;
            ClanWarLeagueGroupUpdated += ClansCache_ClanWarLeagueGroupUpdated;
            ClanWarLogUpdated += ClansCache_ClanWarLogUpdated;
            ClanWarStartingSoon += ClansCache_ClanWarStartingSoon;
            ClanWarUpdated += ClansCache_ClanWarUpdated;

            await UpdateAsync("#8J82PV0C", downloadMembers: false); //fysb unbuckled
            await AddAsync("#22G0JJR8"); //fysb
            await AddAsync("#28RUGUYJU"); //devhls lab
            await AddAsync("#2C8V29YJ"); // russian clan

            await Task.Run(() =>
            {
                _ = RunAsync();
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            ClanUpdated -= ClansCache_ClanUpdated;
            ClanWarAdded -= ClansCache_ClanWarAdded;
            ClanWarEndingSoon -= ClansCache_ClanWarEndingSoon;
            ClanWarEndNotSeen -= ClansCache_ClanWarEndNotSeen;
            ClanWarLeagueGroupUpdated -= ClansCache_ClanWarLeagueGroupUpdated;
            ClanWarLogUpdated -= ClansCache_ClanWarLogUpdated;
            ClanWarStartingSoon -= ClansCache_ClanWarStartingSoon;
            ClanWarUpdated -= ClansCache_ClanWarUpdated;

            await StopAsync();
        }


        private Task ClansCache_ClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "War updated " + ClanWar.NewAttacks(e.Stored, e.Fetched).Count);

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarStartingSoon(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "War starting soon");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarEndNotSeen(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "War war end not seen");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarEndingSoon(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "War ending soon");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarAdded(object sender, ClanWarEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "New war");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarLeagueGroupUpdated(object sender, ClanWarLeagueGroupUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "Group updated");

            return Task.CompletedTask;
        }

        private Task ClansCache_ClanWarLogUpdated(object sender, ClanWarLogUpdatedEventArgs e)
        {
            _logService.Log(LogLevel.Debug, nameof(Program), null, "War log updated");

            return Task.CompletedTask;
        }

        private Task Client_Log(object sender, LogEventArgs log)
        {
            _logService.Log(LogLevel.Debug, sender.GetType().Name, log.Method, log.Message);
            ;

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

        private Task ClansCache_ClanUpdated(object sender, ClanUpdatedEventArgs e)
        {
            var donations = Clan.Donations(e.Stored, e.Fetched);

            if (donations.Count > 0)
                _logService.Log(LogLevel.Debug, nameof(Program), null, "Clan updated" + donations.Count + " " + donations.Sum(d => d.Quanity));
            else
                _logService.Log(LogLevel.Debug, nameof(Program), null, "Clan updated");

            foreach (ClanMember member in Clan.ClanMembersLeft(e.Stored, e.Fetched))
                Console.WriteLine(member.Name + " left");

            foreach (ClanMember member in Clan.ClanMembersJoined(e.Stored, e.Fetched))
                Console.WriteLine(member.Name + " joined");

            return Task.CompletedTask;
        }
    }
}
