namespace devhl.CocApi.Models.Village
{
    public interface IVillage : IInitialize
    {
        string VillageTag { get; }

        string Name { get; }

        string ClanTag { get; }
    }
}
