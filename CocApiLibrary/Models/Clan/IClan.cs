namespace devhl.CocApi.Models.Clan
{
    public interface IClan : IInitialize
    {
        string ClanTag { get; }

        string Name { get; }

        ClanBadgeUrl? BadgeUrls { get; }
    }
}
