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
        public static Unit BK { get; } = new(Village.Home, Resource.DarkElixir, 80, "Barbarian King");
        public static Unit AQ { get; } = new(Village.Home, Resource.DarkElixir, 80, "Archer Queen");
        public static Unit GW { get; } = new(Village.Home, Resource.Elixir, 40, "Grand Warden");
        public static Unit RC { get; } = new(Village.Home, Resource.DarkElixir, 30, "Royal Champion");
        public static Unit BM { get; } = new(Village.BuilderBase, Resource.Elixir, 30, "Battle Machine");

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
            new Unit(Village.Home, Resource.DarkElixir, 10, "L.A.S.S.I"),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Electro Owl"),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Mighty Yak"),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Unicorn")
        };

        public static Unit[] Troops { get; } = new Unit[]
        {
            new Unit(Village.Home, Resource.Elixir, 10, "Barbarian", "Super Barbarian"),
            new Unit(Village.Home, Resource.Elixir, 10, "Archer", "Super Archer"),
            new Unit(Village.Home, Resource.Elixir, 10, "Giant", "Super Giant"),
            new Unit(Village.Home, Resource.Elixir, 8, "Goblin", "Sneaky Goblin"),
            new Unit(Village.Home, Resource.Elixir, 10, "Wall Breaker", "Super Wall Breaker"),
            new Unit(Village.Home, Resource.Elixir, 9, "Balloon"),
            new Unit(Village.Home, Resource.Elixir, 10, "Wizard", "Super Wizard"),
            new Unit(Village.Home, Resource.Elixir, 7, "Healer"),
            new Unit(Village.Home, Resource.Elixir, 8, "Dragon"),
            new Unit(Village.Home, Resource.Elixir, 9, "P.E.K.K.A"),
            new Unit(Village.Home, Resource.Elixir, 8, "Baby Dragon", "Inferno Dragon"),
            new Unit(Village.Home, Resource.Elixir, 7, "Miner"),
            new Unit(Village.Home, Resource.Elixir, 4, "Eletro Dragon"),
            new Unit(Village.Home, Resource.Elixir, 3, "Yeti"),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Minion", "Super Minion"),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Hog Rider"),
            new Unit(Village.Home, Resource.DarkElixir, 9, "Valkyrie", "Super Valkyrie"),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Golem"),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Witch", "Super Witch"),
            new Unit(Village.Home, Resource.DarkElixir, 6, "Lava Hound", "Ice Hound"),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Bowler"),
            new Unit(Village.Home, Resource.DarkElixir, 6, "Ice Golem"),
            new Unit(Village.Home, Resource.DarkElixir, 3, "Head Hunter"),

            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Raged Barbarian"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Sneaky Archer"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Boxer Giant"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Beta Minion"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Bomber"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Baby Dragon"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Cannon Cart"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Night Witch"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Drop Ship"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Super P.E.K.K.A"),
            new Unit(Village.BuilderBase, Resource.Elixir, 18, "Hog Glider")
        };

        public static Unit[] Spells { get; } = new Unit[]
        {
            new Unit(Village.Home, Resource.Elixir, 9, "Lightning Spell"),
            new Unit(Village.Home, Resource.Elixir, 8, "Healing Spell"),
            new Unit(Village.Home, Resource.Elixir, 6, "Rage Spell"),
            new Unit(Village.Home, Resource.Elixir, 4, "Jump Spell"),
            new Unit(Village.Home, Resource.Elixir, 7, "Freeze Spell"),
            new Unit(Village.Home, Resource.Elixir, 7, "Clone Spell"),
            new Unit(Village.Home, Resource.Elixir, 4, "Invisibility Spell"),

            new Unit(Village.Home, Resource.DarkElixir, 8, "Poison Spell"),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Earthquake Spell"),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Haste Spell"),
            new Unit(Village.Home, Resource.DarkElixir, 7, "Skeleton Spell"),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Bat Spell")
        };

        public static Unit[] SiegeMachines { get; } = new Unit[]
        {
            new Unit(Village.Home, Resource.Elixir, 4, "Wall Wrecker"),
            new Unit(Village.Home, Resource.Elixir, 4, "Battle Blimp"),
            new Unit(Village.Home, Resource.Elixir, 4, "Stone Slammer"),
            new Unit(Village.Home, Resource.Elixir, 4, "Siege Barracks"),
            new Unit(Village.Home, Resource.Elixir, 4, "Log Launcher")
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
            List<JsonConverter> results = new()
            {
                new SuperCellDateConverter { DateTimeFormats = new List<string> { "yyyyMMdd'T'HHmmss.fff'Z'", "yyyy'-'MM" } }
            };

            return results;
        }

        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
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
