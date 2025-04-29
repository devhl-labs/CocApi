using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache.Services;

public sealed class CachingService
{
    private readonly ServiceBase[] _perpetualServices;

    public CachingService(ActiveWarService activeWarService, ClanService clanService, ClanWarService clanWarService, CwlWarService cwlWarService, MemberService memberService,
        NewCwlWarService newCwlWarService, NewWarService newWarService, PlayerService playerService, StalePlayerService stalePlayerService, 
        WarService warService)
    {
        _perpetualServices = new ServiceBase[]
        {
            activeWarService,
            clanService,
            clanWarService,
            cwlWarService,
            memberService,
            newCwlWarService,
            newWarService,
            playerService,
            stalePlayerService,
            warService
        };
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var tasks = new Task[_perpetualServices.Length];

        for (int i = 0; i < _perpetualServices.Length; i++)
            tasks[i] = _perpetualServices[i].StopAsync(cancellationToken);

        await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var tasks = new Task[_perpetualServices.Length];

        for (int i = 0; i < _perpetualServices.Length; i++)
            tasks[i] = _perpetualServices[i].StartAsync(cancellationToken);

        await Task.WhenAll(tasks).WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
