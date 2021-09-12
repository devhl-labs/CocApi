using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Cache.Services;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class CustomPlayersClient : Cache.PlayersClient
    {
        public CustomPlayersClient(
            CacheDbContextFactoryProvider cacheContextOptions, 
            PlayersApi playersApi,
            Synchronizer synchronizer,
            IPerpetualExecution<object>[] perpetualServices,
            IOptions<CacheOptions> options) 
        : base(playersApi, cacheContextOptions, synchronizer, perpetualServices, options)
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
