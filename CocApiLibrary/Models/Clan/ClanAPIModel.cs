using CocApiLibrary.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;
using static CocApiLibrary.Extensions;

namespace CocApiLibrary.Models
{
    public class ClanAPIModel : SwallowDelegates, IClanAPIModel, IDownloadable
    {
        // IClanAPIModel
        [Key]
        [JsonPropertyName("Tag")]
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

                    SetRelationalProperties();
                }
            }
        }

        public string Name { get; set; } = string.Empty;

        //[NotMapped]
        public virtual BadgeUrlModel? BadgeUrls { get; set; }

        public int ClanLevel { get; set; }

        public virtual LocationAPIModel? Location { get; set; }

        private IEnumerable<ClanLabelAPIModel>? _labels;

        [ForeignKey(nameof(ClanTag))]
        public virtual IEnumerable<ClanLabelAPIModel>? Labels
        {
            get
            {
                return _labels;
            }

            set
            {
                _labels = value;

                SetRelationalProperties();
            }
        }



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

        [JsonPropertyName("memberList")]
        [ForeignKey(nameof(ClanTag))]
        public virtual IList<ClanVillageAPIModel>? Villages { get; set; }

        [JsonPropertyName("type")]
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

        [JsonPropertyName("members")]
        public int VillageCount { get; set; }

        public WarFrequency WarFrequency { get; set; }







        [JsonIgnore]
        [NotMapped]
        public Dictionary<string, ICurrentWarAPIModel> Wars { get; set; } = new Dictionary<string, ICurrentWarAPIModel>();






        public DateTime DateTimeUtc { get; set; } = DateTime.UtcNow;

        public DateTime Expires { get; set; }

        public string EncodedUrl { get; set; } = string.Empty;

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
                Logger = cocApi.Logger;

                Swallow(() => UpdateClan(cocApi, downloadedClan), nameof(UpdateClan));

                Swallow(() => UpdateLabels(cocApi, downloadedClan), nameof(UpdateLabels));

                Swallow(() => UpdateBadge(cocApi, downloadedClan), nameof(UpdateBadge));

                Swallow(() => UpdateLocation(cocApi, downloadedClan), nameof(UpdateLocation));

                Swallow(() => AnnounceDonations(cocApi, downloadedClan), nameof(AnnounceDonations));

                Swallow(() => AnnounceVillageChanges(cocApi, downloadedClan), nameof(AnnounceVillageChanges));

                Swallow(() => UpdateVillages(downloadedClan), nameof(UpdateVillages));

                Swallow(() => VillagesLeft(cocApi, downloadedClan), nameof(VillagesLeft));

                Swallow(() => VillagesJoined(cocApi, downloadedClan), nameof(VillagesJoined));

                DateTimeUtc = downloadedClan.DateTimeUtc;

                Expires = downloadedClan.Expires;
            }
        }

        private void AnnounceVillageChanges(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            Dictionary<string, Tuple<ClanVillageAPIModel, LeagueAPIModel>> leagueChanges = new Dictionary<string, Tuple<ClanVillageAPIModel, LeagueAPIModel>>();

            Dictionary<string, Tuple<ClanVillageAPIModel, Role>> roleChanges = new Dictionary<string, Tuple<ClanVillageAPIModel, Role>>();


            foreach (ClanVillageAPIModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageAPIModel newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) { continue; }

                if ((oldClanVillage.League == null && newClanVillage.League != null) || (oldClanVillage.League != null && newClanVillage.League != null && oldClanVillage.League.Id != newClanVillage.League.Id))
                {
                    leagueChanges.Add(oldClanVillage.VillageTag, Tuple.Create(oldClanVillage, newClanVillage.League));
                }
                        
                if (oldClanVillage.Name != newClanVillage.Name)
                {
                    cocApi.ClanVillageNameChangedEvent(oldClanVillage, newClanVillage.Name);
                }

                if (oldClanVillage.Role != newClanVillage.Role)
                {
                    roleChanges.Add(oldClanVillage.VillageTag, Tuple.Create(oldClanVillage, newClanVillage.Role));
                }
            }

            cocApi.ClanVillagesLeagueChangedEvent(leagueChanges);

            cocApi.ClanVillagesRoleChangedEvent(roleChanges);
        }

        private void UpdateVillages(ClanAPIModel downloadedClan)
        {
            foreach (ClanVillageAPIModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageAPIModel newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) { continue; }

                oldClanVillage.League = newClanVillage.League;

                oldClanVillage.Name = newClanVillage.Name;

                oldClanVillage.Role = newClanVillage.Role;

                oldClanVillage.ExpLevel = newClanVillage.ExpLevel;

                oldClanVillage.ClanRank = newClanVillage.ClanRank;

                oldClanVillage.PreviousClanRank = newClanVillage.PreviousClanRank;

                oldClanVillage.Donations = newClanVillage.Donations;

                oldClanVillage.DonationsReceived = newClanVillage.DonationsReceived;

                oldClanVillage.Trophies = newClanVillage.Trophies;

                oldClanVillage.VersusTrophies = newClanVillage.VersusTrophies;
            }

        }

        private void AnnounceDonations(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            Dictionary<string, Tuple<ClanVillageAPIModel, int>> receiving = new Dictionary<string, Tuple<ClanVillageAPIModel, int>>();

            Dictionary<string, Tuple<ClanVillageAPIModel, int>> donating = new Dictionary<string, Tuple<ClanVillageAPIModel, int>>();

            foreach(ClanVillageAPIModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageAPIModel? newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) continue;

                if (oldClanVillage.DonationsReceived < newClanVillage.DonationsReceived)
                {
                    receiving.Add(oldClanVillage.VillageTag, Tuple.Create(oldClanVillage, newClanVillage.DonationsReceived));
                }

                if (oldClanVillage.Donations < newClanVillage.Donations)
                {
                    donating.Add(oldClanVillage.VillageTag, Tuple.Create(oldClanVillage, newClanVillage.Donations));
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
                List<ClanLabelAPIModel> added = new List<ClanLabelAPIModel>();

                List<ClanLabelAPIModel> removed = new List<ClanLabelAPIModel>();

                foreach (ClanLabelAPIModel labelAPIModel in Labels.EmptyIfNull())
                {
                    if (!downloadedClan.Labels.Any(l => l.Id == labelAPIModel.Id))
                    {
                        removed.Add(labelAPIModel);
                    }
                }

                foreach (ClanLabelAPIModel labelAPIModel in downloadedClan.Labels.EmptyIfNull())
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

                ClanLevel = downloadedClan.ClanLevel;
                Description = downloadedClan.Description;
                IsWarLogPublic = downloadedClan.IsWarLogPublic;
                VillageCount = downloadedClan.VillageCount;
                Name = downloadedClan.Name;
                RequiredTrophies = downloadedClan.RequiredTrophies;
                Recruitment = downloadedClan.Recruitment;
                WarFrequency = downloadedClan.WarFrequency;
                WarLosses = downloadedClan.WarLosses;
                WarTies = downloadedClan.WarWins;
                WarWinStreak = downloadedClan.WarWinStreak;
            }
        }

        private void VillagesJoined(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            List<ClanVillageAPIModel> newVillages = new List<ClanVillageAPIModel>();

            if (downloadedClan.Villages == null)
            {
                return;
            }

            if (Villages == null)
            {
                Villages = new List<ClanVillageAPIModel>();
            }

            foreach (ClanVillageAPIModel clanVillage in downloadedClan.Villages)
            {
                if (!Villages.Any(m => m.VillageTag == clanVillage.VillageTag))
                {
                    newVillages.Add(clanVillage);

                    Villages.Add(clanVillage);
                }
            }

            cocApi.VillagesJoinedEvent(this, newVillages);
        }

        private void VillagesLeft(CocApi cocApi, ClanAPIModel downloadedClan)
        {
            List<ClanVillageAPIModel> leftVillages = new List<ClanVillageAPIModel>();

            if (Villages == null)
            {
                return;
            }

            foreach (ClanVillageAPIModel clanVillage in Villages)
            {
                if (!downloadedClan.Villages.Any(m => m.VillageTag == clanVillage.VillageTag))
                {
                    leftVillages.Add(clanVillage);
                }
            }

            foreach (ClanVillageAPIModel clanVillage in leftVillages)
            {
                Villages.Remove(clanVillage);
            }

            cocApi.VillagesLeftEvent(this, leftVillages);
        }

        private void SetRelationalProperties()
        {
            if (ClanTag != null && Labels != null)
            {
                foreach(var label in Labels)
                {
                    label.ClanTag = ClanTag;
                }
            }
        }
    }
}
