using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class TroopBuilder
    {
        public string VillageTag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public VillageType Village { get; set; }

        public bool IsHero { get; set; }   

        public override string ToString() => Name;

        public Troop Build() 
        {
            return new Troop
            {
                VillageTag = VillageTag,
                Name = Name,
                Level = Level,
                Village = Village,
                IsHero = IsHero
            };
        }
    }
}
