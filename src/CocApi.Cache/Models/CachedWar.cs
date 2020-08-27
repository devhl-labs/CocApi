using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CocApi.Client;
using CocApi.Model;

namespace CocApi.Cache.Models
{
    public class CachedWar : CachedItem<ClanWar>
    {
        public string ClanTag { get; internal set; }

        public string OpponentTag { get; internal set; }

        public DateTime? PreparationStartTime { get; internal set; }

        public DateTime? EndTime { get; internal set; }

        public string? WarTag { get; internal set; }

        public ClanWar.StateEnum? State { get; internal set; }

        public bool IsFinal { get; internal set; }

        public HttpStatusCode? StatusCodeOpponent { get; internal set; }

        public Announcements Announcements { get; internal set; }

        public ClanWar.TypeEnum Type { get; internal set; }

#nullable disable

        private SortedSet<string> _clanTags;

#nullable enable

        public SortedSet<string> ClanTags
        {
            get
            {
                if (_clanTags != null)
                    return _clanTags;

                _clanTags = new SortedSet<string>
                {
                    ClanTag,

                    OpponentTag
                };

                return _clanTags;
            }
        }

#nullable disable

        public CachedWar()
        {

        }

#nullable enable

        public CachedWar(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration, string? warTag = null) : base(apiResponse, localExpiration)
        {
            ClanTag = apiResponse.Data.Clans.First().Value.Tag;

            OpponentTag = apiResponse.Data.Clans.Skip(1).First().Value.Tag;

            PreparationStartTime = apiResponse.Data.PreparationStartTime;

            EndTime = apiResponse.Data.EndTime;

            State = apiResponse.Data.State;

            WarTag = warTag;

            RawContent = apiResponse.RawContent;

            Type = apiResponse.Data.Type;

            if (cachedClan.Tag == apiResponse.Data.Clans.First().Value.Tag)
                StatusCode = apiResponse.StatusCode;
            else
                StatusCodeOpponent = apiResponse.StatusCode;
        }

        internal void UpdateFrom(CachedClanWar cachedClanWar)
        {
            RawContent = cachedClanWar.RawContent;

            Downloaded = cachedClanWar.Downloaded;

            ServerExpiration = cachedClanWar.ServerExpiration;

            LocalExpiration = cachedClanWar.LocalExpiration;

            if (cachedClanWar.Data != null)
            {
                Data = cachedClanWar.Data;
                
                State = cachedClanWar.Data.State;

                EndTime = cachedClanWar.Data.EndTime;
            }
        }

        internal void UpdateFrom(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiResponse, localExpiration);

            State = apiResponse.Data.State;

            if (State == ClanWar.StateEnum.WarEnded)
                IsFinal = true;

            if (cachedClan.Tag == apiResponse.Data.Clan.Tag)
                StatusCode = apiResponse.StatusCode;
            else
                StatusCodeOpponent = apiResponse.StatusCode;

        }

        internal new void UpdateFrom(ApiException apiException, TimeSpan localExpiration)
        {
            base.UpdateFrom(apiException, localExpiration);
        }
        public bool AllAttacksUsed()
        {
            int totalMembers = Data.TeamSize * 2;

            int totalAttacks = totalMembers;

            if (WarTag == null)
                totalAttacks *= 2;

            int attacks = Data.Clan.Attacks + Data.Opponent.Attacks;

            return totalAttacks == attacks;
        }

        //todo is anything calling this?
        public override bool Equals(object? obj)
        {
            if (Data == null ||
                !(obj is CachedWar war) ||
                war.PreparationStartTime != PreparationStartTime ||
                war.Data == null ||
                Data.PreparationStartTime != war.Data.PreparationStartTime)
                throw new ArgumentException();

            return /*obj is CachedWar war &&*/
                   //Id == war.Id &&
                   //RawContent == war.RawContent &&
                   //Downloaded == war.Downloaded &&
                   //ServerExpiration == war.ServerExpiration &&
                   //LocalExpiration == war.LocalExpiration &&
                   //EqualityComparer<ClanWar?>.Default.Equals(Data, war.Data) &&
                   //ClanTag == war.ClanTag &&
                   //OpponentTag == war.OpponentTag &&
                   //Data.PreparationStartTime == war.Data.PreparationStartTime &&
                   Data.EndTime == war.Data.EndTime &&
                   Data.StartTime == war.Data.StartTime &&
                   WarTag == war.WarTag &&
                   Data.State == war.Data.State &&
                   IsFinal == war.IsFinal &&
                   //StatusCodeClan == war.StatusCodeClan &&
                   //StatusCodeOpponent == war.StatusCodeOpponent &&
                   //Announcements == war.Announcements &&
                   //EqualityComparer<List<string>>.Default.Equals(_clanTags, war._clanTags) &&
                   EqualityComparer<SortedSet<string>>.Default.Equals(ClanTags, war.ClanTags) &&
                   Data.Clans.First().Value.Attacks == war.Data.Clans.First(c => c.Key == Data.Clans.First().Key).Value.Attacks &&
                   Data.Clans.Skip(1).First().Value.Attacks == war.Data.Clans.First(c => c.Key == Data.Clans.Skip(1).First().Key).Value.Attacks;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(PreparationStartTime);
            hash.Add(ClanTags.First());
            hash.Add(ClanTags.Skip(1).First());
            return hash.ToHashCode();
        }
    }
}
