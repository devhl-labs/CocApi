using Microsoft.Extensions.Logging;

namespace CocApi.Cache.Logging;

internal static class CacheLogEvents
{
    // 1000-1099: service lifecycle
    internal static readonly EventId ServiceCycleStarted = new(1000, nameof(ServiceCycleStarted));
    internal static readonly EventId ServiceCycleEnded = new(1001, nameof(ServiceCycleEnded));
    internal static readonly EventId ServiceCycleSlow = new(1002, nameof(ServiceCycleSlow));

    // 1100-1199: cache service and common errors
    internal static readonly EventId ActiveWarUpdateFailed = new(1100, nameof(ActiveWarUpdateFailed));
    internal static readonly EventId ClanServiceUpdateFailed = new(1101, nameof(ClanServiceUpdateFailed));
    internal static readonly EventId ClanServiceWarLogUpdateFailed = new(1102, nameof(ClanServiceWarLogUpdateFailed));
    internal static readonly EventId ClanServiceGroupUpdateFailed = new(1103, nameof(ClanServiceGroupUpdateFailed));
    internal static readonly EventId ClanWarServiceUpdateFailed = new(1104, nameof(ClanWarServiceUpdateFailed));
    internal static readonly EventId ClanWarServiceFetchFailed = new(1105, nameof(ClanWarServiceFetchFailed));
    internal static readonly EventId CwlWarUpdateFailed = new(1106, nameof(CwlWarUpdateFailed));
    internal static readonly EventId FireAndForgetExecutionFailed = new(1107, nameof(FireAndForgetExecutionFailed));
    internal static readonly EventId MemberUpdateFailed = new(1108, nameof(MemberUpdateFailed));
    internal static readonly EventId NewCwlWarProcessingFailed = new(1109, nameof(NewCwlWarProcessingFailed));
    internal static readonly EventId NewCwlWarMethodFailed = new(1110, nameof(NewCwlWarMethodFailed));
    internal static readonly EventId PlayerUpdateFailed = new(1111, nameof(PlayerUpdateFailed));
    internal static readonly EventId WarUpdateFailed = new(1112, nameof(WarUpdateFailed));
    internal static readonly EventId WarComputeFailed = new(1113, nameof(WarComputeFailed));
    internal static readonly EventId SynchronizerConcurrentEventFailed = new(1114, nameof(SynchronizerConcurrentEventFailed));
    internal static readonly EventId TimeToLiveComputationFailed = new(1115, nameof(TimeToLiveComputationFailed));

    // 2000-2099: cache options monitoring
    internal static readonly EventId CacheOptionsWarning = new(2000, nameof(CacheOptionsWarning));
    internal static readonly EventId CacheOptionsChanged = new(2001, nameof(CacheOptionsChanged));
}