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
    /// WarClanLogEntry
    /// </summary>
    public partial class WarClanLogEntry : IEquatable<WarClanLogEntry?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WarClanLogEntry" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="destructionPercentage">destructionPercentage</param>
        /// <param name="stars">stars</param>
        /// <param name="attacks">attacks</param>
        /// <param name="expEarned">expEarned</param>
        /// <param name="name">name</param>
        /// <param name="tag">tag</param>
        [JsonConstructor]
        internal WarClanLogEntry(BadgeUrls badgeUrls, int clanLevel, float destructionPercentage, int stars, int? attacks = default, int? expEarned = default, string? name = default, string? tag = default)
        {
            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            DestructionPercentage = destructionPercentage;
            Stars = stars;
            Attacks = attacks;
            ExpEarned = expEarned;
            Name = name;
            Tag = tag;
            OnCreated();
        }

        partial void OnCreated();

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
        /// Gets or Sets Stars
        /// </summary>
        [JsonPropertyName("stars")]
        public int Stars { get; }

        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [JsonPropertyName("attacks")]
        public int? Attacks { get; }

        /// <summary>
        /// Gets or Sets ExpEarned
        /// </summary>
        [JsonPropertyName("expEarned")]
        public int? ExpEarned { get; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [JsonPropertyName("tag")]
        public string? Tag { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class WarClanLogEntry {\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  DestructionPercentage: ").Append(DestructionPercentage).Append("\n");
            sb.Append("  Stars: ").Append(Stars).Append("\n");
            sb.Append("  Attacks: ").Append(Attacks).Append("\n");
            sb.Append("  ExpEarned: ").Append(ExpEarned).Append("\n");
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
            return this.Equals(input as WarClanLogEntry);
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
                    BadgeUrls == input.BadgeUrls ||
                    (BadgeUrls != null &&
                    BadgeUrls.Equals(input.BadgeUrls))
                ) && 
                (
                    ClanLevel == input.ClanLevel ||
                    ClanLevel.Equals(input.ClanLevel)
                ) && 
                (
                    DestructionPercentage == input.DestructionPercentage ||
                    DestructionPercentage.Equals(input.DestructionPercentage)
                ) && 
                (
                    Stars == input.Stars ||
                    Stars.Equals(input.Stars)
                ) && 
                (
                    Attacks == input.Attacks ||
                    Attacks.Equals(input.Attacks)
                ) && 
                (
                    ExpEarned == input.ExpEarned ||
                    ExpEarned.Equals(input.ExpEarned)
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
                hashCode = (hashCode * 59) + BadgeUrls.GetHashCode();
                hashCode = (hashCode * 59) + ClanLevel.GetHashCode();
                hashCode = (hashCode * 59) + DestructionPercentage.GetHashCode();
                hashCode = (hashCode * 59) + Stars.GetHashCode();

                if (Attacks != null)
                    hashCode = (hashCode * 59) + Attacks.GetHashCode();

                if (ExpEarned != null)
                    hashCode = (hashCode * 59) + ExpEarned.GetHashCode();

                if (Name != null)
                    hashCode = (hashCode * 59) + Name.GetHashCode();

                if (Tag != null)
                    hashCode = (hashCode * 59) + Tag.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type <see cref="WarClanLogEntry" />
    /// </summary>
    public class WarClanLogEntryJsonConverter : JsonConverter<WarClanLogEntry>
    {
        /// <summary>
        /// Deserializes json to <see cref="WarClanLogEntry" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override WarClanLogEntry Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            BadgeUrls? badgeUrls = default;
            int? clanLevel = default;
            float? destructionPercentage = default;
            int? stars = default;
            int? attacks = default;
            int? expEarned = default;
            string? name = default;
            string? tag = default;

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
                        case "badgeUrls":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                badgeUrls = JsonSerializer.Deserialize<BadgeUrls>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "clanLevel":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanLevel = utf8JsonReader.GetInt32();
                            break;
                        case "destructionPercentage":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                destructionPercentage = (float)utf8JsonReader.GetDouble();
                            break;
                        case "stars":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                stars = utf8JsonReader.GetInt32();
                            break;
                        case "attacks":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                attacks = utf8JsonReader.GetInt32();
                            break;
                        case "expEarned":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                expEarned = utf8JsonReader.GetInt32();
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

            if (badgeUrls == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is required for class WarClanLogEntry.");

            if (clanLevel == null)
                throw new ArgumentNullException(nameof(clanLevel), "Property is required for class WarClanLogEntry.");

            if (destructionPercentage == null)
                throw new ArgumentNullException(nameof(destructionPercentage), "Property is required for class WarClanLogEntry.");

            if (stars == null)
                throw new ArgumentNullException(nameof(stars), "Property is required for class WarClanLogEntry.");

            return new WarClanLogEntry(badgeUrls, clanLevel.Value, destructionPercentage.Value, stars.Value, attacks, expEarned, name, tag);
        }

        /// <summary>
        /// Serializes a <see cref="WarClanLogEntry" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warClanLogEntry"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, WarClanLogEntry warClanLogEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, warClanLogEntry, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="WarClanLogEntry" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warClanLogEntry"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, WarClanLogEntry warClanLogEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, warClanLogEntry.BadgeUrls, jsonSerializerOptions);
            writer.WriteNumber("clanLevel", warClanLogEntry.ClanLevel);
            writer.WriteNumber("destructionPercentage", warClanLogEntry.DestructionPercentage);
            writer.WriteNumber("stars", warClanLogEntry.Stars);

            if (warClanLogEntry.Attacks != null)
                writer.WriteNumber("attacks", warClanLogEntry.Attacks.Value);
            else
                writer.WriteNull("attacks");

            if (warClanLogEntry.ExpEarned != null)
                writer.WriteNumber("expEarned", warClanLogEntry.ExpEarned.Value);
            else
                writer.WriteNull("expEarned");

            writer.WriteString("name", warClanLogEntry.Name);
            writer.WriteString("tag", warClanLogEntry.Tag);
        }
    }
}
