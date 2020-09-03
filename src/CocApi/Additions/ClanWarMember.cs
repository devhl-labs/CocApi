using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace CocApi.Model
{
    public partial class ClanWarMember
    {

        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [DataMember(Name = "attacks", EmitDefaultValue = false)]
        public List<ClanWarAttack>? Attacks { get; private set; }

        /// <summary>
        /// Gets or Sets MapPosition
        /// </summary>
        [DataMember(Name = "mapPosition", EmitDefaultValue = false)]
        [JsonProperty("mapPosition")]
        public int RosterPosition { get; private set; }

        [DataMember(Name = "mapPositionCorrected", EmitDefaultValue = false)]
        [JsonProperty("mapPositionCorrected")]
        public int MapPosition { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarMember" /> class.
        /// </summary>
        /// <param name="tag">tag.</param>
        /// <param name="name">name.</param>
        /// <param name="mapPosition">mapPosition.</param>
        /// <param name="townhallLevel">townhallLevel.</param>
        /// <param name="opponentAttacks">opponentAttacks.</param>
        /// <param name="bestOpponentAttack">bestOpponentAttack.</param>
        /// <param name="attacks">attacks.</param>
        [JsonConstructor]
        public ClanWarMember(int mapPositionCorrected = default(int), string tag = default(string), string name = default(string), int mapPosition = default(int), int townhallLevel = default(int), int opponentAttacks = default(int), ClanWarAttack bestOpponentAttack = default(ClanWarAttack), List<ClanWarAttack> attacks = default(List<ClanWarAttack>))
        {
            this.Tag = tag;
            this.Name = name;
            this.RosterPosition = mapPosition;
            this.MapPosition = mapPositionCorrected;
            this.TownhallLevel = townhallLevel;
            this.OpponentAttacks = opponentAttacks;
            this.BestOpponentAttack = bestOpponentAttack;
            this.Attacks = attacks;
        }
    }
}
