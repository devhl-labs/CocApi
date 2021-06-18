using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static CocApi.Clash;

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

        private static readonly Unit _barbarian = new (Village.Home, Resource.Elixir, 10, "Barbarian", 0);
        private static readonly Unit _archer = new (Village.Home, Resource.Elixir, 10, "Archer", 1);
        private static readonly Unit _giant = new (Village.Home, Resource.Elixir, 10, "Giant", 3);
        private static readonly Unit _goblin = new (Village.Home, Resource.Elixir, 8, "Goblin", 2);
        private static readonly Unit _wallBreaker = new (Village.Home, Resource.Elixir, 10, "Wall Breaker", 4);
        private static readonly Unit _balloon = new (Village.Home, Resource.Elixir, 10, "Balloon", 5);
        private static readonly Unit _wizard = new (Village.Home, Resource.Elixir, 10, "Wizard", 6);
        private static readonly Unit _babyDragon = new (Village.Home, Resource.Elixir, 8, "Baby Dragon", 23);
        private static readonly Unit _minion = new (Village.Home, Resource.DarkElixir, 10, "Minion", 10);
        private static readonly Unit _valkyrie = new (Village.Home, Resource.DarkElixir, 9, "Valkyrie", 12);
        private static readonly Unit _witch = new (Village.Home, Resource.DarkElixir, 5, "Witch", 14);
        private static readonly Unit _lavaHound = new (Village.Home, Resource.DarkElixir, 6, "Lava Hound", 17);

        public static Unit[] Troops { get; } = new Unit[]
        {
            _barbarian,
            _archer,
            _giant,
            _goblin,
            _wallBreaker,
            _balloon,
            _wizard,
            _babyDragon,
            _minion,
            _valkyrie,
            _witch,
            _lavaHound,
            new Unit(Village.Home, Resource.Elixir, _barbarian.MaxLevel, "Super Barbarian", 26, _barbarian),
            new Unit(Village.Home, Resource.Elixir, _archer.MaxLevel, "Super Archer", 27, _archer),
            new Unit(Village.Home, Resource.Elixir, _giant.MaxLevel, "Super Giant", 29, _giant),
            new Unit(Village.Home, Resource.Elixir, _goblin.MaxLevel, "Sneaky Goblin", 55, _goblin),
            new Unit(Village.Home, Resource.Elixir, _wallBreaker.MaxLevel,"Super Wall Breaker", 28, _wallBreaker),
            new Unit(Village.Home, Resource.Elixir, _balloon.MaxLevel, "Rocket Balloon", 57, _balloon),
            new Unit(Village.Home, Resource.Elixir, _wizard.MaxLevel, "Super Wizard", 83, _wizard),
            new Unit(Village.Home, Resource.Elixir, 7, "Healer", 7),
            new Unit(Village.Home, Resource.Elixir, 9, "Dragon", 8),
            new Unit(Village.Home, Resource.Elixir, 9, "P.E.K.K.A", 9),
            new Unit(Village.Home, Resource.Elixir, _babyDragon.MaxLevel, "Inferno Dragon", 63, _babyDragon),
            new Unit(Village.Home, Resource.Elixir, 7, "Miner", 24),
            new Unit(Village.Home, Resource.Elixir, 5, "Electro Dragon", 59),
            new Unit(Village.Home, Resource.Elixir, 3, "Yeti", 53),
            new Unit(Village.Home, Resource.Elixir, 3, "Dragon Rider", 65),
            new Unit(Village.Home, Resource.DarkElixir, _minion.MaxLevel, "Super Minion", 84, _minion),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Hog Rider", 11),
            new Unit(Village.Home, Resource.DarkElixir, _valkyrie.MaxLevel, "Super Valkyrie", 64, _valkyrie),
            new Unit(Village.Home, Resource.DarkElixir, 10, "Golem", 13),
            new Unit(Village.Home, Resource.DarkElixir, _wizard.MaxLevel, "Super Witch", 66, _witch),
            new Unit(Village.Home, Resource.DarkElixir, _lavaHound.MaxLevel, "Ice Hound", 76, _lavaHound),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Bowler", 22),
            new Unit(Village.Home, Resource.DarkElixir, 6, "Ice Golem", 58),
            new Unit(Village.Home, Resource.DarkElixir, 3, "Head Hunter", 82),

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
            new Unit(Village.Home, Resource.Elixir, 9, "Lightning Spell", 0),
            new Unit(Village.Home, Resource.Elixir, 8, "Healing Spell", 1),
            new Unit(Village.Home, Resource.Elixir, 6, "Rage Spell", 2),
            new Unit(Village.Home, Resource.Elixir, 4, "Jump Spell", 3),
            new Unit(Village.Home, Resource.Elixir, 7, "Freeze Spell", 5),
            new Unit(Village.Home, Resource.Elixir, 7, "Clone Spell", 16),
            new Unit(Village.Home, Resource.Elixir, 4, "Invisibility Spell", 35),

            new Unit(Village.Home, Resource.DarkElixir, 8, "Poison Spell", 9),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Earthquake Spell", 10),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Haste Spell", 11),
            new Unit(Village.Home, Resource.DarkElixir, 7, "Skeleton Spell", 17),
            new Unit(Village.Home, Resource.DarkElixir, 5, "Bat Spell", 28)
        };

        public static Unit[] SiegeMachines { get; } = new Unit[]
        {
            new Unit(Village.Home, Resource.Elixir, 4, "Wall Wrecker", 51),
            new Unit(Village.Home, Resource.Elixir, 4, "Battle Blimp", 52),
            new Unit(Village.Home, Resource.Elixir, 4, "Stone Slammer", 62),
            new Unit(Village.Home, Resource.Elixir, 4, "Siege Barracks", 75),
            new Unit(Village.Home, Resource.Elixir, 4, "Log Launcher", 87)
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
