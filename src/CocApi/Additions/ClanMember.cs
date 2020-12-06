using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanMember
    {
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);

        /// <summary>
        /// Gets or Sets Role
        /// </summary>
        [DataMember(Name = "role", EmitDefaultValue = false)]
        public Role? Role { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClanMember" /> class.
        /// </summary>
        /// <param name="league">league.</param>
        /// <param name="tag">tag.</param>
        /// <param name="name">name.</param>
        /// <param name="role">role.</param>
        /// <param name="expLevel">expLevel.</param>
        /// <param name="clanRank">clanRank.</param>
        /// <param name="previousClanRank">previousClanRank.</param>
        /// <param name="donations">donations.</param>
        /// <param name="donationsReceived">donationsReceived.</param>
        /// <param name="trophies">trophies.</param>
        /// <param name="versusTrophies">versusTrophies.</param>
        public ClanMember(League league = default(League), string tag = default(string), string name = default(string), Role? role = default(Role?), int expLevel = default(int), int clanRank = default(int), int previousClanRank = default(int), int donations = default(int), int donationsReceived = default(int), int trophies = default(int), int versusTrophies = default(int))
        {
            this.League = league;
            this.Tag = tag;
            this.Name = name;
            this.Role = role;
            this.ExpLevel = expLevel;
            this.ClanRank = clanRank;
            this.PreviousClanRank = previousClanRank;
            this.Donations = donations;
            this.DonationsReceived = donationsReceived;
            this.Trophies = trophies;
            this.VersusTrophies = versusTrophies;
        }
    }
}
