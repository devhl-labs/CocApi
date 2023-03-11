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
    /// ClanWarLeagueGroup
    /// </summary>
    public partial class ClanWarLeagueGroup : IEquatable<ClanWarLeagueGroup?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarLeagueGroup" /> class.
        /// </summary>
        /// <param name="clans">clans</param>
        /// <param name="rounds">rounds</param>
        /// <param name="season">season</param>
        /// <param name="state">state</param>
        [JsonConstructor]
        internal ClanWarLeagueGroup(List<ClanWarLeagueClan> clans, List<ClanWarLeagueRound> rounds, DateTime season, GroupState? state = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (season == null)
                throw new ArgumentNullException("season is a required property for ClanWarLeagueGroup and cannot be null.");

            if (clans == null)
                throw new ArgumentNullException("clans is a required property for ClanWarLeagueGroup and cannot be null.");

            if (rounds == null)
                throw new ArgumentNullException("rounds is a required property for ClanWarLeagueGroup and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            Clans = clans;
            Rounds = rounds;
            Season = season;
            State = state;
        }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [JsonPropertyName("state")]
        public GroupState? State { get; }

        /// <summary>
        /// Gets or Sets Clans
        /// </summary>
        [JsonPropertyName("clans")]
        public List<ClanWarLeagueClan> Clans { get; }

        /// <summary>
        /// Gets or Sets Rounds
        /// </summary>
        [JsonPropertyName("rounds")]
        public List<ClanWarLeagueRound> Rounds { get; }

        /// <summary>
        /// Gets or Sets Season
        /// </summary>
        [JsonPropertyName("season")]
        public DateTime Season { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanWarLeagueGroup {\n");
            sb.Append("  Clans: ").Append(Clans).Append("\n");
            sb.Append("  Rounds: ").Append(Rounds).Append("\n");
            sb.Append("  Season: ").Append(Season).Append("\n");
            sb.Append("  State: ").Append(State).Append("\n");
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
            return this.Equals(input as ClanWarLeagueGroup);
        }

        /// <summary>
        /// Returns true if ClanWarLeagueGroup instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanWarLeagueGroup to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanWarLeagueGroup? input)
        {
            if (input == null)
                return false;

            return 
                (
                    Clans == input.Clans ||
                    Clans != null &&
                    input.Clans != null &&
                    Clans.SequenceEqual(input.Clans)
                ) && 
                (
                    Rounds == input.Rounds ||
                    Rounds != null &&
                    input.Rounds != null &&
                    Rounds.SequenceEqual(input.Rounds)
                ) && 
                (
                    Season == input.Season ||
                    (Season != null &&
                    Season.Equals(input.Season))
                ) && 
                (
                    State == input.State ||
                    (State != null &&
                    State.Equals(input.State))
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
                hashCode = (hashCode * 59) + Clans.GetHashCode();
                hashCode = (hashCode * 59) + Rounds.GetHashCode();
                hashCode = (hashCode * 59) + Season.GetHashCode();

                if (State != null)
                    hashCode = (hashCode * 59) + State.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanWarLeagueGroup
    /// </summary>
    public class ClanWarLeagueGroupJsonConverter : JsonConverter<ClanWarLeagueGroup>
    {
        /// <summary>
        /// The format to use to serialize Season
        /// </summary>
        public static string SeasonFormat { get; set; } = "yyyy'-'MM";

        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanWarLeagueGroup Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            List<ClanWarLeagueClan> clans = default;
            List<ClanWarLeagueRound> rounds = default;
            DateTime season = default;
            GroupState? state = default;

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
                        case "clans":
                            clans = JsonSerializer.Deserialize<List<ClanWarLeagueClan>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "rounds":
                            rounds = JsonSerializer.Deserialize<List<ClanWarLeagueRound>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "season":
                            season = JsonSerializer.Deserialize<DateTime>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "state":
                            string stateRawValue = utf8JsonReader.GetString();
                            state = GroupStateConverter.FromStringOrDefault(stateRawValue);
                            break;
                        default:
                            break;
                    }
                }
            }

            return new ClanWarLeagueGroup(clans, rounds, season, state);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanWarLeagueGroup"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanWarLeagueGroup clanWarLeagueGroup, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("clans");
            JsonSerializer.Serialize(writer, clanWarLeagueGroup.Clans, jsonSerializerOptions);
            writer.WritePropertyName("rounds");
            JsonSerializer.Serialize(writer, clanWarLeagueGroup.Rounds, jsonSerializerOptions);
            writer.WriteString("season", clanWarLeagueGroup.Season.ToString(SeasonFormat));
            if (clanWarLeagueGroup.State == null)
                writer.WriteNull("state");
            var stateRawValue = GroupStateConverter.ToJsonValue(clanWarLeagueGroup.State.Value);
            if (stateRawValue != null)
                writer.WriteString("state", stateRawValue);
            else
                writer.WriteNull("state");

            writer.WriteEndObject();
        }
    }
}
