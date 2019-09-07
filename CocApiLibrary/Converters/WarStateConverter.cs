using CocApiLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Converters
{
    internal class WarStateConverter : JsonConverter<WarState>
    {
        public override WarState Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string value = reader.GetString();

            if (string.IsNullOrEmpty(value)) return WarState.NotInWar;

            if (value == "inWar") return WarState.InWar;

            if (value == "notInWar") return WarState.NotInWar;

            if (value == "preparation") return WarState.Preparation;

            if (value == "warEnded") return WarState.WarEnded;

            throw new Exception($"{value} is not a supported role.");      
        }

        public override void Write(Utf8JsonWriter writer, WarState value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
