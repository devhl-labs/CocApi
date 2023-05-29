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
    /// ClanWar
    /// </summary>
    public partial class ClanWar : IEquatable<ClanWar?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWar" /> class.
        /// </summary>
        /// <param name="attacksPerMember">attacksPerMember</param>
        /// <param name="clan">clan</param>
        /// <param name="endTime">endTime</param>
        /// <param name="opponent">opponent</param>
        /// <param name="preparationStartTime">preparationStartTime</param>
        /// <param name="serverExpiration">serverExpiration</param>
        /// <param name="startTime">startTime</param>
        /// <param name="teamSize">teamSize</param>
        /// <param name="state">state</param>
        /// <param name="warTag">warTag</param>
        [JsonConstructor]
        internal ClanWar(int attacksPerMember, WarClan clan, DateTime endTime, WarClan opponent, DateTime preparationStartTime, DateTime serverExpiration, DateTime startTime, int teamSize, WarState? state = default, string? warTag = default)
        {
            AttacksPerMember = attacksPerMember;
            Clan = clan;
            EndTime = endTime;
            Opponent = opponent;
            PreparationStartTime = preparationStartTime;
            ServerExpiration = serverExpiration;
            StartTime = startTime;
            TeamSize = teamSize;
            State = state;
            WarTag = warTag;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [JsonPropertyName("state")]
        public WarState? State { get; }

        /// <summary>
        /// Gets or Sets AttacksPerMember
        /// </summary>
        [JsonPropertyName("attacksPerMember")]
        public int AttacksPerMember { get; private set; }

        /// <summary>
        /// Gets or Sets Clan
        /// </summary>
        [JsonPropertyName("clan")]
        public WarClan Clan { get; private set; }

        /// <summary>
        /// Gets or Sets EndTime
        /// </summary>
        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; }

        /// <summary>
        /// Gets or Sets Opponent
        /// </summary>
        [JsonPropertyName("opponent")]
        public WarClan Opponent { get; private set; }

        /// <summary>
        /// Gets or Sets PreparationStartTime
        /// </summary>
        [JsonPropertyName("preparationStartTime")]
        public DateTime PreparationStartTime { get; }

        /// <summary>
        /// Gets or Sets ServerExpiration
        /// </summary>
        [JsonPropertyName("serverExpiration")]
        public DateTime ServerExpiration { get; }

        /// <summary>
        /// Gets or Sets StartTime
        /// </summary>
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets or Sets TeamSize
        /// </summary>
        [JsonPropertyName("teamSize")]
        public int TeamSize { get; }

        /// <summary>
        /// Gets or Sets WarTag
        /// </summary>
        [JsonPropertyName("warTag")]
        public string? WarTag { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanWar {\n");
            sb.Append("  AttacksPerMember: ").Append(AttacksPerMember).Append("\n");
            sb.Append("  Clan: ").Append(Clan).Append("\n");
            sb.Append("  EndTime: ").Append(EndTime).Append("\n");
            sb.Append("  Opponent: ").Append(Opponent).Append("\n");
            sb.Append("  PreparationStartTime: ").Append(PreparationStartTime).Append("\n");
            sb.Append("  ServerExpiration: ").Append(ServerExpiration).Append("\n");
            sb.Append("  StartTime: ").Append(StartTime).Append("\n");
            sb.Append("  TeamSize: ").Append(TeamSize).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  WarTag: ").Append(WarTag).Append("\n");
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
            return this.Equals(input as ClanWar);
        }

        /// <summary>
        /// Returns true if ClanWar instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanWar to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanWar? input)
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
                    PreparationStartTime == input.PreparationStartTime ||
                    (PreparationStartTime != null &&
                    PreparationStartTime.Equals(input.PreparationStartTime))
                ) && 
                (
                    ServerExpiration == input.ServerExpiration ||
                    (ServerExpiration != null &&
                    ServerExpiration.Equals(input.ServerExpiration))
                ) && 
                (
                    StartTime == input.StartTime ||
                    (StartTime != null &&
                    StartTime.Equals(input.StartTime))
                ) && 
                (
                    TeamSize == input.TeamSize ||
                    (TeamSize != null &&
                    TeamSize.Equals(input.TeamSize))
                ) && 
                (
                    State == input.State ||
                    (State != null &&
                    State.Equals(input.State))
                ) && 
                (
                    WarTag == input.WarTag ||
                    (WarTag != null &&
                    WarTag.Equals(input.WarTag))
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
                hashCode = (hashCode * 59) + PreparationStartTime.GetHashCode();
                hashCode = (hashCode * 59) + ServerExpiration.GetHashCode();
                hashCode = (hashCode * 59) + StartTime.GetHashCode();
                hashCode = (hashCode * 59) + TeamSize.GetHashCode();

                if (State != null)
                    hashCode = (hashCode * 59) + State.GetHashCode();

                if (WarTag != null)
                    hashCode = (hashCode * 59) + WarTag.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanWar
    /// </summary>
    public class ClanWarJsonConverter : JsonConverter<ClanWar>
    {
        /// <summary>
        /// The format to use to serialize EndTime
        /// </summary>
        public static string EndTimeFormat { get; set; } = "yyyyMMdd'T'HHmmss.fff'Z'";

        /// <summary>
        /// The format to use to serialize PreparationStartTime
        /// </summary>
        public static string PreparationStartTimeFormat { get; set; } = "yyyyMMdd'T'HHmmss.fff'Z'";

        /// <summary>
        /// The format to use to serialize StartTime
        /// </summary>
        public static string StartTimeFormat { get; set; } = "yyyyMMdd'T'HHmmss.fff'Z'";

        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanWar Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int? attacksPerMember = default;
            WarClan? clan = default;
            DateTime? endTime = default;
            WarClan? opponent = default;
            DateTime? preparationStartTime = default;
            DateTime? serverExpiration = default;
            DateTime? startTime = default;
            int? teamSize = default;
            WarState? state = default;
            string? warTag = default;

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
                                clan = JsonSerializer.Deserialize<WarClan>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "endTime":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                endTime = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "opponent":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                opponent = JsonSerializer.Deserialize<WarClan>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "preparationStartTime":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                preparationStartTime = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "serverExpiration":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                serverExpiration = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "startTime":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                startTime = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "teamSize":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                teamSize = utf8JsonReader.GetInt32();
                            break;
                        case "state":
                            string stateRawValue = utf8JsonReader.GetString();
                            state = WarStateConverter.FromStringOrDefault(stateRawValue);
                            break;
                        case "warTag":
                            warTag = utf8JsonReader.GetString();
                            break;
                        default:
                            break;
                    }
                }
            }

            if (serverExpiration == null)
                serverExpiration = new DateTime(2023, 05, 01, 1, 1, 1, 1, 1);

            if (clan == null)
                throw new ArgumentNullException(nameof(clan), "Property is required for class ClanWar.");

            if (teamSize == null)
                throw new ArgumentNullException(nameof(teamSize), "Property is required for class ClanWar.");

            if (attacksPerMember == null)
                attacksPerMember = 1; // cwl war

            if (opponent == null)
                throw new ArgumentNullException(nameof(opponent), "Property is required for class ClanWar.");

            if (startTime == null)
                throw new ArgumentNullException(nameof(startTime), "Property is required for class ClanWar.");

            if (endTime == null)
                throw new ArgumentNullException(nameof(endTime), "Property is required for class ClanWar.");

            if (preparationStartTime == null)
                throw new ArgumentNullException(nameof(preparationStartTime), "Property is required for class ClanWar.");

            return new ClanWar(attacksPerMember.Value, clan, endTime.Value, opponent, preparationStartTime.Value, serverExpiration.Value, startTime.Value, teamSize.Value, state.Value, warTag);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanWar"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanWar clanWar, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("attacksPerMember", clanWar.AttacksPerMember);
            writer.WritePropertyName("clan");
            JsonSerializer.Serialize(writer, clanWar.Clan, jsonSerializerOptions);
            writer.WriteString("endTime", clanWar.EndTime.ToString(EndTimeFormat));
            writer.WritePropertyName("opponent");
            JsonSerializer.Serialize(writer, clanWar.Opponent, jsonSerializerOptions);
            writer.WriteString("preparationStartTime", clanWar.PreparationStartTime.ToString(PreparationStartTimeFormat));
            writer.WritePropertyName("serverExpiration");
            JsonSerializer.Serialize(writer, clanWar.ServerExpiration, jsonSerializerOptions);
            writer.WriteString("startTime", clanWar.StartTime.ToString(StartTimeFormat));
            writer.WriteNumber("teamSize", clanWar.TeamSize);
            if (clanWar.State == null)
                writer.WriteNull("state");
            var stateRawValue = WarStateConverter.ToJsonValue(clanWar.State.Value);
            if (stateRawValue != null)
                writer.WriteString("state", stateRawValue);
            else
                writer.WriteNull("state");
            writer.WriteString("warTag", clanWar.WarTag);

            writer.WriteEndObject();
        }
    }
}
