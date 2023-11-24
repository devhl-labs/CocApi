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
    /// LoginResponse
    /// </summary>
    public partial class LoginResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResponse" /> class.
        /// </summary>
        /// <param name="sessionExpiresInSeconds">sessionExpiresInSeconds</param>
        /// <param name="temporaryAPIToken">temporaryAPIToken</param>
        [JsonConstructor]
        public LoginResponse(int sessionExpiresInSeconds, string temporaryAPIToken)
        {
            SessionExpiresInSeconds = sessionExpiresInSeconds;
            TemporaryAPIToken = temporaryAPIToken;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets SessionExpiresInSeconds
        /// </summary>
        [JsonPropertyName("sessionExpiresInSeconds")]
        public int SessionExpiresInSeconds { get; set; }

        /// <summary>
        /// Gets or Sets TemporaryAPIToken
        /// </summary>
        [JsonPropertyName("temporaryAPIToken")]
        public string TemporaryAPIToken { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LoginResponse {\n");
            sb.Append("  SessionExpiresInSeconds: ").Append(SessionExpiresInSeconds).Append("\n");
            sb.Append("  TemporaryAPIToken: ").Append(TemporaryAPIToken).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A Json converter for type <see cref="LoginResponse" />
    /// </summary>
    public class LoginResponseJsonConverter : JsonConverter<LoginResponse>
    {
        /// <summary>
        /// Deserializes json to <see cref="LoginResponse" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override LoginResponse Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<int?> sessionExpiresInSeconds = default;
            Option<string?> temporaryAPIToken = default;

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
                        case "sessionExpiresInSeconds":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                sessionExpiresInSeconds = new Option<int?>(utf8JsonReader.GetInt32());
                            break;
                        case "temporaryAPIToken":
                            temporaryAPIToken = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!sessionExpiresInSeconds.IsSet)
                throw new ArgumentException("Property is required for class LoginResponse.", nameof(sessionExpiresInSeconds));

            if (!temporaryAPIToken.IsSet)
                throw new ArgumentException("Property is required for class LoginResponse.", nameof(temporaryAPIToken));

            if (sessionExpiresInSeconds.IsSet && sessionExpiresInSeconds.Value == null)
                throw new ArgumentNullException(nameof(sessionExpiresInSeconds), "Property is not nullable for class LoginResponse.");

            if (temporaryAPIToken.IsSet && temporaryAPIToken.Value == null)
                throw new ArgumentNullException(nameof(temporaryAPIToken), "Property is not nullable for class LoginResponse.");

            return new LoginResponse(sessionExpiresInSeconds.Value!.Value!, temporaryAPIToken.Value!);
        }

        /// <summary>
        /// Serializes a <see cref="LoginResponse" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="loginResponse"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, LoginResponse loginResponse, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(ref writer, loginResponse, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="LoginResponse" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="loginResponse"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(ref Utf8JsonWriter writer, LoginResponse loginResponse, JsonSerializerOptions jsonSerializerOptions)
        {
            if (loginResponse.TemporaryAPIToken == null)
                throw new ArgumentNullException(nameof(loginResponse.TemporaryAPIToken), "Property is required for class LoginResponse.");

            writer.WriteNumber("sessionExpiresInSeconds", loginResponse.SessionExpiresInSeconds);

            writer.WriteString("temporaryAPIToken", loginResponse.TemporaryAPIToken);
        }
    }
}
