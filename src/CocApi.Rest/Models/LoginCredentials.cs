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
    /// LoginCredentials
    /// </summary>
    public partial class LoginCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginCredentials" /> class.
        /// </summary>
        /// <param name="email">email</param>
        /// <param name="password">password</param>
        [JsonConstructor]
        public LoginCredentials(string email, string password)
        {
            Email = email;
            Password = password;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets Email
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or Sets Password
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class LoginCredentials {\n");
            sb.Append("  Email: ").Append(Email).Append("\n");
            sb.Append("  Password: ").Append(Password).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    /// <summary>
    /// A Json converter for type <see cref="LoginCredentials" />
    /// </summary>
    public class LoginCredentialsJsonConverter : JsonConverter<LoginCredentials>
    {
        /// <summary>
        /// Deserializes json to <see cref="LoginCredentials" />
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override LoginCredentials Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            Option<string?> email = default;
            Option<string?> password = default;

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
                        case "email":
                            email = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        case "password":
                            password = new Option<string?>(utf8JsonReader.GetString()!);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (!email.IsSet)
                throw new ArgumentException("Property is required for class LoginCredentials.", nameof(email));

            if (!password.IsSet)
                throw new ArgumentException("Property is required for class LoginCredentials.", nameof(password));

            if (email.IsSet && email.Value == null)
                throw new ArgumentNullException(nameof(email), "Property is not nullable for class LoginCredentials.");

            if (password.IsSet && password.Value == null)
                throw new ArgumentNullException(nameof(password), "Property is not nullable for class LoginCredentials.");

            return new LoginCredentials(email.Value!, password.Value!);
        }

        /// <summary>
        /// Serializes a <see cref="LoginCredentials" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="loginCredentials"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, LoginCredentials loginCredentials, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            WriteProperties(writer, loginCredentials, jsonSerializerOptions);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Serializes the properties of <see cref="LoginCredentials" />
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="loginCredentials"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void WriteProperties(Utf8JsonWriter writer, LoginCredentials loginCredentials, JsonSerializerOptions jsonSerializerOptions)
        {
            if (loginCredentials.Email == null)
                throw new ArgumentNullException(nameof(loginCredentials.Email), "Property is required for class LoginCredentials.");

            if (loginCredentials.Password == null)
                throw new ArgumentNullException(nameof(loginCredentials.Password), "Property is required for class LoginCredentials.");

            writer.WriteString("email", loginCredentials.Email);

            writer.WriteString("password", loginCredentials.Password);
        }
    }
}
