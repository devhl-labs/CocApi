using Dapper.SqlWriter;
using devhl.CocApi.Models.War;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace devhl.CocApi.Models.Cache
{
    internal class CachedWar : DBObject
    {
        public string ClanTag { get; set; } = string.Empty;

        public string OpponentTag { get; set; } = string.Empty;

        public string Json { get; set; }

        public DateTime PrepStartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string? WarTag { get; set; }

        public WarState WarState { get; set; }

        public bool IsFinal { get; set; }

        public string WarKey()
        {
            List<string> tags = new List<string>
            {
                ClanTag,

                OpponentTag
            };

            tags = tags.OrderBy(t => t).ToList();

            return $"{PrepStartTime};{tags[0]}";
        }

        public bool IsAvailableByClan { get; set; }

        public bool IsAvailableByOpponent { get; set; }

        public Announcements Announcements { get; set; }

        public int Id { get; set; }


#nullable disable

        private List<string> _clanTags;

#nullable enable

        public List<string> ClanTags
        {
            get
            {
                if (_clanTags != null)
                    return _clanTags;

                _clanTags = new List<string>
                {
                    ClanTag,

                    OpponentTag
                };

                _clanTags = _clanTags.OrderBy(t => t).ToList();

                return _clanTags;
            }
        }

        public CachedWar(CurrentWar currentWar, string clanTag)
        {
            ClanTag = currentWar.WarClans[0].ClanTag;

            OpponentTag = currentWar.WarClans[1].ClanTag;

            PrepStartTime = currentWar.PreparationStartTimeUtc;

            WarState = currentWar.State;

            EndTime = currentWar.EndTimeUtc;

            if (currentWar is LeagueWar leagueWar)
                WarTag = leagueWar.WarTag;

            if (clanTag == ClanTag)
                IsAvailableByClan = true;

            if (clanTag == OpponentTag)
                IsAvailableByOpponent = true;

            Json = currentWar.ToJson();
        }

        public CachedWar(LeagueWar leagueWar)
        {
            ClanTag = leagueWar.WarClans[0].ClanTag;

            OpponentTag = leagueWar.WarClans[1].ClanTag;

            PrepStartTime = leagueWar.PreparationStartTimeUtc;

            WarState = leagueWar.State;

            EndTime = leagueWar.EndTimeUtc;

            WarTag = leagueWar.WarTag;

            IsAvailableByClan = true;

            IsAvailableByOpponent = true;

            Json = leagueWar.ToJson();
        }

#nullable disable

        public CachedWar()
        {

        }

#nullable enable

        //public CurrentWar ToCurrentWar()
        //{
        //    //if (this is LeagueWar leagueWar)
        //    //    return 
        //    //JsonConvert.DeserializeObject<CurrentWar>(Json);

        //    return this.ToJson();
        //}
    }
}
