using Newtonsoft.Json;


namespace devhl.CocApi.Models.Village
{
    public class VillageLabelBuilder
    {
        public string VillageTag { get; set; } = string.Empty;

        public int Id { get; internal set; }

        public VillageLabel Build()
        {
            return new VillageLabel
            {
                VillageTag = VillageTag,
                Id = Id
            };
        }


    }
}
