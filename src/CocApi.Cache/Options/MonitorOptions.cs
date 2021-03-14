namespace CocApi.Cache
{

    public class MonitorOptions : MonitorOptionsBase
    {
        public int ConcurrentUpdates { get; set; } = 50;
    }
}
