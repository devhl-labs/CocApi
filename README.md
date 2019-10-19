# CocApiLibrary
This .NET Standard 2.1 library will provide responses from developer.clashofclans.com\
This is a rewrite of the class library Minion Bot uses.
 
## [CocApiConsoleTest](/CocApiConsoleTest)
The test program shows you how to set up the library.\
The library can grab SC API objects on command.\
It can also automatically keep these objects up to date by polling the API.\
When an object changes, it will fire an event which you can subscribe to.\
You may optionally provide an ILogger to observe what the library is doing.\
The test program will output the following:<br/><br/>
![Test Program console output](https://github.com/devhl-labs/CocApi/blob/controlsloop/CocApiConsoleTest/images/console.jpg)\
If you choose to keep the objects up to date, the library will constantly write to the ILogger.\
Of course, the ILogger is in your program, so you can choose what to print.

## [CocApi](/CocApiLibrary/CocApi.cs)
The CocApi class is the entry point to this library.\
You likely want this class to be a singleton that lives for the life of your application.\
This class is disposable, don't forget to call Dispose when you end your applicaiton.\
This program has the potential to consume a lot of memory.

## [CocApiConfiguration](/CocApiLibrary/CocApiConfiguration.cs)
Use this to configure the CocApi class.\
This class can control how often your SC API token is used.\
You can also control how long a downloaded object is considered good for.\
The default values are very conservative just in case the SC API key you provide is used for something else.\
This is to avoid rate limiting your key.\
If your key is only used in CocApi, you can make the time spans much shorter.\
The API allows about 10 requests a second per key.

## [ICurrentWarAPIModel](/CocApiLibrary/Models/War/ICurrentWarAPIModel.cs)
This interface is implemented by [CurrentWarAPIModel](/CocApiLibrary/Models/War/CurrentWarAPIModel.cs) and [LeagueWarAPIModel](/CocApiLibrary/Models/War/LeagueWarAPIModel.cs).\
The only difference is LeagueWarAPIModel has a WarTag property, and the WarType enum will be SCCWL.\
When CocApi returns an ICurrentWarAPIModel, you can cast it to the appropriate type when necessary.

## [WebResponse](/CocApiLibrary/WebResponse.cs)
This is an internal static class.\
If it prints to the ILogger, that indicates that the library is polling the API.

## [UpdateService](/CocApiLibrary/UpdateService.cs)
This is an internal class.  It is only public so it can inherit a public abstract class.
If it prints to the ILogger, it is updating an object.  If the object being updated is not expired, it will not ask the WebResponse class to poll the API.

## [Extensions](/CocApiLibrary/Extensions.cs)
The static Extensions class contains some things that may be useful, especially for Discord bots.\
DiscordSafe will strip Discord markup characters from a given string.\
LeftToRight will force characters to display left to right.  This is especially helpful while making tables.\
ToDateTime will convert SC API date time objects to C# DateTime, though the library already handles this when downloading your object.

## Outages
When the API goes down, the IsAvaillableChanged event will fire.\
CocApi will not stop trying to update expired objects unless you tell it to in this event.\
When an outage is detected, it will poll the API every five seconds to see if the server is back up.

## Issues
If you try to duplicate the test program in a fresh console application, ensure that your .csproj file contains .Web as seen here:
```xml 
<Project Sdk="Microsoft.NET.Sdk.Web">
```
If you have problems finding one of the required nuget packages, add https://dotnetfeed.blob.core.windows.net/dotnet-core/index.json as a source.
