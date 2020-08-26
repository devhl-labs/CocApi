//using System;
////using System.Text.Json;

//using Newtonsoft.Json;

//namespace CocApi.Cache.Converters
//{
//    //internal class WarStateConverter : JsonConverter<WarState>
//    //{
//    //    public override WarState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    //    {
//    //        string value = reader.GetString();

//    //        if (string.IsNullOrEmpty(value)) return WarState.Unknown;

//    //        if (value == "inWar") return WarState.InWar;

//    //        if (value == "notInWar") return WarState.NotInWar;

//    //        if (value == "preparation") return WarState.Preparation;

//    //        if (value == "warEnded") return WarState.WarEnded;

//    //        throw new Exception($"{value} is not a supported role.");
//    //    }

//    //    public override void Write(Utf8JsonWriter writer, WarState value, JsonSerializerOptions options)
//    //    {
//    //        writer.WriteStringValue(value.ToEnumMemberAttrValue());
//    //    }
//    //}

//    //internal class WarStateConverter : JsonConverter
//    //{
//    //    public override bool CanConvert(Type objectType)
//    //    {
//    //        if (objectType == typeof(WarState)) return true;

//    //        return false;
//    //    }

//    //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
//    //    {
//    //        if (reader.Value == null) return WarState.Unknown;

//    //        string value = reader.Value.ToString();

//    //        if (string.IsNullOrEmpty(value)) return WarState.Unknown;

//    //        if (value == "inWar") return WarState.InWar;

//    //        if (value == "notInWar") return WarState.NotInWar;

//    //        if (value == "preparation") return WarState.Preparation;

//    //        if (value == "warEnded") return WarState.WarEnded;

//    //        throw new Exception($"{value} is not a supported role.");
//    //    }

//    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
//    //    {
//    //        if (value == null)
//    //        {
//    //            writer.WriteNull();
//    //        }
//    //        else
//    //        {
//    //            writer.WriteValue(((WarState) value).ToEnumMemberAttrValue());
//    //        }
//    //    }
//    //}
//}
