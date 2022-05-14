using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache.Services
{
    public class DownloaderService
    {
        private readonly ServiceBase[] _perpetualServices = new ServiceBase[9];

        public DownloaderService(ActiveWarService activeWarService, ClanService clanService, CwlWarService cwlWarService, MemberService memberService,
            NewCwlWarService newCwlWarService, NewWarService newWarService, PlayerService playerService, StalePlayerService stalePlayerService, 
            WarService warService)
        {
            _perpetualServices[0] = activeWarService;
            _perpetualServices[1] = clanService;
            _perpetualServices[2] = cwlWarService;
            _perpetualServices[3] = memberService;
            _perpetualServices[4] = newCwlWarService;
            _perpetualServices[5] = newWarService;
            _perpetualServices[6] = playerService;
            _perpetualServices[7] = stalePlayerService;
            _perpetualServices[8] = warService;

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
}
