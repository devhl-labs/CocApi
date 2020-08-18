using CocApi.Cache.Models;
using CocApi.Cache.Models.Clans;
using Newtonsoft.Json.Serialization;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Cache
{
    public sealed class Labels
    {
        private CocApiClient_old CocApi { get; }

        public Labels(CocApiClient_old cocApi) => CocApi = cocApi;

        //public async Task<Paginated<Label>?> GetClanLabelsAsync(CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        //    => await CocApi.GetOrFetchAsync<Paginated<Label>>(Label.ClanUrl(), cacheOption, cancellationToken).ConfigureAwait(false);

        //public async Task<Paginated<Label>?> GetVillageLabelsAsync(CacheOption cacheOption = CacheOption.AllowAny, CancellationToken? cancellationToken = null)
        //    => await CocApi.GetOrFetchAsync<Paginated<Label>>(Label.VillageUrl(), cacheOption, cancellationToken).ConfigureAwait(false);
    }
}