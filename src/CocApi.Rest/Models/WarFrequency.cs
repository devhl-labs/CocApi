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
    /// Defines WarFrequency
    /// </summary>
    public enum WarFrequency
    {
        /// <summary>
        /// Enum Unknown for value: unknown
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// Enum Never for value: never
        /// </summary>
        Never = 2,

        /// <summary>
        /// Enum LessThanOncePerWeek for value: lessThanOncePerWeek
        /// </summary>
        LessThanOncePerWeek = 3,

        /// <summary>
        /// Enum OncePerWeek for value: oncePerWeek
        /// </summary>
        OncePerWeek = 4,

        /// <summary>
        /// Enum MoreThanOncePerWeek for value: moreThanOncePerWeek
        /// </summary>
        MoreThanOncePerWeek = 5,

        /// <summary>
        /// Enum Always for value: always
        /// </summary>
        Always = 6
    }

    public class WarFrequencyConverter : JsonConverter<WarFrequency>
    {
        public static WarFrequency FromString(string value)
        {
            if (value == "unknown")
                return WarFrequency.Unknown;

            if (value == "never")
                return WarFrequency.Never;

            if (value == "lessThanOncePerWeek")
                return WarFrequency.LessThanOncePerWeek;

            if (value == "oncePerWeek")
                return WarFrequency.OncePerWeek;

            if (value == "moreThanOncePerWeek")
                return WarFrequency.MoreThanOncePerWeek;

            if (value == "always")
                return WarFrequency.Always;

            throw new NotImplementedException($"Could not convert value to type WarFrequency: '{value}'");
        }

        public static string ToJsonValue(WarFrequency value)
        {
            if (value == WarFrequency.Unknown)
                return "unknown";

            if (value == WarFrequency.Never)
                return "never";

            if (value == WarFrequency.LessThanOncePerWeek)
                return "lessThanOncePerWeek";

            if (value == WarFrequency.OncePerWeek)
                return "oncePerWeek";

            if (value == WarFrequency.MoreThanOncePerWeek)
                return "moreThanOncePerWeek";

            if (value == WarFrequency.Always)
                return "always";

            throw new NotImplementedException($"Value could not be handled: '{value}'");
        }

        /// <summary>
        /// Returns a  from the Json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override WarFrequency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawValue = reader.GetString();

            WarFrequency? result = WarFrequencyConverter.FromString(rawValue);
            
            if (result != null)
                return result.Value;

            throw new JsonException();
        }

        /// <summary>
        /// Writes the WarFrequency to the json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warFrequency"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, WarFrequency warFrequency, JsonSerializerOptions options)
        {
            writer.WriteStringValue(warFrequency.ToString());
        }
    }

    public class WarFrequencyNullableConverter : JsonConverter<WarFrequency?>
    {
        /// <summary>
        /// Returns a WarFrequency from the Json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override WarFrequency? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawValue = reader.GetString();

            if (rawValue == null)
                return null;

            WarFrequency? result = WarFrequencyConverter.FromString(rawValue);

            if (result != null)
                return result.Value;

            throw new JsonException();
        }

        /// <summary>
        /// Writes the DateTime to the json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warFrequency"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, WarFrequency? warFrequency, JsonSerializerOptions options)
        {
            writer.WriteStringValue(warFrequency?.ToString() ?? "null");
        }
    }

}
