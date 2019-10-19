# CocApi
This .NET Standard 2.1 library will provide responses from developer.clashofclans.com\
This is a rewrite of the class library Minion Bot uses.
 
## Test Program
The test program shows you how to set up the library.\
The library can grab SC API objects on command.\
It can also automatically keep these objects up to date via polling.\
When an object changes, it will fire an event which you can subscribe to.\
You may optionally provide an ILogger to observe what the library is doing.\
The test program will output the following:\
![Test Program console output](https://github.com/devhl-labs/CocApi/blob/controlsloop/CocApiConsoleTest/images/console.jpg)
If you choose to keep the objects up to date, the library will constantly write to the ILogger.


## Custom things this library provides:
-current war has Clans object.  This is both clans sorted alphabetically by tag\
-Clans objects contain Attacks and Defenses that is copied from the members attacks collection\
-Clans objects contain a result that gets populated once all hits are completed or the war is over\
-WarType is set based on the length of prep day\
-WarTag is added to the current war for league wars\
-tags now reference a regular expression\
-results are stored in memory.  Wont requery the API until the results expire
## Future Upgrades:
-Clan and Opponent objects in current war will go away, use Clans instead\
-setters will be internal\
-potentially implement events and have the library control the loop\
-soft code the time each object expires