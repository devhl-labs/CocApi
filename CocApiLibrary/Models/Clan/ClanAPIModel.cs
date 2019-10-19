using CocApiLibrary.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;
using static CocApiLibrary.Extensions;

namespace CocApiLibrary.Models
{
    public class ClanAPIModel : CommonMethods, IClanAPIModel, IDownloadable
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
        	    if (_tag != value)
        	    {
        		    _tag = value;
        	        
                    EncodedUrl = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(_tag)}";
                }
            }
        }

        public string Name { get; set; } = string.Empty;

        public BadgeUrlModel? BadgeUrls { get; set; }

        public int ClanLevel { get; set; }

        public IEnumerable<LabelAPIModel>? Labels { get; set; }

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
                ILogger? logger = cocApi.Logger;

                Swallow(() => UpdateClan(cocApi, downloadedClan), logger);

                Swallow(() => UpdateLabels(cocApi, downloadedClan), logger);

                Swallow(() => UpdateBadge(cocApi, downloadedClan), logger);

                Swallow(() => UpdateLocation(cocApi, downloadedClan), logger);

                Swallow(() => AnnounceDonations(cocApi, downloadedClan), logger);

                Swallow(() => AnnounceMemberChanges(cocApi, downloadedClan), logger);

                Swallow(() => UpdateMembers(downloadedClan), logger);

                Swallow(() => MembersLeft(cocApi, downloadedClan), logger);

                Swallow(() => MembersJoined(cocApi, downloadedClan), logger);

                DateTimeUTC = downloadedClan.DateTimeUTC;

                Expires = downloadedClan.Expires;
            }
        }

        private void AnnounceMemberChanges(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            Dictionary<string, Tuple<MemberListAPIModel, LeagueAPIModel>> leagueChanges = new Dictionary<string, Tuple<MemberListAPIModel, LeagueAPIModel>>();

            Dictionary<string, Tuple<MemberListAPIModel, Role>> roleChanges = new Dictionary<string, Tuple<MemberListAPIModel, Role>>();


            foreach (MemberListAPIModel oldMember in Members.EmptyIfNull())
            {
                MemberListAPIModel newMember = downloadedClan.Members.FirstOrDefault(m => m.Tag == oldMember.Tag);

                if (newMember == null) { continue; }

                if ((oldMember.League == null && newMember.League != null) || (oldMember.League != null && newMember.League != null && oldMember.League.Id != newMember.League.Id))
                {
                    leagueChanges.Add(oldMember.Tag, Tuple.Create(oldMember, newMember.League));
                }
                        
                if (oldMember.Name != newMember.Name)
                {
                    cocApi.ClanMemberNameChangedEvent(oldMember, newMember.Name);
                }

                if (oldMember.Role != newMember.Role)
                {
                    roleChanges.Add(oldMember.Tag, Tuple.Create(oldMember, newMember.Role));
                }
            }

            cocApi.ClanMembersLeagueChangedEvent(leagueChanges);

            cocApi.ClanMembersRoleChangedEvent(roleChanges);
        }

        private void UpdateMembers(ClanAPIModel downloadedClan)
        {
            foreach (MemberListAPIModel oldMember in Members.EmptyIfNull())
            {
                MemberListAPIModel newMember = downloadedClan.Members.FirstOrDefault(m => m.Tag == oldMember.Tag);

                if (newMember == null) { continue; }

                oldMember.League = newMember.League;

                oldMember.Name = newMember.Name;

                oldMember.Role = newMember.Role;

                oldMember.ExpLevel = newMember.ExpLevel;

                oldMember.ClanRank = newMember.ClanRank;

                oldMember.PreviousClanRank = newMember.PreviousClanRank;

                oldMember.Donations = newMember.Donations;

                oldMember.DonationsReceived = newMember.DonationsReceived;

                oldMember.Trophies = newMember.Trophies;

                oldMember.VersusTrophies = newMember.VersusTrophies;
            }

        }

        private void AnnounceDonations(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            Dictionary<string, Tuple<MemberListAPIModel, int>> receiving = new Dictionary<string, Tuple<MemberListAPIModel, int>>();

            Dictionary<string, Tuple<MemberListAPIModel, int>> donating = new Dictionary<string, Tuple<MemberListAPIModel, int>>();

            foreach(MemberListAPIModel oldMember in Members.EmptyIfNull())
            {
                MemberListAPIModel? newMember = downloadedClan.Members.FirstOrDefault(m => m.Tag == oldMember.Tag);

                if (newMember == null) continue;

                if (oldMember.DonationsReceived != newMember.DonationsReceived && newMember.DonationsReceived != 0)
                {
                    receiving.Add(Tag, Tuple.Create(oldMember, newMember.DonationsReceived));
                }

                if (oldMember.Donations != newMember.Donations && newMember.Donations != 0)
                {
                    donating.Add(Tag, Tuple.Create(oldMember, newMember.Donations));
                }
            }

            cocApi.ClanDonationsEvent(receiving, donating);

        }

        private void UpdateLabels(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            if (Labels == null && downloadedClan.Labels == null) return;

            if (Labels != null && Labels.Count() > 0 && (downloadedClan.Labels == null || downloadedClan.Labels.Count() == 0))
            {
                cocApi.ClanLabelsRemovedEvent(downloadedClan, Labels);

                Labels = downloadedClan.Labels;
            }
            else if ((Labels == null || Labels.Count() == 0) && downloadedClan.Labels != null && downloadedClan.Labels.Count() > 0)
            {
                cocApi.ClanLabelsAddedEvent(downloadedClan, downloadedClan.Labels);

                Labels = downloadedClan.Labels;
            }
            else
            {
                List<LabelAPIModel> added = new List<LabelAPIModel>();

                List<LabelAPIModel> removed = new List<LabelAPIModel>();

                foreach (LabelAPIModel labelAPIModel in Labels.EmptyIfNull())
                {
                    if (!downloadedClan.Labels.Any(l => l.Id == labelAPIModel.Id))
                    {
                        removed.Add(labelAPIModel);
                    }
                }

                foreach (LabelAPIModel labelAPIModel in downloadedClan.Labels.EmptyIfNull())
                {
                    if (!Labels.Any(l => l.Id == labelAPIModel.Id))
                    {
                        added.Add(labelAPIModel);
                    }
                }

                cocApi.ClanLabelsRemovedEvent(downloadedClan, removed);

                cocApi.ClanLabelsAddedEvent(downloadedClan, added);

                Labels = downloadedClan.Labels;
            }
        }

        private void UpdateLocation(CocApi cocApi, ClanAPIModel downloadedClan)
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
                cocApi.ClanLocationChangedEvent(this, downloadedClan);

                Location = downloadedClan.Location;
            }

        }

        private void UpdateBadge(CocApi cocApi, ClanAPIModel downloadedClan)
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

        private void UpdateClan(CocApi cocApi, ClanAPIModel downloadedClan)
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

        private void MembersJoined(CocApi cocApi, ClanAPIModel downloadedClan)
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

        private void MembersLeft(CocApi cocApi, ClanAPIModel downloadedClan)
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
    }
}
