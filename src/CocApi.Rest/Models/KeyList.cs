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
    /// KeyList
    /// </summary>
    public partial class KeyList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyList" /> class.
        /// </summary>
        /// <param name="keys">keys</param>
        /// <param name="sessionExpiresInSeconds">sessionExpiresInSeconds</param>
        /// <param name="status">status</param>
        [JsonConstructor]
        public KeyList(List<Key> keys, int sessionExpiresInSeconds, KeyListStatus status)
        {
            Keys = keys;
            SessionExpiresInSeconds = sessionExpiresInSeconds;
            Status = status;
        }

        /// <summary>
        /// Gets or Sets Keys
        /// </summary>
        [JsonPropertyName("keys")]
        public List<Key> Keys { get; set; }

        /// <summary>
        /// Gets or Sets SessionExpiresInSeconds
        /// </summary>
        [JsonPropertyName("sessionExpiresInSeconds")]
        public int SessionExpiresInSeconds { get; set; }

        /// <summary>
        /// Gets or Sets Status
        /// </summary>
        [JsonPropertyName("status")]
        public KeyListStatus Status { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class KeyList {\n");
            sb.Append("  Keys: ").Append(Keys).Append("\n");
            sb.Append("  SessionExpiresInSeconds: ").Append(SessionExpiresInSeconds).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A Json converter for type KeyList
    /// </summary>
    public class KeyListJsonConverter : JsonConverter<KeyList>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override KeyList Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            List<Key> keys = default;
            int sessionExpiresInSeconds = default;
            KeyListStatus status = default;

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
                        case "keys":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                keys = JsonSerializer.Deserialize<List<Key>>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        case "sessionExpiresInSeconds":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                sessionExpiresInSeconds = utf8JsonReader.GetInt32();
                            break;
                        case "status":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                status = JsonSerializer.Deserialize<KeyListStatus>(ref utf8JsonReader, jsonSerializerOptions);
                            break;
                        default:
                            break;
                    }
                }
            }

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (status == null)
                throw new ArgumentNullException(nameof(status), "Property is required for class KeyList.");

            if (sessionExpiresInSeconds == null)
                throw new ArgumentNullException(nameof(sessionExpiresInSeconds), "Property is required for class KeyList.");

            if (keys == null)
                throw new ArgumentNullException(nameof(keys), "Property is required for class KeyList.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return new KeyList(keys, sessionExpiresInSeconds, status);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="keyList"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, KeyList keyList, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("keys");
            JsonSerializer.Serialize(writer, keyList.Keys, jsonSerializerOptions);
            writer.WriteNumber("sessionExpiresInSeconds", keyList.SessionExpiresInSeconds);
            writer.WritePropertyName("status");
            JsonSerializer.Serialize(writer, keyList.Status, jsonSerializerOptions);

            writer.WriteEndObject();
        }
    }
}
