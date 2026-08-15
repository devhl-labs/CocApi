namespace CocApi.Rest.Models
{
    public partial class ClanMember
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);
    }
}
