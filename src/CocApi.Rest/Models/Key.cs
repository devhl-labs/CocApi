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
    /// Key
    /// </summary>
    public partial class Key
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Key" /> class.
        /// </summary>
        /// <param name="cidrRanges">cidrRanges</param>
        /// <param name="description">description</param>
        /// <param name="developerId">developerId</param>
        /// <param name="id">id</param>
        /// <param name="keyProperty">keyProperty</param>
        /// <param name="name">name</param>
        /// <param name="scopes">scopes</param>
        /// <param name="tier">tier</param>
        /// <param name="origins">origins</param>
        /// <param name="validUntil">validUntil</param>
        [JsonConstructor]
        public Key(List<string> cidrRanges, string description, string developerId, string id, string keyProperty, string name, List<string> scopes, string tier, string? origins = default, DateTime? validUntil = default)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (id == null)
                throw new ArgumentNullException("id is a required property for Key and cannot be null.");

            if (name == null)
                throw new ArgumentNullException("name is a required property for Key and cannot be null.");

            if (keyProperty == null)
                throw new ArgumentNullException("keyProperty is a required property for Key and cannot be null.");

            if (developerId == null)
                throw new ArgumentNullException("developerId is a required property for Key and cannot be null.");

            if (tier == null)
                throw new ArgumentNullException("tier is a required property for Key and cannot be null.");

            if (description == null)
                throw new ArgumentNullException("description is a required property for Key and cannot be null.");

            if (scopes == null)
                throw new ArgumentNullException("scopes is a required property for Key and cannot be null.");

            if (cidrRanges == null)
                throw new ArgumentNullException("cidrRanges is a required property for Key and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            CidrRanges = cidrRanges;
            Description = description;
            DeveloperId = developerId;
            Id = id;
            KeyProperty = keyProperty;
            Name = name;
            Scopes = scopes;
            Tier = tier;
            Origins = origins;
            ValidUntil = validUntil;
        }

        /// <summary>
        /// Gets or Sets CidrRanges
        /// </summary>
        [JsonPropertyName("cidrRanges")]
        public List<string> CidrRanges { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets DeveloperId
        /// </summary>
        [JsonPropertyName("developerId")]
        public string DeveloperId { get; set; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets KeyProperty
        /// </summary>
        [JsonPropertyName("key")]
        public string KeyProperty { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Scopes
        /// </summary>
        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; }

        /// <summary>
        /// Gets or Sets Tier
        /// </summary>
        [JsonPropertyName("tier")]
        public string Tier { get; set; }

        /// <summary>
        /// Gets or Sets Origins
        /// </summary>
        [JsonPropertyName("origins")]
        public string? Origins { get; set; }

        /// <summary>
        /// Gets or Sets ValidUntil
        /// </summary>
        [JsonPropertyName("validUntil")]
        public DateTime? ValidUntil { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Key {\n");
            sb.Append("  CidrRanges: ").Append(CidrRanges).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  DeveloperId: ").Append(DeveloperId).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  KeyProperty: ").Append(KeyProperty).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Scopes: ").Append(Scopes).Append("\n");
            sb.Append("  Tier: ").Append(Tier).Append("\n");
            sb.Append("  Origins: ").Append(Origins).Append("\n");
            sb.Append("  ValidUntil: ").Append(ValidUntil).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A Json converter for type Key
    /// </summary>
    public class KeyJsonConverter : JsonConverter<Key>
    {
        /// <summary>
        /// The format to use to serialize ValidUntil
        /// </summary>
        public static string ValidUntilFormat { get; set; } = "yyyyMMdd'T'HHmmss.fff'Z'";

        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override Key Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            List<string> cidrRanges = default;
            string description = default;
            string developerId = default;
            string id = default;
            string keyProperty = default;
            string name = default;
            List<string> scopes = default;
            string tier = default;
            string origins = default;
            DateTime? validUntil = default;

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
                        case "cidrRanges":
                            cidrRanges = JsonSerializer.Deserialize<List<string>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "description":
                            description = utf8JsonReader.GetString();
                            break;
                        case "developerId":
                            developerId = utf8JsonReader.GetString();
                            break;
                        case "id":
                            id = utf8JsonReader.GetString();
                            break;
                        case "key":
                            keyProperty = utf8JsonReader.GetString();
                            break;
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        case "scopes":
                            scopes = JsonSerializer.Deserialize<List<string>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "tier":
                            tier = utf8JsonReader.GetString();
                            break;
                        case "origins":
                            origins = utf8JsonReader.GetString();
                            break;
                        case "validUntil":
                            validUntil = JsonSerializer.Deserialize<DateTime?>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        default:
                            break;
                    }
                }
            }

            return new Key(cidrRanges, description, developerId, id, keyProperty, name, scopes, tier, origins, validUntil);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="key"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, Key key, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("cidrRanges");
            JsonSerializer.Serialize(writer, key.CidrRanges, jsonSerializerOptions);
            writer.WriteString("description", key.Description);
            writer.WriteString("developerId", key.DeveloperId);
            writer.WriteString("id", key.Id);
            writer.WriteString("key", key.KeyProperty);
            writer.WriteString("name", key.Name);
            writer.WritePropertyName("scopes");
            JsonSerializer.Serialize(writer, key.Scopes, jsonSerializerOptions);
            writer.WriteString("tier", key.Tier);
            writer.WriteString("origins", key.Origins);
            if (key.ValidUntil != null)
                writer.WriteString("validUntil", key.ValidUntil.Value.ToString(ValidUntilFormat));
            else
                writer.WriteNull("validUntil");

            writer.WriteEndObject();
        }
    }
}
