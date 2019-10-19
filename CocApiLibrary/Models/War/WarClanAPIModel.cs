using System.Collections.Generic;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class WarClanAPIModel : IClanAPIModel
    {
        public string WarID { get; set; } = string.Empty;

        private string _tag = string.Empty;
        
        public string Tag
        {
            get
            {
                return _tag;
            }
        
            set
            {
                _tag = value;

                if (BadgeUrls != null) BadgeUrls.Tag = _tag;

                if (_members != null)
                {
                    foreach (var member in _members)
                    {
                        member.Tag = Tag;
                    }
                }
            }
        }

        public string Name { get; set; } = string.Empty;

        private BadgeUrlModel? _badgeUrls;
        
        public BadgeUrlModel? BadgeUrls
        {
            get
            {
                return _badgeUrls;
            }
        
            set
            {
                _badgeUrls = value;

                if (_badgeUrls != null) _badgeUrls.Tag = Tag;
            }
        }

        public int ClanLevel { get; set; }

        private IEnumerable<MemberAPIModel>? _members;
        
        public IEnumerable<MemberAPIModel>? Members
        {
            get
            {
                return _members;
            }
        
            set
            {
                _members = value;

                if (_members != null)
                {
                    foreach(var member in _members)
                    {
                        member.Tag = Tag;
                    }
                }
            }
        }

        [JsonPropertyName("attacks")]
        public int AttackCount { get; set; }

        public int DefenseCount { get; set; }

        public int Stars { get; set; }

        public decimal DestructionPercentage { get; set; }

        [JsonIgnore]
        public Result Result { get; set; }

    }
}
