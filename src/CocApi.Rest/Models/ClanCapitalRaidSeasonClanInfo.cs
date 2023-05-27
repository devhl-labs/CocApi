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
    /// ClanCapitalRaidSeasonClanInfo
    /// </summary>
    public partial class ClanCapitalRaidSeasonClanInfo : IEquatable<ClanCapitalRaidSeasonClanInfo?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanCapitalRaidSeasonClanInfo" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="level">level</param>
        /// <param name="name">name</param>
        /// <param name="tag">tag</param>
        [JsonConstructor]
        internal ClanCapitalRaidSeasonClanInfo(BadgeUrls badgeUrls, int level, string name, string tag)
        {
            BadgeUrls = badgeUrls;
            Level = level;
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
        /// Gets or Sets Level
        /// </summary>
        [JsonPropertyName("level")]
        public int Level { get; }

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
            sb.Append("class ClanCapitalRaidSeasonClanInfo {\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  Level: ").Append(Level).Append("\n");
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
            return this.Equals(input as ClanCapitalRaidSeasonClanInfo);
        }

        /// <summary>
        /// Returns true if ClanCapitalRaidSeasonClanInfo instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanCapitalRaidSeasonClanInfo to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanCapitalRaidSeasonClanInfo? input)
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
                    Level == input.Level ||
                    (Level != null &&
                    Level.Equals(input.Level))
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
                hashCode = (hashCode * 59) + Level.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanCapitalRaidSeasonClanInfo
    /// </summary>
    public class ClanCapitalRaidSeasonClanInfoJsonConverter : JsonConverter<ClanCapitalRaidSeasonClanInfo>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanCapitalRaidSeasonClanInfo Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            BadgeUrls? badgeUrls = default;
            int? level = default;
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
                        case "level":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                level = utf8JsonReader.GetInt32();
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

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

            if (level == null)
                throw new ArgumentNullException(nameof(level), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

            if (badgeUrls == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return new ClanCapitalRaidSeasonClanInfo(badgeUrls, level.Value, name, tag);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeasonClanInfo"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanCapitalRaidSeasonClanInfo clanCapitalRaidSeasonClanInfo, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeasonClanInfo.BadgeUrls, jsonSerializerOptions);
            writer.WriteNumber("level", clanCapitalRaidSeasonClanInfo.Level);
            writer.WriteString("name", clanCapitalRaidSeasonClanInfo.Name);
            writer.WriteString("tag", clanCapitalRaidSeasonClanInfo.Tag);

            writer.WriteEndObject();
        }
    }
}
