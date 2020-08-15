using CocApi.Cache.Models;

namespace CocApi.Cache.Models.Clans
{
    public interface IClan
    {
        string ClanTag { get; }

        string Name { get; }

        BadgeUrl? BadgeUrl { get; }
    }
}
