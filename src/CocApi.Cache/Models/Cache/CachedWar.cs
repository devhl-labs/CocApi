using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool? IsAvailableByClan { get; set; }

        public bool? IsAvailableByOpponent { get; set; }

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
                IsAvailableByClan = true;
            else
                IsAvailableByOpponent = true;
        }

        //public void UpdateFromResponse(CachedClan cachedClan, ApiResponse<ClanWar> apiResponse, TimeSpan localExpiration)
        //{
        //    base.UpdateFromResponse(apiResponse, localExpiration);

        //    UpdateFromResponse(apiResponse, localExpiration);

        //    State = apiResponse.Data.State;

        //    EndTime = apiResponse.Data.EndTime;

        //    State = apiResponse.Data.State;

        //    if (cachedClan.Tag == apiResponse.Data.Clans.First().Value.Tag)
        //        IsAvailableByClan = true;
        //    else
        //        IsAvailableByOpponent = true;
        //}

        public override bool Equals(object? obj)
        {
            return obj is CachedWar war &&
                   Id == war.Id &&
                   RawContent == war.RawContent &&
                   Downloaded == war.Downloaded &&
                   ServerExpiration == war.ServerExpiration &&
                   LocalExpiration == war.LocalExpiration &&
                   EqualityComparer<ClanWar?>.Default.Equals(Data, war.Data) &&
                   ClanTag == war.ClanTag &&
                   OpponentTag == war.OpponentTag &&
                   PreparationStartTime == war.PreparationStartTime &&
                   EndTime == war.EndTime &&
                   WarTag == war.WarTag &&
                   State == war.State &&
                   IsFinal == war.IsFinal &&
                   IsAvailableByClan == war.IsAvailableByClan &&
                   IsAvailableByOpponent == war.IsAvailableByOpponent &&
                   Announcements == war.Announcements &&
                   EqualityComparer<List<string>>.Default.Equals(_clanTags, war._clanTags) &&
                   EqualityComparer<List<string>>.Default.Equals(ClanTags, war.ClanTags);
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
