using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Cache.Services.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Cache.Services;

internal sealed class CacheOptionsMonitorService : IHostedService, IDisposable
{
    private readonly ILogger<CacheOptionsMonitorService> _logger;
    private readonly IOptionsMonitor<CacheOptions> _options;
    private readonly object _sync = new();
    private IDisposable? _onChangeSubscription;
    private CacheOptions? _currentOptions;

    public CacheOptionsMonitorService(
        ILogger<CacheOptionsMonitorService> logger,
        IOptionsMonitor<CacheOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _currentOptions = _options.CurrentValue;

        LogWarnings(_currentOptions, "startup");

        _onChangeSubscription = _options.OnChange((options, _) =>
        {
            CacheOptions? previous;

            lock (_sync)
            {
                previous = _currentOptions;
                _currentOptions = options;
            }

            bool hasChanges = previous == null || LogChangedValues(previous, options, "reload");

            if (hasChanges)
                LogWarnings(options, "reload");
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _onChangeSubscription?.Dispose();
        _onChangeSubscription = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _onChangeSubscription?.Dispose();
    }

    private void LogWarnings(CacheOptions options, string source)
    {
        foreach (string warning in GetWarnings(options))
        {
            _logger.LogWarning("Cache options warning ({Source}): {Warning}", source, warning);
        }
    }

    private bool LogChangedValues(CacheOptions previous, CacheOptions current, string source)
    {
        bool hasChanges = false;

        foreach ((string path, string oldValue, string newValue) in GetDifferences(previous, current))
        {
            hasChanges = true;
            _logger.LogInformation(
                "Cache option changed ({Source}): {Option} {PreviousValue} -> {CurrentValue}",
                source,
                path,
                oldValue,
                newValue);
        }

        return hasChanges;
    }

    private static IEnumerable<(string Path, string OldValue, string NewValue)> GetDifferences(CacheOptions previous, CacheOptions current)
    {
        foreach ((string path, object? oldValue, object? newValue) in GetDifferencesRecursive(previous, current, string.Empty))
            yield return (path, FormatValue(oldValue), FormatValue(newValue));
    }

    private static IEnumerable<(string Path, object? OldValue, object? NewValue)> GetDifferencesRecursive(object? previous, object? current, string prefix)
    {
        if (previous == null && current == null)
            yield break;

        if (previous == null || current == null)
        {
            yield return (prefix, previous, current);
            yield break;
        }

        Type type = previous.GetType();

        if (IsSimpleType(type))
        {
            if (!Equals(previous, current))
                yield return (prefix, previous, current);

            yield break;
        }

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
                continue;

            string path = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            object? oldValue = property.GetValue(previous);
            object? newValue = property.GetValue(current);

            Type propertyType = property.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (IsSimpleType(underlyingType))
            {
                if (!Equals(oldValue, newValue))
                    yield return (path, oldValue, newValue);

                continue;
            }

            foreach ((string nestedPath, object? nestedOldValue, object? nestedNewValue) in GetDifferencesRecursive(oldValue, newValue, path))
                yield return (nestedPath, nestedOldValue, nestedNewValue);
        }
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(Guid) ||
            type == typeof(TimeSpan);
    }

    private static string FormatValue(object? value) => value?.ToString() ?? "null";

    private static List<string> GetWarnings(CacheOptions options)
    {
        List<string> warnings = new();

        bool cwlSettingsAligned =
            options.CwlWars.Enabled ==
            options.Clans.DownloadGroup &&
            options.NewCwlWars.Enabled ==
            options.Clans.Enabled;

        if (!cwlSettingsAligned)
            warnings.Add("CWL settings are partially enabled. The following settings are recommended to be the same: CwlWars.Enabled, Clans.DownloadGroup, NewCwlWars.Enabled, and Clans.Enabled.");

        if (options.ClanMembers.Enabled != options.DeleteStalePlayers.Enabled)
            warnings.Add("Clan member tracking requires stale player cleanup. The following settings are recommended to be the same: ClanMembers.Enabled and DeleteStalePlayers.Enabled.");

        bool currentWarSettingsAligned =
            options.ActiveWars.Enabled ==
            options.Wars.Enabled &&
            options.Clans.Enabled ==
            options.ClanWars.DownloadCurrentWar &&
            options.NewWars.Enabled ==
            options.Clans.Enabled;

        if (!currentWarSettingsAligned)
            warnings.Add("Current war settings are partially enabled. The following settings are recommended to be the same: ActiveWars.Enabled, Wars.Enabled, Clans.Enabled, ClanWars.DownloadCurrentWar, and NewWars.Enabled.");

        return warnings;
    }
}
