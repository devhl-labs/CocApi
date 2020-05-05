using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using devhl.CocApi.Exceptions;
using devhl.CocApi.Models.War;
using Newtonsoft.Json;


namespace devhl.CocApi.Models.Clan
{
    public class Clan : Downloadable, IClan
    {
        public static string Url(string clanTag)
        {
            if (CocApi.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(formattedTag)}";
        }

        public static string Url(ClanQueryOptions clanQueryOptions)
        {
            if (clanQueryOptions.ClanName == null || clanQueryOptions.ClanName.Length < 3)
                throw new ArgumentException("The clan name must be longer than three characters.");

            string url = $"https://api.clashofclans.com/v1/clans?";

            if (clanQueryOptions.ClanName != null)
                url = $"{url}name={Uri.EscapeDataString(clanQueryOptions.ClanName)}&";

            if (clanQueryOptions.WarFrequency != null)

                url = $"{url}warFrequency={clanQueryOptions.WarFrequency}&";

            if (clanQueryOptions.LocationId != null)

                url = $"{url}locationId={clanQueryOptions.LocationId}&";

            if (clanQueryOptions.MinVillages != null)

                url = $"{url}minMembers={clanQueryOptions.MinVillages}&";

            if (clanQueryOptions.MaxVillages != null)

                url = $"{url}maxMembers={clanQueryOptions.MaxVillages}&";

            if (clanQueryOptions.MinClanPoints != null)

                url = $"{url}minClanPoints={clanQueryOptions.MinClanPoints}&";

            if (clanQueryOptions.MinClanLevel != null)

                url = $"{url}minClanLevel={clanQueryOptions.MinClanLevel}&";

            if (clanQueryOptions.Limit != null)

                url = $"{url}limit={clanQueryOptions.Limit}&";

            if (clanQueryOptions.After != null)

                url = $"{url}after={clanQueryOptions.After}&";

            if (clanQueryOptions.Before != null)

                url = $"{url}before={clanQueryOptions.Before}&";

            if (url.EndsWith("&"))
                url = url[0..^1];

            return url;
        }

        [JsonProperty("tag")]
        public string ClanTag { get; internal set; } = string.Empty;

        [JsonProperty]
        public string Name { get; internal set; } = string.Empty;

        [JsonProperty("badgeUrls")]
        public BadgeUrl? BadgeUrl { get; internal set; }

        [JsonProperty]
        public Location? Location { get; internal set; }

        [JsonProperty]
        public int? LocationId { get; internal set; } 

        [JsonProperty]
        public IEnumerable<ClanLabel>? Labels { get; internal set; }

        [JsonProperty]
        public int ClanLevel { get; internal set; }

