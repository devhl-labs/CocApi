using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Converters
{
    internal class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString().ToDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {

            //DateTime result = DateTime.ParseExact(str, "yyyyMMdd'T'HHmmss.fff'Z'", null);

            //result = DateTime.SpecifyKind(result, DateTimeKind.Utc);

            //return result;

            //string result = $"{value.Year.ToString()}{value.Month.";

            string str = value.ToString("yyyyMMdd'T'HHmmss.fff'Z'");

            Console.WriteLine(str);

            writer.WriteStringValue(value.ToString("yyyyMMdd'T'HHmmss.fff'Z'"));


            throw new NotImplementedException();


        }
    }
}
