using System;
//using System.Text.Json;

using Newtonsoft.Json;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Converters
{
    //internal class LeagueStateConverter : JsonConverter<LeagueState>
    //{
    //    public override LeagueState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        string value = reader.GetString();

    //        if (string.IsNullOrEmpty(value)) return LeagueState.NotInWar;

    //        if (value == "inWar") return LeagueState.InWar;

    //        if (value == "ended") return LeagueState.WarsEnded;

    //        if (value == "preparation") return LeagueState.Preparation;

    //        throw new Exception($"{value} is not a supported role.");
    //    }

    //    public override void Write(Utf8JsonWriter writer, LeagueState value, JsonSerializerOptions options)
    //    {
    //        writer.WriteStringValue(value.ToEnumMemberAttrValue());
    //    }
    //}

    //internal class LeagueStateConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        if (objectType == typeof(LeagueState)) return true;

    //        return false;
    //    }

    //    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        if (reader.Value == null) return LeagueState.Unknown;

    //        string value = reader.Value.ToString();

    //        if (string.IsNullOrEmpty(value)) return LeagueState.NotInWar;

    //        if (value == "inWar") return LeagueState.InWar;

    //        if (value == "ended") return LeagueState.WarsEnded;

    //        if (value == "preparation") return LeagueState.Preparation;

    //        throw new Exception($"{value} is not a supported role.");
    //    }

    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteNull();
    //        }
    //        else
    //        {
    //            writer.WriteValue(((LeagueState) value).ToEnumMemberAttrValue());
    //        }
    //    }
    //}
}
