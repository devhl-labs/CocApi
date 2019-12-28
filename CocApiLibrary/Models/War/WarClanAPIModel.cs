using devhl.CocApi.Models.Clan;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
////System.Text.Json.Serialization
using static devhl.CocApi.Enums;
using Newtonsoft.Json;

namespace devhl.CocApi.Models.War
{
    public class WarClanApiModel : IClanApiModel
    {
        // IClanApiModel
        [JsonProperty("Tag")]
        public string ClanTag
        {
            get
            {
                return _clanTag;
            }
        
            set
            {
                _clanTag = value;

                if (_villages != null)
                {
                    foreach (var village in _villages)
                    {
                        village.ClanTag = ClanTag;
                    }
                }

                SetRelationalProperties();
            }
        }


        public string Name { get; set; } = string.Empty;


        public ClanBadgeUrlApiModel? BadgeUrls { get; set; }










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

        private IEnumerable<WarVillageApiModel>? _villages;

        [JsonProperty("members")]
        public IEnumerable<WarVillageApiModel>? Villages
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
                        village.ClanTag = ClanTag;
                    }
                }
            }
        }

        [JsonProperty("attacks")]
        public int AttackCount { get; set; }

        public int DefenseCount { get; set; }

        public int Stars { get; set; }

        public decimal DestructionPercentage { get; set; }


        public string WarClanId { get; set; } = string.Empty;

        public Result Result { get; set; }

        private void SetRelationalProperties()
        {
            if (!string.IsNullOrEmpty(_warId) && !string.IsNullOrEmpty(_clanTag))
            {
                WarClanId = $"{_warId};{_clanTag}";
            }
        }
    }
}
