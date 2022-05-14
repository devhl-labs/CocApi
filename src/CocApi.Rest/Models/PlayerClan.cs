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
    /// PlayerClan
    /// </summary>
    public partial class PlayerClan : IEquatable<PlayerClan?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerClan" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="name">name</param>
        /// <param name="tag">tag</param>
        [JsonConstructor]
        internal PlayerClan(BadgeUrls badgeUrls, int clanLevel, string name, string tag)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for PlayerClan and cannot be null.");

            if (clanLevel == null)
                throw new ArgumentNullException("clanLevel is a required property for PlayerClan and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for PlayerClan and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for PlayerClan and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            Name = name;
            Tag = tag;
        }

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
            sb.Append("class PlayerClan {\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
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
            return this.Equals(input as PlayerClan);
        }

        /// <summary>
        /// Returns true if PlayerClan instances are equal
        /// </summary>
        /// <param name="input">Instance of PlayerClan to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlayerClan? input)
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
                    (ClanLevel != null &&
                    ClanLevel.Equals(input.ClanLevel))
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
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type PlayerClan
    /// </summary>
    public class PlayerClanJsonConverter : JsonConverter<PlayerClan>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override PlayerClan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int currentDepth = reader.CurrentDepth;

            if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = reader.TokenType;

            BadgeUrls badgeUrls = default;
            int clanLevel = default;
            string name = default;
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
                        case "badgeUrls":
                            Utf8JsonReader badgeUrlsReader = reader;
                            badgeUrls = JsonSerializer.Deserialize<BadgeUrls>(ref reader, options);
                            break;
                        case "clanLevel":
                            clanLevel = reader.GetInt32();
                            break;
                        case "name":
                            name = reader.GetString();
                            break;
                        case "tag":
                            tag = reader.GetString();
                            break;
                    }
                }
            }

            return new PlayerClan(badgeUrls, clanLevel, name, tag);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="playerClan"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, PlayerClan playerClan, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, playerClan.BadgeUrls, options);
            writer.WriteNumber("clanLevel", (int)playerClan.ClanLevel);
            writer.WriteString("name", playerClan.Name);
            writer.WriteString("tag", playerClan.Tag);

            writer.WriteEndObject();
        }
    }
}
