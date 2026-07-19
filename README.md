# CocApi
A wrapper for [developer.clashofclans.com](https://developer.clashofclans.com/#/) written in .Net 8.0.
CocApi is used to fetch results from the clash API.
CocApi.Cache will cache responses in a database of your choice.
To keep objects up to date and receive events when the data changes, add the tag to the ClansClient or PlayersClient.

## Help
[![Clash API Developers](https://discordapp.com/api/guilds/566451504332931073/widget.png?style=banner4)](https://discord.gg/clashapi)

## Using the library
```csharp
// query the api
Clan clan = await clansApi.FetchAsync("#clanTag");

// query your cache or fall back to the api
Clan clan = await clansClient.GetOrFetchClanAsync("#clanTag");
```

## Helper Methods
Use the static class CocApi.Clash for various helpers including FormatTag, TryFormatTag, an array of all troop information, and parsing army links. The Clan class has static methods Clan.Donations, Clan.ClanMembersLeft, and Clan.ClanMembersJoined. The ClanWar class has static method ClanWar.NewAttacks. These clan and war methods are useful in the update events.

## Configuring CocApi
Use the IHostBuilder extension method `ConfigureCocApi` to add all the API endpoints to your service provider.
Then inject `IClansApi`, `ILabelsApi`, `ILeaguesApi`, `ILocationsApi`, or `IPlayersApi` where needed.

By default, tokens and HTTP client settings are read from configuration (see [Configuration](#configuration) below).
```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureCocApi()
```

To customize tokens or HTTP client behaviour at registration time:
```csharp
Host.CreateDefaultBuilder(args)
    .ConfigureCocApi((context, options) =>
    {
        // Override the config-based token loading
        options.AddTokens(new ApiKeyToken("your token", ClientUtils.ApiKeyHeader.Authorization));

        // Provide your own builder to fully replace the default pipeline
        // (retry, timeout, circuit breaker, handler). Omitting this uses the defaults.
        options.AddCocApiHttpClients(builder =>
        {
            builder
                .AddRetryPolicy(3)
                .AddTimeoutPolicy(TimeSpan.FromMilliseconds(3000))
                .AddCircuitBreakerPolicy(30, TimeSpan.FromSeconds(10));
        });
    })
```

## Configuration

The following keys are read from `appsettings.json` (or any `IConfiguration` source). Tokens are required; all other keys are optional with sensible defaults.

| Key | Default | Description |
|---|---|---|
| `CocApi:Rest:Tokens` | *(required)* | Array of Clash of Clans API tokens |
| `CocApi:Rest:HttpClient:TokenTimeout` | `33` ms | How long a token can be held before being returned to the pool |
| `CocApi:Rest:HttpClient:Retries` | `1` | Polly retry attempts on transient failures |
| `CocApi:Rest:HttpClient:Timeout` | `1500` ms | Per-request timeout |
| `CocApi:Rest:HttpClient:HandledEventsAllowedBeforeBreaking` | `5` | Failures before the circuit breaker opens |
| `CocApi:Rest:HttpClient:DurationOfBreak` | `30` s | How long the circuit breaker stays open |
| `CocApi:Rest:HttpClient:MaxConnectionsPerServer` | `100` | Maximum simultaneous TCP connections to the API |

## Configuring CocApi.Cache
Before running you will need to [create a migration](docs/scripts/cocapi-ef-migration.ps1) 
and run the [update](docs/scripts/cocapi-ef-update.ps1) on your database provider.
Modify these scripts to suit your needs. At the least you should edit the migration script to inject your connection string.
```ps1
dotnet ef migrations add YourMigrationName `
    --project $PSScriptRoot/YourProject `
    --context CocApi.Cache.CacheDbContext `
    -o ./OutputFolder
```

Use the IHostBuilder extension method ConfigureCocApiCache to your service provider.
This requires CocApi to already be added to the service provider as shown above. 
```csharp
.ConfigureCocApiCache<CustomClansClient, CustomPlayersClient, CustomTimeToLiveProvider>((services, dbContextOptions) =>
{
    IConfiguration configuration = services.GetRequiredService<IConfiguration>();

    string connection = configuration.GetConnectionString("your connection string");

    dbContextOptions.UseNpgsql(connection);
})
```

## Background Services
### ActiveWarService
Downloads the current war for a clan which is warring one of your tracked clans, but otherwise would not be downloaded. This ensures the most recent data is available. It may help if a tracked clan's war log is private. It also helps get the final war stats in the event the clan searches for a new war immediately.

### ClanService
Downloads the clan, war log, and league group for a given clan.

### ClanWarService
Downloads the current war.

### ClanMemberService
Iterates the Clan cache table searching for any clan with DownloadMembers enabled. Every player present in the clan will be downloaded. Players added to the Players table by this service will have Download set to **false**. When the village leaves the tracked clan, it will no longer update and will eventually be removed from the cache. If you wish to continue tracking these villages, on the OnClanUpdated event check for new members using `Clan.ClanMembersJoined(e.Stored, e.Fetched)` and add them to the PlayersClient with Download set to true.

### CwlWarService
Iterates over the Wars cache table for any war with a war tag. Then queries the API and fires any appropriate updates.

### NewCwlWarService
Queries the clan's league group from the cache to obtain the war tags. The API is then queried for each war tag. If the resulting war does not contain the desired clan, the war will be stored in memory. If the resulting war does contain the desired clan the war the NewWar event will be fired.

### NewWarService
Queries the current war cache for any war not yet announced. Fires the NewWar event, and adds the war to the War table.

### PlayerService
Iterates the Players cache table searching for players with Download set to true.

### WarService
Iterates over the Wars cache table. Queries the CurrentWar cache table for both clans in the war. Takes the most recent of the two, checks if any changes have been downloaded, and fires the appropriate events.

## Migrating from v2 to v3

### Target framework
v3 targets **.NET 10**. Update your project's `TargetFramework` to `net10.0`.

### Token registration
Tokens are now loaded automatically from `IConfiguration`. Remove any code that explicitly registered tokens and add them to your configuration source instead:
```json
{
  "CocApi": {
    "Rest": {
      "Tokens": [ "your-token-here" ]
    }
  }
}
```
If you prefer to keep tokens in code, pass them through the options callback:
```csharp
.ConfigureCocApi((context, options) =>
{
    options.AddTokens(new ApiKeyToken("your-token", ClientUtils.ApiKeyHeader.Authorization));
})
```

### `ConfigureCocApi` callback signature
The callback signature changed from `(HostBuilderContext, IServiceCollection, HostConfiguration)` to `(HostBuilderContext, HostConfiguration)`. Remove the `IServiceCollection` parameter and use a separate `ConfigureServices` call for any service registrations you were doing inside the callback.

### `AddCocApiHttpClients` and the default pipeline
v3 ships a default HTTP client pipeline (retry, timeout, circuit breaker, connection limit) that is applied automatically when you do not call `AddCocApiHttpClients`. If you were calling it in v2 with a partial set of policies, your existing call will continue to work — it still sets exactly what you specify. The only new behaviour is that omitting the call now gives you a sensible pipeline instead of a bare `HttpClient`.

### Custom `TimeToLiveProvider`
`NoChangeTimeToLiveAsync<T>(T item, DateTime? lastChangedAt)` is a new optional override on `TimeToLiveProvider`. Implement it to back off polling for stale objects — return a longer TTL when `lastChangedAt` indicates the object has not changed for an extended period. Return values from a fixed set of `TimeSpan`s (e.g. 5 min, 15 min, 1 hr) rather than a unique value per object.

### `CacheOptions` and live monitoring
`CacheOptions` is now monitored via `IOptionsMonitor`, so polling intervals and concurrency limits update at runtime without a restart. If you were mutating options objects directly after startup, switch to updating configuration instead.

### Logging categories and event IDs
All background services now log under structured categories matching the class name (e.g. `CocApi.Cache.Services.ClanService`) and include `EventId`s starting at 1000. You can use these in your logging configuration to filter or sink specific services or event types — for example, to alert only on slow cycles (`EventId` 1002) or to suppress verbose output from a single service.

## Disclaimer
This content is not affiliated with, endorsed, sponsored, or specifically approved by Supercell and Supercell is not responsible for it. For more information see [Supercell's Fan Content Policy](https://supercell.com/en/fan-content-policy/).