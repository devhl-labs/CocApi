using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace CocApi.Model
{
    public partial class Clan : IEquatable<Clan?>
    {
        public string ClanProfileUrl => Clash.ClanProfileUrl(Tag);

        public static string Url(string clanTag)
        {
            if (Clash.TryFormatTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}";
        }

        public static List<Donation> DonationsReceived(Clan stored, Clan fetched)
        {
            List<Donation> results = new List<Donation>();

            foreach(ClanMember storedMember in stored.Members)
            {
                ClanMember fetchedMember = fetched.Members.FirstOrDefault(m => m.Tag == storedMember.Tag);

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
            List<Donation> results = new List<Donation>();

            foreach (ClanMember storedMember in stored.Members)
            {
                ClanMember fetchedMember = fetched.Members.FirstOrDefault(m => m.Tag == storedMember.Tag);

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
            List<ClanMember> results = new List<ClanMember>();

            foreach (ClanMember storedMember in stored.Members.EmptyIfNull())
            {
                ClanMember fetchedMember = fetched.Members.FirstOrDefault(m => m.Tag == storedMember.Tag);

                if (fetchedMember == null)
                    results.Add(storedMember);
            }

            return results;
        }

        public List<ClanMember> ClanMembersLeft(Clan fetched) => ClanMembersLeft(this, fetched);

        public static List<ClanMember> ClanMembersJoined(Clan stored, Clan fetched)
        {
            List<ClanMember> results = new List<ClanMember>();

            foreach (ClanMember fetchedMember in fetched.Members.EmptyIfNull())
            {
                ClanMember storedMember = stored.Members.FirstOrDefault(m => m.Tag == fetchedMember.Tag);

                if (storedMember == null)
                    results.Add(fetchedMember);
            }

            return results;
        }

        public List<ClanMember> ClanMembersJoined(Clan fetched) => ClanMembersJoined(this, fetched);

        ///// <summary>
        ///// Gets or Sets MemberList
        ///// </summary>
        //[DataMember(Name = "memberList", EmitDefaultValue = false)]
        ////[JsonProperty("memberList")]
        //public List<ClanMember> Members { get; private set; }

        ///// <summary>
        ///// Gets or Sets WarFrequency
        ///// </summary>
        //[DataMember(Name = "warFrequency", EmitDefaultValue = false)]
        //public WarFrequency? WarFrequency { get; private set; }

        ////[DataMember(Name = "members", EmitDefaultValue = false)]
        //////[JsonProperty("members")]
        ////public int Members { get; set; }

        ///// <summary>
        ///// Gets or Sets Location
        ///// </summary>
        //[DataMember(Name = "location", EmitDefaultValue = false)]
        //public Location? Location { get; private set; }

        ///// <summary>
        ///// Gets or Sets Type
        ///// </summary>
        //[DataMember(Name = "type", EmitDefaultValue = false)]
        //public RecruitingType? Type { get; private set; }

        //public override bool Equals(object? obj)
        //{
        //    return Equals(obj as Clan);
        //}

        //public bool Equals(Clan? other)
        //{
        //    return other != null &&
        //           Tag == other.Tag;
        //}

        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(Tag);
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Clan" /> class.
        ///// </summary>
        ///// <param name="warLeague">warLeague.</param>
        ///// <param name="memberList">memberList.</param>
        ///// <param name="requiredTrophies">requiredTrophies.</param>
        ///// <param name="clanVersusPoints">clanVersusPoints.</param>
        ///// <param name="tag">tag.</param>
        ///// <param name="isWarLogPublic">isWarLogPublic.</param>
        ///// <param name="warFrequency">warFrequency.</param>
        ///// <param name="clanLevel">clanLevel.</param>
        ///// <param name="warWinStreak">warWinStreak.</param>
        ///// <param name="warWins">warWins.</param>
        ///// <param name="warTies">warTies.</param>
        ///// <param name="warLosses">warLosses.</param>
        ///// <param name="clanPoints">clanPoints.</param>
        ///// <param name="labels">labels.</param>
        ///// <param name="name">name.</param>
        ///// <param name="location">location.</param>
        ///// <param name="type">type.</param>
        ///// <param name="memberList">members.</param>
        ///// <param name="description">description.</param>
        ///// <param name="badgeUrls">badgeUrls.</param>
        //[JsonConstructor]
        //public Clan(WarLeague warLeague = default(WarLeague), List<ClanMember> memberList = default(List<ClanMember>), int requiredTrophies = default(int), int clanVersusPoints = default(int), string tag = default(string), bool isWarLogPublic = default(bool), WarFrequency? warFrequency = default(WarFrequency?), int clanLevel = default(int), int warWinStreak = default(int), int warWins = default(int), int warTies = default(int), int warLosses = default(int), int clanPoints = default(int), List<Label> labels = default(List<Label>), string name = default(string), Location location = default(Location), RecruitingType? type = default(RecruitingType?), /*int members = default(int),*/ string description = default(string), ClanBadgeUrls badgeUrls = default(ClanBadgeUrls))
        //{
        //    this.Members = memberList;
        //    this.WarLeague = warLeague;
        //    //this.MemberList = memberList;
        //    this.RequiredTrophies = requiredTrophies;
        //    this.ClanVersusPoints = clanVersusPoints;
        //    this.Tag = tag;
        //    this.IsWarLogPublic = isWarLogPublic;
        //    this.WarFrequency = warFrequency;
        //    this.ClanLevel = clanLevel;
        //    this.WarWinStreak = warWinStreak;
        //    this.WarWins = warWins;
        //    this.WarTies = warTies;
        //    this.WarLosses = warLosses;
        //    this.ClanPoints = clanPoints;
        //    this.Labels = labels;
        //    this.Name = name;
        //    this.Location = location;
        //    //this.Type = type;
        //    this.Description = description;
        //    this.BadgeUrls = badgeUrls;
        //}

    }
}
