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
        public static Unit BK { get; } = new(Village.Home, Resource.DarkElixir, 1, 80, "Barbarian King");
        public static Unit AQ { get; } = new(Village.Home, Resource.DarkElixir, 2, 80, "Archer Queen");
        public static Unit GW { get; } = new(Village.Home, Resource.Elixir, 3, 40, "Grand Warden");
        public static Unit RC { get; } = new(Village.Home, Resource.DarkElixir, 4, 30, "Royal Champion");
        public static Unit BM { get; } = new(Village.BuilderBase, Resource.Elixir, 5, 30, "Battle Machine");

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
            new Unit(Village.Home, Resource.DarkElixir, 1, 10, "L.A.S.S.I"),
            new Unit(Village.Home, Resource.DarkElixir, 2, 10, "Electro Owl"),
            new Unit(Village.Home, Resource.DarkElixir, 3, 10, "Mighty Yak"),
            new Unit(Village.Home, Resource.DarkElixir, 4, 10, "Unicorn")
        };

        private static readonly Unit _barbarian   = new(Village.Home, Resource.Elixir, 101, 10, "Barbarian", 0);
        private static readonly Unit _archer      = new(Village.Home, Resource.Elixir, 102, 10, "Archer", 1);
        private static readonly Unit _giant       = new(Village.Home, Resource.Elixir, 103, 10, "Giant", 3);
        private static readonly Unit _goblin      = new(Village.Home, Resource.Elixir, 104, 8, "Goblin", 2);
        private static readonly Unit _wallBreaker = new(Village.Home, Resource.Elixir, 105, 10, "Wall Breaker", 4);
        private static readonly Unit _balloon     = new(Village.Home, Resource.Elixir, 106, 10, "Balloon", 5);
        private static readonly Unit _wizard      = new(Village.Home, Resource.Elixir, 107, 10, "Wizard", 6);
        private static readonly Unit _babyDragon  = new(Village.Home, Resource.Elixir, 111, 8, "Baby Dragon", 23);

        private static readonly Unit _minion      = new(Village.Home, Resource.DarkElixir, 201, 10, "Minion", 10);
        private static readonly Unit _valkyrie    = new(Village.Home, Resource.DarkElixir, 203, 9, "Valkyrie", 12);
        private static readonly Unit _witch       = new(Village.Home, Resource.DarkElixir, 205, 5, "Witch", 14);
        private static readonly Unit _lavaHound   = new(Village.Home, Resource.DarkElixir, 206, 6, "Lava Hound", 17);

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

            new Unit(Village.Home, Resource.Elixir, 108, 7, "Healer", 7),
            new Unit(Village.Home, Resource.Elixir, 109, 9, "Dragon", 8),
            new Unit(Village.Home, Resource.Elixir, 110, 9, "P.E.K.K.A", 9),
            new Unit(Village.Home, Resource.Elixir, 112, 7, "Miner", 24),
            new Unit(Village.Home, Resource.Elixir, 113, 5, "Electro Dragon", 59),
            new Unit(Village.Home, Resource.Elixir, 114, 3, "Yeti", 53),
            new Unit(Village.Home, Resource.Elixir, 115, 3, "Dragon Rider", 65),

            // TODO: we need the ids for all the season troops
            new Unit(Village.Home, Resource.Elixir, 180, _wizard.MaxLevel, "Ice Wizard", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 181, 11, "Battle Ram", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 182, _barbarian.MaxLevel, "Pumpkin Barbarian", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 183, 11, "Giant Skeleton", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 184, 9, "Skeleton Barrel", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 185, 8, "El Primo", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 186, _wizard.MaxLevel, "Party Wizard", isSeasonalTroop: true),
            new Unit(Village.Home, Resource.Elixir, 187, 8, "Royal Ghost", isSeasonalTroop: true),

            new Unit(Village.Home, Resource.DarkElixir, 202, 10, "Hog Rider", 11),
            new Unit(Village.Home, Resource.DarkElixir, 204, 10, "Golem", 13),
            new Unit(Village.Home, Resource.DarkElixir, 207, 5, "Bowler", 22),
            new Unit(Village.Home, Resource.DarkElixir, 208, 6, "Ice Golem"),
            new Unit(Village.Home, Resource.DarkElixir, 209, 3, "Headhunter", 82),

            new Unit(Village.Home, Resource.Elixir, 301, _barbarian, "Super Barbarian", 26),
            new Unit(Village.Home, Resource.Elixir, 302, _archer, "Super Archer", 27),
            new Unit(Village.Home, Resource.Elixir, 303, _giant, "Super Giant", 29),
            new Unit(Village.Home, Resource.Elixir, 304, _goblin, "Sneaky Goblin", 55),
            new Unit(Village.Home, Resource.Elixir, 305, _wallBreaker, "Super Wall Breaker", 28),
            new Unit(Village.Home, Resource.Elixir, 306, _balloon, "Rocket Balloon", 57),
            new Unit(Village.Home, Resource.Elixir, 307, _wizard, "Super Wizard", 83),
            new Unit(Village.Home, Resource.Elixir, 308, _babyDragon, "Inferno Dragon", 63),
            new Unit(Village.Home, Resource.DarkElixir, 309, _minion, "Super Minion", 84),
            new Unit(Village.Home, Resource.DarkElixir, 310, _valkyrie, "Super Valkyrie", 64),
            new Unit(Village.Home, Resource.DarkElixir, 311, _witch, "Super Witch", 66),
            new Unit(Village.Home, Resource.DarkElixir, 313, _lavaHound, "Ice Hound", 76),

            new Unit(Village.BuilderBase, Resource.Elixir, 401, 18, "Raged Barbarian"),
            new Unit(Village.BuilderBase, Resource.Elixir, 402, 18, "Sneaky Archer"),
            new Unit(Village.BuilderBase, Resource.Elixir, 403, 18, "Boxer Giant"),
            new Unit(Village.BuilderBase, Resource.Elixir, 404, 18, "Beta Minion"),
            new Unit(Village.BuilderBase, Resource.Elixir, 405, 18, "Bomber"),
            new Unit(Village.BuilderBase, Resource.Elixir, 406, 18, "Baby Dragon"),
            new Unit(Village.BuilderBase, Resource.Elixir, 407, 18, "Cannon Cart"),
            new Unit(Village.BuilderBase, Resource.Elixir, 408, 18, "Night Witch"),
            new Unit(Village.BuilderBase, Resource.Elixir, 409, 18, "Drop Ship"),
            new Unit(Village.BuilderBase, Resource.Elixir, 410, 18, "Super P.E.K.K.A"),
            new Unit(Village.BuilderBase, Resource.Elixir, 411, 18, "Hog Glider")
        };

        public static Unit[] Spells { get; } = new Unit[]
        {
            new Unit(Village.Home, Resource.Elixir, 101, 9, "Lightning Spell", 0),
            new Unit(Village.Home, Resource.Elixir, 102, 8, "Healing Spell", 1),
            new Unit(Village.Home, Resource.Elixir, 103, 6, "Rage Spell", 2),
            new Unit(Village.Home, Resource.Elixir, 104, 4, "Jump Spell", 3),
            new Unit(Village.Home, Resource.Elixir, 105, 7, "Freeze Spell", 5),
            new Unit(Village.Home, Resource.Elixir, 106, 7, "Clone Spell", 16),
            new Unit(Village.Home, Resource.Elixir, 107, 4, "Invisibility Spell", 35),

            new Unit(Village.Home, Resource.DarkElixir, 201, 8, "Poison Spell", 9),
            new Unit(Village.Home, Resource.DarkElixir, 202, 5, "Earthquake Spell", 10),
            new Unit(Village.Home, Resource.DarkElixir, 203, 5, "Haste Spell", 11),
            new Unit(Village.Home, Resource.DarkElixir, 204, 7, "Skeleton Spell", 17),
            new Unit(Village.Home, Resource.DarkElixir, 205, 5, "Bat Spell", 28)
        };

        public static Unit[] SiegeMachines { get; } = new Unit[]
        {
            new Unit(Village.Home, Resource.Elixir, 1, 4, "Wall Wrecker", 51),
            new Unit(Village.Home, Resource.Elixir, 2, 4, "Battle Blimp", 52),
            new Unit(Village.Home, Resource.Elixir, 3, 4, "Stone Slammer", 62),
            new Unit(Village.Home, Resource.Elixir, 4, 4, "Siege Barracks", 75),
            new Unit(Village.Home, Resource.Elixir, 5, 4, "Log Launcher", 87)
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

        public static Additions.UnitQuantity[] ParseArmyLink(string url)
        {
            if (!url.StartsWith("https://link.clashofclans.com/en?action=CopyArmy&army="))
                throw new InvalidOperationException();

            url = url["https://link.clashofclans.com/en?action=CopyArmy&army=".Length..];

            string encodedTroop = url.Split('s')[0][1..];
            string encodedSpell = url.Split('s')[1];

            string[] encodedTroops = encodedTroop.Split('-');
            string[] encodedSpells = encodedSpell.Split('-');

            Additions.UnitQuantity[] results = new Additions.UnitQuantity[encodedTroops.Length + encodedSpells.Length];

            int i = 0;

            foreach(string troop in encodedTroops)
            {
                int count = int.Parse(troop.Split('x')[0]);
                int id = int.Parse(troop.Split('x')[1]);

                results[i] = new Additions.UnitQuantity(count, Troops.FirstOrDefault(t => t.TroopId == id));

                i++;
            }

            foreach (string spell in encodedSpells)
            {
                int count = int.Parse(spell.Split('x')[0]);
                int id = int.Parse(spell.Split('x')[1]);

                results[i] = new Additions.UnitQuantity(count, Spells.FirstOrDefault(t => t.TroopId == id));

                i++;
            }

            return results;
        }
    }
}
