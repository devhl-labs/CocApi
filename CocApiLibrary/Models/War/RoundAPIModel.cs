using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace devhl.CocApi.Models.War
{
    public class RoundApiModel
    {
        //public IEnumerable<string>? WarTags { get; set; }
        [Key]
        public string RoundId { get; set; } = string.Empty;

        public string GroupId { get; set; } = string.Empty;

        public virtual List<string>? WarTags { get; set; }
    }
}
