using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace CocApi
{
    public sealed class SuperCellDateConverter : JsonConverter
    {
        public List<string> DateTimeFormats { get; set; } = new List<string>();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is DateTime dte)
                return dte;

            string dateString = (string)reader.Value;

            //string dateString = reader.Value.ToString();

            foreach (string format in DateTimeFormats)
            {
                // adjust this as necessary to fit your needs
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;
            }
            throw new JsonException("Unable to parse \"" + dateString + "\" as a date.");
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime dte = (DateTime)value;

            if (dte.Day == 0 && dte.Minute == 0 && dte.Second == 0 && dte.Millisecond == 0)
                writer.WriteValue(((DateTime)value).ToString("yyyy'-'MM"));
            else
                writer.WriteValue(((DateTime)value).ToString("yyyyMMdd'T'HHmmss.fff'Z'"));
        }
    }
}
