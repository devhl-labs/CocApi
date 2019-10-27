using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class VillageLeagueAPIModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;


        private VillageLeagueIconUrlAPIModel? _iconUrl;
        
        [JsonPropertyName("IconUrls")]
        [ForeignKey(nameof(LeagueIconUrlId))]
        public virtual VillageLeagueIconUrlAPIModel? LeagueIconUrl
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
