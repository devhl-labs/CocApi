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
    /// Defines WarType
    /// </summary>
    public enum WarType
    {
        /// <summary>
        /// Enum Unknown for value: unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// Enum Random for value: random
        /// </summary>
        Random,

        /// <summary>
        /// Enum Friendly for value: friendly
        /// </summary>
        Friendly,

        /// <summary>
        /// Enum SCCWL for value: sccwl
        /// </summary>
        SCCWL

    }

    public class WarTypeConverter : JsonConverter<WarType>
    {
        public static WarType FromString(string value)
        {
            if (value == "unknown")
                return WarType.Unknown;

            if (value == "random")
                return WarType.Random;

            if (value == "friendly")
                return WarType.Friendly;

            if (value == "sccwl")
                return WarType.SCCWL;

            throw new NotImplementedException($"Could not convert value to type WarType: '{value}'");
        }

        public static WarType? FromStringOrDefault(string value)
        {
            if (value == "unknown")
                return WarType.Unknown;

            if (value == "random")
                return WarType.Random;

            if (value == "friendly")
                return WarType.Friendly;

            if (value == "sccwl")
                return WarType.SCCWL;

            return null;
        }

        public static string ToJsonValue(WarType value)
        {
            if (value == WarType.Unknown)
                return "unknown";

            if (value == WarType.Random)
                return "random";

            if (value == WarType.Friendly)
                return "friendly";

            if (value == WarType.SCCWL)
                return "sccwl";

            throw new NotImplementedException($"Value could not be handled: '{value}'");
        }

        /// <summary>
        /// Returns a  from the Json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override WarType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawValue = reader.GetString();

            WarType? result = WarTypeConverter.FromString(rawValue);
            
            if (result != null)
                return result.Value;

            throw new JsonException();
        }

        /// <summary>
        /// Writes the WarType to the json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warType"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, WarType warType, JsonSerializerOptions options)
        {
            writer.WriteStringValue(warType.ToString());
        }
    }

    public class WarTypeNullableConverter : JsonConverter<WarType?>
    {
        /// <summary>
        /// Returns a WarType from the Json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override WarType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawValue = reader.GetString();

            if (rawValue == null)
                return null;

            WarType? result = WarTypeConverter.FromString(rawValue);

            if (result != null)
                return result.Value;

            throw new JsonException();
        }

        /// <summary>
        /// Writes the DateTime to the json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warType"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, WarType? warType, JsonSerializerOptions options)
        {
            writer.WriteStringValue(warType?.ToString() ?? "null");
        }
    }

}
