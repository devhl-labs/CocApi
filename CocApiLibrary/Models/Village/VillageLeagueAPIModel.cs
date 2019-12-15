using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Village
{
    public class VillageLeagueApiModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;


        private VillageLeagueIconUrlApiModel? _iconUrl;
        
        [JsonPropertyName("IconUrls")]
        //[JsonIgnore]
        [ForeignKey(nameof(LeagueIconUrlId))]
        public virtual VillageLeagueIconUrlApiModel? LeagueIconUrl
        {
            get
            {
                return _iconUrl;
            }
        
            set
            {
                _iconUrl = value;

                if (_iconUrl != null)
                {
                    LeagueIconUrlId = _iconUrl.Id;
                }
                else
                {
                    LeagueIconUrlId = null;
                }
            }
        }

        public string? LeagueIconUrlId { get; set; } = string.Empty;
    }
}
