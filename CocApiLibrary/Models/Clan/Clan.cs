﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using devhl.CocApi.Models.Clan;

using devhl.CocApi.Models.War;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using static devhl.CocApi.Enums;


namespace devhl.CocApi.Models.Clan
{
    public class Clan : Downloadable, IClan
    {
        [JsonIgnore]
        internal ILogger? Logger { get; set; }

        [JsonProperty("Tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty]
        public ClanBadgeUrl? BadgeUrls { get; internal set; }

        [JsonProperty]
        public Location? Location { get; internal set; }

        [JsonProperty]
        public int? LocationId { get; internal set; } 

        [JsonProperty]
        public IEnumerable<ClanLabel>? Labels { get; internal set; }

        [JsonProperty]
        public int ClanLevel { get; internal set; }

        [JsonProperty]
        public string? BadgeUrlsId { get; internal set; } = string.Empty;                    

        /// <summary>
        /// Controls whether this clan will download villages.
        /// </summary>
        public bool DownloadVillages { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download league wars.
        /// </summary>
        public bool DownloadLeagueWars { get; set; } = true;

        [JsonProperty("memberList")]
        public IList<ClanVillage>? Villages { get; internal set; }

        [JsonProperty("type")]
        public RecruitmentType Recruitment { get; internal set; }

        [JsonProperty]
        public string Description { get; internal set; } = string.Empty;

        [JsonProperty]
        public int ClanPoints { get; internal set; }

        [JsonProperty]
        public int ClanVersusPoints { get; internal set; }

        [JsonProperty]
        public int RequiredTrophies { get; internal set; }

        [JsonProperty]
        public int WarWinStreak { get; internal set; }

        [JsonProperty]
        public int WarWins { get; internal set; }

        [JsonProperty]
        public int WarTies { get; internal set; }

        [JsonProperty]
        public int WarLosses { get; internal set; }

        [JsonProperty]
        public bool IsWarLogPublic { get; internal set; } = false;

        [JsonProperty("members")]
        public int VillageCount { get; internal set; }

        [JsonProperty]
        public WarFrequency WarFrequency { get; internal set; }


        public Dictionary<string, CurrentWar> Wars { get; internal set; } = new Dictionary<string, CurrentWar>();

        /// <summary>
        /// This is a flag used to prevent all wars from being announced on startup. 
        /// It is set to true after all wars have been downloaded at least once for this clan.
        /// </summary>
        internal bool AnnounceWars { get; set; } = false;        

        internal void Update(CocApi cocApi, Clan? downloadedClan)
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

        private void AnnounceVillageChanges(CocApi cocApi, Clan downloadedClan)
        {
            List<LeagueChange> leagueChanges = new List<LeagueChange>();

            List<RoleChange> roleChanges = new List<RoleChange>();

            foreach (ClanVillage oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillage newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

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

        private void AnnounceDonations(CocApi cocApi, Clan downloadedClan)
        {
            List<Donation> receiving = new List<Donation>();

            List<Donation> donating = new List<Donation>();

            foreach (ClanVillage oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillage? newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

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

        private void UpdateLabels(CocApi cocApi, Clan downloadedClan)
        {
            List<ClanLabel> added = new List<ClanLabel>();

            List<ClanLabel> removed = new List<ClanLabel>();

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

        private void UpdateLocation(CocApi cocApi, Clan downloadedClan)
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

        private void UpdateBadge(CocApi cocApi, Clan downloadedClan)
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

        private void UpdateClan(CocApi cocApi, Clan downloadedClan)
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

        private void VillagesJoined(CocApi cocApi, Clan downloadedClan)
        {
            List<ClanVillage> newVillages = new List<ClanVillage>();

            if (downloadedClan.Villages == null)
            {
                return;
            }

            foreach (ClanVillage clanVillage in downloadedClan.Villages)
            {
                if (!Villages?.Any(m => m.VillageTag == clanVillage.VillageTag) == true)
                {
                    newVillages.Add(clanVillage);
                }
            }

            cocApi.VillagesJoinedEvent(this, newVillages);
        }

        private void VillagesLeft(CocApi cocApi, Clan downloadedClan)
        {
            List<ClanVillage> leftVillages = new List<ClanVillage>();

            foreach (ClanVillage clanVillage in Villages.EmptyIfNull())
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
            EncodedUrl = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(ClanTag)}";

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

                clanVillage.Initialize();
            }

            if (BadgeUrls != null)
            {
                BadgeUrls.Initialize();

                BadgeUrlsId = BadgeUrls.Id;
            }

            if (Location != null)
            {
                LocationId = Location.Id;
            }
        }
    }
}