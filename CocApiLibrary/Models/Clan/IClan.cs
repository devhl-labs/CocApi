namespace devhl.CocApi.Models.Clan
{
    public interface IClan
    {
        string ClanTag { get; }

        string Name { get; }

        BadgeUrl? BadgeUrl { get; }
    }
}
