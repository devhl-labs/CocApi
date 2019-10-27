using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class VillageLabelAPIModel
    {
        public string VillageTag { get; set; } = string.Empty;

        public int Id { get; set; }

        private string _name = string.Empty;
        
        [NotMapped]
        public string Name
        {
            get
            {
                return _name;
            }
        
            set
            {
                _name = value;

                SetRelationalProperties();
            }
        }



        private LabelUrlAPIModel? _imageUrl;

        [JsonPropertyName("IconUrls")]
        [ForeignKey(nameof(Id))]
        public virtual LabelUrlAPIModel? ImageUrl
        {
            get
            {
                return _imageUrl;
            }
        
            set
            {
                _imageUrl = value;

                SetRelationalProperties();
            }
        }

        private void SetRelationalProperties()
        {
            if (!string.IsNullOrEmpty(Name) && ImageUrl != null)
            {
                ImageUrl.Name = Name;
            }

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            if (Id != null && ImageUrl != null)
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            {
                ImageUrl.Id = Id;
            }
        }
    }
}
