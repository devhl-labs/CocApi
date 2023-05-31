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
    /// ClanWarLeagueClanMember
    /// </summary>
    public partial class ClanWarLeagueClanMember : IEquatable<ClanWarLeagueClanMember?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClanWarLeagueClanMember" /> class.
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="tag">tag</param>
        /// <param name="townHallLevel">townHallLevel</param>
        [JsonConstructor]
        internal ClanWarLeagueClanMember(string name, string tag, int townHallLevel)
        {
            Name = name;
            Tag = tag;
            TownHallLevel = townHallLevel;
            OnCreated();
        }

        partial void OnCreated();

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; }

        /// <summary>
        /// Gets or Sets Tag
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; }

        /// <summary>
        /// Gets or Sets TownHallLevel
        /// </summary>
        [JsonPropertyName("townHallLevel")]
        public int TownHallLevel { get; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ClanWarLeagueClanMember {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Tag: ").Append(Tag).Append("\n");
            sb.Append("  TownHallLevel: ").Append(TownHallLevel).Append("\n");
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
            return this.Equals(input as ClanWarLeagueClanMember);
        }

        /// <summary>
        /// Returns true if ClanWarLeagueClanMember instances are equal
        /// </summary>
        /// <param name="input">Instance of ClanWarLeagueClanMember to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ClanWarLeagueClanMember? input)
        {
            if (input == null)
                return false;

            return 
                (
                    Name == input.Name ||
                    (Name != null &&
                    Name.Equals(input.Name))
                ) && 
                (
                    Tag == input.Tag ||
                    (Tag != null &&
                    Tag.Equals(input.Tag))
                ) && 
                (
                    TownHallLevel == input.TownHallLevel ||
                    TownHallLevel.Equals(input.TownHallLevel)
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
                hashCode = (hashCode * 59) + Name.GetHashCode();
                hashCode = (hashCode * 59) + Tag.GetHashCode();
                hashCode = (hashCode * 59) + TownHallLevel.GetHashCode();

                return hashCode;
            }
        }
    }

    /// <summary>
    /// A Json converter for type ClanWarLeagueClanMember
    /// </summary>
    public class ClanWarLeagueClanMemberJsonConverter : JsonConverter<ClanWarLeagueClanMember>
    {
        /// <summary>
        /// A Json reader.
        /// </summary>
        /// <param name="utf8JsonReader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="JsonException"></exception>
        public override ClanWarLeagueClanMember Read(ref Utf8JsonReader utf8JsonReader, Type typeToConvert, JsonSerializerOptions jsonSerializerOptions)
        {
            int currentDepth = utf8JsonReader.CurrentDepth;

            if (utf8JsonReader.TokenType != JsonTokenType.StartObject && utf8JsonReader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();

            JsonTokenType startingTokenType = utf8JsonReader.TokenType;

            string? name = default;
            string? tag = default;
            int? townHallLevel = default;

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
                        case "name":
                            name = utf8JsonReader.GetString();
                            break;
                        case "tag":
                            tag = utf8JsonReader.GetString();
                            break;
                        case "townHallLevel":
                            if (utf8JsonReader.TokenType != JsonTokenType.Null)
                                townHallLevel = utf8JsonReader.GetInt32();
                            break;
                        default:
                            break;
                    }
                }
            }

            if (tag == null)
                throw new ArgumentNullException(nameof(tag), "Property is required for class ClanWarLeagueClanMember.");

            if (townHallLevel == null)
                throw new ArgumentNullException(nameof(townHallLevel), "Property is required for class ClanWarLeagueClanMember.");

            if (name == null)
                throw new ArgumentNullException(nameof(name), "Property is required for class ClanWarLeagueClanMember.");

            return new ClanWarLeagueClanMember(name, tag, townHallLevel.Value);
        }

        /// <summary>
        /// A Json writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="clanWarLeagueClanMember"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void Write(Utf8JsonWriter writer, ClanWarLeagueClanMember clanWarLeagueClanMember, JsonSerializerOptions jsonSerializerOptions)
        {
            writer.WriteStartObject();

            writer.WriteString("name", clanWarLeagueClanMember.Name);
            writer.WriteString("tag", clanWarLeagueClanMember.Tag);
            writer.WriteNumber("townHallLevel", clanWarLeagueClanMember.TownHallLevel);

            writer.WriteEndObject();
        }
    }
}
