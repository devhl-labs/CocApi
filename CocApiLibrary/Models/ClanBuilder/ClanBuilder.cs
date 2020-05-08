using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models.War;
using Newtonsoft.Json;


namespace devhl.CocApi.Models.Clan
{
    public class ClanBuilder
    {
        public string ClanTag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public BadgeUrlBuilder? BadgeUrl { get; set; }

        //public LocationBuilder? Location { get; set; }  //shouldn't be needed because we have the LocationId

        public int? LocationId { get; set; }

        public List<ClanLabelBuilder>? Labels { get; set; } = new List<ClanLabelBuilder>();

        public int ClanLevel { get; set; }

        public bool QueueClanVillages { get; set; } = true;

        public bool QueueLeagueWars { get; set; } = true;
       
        public bool QueueCurrentWar { get; set; } = true;

        public List<ClanVillageBuilder>? Villages { get; set; } = new List<ClanVillageBuilder>();

        public RecruitmentType Recruitment { get; set; }

        public string Description { get; set; } = string.Empty;

        public int ClanPoints { get; set; }

        public int ClanVersusPoints { get; set; }

        public int RequiredTrophies { get; set; }

        public int WarWinStreak { get; set; }

        public int WarWins { get; set; }

        public int WarTies { get; set; }

        public int WarLosses { get; set; }

        public bool IsWarLogPublic { get; set; } = false;

        public int VillageCount { get; set; }

        public WarFrequency WarFrequency { get; set; }

        //public WarLeague? WarLeague { get; set; }

        public int WarLeagueId { get; set; }

        public DateTime? ServerExpirationUtc { get; set; }

        public override string ToString() => $"{ClanTag} {Name}";

        public Clan Build()
        {
            Clan clan = new Clan
            {
                ClanTag = ClanTag,
                Name = Name,
                LocationId = LocationId,
                ClanLevel = ClanLevel,
                QueueClanVillages = QueueClanVillages,
                QueueLeagueWars = QueueLeagueWars,
                QueueCurrentWar = QueueCurrentWar,
                Recruitment = Recruitment,
                Description = Description,
                ClanPoints = ClanPoints,
                ClanVersusPoints = ClanVersusPoints,
                RequiredTrophies = RequiredTrophies,
                WarWinStreak = WarWinStreak,
                WarWins = WarWins,
                WarTies = WarTies,
                WarLosses = WarLosses,
                IsWarLogPublic = IsWarLogPublic,
                VillageCount = VillageCount,
                WarFrequency = WarFrequency,
                WarLeagueId = WarLeagueId,
                ServerExpirationUtc = ServerExpirationUtc,
                BadgeUrl = BadgeUrl?.Build()
            };

            List<ClanLabel>? clanLabels = null;

            if (Labels != null)
            {
                clanLabels = new List<ClanLabel>();
                foreach(ClanLabelBuilder clanLabelBuilder in Labels)
                    clanLabels.Add(clanLabelBuilder.Build());
            }
            clan.Labels = clanLabels;

            List<ClanVillage>? clanVillages = null;

            if(Villages != null)
            {
                clanVillages = new List<ClanVillage>();
                foreach(ClanVillageBuilder clanVillageBuilder in Villages)
                    clanVillages.Add(clanVillageBuilder.Build());
            }

            clan.Villages = clanVillages;

            return clan;
        }
    }
}
