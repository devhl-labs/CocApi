# CocApi
A wrapper for [developer.clashofclans.com](https://developer.clashofclans.com/#/) written in .Net 5.0. 
CocApi is used to fetch results from the clash API. 
CocApi.Cache will cache responses in a database of your choice.
To keep objects up to date automatically and receive events on updates, add the tag to the village or clan ClientBase object.
 
## Help  
![Discord Banner 2](https://discordapp.com/api/guilds/701245583444279328/widget.png?style=banner2)

## Using the library
```
// fetch commands directly query the api
Clan clan = await clansApi.FetchAsync("#clanTag");

// get commands query your cache database
Clan clan = await clansClient.GetOrFetchClanAsync("#clanTag");
```

## CocApi.Clash
Use the static class CocApi.Clash for various helpers including FormatTag, TryFormatTag, an array of all troop information, and parsing army links.

## Configuring CocApi
Use the IHostBuilder extension method ConfigureCocApi to add all the API endpoints to your service provider.
The TokenProvider object provides rate limiting on a per key basis.
Then you can use dependency injection to inject ClansApi, LabelsApi, LeaguesApi, LocationsApi, and PlayersApi. 
These classes allow you to query the API using the named HttpClient provided.
```
c#
private static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
	.ConfigureCocApi("cocApi", tokenProvider => { /* todo */ })
	.ConfigureServices((hostBuilder, services) => 
	{
	    services.AddHttpClient("cocApi, httpClient =>
		{
			httpClient.BaseAddress = new Uri("https://api.clashofclans.com/v1");
			httpClient.Timeout = TimeSpan.FromSeconds(3);
		})
	}
```

## Configuring CocApi.Cache
Create a class that implements `IDesignTimeDbContextFactory<CocApi.Cache.CocApiCacheContext>`.
This tells the CocApi.Cache how to interact with your database.
Before running you will need to [create a migration](docs/scripts/cocapi-ef-migration.ps1) 
and run the [update](docs/scripts/cocapi-ef-update.ps1) on your database provider.
Modify these scripts to suit your needs. At the least you should edit the migration script to inject your connection string.
```
ps1
dotnet ef migrations add YourMigrationName `
    --project $PSScriptRoot/YourProject `
    --context CocApi.Cache.CocApiCacheContext `
    -o ./OutputFolder `
    -- Your connection string or environment variable that will be injected into your IDesignTimeDbContextFactory.CreateDbContext args parameter
```

It is not required but recommended to create two classes that inherit ClansClientBase and PlayersClientBase from CocApi.Cache.
You probably also want these classes to implement IHostedService if you want to add code to StartAsync or StopAsync.
There are virtual TimeToLiveAsync methods which dictate how long an api response is stored for. 
There are also virtual HasUpdated methods which dictate if the object downloaded from the api has changed compared to the previous download.
When this method returns true, the new data will be written to the disk. If you don't care about the properties that changed, 
return false to prevent unnecessary writes to the hard drive.

Use the IHostBuilder extension method ConfigureCocApiCache to your service provider.
This requires that the CocApi is already added to the service provider as shown above. 
```
c#
// providing types ClansClient and PlayersClient is optional
.ConfigureCocApiCache<ClansClient, PlayersClient>(                
    // tell the cache library how to query your database
    provider => 
	{
		provider.Factory = new YourDbContextFactory();
		
		// use this method to inject your connection string from appsettings.json
		provider.DbContextArgs = new string[] { hostBuilder.Configuration.GetValue<string>("CocApi:Cache:ConnectionString") };
	},
        clansClient => {
            clansClient.ActiveWars.Enabled = false;
            clansClient.ClanMembers.Enabled = false;
            clansClient.Clans.Enabled = true;
            clansClient.NewCwlWars.Enabled = true;
            clansClient.NewWars.Enabled = false;
            clansClient.Wars.Enabled = false;
            clansClient.CwlWars.Enabled = false;
        },
        playersClient => playersClient.Enabled = false)
```

## Logging
Hook into CocApi.Library.HttpRequestResult and CocApi.Cache.Library.Log to receive events from the libraries.

## Disclaimer
This content is not affiliated with, endorsed, sponsored, or specifically approved by Supercell and Supercell is not responsible for it. For more information see [Supercell's Fan Content Policy](https://supercell.com/en/fan-content-policy/).