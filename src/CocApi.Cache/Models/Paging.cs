using Newtonsoft.Json;

namespace CocApi.Cache.Models
{
    public class Paging
    {
        [JsonProperty]
        public Cursor? Cursors { get; private set; }
    }
}
