using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Models
{
    public class LabelAPIModel
    {
        public string Name { get; set; } = string.Empty;

        public int Id { get; set; }

        public IconUrlsAPIModel? IconUrls { get; set; }
    }
}
