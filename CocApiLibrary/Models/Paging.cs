using Newtonsoft.Json;

namespace devhl.CocApi.Models
{
    public class Paging
    {
        [JsonProperty]
        public Cursor? Cursors { get; }
    }
}
