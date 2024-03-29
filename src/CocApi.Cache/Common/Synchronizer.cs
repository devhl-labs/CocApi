﻿using System.Collections.Concurrent;
using CocApi.Cache.Context;
using CocApi.Rest.Models;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache;

public sealed class Synchronizer
{
    public static bool Instantiated { get; private set; }


    internal ConcurrentDictionary<string, CachedClan?> UpdatingClan { get; } = new();
    internal ConcurrentDictionary<string, ClanWar?> UpdatingWar { get; } = new();
    internal ConcurrentDictionary<string, ClanWar?> UpdatingCwlWar { get; } = new();
    internal ConcurrentDictionary<string, CachedPlayer?> UpdatingVillage { get; set; } = new();


    public Synchronizer(ILogger<Synchronizer> logger)
    {
        Instantiated = Library.WarnOnSubsequentInstantiations(logger, Instantiated);
    }
}
