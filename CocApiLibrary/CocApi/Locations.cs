using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Locations
    {
        private readonly CocApi _cocApi;

        public Locations(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        public async Task<Paginated<Location>?> FetchAsync(CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<Location>>(Location.Url(), cancellationToken).ConfigureAwait(false) as Paginated<Location>;
    }
}