using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
////System.Text.Json.Serialization
using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Location;
using devhl.CocApi.Models.War;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using static devhl.CocApi.Enums;


namespace devhl.CocApi.Models.Clan
{
    public class ClanApiModel : Downloadable, IClanApiModel, IInitialize
    {
        [NotMapped]
        [JsonIgnore]
        public ILogger? Logger { get; set; }

        // IClanApiModel
        [Key]
        [JsonProperty("Tag")]
        public string ClanTag
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

        public virtual ClanBadgeUrlApiModel? BadgeUrls { get; set; }

        public virtual LocationApiModel? Location { get; set; }

        [ForeignKey(nameof(ClanTag))]
        public virtual IEnumerable<ClanLabelApiModel>? Labels { get; set; }

        public int ClanLevel { get; set; }

        public string? BadgeUrlsId { get; set; } = string.Empty;

        public int? LocationId { get; set; }                     

        /// <summary>
        /// Controls whether this clan will download villages.
        /// </summary>
        public bool DownloadVillages { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download league wars.
        /// </summary>
        public bool DownloadLeagueWars { get; set; } = true;

        private string _tag = string.Empty;

        //[JsonProperty("memberList")]
        [JsonProperty("memberList")]
        [ForeignKey(nameof(ClanTag))]
        public virtual IList<ClanVillageApiModel>? Villages { get; set; }

        //[JsonProperty("type")]
        [JsonProperty("type")]
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

        //[JsonProperty("members")]
        [JsonProperty("members")]
        public int VillageCount { get; set; }

        public WarFrequency WarFrequency { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Dictionary<string, ICurrentWarApiModel> Wars { get; set; } = new Dictionary<string, ICurrentWarApiModel>();

        /// <summary>
        /// This is a flag used to prevent all wars from being announced on startup. 
        /// It is set to true after all wars have been downloaded at least once for this clan.
        /// </summary>
        internal bool AnnounceWars { get; set; } = false;

        

        internal void Update(CocApi cocApi, ClanApiModel? downloadedClan)
        {
            lock (this)
            {
                if (downloadedClan == null) return;

                Logger ??= cocApi.Logger;

                if (ReferenceEquals(this, downloadedClan))
                {
                    return;
                }

                UpdateClan(cocApi, downloadedClan);

                UpdateLabels(cocApi, downloadedClan);

                UpdateBadge(cocApi, downloadedClan);

                UpdateLocation(cocApi, downloadedClan);

                AnnounceDonations(cocApi, downloadedClan);

                AnnounceVillageChanges(cocApi, downloadedClan);

                VillagesLeft(cocApi, downloadedClan);

                VillagesJoined(cocApi, downloadedClan);
            }
        }

        private void AnnounceVillageChanges(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<LeagueChange> leagueChanges = new List<LeagueChange>();

            List<RoleChange> roleChanges = new List<RoleChange>();

            foreach (ClanVillageApiModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageApiModel newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) { continue; }

                if ((oldClanVillage.League == null && newClanVillage.League != null) || (oldClanVillage.League != null && newClanVillage.League != null && oldClanVillage.League.Id != newClanVillage.League.Id))
                {
                    leagueChanges.Add(new LeagueChange { Village = oldClanVillage, League = newClanVillage.League });
                }
                        
                if (oldClanVillage.Name != newClanVillage.Name)
                {
                    cocApi.ClanVillageNameChangedEvent(oldClanVillage, newClanVillage.Name);
                }

                if (oldClanVillage.Role != newClanVillage.Role)
                {
                    roleChanges.Add(new RoleChange { Village = oldClanVillage, Role = newClanVillage.Role });
                }
            }

            cocApi.ClanVillagesLeagueChangedEvent(this, leagueChanges);

            cocApi.ClanVillagesRoleChangedEvent(this, roleChanges);
        }

        private void AnnounceDonations(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<Donation> receiving = new List<Donation>();

            List<Donation> donating = new List<Donation>();

            foreach (ClanVillageApiModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageApiModel? newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) continue;

                if (oldClanVillage.DonationsReceived < newClanVillage.DonationsReceived)
                {
                    receiving.Add(new Donation { Village = oldClanVillage, Quantity = newClanVillage.DonationsReceived - oldClanVillage.DonationsReceived });
                }

                if (oldClanVillage.Donations < newClanVillage.Donations)
                {
                    donating.Add(new Donation { Village = oldClanVillage, Quantity = newClanVillage.Donations - oldClanVillage.Donations});
                }

                bool resetSent = false;

                if (!resetSent && oldClanVillage.DonationsReceived > newClanVillage.DonationsReceived || oldClanVillage.Donations > newClanVillage.Donations)
                {
                    cocApi.ClanDonationsResetEvent(this, downloadedClan);

                    resetSent = true;
                }                 
            }

            cocApi.ClanDonationsEvent(this, receiving, donating);

        }

