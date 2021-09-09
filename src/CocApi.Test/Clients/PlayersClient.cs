using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Cache.Services;

namespace CocApi.Test
{
    public class PlayersClient : PlayersClientBase
    {
        public PlayersClient(
            CacheDbContextFactoryProvider cacheContextOptions, 
            PlayersApi playersApi, 
            PlayerService playerMonitor,
            MemberService memberMonitor,
            Synchronizer synchronizer) 
        : base(playersApi, cacheContextOptions, playerMonitor, memberMonitor, synchronizer)
        {
            PlayerUpdated += OnPlayerUpdated;
        }

        private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            LogService.Log(LogLevel.Information, this.GetType().Name, "Player updated");

            return Task.CompletedTask;
        }
    }
}
