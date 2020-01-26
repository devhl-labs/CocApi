using System;
//using System.Text.Json;

using Newtonsoft.Json;

//using static devhl.CocApi.Enums;

namespace devhl.CocApi.Converters
{
    //internal class ResultConverter : JsonConverter<Result>
    //{
    //    public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        string result = reader.GetString();

    //        if (string.IsNullOrEmpty(result)) return Result.Undetermined;

    //        result = result.ToLower();

    //        if (result == "win") return Result.Win;

    //        if (result == "lose") return Result.Lose;

    //        if (result == "draw") return Result.Draw;

    //        if (result == "undetermined") return Result.Undetermined;

    //        throw new Exception($"{result} is not a supported role.");
    //    }

    //    public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
    //    {
    //        writer.WriteStringValue(value.ToEnumMemberAttrValue());
    //    }
    //}

    //internal class ResultConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        if (objectType == typeof(Result)) return true;

    //        return false;
    //    }

    //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.Value == null) return Result.Unknown;

    //        string result = reader.Value.ToString();

    //        if (string.IsNullOrEmpty(result)) return Result.Undetermined;

    //        result = result.ToLower();

    //        if (result == "win") return Result.Win;

    //        if (result == "lose") return Result.Lose;

    //        if (result == "draw") return Result.Draw;

    //        if (result == "undetermined") return Result.Undetermined;

    //        throw new Exception($"{result} is not a supported role.");
    //    }

    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteNull();
    //        }
    //        else
    //        {
    //            writer.WriteValue(((Result) value).ToEnumMemberAttrValue());
    //        }
    //    }
    //}

}
