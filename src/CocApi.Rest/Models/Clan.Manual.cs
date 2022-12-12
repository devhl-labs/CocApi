using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class Clan
    {
        public string ClanProfileUrl => Clash.ClanProfileUrl(Tag);

        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string? formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}";
        }

        public static List<Donation> DonationsReceived(Clan stored, Clan fetched)
        {
            List<Donation> results = new();

            foreach (ClanMember storedMember in stored.Members)
            {
                ClanMember? fetchedMember = fetched.Members.FirstOrDefault(m => m.Tag == storedMember.Tag);

                if (fetchedMember == null)
                    continue;

                if (storedMember.DonationsReceived < fetchedMember.DonationsReceived)
                    results.Add(new Donation(fetchedMember, storedMember.DonationsReceived, fetchedMember.DonationsReceived));
            }
            return results;
        }

        public List<Donation> DonationsReceived(Clan fetched) => DonationsReceived(this, fetched);

        public static List<Donation> Donations(Clan stored, Clan fetched)
        {
            List<Donation> results = new();

            foreach (ClanMember storedMember in stored.Members)
            {
                ClanMember? fetchedMember = fetched.Members.FirstOrDefault(m => m.Tag == storedMember.Tag);

                if (fetchedMember == null)
                    continue;

                if (storedMember.Donations < fetchedMember.Donations)
                    results.Add(new Donation(fetchedMember, storedMember.Donations, fetchedMember.Donations));
            }
            return results;
        }

        public List<Donation> Donations(Clan fetched) => Donations(this, fetched);

        public static List<ClanMember> ClanMembersLeft(Clan stored, Clan fetched)
        {
            List<ClanMember> results = new();

            foreach (ClanMember storedMember in stored.Members.EmptyIfNull())
            {
                ClanMember? fetchedMember = fetched.Members.FirstOrDefault(m => m.Tag == storedMember.Tag);

                if (fetchedMember == null)
                    results.Add(storedMember);
            }

            return results;
        }

        public List<ClanMember> ClanMembersLeft(Clan fetched) => ClanMembersLeft(this, fetched);

        public static List<ClanMember> ClanMembersJoined(Clan stored, Clan fetched)
        {
            List<ClanMember> results = new();

            foreach (ClanMember fetchedMember in fetched.Members.EmptyIfNull())
            {
                ClanMember? storedMember = stored.Members.FirstOrDefault(m => m.Tag == fetchedMember.Tag);

                if (storedMember == null)
                    results.Add(fetchedMember);
            }

            return results;
        }

        public List<ClanMember> ClanMembersJoined(Clan fetched) => ClanMembersJoined(this, fetched);

        /// <summary>
        /// Initializes a new instance of the <see cref="Clan" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="clanPoints">clanPoints</param>
        /// <param name="clanVersusPoints">clanVersusPoints</param>
        /// <param name="description">description</param>
        /// <param name="isWarLogPublic">isWarLogPublic</param>
        /// <param name="labels">labels</param>
        /// <param name="members">members</param>
        /// <param name="name">name</param>
        /// <param name="requiredTrophies">requiredTrophies</param>
        /// <param name="tag">tag</param>
        /// <param name="warLeague">warLeague</param>
        /// <param name="warLosses">warLosses</param>
        /// <param name="warTies">warTies</param>
        /// <param name="warWinStreak">warWinStreak</param>
        /// <param name="warWins">warWins</param>
        /// <param name="chatLanguage">chatLanguage</param>
        /// <param name="location">location</param>
        /// <param name="type">type</param>
        /// <param name="warFrequency">warFrequency</param>
        [JsonConstructor]
        public Clan(BadgeUrls badgeUrls, ClanCapital clanCapital, int clanLevel, int clanPoints, int clanVersusPoints, string description, bool isWarLogPublic, List<Label> labels, List<ClanMember> members, string name, int requiredTrophies, string tag, WarLeague warLeague, int? warLosses, int? warTies, int warWinStreak, int warWins, Language? chatLanguage = default, Location? location = default, RecruitingType? type = default, WarFrequency? warFrequency = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            //if (warLeague == null)
            //    throw new ArgumentNullException("warLeague is a required property for Clan and cannot be null.");

            if (requiredTrophies == null)
                throw new ArgumentNullException("requiredTrophies is a required property for Clan and cannot be null.");

            if (clanVersusPoints == null)
                throw new ArgumentNullException("clanVersusPoints is a required property for Clan and cannot be null.");

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for Clan and cannot be null.");

            if (isWarLogPublic == null)
                throw new ArgumentNullException("isWarLogPublic is a required property for Clan and cannot be null.");

            if (clanLevel == null)
                throw new ArgumentNullException("clanLevel is a required property for Clan and cannot be null.");

            if (warWinStreak == null)
                throw new ArgumentNullException("warWinStreak is a required property for Clan and cannot be null.");

            if (warWins == null)
                throw new ArgumentNullException("warWins is a required property for Clan and cannot be null.");

            if (clanPoints == null)
                throw new ArgumentNullException("clanPoints is a required property for Clan and cannot be null.");

            if (labels == null)
                throw new ArgumentNullException("labels is a required property for Clan and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for Clan and cannot be null.");

            if (members == null)
                throw new ArgumentNullException("members is a required property for Clan and cannot be null.");

            if (description == null)
                throw new ArgumentNullException("description is a required property for Clan and cannot be null.");

            if (clanCapital == null)
                throw new ArgumentNullException("clanCapital is a required property for Clan and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for Clan and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            BadgeUrls = badgeUrls;
            ClanCapital = clanCapital;
            ClanLevel = clanLevel;
            ClanPoints = clanPoints;
            ClanVersusPoints = clanVersusPoints;
            Description = description;
            IsWarLogPublic = isWarLogPublic;
            Labels = labels;
            Members = members;
            Name = name;
            RequiredTrophies = requiredTrophies;
            Tag = tag;
            WarLeague = warLeague;
            WarLosses = warLosses;
            WarTies = warTies;
            WarWinStreak = warWinStreak;
            WarWins = warWins;
            ChatLanguage = chatLanguage;
            Location = location;
            Type = type;
            WarFrequency = warFrequency;
        }

        /// <summary>
        /// Gets or Sets MemberList
        /// </summary>
        [JsonPropertyName("memberList")]
        public List<ClanMember> Members { get; }
    }
}
