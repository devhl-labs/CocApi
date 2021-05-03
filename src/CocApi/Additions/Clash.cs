using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CocApi
{
    public static class Clash
    {
        public static Unit BK = new Unit("Barbarian King", 80, Village.Home, Resource.DarkElixir);
        public static Unit AQ = new Unit("Archer Queen", 80, Village.Home, Resource.DarkElixir);
        public static Unit GW = new Unit("Grand Warden", 40, Village.Home, Resource.Elixir);
        public static Unit RC = new Unit("Royal Champion", 30, Village.Home, Resource.DarkElixir);
        public static Unit BM = new Unit("Battle Machine", 30, Village.BuilderBase, Resource.Elixir);

        public static Unit[] Heroes { get; } = new Unit[]
        {
            BK,
            AQ,
            GW,
            RC,
            BM
        };

        public enum Village
        {
            Home,
            BuilderBase
        }

        public enum Resource
        {
            Gold,
            Elixir,
            DarkElixir
        }

        public static Unit[] Pets { get; } = new Unit[]
        {
            new Unit("L.A.S.S.I", 10, Village.Home, Resource.DarkElixir),
            new Unit("Electro Owl", 10, Village.Home, Resource.DarkElixir),
            new Unit("Mighty Yak", 10, Village.Home, Resource.DarkElixir),
            new Unit("Unicorn", 10, Village.Home, Resource.DarkElixir)
        };

        public static Unit[] Troops { get; } = new Unit[]
        {
            new Unit("Barbarian", 10, Village.Home, Resource.Elixir, "Super Barbarian"),
            new Unit("Archer", 10, Village.Home, Resource.Elixir, "Super Archer"),
            new Unit("Giant", 10, Village.Home, Resource.Elixir, "Super Giant"),
            new Unit("Goblin", 8, Village.Home, Resource.Elixir, "Sneaky Goblin"),
            new Unit("Wall Breaker", 10, Village.Home, Resource.Elixir, "Super Wall Breaker"),
            new Unit("Balloon", 9, Village.Home, Resource.Elixir),
            new Unit("Wizard", 10, Village.Home, Resource.Elixir, "Super Wizard"),
            new Unit("Healer", 7, Village.Home, Resource.Elixir),
            new Unit("Dragon", 8, Village.Home, Resource.Elixir),
            new Unit("P.E.K.K.A", 9, Village.Home, Resource.Elixir),
            new Unit("Baby Dragon", 8, Village.Home, Resource.Elixir, "Inferno Dragon"),
            new Unit("Miner", 7, Village.Home, Resource.Elixir),
            new Unit("Eletro Dragon", 4, Village.Home, Resource.Elixir),
            new Unit("Yeti", 3, Village.Home, Resource.Elixir),
            new Unit("Minion", 10, Village.Home, Resource.DarkElixir, "Super Minion"),
            new Unit("Hog Rider", 10, Village.Home, Resource.DarkElixir),
            new Unit("Valkyrie", 9, Village.Home, Resource.DarkElixir, "Super Valkyrie"),
            new Unit("Golem", 10, Village.Home, Resource.DarkElixir),
            new Unit("Witch", 5, Village.Home, Resource.DarkElixir, "Super Witch"),
            new Unit("Lava Hound", 6, Village.Home, Resource.DarkElixir, "Ice Hound"),
            new Unit("Bowler", 5, Village.Home, Resource.DarkElixir),
            new Unit("Ice Golem", 6, Village.Home, Resource.DarkElixir),
            new Unit("Head Hunter", 3, Village.Home, Resource.DarkElixir),

            new Unit("Raged Barbarian", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Sneaky Archer", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Boxer Giant", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Beta Minion", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Bomber", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Baby Dragon", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Cannon Cart", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Night Witch", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Drop Ship", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Super P.E.K.K.A", 18, Village.BuilderBase, Resource.Elixir),
            new Unit("Hog Glider", 18, Village.BuilderBase, Resource.Elixir)
        };

        public static Unit[] Spells { get; } = new Unit[]
        {
            new Unit("Lightning Spell", 9, Village.Home, Resource.Elixir),
            new Unit("Healing Spell", 8, Village.Home, Resource.Elixir),
            new Unit("Rage Spell", 6, Village.Home, Resource.Elixir),
            new Unit("Jump Spell", 4, Village.Home, Resource.Elixir),
            new Unit("Freeze Spell", 7, Village.Home, Resource.Elixir),
            new Unit("Clone Spell", 7, Village.Home, Resource.Elixir),
            new Unit("Invisibility Spell", 4, Village.Home, Resource.Elixir),
            new Unit("Poison Spell", 8, Village.Home, Resource.DarkElixir),
            new Unit("Earthquake Spell", 5, Village.Home, Resource.DarkElixir),
            new Unit("Haste Spell", 5, Village.Home, Resource.DarkElixir),
            new Unit("Skeleton Spell", 7, Village.Home, Resource.DarkElixir),
            new Unit("Bat Spell", 5, Village.Home, Resource.DarkElixir)
        };

        public static Unit[] SiegeMachines { get; } = new Unit[]
        {
            new Unit("Wall Wrecker", 4, Village.Home, Resource.Elixir),
            new Unit("Battle Blimp", 4, Village.Home, Resource.Elixir),
            new Unit("Stone Slammer", 4, Village.Home, Resource.Elixir),
            new Unit("Siege Barracks", 4, Village.Home, Resource.Elixir),
            new Unit("Log Launcher", 4, Village.Home, Resource.Elixir)

        };

        public const int MAX_TOWN_HALL_LEVEL = 14;

        public const int MAX_BUILD_BASE_LEVEL = 9;

        public static Regex TagRegEx { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");

        public static bool IsValidTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return false;

            if (tag == "#0")
                return false;

            if (tag.Length < 4)
                return false;

            if (tag.Count(t => t == '#') != 1)
                return false;

            return TagRegEx.IsMatch(tag);
        }

        public static bool TryFormatTag(string userInput, [NotNullWhen(true)] out string? formattedTag)
        {
            formattedTag = NormalizeTag(userInput);

            if (IsValidTag(formattedTag))
                return true;

            formattedTag = null;

            return false;
        }

        public static string FormatTag(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new InvalidTagException(userInput);

            string formattedTag = NormalizeTag(userInput);

            if (IsValidTag(formattedTag) == false)
                throw new InvalidTagException(userInput);

            return formattedTag;
        }

        [return: NotNullIfNotNull("userInput")]
        private static string? NormalizeTag(string? userInput)
        {
            if (userInput == null)
                return userInput;

            if (userInput.StartsWith("\"") && userInput.EndsWith("\"") && userInput.Length > 2)
                userInput = userInput[1..^1];

            else if (userInput.StartsWith("`") && userInput.EndsWith("`") && userInput.Length > 2)
                userInput = userInput[1..^1];

            else if (userInput.StartsWith("'") && userInput.EndsWith("'") && userInput.Length > 2)
                userInput = userInput[1..^1];

            string formattedTag = userInput.ToUpper();

            formattedTag = formattedTag.Replace("O", "0");

            formattedTag = Regex.Replace(formattedTag, "#+", "#");

            if (!formattedTag.StartsWith("#"))
                formattedTag = $"#{formattedTag}";

            return formattedTag;
        }

        public static bool IsCwlEnabled
        {
            get
            {
                int day = DateTime.UtcNow.Day;

                if (day > 0 && day < 11)
                    return true;

                //add three hours to the end to ensure we get everything
                if (day == 11 && DateTime.UtcNow.Hour < 3)
                    return true;

                return false;
            }
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

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        public static string PlayerProfileUrl(string tag) => $"https://link.clashofclans.com/?action=OpenPlayerProfile&tag={tag[1..]}";

        public static string PlayerProfileUrl(this CocApi.Model.Player player) => PlayerProfileUrl(player.Tag);

        public static string PlayerProfileUrl(this CocApi.Model.ClanMember member) => PlayerProfileUrl(member.Tag);

        public static string PlayerProfileUrl(this CocApi.Model.ClanWarMember member) => PlayerProfileUrl(member.Tag);

        public static string ClanProfileUrl(string tag) => $"https://link.clashofclans.com/?action=OpenClanProfile&tag={tag[1..]}";

        public static string ClanProfileUrl(this CocApi.Model.Clan clan) => ClanProfileUrl(clan.Tag);

        public static string ClanProfileUrl(this CocApi.Model.WarClan clan) => ClanProfileUrl(clan.Tag);
    }
}
