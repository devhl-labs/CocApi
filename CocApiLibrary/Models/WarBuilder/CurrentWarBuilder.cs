using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using devhl.CocApi.Converters;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using devhl.CocApi.Exceptions;

namespace devhl.CocApi.Models.War
{
    public class CurrentWarBuilder
    {
        public DateTime EndTimeUtc { get; set; }

        public DateTime PreparationStartTimeUtc { get; set; }

        public DateTime StartTimeUtc { get; set; }

        public int TeamSize { get; set; }

        public WarState State { get; set; }

        public IList<WarClanBuilder> WarClans { get; set; } = new List<WarClanBuilder>();

        public IList<AttackBuilder> Attacks { get; set; } = new List<AttackBuilder>();

        /// <summary>
        /// This value is used internally to identify unique wars.
        /// </summary>
        internal string WarKey { get; private set; } = string.Empty;

        public WarType WarType { get; set; } = WarType.Random;

        public DateTime? WarEndingSoonUtc { get; set; }

        public DateTime? WarStartingSoonUtc { get; set; }

        public List<WarVillageBuilder> WarVillages { get; set; } = new List<WarVillageBuilder>();

        public string? WarTag { get; set; }

        public WarAnnouncements WarAnnouncements {get;set;}

        public T Build<T>() where T : CurrentWar, new()
        {
            if (PreparationStartTimeUtc == null || PreparationStartTimeUtc == DateTime.MinValue)
                throw new CocApiException("PreparationStartTimeUtc is required.");

            if (WarClans == null || WarClans.Count != 2)
                throw new CocApiException("WarClans must contain two members.");

            if (WarType == WarType.SCCWL && (WarTag == null || WarTag == "#0" || WarTag == string.Empty))
                throw new CocApiException("WarTag is required for SCCWL wars.");

                WarClans = WarClans.OrderBy(x => x.ClanTag).ToList();

            WarKey = $"{PreparationStartTimeUtc};{WarClans[0].ClanTag}";

            T war = new T
            {
                EndTimeUtc = EndTimeUtc,
                PreparationStartTimeUtc = PreparationStartTimeUtc,
                StartTimeUtc = StartTimeUtc,
                TeamSize = TeamSize,
                State = State,
                WarType = WarType,
                WarKey = WarKey,
                WarAnnouncements = WarAnnouncements
            };

            foreach (WarClanBuilder warClanBuilder in WarClans)
            {
                war.WarClans.Add(warClanBuilder.Build(WarKey));
            }         

            war.WarEndingSoonUtc = EndTimeUtc.AddHours(-1);
            war.WarStartingSoonUtc = StartTimeUtc.AddHours(-1);

            if (war is LeagueWar leagueWar)
            {
                if (WarTag == null)
                    throw new CocApiException("WarTag is required for league wars.");

                leagueWar.WarTag = WarTag;
            }

            Attacks = Attacks.OrderBy(a => a.Order).ToList();

            foreach (AttackBuilder attackBuilder in Attacks)
                war.Attacks.Add(attackBuilder.Build(WarKey, PreparationStartTimeUtc));

            List<WarVillage> warVillages = new List<WarVillage>();
            foreach(WarVillageBuilder warVillageBuilder1 in WarVillages)
            {
                warVillages.Add(warVillageBuilder1.Build(WarKey));
            }

            war.WarClans[0].WarVillages = warVillages.Where(wv => wv.ClanTag == war.WarClans[0].ClanTag);
            war.WarClans[1].WarVillages = warVillages.Where(wv => wv.ClanTag == war.WarClans[1].ClanTag);

            foreach(WarVillage warVillage2 in warVillages)
            {
                warVillage2.Attacks = war.Attacks.Where(a => a.AttackerTag == warVillage2.VillageTag).ToList();
            }

            return war;
        }

        public override string ToString() => PreparationStartTimeUtc.ToString();
    }
}
