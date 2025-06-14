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
using CocApi.Rest.Client;

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
                    Level.Equals(input.Level)
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
    /// A Json converter for type <see cref="ClanCapitalRaidSeasonClanInfo" />
    /// </summary>
    public class ClanCapitalRaidSeasonClanInfoJsonConverter : JsonConverter<ClanCapitalRaidSeasonClanInfo>
    {
        /// <summary>
        /// Deserializes json to <see cref="ClanCapitalRaidSeasonClanInfo" />
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

            Option<BadgeUrls?> badgeUrls = default;
            Option<int?> level = default;
            Option<string?> name = default;
            Option<string?> tag = default;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? localVarJsonPropertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    switch (localVarJsonPropertyName)
                    {
                        case "badgeUrls":
                            badgeUrls = new Option<BadgeUrls?>(JsonSerializer.Deserialize<BadgeUrls>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "level":
                            level = new Option<int?>(utf8JsonReader.TokenType == JsonTokenType.Null ? (int?)null : utf8JsonReader.GetInt32());
                            break;
                        case "name":
                            name = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "tag":
                            tag = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!badgeUrls.IsSet)
                throw new ArgumentException("Property is required for class ClanCapitalRaidSeasonClanInfo.", nameof(badgeUrls));

            if (!level.IsSet)
                throw new ArgumentException("Property is required for class ClanCapitalRaidSeasonClanInfo.", nameof(level));

            if (!name.IsSet)
                throw new ArgumentException("Property is required for class ClanCapitalRaidSeasonClanInfo.", nameof(name));

            if (!tag.IsSet)
                throw new ArgumentException("Property is required for class ClanCapitalRaidSeasonClanInfo.", nameof(tag));

            if (badgeUrls.IsSet && badgeUrls.Value == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is not nullable for class ClanCapitalRaidSeasonClanInfo.");

            if (level.IsSet && level.Value == null)
                throw new ArgumentNullException(nameof(level), "Property is not nullable for class ClanCapitalRaidSeasonClanInfo.");

            if (name.IsSet && name.Value == null)
                throw new ArgumentNullException(nameof(name), "Property is not nullable for class ClanCapitalRaidSeasonClanInfo.");

            if (tag.IsSet && tag.Value == null)
                throw new ArgumentNullException(nameof(tag), "Property is not nullable for class ClanCapitalRaidSeasonClanInfo.");

            return new ClanCapitalRaidSeasonClanInfo(badgeUrls.Value!, level.Value!.Value!, name.Value!, tag.Value!);
        }

        /// <summary>
        /// Serializes a <see cref="ClanCapitalRaidSeasonClanInfo" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeasonClanInfo"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanCapitalRaidSeasonClanInfo clanCapitalRaidSeasonClanInfo, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(writer, clanCapitalRaidSeasonClanInfo, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="ClanCapitalRaidSeasonClanInfo" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeasonClanInfo"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(Utf8JsonWriter writer, ClanCapitalRaidSeasonClanInfo clanCapitalRaidSeasonClanInfo, JsonSerializerOptions jsonSerializerOptions)
        {
            if (clanCapitalRaidSeasonClanInfo.BadgeUrls == null)
                throw new ArgumentNullException(nameof(clanCapitalRaidSeasonClanInfo.BadgeUrls), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

            if (clanCapitalRaidSeasonClanInfo.Name == null)
                throw new ArgumentNullException(nameof(clanCapitalRaidSeasonClanInfo.Name), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

            if (clanCapitalRaidSeasonClanInfo.Tag == null)
                throw new ArgumentNullException(nameof(clanCapitalRaidSeasonClanInfo.Tag), "Property is required for class ClanCapitalRaidSeasonClanInfo.");

            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeasonClanInfo.BadgeUrls, jsonSerializerOptions);
            writer.WriteNumber("level", clanCapitalRaidSeasonClanInfo.Level);

            writer.WriteString("name", clanCapitalRaidSeasonClanInfo.Name);

            writer.WriteString("tag", clanCapitalRaidSeasonClanInfo.Tag);
        }
    }
}
