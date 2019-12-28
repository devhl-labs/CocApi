using System;
using System.Linq;
//using System.Text.Json;
////System.Text.Json.Serialization
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static devhl.CocApi.Enums;

namespace devhl.CocApi.Converters
{
    //public class RoleConverter : JsonConverter<Role>
    //{
    //    public override Role Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        string role = reader.GetString().ToLower();

    //        if (string.IsNullOrEmpty(role)) return Role.Unknown;

    //        if (role == "leader") return Role.Leader;

    //        if (role == "coleader") return Role.Coleader;

    //        if (role == "admin") return Role.Elder;

    //        if (role == "member") return Role.Member;

    //        throw new Exception($"{role} is not a supported role.");
    //    }

    //    public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options)
    //    {
    //        writer.WriteStringValue(value.ToEnumMemberAttrValue());
    //    }
    //}

    //internal class RoleConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        if (objectType == typeof(Role)) return true;

    //        return false;
    //    }

    //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.Value == null) { return Role.Unknown; }

    //        string role = reader.Value.ToString().ToLower();

    //        if (string.IsNullOrEmpty(role)) return Role.Unknown;

    //        if (role == "leader") return Role.Leader;

    //        if (role == "coleader") return Role.Coleader;

    //        if (role == "admin") return Role.Elder;

    //        if (role == "member") return Role.Member;

    //        throw new Exception($"{role} is not a supported role.");
    //    }

    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteNull();
    //        }
    //        else
    //        {
    //            var a = ((Role) value).ToEnumMemberAttrValue();


    //            writer.WriteValue(((Role) value).ToEnumMemberAttrValue());
    //        }

    //    }
    //}

    internal class RoleConverter : StringEnumConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Role));
        }

        //public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        //{
        //    return base.ReadJson(reader, objectType, existingValue, serializer);
        //}

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var a = ((Role) value).ToEnumMemberAttrValue();


                writer.WriteValue(((Role) value).ToEnumMemberAttrValue());
            }
        }
    }
}
