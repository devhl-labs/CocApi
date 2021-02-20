# CocApi
A wrapper for [developer.clashofclans.com](https://developer.clashofclans.com/#/) written in .Net 5.0. 
CocApi is used to fetch results from the clash API. 
CocApi.Cache will cache responses in a database of your choice.
To keep objects up to date automatically and receive events on updates, add the tag to the village or clan ClientBase object.
 
## Help  
![Discord Banner 2](https://discordapp.com/api/guilds/701245583444279328/widget.png?style=banner2)

## Using the library
```
Clan clan = await clansApi.FetchAsync("#clanTag");
Clan clan = await clansClient.GetOrFetchClanAsync("#clanTag");
```

## Objects
Api objects like ClansApi and PlayersApi query the api.
TokenProvider stores your clash tokens and controls the client side rate limiting. 
Ensure the TokenProvider is a singleton so the rate limiting works.
The default rate limit of one second is extremely conservative. 
You should be able to do 33 milliseconds.
The clients like ClansClientBase control the caching and updating of objects. 
It is strongly recommended that you inherit this base class and override certain methods such as TimeToLive.
This will control how frequently the objects download and what property changes cause the update events to fire.

## Your Database
Create a class that implements `IDesignTimeDbContextFactory<CocApi.Cache.CocApiCacheContext>`.
This tells the CocApi.Cache how to interact with your database.
Before running you will need to create a migration and run the update on your database provider.
See [scripts](docs/scripts) for examples.

## Disclaimer
This content is not affiliated with, endorsed, sponsored, or specifically approved by Supercell and Supercell is not responsible for it. For more information see [Supercell's Fan Content Policy](https://supercell.com/en/fan-content-policy/).