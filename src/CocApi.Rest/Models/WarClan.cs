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
    /// WarClan
    /// </summary>
    public partial class WarClan : IEquatable<WarClan?>
    {
        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [JsonPropertyName("attacks")]
        public int Attacks { get; }

        /// <summary>
        /// Gets or Sets BadgeUrls
        /// </summary>
        [JsonPropertyName("badgeUrls")]
        public BadgeUrls BadgeUrls { get; }

        /// <summary>
        /// Gets or Sets ClanLevel
        /// </summary>
        [JsonPropertyName("clanLevel")]
        public int ClanLevel { get; }

        /// <summary>
        /// Gets or Sets DestructionPercentage
        /// </summary>
        [JsonPropertyName("destructionPercentage")]
        public float DestructionPercentage { get; }

        /// <summary>
        /// Gets or Sets ExpEarned
        /// </summary>
        [JsonPropertyName("expEarned")]
        public int ExpEarned { get; }

        /// <summary>
        /// Gets or Sets Members
        /// </summary>
        [JsonPropertyName("members")]
        public List<ClanWarMember> Members { get; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or Sets Stars
        /// </summary>
        [JsonPropertyName("stars")]
        public int Stars { get; }

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
            sb.Append("class WarClan {\n");
            sb.Append("  Attacks: ").Append(Attacks).Append("\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  DestructionPercentage: ").Append(DestructionPercentage).Append("\n");
            sb.Append("  ExpEarned: ").Append(ExpEarned).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Stars: ").Append(Stars).Append("\n");
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
            return this.Equals(input as WarClan);
        }

        /// <summary>
        /// Returns true if WarClan instances are equal
        /// </summary>
        /// <param name="input">Instance of WarClan to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(WarClan? input)
        {
            if (input == null)
                return false;

            return 
                (
                    Attacks == input.Attacks ||
                    (Attacks != null &&
                    Attacks.Equals(input.Attacks))
                ) && 
                (
                    BadgeUrls == input.BadgeUrls ||
                    (BadgeUrls != null &&
                    BadgeUrls.Equals(input.BadgeUrls))
                ) && 
                (
                    ClanLevel == input.ClanLevel ||
                    (ClanLevel != null &&
                    ClanLevel.Equals(input.ClanLevel))
                ) && 
                (
                    DestructionPercentage == input.DestructionPercentage ||
                    (DestructionPercentage != null &&
                    DestructionPercentage.Equals(input.DestructionPercentage))
                ) && 
                (
                    ExpEarned == input.ExpEarned ||
                    (ExpEarned != null &&
                    ExpEarned.Equals(input.ExpEarned))
                ) && 
                (
                    Members == input.Members ||
                    Members != null &&
                    input.Members != null &&
                    Members.SequenceEqual(input.Members)
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    Stars == input.Stars ||
                    (Stars != null &&
                    Stars.Equals(input.Stars))
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
                hashCode = (hashCode * 59) + Attacks.GetHashCode();
                hashCode = (hashCode * 59) + BadgeUrls.GetHashCode();
                hashCode = (hashCode * 59) + ClanLevel.GetHashCode();
                hashCode = (hashCode * 59) + DestructionPercentage.GetHashCode();
                hashCode = (hashCode * 59) + ExpEarned.GetHashCode();
                hashCode = (hashCode * 59) + Members.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + Stars.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type WarClan
    /// </summary>
    public class WarClanJsonConverter : JsonConverter<WarClan>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override WarClan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int currentDepth = reader.CurrentDepth;

            if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = reader.TokenType;

            int attacks = default;
            BadgeUrls badgeUrls = default;
            int clanLevel = default;
            float destructionPercentage = default;
            int expEarned = default;
            List<ClanWarMember> members = default;
            string name = default;
            int stars = default;
            string tag = default;

            while (reader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && reader.TokenType == JsonTokenType.EndObject && currentDepth == reader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && reader.TokenType == JsonTokenType.EndArray && currentDepth == reader.CurrentDepth)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "attacks":
                            attacks = reader.GetInt32();
                            break;
                        case "badgeUrls":
                            Utf8JsonReader badgeUrlsReader = reader;
                            badgeUrls = JsonSerializer.Deserialize<BadgeUrls>(ref reader, options);
                            break;
                        case "clanLevel":
                            clanLevel = reader.GetInt32();
                            break;
                        case "destructionPercentage":
                            destructionPercentage = (float)reader.GetDouble();
                            break;
                        case "expEarned":
                            expEarned = reader.GetInt32();
                            break;
                        case "members":
                            Utf8JsonReader membersReader = reader;
                            members = JsonSerializer.Deserialize<List<ClanWarMember>>(ref reader, options);
                            break;
                        case "name":
                            name = reader.GetString();
                            break;
                        case "stars":
                            stars = reader.GetInt32();
                            break;
                        case "tag":
                            tag = reader.GetString();
                            break;
                    }
                }
            }

            return new WarClan(attacks, badgeUrls, clanLevel, destructionPercentage, expEarned, members, name, stars, tag);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warClan"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, WarClan warClan, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("attacks", (int)warClan.Attacks);
            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, warClan.BadgeUrls, options);
            writer.WriteNumber("clanLevel", (int)warClan.ClanLevel);
            writer.WriteNumber("destructionPercentage", (int)warClan.DestructionPercentage);
            writer.WriteNumber("expEarned", (int)warClan.ExpEarned);
            writer.WritePropertyName("members");
            JsonSerializer.Serialize(writer, warClan.Members, options);
            writer.WriteString("name", warClan.Name);
            writer.WriteNumber("stars", (int)warClan.Stars);
            writer.WriteString("tag", warClan.Tag);

            writer.WriteEndObject();
        }
    }
}
