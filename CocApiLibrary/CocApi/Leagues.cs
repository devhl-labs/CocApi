using devhl.CocApi.Models;
using devhl.CocApi.Models.Village;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Leagues
    {
        private CocApi CocApi { get; }

        public Leagues(CocApi cocApi) => CocApi = cocApi;

        public async Task<Paginated<League>?> GetAsync(CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Paginated<League>>(League.Url(), cacheOption, cancellationToken).ConfigureAwait(false);
    }
}