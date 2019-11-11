using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Converters
{
    internal class ResultConverter : JsonConverter<Result>
    {
        public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string result = reader.GetString();

            if (string.IsNullOrEmpty(result)) return Result.Undetermined;

            result = result.ToLower();

            if (result == "win") return Result.Win;

            if (result == "lose") return Result.Lose;

            if (result == "draw") return Result.Draw;

            if (result == "undetermined") return Result.Undetermined;

            throw new Exception($"{result} is not a supported role.");      
        }

        public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
