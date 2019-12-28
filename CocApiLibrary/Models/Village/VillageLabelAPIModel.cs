using System.ComponentModel.DataAnnotations.Schema;
//System.Text.Json.Serialization

namespace devhl.CocApi.Models.Village
{
    public class VillageLabelApiModel : LabelApiModel
    {
        public string VillageTag { get; set; } = string.Empty;
    }
}
