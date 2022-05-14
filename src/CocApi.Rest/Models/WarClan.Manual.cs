using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class WarClan
    {
        public string ClanProfileUrl => Clash.ClanProfileUrl(Tag);


        [JsonPropertyName("result")]
        public Result? Result { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WarClan" /> class.
        /// </summary>
        /// <param name="attacks">attacks</param>
        /// <param name="badgeUrls">badgeUrls</param>
        /// <param name="clanLevel">clanLevel</param>
        /// <param name="destructionPercentage">destructionPercentage</param>
        /// <param name="expEarned">expEarned</param>
        /// <param name="members">members</param>
        /// <param name="name">name</param>
        /// <param name="stars">stars</param>
        /// <param name="tag">tag</param>
        public WarClan(int attacks, BadgeUrls badgeUrls, int clanLevel, float destructionPercentage, int expEarned, List<ClanWarMember> members, string name, int stars, string tag)
        {
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            if (destructionPercentage == null)
                throw new ArgumentNullException("destructionPercentage is a required property for WarClan and cannot be null.");

            //if (tag == null)
            //    throw new ArgumentNullException("tag is a required property for WarClan and cannot be null.");

            //if (name == null)
            //    throw new ArgumentNullException("name is a required property for WarClan and cannot be null.");

            if (badgeUrls == null)
                throw new ArgumentNullException("badgeUrls is a required property for WarClan and cannot be null.");

            if (clanLevel == null)
                throw new ArgumentNullException("clanLevel is a required property for WarClan and cannot be null.");

            if (attacks == null)
                throw new ArgumentNullException("attacks is a required property for WarClan and cannot be null.");

            if (stars == null)
                throw new ArgumentNullException("stars is a required property for WarClan and cannot be null.");

            //if (expEarned == null)
            //    throw new ArgumentNullException("expEarned is a required property for WarClan and cannot be null.");

            //if (members == null)
            //    throw new ArgumentNullException("members is a required property for WarClan and cannot be null.");

#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'

            Attacks = attacks;
            BadgeUrls = badgeUrls;
            ClanLevel = clanLevel;
            DestructionPercentage = destructionPercentage;
            ExpEarned = expEarned;
            Members = members;
            Name = name;
            Stars = stars;
            Tag = tag;
        }
    }
}
