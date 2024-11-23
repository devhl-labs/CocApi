using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace CocApi
{
    public static class Clash
    {
        public static Unit BarbarianKing { get; } = new(Village.Home, Resource.DarkElixir, 1, 90, "Barbarian King");
        public static Unit ArcherQueen { get; } = new(Village.Home, Resource.DarkElixir, 2, 90, "Archer Queen");
        public static Unit GrandWarden { get; } = new(Village.Home, Resource.Elixir, 3, 65, "Grand Warden");
        public static Unit RoyalChampion { get; } = new(Village.Home, Resource.DarkElixir, 4, 40, "Royal Champion");
        public static Unit BattleMachine { get; } = new(Village.BuilderBase, Resource.Elixir, 5, 35, "Battle Machine");
        public static Unit BattleCopter { get; } = new(Village.BuilderBase, Resource.Elixir, 6, 35, "Battle Copter");

        public static Unit[] Heroes { get; } = new Unit[]
        {
            BarbarianKing,
            ArcherQueen,
            GrandWarden,
            RoyalChampion,
            BattleMachine,
            BattleCopter
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

        public static Unit[] Pets { get; } =
        [
            new Unit(Village.Home, Resource.DarkElixir, 1, 10, "L.A.S.S.I"),
            new Unit(Village.Home, Resource.DarkElixir, 2, 10, "Electro Owl"),
            new Unit(Village.Home, Resource.DarkElixir, 3, 10, "Mighty Yak"),
            new Unit(Village.Home, Resource.DarkElixir, 4, 10, "Unicorn"),
            new Unit(Village.Home, Resource.DarkElixir, 5, 10, "Frosty"),
            new Unit(Village.Home, Resource.DarkElixir, 6, 10, "Diggy"),
            new Unit(Village.Home, Resource.DarkElixir, 7, 10, "Poison Lizard"),
            new Unit(Village.Home, Resource.DarkElixir, 8, 10, "Phoenix"),
            new Unit(Village.Home, Resource.DarkElixir, 9, 10, "Spirit Fox")
        ];

        private static readonly Unit _barbarian    = new(Village.Home, Resource.Elixir, 101, 12, "Barbarian", 0);
        private static readonly Unit _archer       = new(Village.Home, Resource.Elixir, 102, 12, "Archer", 1);
        private static readonly Unit _giant        = new(Village.Home, Resource.Elixir, 103, 12, "Giant", 3);
        private static readonly Unit _goblin       = new(Village.Home, Resource.Elixir, 104, 9, "Goblin", 2);
        private static readonly Unit _wallBreaker  = new(Village.Home, Resource.Elixir, 105, 12, "Wall Breaker", 4);
        private static readonly Unit _balloon      = new(Village.Home, Resource.Elixir, 106, 11, "Balloon", 5);
        private static readonly Unit _wizard       = new(Village.Home, Resource.Elixir, 107, 12, "Wizard", 6);
        private static readonly Unit _dragon       = new(Village.Home, Resource.Elixir, 109, 11,  "Dragon", 8);
        private static readonly Unit _babyDragon   = new(Village.Home, Resource.Elixir, 111, 10, "Baby Dragon", 23);
        private static readonly Unit _miner        = new(Village.Home, Resource.Elixir, 112, 10, "Miner", 24);
        private static readonly Unit _electroTitan = new(Village.Home, Resource.Elixir, 113, 4, "Electro Titan", 95);

        private static readonly Unit _minion      = new(Village.Home, Resource.DarkElixir, 201, 12, "Minion", 10);
        private static readonly Unit _valkyrie    = new(Village.Home, Resource.DarkElixir, 203, 11, "Valkyrie", 12);
        private static readonly Unit _witch       = new(Village.Home, Resource.DarkElixir, 205, 7, "Witch", 14);
        private static readonly Unit _lavaHound   = new(Village.Home, Resource.DarkElixir, 206, 6, "Lava Hound", 17);
        private static readonly Unit _bowler      = new(Village.Home, Resource.DarkElixir, 207, 8, "Bowler", 22);
        private static readonly Unit _hogRider = new(Village.Home, Resource.DarkElixir, 202, 13, "Hog Rider", 11);

        public static Unit[] Troops { get; } =
        [
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
            _electroTitan,

            new Unit(Village.Home, Resource.Elixir, 108, 9, "Healer", 7),
            new Unit(Village.Home, Resource.Elixir, 110, 11, "P.E.K.K.A", 9),
            new Unit(Village.Home, Resource.Elixir, 112, _miner.MaxLevel, "Miner", 24),
            new Unit(Village.Home, Resource.Elixir, 113, 7, "Electro Dragon", 59),
            new Unit(Village.Home, Resource.Elixir, 114, 6, "Yeti", 53),
            new Unit(Village.Home, Resource.Elixir, 115, 4, "Dragon Rider", 65),

            new Unit(Village.Home, Resource.Elixir, 180, _wizard.MaxLevel, "Ice Wizard", 30, true),
            new Unit(Village.Home, Resource.Elixir, 181, 11, "Battle Ram", 45, true),
            new Unit(Village.Home, Resource.Elixir, 182, _barbarian.MaxLevel, "Pumpkin Barbarian", 48, true),
            new Unit(Village.Home, Resource.Elixir, 183, 11, "Giant Skeleton", 50, true),
            new Unit(Village.Home, Resource.Elixir, 184, 9, "Skeleton Barrel", 61, true),
            new Unit(Village.Home, Resource.Elixir, 185, 8, "El Primo", 67, true),
            new Unit(Village.Home, Resource.Elixir, 186, _wizard.MaxLevel, "Party Wizard", 72, true),
            new Unit(Village.Home, Resource.Elixir, 187, 8, "Royal Ghost", 47, true),
            new Unit(Village.Home, Resource.Elixir, 188, 3, "Root Rider", 110),

            _hogRider,
            new Unit(Village.Home, Resource.DarkElixir, 204, 13, "Golem", 13),
            new Unit(Village.Home, Resource.DarkElixir, 208, 8, "Ice Golem"),
            new Unit(Village.Home, Resource.DarkElixir, 209, 3, "Headhunter", 82),
            new Unit(Village.Home, Resource.DarkElixir, 210, 4, "Apprentice Warden", 97),
            new Unit(Village.Home, Resource.DarkElixir, 222, 4, "Druid", 123),

            new Unit(Village.Home, Resource.Elixir, 301, _barbarian, "Super Barbarian", 26),
            new Unit(Village.Home, Resource.Elixir, 302, _archer, "Super Archer", 27),
            new Unit(Village.Home, Resource.Elixir, 303, _giant, "Super Giant", 29),
            new Unit(Village.Home, Resource.Elixir, 304, _goblin, "Sneaky Goblin", 55),
            new Unit(Village.Home, Resource.Elixir, 305, _wallBreaker, "Super Wall Breaker", 28),
            new Unit(Village.Home, Resource.Elixir, 306, _balloon, "Rocket Balloon", 57),
            new Unit(Village.Home, Resource.Elixir, 307, _wizard, "Super Wizard", 83),
            new Unit(Village.Home, Resource.DarkElixir, 308, _dragon, "Super Dragon", 81),
            new Unit(Village.Home, Resource.Elixir, 309, _babyDragon, "Inferno Dragon", 63),
            new Unit(Village.Home, Resource.DarkElixir, 310, _minion, "Super Minion", 84),
            new Unit(Village.Home, Resource.DarkElixir, 311, _valkyrie, "Super Valkyrie", 64),
            new Unit(Village.Home, Resource.DarkElixir, 312, _witch, "Super Witch", 66),
            new Unit(Village.Home, Resource.DarkElixir, 313, _lavaHound, "Ice Hound", 76),
            new Unit(Village.Home, Resource.DarkElixir, 314, _bowler, "Super Bowler", 80),
            new Unit(Village.Home, Resource.DarkElixir, 315, _miner, "Super Miner", 56),
            new Unit(Village.Home, Resource.DarkElixir, 316, _hogRider, "Super Hog Rider", 98),

            new Unit(Village.BuilderBase, Resource.Elixir, 401, 20, "Raged Barbarian"),
            new Unit(Village.BuilderBase, Resource.Elixir, 402, 20, "Sneaky Archer"),
            new Unit(Village.BuilderBase, Resource.Elixir, 403, 20, "Boxer Giant"),
            new Unit(Village.BuilderBase, Resource.Elixir, 404, 20, "Beta Minion"),
            new Unit(Village.BuilderBase, Resource.Elixir, 405, 20, "Bomber"),
            new Unit(Village.BuilderBase, Resource.Elixir, 406, 20, "Baby Dragon"),
            new Unit(Village.BuilderBase, Resource.Elixir, 407, 20, "Cannon Cart"),
            new Unit(Village.BuilderBase, Resource.Elixir, 408, 20, "Night Witch"),
            new Unit(Village.BuilderBase, Resource.Elixir, 409, 20, "Drop Ship"),
            new Unit(Village.BuilderBase, Resource.Elixir, 410, 20, "Super P.E.K.K.A"),
            new Unit(Village.BuilderBase, Resource.Elixir, 411, 20, "Hog Glider"),
            new Unit(Village.BuilderBase, Resource.Elixir, 412, 20, "Electrofire Wizard")
        ];

        public static Unit[] Spells { get; } =
        [
            new Unit(Village.Home, Resource.Elixir, 101, 11, "Lightning Spell", 0),
            new Unit(Village.Home, Resource.Elixir, 102, 10, "Healing Spell", 1),
            new Unit(Village.Home, Resource.Elixir, 103, 6, "Rage Spell", 2),
            new Unit(Village.Home, Resource.Elixir, 104, 5, "Jump Spell", 3),
            new Unit(Village.Home, Resource.Elixir, 105, 7, "Freeze Spell", 5),
            new Unit(Village.Home, Resource.Elixir, 106, 8, "Clone Spell", 16),
            new Unit(Village.Home, Resource.Elixir, 107, 4, "Invisibility Spell", 35),
            new Unit(Village.Home, Resource.Elixir, 108, 1, "Santa's Surprise", 4, true),
            new Unit(Village.Home, Resource.Elixir, 109, 1, "Birthday Boom", 22, true),
            new Unit(Village.Home, Resource.Elixir, 110, 5, "Recall Spell", 53),
            new Unit(Village.Home, Resource.Elixir, 111, 4, "Revive Spell", 98),

            new Unit(Village.Home, Resource.DarkElixir, 201, 10, "Poison Spell", 9),
            new Unit(Village.Home, Resource.DarkElixir, 202, 5, "Earthquake Spell", 10),
            new Unit(Village.Home, Resource.DarkElixir, 203, 5, "Haste Spell", 11),
            new Unit(Village.Home, Resource.DarkElixir, 204, 8, "Skeleton Spell", 17),
            new Unit(Village.Home, Resource.DarkElixir, 205, 6, "Bat Spell", 28),
            new Unit(Village.Home, Resource.DarkElixir, 206, 4, "Overgrowth Spell", 70)
        ];

        public static Unit[] SiegeMachines { get; } =
        [
            new Unit(Village.Home, Resource.Elixir, 1, 5, "Wall Wrecker", 51),
            new Unit(Village.Home, Resource.Elixir, 2, 4, "Battle Blimp", 52),
            new Unit(Village.Home, Resource.Elixir, 3, 5, "Stone Slammer", 62),
            new Unit(Village.Home, Resource.Elixir, 4, 5, "Siege Barracks", 75),
            new Unit(Village.Home, Resource.Elixir, 5, 5, "Log Launcher", 87),
            new Unit(Village.Home, Resource.Elixir, 6, 5, "Flame Flinger", 91),
            new Unit(Village.Home, Resource.Elixir, 7, 5, "Battle Drill", 92)
        ];

        public const int MAX_TOWN_HALL_LEVEL = 17;

        public const int MAX_BUILD_BASE_LEVEL = 10;

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

        /// <summary>
        /// Attempts to correct the given string to make a proper tag. The result may not be a valid tag.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(value))]
        public static string? NormalizeTag(string? value)
        {
            if (value == null)
                return value;

            if (value.StartsWith('\"') && value.EndsWith('\"') && value.Length > 2)
                value = value[1..^1];

            else if (value.StartsWith('`') && value.EndsWith('`') && value.Length > 2)
                value = value[1..^1];

            else if (value.StartsWith('\'') && value.EndsWith('\'') && value.Length > 2)
                value = value[1..^1];

            value = value.ToUpper().Replace("O", "0");

            value = Regex.Replace(value, "#+", "#");

            if (!value.StartsWith('#'))
                value = $"#{value}";

            return value;
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

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source) => source ?? Enumerable.Empty<T>();

        public static string PlayerProfileUrl(string tag) => $"https://link.clashofclans.com/?action=OpenPlayerProfile&tag={tag[1..]}";

        public static string ClanProfileUrl(string tag) => $"https://link.clashofclans.com/?action=OpenClanProfile&tag={tag[1..]}";

        public static UnitQuantity[] ParseArmyLink(string url)
        {
            if (!url.StartsWith("https://link.clashofclans.com/en?action=CopyArmy&army="))
                throw new InvalidOperationException();

            url = url["https://link.clashofclans.com/en?action=CopyArmy&army=".Length..];

            string encodedTroop = url.Split('s')[0][1..];
            string encodedSpell = url.Split('s')[1];

            string[] encodedTroops = encodedTroop.Split('-');
            string[] encodedSpells = encodedSpell.Split('-');

            UnitQuantity[] results = new UnitQuantity[encodedTroops.Length + encodedSpells.Length];

            int i = 0;

            foreach(string troop in encodedTroops)
            {
                int count = int.Parse(troop.Split('x')[0]);
                int id = int.Parse(troop.Split('x')[1]);

                results[i] = new UnitQuantity(count, Troops.FirstOrDefault(t => t.TroopId == id));

                i++;
            }

            foreach (string spell in encodedSpells)
            {
                int count = int.Parse(spell.Split('x')[0]);
                int id = int.Parse(spell.Split('x')[1]);

                results[i] = new UnitQuantity(count, Spells.FirstOrDefault(t => t.TroopId == id));

                i++;
            }

            return results;
        }
    }
}
