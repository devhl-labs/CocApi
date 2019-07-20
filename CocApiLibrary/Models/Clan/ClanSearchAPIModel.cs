﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class ClanSearchModel
    {
        public IEnumerable<ClanAPIModel>? Items { get; set; }

        public PagingAPIModel? Paging { get; set; }
    }
}