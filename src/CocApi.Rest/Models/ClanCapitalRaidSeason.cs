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
    /// ClanCapitalRaidSeason
    /// </summary>
    public partial class ClanCapitalRaidSeason : IEquatable<ClanCapitalRaidSeason?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanCapitalRaidSeason" /> class.
        /// </summary>
        /// <param name="attackLog">attackLog</param>
        /// <param name="capitalTotalLoot">capitalTotalLoot</param>
        /// <param name="defenseLog">defenseLog</param>
        /// <param name="defensiveReward">defensiveReward</param>
        /// <param name="endTime">endTime</param>
        /// <param name="enemyDistrictsDestroyed">enemyDistrictsDestroyed</param>
        /// <param name="offensiveReward">offensiveReward</param>
        /// <param name="raidsCompleted">raidsCompleted</param>
        /// <param name="startTime">startTime</param>
        /// <param name="state">state</param>
        /// <param name="totalAttacks">totalAttacks</param>
        /// <param name="members">members</param>
        [JsonConstructor]
        internal ClanCapitalRaidSeason(List<ClanCapitalRaidSeasonAttackLogEntry> attackLog, int capitalTotalLoot, List<ClanCapitalRaidSeasonDefenseLogEntry> defenseLog, int defensiveReward, DateTime endTime, int enemyDistrictsDestroyed, int offensiveReward, int raidsCompleted, DateTime startTime, StateEnum state, int totalAttacks, List<ClanCapitalRaidSeasonMember>? members = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (attackLog == null)
                throw new ArgumentNullException("attackLog is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (defenseLog == null)
                throw new ArgumentNullException("defenseLog is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (state == null)
                throw new ArgumentNullException("state is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (startTime == null)
                throw new ArgumentNullException("startTime is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (endTime == null)
                throw new ArgumentNullException("endTime is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (capitalTotalLoot == null)
                throw new ArgumentNullException("capitalTotalLoot is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (raidsCompleted == null)
                throw new ArgumentNullException("raidsCompleted is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (totalAttacks == null)
                throw new ArgumentNullException("totalAttacks is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (enemyDistrictsDestroyed == null)
                throw new ArgumentNullException("enemyDistrictsDestroyed is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (offensiveReward == null)
                throw new ArgumentNullException("offensiveReward is a required property for ClanCapitalRaidSeason and cannot be null.");

            if (defensiveReward == null)
                throw new ArgumentNullException("defensiveReward is a required property for ClanCapitalRaidSeason and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            AttackLog = attackLog;
            CapitalTotalLoot = capitalTotalLoot;
            DefenseLog = defenseLog;
            DefensiveReward = defensiveReward;
            EndTime = endTime;
            EnemyDistrictsDestroyed = enemyDistrictsDestroyed;
            OffensiveReward = offensiveReward;
            RaidsCompleted = raidsCompleted;
            StartTime = startTime;
            State = state;
            TotalAttacks = totalAttacks;
            Members = members;
        }

        /// <summary>
        /// Defines State
        /// </summary>
        public enum StateEnum
        {
            /// <summary>
            /// Enum Unknown for value: unknown
            /// </summary>
            Unknown = 0,

            /// <summary>
            /// Enum Ongoing for value: ongoing
            /// </summary>
            Ongoing = 1,

            /// <summary>
            /// Enum Ended for value: ended
            /// </summary>
            Ended = 2

        }

        /// <summary>
        /// Returns a StateEnum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static StateEnum StateEnumFromString(string value)
        {
            if (value == "unknown")
                return StateEnum.Unknown;

            if (value == "ongoing")
                return StateEnum.Ongoing;

            if (value == "ended")
                return StateEnum.Ended;

            throw new NotImplementedException($"Could not convert value to type StateEnum: '{value}'");
        }

        /// <summary>
        /// Returns equivalent json value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string StateEnumToJsonValue(StateEnum value)
        {
            if (value == StateEnum.Unknown)
                return "unknown";

            if (value == StateEnum.Ongoing)
                return "ongoing";

            if (value == StateEnum.Ended)
                return "ended";

            throw new NotImplementedException($"Value could not be handled: '{value}'");
        }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [JsonPropertyName("state")]
        public StateEnum State { get; }

        /// <summary>
        /// Gets or Sets AttackLog
        /// </summary>
        [JsonPropertyName("attackLog")]
        public List<ClanCapitalRaidSeasonAttackLogEntry> AttackLog { get; }

        /// <summary>
        /// Gets or Sets CapitalTotalLoot
        /// </summary>
        [JsonPropertyName("capitalTotalLoot")]
        public int CapitalTotalLoot { get; }

        /// <summary>
        /// Gets or Sets DefenseLog
        /// </summary>
        [JsonPropertyName("defenseLog")]
        public List<ClanCapitalRaidSeasonDefenseLogEntry> DefenseLog { get; }

        /// <summary>
        /// Gets or Sets DefensiveReward
        /// </summary>
        [JsonPropertyName("defensiveReward")]
        public int DefensiveReward { get; }

        /// <summary>
        /// Gets or Sets EndTime
        /// </summary>
        [JsonPropertyName("endTime")]
        public DateTime EndTime { get; }

        /// <summary>
        /// Gets or Sets EnemyDistrictsDestroyed
        /// </summary>
        [JsonPropertyName("enemyDistrictsDestroyed")]
        public int EnemyDistrictsDestroyed { get; }

        /// <summary>
        /// Gets or Sets OffensiveReward
        /// </summary>
        [JsonPropertyName("offensiveReward")]
        public int OffensiveReward { get; }

        /// <summary>
        /// Gets or Sets RaidsCompleted
        /// </summary>
        [JsonPropertyName("raidsCompleted")]
        public int RaidsCompleted { get; }

        /// <summary>
        /// Gets or Sets StartTime
        /// </summary>
        [JsonPropertyName("startTime")]
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets or Sets TotalAttacks
        /// </summary>
        [JsonPropertyName("totalAttacks")]
        public int TotalAttacks { get; }

        /// <summary>
        /// Gets or Sets Members
        /// </summary>
        [JsonPropertyName("members")]
        public List<ClanCapitalRaidSeasonMember>? Members { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanCapitalRaidSeason {\n");
            sb.Append("  AttackLog: ").Append(AttackLog).Append("\n");
            sb.Append("  CapitalTotalLoot: ").Append(CapitalTotalLoot).Append("\n");
            sb.Append("  DefenseLog: ").Append(DefenseLog).Append("\n");
            sb.Append("  DefensiveReward: ").Append(DefensiveReward).Append("\n");
            sb.Append("  EndTime: ").Append(EndTime).Append("\n");
            sb.Append("  EnemyDistrictsDestroyed: ").Append(EnemyDistrictsDestroyed).Append("\n");
            sb.Append("  OffensiveReward: ").Append(OffensiveReward).Append("\n");
            sb.Append("  RaidsCompleted: ").Append(RaidsCompleted).Append("\n");
            sb.Append("  StartTime: ").Append(StartTime).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
            sb.Append("  TotalAttacks: ").Append(TotalAttacks).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
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
            return this.Equals(input as ClanCapitalRaidSeason);
        }

        /// <summary>
        /// Returns true if ClanCapitalRaidSeason instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanCapitalRaidSeason to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanCapitalRaidSeason? input)
        {
            if (input == null)
                return false;

            return 
                (
                    AttackLog == input.AttackLog ||
                    AttackLog != null &&
                    input.AttackLog != null &&
                    AttackLog.SequenceEqual(input.AttackLog)
                ) && 
                (
                    CapitalTotalLoot == input.CapitalTotalLoot ||
                    (CapitalTotalLoot != null &&
                    CapitalTotalLoot.Equals(input.CapitalTotalLoot))
                ) && 
                (
                    DefenseLog == input.DefenseLog ||
                    DefenseLog != null &&
                    input.DefenseLog != null &&
                    DefenseLog.SequenceEqual(input.DefenseLog)
                ) && 
                (
                    DefensiveReward == input.DefensiveReward ||
                    (DefensiveReward != null &&
                    DefensiveReward.Equals(input.DefensiveReward))
                ) && 
                (
                    EndTime == input.EndTime ||
                    (EndTime != null &&
                    EndTime.Equals(input.EndTime))
                ) && 
                (
                    EnemyDistrictsDestroyed == input.EnemyDistrictsDestroyed ||
                    (EnemyDistrictsDestroyed != null &&
                    EnemyDistrictsDestroyed.Equals(input.EnemyDistrictsDestroyed))
                ) && 
                (
                    OffensiveReward == input.OffensiveReward ||
                    (OffensiveReward != null &&
                    OffensiveReward.Equals(input.OffensiveReward))
                ) && 
                (
                    RaidsCompleted == input.RaidsCompleted ||
                    (RaidsCompleted != null &&
                    RaidsCompleted.Equals(input.RaidsCompleted))
                ) && 
                (
                    StartTime == input.StartTime ||
                    (StartTime != null &&
                    StartTime.Equals(input.StartTime))
                ) && 
                (
                    State == input.State ||
                    (State != null &&
                    State.Equals(input.State))
                ) && 
                (
                    TotalAttacks == input.TotalAttacks ||
                    (TotalAttacks != null &&
                    TotalAttacks.Equals(input.TotalAttacks))
                ) && 
                (
                    Members == input.Members ||
                    Members != null &&
                    input.Members != null &&
                    Members.SequenceEqual(input.Members)
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
                hashCode = (hashCode * 59) + AttackLog.GetHashCode();
                hashCode = (hashCode * 59) + CapitalTotalLoot.GetHashCode();
                hashCode = (hashCode * 59) + DefenseLog.GetHashCode();
                hashCode = (hashCode * 59) + DefensiveReward.GetHashCode();
                hashCode = (hashCode * 59) + EndTime.GetHashCode();
                hashCode = (hashCode * 59) + EnemyDistrictsDestroyed.GetHashCode();
                hashCode = (hashCode * 59) + OffensiveReward.GetHashCode();
                hashCode = (hashCode * 59) + RaidsCompleted.GetHashCode();
                hashCode = (hashCode * 59) + StartTime.GetHashCode();
                hashCode = (hashCode * 59) + State.GetHashCode();
                hashCode = (hashCode * 59) + TotalAttacks.GetHashCode();

                if (Members != null)
                    hashCode = (hashCode * 59) + Members.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanCapitalRaidSeason
    /// </summary>
    public class ClanCapitalRaidSeasonJsonConverter : JsonConverter<ClanCapitalRaidSeason>
    {
        /// <summary>
        /// The format to use to serialize EndTime
        /// </summary>
        public static string EndTimeFormat { get; set; } = "yyyyMMdd'T'HHmmss.fff'Z'";

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
        public override ClanCapitalRaidSeason Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            List<ClanCapitalRaidSeasonAttackLogEntry> attackLog = default;
            int capitalTotalLoot = default;
            List<ClanCapitalRaidSeasonDefenseLogEntry> defenseLog = default;
            int defensiveReward = default;
            DateTime endTime = default;
            int enemyDistrictsDestroyed = default;
            int offensiveReward = default;
            int raidsCompleted = default;
            DateTime startTime = default;
            ClanCapitalRaidSeason.StateEnum state = default;
            int totalAttacks = default;
            List<ClanCapitalRaidSeasonMember> members = default;

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
                        case "attackLog":
                            attackLog = JsonSerializer.Deserialize<List<ClanCapitalRaidSeasonAttackLogEntry>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "capitalTotalLoot":
                            capitalTotalLoot = utf8JsonReader.GetInt32();
                            break;
                        case "defenseLog":
                            defenseLog = JsonSerializer.Deserialize<List<ClanCapitalRaidSeasonDefenseLogEntry>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "defensiveReward":
                            defensiveReward = utf8JsonReader.GetInt32();
                            break;
                        case "endTime":
                            endTime = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "enemyDistrictsDestroyed":
                            enemyDistrictsDestroyed = utf8JsonReader.GetInt32();
                            break;
                        case "offensiveReward":
                            offensiveReward = utf8JsonReader.GetInt32();
                            break;
                        case "raidsCompleted":
                            raidsCompleted = utf8JsonReader.GetInt32();
                            break;
                        case "startTime":
                            startTime = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "state":
                            string stateRawValue = utf8JsonReader.GetString();
                            state = ClanCapitalRaidSeason.StateEnumFromString(stateRawValue);
                            break;
                        case "totalAttacks":
                            totalAttacks = utf8JsonReader.GetInt32();
                            break;
                        case "members":
                            members = JsonSerializer.Deserialize<List<ClanCapitalRaidSeasonMember>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        default:
                            break;
                    }
                }
            }

            return new ClanCapitalRaidSeason(attackLog, capitalTotalLoot, defenseLog, defensiveReward, endTime, enemyDistrictsDestroyed, offensiveReward, raidsCompleted, startTime, state, totalAttacks, members);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeason"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanCapitalRaidSeason clanCapitalRaidSeason, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("attackLog");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeason.AttackLog, jsonSerializerOptions);
            writer.WriteNumber("capitalTotalLoot", clanCapitalRaidSeason.CapitalTotalLoot);
            writer.WritePropertyName("defenseLog");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeason.DefenseLog, jsonSerializerOptions);
            writer.WriteNumber("defensiveReward", clanCapitalRaidSeason.DefensiveReward);
            writer.WriteString("endTime", clanCapitalRaidSeason.EndTime.ToString(EndTimeFormat));
            writer.WriteNumber("enemyDistrictsDestroyed", clanCapitalRaidSeason.EnemyDistrictsDestroyed);
            writer.WriteNumber("offensiveReward", clanCapitalRaidSeason.OffensiveReward);
            writer.WriteNumber("raidsCompleted", clanCapitalRaidSeason.RaidsCompleted);
            writer.WriteString("startTime", clanCapitalRaidSeason.StartTime.ToString(StartTimeFormat));
            var stateRawValue = ClanCapitalRaidSeason.StateEnumToJsonValue(clanCapitalRaidSeason.State);
            if (stateRawValue != null)
                writer.WriteString("state", stateRawValue);
            else
                writer.WriteNull("state");
            writer.WriteNumber("totalAttacks", clanCapitalRaidSeason.TotalAttacks);
            writer.WritePropertyName("members");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeason.Members, jsonSerializerOptions);

            writer.WriteEndObject();
        }
    }
}
