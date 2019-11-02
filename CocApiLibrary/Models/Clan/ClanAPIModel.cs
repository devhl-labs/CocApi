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
    public class ClanApiModel : SwallowDelegates, IClanApiModel, IDownloadable
    {
        // IClanApiModel
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
        public virtual ClanBadgeUrlModel? BadgeUrls { get; set; }

        public int ClanLevel { get; set; }

        public virtual LocationApiModel? Location { get; set; }

        private IEnumerable<ClanLabelApiModel>? _labels;

        [ForeignKey(nameof(ClanTag))]
        public virtual IEnumerable<ClanLabelApiModel>? Labels
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
        public virtual IList<ClanVillageApiModel>? Villages { get; set; }

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
        public Dictionary<string, ICurrentWarApiModel> Wars { get; set; } = new Dictionary<string, ICurrentWarApiModel>();






        public DateTime UpdateAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime Expires { get; set; }

        public string EncodedUrl { get; set; } = string.Empty;

        public DateTime? CacheExpiresAtUtc { get; set; }

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

        

        internal void Update(CocApi cocApi, ClanApiModel downloadedClan)
        {
            lock (_updateLock)
            {
                Logger ??= cocApi.Logger;

                if (ReferenceEquals(this, downloadedClan))
                {
                    return;
                }

                Swallow(() => UpdateClan(cocApi, downloadedClan), nameof(UpdateClan));

                Swallow(() => UpdateLabels(cocApi, downloadedClan), nameof(UpdateLabels));

                Swallow(() => UpdateBadge(cocApi, downloadedClan), nameof(UpdateBadge));

                Swallow(() => UpdateLocation(cocApi, downloadedClan), nameof(UpdateLocation));

                Swallow(() => AnnounceDonations(cocApi, downloadedClan), nameof(AnnounceDonations));

                Swallow(() => AnnounceVillageChanges(cocApi, downloadedClan), nameof(AnnounceVillageChanges));

                //Swallow(() => UpdateVillages(downloadedClan), nameof(UpdateVillages));

                Swallow(() => VillagesLeft(cocApi, downloadedClan), nameof(VillagesLeft));

                Swallow(() => VillagesJoined(cocApi, downloadedClan), nameof(VillagesJoined));

                UpdateAtUtc = downloadedClan.UpdateAtUtc;

                Expires = downloadedClan.Expires;
            }
        }

        private void AnnounceVillageChanges(CocApi cocApi, ClanApiModel downloadedClan)
        {
            Dictionary<string, Tuple<ClanVillageApiModel, VillageLeagueApiModel>> leagueChanges = new Dictionary<string, Tuple<ClanVillageApiModel, VillageLeagueApiModel>>();

            Dictionary<string, Tuple<ClanVillageApiModel, Role>> roleChanges = new Dictionary<string, Tuple<ClanVillageApiModel, Role>>();


            foreach (ClanVillageApiModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageApiModel newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

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

        //private void UpdateVillages(ClanApiModel downloadedClan)
        //{
        //    foreach (ClanVillageApiModel oldClanVillage in Villages.EmptyIfNull())
        //    {
        //        ClanVillageApiModel newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

        //        if (newClanVillage == null) { continue; }

        //        oldClanVillage.League = newClanVillage.League;

        //        oldClanVillage.Name = newClanVillage.Name;

        //        oldClanVillage.Role = newClanVillage.Role;

        //        oldClanVillage.ExpLevel = newClanVillage.ExpLevel;

        //        oldClanVillage.ClanRank = newClanVillage.ClanRank;

        //        oldClanVillage.PreviousClanRank = newClanVillage.PreviousClanRank;

        //        oldClanVillage.Donations = newClanVillage.Donations;

        //        oldClanVillage.DonationsReceived = newClanVillage.DonationsReceived;

        //        oldClanVillage.Trophies = newClanVillage.Trophies;

        //        oldClanVillage.VersusTrophies = newClanVillage.VersusTrophies;
        //    }

        //}

        private void AnnounceDonations(CocApi cocApi, ClanApiModel downloadedClan)
        {
            Dictionary<string, Tuple<ClanVillageApiModel, int>> receiving = new Dictionary<string, Tuple<ClanVillageApiModel, int>>();

            Dictionary<string, Tuple<ClanVillageApiModel, int>> donating = new Dictionary<string, Tuple<ClanVillageApiModel, int>>();

            foreach(ClanVillageApiModel oldClanVillage in Villages.EmptyIfNull())
            {
                ClanVillageApiModel? newClanVillage = downloadedClan.Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

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

        private void UpdateLabels(CocApi cocApi, ClanApiModel downloadedClan)
        {
            if (Labels == null && downloadedClan.Labels == null) return;

            if (Labels != null && Labels.Count() > 0 && (downloadedClan.Labels == null || downloadedClan.Labels.Count() == 0))
            {
                cocApi.ClanLabelsRemovedEvent(downloadedClan, Labels);

                //Labels = downloadedClan.Labels;
            }
            else if ((Labels == null || Labels.Count() == 0) && downloadedClan.Labels != null && downloadedClan.Labels.Count() > 0)
            {
                cocApi.ClanLabelsAddedEvent(downloadedClan, downloadedClan.Labels);

                //Labels = downloadedClan.Labels;
            }
            else
            {
                List<ClanLabelApiModel> added = new List<ClanLabelApiModel>();

                List<ClanLabelApiModel> removed = new List<ClanLabelApiModel>();

                foreach (ClanLabelApiModel labelApiModel in Labels.EmptyIfNull())
                {
                    if (!downloadedClan.Labels.Any(l => l.Id == labelApiModel.Id))
                    {
                        removed.Add(labelApiModel);
                    }
                }

                foreach (ClanLabelApiModel labelApiModel in downloadedClan.Labels.EmptyIfNull())
                {
                    if (!Labels.Any(l => l.Id == labelApiModel.Id))
                    {
                        added.Add(labelApiModel);
                    }
                }

                cocApi.ClanLabelsRemovedEvent(downloadedClan, removed);

                cocApi.ClanLabelsAddedEvent(downloadedClan, added);

                //Labels = downloadedClan.Labels;
            }
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

                //Location = downloadedClan.Location;
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

                //BadgeUrls = downloadedClan.BadgeUrls;
            }
        }

        private void UpdateClan(CocApi cocApi, ClanApiModel downloadedClan)
        {
            if (ClanPoints != downloadedClan.ClanPoints)
            {
                cocApi.ClanPointsChangedEvent(this, downloadedClan.ClanPoints);

                //ClanPoints = downloadedClan.ClanPoints;
            }

            if (ClanVersusPoints != downloadedClan.ClanVersusPoints)
            {
                cocApi.ClanVersusPointsChangedEvent(this, downloadedClan.ClanVersusPoints);

                //ClanVersusPoints = downloadedClan.ClanVersusPoints;
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

                //ClanLevel = downloadedClan.ClanLevel;
                //Description = downloadedClan.Description;
                //IsWarLogPublic = downloadedClan.IsWarLogPublic;
                //VillageCount = downloadedClan.VillageCount;
                //Name = downloadedClan.Name;
                //RequiredTrophies = downloadedClan.RequiredTrophies;
                //Recruitment = downloadedClan.Recruitment;
                //WarFrequency = downloadedClan.WarFrequency;
                //WarLosses = downloadedClan.WarLosses;
                //WarTies = downloadedClan.WarWins;
                //WarWinStreak = downloadedClan.WarWinStreak;
            }
        }

        private void VillagesJoined(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<ClanVillageApiModel> newVillages = new List<ClanVillageApiModel>();

            if (downloadedClan.Villages == null)
            {
                return;
            }

            //if (Villages == null)
            //{
            //    Villages = new List<ClanVillageApiModel>();
            //}

            foreach (ClanVillageApiModel clanVillage in downloadedClan.Villages)
            {
                if (!Villages?.Any(m => m.VillageTag == clanVillage.VillageTag) == true)
                {
                    newVillages.Add(clanVillage);

                    //Villages.Add(clanVillage);
                }
            }

            cocApi.VillagesJoinedEvent(this, newVillages);
        }

        private void VillagesLeft(CocApi cocApi, ClanApiModel downloadedClan)
        {
            List<ClanVillageApiModel> leftVillages = new List<ClanVillageApiModel>();

            //if (Villages == null)
            //{
            //    return;
            //}

            foreach (ClanVillageApiModel clanVillage in Villages.EmptyIfNull())
            {
                if (!downloadedClan.Villages.Any(m => m.VillageTag == clanVillage.VillageTag))
                {
                    leftVillages.Add(clanVillage);
                }
            }

            foreach (ClanVillageApiModel clanVillage in leftVillages)
            {
                //Villages.Remove(clanVillage);
            }

            cocApi.VillagesLeftEvent(this, leftVillages);
        }

        private void SetRelationalProperties()
        {
            if (!string.IsNullOrEmpty(ClanTag) && _labels != null)
            {
                foreach(var label in _labels)
                {
                    label.ClanTag = ClanTag;
                }
            }
        }
    }
}
