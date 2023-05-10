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
    /// Clan
    /// </summary>
    public partial class Clan : IEquatable<Clan?>
    {
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
        /// Gets or Sets CapitalLeague
        /// </summary>
        [JsonPropertyName("capitalLeague")]
        public CapitalLeague CapitalLeague { get; }

        /// <summary>
        /// Gets or Sets ClanCapital
        /// </summary>
        [JsonPropertyName("clanCapital")]
        public ClanCapital ClanCapital { get; }

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
        /// Gets or Sets ClanVersusPoints
        /// </summary>
        [JsonPropertyName("clanVersusPoints")]
        public int ClanVersusPoints { get; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; }

        /// <summary>
        /// Gets or Sets IsFamilyFriendly
        /// </summary>
        [JsonPropertyName("isFamilyFriendly")]
        public bool IsFamilyFriendly { get; }

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
            sb.Append("class Clan {\n");
            sb.Append("  BadgeUrls: ").Append(BadgeUrls).Append("\n");
            sb.Append("  CapitalLeague: ").Append(CapitalLeague).Append("\n");
            sb.Append("  ClanCapital: ").Append(ClanCapital).Append("\n");
            sb.Append("  ClanLevel: ").Append(ClanLevel).Append("\n");
            sb.Append("  ClanPoints: ").Append(ClanPoints).Append("\n");
            sb.Append("  ClanVersusPoints: ").Append(ClanVersusPoints).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  IsFamilyFriendly: ").Append(IsFamilyFriendly).Append("\n");
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
            return this.Equals(input as Clan);
        }

        /// <summary>
        /// Returns true if Clan instances are equal
        /// </summary>
        /// <param name="input">Instance of Clan to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Clan? input)
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
                    CapitalLeague == input.CapitalLeague ||
                    (CapitalLeague != null &&
                    CapitalLeague.Equals(input.CapitalLeague))
                ) && 
                (
                    ClanCapital == input.ClanCapital ||
                    (ClanCapital != null &&
                    ClanCapital.Equals(input.ClanCapital))
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
                    ClanVersusPoints == input.ClanVersusPoints ||
                    (ClanVersusPoints != null &&
                    ClanVersusPoints.Equals(input.ClanVersusPoints))
                ) && 
                (
                    Description == input.Description ||
                    (Description != null &&
                    Description.Equals(input.Description))
                ) && 
                (
                    IsFamilyFriendly == input.IsFamilyFriendly ||
                    (IsFamilyFriendly != null &&
                    IsFamilyFriendly.Equals(input.IsFamilyFriendly))
                ) && 
                (
                    IsWarLogPublic == input.IsWarLogPublic ||
                    (IsWarLogPublic != null &&
                    IsWarLogPublic.Equals(input.IsWarLogPublic))
                ) && 
                (
                    Labels == input.Labels ||
                    Labels != null &&
                    input.Labels != null &&
                    Labels.SequenceEqual(input.Labels)
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
                    RequiredTrophies == input.RequiredTrophies ||
                    (RequiredTrophies != null &&
                    RequiredTrophies.Equals(input.RequiredTrophies))
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
                    (WarWinStreak != null &&
                    WarWinStreak.Equals(input.WarWinStreak))
                ) && 
                (
                    WarWins == input.WarWins ||
                    (WarWins != null &&
                    WarWins.Equals(input.WarWins))
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
                    (Type != null &&
                    Type.Equals(input.Type))
                ) && 
                (
                    WarFrequency == input.WarFrequency ||
                    (WarFrequency != null &&
                    WarFrequency.Equals(input.WarFrequency))
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
                hashCode = (hashCode * 59) + CapitalLeague.GetHashCode();
                hashCode = (hashCode * 59) + ClanCapital.GetHashCode();
                hashCode = (hashCode * 59) + ClanLevel.GetHashCode();
                hashCode = (hashCode * 59) + ClanPoints.GetHashCode();
                hashCode = (hashCode * 59) + ClanVersusPoints.GetHashCode();
                hashCode = (hashCode * 59) + Description.GetHashCode();
                hashCode = (hashCode * 59) + IsFamilyFriendly.GetHashCode();
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
    /// A Json converter for type Clan
    /// </summary>
    public class ClanJsonConverter : JsonConverter<Clan>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override Clan Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            BadgeUrls badgeUrls = default;
            CapitalLeague capitalLeague = default;
            ClanCapital clanCapital = default;
            int clanLevel = default;
            int clanPoints = default;
            int clanVersusPoints = default;
            string description = default;
            bool isFamilyFriendly = default;
            bool isWarLogPublic = default;
            List<Label> labels = default;
            List<ClanMember> memberList = default;
            string name = default;
            int requiredTrophies = default;
            string tag = default;
            WarLeague warLeague = default;
            int warWinStreak = default;
            int warWins = default;
            Language chatLanguage = default;
            Location location = default;
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
                        case "capitalLeague":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                capitalLeague = JsonSerializer.Deserialize<CapitalLeague>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "clanCapital":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanCapital = JsonSerializer.Deserialize<ClanCapital>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "clanLevel":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanLevel = utf8JsonReader.GetInt32();
                            break;
                        case "clanPoints":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanPoints = utf8JsonReader.GetInt32();
                            break;
                        case "clanVersusPoints":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanVersusPoints = utf8JsonReader.GetInt32();
                            break;
                        case "description":
                            description = utf8JsonReader.GetString();
                            break;
                        case "isFamilyFriendly":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isFamilyFriendly = utf8JsonReader.GetBoolean();
                            break;
                        case "isWarLogPublic":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                isWarLogPublic = utf8JsonReader.GetBoolean();
                            break;
                        case "labels":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                labels = JsonSerializer.Deserialize<List<Label>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "memberList":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                memberList = JsonSerializer.Deserialize<List<ClanMember>>(ref utf8JsonReader, jsonSerializerOptions);
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

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (warLeague == null)
                throw new ArgumentNullException(nameof(warLeague), "Property is required for class Clan.");

            if (memberList == null)
                throw new ArgumentNullException(nameof(memberList), "Property is required for class Clan.");

            if (requiredTrophies == null)
                throw new ArgumentNullException(nameof(requiredTrophies), "Property is required for class Clan.");

            if (clanVersusPoints == null)
                throw new ArgumentNullException(nameof(clanVersusPoints), "Property is required for class Clan.");

            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Property is required for class Clan.");

            if (isWarLogPublic == null)
                throw new ArgumentNullException(nameof(isWarLogPublic), "Property is required for class Clan.");

            if (clanLevel == null)
                throw new ArgumentNullException(nameof(clanLevel), "Property is required for class Clan.");

            if (warWinStreak == null)
                throw new ArgumentNullException(nameof(warWinStreak), "Property is required for class Clan.");

            if (warWins == null)
                throw new ArgumentNullException(nameof(warWins), "Property is required for class Clan.");

            if (clanPoints == null)
                throw new ArgumentNullException(nameof(clanPoints), "Property is required for class Clan.");

            if (labels == null)
                throw new ArgumentNullException(nameof(labels), "Property is required for class Clan.");

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class Clan.");

            if (memberList == null)
                throw new ArgumentNullException(nameof(memberList), "Property is required for class Clan.");

            if (description == null)
                throw new ArgumentNullException(nameof(description), "Property is required for class Clan.");

            if (clanCapital == null)
                throw new ArgumentNullException(nameof(clanCapital), "Property is required for class Clan.");

            if (badgeUrls == null)
                throw new ArgumentNullException(nameof(badgeUrls), "Property is required for class Clan.");

            if (capitalLeague == null)
                throw new ArgumentNullException(nameof(capitalLeague), "Property is required for class Clan.");

            if (isFamilyFriendly == null)
                throw new ArgumentNullException(nameof(isFamilyFriendly), "Property is required for class Clan.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return new Clan(badgeUrls, capitalLeague, clanCapital, clanLevel, clanPoints, clanVersusPoints, description, isFamilyFriendly, isWarLogPublic, labels, memberList, name, requiredTrophies, tag, warLeague, warLosses, warTies, warWinStreak, warWins, chatLanguage, location, type, warFrequency);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clan"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, Clan clan, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("badgeUrls");
            JsonSerializer.Serialize(writer, clan.BadgeUrls, jsonSerializerOptions);
            writer.WritePropertyName("capitalLeague");
            JsonSerializer.Serialize(writer, clan.CapitalLeague, jsonSerializerOptions);
            writer.WritePropertyName("clanCapital");
            JsonSerializer.Serialize(writer, clan.ClanCapital, jsonSerializerOptions);
            writer.WriteNumber("clanLevel", clan.ClanLevel);
            writer.WriteNumber("clanPoints", clan.ClanPoints);
            writer.WriteNumber("clanVersusPoints", clan.ClanVersusPoints);
            writer.WriteString("description", clan.Description);
            writer.WriteBoolean("isFamilyFriendly", clan.IsFamilyFriendly);
            writer.WriteBoolean("isWarLogPublic", clan.IsWarLogPublic);
            writer.WritePropertyName("labels");
            JsonSerializer.Serialize(writer, clan.Labels, jsonSerializerOptions);
            writer.WritePropertyName("memberList");
            JsonSerializer.Serialize(writer, clan.Members, jsonSerializerOptions);
            writer.WriteString("name", clan.Name);
            writer.WriteNumber("requiredTrophies", clan.RequiredTrophies);
            writer.WriteString("tag", clan.Tag);
            writer.WritePropertyName("warLeague");
            JsonSerializer.Serialize(writer, clan.WarLeague, jsonSerializerOptions);
            writer.WriteNumber("warWinStreak", clan.WarWinStreak);
            writer.WriteNumber("warWins", clan.WarWins);
            writer.WritePropertyName("chatLanguage");
            JsonSerializer.Serialize(writer, clan.ChatLanguage, jsonSerializerOptions);
            writer.WritePropertyName("location");
            JsonSerializer.Serialize(writer, clan.Location, jsonSerializerOptions);
            if (clan.Type == null)
                writer.WriteNull("type");
            var typeRawValue = RecruitingTypeConverter.ToJsonValue(clan.Type.Value);
            if (typeRawValue != null)
                writer.WriteString("type", typeRawValue);
            else
                writer.WriteNull("type");
            if (clan.WarFrequency == null)
                writer.WriteNull("warFrequency");
            var warFrequencyRawValue = WarFrequencyConverter.ToJsonValue(clan.WarFrequency.Value);
            if (warFrequencyRawValue != null)
                writer.WriteString("warFrequency", warFrequencyRawValue);
            else
                writer.WriteNull("warFrequency");
            if (clan.WarLosses != null)
                writer.WriteNumber("warLosses", clan.WarLosses.Value);
            else
                writer.WriteNull("warLosses");
            if (clan.WarTies != null)
                writer.WriteNumber("warTies", clan.WarTies.Value);
            else
                writer.WriteNull("warTies");

            writer.WriteEndObject();
        }
    }
}
