/*
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
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
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = CocApi.Client.OpenAPIDateConverter;

namespace CocApi.Model
{
    /// <summary>
    /// WarClanLogEntry
    /// </summary>
    [DataContract(Name = "WarClanLogEntry")]
    public partial class WarClanLogEntry : IEquatable<WarClanLogEntry>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WarClanLogEntry" /> class.
        /// </summary>
        /// <param name="destructionPercentage">destructionPercentage.</param>
        /// <param name="tag">tag.</param>
        /// <param name="name">name.</param>
        /// <param name="badgeUrls">badgeUrls.</param>
        /// <param name="clanLevel">clanLevel.</param>
        /// <param name="attacks">attacks.</param>
        /// <param name="stars">stars.</param>
        /// <param name="expEarned">expEarned.</param>
        public WarClanLogEntry(float destructionPercentage, string tag, string name, ClanBadgeUrls badgeUrls, int clanLevel, int attacks, int stars, int expEarned)
        {
            DestructionPercentage = destructionPercentage;
            Tag = tag;
            Name = name;
            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            Attacks = attacks;
            Stars = stars;
            ExpEarned = expEarned;
        }

        /// <summary>
        /// Gets or Sets DestructionPercentage
        /// </summary>
        [DataMember(Name = "destructionPercentage", EmitDefaultValue = false)]
        public float DestructionPercentage { get; private set; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [DataMember(Name = "tag", EmitDefaultValue = false)]
        public string Tag { get; private set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or Sets BadgeUrls
        /// </summary>
        [DataMember(Name = "badgeUrls", EmitDefaultValue = false)]
        public ClanBadgeUrls BadgeUrls { get; private set; }

        /// <summary>
        /// Gets or Sets ClanLevel
        /// </summary>
        [DataMember(Name = "clanLevel", EmitDefaultValue = false)]
        public int ClanLevel { get; private set; }

        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [DataMember(Name = "attacks", EmitDefaultValue = false)]
        public int Attacks { get; private set; }

        /// <summary>
        /// Gets or Sets Stars
        /// </summary>
        [DataMember(Name = "stars", EmitDefaultValue = false)]
        public int Stars { get; private set; }

        /// <summary>
        /// Gets or Sets ExpEarned
        /// </summary>
        [DataMember(Name = "expEarned", EmitDefaultValue = false)]
        public int ExpEarned { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class WarClanLogEntry {\n");
            sb.Append("  DestructionPercentage: ").Append(DestructionPercentage).Append('\n');
            sb.Append("  Tag: ").Append(Tag).Append('\n');
            sb.Append("  Name: ").Append(Name).Append('\n');
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append('\n');
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append('\n');
            sb.Append("  Attacks: ").Append(Attacks).Append('\n');
            sb.Append("  Stars: ").Append(Stars).Append('\n');
            sb.Append("  ExpEarned: ").Append(ExpEarned).Append('\n');
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson(Newtonsoft.Json.JsonSerializerSettings? jsonSerializerSettings = null)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, jsonSerializerSettings ?? CocApi.Client.ClientUtils.JsonSerializerSettings);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object? input)
        {
            return Equals(input as WarClanLogEntry);
        }

        /// <summary>
        /// Returns true if WarClanLogEntry instances are equal
        /// </summary>
        /// <param name="input">Instance of WarClanLogEntry to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(WarClanLogEntry? input)
        {
            if (input == null)
                return false;

            return 
                (
                    DestructionPercentage == input.DestructionPercentage ||
                    DestructionPercentage.Equals(input.DestructionPercentage)
                ) && 
                (
                    Tag == input.Tag ||
                    (Tag != null &&
                    Tag.Equals(input.Tag))
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    BadgeUrls == input.BadgeUrls ||
                    (BadgeUrls != null &&
                    BadgeUrls.Equals(input.BadgeUrls))
                ) && 
                (
                    ClanLevel == input.ClanLevel ||
                    ClanLevel.Equals(input.ClanLevel)
                ) && 
                (
                    Attacks == input.Attacks ||
                    Attacks.Equals(input.Attacks)
                ) && 
                (
                    Stars == input.Stars ||
                    Stars.Equals(input.Stars)
                ) && 
                (
                    ExpEarned == input.ExpEarned ||
                    ExpEarned.Equals(input.ExpEarned)
                );
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

