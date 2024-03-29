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
        /// Gets or Sets MemberList
        /// </summary>
        [JsonPropertyName("memberList")]
        public List<ClanMember> Members { get; }
    }
}
