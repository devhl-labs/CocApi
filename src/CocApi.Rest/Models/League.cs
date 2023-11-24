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
using CocApi.Rest.Client;

namespace CocApi.Rest.Models
{
    /// <summary>
    /// League
    /// </summary>
    public partial class League : IEquatable<League?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="League" /> class.
        /// </summary>
        /// <param name="iconUrls">iconUrls</param>
        /// <param name="id">id</param>
        /// <param name="name">name</param>
        [JsonConstructor]
        internal League(IconUrls iconUrls, int id, string name)
        {
            IconUrls = iconUrls;
            Id = id;
            Name = name;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets IconUrls
        /// </summary>
        [JsonPropertyName("iconUrls")]
        public IconUrls IconUrls { get; }

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
            sb.Append("class League {\n");
            sb.Append("  IconUrls: ").Append(IconUrls).Append("\n");
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
            return this.Equals(input as League);
        }

        /// <summary>
        /// Returns true if League instances are equal
        /// </summary>
        /// <param name="input">Instance of League to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(League? input)
        {
            if (input == null)
                return false;

            return 
                (
                    IconUrls == input.IconUrls ||
                    (IconUrls != null &&
                    IconUrls.Equals(input.IconUrls))
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
                hashCode = (hashCode * 59) + IconUrls.GetHashCode();
                hashCode = (hashCode * 59) + Id.GetHashCode();
                hashCode = (hashCode * 59) + Name.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type <see cref="League" />
    /// </summary>
    public class LeagueJsonConverter : JsonConverter<League>
    {
        /// <summary>
        /// Deserializes json to <see cref="League" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override League Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<IconUrls?> iconUrls = default;
            Option<int?> id = default;
            Option<string?> name = default;

            while (utf8JsonReader.Read())
            {
                if (startingTokenType == JsonTokenType.StartObject && utf8JsonReader.TokenType == JsonTokenType.EndObject && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (startingTokenType == JsonTokenType.StartArray && utf8JsonReader.TokenType == JsonTokenType.EndArray && currentDepth == utf8JsonReader.CurrentDepth)
                    break;

                if (utf8JsonReader.TokenType == JsonTokenType.PropertyName && currentDepth == utf8JsonReader.CurrentDepth - 1)
                {
                    string? localVarJsonPropertyName = utf8JsonReader.GetString();
                    utf8JsonReader.Read();

                    switch (localVarJsonPropertyName)
                    {
                        case "iconUrls":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                iconUrls = new Option<IconUrls?>(JsonSerializer.Deserialize<IconUrls>(ref utf8JsonReader, jsonSerializerOptions)!);
                            break;
                        case "id":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                id = new Option<int?>(utf8JsonReader.GetInt32());
                            break;
                        case "name":
                            name = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!iconUrls.IsSet)
                throw new ArgumentException("Property is required for class League.", nameof(iconUrls));

            if (!id.IsSet)
                throw new ArgumentException("Property is required for class League.", nameof(id));

            if (!name.IsSet)
                throw new ArgumentException("Property is required for class League.", nameof(name));

            if (iconUrls.IsSet && iconUrls.Value == null)
                throw new ArgumentNullException(nameof(iconUrls), "Property is not nullable for class League.");

            if (id.IsSet && id.Value == null)
                throw new ArgumentNullException(nameof(id), "Property is not nullable for class League.");

            if (name.IsSet && name.Value == null)
                throw new ArgumentNullException(nameof(name), "Property is not nullable for class League.");

            return new League(iconUrls.Value!, id.Value!.Value!, name.Value!);
        }

        /// <summary>
        /// Serializes a <see cref="League" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="league"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, League league, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, league, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="League" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="league"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, League league, JsonSerializerOptions jsonSerializerOptions)
        {
            if (league.IconUrls == null)
                throw new ArgumentNullException(nameof(league.IconUrls), "Property is required for class League.");

            if (league.Name == null)
                throw new ArgumentNullException(nameof(league.Name), "Property is required for class League.");

            writer.WritePropertyName("iconUrls");
            JsonSerializer.Serialize(writer, league.IconUrls, jsonSerializerOptions);
            writer.WriteNumber("id", league.Id);

            writer.WriteString("name", league.Name);
        }
    }
}
