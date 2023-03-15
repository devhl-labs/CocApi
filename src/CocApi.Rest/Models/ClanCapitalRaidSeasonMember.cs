// <auto-generated>
/*
 * Clash of Clans API
 *
 * Check out <a href=\"https://developer.clashofclans.com/#/getting-started\" target=\"_parent\">Getting Started</a> for instructions and links to other resources. Clash of Clans API uses <a href=\"https://jwt.io/\" target=\"_blank\">JSON Web Tokens</a> for authorizing the requests. Tokens are created by developers on <a href=\"https://developer.clashofclans.com/#/account\" target=\"_parent\">My Account</a> page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \"Authorization: Bearer API_TOKEN\". 
 *
 * The version of the OpenAPI document: v1
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    /// <summary>
    /// ClanCapitalRaidSeasonMember
    /// </summary>
    public partial class ClanCapitalRaidSeasonMember : IEquatable<ClanCapitalRaidSeasonMember?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanCapitalRaidSeasonMember" /> class.
        /// </summary>
        /// <param name="attackLimit">attackLimit</param>
        /// <param name="attacks">attacks</param>
        /// <param name="bonusAttackLimit">bonusAttackLimit</param>
        /// <param name="capitalResourcesLooted">capitalResourcesLooted</param>
        /// <param name="name">name</param>
        /// <param name="tag">tag</param>
        [JsonConstructor]
        internal ClanCapitalRaidSeasonMember(int attackLimit, int attacks, int bonusAttackLimit, int capitalResourcesLooted, string name, string tag)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for ClanCapitalRaidSeasonMember and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for ClanCapitalRaidSeasonMember and cannot be null.");

            if (attacks == null)
                throw new ArgumentNullException("attacks is a required property for ClanCapitalRaidSeasonMember and cannot be null.");

            if (attackLimit == null)
                throw new ArgumentNullException("attackLimit is a required property for ClanCapitalRaidSeasonMember and cannot be null.");

            if (bonusAttackLimit == null)
                throw new ArgumentNullException("bonusAttackLimit is a required property for ClanCapitalRaidSeasonMember and cannot be null.");

            if (capitalResourcesLooted == null)
                throw new ArgumentNullException("capitalResourcesLooted is a required property for ClanCapitalRaidSeasonMember and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            AttackLimit = attackLimit;
            Attacks = attacks;
            BonusAttackLimit = bonusAttackLimit;
            CapitalResourcesLooted = capitalResourcesLooted;
            Name = name;
            Tag = tag;
        }

        /// <summary>
        /// Gets or Sets AttackLimit
        /// </summary>
        [JsonPropertyName("attackLimit")]
        public int AttackLimit { get; }

        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [JsonPropertyName("attacks")]
        public int Attacks { get; }

        /// <summary>
        /// Gets or Sets BonusAttackLimit
        /// </summary>
        [JsonPropertyName("bonusAttackLimit")]
        public int BonusAttackLimit { get; }

        /// <summary>
        /// Gets or Sets CapitalResourcesLooted
        /// </summary>
        [JsonPropertyName("capitalResourcesLooted")]
        public int CapitalResourcesLooted { get; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanCapitalRaidSeasonMember {\n");
            sb.Append("  AttackLimit: ").Append(AttackLimit).Append("\n");
            sb.Append("  Attacks: ").Append(Attacks).Append("\n");
            sb.Append("  BonusAttackLimit: ").Append(BonusAttackLimit).Append("\n");
            sb.Append("  CapitalResourcesLooted: ").Append(CapitalResourcesLooted).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object? input)
        {
            return this.Equals(input as ClanCapitalRaidSeasonMember);
        }

        /// <summary>
        /// Returns true if ClanCapitalRaidSeasonMember instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanCapitalRaidSeasonMember to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanCapitalRaidSeasonMember? input)
        {
            if (input == null)
                return false;

            return 
                (
                    AttackLimit == input.AttackLimit ||
                    (AttackLimit != null &&
                    AttackLimit.Equals(input.AttackLimit))
                ) && 
                (
                    Attacks == input.Attacks ||
                    (Attacks != null &&
                    Attacks.Equals(input.Attacks))
                ) && 
                (
                    BonusAttackLimit == input.BonusAttackLimit ||
                    (BonusAttackLimit != null &&
                    BonusAttackLimit.Equals(input.BonusAttackLimit))
                ) && 
                (
                    CapitalResourcesLooted == input.CapitalResourcesLooted ||
                    (CapitalResourcesLooted != null &&
                    CapitalResourcesLooted.Equals(input.CapitalResourcesLooted))
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    Tag == input.Tag ||
                    (Tag != null &&
                    Tag.Equals(input.Tag))
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
                hashCode = (hashCode * 59) + AttackLimit.GetHashCode();
                hashCode = (hashCode * 59) + Attacks.GetHashCode();
                hashCode = (hashCode * 59) + BonusAttackLimit.GetHashCode();
                hashCode = (hashCode * 59) + CapitalResourcesLooted.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanCapitalRaidSeasonMember
    /// </summary>
    public class ClanCapitalRaidSeasonMemberJsonConverter : JsonConverter<ClanCapitalRaidSeasonMember>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanCapitalRaidSeasonMember Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int attackLimit = default;
            int attacks = default;
            int bonusAttackLimit = default;
            int capitalResourcesLooted = default;
            string name = default;
            string tag = default;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? propertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    switch (propertyName)
                    {
                        case "attackLimit":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out attackLimit);
                            break;
                        case "attacks":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out attacks);
                            break;
                        case "bonusAttackLimit":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out bonusAttackLimit);
                            break;
                        case "capitalResourcesLooted":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out capitalResourcesLooted);
                            break;
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        case "tag":
                            tag = utf8JsonReader.GetString();
                            break;
                        default:
                            break;
                    }
                }
            }

            return new ClanCapitalRaidSeasonMember(attackLimit, attacks, bonusAttackLimit, capitalResourcesLooted, name, tag);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeasonMember"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanCapitalRaidSeasonMember clanCapitalRaidSeasonMember, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("attackLimit", clanCapitalRaidSeasonMember.AttackLimit);
            writer.WriteNumber("attacks", clanCapitalRaidSeasonMember.Attacks);
            writer.WriteNumber("bonusAttackLimit", clanCapitalRaidSeasonMember.BonusAttackLimit);
            writer.WriteNumber("capitalResourcesLooted", clanCapitalRaidSeasonMember.CapitalResourcesLooted);
            writer.WriteString("name", clanCapitalRaidSeasonMember.Name);
            writer.WriteString("tag", clanCapitalRaidSeasonMember.Tag);

            writer.WriteEndObject();
        }
    }
}
