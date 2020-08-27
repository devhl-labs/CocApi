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
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = CocApi.Client.OpenAPIDateConverter;

namespace CocApi.Model
{
/// <summary>
    /// PlayerVersusRanking
    /// </summary>
    [DataContract]
    public partial class PlayerVersusRanking :  IEquatable<PlayerVersusRanking>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerVersusRanking" /> class.
        /// </summary>
        /// <param name="clan">clan.</param>
        /// <param name="versusBattleWins">versusBattleWins.</param>
        /// <param name="tag">tag.</param>
        /// <param name="name">name.</param>
        /// <param name="expLevel">expLevel.</param>
        /// <param name="rank">rank.</param>
        /// <param name="previousRank">previousRank.</param>
        /// <param name="versusTrophies">versusTrophies.</param>
        public PlayerVersusRanking(PlayerRankingClan clan = default(PlayerRankingClan), int versusBattleWins = default(int), string tag = default(string), string name = default(string), int expLevel = default(int), int rank = default(int), int previousRank = default(int), int versusTrophies = default(int))
        {
            this.Clan = clan;
            this.VersusBattleWins = versusBattleWins;
            this.Tag = tag;
            this.Name = name;
            this.ExpLevel = expLevel;
            this.Rank = rank;
            this.PreviousRank = previousRank;
            this.VersusTrophies = versusTrophies;
        }
        
        /// <summary>
        /// Gets or Sets Clan
        /// </summary>
        [DataMember(Name="clan", EmitDefaultValue=false)]
        public PlayerRankingClan Clan { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets VersusBattleWins
        /// </summary>
        [DataMember(Name="versusBattleWins", EmitDefaultValue=false)]
        public int VersusBattleWins { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [DataMember(Name="tag", EmitDefaultValue=false)]
        public string Tag { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets ExpLevel
        /// </summary>
        [DataMember(Name="expLevel", EmitDefaultValue=false)]
        public int ExpLevel { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Rank
        /// </summary>
        [DataMember(Name="rank", EmitDefaultValue=false)]
        public int Rank { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets PreviousRank
        /// </summary>
        [DataMember(Name="previousRank", EmitDefaultValue=false)]
        public int PreviousRank { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets VersusTrophies
        /// </summary>
        [DataMember(Name="versusTrophies", EmitDefaultValue=false)]
        public int VersusTrophies { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PlayerVersusRanking {\n");
            sb.Append("  Clan: ").Append(Clan).Append("\n");
            sb.Append("  VersusBattleWins: ").Append(VersusBattleWins).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  ExpLevel: ").Append(ExpLevel).Append("\n");
            sb.Append("  Rank: ").Append(Rank).Append("\n");
            sb.Append("  PreviousRank: ").Append(PreviousRank).Append("\n");
            sb.Append("  VersusTrophies: ").Append(VersusTrophies).Append("\n");
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
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as PlayerVersusRanking);
        }

        /// <summary>
        /// Returns true if PlayerVersusRanking instances are equal
        /// </summary>
        /// <param name="input">Instance of PlayerVersusRanking to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlayerVersusRanking input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Clan == input.Clan ||
                    (this.Clan != null &&
                    this.Clan.Equals(input.Clan))
                ) && 
                (
                    this.VersusBattleWins == input.VersusBattleWins ||
                    this.VersusBattleWins.Equals(input.VersusBattleWins)
                ) && 
                (
                    this.Tag == input.Tag ||
                    (this.Tag != null &&
                    this.Tag.Equals(input.Tag))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.ExpLevel == input.ExpLevel ||
                    this.ExpLevel.Equals(input.ExpLevel)
                ) && 
                (
                    this.Rank == input.Rank ||
                    this.Rank.Equals(input.Rank)
                ) && 
                (
                    this.PreviousRank == input.PreviousRank ||
                    this.PreviousRank.Equals(input.PreviousRank)
                ) && 
                (
                    this.VersusTrophies == input.VersusTrophies ||
                    this.VersusTrophies.Equals(input.VersusTrophies)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                return hashCode;
            }
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
