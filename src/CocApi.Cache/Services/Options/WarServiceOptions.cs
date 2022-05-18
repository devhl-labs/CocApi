using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Services.Options
{
    public class WarServiceOptions : RecurringServiceOptions
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ConcurrentUpdates { get; set; } = 50;
    }
}
