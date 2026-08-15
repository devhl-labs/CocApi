namespace CocApi.Rest.Models
{
    public partial class ClanWarMember
    {
        partial void OnCreated()
        {
            RosterPosition = MapPosition;
        }

        [System.Text.Json.Serialization.JsonIgnore]
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);

        /// <summary>
        /// The member's position in the full CWL war roster (e.g. 1–50).
        /// In regular wars where all members participate, equals <see cref="WarPosition"/>.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public int RosterPosition { get; private set; }

        /// <summary>
        /// The member's sequential position in the current war (1-N).
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public int WarPosition { get; internal set; }
    }
}
