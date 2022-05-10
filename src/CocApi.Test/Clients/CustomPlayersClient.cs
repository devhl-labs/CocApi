using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Rest.IApis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class CustomPlayersClient : Cache.PlayersClient
    {
        public CustomPlayersClient(
            ILogger<CustomPlayersClient> logger,
            IServiceScopeFactory scopeFactory, 
            IPlayersApi playersApi,
            Synchronizer synchronizer,
            IPerpetualExecution<object>[] perpetualServices,
            IOptions<CacheOptions> options) 
        : base(logger, playersApi, scopeFactory, synchronizer, perpetualServices, options)
        {
            PlayerUpdated += OnPlayerUpdated;
        }

        private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
        {
            Logger.LogInformation("Player {playerTag} {playerName} has updated.", e.Fetched.Tag, e.Fetched.Name);

            return Task.CompletedTask;
        }
    }
}
