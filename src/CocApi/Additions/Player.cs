using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Model
{
    public partial class Player : IEquatable<Player?>
    {
        public static string Url(string villageTag)
        {
            if (Clash.TryFormatTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            return $"players/{Uri.EscapeDataString(formattedTag)}";
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player? other)
        {
            return other != null &&
                   Tag == other.Tag;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag);
        }
    }
}
