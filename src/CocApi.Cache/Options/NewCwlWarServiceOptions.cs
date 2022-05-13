using ScheduledServices.Services.Options;
using System.ComponentModel;

namespace CocApi.Cache.Options
{
    public class NewCwlWarServiceOptions : RecurringServiceOptions
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int ConcurrentUpdates { get; set; } = 50;
    }
}
