//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Text;
//using static CocApiLibrary.Enums;

//namespace CocApiLibrary.Models
//{
//    public class SpellModel
//    {
//        public string Name { get; set; } = string.Empty;

//        public int MaxLevel { get; set; }

//        public VillageType Village { get; set; }
        

//        //private string _spellId = string.Empty;

//        [Key]
//        public string SpellId
//        {
//            get
//            {
//                return $"{Village.ToString()};{Name}";
//            }
        
//            set
//            {
//                //_spellId = value;
//            }
//        }






//        //{
//        //    get
//        //    {
//        //        return $"{Village.ToString()};{Name}";
//        //    }

//        //}

//    }
//}
