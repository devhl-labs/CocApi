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
    /// PlayerLegendStatistics
    /// </summary>
    public partial class PlayerLegendStatistics : IEquatable<PlayerLegendStatistics?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLegendStatistics" /> class.
        /// </summary>
        /// <param name="currentSeason">currentSeason</param>
        /// <param name="legendTrophies">legendTrophies</param>
        /// <param name="bestSeason">bestSeason</param>
        /// <param name="bestVersusSeason">bestVersusSeason</param>
        /// <param name="previousSeason">previousSeason</param>
        /// <param name="previousVersusSeason">previousVersusSeason</param>
        [JsonConstructor]
        internal PlayerLegendStatistics(LegendLeagueTournamentSeasonResult currentSeason, int legendTrophies, LegendLeagueTournamentSeasonResult? bestSeason = default, LegendLeagueTournamentSeasonResult? bestVersusSeason = default, LegendLeagueTournamentSeasonResult? previousSeason = default, LegendLeagueTournamentSeasonResult? previousVersusSeason = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (currentSeason == null)
                throw new ArgumentNullException("currentSeason is a required property for PlayerLegendStatistics and cannot be null.");

            if (legendTrophies == null)
                throw new ArgumentNullException("legendTrophies is a required property for PlayerLegendStatistics and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            CurrentSeason = currentSeason;
            LegendTrophies = legendTrophies;
            BestSeason = bestSeason;
            BestVersusSeason = bestVersusSeason;
            PreviousSeason = previousSeason;
            PreviousVersusSeason = previousVersusSeason;
        }

        /// <summary>
        /// Gets or Sets CurrentSeason
        /// </summary>
        [JsonPropertyName("currentSeason")]
        public LegendLeagueTournamentSeasonResult CurrentSeason { get; }

        /// <summary>
        /// Gets or Sets LegendTrophies
        /// </summary>
        [JsonPropertyName("legendTrophies")]
        public int LegendTrophies { get; }

        /// <summary>
        /// Gets or Sets BestSeason
        /// </summary>
        [JsonPropertyName("bestSeason")]
        public LegendLeagueTournamentSeasonResult? BestSeason { get; }

        /// <summary>
        /// Gets or Sets BestVersusSeason
        /// </summary>
        [JsonPropertyName("bestVersusSeason")]
        public LegendLeagueTournamentSeasonResult? BestVersusSeason { get; }

        /// <summary>
        /// Gets or Sets PreviousSeason
        /// </summary>
        [JsonPropertyName("previousSeason")]
        public LegendLeagueTournamentSeasonResult? PreviousSeason { get; }

        /// <summary>
        /// Gets or Sets PreviousVersusSeason
        /// </summary>
        [JsonPropertyName("previousVersusSeason")]
        public LegendLeagueTournamentSeasonResult? PreviousVersusSeason { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class PlayerLegendStatistics {\n");
            sb.Append("  CurrentSeason: ").Append(CurrentSeason).Append("\n");
            sb.Append("  LegendTrophies: ").Append(LegendTrophies).Append("\n");
            sb.Append("  BestSeason: ").Append(BestSeason).Append("\n");
            sb.Append("  BestVersusSeason: ").Append(BestVersusSeason).Append("\n");
            sb.Append("  PreviousSeason: ").Append(PreviousSeason).Append("\n");
            sb.Append("  PreviousVersusSeason: ").Append(PreviousVersusSeason).Append("\n");
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
            return this.Equals(input as PlayerLegendStatistics);
        }

        /// <summary>
        /// Returns true if PlayerLegendStatistics instances are equal
        /// </summary>
        /// <param name="input">Instance of PlayerLegendStatistics to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlayerLegendStatistics? input)
        {
            if (input == null)
                return false;

            return 
                (
                    CurrentSeason == input.CurrentSeason ||
                    (CurrentSeason != null &&
                    CurrentSeason.Equals(input.CurrentSeason))
                ) && 
                (
                    LegendTrophies == input.LegendTrophies ||
                    (LegendTrophies != null &&
                    LegendTrophies.Equals(input.LegendTrophies))
                ) && 
                (
                    BestSeason == input.BestSeason ||
                    (BestSeason != null &&
                    BestSeason.Equals(input.BestSeason))
                ) && 
                (
                    BestVersusSeason == input.BestVersusSeason ||
                    (BestVersusSeason != null &&
                    BestVersusSeason.Equals(input.BestVersusSeason))
                ) && 
                (
                    PreviousSeason == input.PreviousSeason ||
                    (PreviousSeason != null &&
                    PreviousSeason.Equals(input.PreviousSeason))
                ) && 
                (
                    PreviousVersusSeason == input.PreviousVersusSeason ||
                    (PreviousVersusSeason != null &&
                    PreviousVersusSeason.Equals(input.PreviousVersusSeason))
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
                hashCode = (hashCode * 59) + CurrentSeason.GetHashCode();
                hashCode = (hashCode * 59) + LegendTrophies.GetHashCode();

                if (BestSeason != null)
                    hashCode = (hashCode * 59) + BestSeason.GetHashCode();

                if (BestVersusSeason != null)
                    hashCode = (hashCode * 59) + BestVersusSeason.GetHashCode();

                if (PreviousSeason != null)
                    hashCode = (hashCode * 59) + PreviousSeason.GetHashCode();

                if (PreviousVersusSeason != null)
                    hashCode = (hashCode * 59) + PreviousVersusSeason.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type PlayerLegendStatistics
    /// </summary>
    public class PlayerLegendStatisticsJsonConverter : JsonConverter<PlayerLegendStatistics>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override PlayerLegendStatistics Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int currentDepth = reader.CurrentDepth;

            if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = reader.TokenType;

            LegendLeagueTournamentSeasonResult currentSeason = default;
            int legendTrophies = default;
            LegendLeagueTournamentSeasonResult bestSeason = default;
            LegendLeagueTournamentSeasonResult bestVersusSeason = default;
            LegendLeagueTournamentSeasonResult previousSeason = default;
            LegendLeagueTournamentSeasonResult previousVersusSeason = default;

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
                        case "currentSeason":
                            Utf8JsonReader currentSeasonReader = reader;
                            currentSeason = JsonSerializer.Deserialize<LegendLeagueTournamentSeasonResult>(ref reader, options);
                            break;
                        case "legendTrophies":
                            legendTrophies = reader.GetInt32();
                            break;
                        case "bestSeason":
                            Utf8JsonReader bestSeasonReader = reader;
                            bestSeason = JsonSerializer.Deserialize<LegendLeagueTournamentSeasonResult>(ref reader, options);
                            break;
                        case "bestVersusSeason":
                            Utf8JsonReader bestVersusSeasonReader = reader;
                            bestVersusSeason = JsonSerializer.Deserialize<LegendLeagueTournamentSeasonResult>(ref reader, options);
                            break;
                        case "previousSeason":
                            Utf8JsonReader previousSeasonReader = reader;
                            previousSeason = JsonSerializer.Deserialize<LegendLeagueTournamentSeasonResult>(ref reader, options);
                            break;
                        case "previousVersusSeason":
                            Utf8JsonReader previousVersusSeasonReader = reader;
                            previousVersusSeason = JsonSerializer.Deserialize<LegendLeagueTournamentSeasonResult>(ref reader, options);
                            break;
                    }
                }
            }

            return new PlayerLegendStatistics(currentSeason, legendTrophies, bestSeason, bestVersusSeason, previousSeason, previousVersusSeason);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="playerLegendStatistics"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, PlayerLegendStatistics playerLegendStatistics, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("currentSeason");
            JsonSerializer.Serialize(writer, playerLegendStatistics.CurrentSeason, options);
            writer.WriteNumber("legendTrophies", (int)playerLegendStatistics.LegendTrophies);
            writer.WritePropertyName("bestSeason");
            JsonSerializer.Serialize(writer, playerLegendStatistics.BestSeason, options);
            writer.WritePropertyName("bestVersusSeason");
            JsonSerializer.Serialize(writer, playerLegendStatistics.BestVersusSeason, options);
            writer.WritePropertyName("previousSeason");
            JsonSerializer.Serialize(writer, playerLegendStatistics.PreviousSeason, options);
            writer.WritePropertyName("previousVersusSeason");
            JsonSerializer.Serialize(writer, playerLegendStatistics.PreviousVersusSeason, options);

            writer.WriteEndObject();
        }
    }
}
