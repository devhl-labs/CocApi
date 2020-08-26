using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CocApi.Client;
using CocApi.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CocApi.Cache.Models
{
    public class CachedClanWarLog : CachedItem<ClanWarLog>
    {
        public string Tag { get; set; } = string.Empty;


        public new void UpdateFromResponse(ApiResponse<ClanWarLog>? responseItem, TimeSpan localExpiration)
        {
            base.UpdateFrom(responseItem, localExpiration);
        }

        public new void UpdateFromResponse(ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);
        }

            //internal async Task UpdateAsync(ClansCache clansCache, CachedClan cachedClan, IServiceProvider services)
            //{
            //    ////todo uncomment this
            //    //if (cachedClan.Data != null && cachedClan.Data.IsWarLogPublic == false)
            //    //    return;

            //    using var scope = services.CreateScope();

            //    CachedContext dbContext = scope.ServiceProvider.GetRequiredService<CachedContext>();

            //    CachedClanWarLog log = await dbContext.WarLogs.Where(g => g.Tag == cachedClan.Tag).FirstAsync();

            //    if (log.IsLocallyExpired() == false || log.IsServerExpired() == false)
            //        return;

            //    ApiResponse<ClanWarLog> apiResponse = await _cocApi.ClansApi.GetClanWarLogWithHttpInfoAsync(cachedClan.Tag);

            //    if (log.Data != null && log.Data.Equals(apiResponse.Data) == false)
            //        clansCache.OnClanWarLogUpdated(log.Data, apiResponse.Data);

            //    if (apiResponse == null)
            //        log.UpdateFromResponse(apiResponse, _cocApiConfiguration.PrivateWarLogTimeToLive);
            //    else
            //        log.UpdateFromResponse(apiResponse, _cocApiConfiguration.WarLogTimeToLive);

            //    dbContext.WarLogs.Update(log);

            //    await dbContext.SaveChangesAsync();
            //}

        }
}
