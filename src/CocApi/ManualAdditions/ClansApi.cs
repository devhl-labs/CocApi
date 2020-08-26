using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocApi.Model;

namespace CocApi.Api
{
    public partial class ClansApi
    {
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetClanWarLeagueWarWithHttpInfoAsync(string warTag)
        {
            var response = await getClanWarLeagueWarWithHttpInfoAsync(warTag);

            response.Data.WarTag = warTag;

            foreach(WarClan warClan in response.Data.Clans.Values)
                foreach(ClanWarMember member in warClan.Members)
                    foreach(ClanWarAttack attack in member.Attacks)
                    {
                        WarClan defendingClan = response.Data.Clans.First(c => c.Key != warClan.Tag).Value;
                        ClanWarMember defending = defendingClan.Members.First(m => m.Tag == attack.DefenderTag);

                        attack.AttackerClanTag = warClan.Tag;
                        attack.DefenderClanTag = defendingClan.Tag;
                        attack.AttackerTownHall = member.TownhallLevel;
                        attack.DefenderTownHall = defending.TownhallLevel;
                        attack.AttackerMapPosition = member.MapPosition;
                        attack.DefenderMapPosition = defending.MapPosition;
                    }

            foreach (var clan in response.Data.Clans.Values)
            {
                var grouped = clan.AllAttacks.GroupBy(a => a.DefenderMapPosition);

                foreach(var group in grouped)
                {
                    bool fresh = true;
                    int maxStars = 0;

                    foreach(var attack in group.OrderBy(g => g.Order))
                    {
                        attack.Fresh = fresh;
                        fresh = false;

                        attack.StarsGained = attack.Stars - maxStars;

                        if (attack.StarsGained < 0)
                            attack.StarsGained = 0;

                        if (attack.Stars > maxStars)
                            maxStars = attack.Stars;
                    }
                }


            }  

            return response;
        }
    }
}
