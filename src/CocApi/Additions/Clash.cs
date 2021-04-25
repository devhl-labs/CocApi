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
        public enum Hero
        {
            BK,
            AQ,
            GW,
            RC,
            BM
        }

        public static string Name(this Hero hero)
        {
            return hero switch
            {
                Hero.BK => "Barbarian King",
                Hero.AQ => "Archer Queen",
                Hero.GW => "Grand Warden",
                Hero.RC => "Royal Champion",
                Hero.BM => "Battle Machine",
                _ => throw new NotImplementedException(),
            };
        }

        public enum Pet
        {
            Lassi,
            ElectroOwl,
            MightyYak,
            Unicorn
        }

        public static string Name(this Pet pet)
        {
            return pet switch
            {
                Pet.Lassi => "L.A.S.S.I",
                Pet.ElectroOwl => "Electro Owl",
                Pet.MightyYak => "Mighty Yak",
                Pet.Unicorn => "Unicorn",
                _ => throw new NotImplementedException(),
            };
        }

        public enum SuperTroop
        {
            Barbarian,
            Archer,
            Giant,
            Goblin,
            WallBreaker,
            Wizard,
            InfernoDragon,
            Minion,
            Valkyrie,
            Witch,
            IceHound
        }

        public static string Name(this SuperTroop troop)
        {
            return troop switch
            {
                SuperTroop.Barbarian => "Super Barbarian",
                SuperTroop.Archer => "Super Archer",
                SuperTroop.Giant => "Super Giant",
                SuperTroop.Goblin => "Sneaky Goblin",
                SuperTroop.WallBreaker => "Super Wall Breaker",
                SuperTroop.Wizard => "Super Wizard",
                SuperTroop.InfernoDragon => "Inferno Dragon",
                SuperTroop.Minion => "Super Minion",
                SuperTroop.Valkyrie => "Super Valkyrie",
                SuperTroop.Witch => "Super Witch",
                SuperTroop.IceHound => "Ice Hound",
                _ => throw new NotImplementedException("You provided a wrong super troop name or the case has not been handled yet.")
            };
        }

        public static Troop BaseTroop(this SuperTroop troop)
        {
            return troop switch
            {
                SuperTroop.Barbarian => Troop.Barbarian,
                SuperTroop.Archer => Troop.Archer,
                SuperTroop.Giant => Troop.Giant,
                SuperTroop.Goblin => Troop.Goblin,
                SuperTroop.WallBreaker => Troop.WallBreaker,
                SuperTroop.Wizard => Troop.Wizard,
                SuperTroop.InfernoDragon => Troop.BabyDragon,
                SuperTroop.Minion => Troop.Minion,
                SuperTroop.Valkyrie => Troop.Valkyrie,
                SuperTroop.Witch => Troop.Witch,
                SuperTroop.IceHound => Troop.IceGolem,
                _ => throw new NotImplementedException()
            };
        }

        public enum Troop
        {
            Barbarian,
            Archer,
            Giant,
            Goblin,
            WallBreaker,
            Balloon,
            Wizard,
            Healer,
            Dragon,
            Pekka,
            BabyDragon,
            Miner,
            ElectroDragon,
            Yeti,
            Minion,
            HogRider,
            Valkyrie,
            Golem,
            Witch,
            LavaHound,
            Bowler,
            IceGolem,
            HeadHunter,
        }

        public static string Name(this Troop troop)
        {
            return troop switch
            {
                Troop.Barbarian => "Barbarian",
                Troop.Archer => "Archer",
                Troop.Giant => "Giant",
                Troop.Goblin => "Goblin",
                Troop.WallBreaker => "Wall Breaker",
                Troop.Balloon => "Balloon",
                Troop.Wizard => "Wizard",
                Troop.Healer => "Healer",
                Troop.Dragon => "Dragon",
                Troop.Pekka => "P.E.K.K.A",
                Troop.BabyDragon => "Baby Dragon",
                Troop.Miner => "Miner",
                Troop.ElectroDragon => "Electro Dragon",
                Troop.Yeti => "Yeti",
                Troop.Minion => "Minion",
                Troop.HogRider => "Hog Rider",
                Troop.Valkyrie => "Valkyrie",
                Troop.Golem => "Golem",
                Troop.Witch => "Witch",
                Troop.LavaHound => "Lava Hound",
                Troop.Bowler => "Bowler",
                Troop.IceGolem => "Ice Golem",
                Troop.HeadHunter => "Head Hunter",
                _ => throw new NotImplementedException(),
            };
        }

        public enum BuilderBaseTroop
        {
            RagedBarbarian,
            SneakyGoblin,
            BoxerGiant,
            BetaMinion,
            Bomber,
            BabyDragon,
            CannonCart,
            NightWitch,
            DropShip,
            SuperPekka,
            HogGlider
        }

        public static string Name(this BuilderBaseTroop troop)
        {
            return troop switch
            {
                BuilderBaseTroop.RagedBarbarian => "Raged Barbarian",
                BuilderBaseTroop.SneakyGoblin => "Sneaky Archer",
                BuilderBaseTroop.BoxerGiant => "Boxer Giant",
                BuilderBaseTroop.BetaMinion => "Beta Minion",
                BuilderBaseTroop.Bomber => "Bomber",
                BuilderBaseTroop.BabyDragon => "Baby Dragon",
                BuilderBaseTroop.CannonCart => "Cannon Cart",
                BuilderBaseTroop.NightWitch => "Night Witch",
                BuilderBaseTroop.DropShip => "Drop Ship",
                BuilderBaseTroop.SuperPekka => "Super P.E.K.K.A",
                BuilderBaseTroop.HogGlider => "Hog Glider",
                _ => throw new NotImplementedException(),
            };
        }

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
