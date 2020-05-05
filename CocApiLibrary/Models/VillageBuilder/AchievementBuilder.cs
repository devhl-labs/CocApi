using Newtonsoft.Json;

namespace devhl.CocApi.Models.Village
{
    public class AchievementBuilder
    {

        public string VillageTag { get; set; } = string.Empty;

        public int Stars { get; set; }

        public int Value { get; set; }

        public int Target { get; set; }

        //public string CompletionInfo { get; set; } = string.Empty;

        public VillageType Village { get; set; }

        public string Name { get; set; } = string.Empty;

        //public string Info { get; set; } = string.Empty;

        public override string ToString() => Name;

        public Achievement Build()
        {
            return new Achievement
            {
                VillageTag = VillageTag,
                Stars = Stars,
                Value = Value,
                Target = Target,
                //CompletionInfo = CompletionInfo,
                Village = Village,
                Name = Name,
                //Info = Info
            };
        }
    }
}
