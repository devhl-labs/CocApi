using System.ComponentModel.DataAnnotations.Schema;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class VillageSpellApiModel
    {
        public string VillageTag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public int MaxLevel { get; set; }

        public VillageType Village { get; set; }
    }
}
