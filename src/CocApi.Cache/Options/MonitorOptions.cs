using System.ComponentModel;
using System.Diagnostics;

namespace CocApi.Cache
{

    public class MonitorOptions : MonitorOptionsBase
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ConcurrentUpdates { get; set; } = 50;
    }
}
