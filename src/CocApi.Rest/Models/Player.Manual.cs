using System;
using System.Linq;

namespace CocApi.Rest.Models
{
    public partial class Player
    {

        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);

        public static string Url(string villageTag)
        {
            if (Clash.TryFormatTag(villageTag, out string? formattedTag) == false)
                throw new InvalidTagException(villageTag);

            return $"players/{Uri.EscapeDataString(formattedTag)}";
        }

        public int HeroLevel(Unit hero) => Heroes.First(h => h.Name == hero.Name).Level;

        public int? HeroLevelOrDefault(Unit hero) => Heroes.FirstOrDefault(h => h.Name == hero.Name)?.Level;
    }
}
