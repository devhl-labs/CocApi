using devhl.CocApi.Models;
using devhl.CocApi.Models.Cache;
using devhl.CocApi.Models.Clan;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Locations
    {
        private CocApi CocApi { get; }

        public Locations(CocApi cocApi) => CocApi = cocApi;

        public async Task<Paginated<Location>?> GetAsync(CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Paginated<Location>>(Location.Url(), cacheOption, cancellationToken).ConfigureAwait(false);
    }
}