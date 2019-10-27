using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LeagueAPIModel
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;


        private LeagueIconUrlAPIModel? _iconUrl;
        
        [JsonPropertyName("IconUrls")]
        [ForeignKey(nameof(LeagueIconUrlId))]
        public virtual LeagueIconUrlAPIModel? LeagueIconUrl
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
