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
    /// ClanCapitalRaidSeasonDistrict
    /// </summary>
    public partial class ClanCapitalRaidSeasonDistrict : IEquatable<ClanCapitalRaidSeasonDistrict?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanCapitalRaidSeasonDistrict" /> class.
        /// </summary>
        /// <param name="attackCount">attackCount</param>
        /// <param name="destructionPercent">destructionPercent</param>
        /// <param name="districtHallLevel">districtHallLevel</param>
        /// <param name="id">id</param>
        /// <param name="name">name</param>
        /// <param name="stars">stars</param>
        /// <param name="totalLooted">totalLooted</param>
        /// <param name="attacks">attacks</param>
        [JsonConstructor]
        internal ClanCapitalRaidSeasonDistrict(int attackCount, int destructionPercent, int districtHallLevel, int id, string name, int stars, int totalLooted, List<ClanCapitalRaidSeasonAttack>? attacks = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (stars == null)
                throw new ArgumentNullException("stars is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

            if (id == null)
                throw new ArgumentNullException("id is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

            if (destructionPercent == null)
                throw new ArgumentNullException("destructionPercent is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

            if (attackCount == null)
                throw new ArgumentNullException("attackCount is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

            if (totalLooted == null)
                throw new ArgumentNullException("totalLooted is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

            if (districtHallLevel == null)
                throw new ArgumentNullException("districtHallLevel is a required property for ClanCapitalRaidSeasonDistrict and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            AttackCount = attackCount;
            DestructionPercent = destructionPercent;
            DistrictHallLevel = districtHallLevel;
            Id = id;
            Name = name;
            Stars = stars;
            TotalLooted = totalLooted;
            Attacks = attacks;
        }

        /// <summary>
        /// Gets or Sets AttackCount
        /// </summary>
        [JsonPropertyName("attackCount")]
        public int AttackCount { get; }

        /// <summary>
        /// Gets or Sets DestructionPercent
        /// </summary>
        [JsonPropertyName("destructionPercent")]
        public int DestructionPercent { get; }

        /// <summary>
        /// Gets or Sets DistrictHallLevel
        /// </summary>
        [JsonPropertyName("districtHallLevel")]
        public int DistrictHallLevel { get; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or Sets Stars
        /// </summary>
        [JsonPropertyName("stars")]
        public int Stars { get; }

        /// <summary>
        /// Gets or Sets TotalLooted
        /// </summary>
        [JsonPropertyName("totalLooted")]
        public int TotalLooted { get; }

        /// <summary>
        /// Gets or Sets Attacks
        /// </summary>
        [JsonPropertyName("attacks")]
        public List<ClanCapitalRaidSeasonAttack>? Attacks { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanCapitalRaidSeasonDistrict {\n");
            sb.Append("  AttackCount: ").Append(AttackCount).Append("\n");
            sb.Append("  DestructionPercent: ").Append(DestructionPercent).Append("\n");
            sb.Append("  DistrictHallLevel: ").Append(DistrictHallLevel).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Stars: ").Append(Stars).Append("\n");
            sb.Append("  TotalLooted: ").Append(TotalLooted).Append("\n");
            sb.Append("  Attacks: ").Append(Attacks).Append("\n");
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
            return this.Equals(input as ClanCapitalRaidSeasonDistrict);
        }

        /// <summary>
        /// Returns true if ClanCapitalRaidSeasonDistrict instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanCapitalRaidSeasonDistrict to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanCapitalRaidSeasonDistrict? input)
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
                    DestructionPercent == input.DestructionPercent ||
                    (DestructionPercent != null &&
                    DestructionPercent.Equals(input.DestructionPercent))
                ) && 
                (
                    DistrictHallLevel == input.DistrictHallLevel ||
                    (DistrictHallLevel != null &&
                    DistrictHallLevel.Equals(input.DistrictHallLevel))
                ) && 
                (
                    Id == input.Id ||
                    (Id != null &&
                    Id.Equals(input.Id))
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    Stars == input.Stars ||
                    (Stars != null &&
                    Stars.Equals(input.Stars))
                ) && 
                (
                    TotalLooted == input.TotalLooted ||
                    (TotalLooted != null &&
                    TotalLooted.Equals(input.TotalLooted))
                ) && 
                (
                    Attacks == input.Attacks ||
                    Attacks != null &&
                    input.Attacks != null &&
                    Attacks.SequenceEqual(input.Attacks)
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
                hashCode = (hashCode * 59) + DestructionPercent.GetHashCode();
                hashCode = (hashCode * 59) + DistrictHallLevel.GetHashCode();
                hashCode = (hashCode * 59) + Id.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + Stars.GetHashCode();
                hashCode = (hashCode * 59) + TotalLooted.GetHashCode();

                if (Attacks != null)
                    hashCode = (hashCode * 59) + Attacks.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanCapitalRaidSeasonDistrict
    /// </summary>
    public class ClanCapitalRaidSeasonDistrictJsonConverter : JsonConverter<ClanCapitalRaidSeasonDistrict>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanCapitalRaidSeasonDistrict Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int attackCount = default;
            int destructionPercent = default;
            int districtHallLevel = default;
            int id = default;
            string name = default;
            int stars = default;
            int totalLooted = default;
            List<ClanCapitalRaidSeasonAttack> attacks = default;

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
                            attackCount = utf8JsonReader.GetInt32();
                            break;
                        case "destructionPercent":
                            destructionPercent = utf8JsonReader.GetInt32();
                            break;
                        case "districtHallLevel":
                            districtHallLevel = utf8JsonReader.GetInt32();
                            break;
                        case "id":
                            id = utf8JsonReader.GetInt32();
                            break;
                        case "name":
                            name = JsonSerializer.Deserialize<string>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "stars":
                            stars = utf8JsonReader.GetInt32();
                            break;
                        case "totalLooted":
                            totalLooted = utf8JsonReader.GetInt32();
                            break;
                        case "attacks":
                            attacks = JsonSerializer.Deserialize<List<ClanCapitalRaidSeasonAttack>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        default:
                            break;
                    }
                }
            }

            return new ClanCapitalRaidSeasonDistrict(attackCount, destructionPercent, districtHallLevel, id, name, stars, totalLooted, attacks);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanCapitalRaidSeasonDistrict"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanCapitalRaidSeasonDistrict clanCapitalRaidSeasonDistrict, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("attackCount", clanCapitalRaidSeasonDistrict.AttackCount);
            writer.WriteNumber("destructionPercent", clanCapitalRaidSeasonDistrict.DestructionPercent);
            writer.WriteNumber("districtHallLevel", clanCapitalRaidSeasonDistrict.DistrictHallLevel);
            writer.WriteNumber("id", clanCapitalRaidSeasonDistrict.Id);
            writer.WritePropertyName("name");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeasonDistrict.Name, jsonSerializerOptions);
            writer.WriteNumber("stars", clanCapitalRaidSeasonDistrict.Stars);
            writer.WriteNumber("totalLooted", clanCapitalRaidSeasonDistrict.TotalLooted);
            writer.WritePropertyName("attacks");
            JsonSerializer.Serialize(writer, clanCapitalRaidSeasonDistrict.Attacks, jsonSerializerOptions);

            writer.WriteEndObject();
        }
    }
}