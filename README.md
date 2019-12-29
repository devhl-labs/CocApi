# CocApiLibrary
This .NET Standard 2.1 library will provide responses from [developer.clashofclans.com](https://developer.clashofclans.com/#/).
 
## [CocApiConsoleTest](/CocApiConsoleTest)
The test program shows you how to set up the library.
The library can grab SC API objects on command.
It can also automatically keep these objects up to date by polling the API.
When an object changes, it will fire an event which you can subscribe to.
You may optionally provide an ILogger to observe what the library is doing.
The test program will output the following:<br/><br/>
![Test Program console output](/images/console.jpg)\
If you choose to keep the objects up to date, the library will constantly write to the ILogger.
Of course, the ILogger is in your program, so you can choose what to print.

## [CocApi](/CocApiLibrary/CocApi.cs)
The CocApi class is the entry point to this library.
You likely want this class to be a singleton that lives for the life of your application.
This class is disposable, don't forget to call Dispose when you end your applicaiton.
This program has the potential to consume a lot of memory.

## [CocApiConfiguration](/CocApiLibrary/CocApiConfiguration.cs)
Use this to configure the CocApi class.
This class can control how often your SC API token is used.
You can also control how long a downloaded object is considered good for.
The default values are very conservative just in case the SC API key you provide is used for something else.
This is to avoid rate limiting your key.
If your key is only used in CocApi, you can make the time spans much shorter.
The API allows about 10 requests a second per key.

## [WebResponse](/CocApiLibrary/WebResponse.cs)
If it prints to the ILogger, that indicates that the library is polling the API.

## [IActiveWar](/CocApiLibrary/Models/War/IActiveWar.cs)
This interface is implemented by [CurrentWar](/CocApiLibrary/Models/War/CurrentWar.cs) and [LeagueWar](/CocApiLibrary/Models/War/LeagueWar.cs).
The only difference is LeagueWar has a WarTag property, and the WarType enum will be SCCWL.
When CocApi returns an IActiveWar you can cast it to the appropriate type when necessary.
LeagueWar also inherits CurrentWar.  If you have to cast, ensure you start with LeagueWar.

## IWar
This empty interface is implemented by IActiveWar and the NotInWar model.  The library will never return an IActiveWar with state = notInWar.

## ILeagueGroup
This empty interface is implemented by LeagueGroup and LeagueGroupNotFound.  Clans that are not in CWL war will return a LeagueGroupNotFound.

## [Extensions](/CocApiLibrary/Extensions.cs)
The static Extensions class contains some things that may be useful, especially for Discord bots.
DiscordSafe will strip Discord markup characters from a given string.
LeftToRight will force characters to display left to right.  This is especially helpful while making tables.
ToDateTime will convert SC API date time objects to C# DateTime, though the library already handles this when downloading your object.

## Outages
When the API goes down, the IsAvailableChanged event will fire.
CocApi will not stop trying to update expired objects unless you tell it to in this event.
If an error occurs while watching clans, the library will attempt to restart the UpdateService.

## Issues
If you have problems finding one of the required nuget packages, add https://dotnetfeed.blob.core.windows.net/dotnet-core/index.json as a source.</br></br>
If the objects update too slow or two fast, modify the properties of the CocApiConfiguration.  TokenObject rate limits are bad.  TokenObject preemptive rate limits are okay, though it does indicate the library is updating as fast as the TokenTimeOut allows. 

## Disclaimer
This content is not affiliated with, endorsed, sponsored, or specifically approved by Supercell and Supercell is not responsible for it. For more information see [Supercell's Fan Content Policy](https://supercell.com/en/fan-content-policy/).