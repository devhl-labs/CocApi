using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class WarClanAPIModel : IClanAPIModel
    {
        // IClanAPIModel
        [JsonPropertyName("Tag")]
        public string ClanTag
        {
            get
            {
                return _clanTag;
            }
        
            set
            {
                _clanTag = value;

                //if (BadgeUrls != null) BadgeUrls.ClanTag = _tag;

                if (_villages != null)
                {
                    foreach (var village in _villages)
                    {
                        village.VillageTag = ClanTag;
                    }
                }

                SetRelationalProperties();
            }
        }

        [NotMapped]
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public BadgeUrlModel? BadgeUrls { get; set; }

        [NotMapped]
        public int ClanLevel { get; set; }








        private string _warId = string.Empty;
        
        public string WarId
        {
            get
            {
                return _warId;
            }
        
            set
            {
                _warId = value;

                SetRelationalProperties();
            }
        }

        private string _clanTag = string.Empty;    

        private IEnumerable<WarVillageAPIModel>? _villages;

        [ForeignKey(nameof(WarClanId))]
        public virtual IEnumerable<WarVillageAPIModel>? Villages
        {
            get
            {
                return _villages;
            }

            set
            {
                _villages = value;

                if (_villages != null)
                {
                    foreach (var village in _villages)
                    {
                        village.VillageTag = ClanTag;
                    }
                }
            }
        }

        [JsonPropertyName("attacks")]
        public int AttackCount { get; set; }

        public int DefenseCount { get; set; }

        public int Stars { get; set; }

        public decimal DestructionPercentage { get; set; }

        [Key]
        public string WarClanId { get; set; } = string.Empty;

        [JsonIgnore]
        public Result Result { get; set; }

        private void SetRelationalProperties()
        {
            if (_warId != null && _clanTag != null)
            {
                WarClanId = $"{_warId};{_clanTag}";
            }
        }
    }
}
