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
        [ForeignKey(nameof(IconUrlId))]
        public virtual LeagueIconUrlAPIModel? IconUrl
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
                    IconUrlId = _iconUrl.Id;
                }
                else
                {
                    IconUrlId = null;
                }
            }
        }

        public string? IconUrlId { get; set; } = string.Empty;
    }
}
