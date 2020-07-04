using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using Newtonsoft.Json.Serialization;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public sealed class Labels
    {
        private CocApi CocApi { get; }

        public Labels(CocApi cocApi) => CocApi = cocApi;

        public async Task<Paginated<Label>?> GetClanLabelsAsync(CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Paginated<Label>>(Label.ClanUrl(), cacheOption, cancellationToken).ConfigureAwait(false);

        public async Task<Paginated<Label>?> GetVillageLabelsAsync(CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
            => await CocApi.GetOrFetchAsync<Paginated<Label>>(Label.VillageUrl(), cacheOption, cancellationToken).ConfigureAwait(false);
    }
}