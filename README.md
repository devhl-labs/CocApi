# CocApi
A wrapper for [developer.clashofclans.com](https://developer.clashofclans.com/#/) written in .Net 5.0. 
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
Use the IHostBuilder extension method ConfigureCocApi to add all the API endpoints to your service provider.
The TokenProvider object provides rate limiting on a per key basis.
Then you can use dependency injection to inject ClansApi, LabelsApi, LeaguesApi, LocationsApi, and PlayersApi. 
These classes allow you to query the API using the typed HttpClient provided.
```csharp
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)

        // configure how to query the Clash API
        .ConfigureCocApi((context, services, options) =>
        { 
            options.AddTokens(new TokenBuilder("your token", TimeSpan.FromMilliseconds(33)));

            options.AddCocApiHttpClients(
                builder: builder => builder

                    // only required if you use the DeveloperApi class to query, create, and delete tokens
                    .ConfigurePrimaryHttpMessageHandler(services =>
                    {
                        return new HttpClientHandler()
                        {
                            CookieContainer = services.GetRequiredService<CookieContainer>().Value
                        };
                    })

                    // optionally configure how the HttpClient handles Clash API outages
                    .AddRetryPolicy(3)
                    .AddTimeoutPolicy(TimeSpan.FromMilliseconds(3000))
                    .AddCircuitBreakerPolicy(30, TimeSpan.FromSeconds(10))

                    // this property is important if you query the api very fast
                    .ConfigurePrimaryHttpMessageHandler(sp => new SocketsHttpHandler
                    {
                        MaxConnectionsPerServer = section.GetValue<int>("MaxConnectionsPerServer")
                    })
        })
```

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
This requires that the CocApi is already added to the service provider as shown above. 
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
Downloads the clan, current war, war log, and league group for a given clan.

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

## Migrating from version 1
Internally version 1 used SQLite. If you wish to import the cache from SQLite to your database, utilize the ImportDataToVersion2 method in either the ClansClient or PlayersClient. You only need to do this once. A future release of CocApi.Cache will remove this method and all traces of SQLite.

## Logging
Logs are sent using `Microsoft.Extensions.Logging.ILogger`. The test project shows how to use Serilog, but you can use whatever you like.

## Disclaimer
This content is not affiliated with, endorsed, sponsored, or specifically approved by Supercell and Supercell is not responsible for it. For more information see [Supercell's Fan Content Policy](https://supercell.com/en/fan-content-policy/).