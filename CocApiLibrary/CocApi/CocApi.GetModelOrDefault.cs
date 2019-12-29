using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

using devhl.CocApi.Models;
using devhl.CocApi.Exceptions;
using static devhl.CocApi.Enums;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace devhl.CocApi
{
    public sealed partial class CocApi : IDisposable
    {
        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clanTag is not found.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public Clan? GetClanOrDefault(string clanTag)
        {
            AllClans.TryGetValue(clanTag, out Clan? result, AllClans);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the war log is private.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public IWar? GetWarByClanTagOrDefault(string clanTag)
        {
            AllWarsByClanTag.TryGetValue(clanTag, out IWar? result, AllWarsByClanTag);

            return result;
        }

        public IActiveWar? GetWarByWarIdOrDefault(string warId)
        {
            AllWarsByWarId.TryGetValue(warId, out IActiveWar? currentWar, AllWarsByWarId);

            return currentWar;
        }
 

        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the clan is not in a clan is not found to be in a league.  
        /// Returns <see cref="LeagueGroup"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ILeagueGroup? GetLeagueGroupOrDefault(string clanTag)
        {
            AllLeagueGroups.TryGetValue(clanTag, out ILeagueGroup? result, AllLeagueGroups);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the warTag is not found.
        /// </summary>
        /// <param name="warTag"></param>
        /// <returns></returns>
        public LeagueWar? GetLeagueWarOrDefault(string warTag)
        {
            AllWarsByWarTag.TryGetValue(warTag, out LeagueWar? result, AllWarsByWarTag);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.  Returns null if the villageTag is not found.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <returns></returns>
        public Village? GetVillageOrDefault(string villageTag)
        {            
            AllVillages.TryGetValue(villageTag, out Village? result, AllVillages);

            return result;
        }
    }
}
