using System;

using devhl.CocApi.Models;
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;

namespace devhl.CocApi
{
    public sealed partial class CocApi : IDisposable
    {
        /// <summary>
        /// This only searches what is currently in memory.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public Clan? GetClanOrDefault(string clanTag)
        {
            if (IsValidTag(clanTag, out string formattedTag) == false)
                return null;

            AllClans.TryGetValue(formattedTag, out Clan? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public IWar? GetCurrentWarByClanTagOrDefault(string clanTag)
        {
            if (IsValidTag(clanTag, out string formattedTag) == false)
                return null;

            AllCurrentWarsByClanTag.TryGetValue(formattedTag, out IWar? result);

            return result;
        }

        public CurrentWar? GetCurrentWarByWarKeyOrDefault(string warKey)
        {
            if (IsValidTag(warKey, out string formattedWarKey) == false)
                return null;

            AllWarsByWarKey.TryGetValue(formattedWarKey, out CurrentWar? currentWar);

            return currentWar;
        }
 

        /// <summary>
        /// This only searches what is currently in memory.  
        /// Returns <see cref="LeagueGroup"/> or <see cref="LeagueGroupNotFound"/>
        /// </summary>
        /// <param name="clanTag"></param>
        /// <returns></returns>
        public ILeagueGroup? GetLeagueGroupOrDefault(string clanTag)
        {
            if (IsValidTag(clanTag, out string formattedTag) == false)
                return null;

            AllLeagueGroups.TryGetValue(formattedTag, out ILeagueGroup? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.
        /// </summary>
        /// <param name="warTag"></param>
        /// <returns></returns>
        public LeagueWar? GetLeagueWarOrDefault(string warTag)
        {
            if (IsValidTag(warTag, out string formattedTag) == false)
                return null;

            AllLeagueWarsByWarTag.TryGetValue(formattedTag, out LeagueWar? result);

            return result;
        }


        /// <summary>
        /// This only searches what is currently in memory.
        /// </summary>
        /// <param name="villageTag"></param>
        /// <returns></returns>
        public Village? GetVillageOrDefault(string villageTag)
        {
            if (IsValidTag(villageTag, out string formattedTag) == false)
                return null;

            AllVillages.TryGetValue(formattedTag, out Village? result);

            return result;
        }

        public Paginated<League> GetLeagues() => AllLeagues;

        public Paginated<Label> GetClanLabels() => AllClanLabels;

        public Paginated<Label> GetVillageLabels() => AllVillageLabels;

        public Paginated<Location> GetLocations() => AllLocations;
    }
}
