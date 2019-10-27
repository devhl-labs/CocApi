using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class WarVillageAPIModel : IVillageAPIModel
    {
        // IVillageAPIModel

        private string _villageTag = string.Empty;

        [JsonPropertyName("Tag")]
        public string VillageTag
        {
            get
            {
                return _villageTag;
            }
        
            set
            {
                _villageTag = value;

                SetRelationalProperties();
            }
        }

        [NotMapped]
        public string Name { get; set; } = string.Empty;

        public string ClanTag { get; set; } = string.Empty;







        [Key]
        public string WarVillageId { get; set; } = string.Empty;

        public int TownhallLevel { get; set; }

        public int MapPosition { get; set; }

        private string _warClanId = string.Empty;

        public string WarClanId
        {
            get
            {
                return _warClanId;
            }

            set
            {
                _warClanId = value;

                SetRelationalProperties();
            }
        }

        [NotMapped]
        public IList<AttackAPIModel>? Attacks { get; set; }

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


        private void SetRelationalProperties()
        {
            if (_warId != null && _villageTag != null)
            {
                WarVillageId = $"{_warId};{_villageTag}";
            }
        }
    }
}
