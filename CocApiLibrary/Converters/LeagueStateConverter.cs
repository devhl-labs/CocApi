using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Converters
{
    internal class LeagueStateConverter : JsonConverter<LeagueState>
    {
        public override LeagueState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();

            if (string.IsNullOrEmpty(value)) return LeagueState.NotInWar;

            if (value == "inWar") return LeagueState.InWar;

            if (value == "ended") return LeagueState.WarsEnded;

            if (value == "preparation") return LeagueState.Preparation;

            throw new Exception($"{value} is not a supported role.");      
        }

        public override void Write(Utf8JsonWriter writer, LeagueState value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
