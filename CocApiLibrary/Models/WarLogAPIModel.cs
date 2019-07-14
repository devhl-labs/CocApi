using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class WarLogModel
    {
        public IEnumerable<WarLogEntryModel>? Items { get; set; }

        public PagingAPIModel? Paging { get; set; }

        internal void Process()
        {
            Items?.ForEach(item =>
            {
                item.Process();
            });
        }
    }
}
