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
    /// ClanCapitalRaidSeasonAttackLogEntry
    /// </summary>
    public partial class ClanCapitalRaidSeasonAttackLogEntry : IEquatable<ClanCapitalRaidSeasonAttackLogEntry?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanCapitalRaidSeasonAttackLogEntry" /> class.
        /// </summary>
        /// <param name="attackCount">attackCount</param>
        /// <param name="defender">defender</param>
        /// <param name="districtCount">districtCount</param>
        /// <param name="districts">districts</param>
        /// <param name="districtsDestroyed">districtsDestroyed</param>
        [JsonConstructor]
        internal ClanCapitalRaidSeasonAttackLogEntry(int attackCount, ClanCapitalRaidSeasonClanInfo defender, int districtCount, List<ClanCapitalRaidSeasonDistrict> districts, int districtsDestroyed)
        {
            AttackCount = attackCount;
            Defender = defender;
            DistrictCount = districtCount;
            Districts = districts;
            DistrictsDestroyed = districtsDestroyed;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets AttackCount
        /// </summary>
        [JsonPropertyName("attackCount")]
        public int AttackCount { get; }

        /// <summary>
        /// Gets or Sets Defender
        /// </summary>
        [JsonPropertyName("defender")]
        public ClanCapitalRaidSeasonClanInfo Defender { get; }

        /// <summary>
        /// Gets or Sets DistrictCount
        /// </summary>
        [JsonPropertyName("districtCount")]
        public int DistrictCount { get; }

        /// <summary>
        /// Gets or Sets Districts
        /// </summary>
        [JsonPropertyName("districts")]
        public List<ClanCapitalRaidSeasonDistrict> Districts { get; }

        /// <summary>
        /// Gets or Sets DistrictsDestroyed
        /// </summary>
        [JsonPropertyName("districtsDestroyed")]
        public int DistrictsDestroyed { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanCapitalRaidSeasonAttackLogEntry {\n");
            sb.Append("  AttackCount: ").Append(AttackCount).Append("\n");
            sb.Append("  Defender: ").Append(Defender).Append("\n");
            sb.Append("  DistrictCount: ").Append(DistrictCount).Append("\n");
            sb.Append("  Districts: ").Append(Districts).Append("\n");
            sb.Append("  DistrictsDestroyed: ").Append(DistrictsDestroyed).Append("\n");
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
            return this.Equals(input as ClanCapitalRaidSeasonAttackLogEntry);
        }

        /// <summary>
        /// Returns true if ClanCapitalRaidSeasonAttackLogEntry instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanCapitalRaidSeasonAttackLogEntry to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanCapitalRaidSeasonAttackLogEntry? input)
        {
            if (input == null)
                return false;

            return 
                (
                    AttackCount == input.AttackCount ||
                    (AttackCount != null &&
                    AttackCount.Equals(input.AttackCount))
                ) && 
                (
                    Defender == input.Defender ||
                    (Defender != null &&
                    Defender.Equals(input.Defender))
                ) && 
                (
                    DistrictCount == input.DistrictCount ||
                    (DistrictCount != null &&
                    DistrictCount.Equals(input.DistrictCount))
                ) && 
                (
                    Districts == input.Districts ||
                    Districts != null &&
                    input.Districts != null &&
                    Districts.SequenceEqual(input.Districts)
                ) && 
                (
                    DistrictsDestroyed == input.DistrictsDestroyed ||
                    (DistrictsDestroyed != null &&
                    DistrictsDestroyed.Equals(input.DistrictsDestroyed))
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
                hashCode = (hashCode * 59) + AttackCount.GetHashCode();
                hashCode = (hashCode * 59) + Defender.GetHashCode();
                hashCode = (hashCode * 59) + DistrictCount.GetHashCode();
                hashCode = (hashCode * 59) + Districts.GetHashCode();
                hashCode = (hashCode * 59) + DistrictsDestroyed.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanCapitalRaidSeasonAttackLogEntry
    /// </summary>
    public class ClanCapitalRaidSeasonAttackLogEntryJsonConverter : JsonConverter<ClanCapitalRaidSeasonAttackLogEntry>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanCapitalRaidSeasonAttackLogEntry Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int? attackCount = default;
            ClanCapitalRaidSeasonClanInfo? defender = default;
            int? districtCount = default;
            List<ClanCapitalRaidSeasonDistrict>? districts = default;
            int? districtsDestroyed = default;

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
                        case "attackCount":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                attackCount = utf8JsonReader.GetInt32();
                            break;
                        case "defender":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                defender = JsonSerializer.Deserialize<ClanCapitalRaidSeasonClanInfo>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "districtCount":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                districtCount = utf8JsonReader.GetInt32();
                            break;
                        case "districts":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                districts = JsonSerializer.Deserialize<List<ClanCapitalRaidSeasonDistrict>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "districtsDestroyed":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                districtsDestroyed = utf8JsonReader.GetInt32();
                            break;
                        default:
                            break;
                    }
                }
            }

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (defender == null)
                throw new ArgumentNullException(nameof(defender), "Property is required for class ClanCapitalRaidSeasonAttackLogEntry.");

            if (attackCount == null)
                throw new ArgumentNullException(nameof(attackCount), "Property is required for class ClanCapitalRaidSeasonAttackLogEntry.");

            if (districtCount == null)
                throw new ArgumentNullException(nameof(districtCount), "Property is required for class ClanCapitalRaidSeasonAttackLogEntry.");

            if (districtsDestroyed == null)
                throw new ArgumentNullException(nameof(districtsDestroyed), "Property is required for class ClanCapitalRaidSeasonAttackLogEntry.");

            if (districts == null)
                throw new ArgumentNullException(nameof(districts), "Property is required for class ClanCapitalRaidSeasonAttackLogEntry.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return new ClanCapitalRaidSeasonAttackLogEntry(attackCount.Value, defender, districtCount.Value, districts, districtsDestroyed.Value);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeasonAttackLogEntry"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanCapitalRaidSeasonAttackLogEntry clanCapitalRaidSeasonAttackLogEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("attackCount", clanCapitalRaidSeasonAttackLogEntry.AttackCount);
            writer.WritePropertyName("defender");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeasonAttackLogEntry.Defender, jsonSerializerOptions);
            writer.WriteNumber("districtCount", clanCapitalRaidSeasonAttackLogEntry.DistrictCount);
            writer.WritePropertyName("districts");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeasonAttackLogEntry.Districts, jsonSerializerOptions);
            writer.WriteNumber("districtsDestroyed", clanCapitalRaidSeasonAttackLogEntry.DistrictsDestroyed);

            writer.WriteEndObject();
        }
    }
}
