﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace devhl.CocApi.Models
{
    public class VillageLabelApiModel : LabelApiModel
    {
        public string VillageTag { get; set; } = string.Empty;

//        public int Id { get; set; }

//        private string _name = string.Empty;
        
//        [NotMapped]
//        public string Name
//        {
//            get
//            {
//                return _name;
//            }
        
//            set
//            {
//                _name = value;

//                SetRelationalProperties();
//            }
//        }



//        private LabelUrlApiModel? _imageUrl;

//        [JsonPropertyName("IconUrls")]
//        [ForeignKey(nameof(Id))]
//        public virtual LabelUrlApiModel? LabelUrl
//        {
//            get
//            {
//                return _imageUrl;
//            }
        
//            set
//            {
//                _imageUrl = value;

//                SetRelationalProperties();
//            }
//        }

//        private void SetRelationalProperties()
//        {
//            if (!string.IsNullOrEmpty(_name) && LabelUrl != null)
//            {
//                LabelUrl.Name = _name;
//            }

//#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
//            if (Id != null && LabelUrl != null)
//#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
//            {
//                LabelUrl.Id = Id;
//            }
//        }
    }
}