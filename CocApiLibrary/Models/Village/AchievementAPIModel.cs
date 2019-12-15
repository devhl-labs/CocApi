using System.ComponentModel.DataAnnotations.Schema;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class AchievementApiModel
    {
        [ForeignKey(nameof(VillageTag))]
        public string VillageTag { get; set; } = string.Empty;

        private string _name = string.Empty;
        
        public string Name
        {
            get
            {
                return _name;
            }
        
            set
            {
                _name = value;

                UniquesNames();
            }
        }

        public int Stars { get; set; }

        public int Value { get; set; }

        public int Target { get; set; }

        private string _info = string.Empty;
        
        public string Info
        {
            get
            {
                return _info;
            }
        
            set
            {
                _info = value;

                UniquesNames();
            }
        }

        public string CompletionInfo { get; set; } = string.Empty;

        public VillageType Village { get; set; }

        private void UniquesNames()
        {
            if (Info == "Connect your account to Supercell ID for safe keeping.")
            {
                _name = "Keep your village safe2";
            }
        }
    }
}
