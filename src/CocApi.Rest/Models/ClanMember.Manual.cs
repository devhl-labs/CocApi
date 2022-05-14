namespace CocApi.Rest.Models
{
    public partial class ClanMember
    {
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);
    }
}
