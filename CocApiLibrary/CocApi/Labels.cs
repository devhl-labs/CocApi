using devhl.CocApi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Labels
    {
        private readonly CocApi _cocApi;

        public Labels(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        public async Task<Paginated<Label>?> FetchClanLabelsAsync(CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<Label>>(Label.ClanUrl(), cancellationToken).ConfigureAwait(false) as Paginated<Label>;

        public async Task<Paginated<Label>?> FetchVillageLabelsAsync(CancellationToken? cancellationToken = null)
            => await _cocApi.FetchAsync<Paginated<Label>>(Label.VillageUrl(), cancellationToken).ConfigureAwait(false) as Paginated<Label>;
    }
}