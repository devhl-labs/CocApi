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
    /// ClanDistrictData
    /// </summary>
    public partial class ClanDistrictData : IEquatable<ClanDistrictData?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanDistrictData" /> class.
        /// </summary>
        /// <param name="districtHallLevel">districtHallLevel</param>
        /// <param name="id">id</param>
        /// <param name="name">name</param>
        [JsonConstructor]
        internal ClanDistrictData(int districtHallLevel, int id, string name)
        {
            DistrictHallLevel = districtHallLevel;
            Id = id;
            Name = name;
            OnCreated();
        }

        partial void OnCreated();

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
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanDistrictData {\n");
            sb.Append("  DistrictHallLevel: ").Append(DistrictHallLevel).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
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
            return this.Equals(input as ClanDistrictData);
        }

        /// <summary>
        /// Returns true if ClanDistrictData instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanDistrictData to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanDistrictData? input)
        {
            if (input == null)
                return false;

            return 
                (
                    DistrictHallLevel == input.DistrictHallLevel ||
                    DistrictHallLevel.Equals(input.DistrictHallLevel)
                ) && 
                (
                    Id == input.Id ||
                    Id.Equals(input.Id)
                ) && 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
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
                hashCode = (hashCode * 59) + DistrictHallLevel.GetHashCode();
                hashCode = (hashCode * 59) + Id.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanDistrictData
    /// </summary>
    public class ClanDistrictDataJsonConverter : JsonConverter<ClanDistrictData>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanDistrictData Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            int? districtHallLevel = default;
            int? id = default;
            string? name = default;

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
                        case "districtHallLevel":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                districtHallLevel = utf8JsonReader.GetInt32();
                            break;
                        case "id":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                id = utf8JsonReader.GetInt32();
                            break;
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        default:
                            break;
                    }
                }
            }

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class ClanDistrictData.");

            if (id == null)
                throw new ArgumentNullException(nameof(id), "Property is required for class ClanDistrictData.");

            if (districtHallLevel == null)
                throw new ArgumentNullException(nameof(districtHallLevel), "Property is required for class ClanDistrictData.");

            return new ClanDistrictData(districtHallLevel.Value, id.Value, name);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanDistrictData"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanDistrictData clanDistrictData, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteNumber("districtHallLevel", clanDistrictData.DistrictHallLevel);
            writer.WriteNumber("id", clanDistrictData.Id);
            writer.WriteString("name", clanDistrictData.Name);

            writer.WriteEndObject();
        }
    }
}
