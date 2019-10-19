# CocApiLibrary
This .NET Standard 2.1 library will provide responses from developer.clashofclans.com\
This is a rewrite of the class library Minion Bot uses.
 
## Test Program
The test program shows you how to set up the library.\
The library can grab SC API objects on command.\
It can also automatically keep these objects up to date via polling.\
When an object changes, it will fire an event which you can subscribe to.\
You may optionally provide an ILogger to observe what the library is doing.\
The test program will output the following:\ \
![Test Program console output](https://github.com/devhl-labs/CocApi/blob/controlsloop/CocApiConsoleTest/images/console.jpg)\
If you choose to keep the objects up to date, the library will constantly write to the ILogger.

## CocApi
The CocApi class is the entry point to this library.\
You likely want this class to be a singleton that lives for the life of your application.\
This class is disposable, don't forget to call Dispose when you end your applicaiton.\
This program has the potential to consume a lot of memory.\

## CocApiConfiguration
Use this to configure the CocApi class.\
This class can control how often your SC API token is used.\
You can also control how long a downloaded object is considered good for.\
The default values are very conservative just in case the SC API key you provide is used for something else.\
This is to avoid rate limiting your key.\
If your key is only used in CocApi, you can make the time spans much shorter.\
The API allows about 10 requests a second per key.

## ICurrentWarAPIModel
This interface is implemented by CurrentWarAPIModel and LeagueWarAPIModel.\
The only difference is LeagueWarAPIModel has a WarTag property, and the WarType enum will be SCCWL.\
When CocApi returns an ICurrentWarAPIModel, you can cast it to the appropriate type when necessary.

## UpdateService
This is an internal class.  It is only public so it can inherit an abstract class.

## Extensions
The static Extensions class contains some things that may be useful, especially for Discord Bots.\
DiscordSafe will strip Discord markup characters from a given string.\
LeftToRight will force characters to display left to right.  This is especially helpful while making tables.\
ToDateTime will convert SC API date time objects to C# DateTime, though the library already handles this when downloading your object.

## Outages
When the API goes down, the IsAvaillableChanged event will fire.\
CocApi will not stop trying to update expired objects unless you tell it to in this event.\
When an outage is detected, it will poll the API every five seconds to see if the server is back up.