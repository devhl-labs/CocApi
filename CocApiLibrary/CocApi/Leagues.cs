using devhl.CocApi.Models;
using devhl.CocApi.Models.Village;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Leagues
    {
        private readonly CocApi _cocApi;

        public Leagues(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        public async Task<Paginated<League>?> FetchAsync(CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<League>>(League.Url(), cancellationToken).ConfigureAwait(false) as Paginated<League>;
    }
}