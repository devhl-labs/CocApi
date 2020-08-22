using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CocApi.Client;
using CocApi.Model;
using RestSharp.Extensions;

namespace CocApi.Cache.Models.Cache
{
    public class CachedWar : CachedItem<ClanWar>
    {
        public string ClanTag { get; set; }

        public string OpponentTag { get; set; }

        public DateTime? PreparationStartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string? WarTag { get; set; }

        public ClanWar.StateEnum? State { get; set; }

        public bool IsFinal { get; set; }

        public HttpStatusCode? StatusCodeClan { get; set; }

        public HttpStatusCode? StatusCodeOpponent { get; set; }

        public Announcements Announcements { get; set; }

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

            if (cachedClan.Tag == apiResponse.Data.Clans.First().Value.Tag)
                StatusCodeClan = apiResponse.StatusCode;
            else
                StatusCodeOpponent = apiResponse.StatusCode;
        }

        public void UpdateFromCache(CachedClanWar cachedClanWar)
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
                   EqualityComparer<List<string>>.Default.Equals(ClanTags, war.ClanTags) &&
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
