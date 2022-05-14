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
        /// <param name="role">role</param>
        [JsonConstructor]
        internal ClanMember(int clanRank, int donations, int donationsReceived, int expLevel, League league, string name, int previousClanRank, string tag, int trophies, int versusTrophies, Role? role = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (league == null)
                throw new ArgumentNullException("league is a required property for ClanMember and cannot be null.");

            if (tag == null)
                throw new ArgumentNullException("tag is a required property for ClanMember and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for ClanMember and cannot be null.");

            if (expLevel == null)
                throw new ArgumentNullException("expLevel is a required property for ClanMember and cannot be null.");

            if (clanRank == null)
                throw new ArgumentNullException("clanRank is a required property for ClanMember and cannot be null.");

            if (previousClanRank == null)
                throw new ArgumentNullException("previousClanRank is a required property for ClanMember and cannot be null.");

            if (donations == null)
                throw new ArgumentNullException("donations is a required property for ClanMember and cannot be null.");

            if (donationsReceived == null)
                throw new ArgumentNullException("donationsReceived is a required property for ClanMember and cannot be null.");

            if (trophies == null)
                throw new ArgumentNullException("trophies is a required property for ClanMember and cannot be null.");

            if (versusTrophies == null)
                throw new ArgumentNullException("versusTrophies is a required property for ClanMember and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

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
            Role = role;
        }

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
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanMember Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int currentDepth = reader.CurrentDepth;

            if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = reader.TokenType;

            int clanRank = default;
            int donations = default;
            int donationsReceived = default;
            int expLevel = default;
            League league = default;
            string name = default;
            int previousClanRank = default;
            string tag = default;
            int trophies = default;
            int versusTrophies = default;
            Role? role = default;

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
                        case "clanRank":
                            clanRank = reader.GetInt32();
                            break;
                        case "donations":
                            donations = reader.GetInt32();
                            break;
                        case "donationsReceived":
                            donationsReceived = reader.GetInt32();
                            break;
                        case "expLevel":
                            expLevel = reader.GetInt32();
                            break;
                        case "league":
                            Utf8JsonReader leagueReader = reader;
                            league = JsonSerializer.Deserialize<League>(ref reader, options);
                            break;
                        case "name":
                            name = reader.GetString();
                            break;
                        case "previousClanRank":
                            previousClanRank = reader.GetInt32();
                            break;
                        case "tag":
                            tag = reader.GetString();
                            break;
                        case "trophies":
                            trophies = reader.GetInt32();
                            break;
                        case "versusTrophies":
                            versusTrophies = reader.GetInt32();
                            break;
                        case "role":
                            string roleRawValue = reader.GetString();
                            role = RoleConverter.FromString(roleRawValue);
                            break;
                    }
                }
            }

            return new ClanMember(clanRank, donations, donationsReceived, expLevel, league, name, previousClanRank, tag, trophies, versusTrophies, role);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanMember"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanMember clanMember, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("clanRank", (int)clanMember.ClanRank);
            writer.WriteNumber("donations", (int)clanMember.Donations);
            writer.WriteNumber("donationsReceived", (int)clanMember.DonationsReceived);
            writer.WriteNumber("expLevel", (int)clanMember.ExpLevel);
            writer.WritePropertyName("league");
            JsonSerializer.Serialize(writer, clanMember.League, options);
            writer.WriteString("name", clanMember.Name);
            writer.WriteNumber("previousClanRank", (int)clanMember.PreviousClanRank);
            writer.WriteString("tag", clanMember.Tag);
            writer.WriteNumber("trophies", (int)clanMember.Trophies);
            writer.WriteNumber("versusTrophies", (int)clanMember.VersusTrophies);
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
