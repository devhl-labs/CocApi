using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CocApi.Model
{
    public partial class Player : IEquatable<Player?>
    {
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);
        public static string Url(string villageTag)
        {
            if (Clash.TryFormatTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            return $"players/{Uri.EscapeDataString(formattedTag)}";
        }

        /// <summary>
        /// Gets or Sets Clan
        /// </summary>
        [DataMember(Name = "clan", EmitDefaultValue = false)]
        public PlayerClan? Clan { get; private set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Player);
        }

        public bool Equals(Player? other)
        {
            return other != null &&
                   Tag == other.Tag;
        }

        /// <summary>
        /// Gets or Sets League
        /// </summary>
        [DataMember(Name = "league", EmitDefaultValue = false)]
        public League? League { get; private set; }

        /// <summary>
        /// Gets or Sets Role
        /// </summary>
        [DataMember(Name = "role", EmitDefaultValue = false)]
        public Role? Role { get; private set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag);
        }

        public int? HeroLevel(string name) => Heroes.First(h => h.Name == name).Level;
        

        public int? HeroLevelOrDefault(string name) => Heroes.FirstOrDefault(h => h.Name == name)?.Level;        
    }
}
