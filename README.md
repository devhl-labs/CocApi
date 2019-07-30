# CocApi\
This library will provide responses from developer.clashofclans.com\
This is a rewrite of the class library Minion Bot uses.  It is a work in progress.  It uses .NET Standard 2.1 which is still in preview.\
Custom things this library provides:\
-current war has Clans object.  This is both clans sorted alphabetically by tag\
-Clans objects contain Attacks and Defenses that is copied from the members attacks collection\
-Clans objects contain a result that gets populated once all hits are completed or the war is over\
-WarType is set based on the length of prep day\
-WarTag is added to the current war for league wars\
-tags now reference a regular expression\
-results are stored in memory.  Wont requery the API until the results expire\
Future Upgrades:\
-Clan and Opponent objects in current war will go away, use Clans instead\
-setters will be internal\
-potentially implement events and have the library control the loop\
-soft code the time each object expires\