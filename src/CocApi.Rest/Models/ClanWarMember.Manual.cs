using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class ClanWarMember
    {
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);


        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarMember" /> class.
        /// </summary>
        /// <param name="mapPosition">mapPosition</param>
        /// <param name="name">name</param>
        /// <param name="opponentAttacks">opponentAttacks</param>
        /// <param name="tag">tag</param>
        /// <param name="townhallLevel">townhallLevel</param>
        /// <param name="attacks">attacks</param>
        /// <param name="bestOpponentAttack">bestOpponentAttack</param>
        [JsonConstructor]
        internal ClanWarMember(int mapPosition, string name, int opponentAttacks, string tag, int townhallLevel, List<ClanWarAttack>? attacks = default, ClanWarAttack? bestOpponentAttack = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for ClanWarMember and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for ClanWarMember and cannot be null.");

            if (mapPosition == null)
                throw new ArgumentNullException("mapPosition is a required property for ClanWarMember and cannot be null.");

            if (townhallLevel == null)
                throw new ArgumentNullException("townhallLevel is a required property for ClanWarMember and cannot be null.");

            if (opponentAttacks == null)
                throw new ArgumentNullException("opponentAttacks is a required property for ClanWarMember and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            RosterPosition = mapPosition; // this is intentional. The MapPosition will be caculated in ClanWar#Initialize
            Name = name;
            OpponentAttacks = opponentAttacks;
            Tag = tag;
            TownhallLevel = townhallLevel;
            Attacks = attacks;
            BestOpponentAttack = bestOpponentAttack;
        }


        /// <summary>
        /// Gets or Sets MapPosition
        /// </summary>
        [JsonPropertyName("mapPosition")]
        public int RosterPosition { get; private set; }

        /// <summary>
        /// Gets or Sets MapPosition
        /// </summary>
        [JsonPropertyName("mapPositionCorrected")]
        public int MapPosition { get; internal set; }
    }
}
