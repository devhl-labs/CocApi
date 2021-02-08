using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Api
{
    public partial class ClansApi
    {
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> FetchClanWarLeagueWarResponseAsync(string warTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await InternalFetchClanWarLeagueWarResponseAsync(warTag, cancellationToken);

            //response.Data.WarTag = warTag;

            //response.Data.ServerExpiration = response.ServerExpiration;

            if (response.Data != null)
                response.Data.Initialize(response.ServerExpiration, warTag);

            return response;
        }

        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> FetchCurrentWarResponseAsync(string clanTag, System.Threading.CancellationToken? cancellationToken = default)
        {
            var response = await InternalFetchCurrentWarResponseAsync(clanTag, cancellationToken);

            //response.Data.ServerExpiration = response.ServerExpiration;

            if (response.Data != null)
                response.Data.Initialize(response.ServerExpiration, null);

            return response;
        }
    }
}
