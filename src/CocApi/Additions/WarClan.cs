//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Runtime.Serialization;
//using System.Text;
//using Newtonsoft.Json;

//namespace CocApi.Model
//{
//    public partial class WarClan
//    {
//        private List<ClanWarAttack> _allAttacks;

//        [DataMember(Name = "allAttacks", EmitDefaultValue = false)]
//        [JsonProperty("allAttacks")]
//        public List<ClanWarAttack> Attacks
//        {
//            get
//            {
//                if (_allAttacks == null)
//                {
//                    _allAttacks = new List<ClanWarAttack>();
//                    foreach (var member in Members)
//                        foreach (var attack in member.Attacks.EmptyIfNull())
//                            _allAttacks.Add(attack);
//                }

//                return _allAttacks;
//            }

//            internal set
//            {
//                _allAttacks = value;
//            }
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="WarClan" /> class.
//        /// </summary>
//        /// <param name="destructionPercentage">destructionPercentage.</param>
//        /// <param name="tag">tag.</param>
//        /// <param name="name">name.</param>
//        /// <param name="badgeUrls">badgeUrls.</param>
//        /// <param name="clanLevel">clanLevel.</param>
//        /// <param name="attacks">attacks.</param>
//        /// <param name="stars">stars.</param>
//        /// <param name="expEarned">expEarned.</param>
//        /// <param name="members">members.</param>
//        [JsonConstructor]
//        public WarClan(List<ClanWarAttack> allAttacks = default(List<ClanWarAttack>), float destructionPercentage = default(float), string tag = default(string), string name = default(string), ClanBadgeUrls badgeUrls = default(ClanBadgeUrls), int clanLevel = default(int), /*int attacks = default(int),*/ int stars = default(int), int expEarned = default(int), List<ClanWarMember> members = default(List<ClanWarMember>))
//        {
//            this.Attacks = allAttacks;
//            this.DestructionPercentage = destructionPercentage;
//            this.Tag = tag;
//            this.Name = name;
//            this.BadgeUrls = badgeUrls;
//            this.ClanLevel = clanLevel;
//            //this.Attacks = attacks;
//            this.Stars = stars;
//            this.ExpEarned = expEarned;
//            this.Members = members;
//        }
//    }
//}
