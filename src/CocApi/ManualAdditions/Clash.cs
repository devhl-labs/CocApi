using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CocApi
{
    public static class Clash
    {
        public static class Heroes
        {
            public const string BK = "Barbarian King";
            public const string AQ = "Archer Queen";
            public const string GW = "Grand Warden";
            public const string RC = "Royal Champion";
            public const string BM = "Battle Machine";
        }

        public const int MAX_TOWN_HALL_LEVEL = 13;

        public const int MAX_BUILD_BASE_LEVEL = 8;

        public static Regex TagRegEx { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public static bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return false;

            if (tag == "#0")
                return false;

            if (tag.Length < 4)
                return false;

            return TagRegEx.IsMatch(tag);
        }

        public static bool TryGetValidTag(string userInput, out string formattedTag)
        {
            formattedTag = string.Empty;

            if (string.IsNullOrEmpty(userInput))
                return false;

            if (userInput.StartsWith("\"") && userInput.EndsWith("\"") && userInput.Length > 2)
                userInput = userInput[1..^1];

            else if (userInput.StartsWith("`") && userInput.EndsWith("`") && userInput.Length > 2)
                userInput = userInput[1..^1];

            else if (userInput.StartsWith("'") && userInput.EndsWith("'") && userInput.Length > 2)
                userInput = userInput[1..^1];

            formattedTag = userInput.ToUpper();

            formattedTag = formattedTag.Replace("O", "0");

            if (!formattedTag.StartsWith("#"))
                formattedTag = $"#{formattedTag}";

            var result = IsValidTag(formattedTag);

            if (!result)
                formattedTag = string.Empty;

            return result;
        }

        public static bool IsCwlEnabled()
        {
            int day = DateTime.UtcNow.Day;

            if (day > 0 && day < 11)            
                return true;          

            //add three hours to the end to ensure we get everything
            if (day == 11 && DateTime.UtcNow.Hour < 3)            
                return true;                      

            return false;
        }

        public static List<JsonConverter> JsonConverters()
        {
            List<JsonConverter> results = new List<JsonConverter>
            {
                new SuperCellDateConverter { DateTimeFormats = new List<string> { "yyyyMMdd'T'HHmmss.fff'Z'", "yyyy'-'MM" } }
            };

            return results;
        }

        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            // OpenAPI generated types generally hide default constructors.
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    OverrideSpecifiedNames = true
                }
            },
            Converters = JsonConverters()
        };
    }
}
