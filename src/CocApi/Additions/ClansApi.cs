﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Api
{
    public partial class ClansApi
    {
        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetClanWarLeagueWarResponseAsync(string warTag)
        {
            var response = await getCurrentWarResponseAsync(warTag);

            response.Data.WarTag = warTag;

            InitializeWar(response);

            return response;
        }

        public async System.Threading.Tasks.Task<CocApi.Client.ApiResponse<ClanWar>> GetCurrentWarResponseAsync(string clanTag)
        {
            var response = await getCurrentWarResponseAsync(clanTag);

            InitializeWar(response);

            return response;
        }

        private void InitializeWar(ApiResponse<ClanWar> apiResponse)
        {
            if (apiResponse.Data.State == ClanWar.StateEnum.NotInWar)
                return;

            foreach (WarClan warClan in apiResponse.Data.Clans.Values)
                foreach (ClanWarMember member in warClan.Members)
                    foreach (ClanWarAttack attack in member.Attacks.EmptyIfNull())
                    {
                        WarClan defendingClan = apiResponse.Data.Clans.First(c => c.Key != warClan.Tag).Value;
                        ClanWarMember defending = defendingClan.Members.First(m => m.Tag == attack.DefenderTag);

                        attack.AttackerClanTag = warClan.Tag;
                        attack.DefenderClanTag = defendingClan.Tag;
                        attack.AttackerTownHall = member.TownhallLevel;
                        attack.DefenderTownHall = defending.TownhallLevel;
                        attack.AttackerMapPosition = member.MapPosition;
                        attack.DefenderMapPosition = defending.MapPosition;
                    }

            foreach (var clan in apiResponse.Data.Clans.Values)
            {
                var grouped = clan.AllAttacks.GroupBy(a => a.DefenderMapPosition);

                foreach (var group in grouped)
                {
                    bool fresh = true;
                    int maxStars = 0;

                    foreach (var attack in group.OrderBy(g => g.Order))
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

            TimeSpan timeSpan = apiResponse.Data.StartTime - apiResponse.Data.PreparationStartTime;

            if (timeSpan.TotalHours == 24
                || timeSpan.TotalHours == 20
                || timeSpan.TotalHours == 16
                || timeSpan.TotalHours == 12
                || timeSpan.TotalHours == 8
                || timeSpan.TotalHours == 6
                || timeSpan.TotalHours == 4
                || timeSpan.TotalHours == 2
                || timeSpan.TotalHours == 1
                || timeSpan.TotalMinutes == 30
                || timeSpan.TotalMinutes == 15)
            {
                apiResponse.Data.Type = ClanWar.TypeEnum.Friendly;
            }

            if (timeSpan.TotalHours == 23)
                apiResponse.Data.Type = ClanWar.TypeEnum.Random;

            if (apiResponse.Data.WarTag != null)
                apiResponse.Data.Type = ClanWar.TypeEnum.SCCWL;
        }
    }
}