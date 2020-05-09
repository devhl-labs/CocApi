using System.Text.RegularExpressions;

namespace devhl.CocApi
{
    public sealed partial class CocApi
    {
        public static class Heroes
        {
            public const string BK = "Barbarian King";
            public const string AQ = "Archer Queen";
            public const string GW = "Grand Warden";
            public const string RC = "Royal Champion";
            public const string BM = "Battle Machine";
        }

        public const int MaxTownHallLevel = 13;

        public const int MaxBuildBaseLevel = 8;

        public static Regex ValidTagCharacters { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public static bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            if (tag == "#0")
            {
                return false;
            }

            return ValidTagCharacters.IsMatch(tag);
        }

        public static bool TryGetValidTag(string userInput, out string formattedTag)
        {
            formattedTag = string.Empty;

            if (string.IsNullOrEmpty(userInput))
                return false;

            formattedTag = userInput.ToUpper();

            formattedTag = formattedTag.Replace("O", "0");

            if (!formattedTag.StartsWith("#"))
                formattedTag = $"#{formattedTag}";

            var result = IsValidTag(formattedTag);

            if (!result)
                formattedTag = string.Empty;

            return result;
        }
    }
}
