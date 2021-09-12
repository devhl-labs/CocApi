using System.ComponentModel;

namespace CocApi.Cache
{

    public class ServiceOptions : ServiceOptionsBase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ConcurrentUpdates { get; set; } = 50;
    }
}
