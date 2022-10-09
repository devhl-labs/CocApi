using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache.Services;

public sealed class CachingService
{
    private readonly ServiceBase[] _perpetualServices;

    public CachingService(ActiveWarService activeWarService, ClanService clanService, CwlWarService cwlWarService, MemberService memberService,
        NewCwlWarService newCwlWarService, NewWarService newWarService, PlayerService playerService, StalePlayerService stalePlayerService, 
        WarService warService)
    {
        _perpetualServices = new ServiceBase[]
        {
            activeWarService,
            clanService,
            cwlWarService,
            memberService,
            newCwlWarService,
            newWarService,
            playerService,
            stalePlayerService,
            warService
        };
    }

    public async Task StopAsync(CancellationToken? cancellationToken = null)
    {
        var tasks = new Task[_perpetualServices.Length];

        for (int i = 0; i < _perpetualServices.Length; i++)
            tasks[i] = _perpetualServices[i].StopAsync(cancellationToken.GetValueOrDefault());

        await Task.WhenAll(tasks);
    }

    public async Task StartAsync(CancellationToken? cancellationToken = null)
    {
        var tasks = new Task[_perpetualServices.Length];

        for (int i = 0; i < _perpetualServices.Length; i++)
            tasks[i] = _perpetualServices[i].StartAsync(cancellationToken.GetValueOrDefault());

        await Task.WhenAll(tasks);
    }
}
