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
    /// ClanRanking
    /// </summary>
    public partial class ClanRanking : IEquatable<ClanRanking?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanRanking" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="clanPoints">clanPoints</param>
        /// <param name="members">members</param>
        /// <param name="name">name</param>
        /// <param name="previousRank">previousRank</param>
        /// <param name="rank">rank</param>
        /// <param name="tag">tag</param>
        /// <param name="location">location</param>
        [JsonConstructor]
        internal ClanRanking(BadgeUrls badgeUrls, int clanLevel, int clanPoints, int members, string name, int previousRank, int rank, string tag, Location? location = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (clanLevel == null)
                throw new ArgumentNullException("clanLevel is a required property for ClanRanking and cannot be null.");

            if (clanPoints == null)
                throw new ArgumentNullException("clanPoints is a required property for ClanRanking and cannot be null.");

            if (members == null)
                throw new ArgumentNullException("members is a required property for ClanRanking and cannot be null.");

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for ClanRanking and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for ClanRanking and cannot be null.");

            if (rank == null)
                throw new ArgumentNullException("rank is a required property for ClanRanking and cannot be null.");

            if (previousRank == null)
                throw new ArgumentNullException("previousRank is a required property for ClanRanking and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for ClanRanking and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            ClanPoints = clanPoints;
            Members = members;
            Name = name;
            PreviousRank = previousRank;
            Rank = rank;
            Tag = tag;
            Location = location;
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
        /// Gets or Sets ClanPoints
        /// </summary>
        [JsonPropertyName("clanPoints")]
        public int ClanPoints { get; }

        /// <summary>
        /// Gets or Sets Members
        /// </summary>
        [JsonPropertyName("members")]
        public int Members { get; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or Sets PreviousRank
        /// </summary>
        [JsonPropertyName("previousRank")]
        public int PreviousRank { get; }

        /// <summary>
        /// Gets or Sets Rank
        /// </summary>
        [JsonPropertyName("rank")]
        public int Rank { get; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        [JsonPropertyName("location")]
        public Location? Location { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanRanking {\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  ClanPoints: ").Append(ClanPoints).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  PreviousRank: ").Append(PreviousRank).Append("\n");
            sb.Append("  Rank: ").Append(Rank).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  Location: ").Append(Location).Append("\n");
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
            return this.Equals(input as ClanRanking);
        }

        /// <summary>
        /// Returns true if ClanRanking instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanRanking to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanRanking? input)
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
                    ClanPoints == input.ClanPoints ||
                    (ClanPoints != null &&
                    ClanPoints.Equals(input.ClanPoints))
                ) && 
                (
                    Members == input.Members ||
                    (Members != null &&
                    Members.Equals(input.Members))
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    PreviousRank == input.PreviousRank ||
                    (PreviousRank != null &&
                    PreviousRank.Equals(input.PreviousRank))
                ) && 
                (
                    Rank == input.Rank ||
                    (Rank != null &&
                    Rank.Equals(input.Rank))
                ) && 
                (
                    Tag == input.Tag ||
                    (Tag != null &&
                    Tag.Equals(input.Tag))
                ) && 
                (
                    Location == input.Location ||
                    (Location != null &&
                    Location.Equals(input.Location))
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
                hashCode = (hashCode * 59) + ClanPoints.GetHashCode();
                hashCode = (hashCode * 59) + Members.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + PreviousRank.GetHashCode();
                hashCode = (hashCode * 59) + Rank.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();

                if (Location != null)
                    hashCode = (hashCode * 59) + Location.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanRanking
    /// </summary>
    public class ClanRankingJsonConverter : JsonConverter<ClanRanking>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanRanking Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            BadgeUrls badgeUrls = default;
            int clanLevel = default;
            int clanPoints = default;
            int members = default;
            string name = default;
            int previousRank = default;
            int rank = default;
            string tag = default;
            Location location = default;

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
                                utf8JsonReader.TryGetInt32(out clanLevel);
                            break;
                        case "clanPoints":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out clanPoints);
                            break;
                        case "members":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out members);
                            break;
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        case "previousRank":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out previousRank);
                            break;
                        case "rank":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                utf8JsonReader.TryGetInt32(out rank);
                            break;
                        case "tag":
                            tag = utf8JsonReader.GetString();
                            break;
                        case "location":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                location = JsonSerializer.Deserialize<Location>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        default:
                            break;
                    }
                }
            }

            return new ClanRanking(badgeUrls, clanLevel, clanPoints, members, name, previousRank, rank, tag, location);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanRanking"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanRanking clanRanking, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, clanRanking.BadgeUrls, jsonSerializerOptions);
            writer.WriteNumber("clanLevel", clanRanking.ClanLevel);
            writer.WriteNumber("clanPoints", clanRanking.ClanPoints);
            writer.WriteNumber("members", clanRanking.Members);
            writer.WriteString("name", clanRanking.Name);
            writer.WriteNumber("previousRank", clanRanking.PreviousRank);
            writer.WriteNumber("rank", clanRanking.Rank);
            writer.WriteString("tag", clanRanking.Tag);
            writer.WritePropertyName("location");
            JsonSerializer.Serialize(writer, clanRanking.Location, jsonSerializerOptions);

            writer.WriteEndObject();
        }
    }
}
