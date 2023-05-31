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
    /// ClanListEntry
    /// </summary>
    public partial class ClanListEntry : IEquatable<ClanListEntry?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanListEntry" /> class.
        /// </summary>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanBuilderBasePoints">clanBuilderBasePoints</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="clanPoints">clanPoints</param>
        /// <param name="isWarLogPublic">isWarLogPublic</param>
        /// <param name="labels">labels</param>
        /// <param name="members">members</param>
        /// <param name="name">name</param>
        /// <param name="requiredTrophies">requiredTrophies</param>
        /// <param name="tag">tag</param>
        /// <param name="warLeague">warLeague</param>
        /// <param name="warWinStreak">warWinStreak</param>
        /// <param name="warWins">warWins</param>
        /// <param name="chatLanguage">chatLanguage</param>
        /// <param name="location">location</param>
        /// <param name="type">type</param>
        /// <param name="warFrequency">warFrequency</param>
        /// <param name="warLosses">warLosses</param>
        /// <param name="warTies">warTies</param>
        [JsonConstructor]
        internal ClanListEntry(BadgeUrls badgeUrls, int clanBuilderBasePoints, int clanLevel, int clanPoints, bool isWarLogPublic, List<Label> labels, int members, string name, int requiredTrophies, string tag, WarLeague warLeague, int warWinStreak, int warWins, Language? chatLanguage = default, Location? location = default, RecruitingType? type = default, WarFrequency? warFrequency = default, int? warLosses = default, int? warTies = default)
        {
            BadgeUrls = badgeUrls;
            ClanBuilderBasePoints = clanBuilderBasePoints;
            ClanLevel = clanLevel;
            ClanPoints = clanPoints;
            IsWarLogPublic = isWarLogPublic;
            Labels = labels;
            Members = members;
            Name = name;
            RequiredTrophies = requiredTrophies;
            Tag = tag;
            WarLeague = warLeague;
            WarWinStreak = warWinStreak;
            WarWins = warWins;
            ChatLanguage = chatLanguage;
            Location = location;
            Type = type;
            WarFrequency = warFrequency;
            WarLosses = warLosses;
            WarTies = warTies;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [JsonPropertyName("type")]
        public RecruitingType? Type { get; }

        /// <summary>
        /// Gets or Sets WarFrequency
        /// </summary>
        [JsonPropertyName("warFrequency")]
        public WarFrequency? WarFrequency { get; }

        /// <summary>
        /// Gets or Sets BadgeUrls
        /// </summary>
        [JsonPropertyName("badgeUrls")]
        public BadgeUrls BadgeUrls { get; }

        /// <summary>
        /// Gets or Sets ClanBuilderBasePoints
        /// </summary>
        [JsonPropertyName("clanBuilderBasePoints")]
        public int ClanBuilderBasePoints { get; }

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
        /// Gets or Sets IsWarLogPublic
        /// </summary>
        [JsonPropertyName("isWarLogPublic")]
        public bool IsWarLogPublic { get; }

        /// <summary>
        /// Gets or Sets Labels
        /// </summary>
        [JsonPropertyName("labels")]
        public List<Label> Labels { get; }

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
        /// Gets or Sets RequiredTrophies
        /// </summary>
        [JsonPropertyName("requiredTrophies")]
        public int RequiredTrophies { get; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; }

        /// <summary>
        /// Gets or Sets WarLeague
        /// </summary>
        [JsonPropertyName("warLeague")]
        public WarLeague WarLeague { get; }

        /// <summary>
        /// Gets or Sets WarWinStreak
        /// </summary>
        [JsonPropertyName("warWinStreak")]
        public int WarWinStreak { get; }

        /// <summary>
        /// Gets or Sets WarWins
        /// </summary>
        [JsonPropertyName("warWins")]
        public int WarWins { get; }

        /// <summary>
        /// Gets or Sets ChatLanguage
        /// </summary>
        [JsonPropertyName("chatLanguage")]
        public Language? ChatLanguage { get; }

        /// <summary>
        /// Gets or Sets Location
        /// </summary>
        [JsonPropertyName("location")]
        public Location? Location { get; }

        /// <summary>
        /// Gets or Sets WarLosses
        /// </summary>
        [JsonPropertyName("warLosses")]
        public int? WarLosses { get; }

        /// <summary>
        /// Gets or Sets WarTies
        /// </summary>
        [JsonPropertyName("warTies")]
        public int? WarTies { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanListEntry {\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  ClanBuilderBasePoints: ").Append(ClanBuilderBasePoints).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  ClanPoints: ").Append(ClanPoints).Append("\n");
            sb.Append("  IsWarLogPublic: ").Append(IsWarLogPublic).Append("\n");
            sb.Append("  Labels: ").Append(Labels).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  RequiredTrophies: ").Append(RequiredTrophies).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  WarLeague: ").Append(WarLeague).Append("\n");
            sb.Append("  WarWinStreak: ").Append(WarWinStreak).Append("\n");
            sb.Append("  WarWins: ").Append(WarWins).Append("\n");
            sb.Append("  ChatLanguage: ").Append(ChatLanguage).Append("\n");
            sb.Append("  Location: ").Append(Location).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  WarFrequency: ").Append(WarFrequency).Append("\n");
            sb.Append("  WarLosses: ").Append(WarLosses).Append("\n");
            sb.Append("  WarTies: ").Append(WarTies).Append("\n");
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
            return this.Equals(input as ClanListEntry);
        }

        /// <summary>
        /// Returns true if ClanListEntry instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanListEntry to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanListEntry? input)
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
                    ClanBuilderBasePoints == input.ClanBuilderBasePoints ||
                    ClanBuilderBasePoints.Equals(input.ClanBuilderBasePoints)
                ) && 
                (
                    ClanLevel == input.ClanLevel ||
                    ClanLevel.Equals(input.ClanLevel)
                ) && 
                (
                    ClanPoints == input.ClanPoints ||
                    ClanPoints.Equals(input.ClanPoints)
                ) && 
                (
                    IsWarLogPublic == input.IsWarLogPublic ||
                    IsWarLogPublic.Equals(input.IsWarLogPublic)
                ) && 
                (
                    Labels == input.Labels ||
                    Labels != null &&
                    input.Labels != null &&
                    Labels.SequenceEqual(input.Labels)
                ) && 
                (
                    Members == input.Members ||
                    Members.Equals(input.Members)
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    RequiredTrophies == input.RequiredTrophies ||
                    RequiredTrophies.Equals(input.RequiredTrophies)
                ) && 
                (
                    Tag == input.Tag ||
                    (Tag != null &&
                    Tag.Equals(input.Tag))
                ) && 
                (
                    WarLeague == input.WarLeague ||
                    (WarLeague != null &&
                    WarLeague.Equals(input.WarLeague))
                ) && 
                (
                    WarWinStreak == input.WarWinStreak ||
                    WarWinStreak.Equals(input.WarWinStreak)
                ) && 
                (
                    WarWins == input.WarWins ||
                    WarWins.Equals(input.WarWins)
                ) && 
                (
                    ChatLanguage == input.ChatLanguage ||
                    (ChatLanguage != null &&
                    ChatLanguage.Equals(input.ChatLanguage))
                ) && 
                (
                    Location == input.Location ||
                    (Location != null &&
                    Location.Equals(input.Location))
                ) && 
                (
                    Type == input.Type ||
                    Type.Equals(input.Type)
                ) && 
                (
                    WarFrequency == input.WarFrequency ||
                    WarFrequency.Equals(input.WarFrequency)
                ) && 
                (
                    WarLosses == input.WarLosses ||
                    (WarLosses != null &&
                    WarLosses.Equals(input.WarLosses))
                ) && 
                (
                    WarTies == input.WarTies ||
                    (WarTies != null &&
                    WarTies.Equals(input.WarTies))
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
                hashCode = (hashCode * 59) + ClanBuilderBasePoints.GetHashCode();
                hashCode = (hashCode * 59) + ClanLevel.GetHashCode();
                hashCode = (hashCode * 59) + ClanPoints.GetHashCode();
                hashCode = (hashCode * 59) + IsWarLogPublic.GetHashCode();
                hashCode = (hashCode * 59) + Labels.GetHashCode();
                hashCode = (hashCode * 59) + Members.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + RequiredTrophies.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();
                hashCode = (hashCode * 59) + WarLeague.GetHashCode();
                hashCode = (hashCode * 59) + WarWinStreak.GetHashCode();
                hashCode = (hashCode * 59) + WarWins.GetHashCode();

                if (ChatLanguage != null)
                    hashCode = (hashCode * 59) + ChatLanguage.GetHashCode();

                if (Location != null)
                    hashCode = (hashCode * 59) + Location.GetHashCode();

                if (Type != null)
                    hashCode = (hashCode * 59) + Type.GetHashCode();

                if (WarFrequency != null)
                    hashCode = (hashCode * 59) + WarFrequency.GetHashCode();

                if (WarLosses != null)
                    hashCode = (hashCode * 59) + WarLosses.GetHashCode();

                if (WarTies != null)
                    hashCode = (hashCode * 59) + WarTies.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanListEntry
    /// </summary>
    public class ClanListEntryJsonConverter : JsonConverter<ClanListEntry>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanListEntry Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            BadgeUrls? badgeUrls = default;
            int? clanBuilderBasePoints = default;
            int? clanLevel = default;
            int? clanPoints = default;
            bool? isWarLogPublic = default;
            List<Label>? labels = default;
            int? members = default;
            string? name = default;
            int? requiredTrophies = default;
            string? tag = default;
            WarLeague? warLeague = default;
            int? warWinStreak = default;
            int? warWins = default;
            Language? chatLanguage = default;
            Location? location = default;
            RecruitingType? type = default;
            WarFrequency? warFrequency = default;
            int? warLosses = default;
            int? warTies = default;

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
                        case "clanBuilderBasePoints":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanBuilderBasePoints = utf8JsonReader.GetInt32();
                            break;
                        case "clanLevel":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanLevel = utf8JsonReader.GetInt32();
                            break;
                        case "clanPoints":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanPoints = utf8JsonReader.GetInt32();
                            break;
                        case "isWarLogPublic":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isWarLogPublic = utf8JsonReader.GetBoolean();
                            break;
                        case "labels":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                labels = JsonSerializer.Deserialize<List<Label>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "members":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                members = utf8JsonReader.GetInt32();
                            break;
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        case "requiredTrophies":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                requiredTrophies = utf8JsonReader.GetInt32();
                            break;
                        case "tag":
                            tag = utf8JsonReader.GetString();
                            break;
                        case "warLeague":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                warLeague = JsonSerializer.Deserialize<WarLeague>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "warWinStreak":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                warWinStreak = utf8JsonReader.GetInt32();
                            break;
                        case "warWins":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                warWins = utf8JsonReader.GetInt32();
                            break;
                        case "chatLanguage":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                chatLanguage = JsonSerializer.Deserialize<Language>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "location":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                location = JsonSerializer.Deserialize<Location>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "type":
                            string typeRawValue = utf8JsonReader.GetString();
                            type = RecruitingTypeConverter.FromStringOrDefault(typeRawValue);
                            break;
                        case "warFrequency":
                            string warFrequencyRawValue = utf8JsonReader.GetString();
                            warFrequency = WarFrequencyConverter.FromStringOrDefault(warFrequencyRawValue);
                            break;
                        case "warLosses":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                warLosses = utf8JsonReader.GetInt32();
                            break;
                        case "warTies":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                warTies = utf8JsonReader.GetInt32();
                            break;
                        default:
                            break;
                    }
                }
            }

            if (warLeague == null)
                throw new ArgumentNullException(nameof(warLeague), "Property is required for class ClanListEntry.");

            if (requiredTrophies == null)
                throw new ArgumentNullException(nameof(requiredTrophies), "Property is required for class ClanListEntry.");

            if (clanBuilderBasePoints == null)
                throw new ArgumentNullException(nameof(clanBuilderBasePoints), "Property is required for class ClanListEntry.");

            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Property is required for class ClanListEntry.");

            if (isWarLogPublic == null)
                throw new ArgumentNullException(nameof(isWarLogPublic), "Property is required for class ClanListEntry.");

            if (clanLevel == null)
                throw new ArgumentNullException(nameof(clanLevel), "Property is required for class ClanListEntry.");

            if (warWinStreak == null)
                throw new ArgumentNullException(nameof(warWinStreak), "Property is required for class ClanListEntry.");

            if (warWins == null)
                throw new ArgumentNullException(nameof(warWins), "Property is required for class ClanListEntry.");

            if (clanPoints == null)
                throw new ArgumentNullException(nameof(clanPoints), "Property is required for class ClanListEntry.");

            if (labels == null)
                throw new ArgumentNullException(nameof(labels), "Property is required for class ClanListEntry.");

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class ClanListEntry.");

            if (members == null)
                throw new ArgumentNullException(nameof(members), "Property is required for class ClanListEntry.");

            if (badgeUrls == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is required for class ClanListEntry.");

            return new ClanListEntry(badgeUrls, clanBuilderBasePoints.Value, clanLevel.Value, clanPoints.Value, isWarLogPublic.Value, labels, members.Value, name, requiredTrophies.Value, tag, warLeague, warWinStreak.Value, warWins.Value, chatLanguage, location, type, warFrequency, warLosses, warTies);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanListEntry"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanListEntry clanListEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, clanListEntry.BadgeUrls, jsonSerializerOptions);
            writer.WriteNumber("clanBuilderBasePoints", clanListEntry.ClanBuilderBasePoints);
            writer.WriteNumber("clanLevel", clanListEntry.ClanLevel);
            writer.WriteNumber("clanPoints", clanListEntry.ClanPoints);
            writer.WriteBoolean("isWarLogPublic", clanListEntry.IsWarLogPublic);
            writer.WritePropertyName("labels");
            JsonSerializer.Serialize(writer, clanListEntry.Labels, jsonSerializerOptions);
            writer.WriteNumber("members", clanListEntry.Members);
            writer.WriteString("name", clanListEntry.Name);
            writer.WriteNumber("requiredTrophies", clanListEntry.RequiredTrophies);
            writer.WriteString("tag", clanListEntry.Tag);
            writer.WritePropertyName("warLeague");
            JsonSerializer.Serialize(writer, clanListEntry.WarLeague, jsonSerializerOptions);
            writer.WriteNumber("warWinStreak", clanListEntry.WarWinStreak);
            writer.WriteNumber("warWins", clanListEntry.WarWins);
            writer.WritePropertyName("chatLanguage");
            JsonSerializer.Serialize(writer, clanListEntry.ChatLanguage, jsonSerializerOptions);
            writer.WritePropertyName("location");
            JsonSerializer.Serialize(writer, clanListEntry.Location, jsonSerializerOptions);
            if (clanListEntry.Type == null)
                writer.WriteNull("type");
            var typeRawValue = RecruitingTypeConverter.ToJsonValue(clanListEntry.Type.Value);
            if (typeRawValue != null)
                writer.WriteString("type", typeRawValue);
            else
                writer.WriteNull("type");
            if (clanListEntry.WarFrequency == null)
                writer.WriteNull("warFrequency");
            var warFrequencyRawValue = WarFrequencyConverter.ToJsonValue(clanListEntry.WarFrequency.Value);
            if (warFrequencyRawValue != null)
                writer.WriteString("warFrequency", warFrequencyRawValue);
            else
                writer.WriteNull("warFrequency");
            if (clanListEntry.WarLosses != null)
                writer.WriteNumber("warLosses", clanListEntry.WarLosses.Value);
            else
                writer.WriteNull("warLosses");
            if (clanListEntry.WarTies != null)
                writer.WriteNumber("warTies", clanListEntry.WarTies.Value);
            else
                writer.WriteNull("warTies");

            writer.WriteEndObject();
        }
    }
}
