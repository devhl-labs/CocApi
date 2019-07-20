using CocApiLibrary.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class WarLogModel : IProcess
    {
        public IEnumerable<WarLogEntryModel>? Items { get; set; }

        public PagingAPIModel? Paging { get; set; }

        void IProcess.Process()
        {
            Items?.ForEach(item =>
            {
                if (item is IProcess process) process.Process();
            });
        }
    }
}
