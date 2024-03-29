using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class WarClan
    {
        public string ClanProfileUrl => Clash.ClanProfileUrl(Tag);


        [JsonPropertyName("result")]
        public Result? Result { get; internal set; }
    }
}
