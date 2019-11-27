using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Converters
{
    public class RoleConverter : JsonConverter<Role>
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
            writer.WriteStringValue(value.ToEnumMemberAttrValue());
        }
    }
}
