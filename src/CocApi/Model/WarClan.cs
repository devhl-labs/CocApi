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
    /// WarClan
    /// </summary>
    [DataContract]
    public partial class WarClan :  IEquatable<WarClan>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WarClan" /> class.
        /// </summary>
        /// <param name="destructionPercentage">destructionPercentage.</param>
        /// <param name="tag">tag.</param>
        /// <param name="name">name.</param>
        /// <param name="badgeUrls">badgeUrls.</param>
        /// <param name="clanLevel">clanLevel.</param>
        /// <param name="attacks">attacks.</param>
        /// <param name="stars">stars.</param>
        /// <param name="expEarned">expEarned.</param>
        /// <param name="members">members.</param>
        public WarClan(Object destructionPercentage = default(Object), string tag = default(string), string name = default(string), ClanBadgeUrls badgeUrls = default(ClanBadgeUrls), int clanLevel = default(int), int attacks = default(int), int stars = default(int), int expEarned = default(int), List<ClanWarMember> members = default(List<ClanWarMember>))
        {
            this.DestructionPercentage = destructionPercentage;
            this.Tag = tag;
            this.Name = name;
            this.BadgeUrls = badgeUrls;
            this.ClanLevel = clanLevel;
            this.Attacks = attacks;
            this.Stars = stars;
            this.ExpEarned = expEarned;
            this.Members = members;
        }
        
        /// <summary>
        /// Gets or Sets DestructionPercentage
        /// </summary>
        [DataMember(Name="destructionPercentage", EmitDefaultValue=false)]
        public Object DestructionPercentage { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

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
        /// Gets or Sets BadgeUrls
        /// </summary>
        [DataMember(Name="badgeUrls", EmitDefaultValue=false)]
        public ClanBadgeUrls BadgeUrls { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets ClanLevel
        /// </summary>
        [DataMember(Name="clanLevel", EmitDefaultValue=false)]
        public int ClanLevel { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [DataMember(Name="attacks", EmitDefaultValue=false)]
        public int Attacks { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Stars
        /// </summary>
        [DataMember(Name="stars", EmitDefaultValue=false)]
        public int Stars { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets ExpEarned
        /// </summary>
        [DataMember(Name="expEarned", EmitDefaultValue=false)]
        public int ExpEarned { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Gets or Sets Members
        /// </summary>
        [DataMember(Name="members", EmitDefaultValue=false)]
        public List<ClanWarMember> Members { get; private set; } //{#isReadOnly}private {/isReadOnly}set;

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class WarClan {\n");
            sb.Append("  DestructionPercentage: ").Append(DestructionPercentage).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  Attacks: ").Append(Attacks).Append("\n");
            sb.Append("  Stars: ").Append(Stars).Append("\n");
            sb.Append("  ExpEarned: ").Append(ExpEarned).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
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
            return this.Equals(input as WarClan);
        }

        /// <summary>
        /// Returns true if WarClan instances are equal
        /// </summary>
        /// <param name="input">Instance of WarClan to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(WarClan input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.DestructionPercentage == input.DestructionPercentage ||
                    (this.DestructionPercentage != null &&
                    this.DestructionPercentage.Equals(input.DestructionPercentage))
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
                    this.BadgeUrls == input.BadgeUrls ||
                    (this.BadgeUrls != null &&
                    this.BadgeUrls.Equals(input.BadgeUrls))
                ) && 
                (
                    this.ClanLevel == input.ClanLevel ||
                    this.ClanLevel.Equals(input.ClanLevel)
                ) && 
                (
                    this.Attacks == input.Attacks ||
                    this.Attacks.Equals(input.Attacks)
                ) && 
                (
                    this.Stars == input.Stars ||
                    this.Stars.Equals(input.Stars)
                ) && 
                (
                    this.ExpEarned == input.ExpEarned ||
                    this.ExpEarned.Equals(input.ExpEarned)
                ) && 
                (
                    this.Members == input.Members ||
                    this.Members != null &&
                    input.Members != null &&
                    this.Members.SequenceEqual(input.Members)
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