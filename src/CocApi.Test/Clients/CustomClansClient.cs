using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CocApi.Cache;
using CocApi.Cache.Services;
using CocApi.Cache.Services.Options;
using CocApi.Rest.IApis;
using CocApi.Rest.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CocApi.Test;

public class CustomClansClient : ClansClient
{
    public CustomClansClient(
        ILogger<CustomClansClient> logger,
        IServiceScopeFactory scopeFactory,
        IClansApi clansApi, 
        Synchronizer synchronizer,
        ClanService clanService,
        NewWarService newWarService,
        NewCwlWarService newCwlWarService,
        CwlWarService cwlWarService,
        WarService warService,
        IOptions<CacheOptions> options)
        : base(logger, clansApi, scopeFactory, synchronizer, clanService, newWarService, newCwlWarService, warService, cwlWarService, options)
    {
        ClanUpdated += OnClanUpdated;
        ClanWarAdded += OnClanWarAdded;
        ClanWarUpdated += OnClanWarUpdated;
    }

    private Task OnClanUpdated(object sender, ClanUpdatedEventArgs e)
    {
        if (e.Stored == null)
            return Task.CompletedTask;

        List<Donation> donations = Clan.Donations(e.Stored, e.Fetched);

        if (donations.Count > 0)
            Logger.LogInformation("{donationsSum} troops donated in {clanTag} {clanName}", donations.Sum(d => d.Quanity), e.Fetched.Tag, e.Fetched.Name);

        foreach (ClanMember member in Clan.ClanMembersLeft(e.Stored, e.Fetched))
            Logger.LogInformation("{memberTag} {memberName} left clan {clanTag} {clanName}", member.Tag, member.Name, e.Fetched.Tag, e.Fetched.Name);

        foreach (ClanMember member in Clan.ClanMembersJoined(e.Stored, e.Fetched))
            Logger.LogInformation("{memberTag} {memberName} joined clan {clanTag} {clanName}", member.Tag, member.Name, e.Fetched.Tag, e.Fetched.Name);

        return Task.CompletedTask;
    }

    private Task OnClanWarUpdated(object sender, ClanWarUpdatedEventArgs e)
    {
        Logger.LogInformation("{newAttackCount} new attacks between {clanTag} vs {opponentTag}.", ClanWar.NewAttacks(e.Stored, e.Fetched).Count, e.Fetched.Clan.Tag, e.Fetched.Opponent.Tag);

        return Task.CompletedTask;
    }

    private Task OnClanWarAdded(object sender, WarAddedEventArgs e)
    {
        Logger.LogInformation("New war between {clanTag} and {opponentTag}.", e.War.Clan.Tag, e.War.Opponent.Tag);

        return Task.CompletedTask;
    }
}
