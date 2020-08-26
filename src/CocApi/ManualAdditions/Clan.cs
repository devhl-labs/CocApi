using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocApi.Model
{
    public partial class Clan
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}";
        }

        public static List<Donation> DonationsReceived(Clan stored, Clan fetched)
        {
            List<Donation> results = new List<Donation>();

            foreach(ClanMember storedMember in stored.MemberList.EmptyIfNull())
            {
                ClanMember fetchedMember = fetched.MemberList.FirstOrDefault(m => m.Tag == storedMember.Tag);

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

            foreach (ClanMember storedMember in stored.MemberList.EmptyIfNull())
            {
                ClanMember fetchedMember = fetched.MemberList.FirstOrDefault(m => m.Tag == storedMember.Tag);

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

            foreach (ClanMember storedMember in stored.MemberList.EmptyIfNull())
            {
                ClanMember fetchedMember = fetched.MemberList.FirstOrDefault(m => m.Tag == storedMember.Tag);

                if (fetchedMember == null)
                    results.Add(storedMember);
            }

            return results;
        }

        public List<ClanMember> ClanMembersLeft(Clan fetched) => ClanMembersLeft(this, fetched);

        public static List<ClanMember> ClanMembersJoined(Clan stored, Clan fetched)
        {
            List<ClanMember> results = new List<ClanMember>();

            foreach (ClanMember fetchedMember in fetched.MemberList.EmptyIfNull())
            {
                ClanMember storedMember = fetched.MemberList.FirstOrDefault(m => m.Tag == fetchedMember.Tag);

                if (storedMember == null)
                    results.Add(fetchedMember);
            }

            return results;
        }

        public List<ClanMember> ClanMembersJoined(Clan fetched) => ClanMembersJoined(this, fetched);
    }

    public class Donation
    {
        public ClanMember ClanMember { get; }

        public int OldValue { get; }

        public int NewValue { get; }

        public Donation(ClanMember clanMember, int oldValue, int newValue)
        {
            ClanMember = clanMember;

            OldValue = oldValue;

            NewValue = newValue;
        }

        public int Quanity
        {
            get
            {
                return NewValue - OldValue;
            }
        }
    }
}
