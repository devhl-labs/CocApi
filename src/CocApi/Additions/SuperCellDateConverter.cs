using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;

namespace CocApi
{
    public class SuperCellDateConverter : JsonConverter
    {
        public List<string> DateTimeFormats { get; set; }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string dateString = (string)reader.Value;

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
            throw new NotImplementedException();
        }
    }
}
