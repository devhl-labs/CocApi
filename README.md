# CocApi
A wrapper for [developer.clashofclans.com](https://developer.clashofclans.com/#/) written in .Net Standard 2.1.
 
## Help  
![Discord Banner 2](https://discordapp.com/api/guilds/701245583444279328/widget.png?style=banner2)

## Using the library
```
Clan? queued = cocApi.Clans.Get("#clanTag");
Clan? queuedAsync = await cocApi.Clans.GetAsync("#clanTag");
Clan? fetchedAsync = await cocApi.Clans.FetchAsync("#clanTag");
```
The queued clan will contain whatever is already stored in memory.
The queuedAsync clan will return what is in memory.  If nothing is found or if the object is expired, it will query the API.
The fetchedAsync clan will only return what the API returns.

## [Configuring CocApi](/CocApiConsoleTest/Program.cs)
The test program shows you how to set up the library.
The library can grab SC API objects on command.
It can also automatically keep these objects up to date by polling the API.
When an object changes, it will fire an event which you can subscribe to.
There are also a few static methods in this class that may be useful.
The test program will output the following:<br/><br/>
![Test Program console output](/images/console.jpg)\

## IWar
This empty interface is implemented by [CurrentWar](/CocApiLibrary/Models/War/CurrentWar.cs), [LeagueWar](/CocApiLibrary/Models/War/LeagueWar.cs), [NotInWar](/CocApiLibrary/Models/War/NotInWar.cs) and [PrivateWarLog](/CocApiLibrary/Models/War/PrivateWarLog.cs).
When CocApi returns an IWar, you can cast it to the appropriate type when necessary.
LeagueWar also inherits CurrentWar.  If you have to cast, ensure you start with LeagueWar.

## ILeagueGroup
This empty interface is implemented by LeagueGroup and LeagueGroupNotFound.

## The Log Event
Log events with a source of WebResponse represent a request sent to the API.
Log events with a LogLevel of Trace are only for troubleshooting issues in the library.
LogEventArgs may be cast to ClanEventArgs, CurrentWarEventArgs, ExceptionEventArgs, or VillageEventArgs.

## Outages
When the API goes down, the IsAvailableChanged event will fire.
CocApi will not stop trying to update expired objects.
If an error occurs while updating an object, the library will attempt to restart the queue.

## Disclaimer
This content is not affiliated with, endorsed, sponsored, or specifically approved by Supercell and Supercell is not responsible for it. For more information see [Supercell's Fan Content Policy](https://supercell.com/en/fan-content-policy/).