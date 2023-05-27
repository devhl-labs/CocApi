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
    /// ClanWarLogEntry
    /// </summary>
    public partial class ClanWarLogEntry : IEquatable<ClanWarLogEntry?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarLogEntry" /> class.
        /// </summary>
        /// <param name="attacksPerMember">attacksPerMember</param>
        /// <param name="clan">clan</param>
        /// <param name="endTime">endTime</param>
        /// <param name="opponent">opponent</param>
        /// <param name="teamSize">teamSize</param>
        /// <param name="result">result</param>
        [JsonConstructor]
        internal ClanWarLogEntry(int attacksPerMember, WarClanLogEntry clan, DateTime endTime, WarClanLogEntry opponent, int teamSize, Result? result = default)
        {
            AttacksPerMember = attacksPerMember;
            Clan = clan;
            EndTime = endTime;
            Opponent = opponent;
            TeamSize = teamSize;
            Result = result;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets Result
        /// </summary>
        [JsonPropertyName("result")]
        public Result? Result { get; }

        /// <summary>
        /// Gets or Sets AttacksPerMember
        /// </summary>
        [JsonPropertyName("attacksPerMember")]
        public int AttacksPerMember { get; }

        /// <summary>
        /// Gets or Sets Clan
        /// </summary>
        [JsonPropertyName("clan")]
        public WarClanLogEntry Clan { get; }

        /// <summary>
        /// Gets or Sets EndTime
        /// </summary>
        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; }

        /// <summary>
        /// Gets or Sets Opponent
        /// </summary>
        [JsonPropertyName("opponent")]
        public WarClanLogEntry Opponent { get; }

        /// <summary>
        /// Gets or Sets TeamSize
        /// </summary>
        [JsonPropertyName("teamSize")]
        public int TeamSize { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanWarLogEntry {\n");
            sb.Append("  AttacksPerMember: ").Append(AttacksPerMember).Append("\n");
            sb.Append("  Clan: ").Append(Clan).Append("\n");
            sb.Append("  EndTime: ").Append(EndTime).Append("\n");
            sb.Append("  Opponent: ").Append(Opponent).Append("\n");
            sb.Append("  TeamSize: ").Append(TeamSize).Append("\n");
            sb.Append("  Result: ").Append(Result).Append("\n");
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
            return this.Equals(input as ClanWarLogEntry);
        }

        /// <summary>
        /// Returns true if ClanWarLogEntry instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanWarLogEntry to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanWarLogEntry? input)
        {
            if (input == null)
                return false;

            return 
                (
                    AttacksPerMember == input.AttacksPerMember ||
                    (AttacksPerMember != null &&
                    AttacksPerMember.Equals(input.AttacksPerMember))
                ) && 
                (
                    Clan == input.Clan ||
                    (Clan != null &&
                    Clan.Equals(input.Clan))
                ) && 
                (
                    EndTime == input.EndTime ||
                    (EndTime != null &&
                    EndTime.Equals(input.EndTime))
                ) && 
                (
                    Opponent == input.Opponent ||
                    (Opponent != null &&
                    Opponent.Equals(input.Opponent))
                ) && 
                (
                    TeamSize == input.TeamSize ||
                    (TeamSize != null &&
                    TeamSize.Equals(input.TeamSize))
                ) && 
                (
                    Result == input.Result ||
                    (Result != null &&
                    Result.Equals(input.Result))
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
                hashCode = (hashCode * 59) + AttacksPerMember.GetHashCode();
                hashCode = (hashCode * 59) + Clan.GetHashCode();
                hashCode = (hashCode * 59) + EndTime.GetHashCode();
                hashCode = (hashCode * 59) + Opponent.GetHashCode();
                hashCode = (hashCode * 59) + TeamSize.GetHashCode();

                if (Result != null)
                    hashCode = (hashCode * 59) + Result.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanWarLogEntry
    /// </summary>
    public class ClanWarLogEntryJsonConverter : JsonConverter<ClanWarLogEntry>
    {
        /// <summary>
        /// The format to use to serialize EndTime
        /// </summary>
        public static string EndTimeFormat { get; set; } = "yyyyMMdd'T'HHmmss.fff'Z'";

        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanWarLogEntry Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int? attacksPerMember = default;
            WarClanLogEntry? clan = default;
            DateTime? endTime = default;
            WarClanLogEntry? opponent = default;
            int? teamSize = default;
            Result? result = default;

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
                        case "attacksPerMember":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                attacksPerMember = utf8JsonReader.GetInt32();
                            break;
                        case "clan":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                clan = JsonSerializer.Deserialize<WarClanLogEntry>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "endTime":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                endTime = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "opponent":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                opponent = JsonSerializer.Deserialize<WarClanLogEntry>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "teamSize":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                teamSize = utf8JsonReader.GetInt32();
                            break;
                        case "result":
                            string resultRawValue = utf8JsonReader.GetString();
                            result = ResultConverter.FromStringOrDefault(resultRawValue);
                            break;
                        default:
                            break;
                    }
                }
            }

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (clan == null)
                throw new ArgumentNullException(nameof(clan), "Property is required for class ClanWarLogEntry.");

            if (teamSize == null)
                throw new ArgumentNullException(nameof(teamSize), "Property is required for class ClanWarLogEntry.");

            if (attacksPerMember == null)
                throw new ArgumentNullException(nameof(attacksPerMember), "Property is required for class ClanWarLogEntry.");

            if (opponent == null)
                throw new ArgumentNullException(nameof(opponent), "Property is required for class ClanWarLogEntry.");

            if (endTime == null)
                throw new ArgumentNullException(nameof(endTime), "Property is required for class ClanWarLogEntry.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return new ClanWarLogEntry(attacksPerMember.Value, clan, endTime.Value, opponent, teamSize.Value, result.Value);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanWarLogEntry"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanWarLogEntry clanWarLogEntry, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("attacksPerMember", clanWarLogEntry.AttacksPerMember);
            writer.WritePropertyName("clan");
            JsonSerializer.Serialize(writer, clanWarLogEntry.Clan, jsonSerializerOptions);
            writer.WriteString("endTime", clanWarLogEntry.EndTime.ToString(EndTimeFormat));
            writer.WritePropertyName("opponent");
            JsonSerializer.Serialize(writer, clanWarLogEntry.Opponent, jsonSerializerOptions);
            writer.WriteNumber("teamSize", clanWarLogEntry.TeamSize);
            if (clanWarLogEntry.Result == null)
                writer.WriteNull("result");
            var resultRawValue = ResultConverter.ToJsonValue(clanWarLogEntry.Result.Value);
            if (resultRawValue != null)
                writer.WriteString("result", resultRawValue);
            else
                writer.WriteNull("result");

            writer.WriteEndObject();
        }
    }
}
