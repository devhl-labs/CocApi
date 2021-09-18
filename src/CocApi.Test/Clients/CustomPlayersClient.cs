using System.Threading.Tasks;
using CocApi.Api;
using CocApi.Cache;
using CocApi.Cache.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class CustomPlayersClient : Cache.PlayersClient
    {
        public CustomPlayersClient(
            ILogger<CustomPlayersClient> logger,
            CacheDbContextFactoryProvider cacheContextOptions, 
            PlayersApi playersApi,
            Synchronizer synchronizer,
            IPerpetualExecution<object>[] perpetualServices,
            IOptions<CacheOptions> options) 
        : base(logger, playersApi, cacheContextOptions, synchronizer, perpetualServices, options)
        {
            PlayerUpdated += OnPlayerUpdated;
        }

        private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            Logger.LogInformation("Player {0} {1} has updated.", e.Fetched.Tag, e.Fetched.Name);

            return Task.CompletedTask;
        }
    }
}
