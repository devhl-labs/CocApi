namespace CocApi.Test;

public class UtcTimestampEnricher : Serilog.Core.ILogEventEnricher
{
    public void Enrich(Serilog.Events.LogEvent logEvent, Serilog.Core.ILogEventPropertyFactory factory)
    {
        logEvent.AddPropertyIfAbsent(
            factory.CreateProperty("UtcTimestamp", logEvent.Timestamp.UtcDateTime));
    }
}
