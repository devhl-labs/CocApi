using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models
{
    public class WarVillageApiModel : IVillageApiModel
    {
        // IVillageApiModel

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
        public IList<AttackApiModel>? Attacks { get; set; }

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
            if (!string.IsNullOrEmpty(_warId) && !string.IsNullOrEmpty(_villageTag))
            {
                WarVillageId = $"{_warId};{_villageTag}";
            }
        }
    }
}
