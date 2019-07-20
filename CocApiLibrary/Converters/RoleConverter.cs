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

            if (role == "leader") return Role.leader;

            if (role == "coleader") return Role.coleader;

            if (role == "admin") return Role.elder;

            if (role == "member") return Role.member;

            throw new Exception($"{role} is not a supported role.");      
        }

        public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
