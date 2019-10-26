using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class RoundAPIModel
    {
        //public IEnumerable<string>? WarTags { get; set; }
        [Key]
        public string RoundId { get; set; } = string.Empty;

        public string GroupId { get; set; } = string.Empty;

        public virtual List<string>? WarTags { get; set; }
    }
}
