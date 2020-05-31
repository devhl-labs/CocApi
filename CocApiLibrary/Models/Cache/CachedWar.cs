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

        public DateTime ProcessedAt { get; set; }

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

        public CachedWar(CurrentWar currentWar)
        {
            ClanTag = currentWar.WarClans[0].ClanTag;

            OpponentTag = currentWar.WarClans[1].ClanTag;

            PrepStartTime = currentWar.PreparationStartTimeUtc;

            WarState = currentWar.State;

            EndTime = currentWar.EndTimeUtc;

            if (currentWar is LeagueWar leagueWar)
                WarTag = leagueWar.WarTag;

            Json = JsonConvert.SerializeObject(currentWar);
        }

#nullable disable

        public CachedWar()
        {

        }

#nullable enable

        public CurrentWar ToCurrentWar() => JsonConvert.DeserializeObject<CurrentWar>(Json);
    }
}
