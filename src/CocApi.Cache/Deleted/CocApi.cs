//using System.Text.RegularExpressions;

//namespace CocApi
//{
//    public static class CocApi
//    {
//        public static class Heroes
//        {
//            public const string BK = "Barbarian King";
//            public const string AQ = "Archer Queen";
//            public const string GW = "Grand Warden";
//            public const string RC = "Royal Champion";
//            public const string BM = "Battle Machine";
//        }

//        public const int MAX_TOWN_HALL_LEVEL = 13;

//        public const int MAX_BUILD_BASE_LEVEL = 8;

//        public static Regex TagRegEx { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

//        public static bool IsValidTag(string tag)
//        {
//            if (string.IsNullOrEmpty(tag))
//                return false;

//            if (tag == "#0")
//                return false;

//            if (tag.Length < 4)
//                return false;

//            return TagRegEx.IsMatch(tag);
//        }

//        public static bool TryGetValidTag(string userInput, out string formattedTag)
//        {
//            formattedTag = string.Empty;

//            if (string.IsNullOrEmpty(userInput))
//                return false;

//            if (userInput.StartsWith("\"") && userInput.EndsWith("\"") && userInput.Length > 2)
//                userInput = userInput[1..^1];

//            else if (userInput.StartsWith("`") && userInput.EndsWith("`") && userInput.Length > 2)
//                userInput = userInput[1..^1];

//            else if (userInput.StartsWith("'") && userInput.EndsWith("'") && userInput.Length > 2)
//                userInput = userInput[1..^1];

//            formattedTag = userInput.ToUpper();

//            formattedTag = formattedTag.Replace("O", "0");

//            if (!formattedTag.StartsWith("#"))
//                formattedTag = $"#{formattedTag}";

//            var result = IsValidTag(formattedTag);

//            if (!result)
//                formattedTag = string.Empty;

//            return result;
//        }
//    }
//}
