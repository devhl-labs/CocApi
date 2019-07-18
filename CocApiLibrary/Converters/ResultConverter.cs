using CocApiStandardLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CocApiStandardLibrary.Enums;

namespace CocApiLibrary.Converters
{
    internal class ResultConverter : JsonConverter<Result>
    {
        public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string result = reader.GetString();

            if (string.IsNullOrEmpty(result)) return Result.undetermined;

            result = result.ToLower();

            if (result == "win") return Result.win;

            if (result == "lose") return Result.lose;

            if (result == "draw") return Result.draw;

            if (result == "undetermined") return Result.undetermined;

            throw new Exception($"{result} is not a supported role.");      
        }

        public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
