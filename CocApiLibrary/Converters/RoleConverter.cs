using CocApiLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Converters
{
    internal class RoleConverter : JsonConverter<Role>
    {
        public override Role Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string role = reader.GetString().ToLower();

            if (string.IsNullOrEmpty(role)) return Role.Unknown;

            if (role == "leader") return Role.Leader;

            if (role == "coleader") return Role.Coleader;

            if (role == "admin") return Role.Elder;

            if (role == "member") return Role.Member;

            throw new Exception($"{role} is not a supported role.");      
        }

        public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
