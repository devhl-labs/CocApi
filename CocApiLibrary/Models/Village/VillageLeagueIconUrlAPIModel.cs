﻿using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace devhl.CocApi.Models
{
    public class VillageLeagueIconUrlApiModel
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

                Id = SetId(_medium);
            }
        }

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

                Id = SetId(_small);
            }
        }

        private string _tiny = string.Empty;
        
        public string Tiny
        {
            get
            {
                return _tiny;
            }
        
            set
            {
                _tiny = value;

                Id = SetId(_tiny);
            }
        }

        private string SetId(string str) => str.Split("/").Last();       
    }
}