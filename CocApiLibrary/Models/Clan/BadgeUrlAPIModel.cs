using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class BadgeUrlModel
    {
        //[Key]
        //public string ClanTag { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;

        private string _small = string.Empty;
        
        public string Small
        {
            get
            {
                return _small;
            }
        
            set
            {
                _small = value;

                Id = _small.Split("/").Last();
            }
        }

        public string Large { get; set; } = string.Empty;

        public string Medium { get; set; } = string.Empty;
    }
}