        /// <summary>
        /// Controls whether this clan will download villages.
        /// </summary>
        [JsonProperty]
        public bool DownloadClanVillages { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download league wars.
        /// </summary>
        [JsonProperty]
        public bool DownloadLeagueWars { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download the current war.
        /// </summary>
        [JsonProperty]        
        public bool DownloadCurrentWar { get; set; } = true;

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

        [JsonProperty]
        public WarLeague? WarLeague { get; private set; }

        [JsonProperty]
        public int WarLeagueId { get; internal set; }

        public ConcurrentDictionary<string, CurrentWar> Wars { get; internal set; } = new ConcurrentDictionary<string, CurrentWar>();        

        internal void Update(CocApi cocApi, Clan? storedClan)
        {
            if (storedClan == null || ReferenceEquals(this, storedClan)) return;

            DownloadClanVillages = storedClan.DownloadClanVillages;

            DownloadCurrentWar = storedClan.DownloadCurrentWar;

            DownloadLeagueWars = storedClan.DownloadLeagueWars;

            UpdateClan(cocApi, storedClan);

            UpdateLabels(cocApi, storedClan);

            UpdateBadge(cocApi, storedClan);

            UpdateLocation(cocApi, storedClan);

            AnnounceDonations(cocApi, storedClan);

            AnnounceVillageChanges(cocApi, storedClan);

            VillagesLeft(cocApi, storedClan);

            VillagesJoined(cocApi, storedClan);            
        }

        private void AnnounceVillageChanges(CocApi cocApi, Clan storedClan)
        {
            List<LeagueChange> leagueChanges = new List<LeagueChange>();

            List<RoleChange> roleChanges = new List<RoleChange>();

            foreach (ClanVillage oldClanVillage in storedClan.Villages.EmptyIfNull())
            {
                ClanVillage newClanVillage = Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) continue;

                if ((oldClanVillage.League == null && newClanVillage.League != null) || (oldClanVillage.League != null && newClanVillage.League != null && oldClanVillage.League.Id != newClanVillage.League.Id))
                {
                    leagueChanges.Add(new LeagueChange { Village = newClanVillage, OldLeague = oldClanVillage.League });
                }
                        
                if (oldClanVillage.Name != newClanVillage.Name)
                {
                    cocApi.Clans.ClanVillageNameChangedEvent(newClanVillage, oldClanVillage.Name);
                }

                if (oldClanVillage.Role != newClanVillage.Role)
                {
                    roleChanges.Add(new RoleChange { Village = newClanVillage, OldRole = oldClanVillage.Role });
                }
            }

            cocApi.Clans.ClanVillagesLeagueChangedEvent(this, leagueChanges);

            cocApi.Clans.OnClanVillagesRoleChanged(this, roleChanges);
        }

        private void AnnounceDonations(CocApi cocApi, Clan storedClan)
        {
            List<Donation> receiving = new List<Donation>();

            List<Donation> donating = new List<Donation>();

            foreach (ClanVillage oldClanVillage in storedClan.Villages.EmptyIfNull())
            {
                ClanVillage? newClanVillage = Villages.FirstOrDefault(m => m.VillageTag == oldClanVillage.VillageTag);

                if (newClanVillage == null) continue;

                if (oldClanVillage.DonationsReceived < newClanVillage.DonationsReceived)
                {
                    receiving.Add(new Donation { Village = newClanVillage, Increase = newClanVillage.DonationsReceived - oldClanVillage.DonationsReceived });
                }

                if (oldClanVillage.Donations < newClanVillage.Donations)
                {
                    donating.Add(new Donation { Village = newClanVillage, Increase = newClanVillage.Donations - oldClanVillage.Donations});
                }                 
            }

            cocApi.Clans.OnDonation(this, receiving, donating);

        }

        private void UpdateLabels(CocApi cocApi, Clan storedClan)
        {
            List<ClanLabel> added = new List<ClanLabel>();

            List<ClanLabel> removed = new List<ClanLabel>();

            foreach(var oldLabel in storedClan.Labels.EmptyIfNull())
            {
                if (!Labels.Any(l => l.Id == oldLabel.Id))
                {
                    removed.Add(oldLabel);
                }
            }

            foreach(var newLabel in Labels.EmptyIfNull())
            {
                if (!storedClan.Labels.Any(l => l.Id == newLabel.Id))
                {
                    added.Add(newLabel);
                }
            }

            if (storedClan.Labels == null && Labels != null && added.Count == 0)
            {
                foreach(var newLabel in Labels)
                {
                    added.Add(newLabel);
                }
            }

            if (Labels == null && storedClan.Labels != null && removed.Count == 0)
            {
                foreach(var removedLabel in storedClan.Labels)
                {
                    removed.Add(removedLabel);
                }
            }

            cocApi.Clans.OnLabelsChanged(this, added, removed);
        }

        private void UpdateLocation(CocApi cocApi, Clan storedClan)
        {
            if (storedClan.LocationId != LocationId)
                cocApi.Clans.LocationChangedEvent(storedClan, this);
            //if (storedClan.Location == null && Location != null)
            //{
            //    cocApi.Clans.LocationChangedEvent(storedClan, this);
            //    return;
            //}

            //if (storedClan.Location == null && Location != null ||
            //    storedClan.Location?.CountryCode != Location?.CountryCode ||
            //    storedClan.Location?.Id != Location?.Id ||
            //    storedClan.Location?.IsCountry != Location?.IsCountry ||
            //    storedClan.Location?.Name != Location?.Name)
            //{
            //    cocApi.Clans.LocationChangedEvent(storedClan, this);
            //}
        }

        private void UpdateBadge(CocApi cocApi, Clan storedClan)
        {
            if (storedClan.BadgeUrl == null && BadgeUrl != null)
            {
                cocApi.Clans.OnBadgeUrlChanged(this, storedClan);
                return;
            }

            if (storedClan.BadgeUrl == null && BadgeUrl != null |
                storedClan.BadgeUrl?.Large != BadgeUrl?.Large ||
                storedClan.BadgeUrl?.Medium != BadgeUrl?.Medium ||
                storedClan.BadgeUrl?.Small != BadgeUrl?.Small)
            {
                cocApi.Clans.OnBadgeUrlChanged(storedClan, this);
            }
        }

        private void UpdateClan(CocApi cocApi, Clan storedClan)
        {
            if (storedClan.ClanPoints != ClanPoints)
            {
                cocApi.Clans.ClanPointsChangedEvent(this, ClanPoints - storedClan.ClanPoints);
            }

            if (storedClan.ClanVersusPoints != ClanVersusPoints)
            {
                cocApi.Clans.ClanVersusPointsChangedEvent(this, ClanVersusPoints - storedClan.ClanVersusPoints);
            }

            if (ClanLevel != storedClan.ClanLevel ||
                Description != storedClan.Description ||
                IsWarLogPublic != storedClan.IsWarLogPublic ||
                VillageCount != storedClan.VillageCount ||
                Name != storedClan.Name ||
                RequiredTrophies != storedClan.RequiredTrophies ||
                Recruitment != storedClan.Recruitment ||
                WarFrequency != storedClan.WarFrequency ||
                WarLosses != storedClan.WarLosses ||
                WarTies != storedClan.WarTies ||
                WarWins != storedClan.WarWins ||
                WarWinStreak != storedClan.WarWinStreak
            )
            {
                cocApi.Clans.OnClanChanged(storedClan, this);
            }

            if (WarLeague?.Id != storedClan.WarLeague?.Id)
            {
                cocApi.Clans.OnWarLeagueChanged(this, WarLeague);
            }
        }

        private void VillagesJoined(CocApi cocApi, Clan storedClan)
        {
            List<ClanVillage> newVillages = new List<ClanVillage>();

            if (Villages == null)
            {
                return;
            }

            foreach (ClanVillage clanVillage in Villages)
            {
                if (storedClan.Villages?.Any(m => m.VillageTag == clanVillage.VillageTag) == false)
                {
                    newVillages.Add(clanVillage);
                }
            }

            cocApi.Clans.OnClanVillagesJoined(this, newVillages);
        }

        private void VillagesLeft(CocApi cocApi, Clan storedClan)
        {
            List<ClanVillage> leftVillages = new List<ClanVillage>();

            foreach (ClanVillage clanVillage in storedClan.Villages.EmptyIfNull())
            {
                if (Villages.Any(m => m.VillageTag == clanVillage.VillageTag) == false)
                {
                    leftVillages.Add(clanVillage);
                }
            }

            cocApi.Clans.OnClanVillagesLeft(this, leftVillages);
        }

        public void Initialize(CocApi cocApi)
        {
            EncodedUrl = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(ClanTag)}";

            if (!string.IsNullOrEmpty(ClanTag) && Labels != null)
            {
                foreach(var label in Labels) label.ClanTag = ClanTag;
            }

            foreach (var clanVillage in Villages.EmptyIfNull())
            {
                clanVillage.ClanTag = ClanTag;

                clanVillage.Initialize(cocApi);
            }

            if (BadgeUrl != null) 
                BadgeUrl.ClanTag = ClanTag;

            if (Location != null) 
                LocationId = Location.Id;

            if (WarLeague != null)
                WarLeagueId = WarLeague.Id;
        }

        public override string ToString() => $"{ClanTag} {Name}";
    }
}
