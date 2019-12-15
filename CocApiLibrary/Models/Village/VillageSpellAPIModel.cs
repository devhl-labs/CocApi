using System.ComponentModel.DataAnnotations.Schema;

using static devhl.CocApi.Enums;

namespace devhl.CocApi.Models.Village
{
    public class VillageSpellApiModel
    {
        [ForeignKey(nameof(VillageTag))]
        public string VillageTag { get; set; } = string.Empty;

        private string _name = string.Empty;

        //[NotMapped]
        public string Name
        {
            get
            {
                return _name;
            }
        
            set
            {
                _name = value;

                //SetRelationalProperties();
            }
        }

        public int Level { get; set; }

        private int _maxLevel;

        //[NotMapped]
        public int MaxLevel
        {
            get
            {
                return _maxLevel;
            }
        
            set
            {
                _maxLevel = value;

                //SetRelationalProperties();
            }
        }

        private VillageType _village;

        //[NotMapped]  
        public VillageType Village
        {
            get
            {
                return _village;
            }
        
            set
            {
                _village = value;

                //SetRelationalProperties();
            }
        }

        //[ForeignKey(nameof(SpellId))]
        //public SpellModel SpellModel { get; set; } = new SpellModel();
        
        //[Key]
        //public string SpellId
        //{
        //    get
        //    {
        //        return $"{Village.ToString()};{Name}";
        //    }

        //    set
        //    {
        //    }
        //}

        //private void SetRelationalProperties()
        //{
        //    if (MaxLevel != 0)
        //    {
        //        SpellModel.MaxLevel = MaxLevel;
        //    }
        //    else if(SpellModel.MaxLevel != 0)
        //    {
        //        MaxLevel = SpellModel.MaxLevel;
        //    }

        //    if (!string.IsNullOrEmpty(Name))
        //    {
        //        SpellModel.Name = Name;
        //    }
        //    else if (!string.IsNullOrEmpty(SpellModel.Name))
        //    {
        //        Name = SpellModel.Name;
        //    }

        //    if (Village != VillageType.Unknown)
        //    {
        //        SpellModel.Village = Village;
        //    }
        //    else if (SpellModel.Village != VillageType.Unknown)
        //    {
        //        Village = SpellModel.Village;
        //    }
        ////}
    }
}