        private void UpdateLabels(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<ClanLabelApiModel> added = new List<ClanLabelApiModel>();

            List<ClanLabelApiModel> removed = new List<ClanLabelApiModel>();

            foreach(var oldLabel in Labels.EmptyIfNull())
            {
                if (!downloadedClan.Labels.Any(l => l.Id == oldLabel.Id))
                {
                    removed.Add(oldLabel);
                }
            }

            foreach(var newLabel in downloadedClan.Labels.EmptyIfNull())
            {
                if (!Labels.Any(l => l.Id == newLabel.Id))
                {
                    added.Add(newLabel);
                }
            }

            if (Labels == null && downloadedClan.Labels != null && added.Count == 0)
            {
                foreach(var newLabel in downloadedClan.Labels)
                {
                    added.Add(newLabel);
                }
            }

            if (downloadedClan.Labels == null && Labels != null && removed.Count == 0)
            {
                foreach(var removedLabel in Labels)
                {
                    removed.Add(removedLabel);
                }
            }

            cocApi.ClanLabelsChangedEvent(downloadedClan, added, removed);
        }

        private void UpdateLocation(CocApi cocApi, ClanApiModel downloadedClan)
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
            }

        }

        private void UpdateBadge(CocApi cocApi, ClanApiModel downloadedClan)
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
            }
        }

        private void UpdateClan(CocApi cocApi, ClanApiModel downloadedClan)
        {
            if (ClanPoints != downloadedClan.ClanPoints)
            {
                cocApi.ClanPointsChangedEvent(this, downloadedClan.ClanPoints);
            }

            if (ClanVersusPoints != downloadedClan.ClanVersusPoints)
            {
                cocApi.ClanVersusPointsChangedEvent(this, downloadedClan.ClanVersusPoints);
            }

            if (ClanLevel != downloadedClan.ClanLevel ||
                Description != downloadedClan.Description ||
                IsWarLogPublic != downloadedClan.IsWarLogPublic ||
                VillageCount != downloadedClan.VillageCount ||
                Name != downloadedClan.Name ||
                RequiredTrophies != downloadedClan.RequiredTrophies ||
                Recruitment != downloadedClan.Recruitment ||
                WarFrequency != downloadedClan.WarFrequency ||
                WarLosses != downloadedClan.WarLosses ||
                WarTies != downloadedClan.WarTies ||
                WarWins != downloadedClan.WarWins ||
                WarWinStreak != downloadedClan.WarWinStreak
            )
            {
                cocApi.ClanChangedEvent(this, downloadedClan);
            }
        }

        private void VillagesJoined(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<ClanVillageApiModel> newVillages = new List<ClanVillageApiModel>();

            if (downloadedClan.Villages == null)
            {
                return;
            }

            foreach (ClanVillageApiModel clanVillage in downloadedClan.Villages)
            {
                if (!Villages?.Any(m => m.VillageTag == clanVillage.VillageTag) == true)
                {
                    newVillages.Add(clanVillage);
                }
            }

            cocApi.VillagesJoinedEvent(this, newVillages);
        }

        private void VillagesLeft(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<ClanVillageApiModel> leftVillages = new List<ClanVillageApiModel>();

            foreach (ClanVillageApiModel clanVillage in Villages.EmptyIfNull())
            {
                if (!downloadedClan.Villages.Any(m => m.VillageTag == clanVillage.VillageTag))
                {
                    leftVillages.Add(clanVillage);
                }
            }

            cocApi.VillagesLeftEvent(this, leftVillages);
        }

        public void Initialize()
        {
            if (!string.IsNullOrEmpty(ClanTag) && Labels != null)
            {
                foreach(var label in Labels)
                {
                    label.ClanTag = ClanTag;
                }
            }

            foreach (var clanVillage in Villages.EmptyIfNull())
            {
                clanVillage.ClanTag = ClanTag;
            }

            if (BadgeUrls != null)
            {
                BadgeUrlsId = BadgeUrls.Id;
            }

            if (Location != null)
            {
                LocationId = Location.Id;
            }
        }
    }
}
