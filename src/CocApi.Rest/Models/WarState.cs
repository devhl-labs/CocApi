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
    /// Defines WarState
    /// </summary>
    public enum WarState
    {
        /// <summary>
        /// Enum NotInWar for value: notInWar
        /// </summary>
        NotInWar = 1,

        /// <summary>
        /// Enum Preparation for value: preparation
        /// </summary>
        Preparation = 2,

        /// <summary>
        /// Enum InWar for value: inWar
        /// </summary>
        InWar = 3,

        /// <summary>
        /// Enum WarEnded for value: warEnded
        /// </summary>
        WarEnded = 4
    }

    /// <summary>
    /// Converts <see cref="WarState"/> to and from the JSON value
    /// </summary>
    public static class WarStateValueConverter
    {
        /// <summary>
        /// Parses a given value to <see cref="WarState"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static WarState FromString(string value)
        {
            if (value.Equals("notInWar"))
                return WarState.NotInWar;

            if (value.Equals("preparation"))
                return WarState.Preparation;

            if (value.Equals("inWar"))
                return WarState.InWar;

            if (value.Equals("warEnded"))
                return WarState.WarEnded;

            throw new NotImplementedException($"Could not convert value to type WarState: '{value}'");
        }

        /// <summary>
        /// Parses a given value to <see cref="WarState"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static WarState? FromStringOrDefault(string value)
        {
            if (value.Equals("notInWar"))
                return WarState.NotInWar;

            if (value.Equals("preparation"))
                return WarState.Preparation;

            if (value.Equals("inWar"))
                return WarState.InWar;

            if (value.Equals("warEnded"))
                return WarState.WarEnded;

            return null;
        }

        /// <summary>
        /// Converts the <see cref="WarState"/> to the json value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static string ToJsonValue(WarState value)
        {
            if (value == WarState.NotInWar)
                return "notInWar";

            if (value == WarState.Preparation)
                return "preparation";

            if (value == WarState.InWar)
                return "inWar";

            if (value == WarState.WarEnded)
                return "warEnded";

            throw new NotImplementedException($"Value could not be handled: '{value}'");
        }
    }

    /// <summary>
    /// A Json converter for type <see cref="WarState"/>
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public class WarStateJsonConverter : JsonConverter<WarState>
    {
        /// <summary>
        /// Returns a  from the Json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override WarState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawValue = reader.GetString();

            WarState? result = rawValue == null
                ? null
                : WarStateValueConverter.FromStringOrDefault(rawValue);

            if (result != null)
                return result.Value;

            throw new JsonException();
        }

        /// <summary>
        /// Writes the WarState to the json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warState"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, WarState warState, JsonSerializerOptions options)
        {
            writer.WriteStringValue(warState.ToString());
        }
    }

    /// <summary>
    /// A Json converter for type <see cref="WarState"/>
    /// </summary>
    public class WarStateNullableJsonConverter : JsonConverter<WarState?>
    {
        /// <summary>
        /// Returns a WarState from the Json object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override WarState? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? rawValue = reader.GetString();

            WarState? result = rawValue == null
                ? null
                : WarStateValueConverter.FromStringOrDefault(rawValue);

            if (result != null)
                return result.Value;

            throw new JsonException();
        }

        /// <summary>
        /// Writes the DateTime to the json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="warState"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, WarState? warState, JsonSerializerOptions options)
        {
            writer.WriteStringValue(warState?.ToString() ?? "null");
        }
    }
}
