using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LeagueIconUrlAPIModel
    {
        [Key]
        public string Id { get; set; } = string.Empty;


        private string _medium = string.Empty;
        
        public string Medium
        {
            get
            {
                return _medium;
            }
        
            set
            {
                _medium = value;

                Id = _medium.Split("/").Last();
            }
        }

        public string Small { get; set; } = string.Empty;

        public string Tiny { get; set; } = string.Empty;
    }
}
