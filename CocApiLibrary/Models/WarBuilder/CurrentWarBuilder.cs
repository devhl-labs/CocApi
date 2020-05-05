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

        public IList<WarClanBuilder> WarClans { get; private set; } = new List<WarClanBuilder>();

        public IList<AttackBuilder> Attacks { get; set; } = new List<AttackBuilder>();

        /// <summary>
        /// This value is used internally to identify unique wars.
        /// </summary>
        public string WarKey { get; private set; } = string.Empty;

        public WarType WarType { get; set; } = WarType.Random;

        public DateTime WarEndingSoonUtc { get; internal set; }

        public DateTime WarStartingSoonUtc { get; internal set; }

        public CurrentWarFlagsBuilder Flags { get; set; } = new CurrentWarFlagsBuilder();

        public List<WarVillageBuilder> WarVillages { get; set; } = new List<WarVillageBuilder>();

        public string? WarTag { get; set; }

        public T Build<T>() where T : CurrentWar, new()
        {
            if (PreparationStartTimeUtc == null || PreparationStartTimeUtc == DateTime.MinValue)
                throw new CocApiException("PreparationStartTimeUtc is required.");

            if (WarClans == null || WarClans.Count != 2)
                throw new CocApiException("WarClans must contain two members.");

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
                WarKey = WarKey
            };

            foreach (WarClanBuilder warClanBuilder in WarClans)
            {
                war.WarClans.Add(warClanBuilder.Build(WarKey));

                
            }


            WarEndingSoonUtc = EndTimeUtc.AddHours(-1);
            WarStartingSoonUtc = StartTimeUtc.AddHours(-1);

            if (war is LeagueWar leagueWar)
            {
                if (WarTag == null)
                    throw new CocApiException("WarTag is required for league wars.");

                leagueWar.WarTag = WarTag;
            }

            Attacks = Attacks.OrderBy(a => a.Order).ToList();

            foreach (AttackBuilder attackBuilder in Attacks)
                war.Attacks.Add(attackBuilder.Build(WarKey, PreparationStartTimeUtc));

            war.Flags = Flags.Build(WarKey);

            List<WarVillage> warVillages = new List<WarVillage>();
            foreach(WarVillageBuilder warVillageBuilder1 in WarVillages)
            {
                warVillages.Add(warVillageBuilder1.Build(WarKey));
            }

            war.WarClans[0].WarVillages = warVillages.Where(wv => wv.ClanTag == war.WarClans[0].ClanTag);
            war.WarClans[1].WarVillages = warVillages.Where(wv => wv.ClanTag == war.WarClans[1].ClanTag);

            //foreach (WarVillageBuilder warVillageBuilder in WarVillages)
            //{
            //    WarClan warClan = war.WarClans.First(wc => wc.ClanTag == warVillageBuilder.ClanTag);

            //    warClan.WarVillages ??= new List<WarVillage>();

            //    WarVillage warVillage = warClan.WarVillages.add(warVillageBuilder.Build(WarKey)).First();

            //    if (war.Attacks.Count(a => a.AttackerTag == warVillageBuilder.VillageTag) > 0)
            //    {
            //        warVillage.Attacks ??= war.Attacks.Where(a => a.AttackerTag == warVillage.VillageTag).ToList();
            //    }
            //}

            foreach(WarVillage warVillage2 in warVillages)
            {
                warVillage2.Attacks = war.Attacks.Where(a => a.AttackerTag == warVillage2.VillageTag).ToList();
            }

            return war;
        }

        public override string ToString() => PreparationStartTimeUtc.ToString();
    }
}
