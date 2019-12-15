using System.Text.Json.Serialization;

namespace devhl.CocApi.Models.War
{
    public class LeagueWarApiModel : CurrentWarApiModel, IInitialize, ICurrentWarApiModel
    {
        public string WarTag { get; set; } = string.Empty;

        public new void Initialize()
        {
            base.Initialize();
        }
    }
}
