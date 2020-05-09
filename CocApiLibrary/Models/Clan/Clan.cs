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
        public bool QueueClanVillages { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download league wars.
        /// </summary>
        [JsonProperty]
        public bool QueueLeagueWars { get; set; } = true;

        /// <summary>
        /// Controls whether this clan will download the current war.
        /// </summary>
        [JsonProperty]        
        public bool QueueCurrentWar { get; set; } = true;

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

        internal void Update(CocApi cocApi, Clan fetched)
        {
            if (ReferenceEquals(this, fetched)) return;

            QueueClanVillages = fetched.QueueClanVillages;

            QueueCurrentWar = fetched.QueueCurrentWar;

            QueueLeagueWars = fetched.QueueLeagueWars;

            UpdateClan(cocApi, fetched);

            UpdateLabels(cocApi, fetched);

            UpdateBadge(cocApi, fetched);

            AnnounceDonations(cocApi, fetched);

            AnnounceVillageChanges(cocApi, fetched);

            VillagesLeft(cocApi, fetched);

            VillagesJoined(cocApi, fetched);            
        }

        private void AnnounceVillageChanges(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(AnnounceVillageChanges), fetched));

            List<LeagueChange> leagueChanges = new List<LeagueChange>();

            List<RoleChange> roleChanges = new List<RoleChange>();

            foreach (ClanVillage queuedVillage in Villages.EmptyIfNull())
            {
                ClanVillage fetchedVillage = fetched.Villages.EmptyIfNull().FirstOrDefault(m => m.VillageTag == queuedVillage.VillageTag);

                if (fetchedVillage == null) continue;

                if (queuedVillage.ClanRank != fetchedVillage.ClanRank ||
                    queuedVillage.ExpLevel != fetchedVillage.ExpLevel ||
                    queuedVillage.LeagueId != fetchedVillage.LeagueId ||
                    queuedVillage.Name != fetchedVillage.Name ||
                    queuedVillage.PreviousClanRank != fetchedVillage.PreviousClanRank ||
                    queuedVillage.Role != fetchedVillage.Role ||
                    queuedVillage.Trophies != fetchedVillage.Trophies ||
                    queuedVillage.VersusTrophies != fetchedVillage.VersusTrophies)
                {
                    cocApi.Clans.OnClanVillageChanged(fetched, fetchedVillage, queuedVillage);
                }
            }
        }

        private void AnnounceDonations(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(AnnounceDonations), fetched));

            List<Donation> receiving = new List<Donation>();

            List<Donation> donating = new List<Donation>();

            foreach (ClanVillage queuedClanVillage in Villages.EmptyIfNull())
            {
                ClanVillage? fetchedClanVillage = fetched.Villages.EmptyIfNull().FirstOrDefault(m => m.VillageTag == queuedClanVillage.VillageTag);

                if (fetchedClanVillage == null) continue;

                if (queuedClanVillage.DonationsReceived < fetchedClanVillage.DonationsReceived)
                {
                    receiving.Add(new Donation { Fetched = fetchedClanVillage, Queued = queuedClanVillage });
                }

                if (queuedClanVillage.Donations < fetchedClanVillage.Donations)
                {
                    donating.Add(new Donation { Fetched = fetchedClanVillage, Queued = queuedClanVillage});
                }                 
            }

            cocApi.Clans.OnDonation(fetched, receiving, donating);

        }

        private void UpdateLabels(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(UpdateLabels), fetched));

            List<ClanLabel> added = new List<ClanLabel>();

            List<ClanLabel> removed = new List<ClanLabel>();

            foreach(var queuedLabel in Labels.EmptyIfNull())
            {
                if (!fetched.Labels.EmptyIfNull().Any(l => l.Id == queuedLabel.Id))
                {
                    removed.Add(queuedLabel);
                }
            }

            foreach(var fetchedLabel in fetched.Labels.EmptyIfNull())
            {
                if (!Labels.EmptyIfNull().Any(l => l.Id == fetchedLabel.Id))
                {
                    added.Add(fetchedLabel);
                }
            }

            if (Labels == null && fetched.Labels != null && added.Count == 0)
            {
                foreach(var fetchedLabel in fetched.Labels)
                {
                    added.Add(fetchedLabel);
                }
            }

            if (fetched.Labels == null && Labels != null && removed.Count == 0)
            {
                foreach(var queuedLabel in Labels)
                {
                    removed.Add(queuedLabel);
                }
            }

            cocApi.Clans.OnLabelsChanged(fetched, added, removed);
        }

        private void UpdateBadge(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(UpdateBadge), fetched));

            if (fetched.BadgeUrl == null && BadgeUrl != null |
                fetched.BadgeUrl?.Large != BadgeUrl?.Large ||
                fetched.BadgeUrl?.Medium != BadgeUrl?.Medium ||
                fetched.BadgeUrl?.Small != BadgeUrl?.Small)
            {
                cocApi.Clans.OnBadgeUrlChanged(fetched, this);
            }
        }

        private void UpdateClan(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(UpdateClan), fetched));

            if (ClanLevel != fetched.ClanLevel ||
                Description != fetched.Description ||
                IsWarLogPublic != fetched.IsWarLogPublic ||
                VillageCount != fetched.VillageCount ||
                Name != fetched.Name ||
                RequiredTrophies != fetched.RequiredTrophies ||
                Recruitment != fetched.Recruitment ||
                WarFrequency != fetched.WarFrequency ||
                WarLosses != fetched.WarLosses ||
                WarTies != fetched.WarTies ||
                WarWins != fetched.WarWins ||
                WarWinStreak != fetched.WarWinStreak ||
                ClanPoints != fetched.ClanPoints ||
                ClanVersusPoints != fetched.ClanVersusPoints||
                LocationId != fetched.LocationId || 
                WarLeagueId != fetched.WarLeagueId
            )
            {
                cocApi.Clans.OnClanChanged(fetched, this);
            }
        }

        private void VillagesJoined(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(VillagesJoined), fetched));

            List<ClanVillage> newVillages = new List<ClanVillage>();

            if (fetched.Villages == null)
                return;

            foreach (ClanVillage clanVillage in fetched.Villages.EmptyIfNull())
            {
                if (Villages?.EmptyIfNull().Any(m => m.VillageTag == clanVillage.VillageTag) == false)
                {
                    newVillages.Add(clanVillage);
                }
            }

            cocApi.Clans.OnClanVillagesJoined(fetched, newVillages);
        }

        private void VillagesLeft(CocApi cocApi, Clan fetched)
        {
            cocApi.OnLog(new ClanLogEventArgs(nameof(Clan), nameof(VillagesLeft), fetched));

            List<ClanVillage> leftVillages = new List<ClanVillage>();

            foreach (ClanVillage clanVillage in Villages.EmptyIfNull())
            {
                if (fetched.Villages.EmptyIfNull().Any(m => m.VillageTag == clanVillage.VillageTag) == false)
                {
                    leftVillages.Add(clanVillage);
                }
            }

            cocApi.Clans.OnClanVillagesLeft(fetched, leftVillages);
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
