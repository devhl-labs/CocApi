using System;
//using System.Text.Json;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Converters
{
    //internal class DateTimeConverter : JsonConverter<DateTime>
    //{
    //    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        return reader.GetString().ToDateTime();
    //    }

    //    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    //    {
    //        writer.WriteStringValue(value.ToString("yyyyMMdd'T'HHmmss.fff'Z'"));
    //    }
    //}



    internal class DateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateTime)) return true;

            return false;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return reader.Value?.ToString().ToDateTime();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                DateTime date = (DateTime) value;

                writer.WriteValue(date.ToString("yyyyMMdd'T'HHmmss.fff'Z'"));
            }
        }
    }
}
