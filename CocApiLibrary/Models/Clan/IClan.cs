namespace devhl.CocApi.Models.Clan
{
    public interface IClan : IInitialize
    {
        string ClanTag { get; }

        string Name { get; }

        BadgeUrl? BadgeUrl { get; }
    }
}
