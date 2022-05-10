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
    /// BadgeUrls
    /// </summary>
    public partial class BadgeUrls : IEquatable<BadgeUrls?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadgeUrls" /> class.
        /// </summary>
        /// <param name="large">large</param>
        /// <param name="medium">medium</param>
        /// <param name="small">small</param>
        [JsonConstructor]
        internal BadgeUrls(string large, string medium, string small)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (small == null)
                throw new ArgumentNullException("small is a required property for BadgeUrls and cannot be null.");

            if (medium == null)
                throw new ArgumentNullException("medium is a required property for BadgeUrls and cannot be null.");

            if (large == null)
                throw new ArgumentNullException("large is a required property for BadgeUrls and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            Large = large;
            Medium = medium;
            Small = small;
        }

        /// <summary>
        /// Gets or Sets Large
        /// </summary>
        [JsonPropertyName("large")]
        public string Large { get; }

        /// <summary>
        /// Gets or Sets Medium
        /// </summary>
        [JsonPropertyName("medium")]
        public string Medium { get; }

        /// <summary>
        /// Gets or Sets Small
        /// </summary>
        [JsonPropertyName("small")]
        public string Small { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class BadgeUrls {\n");
            sb.Append("  Large: ").Append(Large).Append("\n");
            sb.Append("  Medium: ").Append(Medium).Append("\n");
            sb.Append("  Small: ").Append(Small).Append("\n");
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
            return this.Equals(input as BadgeUrls);
        }

        /// <summary>
        /// Returns true if BadgeUrls instances are equal
        /// </summary>
        /// <param name="input">Instance of BadgeUrls to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BadgeUrls? input)
        {
            if (input == null)
                return false;

            return 
                (
                    Large == input.Large ||
                    (Large != null &&
                    Large.Equals(input.Large))
                ) && 
                (
                    Medium == input.Medium ||
                    (Medium != null &&
                    Medium.Equals(input.Medium))
                ) && 
                (
                    Small == input.Small ||
                    (Small != null &&
                    Small.Equals(input.Small))
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
                hashCode = (hashCode * 59) + Large.GetHashCode();
                hashCode = (hashCode * 59) + Medium.GetHashCode();
                hashCode = (hashCode * 59) + Small.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type BadgeUrls
    /// </summary>
    public class BadgeUrlsJsonConverter : JsonConverter<BadgeUrls>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override BadgeUrls Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int currentDepth = reader.CurrentDepth;

            if (reader.TokenType != JsonTokenType.StartObject && reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = reader.TokenType;

            string large = default;
            string medium = default;
            string small = default;

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
                        case "large":
                            large = reader.GetString();
                            break;
                        case "medium":
                            medium = reader.GetString();
                            break;
                        case "small":
                            small = reader.GetString();
                            break;
                    }
                }
            }

            return new BadgeUrls(large, medium, small);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="badgeUrls"></param>
        /// <param name="options"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, BadgeUrls badgeUrls, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("large", badgeUrls.Large);
            writer.WriteString("medium", badgeUrls.Medium);
            writer.WriteString("small", badgeUrls.Small);

            writer.WriteEndObject();
        }
    }
}
