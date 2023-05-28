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
    /// ClanMember
    /// </summary>
    public partial class ClanMember : IEquatable<ClanMember?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanMember" /> class.
        /// </summary>
        /// <param name="clanRank">clanRank</param>
        /// <param name="donations">donations</param>
        /// <param name="donationsReceived">donationsReceived</param>
        /// <param name="expLevel">expLevel</param>
        /// <param name="league">league</param>
        /// <param name="name">name</param>
        /// <param name="previousClanRank">previousClanRank</param>
        /// <param name="tag">tag</param>
        /// <param name="trophies">trophies</param>
        /// <param name="versusTrophies">versusTrophies</param>
        /// <param name="builderBaseLeague">builderBaseLeague</param>
        /// <param name="builderBaseTrophies">builderBaseTrophies</param>
        /// <param name="playerHouse">playerHouse</param>
        /// <param name="role">role</param>
        [JsonConstructor]
        internal ClanMember(int clanRank, int donations, int donationsReceived, int expLevel, League league, string name, int previousClanRank, string tag, int trophies, int versusTrophies, BuilderBaseLeague? builderBaseLeague = default, int? builderBaseTrophies = default, PlayerHouse? playerHouse = default, Role? role = default)
        {
            ClanRank = clanRank;
            Donations = donations;
            DonationsReceived = donationsReceived;
            ExpLevel = expLevel;
            League = league;
            Name = name;
            PreviousClanRank = previousClanRank;
            Tag = tag;
            Trophies = trophies;
            VersusTrophies = versusTrophies;
            BuilderBaseLeague = builderBaseLeague;
            BuilderBaseTrophies = builderBaseTrophies;
            PlayerHouse = playerHouse;
            Role = role;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets Role
        /// </summary>
        [JsonPropertyName("role")]
        public Role? Role { get; }

        /// <summary>
        /// Gets or Sets ClanRank
        /// </summary>
        [JsonPropertyName("clanRank")]
        public int ClanRank { get; }

        /// <summary>
        /// Gets or Sets Donations
        /// </summary>
        [JsonPropertyName("donations")]
        public int Donations { get; }

        /// <summary>
        /// Gets or Sets DonationsReceived
        /// </summary>
        [JsonPropertyName("donationsReceived")]
        public int DonationsReceived { get; }

        /// <summary>
        /// Gets or Sets ExpLevel
        /// </summary>
        [JsonPropertyName("expLevel")]
        public int ExpLevel { get; }

        /// <summary>
        /// Gets or Sets League
        /// </summary>
        [JsonPropertyName("league")]
        public League League { get; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or Sets PreviousClanRank
        /// </summary>
        [JsonPropertyName("previousClanRank")]
        public int PreviousClanRank { get; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; }

        /// <summary>
        /// Gets or Sets Trophies
        /// </summary>
        [JsonPropertyName("trophies")]
        public int Trophies { get; }

        /// <summary>
        /// Gets or Sets VersusTrophies
        /// </summary>
        [JsonPropertyName("versusTrophies")]
        public int VersusTrophies { get; }

        /// <summary>
        /// Gets or Sets BuilderBaseLeague
        /// </summary>
        [JsonPropertyName("builderBaseLeague")]
        public BuilderBaseLeague? BuilderBaseLeague { get; }

        /// <summary>
        /// Gets or Sets BuilderBaseTrophies
        /// </summary>
        [JsonPropertyName("builderBaseTrophies")]
        public int? BuilderBaseTrophies { get; }

        /// <summary>
        /// Gets or Sets PlayerHouse
        /// </summary>
        [JsonPropertyName("playerHouse")]
        public PlayerHouse? PlayerHouse { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanMember {\n");
            sb.Append("  ClanRank: ").Append(ClanRank).Append("\n");
            sb.Append("  Donations: ").Append(Donations).Append("\n");
            sb.Append("  DonationsReceived: ").Append(DonationsReceived).Append("\n");
            sb.Append("  ExpLevel: ").Append(ExpLevel).Append("\n");
            sb.Append("  League: ").Append(League).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  PreviousClanRank: ").Append(PreviousClanRank).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  Trophies: ").Append(Trophies).Append("\n");
            sb.Append("  VersusTrophies: ").Append(VersusTrophies).Append("\n");
            sb.Append("  BuilderBaseLeague: ").Append(BuilderBaseLeague).Append("\n");
            sb.Append("  BuilderBaseTrophies: ").Append(BuilderBaseTrophies).Append("\n");
            sb.Append("  PlayerHouse: ").Append(PlayerHouse).Append("\n");
            sb.Append("  Role: ").Append(Role).Append("\n");
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
            return this.Equals(input as ClanMember);
        }

        /// <summary>
        /// Returns true if ClanMember instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanMember to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanMember? input)
        {
            if (input == null)
                return false;

            return 
                (
                    ClanRank == input.ClanRank ||
                    (ClanRank != null &&
                    ClanRank.Equals(input.ClanRank))
                ) && 
                (
                    Donations == input.Donations ||
                    (Donations != null &&
                    Donations.Equals(input.Donations))
                ) && 
                (
                    DonationsReceived == input.DonationsReceived ||
                    (DonationsReceived != null &&
                    DonationsReceived.Equals(input.DonationsReceived))
                ) && 
                (
                    ExpLevel == input.ExpLevel ||
                    (ExpLevel != null &&
                    ExpLevel.Equals(input.ExpLevel))
                ) && 
                (
                    League == input.League ||
                    (League != null &&
                    League.Equals(input.League))
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    PreviousClanRank == input.PreviousClanRank ||
                    (PreviousClanRank != null &&
                    PreviousClanRank.Equals(input.PreviousClanRank))
                ) && 
                (
                    Tag == input.Tag ||
                    (Tag != null &&
                    Tag.Equals(input.Tag))
                ) && 
                (
                    Trophies == input.Trophies ||
                    (Trophies != null &&
                    Trophies.Equals(input.Trophies))
                ) && 
                (
                    VersusTrophies == input.VersusTrophies ||
                    (VersusTrophies != null &&
                    VersusTrophies.Equals(input.VersusTrophies))
                ) && 
                (
                    BuilderBaseLeague == input.BuilderBaseLeague ||
                    (BuilderBaseLeague != null &&
                    BuilderBaseLeague.Equals(input.BuilderBaseLeague))
                ) && 
                (
                    BuilderBaseTrophies == input.BuilderBaseTrophies ||
                    (BuilderBaseTrophies != null &&
                    BuilderBaseTrophies.Equals(input.BuilderBaseTrophies))
                ) && 
                (
                    PlayerHouse == input.PlayerHouse ||
                    (PlayerHouse != null &&
                    PlayerHouse.Equals(input.PlayerHouse))
                ) && 
                (
                    Role == input.Role ||
                    (Role != null &&
                    Role.Equals(input.Role))
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
                hashCode = (hashCode * 59) + ClanRank.GetHashCode();
                hashCode = (hashCode * 59) + Donations.GetHashCode();
                hashCode = (hashCode * 59) + DonationsReceived.GetHashCode();
                hashCode = (hashCode * 59) + ExpLevel.GetHashCode();
                hashCode = (hashCode * 59) + League.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + PreviousClanRank.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();
                hashCode = (hashCode * 59) + Trophies.GetHashCode();
                hashCode = (hashCode * 59) + VersusTrophies.GetHashCode();

                if (BuilderBaseLeague != null)
                    hashCode = (hashCode * 59) + BuilderBaseLeague.GetHashCode();

                if (BuilderBaseTrophies != null)
                    hashCode = (hashCode * 59) + BuilderBaseTrophies.GetHashCode();

                if (PlayerHouse != null)
                    hashCode = (hashCode * 59) + PlayerHouse.GetHashCode();

                if (Role != null)
                    hashCode = (hashCode * 59) + Role.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanMember
    /// </summary>
    public class ClanMemberJsonConverter : JsonConverter<ClanMember>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanMember Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int? clanRank = default;
            int? donations = default;
            int? donationsReceived = default;
            int? expLevel = default;
            League? league = default;
            string? name = default;
            int? previousClanRank = default;
            string? tag = default;
            int? trophies = default;
            int? versusTrophies = default;
            BuilderBaseLeague? builderBaseLeague = default;
            int? builderBaseTrophies = default;
            PlayerHouse? playerHouse = default;
            Role? role = default;

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
                        case "clanRank":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clanRank = utf8JsonReader.GetInt32();
                            break;
                        case "donations":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                donations = utf8JsonReader.GetInt32();
                            break;
                        case "donationsReceived":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                donationsReceived = utf8JsonReader.GetInt32();
                            break;
                        case "expLevel":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                expLevel = utf8JsonReader.GetInt32();
                            break;
                        case "league":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                league = JsonSerializer.Deserialize<League>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        case "previousClanRank":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                previousClanRank = utf8JsonReader.GetInt32();
                            break;
                        case "tag":
                            tag = utf8JsonReader.GetString();
                            break;
                        case "trophies":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                trophies = utf8JsonReader.GetInt32();
                            break;
                        case "versusTrophies":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                versusTrophies = utf8JsonReader.GetInt32();
                            break;
                        case "builderBaseLeague":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                builderBaseLeague = JsonSerializer.Deserialize<BuilderBaseLeague>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "builderBaseTrophies":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                builderBaseTrophies = utf8JsonReader.GetInt32();
                            break;
                        case "playerHouse":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                playerHouse = JsonSerializer.Deserialize<PlayerHouse>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "role":
                            string roleRawValue = utf8JsonReader.GetString();
                            role = RoleConverter.FromStringOrDefault(roleRawValue);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (league == null)
                throw new ArgumentNullException(nameof(league), "Property is required for class ClanMember.");

            if (versusTrophies == null)
                throw new ArgumentNullException(nameof(versusTrophies), "Property is required for class ClanMember.");

            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Property is required for class ClanMember.");

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class ClanMember.");

            if (expLevel == null)
                throw new ArgumentNullException(nameof(expLevel), "Property is required for class ClanMember.");

            if (clanRank == null)
                throw new ArgumentNullException(nameof(clanRank), "Property is required for class ClanMember.");

            if (previousClanRank == null)
                throw new ArgumentNullException(nameof(previousClanRank), "Property is required for class ClanMember.");

            if (donations == null)
                throw new ArgumentNullException(nameof(donations), "Property is required for class ClanMember.");

            if (donationsReceived == null)
                throw new ArgumentNullException(nameof(donationsReceived), "Property is required for class ClanMember.");

            if (trophies == null)
                throw new ArgumentNullException(nameof(trophies), "Property is required for class ClanMember.");

            return new ClanMember(clanRank.Value, donations.Value, donationsReceived.Value, expLevel.Value, league, name, previousClanRank.Value, tag, trophies.Value, versusTrophies.Value, builderBaseLeague, builderBaseTrophies, playerHouse, role.Value);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanMember"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanMember clanMember, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("clanRank", clanMember.ClanRank);
            writer.WriteNumber("donations", clanMember.Donations);
            writer.WriteNumber("donationsReceived", clanMember.DonationsReceived);
            writer.WriteNumber("expLevel", clanMember.ExpLevel);
            writer.WritePropertyName("league");
            JsonSerializer.Serialize(writer, clanMember.League, jsonSerializerOptions);
            writer.WriteString("name", clanMember.Name);
            writer.WriteNumber("previousClanRank", clanMember.PreviousClanRank);
            writer.WriteString("tag", clanMember.Tag);
            writer.WriteNumber("trophies", clanMember.Trophies);
            writer.WriteNumber("versusTrophies", clanMember.VersusTrophies);
            writer.WritePropertyName("builderBaseLeague");
            JsonSerializer.Serialize(writer, clanMember.BuilderBaseLeague, jsonSerializerOptions);
            if (clanMember.BuilderBaseTrophies != null)
                writer.WriteNumber("builderBaseTrophies", clanMember.BuilderBaseTrophies.Value);
            else
                writer.WriteNull("builderBaseTrophies");
            writer.WritePropertyName("playerHouse");
            JsonSerializer.Serialize(writer, clanMember.PlayerHouse, jsonSerializerOptions);
            if (clanMember.Role == null)
                writer.WriteNull("role");
            var roleRawValue = RoleConverter.ToJsonValue(clanMember.Role.Value);
            if (roleRawValue != null)
                writer.WriteString("role", roleRawValue);
            else
                writer.WriteNull("role");

            writer.WriteEndObject();
        }
    }
}
