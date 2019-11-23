namespace devhl.CocApi.Models
{
    public interface IClanApiModel
    {
        string ClanTag { get; }

        string Name { get; }

        //int ClanLevel { get; set; }

        ClanBadgeUrlApiModel? BadgeUrls { get; set; }
    }
}
