namespace devhl.CocApi.Models.Village
{
    public class SpellBuilder
    {
        public string VillageTag { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int Level { get; set; }

        public VillageType Village { get; set; }

        public override string ToString() => Name;

        public Spell Build()
        {
            return new Spell
            {
                VillageTag = VillageTag,
                Name = Name,
                Level = Level,
                Village = Village
            };
        }
    }
}
