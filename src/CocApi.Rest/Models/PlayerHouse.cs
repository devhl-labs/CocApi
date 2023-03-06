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
    /// PlayerHouse
    /// </summary>
    public partial class PlayerHouse : IEquatable<PlayerHouse?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerHouse" /> class.
        /// </summary>
        /// <param name="elements">elements</param>
        [JsonConstructor]
        internal PlayerHouse(List<PlayerHouseElement> elements)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (elements == null)
                throw new ArgumentNullException("elements is a required property for PlayerHouse and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            Elements = elements;
        }

        /// <summary>
        /// Gets or Sets Elements
        /// </summary>
        [JsonPropertyName("elements")]
        public List<PlayerHouseElement> Elements { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class PlayerHouse {\n");
            sb.Append("  Elements: ").Append(Elements).Append("\n");
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
            return this.Equals(input as PlayerHouse);
        }

        /// <summary>
        /// Returns true if PlayerHouse instances are equal
        /// </summary>
        /// <param name="input">Instance of PlayerHouse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PlayerHouse? input)
        {
            if (input == null)
                return false;

            return 
                (
                    Elements == input.Elements ||
                    Elements != null &&
                    input.Elements != null &&
                    Elements.SequenceEqual(input.Elements)
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
                hashCode = (hashCode * 59) + Elements.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type PlayerHouse
    /// </summary>
    public class PlayerHouseJsonConverter : JsonConverter<PlayerHouse>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override PlayerHouse Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            List<PlayerHouseElement> elements = default;

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
                        case "elements":
                            elements = JsonSerializer.Deserialize<List<PlayerHouseElement>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        default:
                            break;
                    }
                }
            }

            return new PlayerHouse(elements);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="playerHouse"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, PlayerHouse playerHouse, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("elements");
            JsonSerializer.Serialize(writer, playerHouse.Elements, jsonSerializerOptions);

            writer.WriteEndObject();
        }
    }
}