using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
////System.Text.Json.Serialization
using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class VillageLeagueApiModel
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;


        private VillageLeagueIconUrlApiModel? _iconUrl;
        
        [JsonProperty("IconUrls")]
        public VillageLeagueIconUrlApiModel? LeagueIconUrl
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
