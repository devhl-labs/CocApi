using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class ClanWarMember
    {
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);


        /// <summary>
        /// Gets or Sets MapPosition
        /// </summary>
        [JsonPropertyName("mapPosition")]
        public int RosterPosition { get; private set; }

        /// <summary>
        /// Gets or Sets MapPosition
        /// </summary>
        [JsonPropertyName("mapPositionCorrected")]
        public int MapPosition { get; internal set; }
    }
}
