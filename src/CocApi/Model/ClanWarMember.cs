/* 
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = CocApi.Client.OpenAPIDateConverter;

namespace CocApi
{
}



namespace CocApi.Model
{
    /// <summary>
    /// ClanWarMember
    /// </summary>
    [DataContract]
    public partial class ClanWarMember :  IValidatableObject
    {
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
        public ClanWarMember(string tag = default(string), string name = default(string), int mapPosition = default(int), int townhallLevel = default(int), int opponentAttacks = default(int), ClanWarAttack bestOpponentAttack = default(ClanWarAttack), List<ClanWarAttack> attacks = default(List<ClanWarAttack>))
        {
            this.Tag = tag;
            this.Name = name;
            this.MapPosition = mapPosition;
            this.TownhallLevel = townhallLevel;
            this.OpponentAttacks = opponentAttacks;
            this.BestOpponentAttack = bestOpponentAttack;
            this.Attacks = attacks;
        }
        
        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [DataMember(Name="tag", EmitDefaultValue=false)]
        public string Tag { get; private set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; private set; }

        ///// <summary>
        ///// Gets or Sets MapPosition
        ///// </summary>
        //[DataMember(Name="mapPosition", EmitDefaultValue=false)]
        //public int MapPosition { get; private set; }

        /// <summary>
        /// Gets or Sets TownhallLevel
        /// </summary>
        [DataMember(Name="townhallLevel", EmitDefaultValue=false)]
        public int TownhallLevel { get; private set; }

        /// <summary>
        /// Gets or Sets OpponentAttacks
        /// </summary>
        [DataMember(Name="opponentAttacks", EmitDefaultValue=false)]
        public int OpponentAttacks { get; private set; }

        /// <summary>
        /// Gets or Sets BestOpponentAttack
        /// </summary>
        [DataMember(Name="bestOpponentAttack", EmitDefaultValue=false)]
        public ClanWarAttack BestOpponentAttack { get; private set; }

        ///// <summary>
        ///// Gets or Sets Attacks
        ///// </summary>
        //[DataMember(Name="attacks", EmitDefaultValue=false)]
        //public List<ClanWarAttack> Attacks { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ClanWarMember {\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  MapPosition: ").Append(MapPosition).Append("\n");
            sb.Append("  TownhallLevel: ").Append(TownhallLevel).Append("\n");
            sb.Append("  OpponentAttacks: ").Append(OpponentAttacks).Append("\n");
            sb.Append("  BestOpponentAttack: ").Append(BestOpponentAttack).Append("\n");
            sb.Append("  Attacks: ").Append(Attacks).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}