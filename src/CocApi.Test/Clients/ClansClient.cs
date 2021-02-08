using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Model;
using Microsoft.Extensions.Hosting;

namespace CocApi.Test
{
    public class ClansClient : ClansClientBase, IHostedService
    {
        private readonly PlayersClient _playersCache;
        private readonly LocationsApi _locationsApi;
        private readonly PlayersApi _playersApi;

        public ClansClient(
            Cache.ClientConfiguration cacheConfiguration,
            ClansApi clansApi, PlayersClient playersCache, LocationsApi locationsApi, PlayersApi playersApi) 
            : base(cacheConfiguration, clansApi, playersCache)
        {
            _playersCache = playersCache;
            _locationsApi = locationsApi;
            _playersApi = playersApi;

            ClanUpdated += OnClanUpdated;
            ClanWarAdded += OnClanWarAdded;
            ClanWarLogUpdated += OnClanWarLogUpdated;
            ClanWarUpdated += OnClanWarUpdated;
            Log += OnLog;
        }

        private Task OnLog(object sender, LogEventArgs log)
        {
            if (log is ExceptionEventArgs exception)
                LogService.Log(LogLevel.Warning, sender.GetType().Name, exception.Method, exception.Message, exception.Exception.Message);
            else            
                LogService.Log(LogLevel.Information, sender.GetType().Name, log.Method, log.Message);            

            return Task.CompletedTask;
        }

        public new async Task StartAsync(CancellationToken cancellationToken)
        {
            DownloadMembers = false;
            DownloadCurrentWars = true;
            DownloadCwl = true;

            Migrate();

            await _playersCache.AddOrUpdateAsync("#29GPU9CUJ"); //squirrel man

            await AddOrUpdateAsync("#8J82PV0C", true, true, true); //fysb unbuckled
            await AddOrUpdateAsync("#22G0JJR8", true, true, true); //fysb
            await AddOrUpdateAsync("#28RUGUYJU", true, true, true); //devhls lab
            await AddOrUpdateAsync("#2C8V29YJ", true, true, true); // russian clan
            await AddOrUpdateAsync("#JYULPG28"); // inphase
            await AddOrUpdateAsync("#2P0YUY0L0"); // testing closed war log
            await AddOrUpdateAsync("#PJYPYG9P", true, true, true); // war heads

            var playerGlobalRankings = await _locationsApi.FetchPlayerRankingAsync("global");
            var playerVersusGlobalRankings = await _locationsApi.FetchPlayerVersusRankingAsync("global");
            var clanGlobalRankings = await _locationsApi.FetchClanRankingOrDefaultAsync("global");
            var clanGlobalVersusRankings = await _locationsApi.FetchClanVersusRankingAsync("global");

            var playerToken = await _playersApi.VerifyTokenResponseAsync("#29GPU9CUJ", new VerifyTokenRequest("a"));

            await _playersCache.StartAsync(cancellationToken);
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

        private Task OnClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, null, "War updated " + ClanWar.NewAttacks(e.Stored, e.Fetched).Count);

            return Task.CompletedTask;
        }

        private Task OnClanWarAdded(object sender, ClanWarEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, null, "New war");

            return Task.CompletedTask;
        }

        private Task OnClanWarLogUpdated(object sender, ClanWarLogUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, null, "War log updated");

            return Task.CompletedTask;
        }

        private async Task OnClanUpdated(object sender, ClanUpdatedEventArgs e)
        {
            var donations = Clan.Donations(e.Stored, e.Fetched);

            if (donations.Count > 0)
                LogService.Log(LogLevel.Information, this.GetType().Name, null, "Clan updated" + donations.Count + " " + donations.Sum(d => d.Quanity));
            else
                LogService.Log(LogLevel.Information, this.GetType().Name, null, "Clan updated");

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
