using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Cache.Services.Options;
using CocApi.Rest.IBaseApis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Test;

public class CustomPlayersClient : Cache.PlayersClient
{
    public CustomPlayersClient(
        ILogger<CustomPlayersClient> logger,
        IServiceScopeFactory scopeFactory, 
        IPlayersApi playersApi,
        Synchronizer synchronizer,
        PlayerService playerService,
        MemberService memberService,
        IOptions<CacheOptions> options)
    : base(logger, playersApi, scopeFactory, synchronizer, playerService, memberService, options)
    {
        PlayerUpdated += OnPlayerUpdated;
    }

    private Task OnPlayerUpdated(object sender, PlayerUpdatedEventArgs e)
    {
        Logger.LogInformation("Player {playerTag} {playerName} has updated.", e.Fetched.Tag, e.Fetched.Name);

        return Task.CompletedTask;
    }
}
