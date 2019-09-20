using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace CocApiLibrary.Models
{
    public class ClanAPIModel : IClanAPIModel, IDownloadable
    {
        /// <summary>
        /// Controls whether this clan will download village members.
        /// </summary>
        public bool DownloadVillages { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download league wars.
        /// </summary>
        public bool DownloadLeagueWars { get; set; } = true;

        private string _tag = string.Empty;
        
        public string Tag
        {
            get
            {
                return _tag;
            }
        
            set
            {
        	    if(_tag != value)
        	    {
        		    _tag = value;
        	        
                    EncodedUrl = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(_tag)}";
                }
            }
        }

        public string Name { get; set; } = string.Empty;

        public BadgeUrlModel? BadgeUrls { get; set; }

        public int ClanLevel { get; set; }

        [JsonPropertyName("memberList")]
        public IList<MemberListAPIModel>? Members { get; set; }

        public ClanType Type { get; set; }

        public string Description { get; set; } = string.Empty;

        public LocationModel? Location { get; set; }

        public int ClanPoints { get; set; }

        public int ClanVersusPoints { get; set; }

        public int RequiredTrophies { get; set; }

        public int WarWinStreak { get; set; }

        public int WarWins { get; set; }

        public int WarTies { get; set; }

        public int WarLosses { get; set; }

        public bool IsWarLogPublic { get; set; } = false;

        [JsonPropertyName("members")]
        public int MemberCount { get; set; }

        public WarFrequency WarFrequency { get; set; }







        [JsonIgnore]
        public Dictionary<string, ICurrentWarAPIModel> Wars { get; set; } = new Dictionary<string, ICurrentWarAPIModel>();






        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// This is a flag used to prevent all wars from being announced on startup. 
        /// It is set to true after all wars have been downloaded at least once for this clan.
        /// </summary>
        internal bool AnnounceWars { get; set; } = false;

        private readonly object _updateLock = new object();

        internal void Update(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            lock (_updateLock)
            {
                UpdateClanTry(cocApi, downloadedClan);

                UpdateBadgeTry(cocApi, downloadedClan);

                UpdateLocationTry(cocApi, downloadedClan);

                MembersLeftTry(cocApi, downloadedClan);

                MembersJoinedTry(cocApi, downloadedClan);

                DateTimeUTC = downloadedClan.DateTimeUTC;

                Expires = downloadedClan.Expires;
            }
        }







        private void UpdateLocationTry(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            try
            {
                if (Location == null && downloadedClan.Location != null)
                {
                    cocApi.ClanLocationChangedEvent(this, downloadedClan);
                    return;
                }

                if (Location == null && downloadedClan.Location != null ||
                    Location?.CountryCode != downloadedClan.Location?.CountryCode ||
                    Location?.Id != downloadedClan.Location?.Id ||
                    Location?.IsCountry != downloadedClan.Location?.IsCountry ||
                    Location?.Name != downloadedClan.Location?.Name)
                {
                    cocApi.ClanBadgeUrlChangedEvent(this, downloadedClan);

                    Location = downloadedClan.Location;
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateBadgeTry(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            try
            {
                if (BadgeUrls == null && downloadedClan.BadgeUrls != null)
                {
                    cocApi.ClanBadgeUrlChangedEvent(this, downloadedClan);
                    return;
                }

                if (BadgeUrls == null && downloadedClan.BadgeUrls != null |
                    BadgeUrls?.Large != downloadedClan.BadgeUrls?.Large ||
                    BadgeUrls?.Medium != downloadedClan.BadgeUrls?.Medium ||
                    BadgeUrls?.Small != downloadedClan.BadgeUrls?.Small)
                {
                    cocApi.ClanBadgeUrlChangedEvent(this, downloadedClan);

                    BadgeUrls = downloadedClan.BadgeUrls;
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateClanTry(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            try
            {
                if (ClanPoints != downloadedClan.ClanPoints)
                {
                    cocApi.ClanPointsChangedEvent(this, downloadedClan.ClanPoints);

                    ClanPoints = downloadedClan.ClanPoints;
                }

                if (ClanVersusPoints != downloadedClan.ClanVersusPoints)
                {
                    cocApi.ClanVersusPointsChangedEvent(this, downloadedClan.ClanVersusPoints);

                    ClanVersusPoints = downloadedClan.ClanVersusPoints;
                }

                if (ClanLevel != downloadedClan.ClanLevel ||
                    Description != downloadedClan.Description ||
                    IsWarLogPublic != downloadedClan.IsWarLogPublic ||
                    MemberCount != downloadedClan.MemberCount ||
                    Name != downloadedClan.Name ||
                    RequiredTrophies != downloadedClan.RequiredTrophies ||
                    Type != downloadedClan.Type ||
                    WarFrequency != downloadedClan.WarFrequency ||
                    WarLosses != downloadedClan.WarLosses ||
                    WarTies != downloadedClan.WarTies ||
                    WarWins != downloadedClan.WarWins ||
                    WarWinStreak != downloadedClan.WarWinStreak
                )
                {
                    cocApi.ClanChangedEvent(this, downloadedClan);

                    ClanLevel = downloadedClan.ClanLevel;
                    Description = downloadedClan.Description;
                    IsWarLogPublic = downloadedClan.IsWarLogPublic;
                    MemberCount = downloadedClan.MemberCount;
                    Name = downloadedClan.Name;
                    RequiredTrophies = downloadedClan.RequiredTrophies;
                    Type = downloadedClan.Type;
                    WarFrequency = downloadedClan.WarFrequency;
                    WarLosses = downloadedClan.WarLosses;
                    WarTies = downloadedClan.WarWins;
                    WarWinStreak = downloadedClan.WarWinStreak;
                }
            }
            catch (Exception)
            {
            }
        }

        private void MembersJoinedTry(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            try
            {
                List<MemberListAPIModel> newMembers = new List<MemberListAPIModel>();

                if (downloadedClan.Members == null)
                {
                    return;
                }

                if (Members == null)
                {
                    Members = new List<MemberListAPIModel>();
                }

                foreach (MemberListAPIModel member in downloadedClan.Members)
                {
                    if (!Members.Any(m => m.Tag == member.Tag))
                    {
                        newMembers.Add(member);

                        Members.Add(member);
                    }
                }

                cocApi.MembersJoinedEvent(this, newMembers);
            }
            catch (Exception)
            {
            }
        }

        private void MembersLeftTry(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            try
            {
                List<MemberListAPIModel> leftMembers = new List<MemberListAPIModel>();

                if (Members == null)
                {
                    return;
                }

                foreach (MemberListAPIModel member in Members)
                {
                    if (!downloadedClan.Members.Any(m => m.Tag == member.Tag))
                    {
                        leftMembers.Add(member);
                    }
                }

                foreach (MemberListAPIModel member in leftMembers)
                {
                    Members.Remove(member);
                }

                cocApi.MembersLeftEvent(this, leftMembers);
            }
            catch (Exception)
            {
            }
        }




    }
}
