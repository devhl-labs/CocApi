using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.Clan
{
    public class ClanLabelApiModel : LabelApiModel
    {
        public string ClanTag { get; set; } = string.Empty;
    }
}
