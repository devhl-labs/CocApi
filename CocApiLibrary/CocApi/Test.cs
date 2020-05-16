using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using devhl.CocApi.Models.War;
using System.Collections.Generic;
using System.Data;

namespace devhl.CocApi
{
    public class Test
    {
        private readonly CocApi _cocApi;

        public Test(CocApi cocApi) => _cocApi = cocApi;


        // villages
        public void OnApiIsAvailalbeChanged(bool available) => _cocApi.IsAvailable = available;

        public void OnVillageAchievementsChanged(Village fetched, List<Achievement> achievements) => _cocApi.Villages.OnVillageAchievementsChanged(fetched, achievements);

        public void OnVillageChanged(Village fetched, Village queued) => _cocApi.Villages.OnVillageChanged(fetched, queued);

        public void OnVillageHeroesChanged(Village fetched, List<Troop> heroes) => _cocApi.Villages.OnVillageHeroesChanged(fetched, heroes);

        public void OnVillageLabelsChanged(Village fetched, List<VillageLabel> added, List<VillageLabel> removed) => _cocApi.Villages.OnVillageLabelsChanged(fetched, added, removed);

        public void OnVillageLegendLeagueChanged(Village fetched, Village queued) => _cocApi.Villages.OnVillageLegendLeagueChanged(fetched, queued);

        public void OnVillageSpellsChanged(Village fetched, List<Spell> spells) => _cocApi.Villages.OnVillageSpellsChanged(fetched, spells);

        public void OnVillageTroopsChanged(Village fetched, List<Troop> troops) => _cocApi.Villages.OnVillageTroopsChanged(fetched, troops);

        public void OnVillageQueueCompleted() => _cocApi.Villages.OnQueuePopulated();



        // wars
        public void OnWarQueueCompleted() => _cocApi.Wars.OnQueuePopulated();

        public void OnNewAttacks(CurrentWar fetched, List<Attack> attacks) => _cocApi.Wars.OnNewAttacks(fetched, attacks);

        public void OnNewWar(CurrentWar fetched) => _cocApi.Wars.OnNewWar(fetched);

        public void OnWarChanged(CurrentWar fetched, CurrentWar queued) => _cocApi.Wars.OnWarChanged(fetched, queued);

        public void OnWarEnded(CurrentWar fetched) => _cocApi.Wars.OnWarEnded(fetched);

        public void OnWarEndingSoon(CurrentWar fetched) => _cocApi.Wars.OnWarEndingSoon(fetched);

        public void OnWarEndNotSeen(CurrentWar fetched) => _cocApi.Wars.OnWarEndNotSeen(fetched);

        public void OnWarEndSeen(CurrentWar fetched) => _cocApi.Wars.OnWarEndSeen(fetched);

        public void OnWarIsAccessibleChanged(CurrentWar currentWar, bool isAccessible) => _cocApi.Wars.OnWarIsAccessibleChanged(currentWar, isAccessible);

        public void OnWarStarted(CurrentWar fetched) => _cocApi.Wars.OnWarStarted(fetched);

        public void OnWarStartingSoon(CurrentWar fetched) => _cocApi.Wars.OnWarStartingSoon(fetched);

        public void OnFinalAttacksNotSeen(CurrentWar storedWar, WarLogEntry warLogEntry) => _cocApi.Wars.OnFinalAttacksNotSeen(storedWar, warLogEntry);



        // clans
        public void OnBadgeUrlChanged(Clan fetched, Clan queued) => _cocApi.Clans.OnBadgeUrlChanged(fetched, queued);

        public void OnClanChanged(Clan fetched, Clan queued) => _cocApi.Clans.OnClanChanged(fetched, queued);

        public void OnClanVillageChanged(Clan fetched, ClanVillage fetchedVillage, ClanVillage queuedVillage) => _cocApi.Clans.OnClanVillageChanged(fetched, fetchedVillage, queuedVillage);

        public void OnClanVillagesJoined(Clan fetched, List<ClanVillage> fetchedClanVillages) => _cocApi.Clans.OnClanVillagesJoined(fetched, fetchedClanVillages);

        public void OnClanVillagesLeft(Clan fetched, List<ClanVillage> fetchedClanVillages) => _cocApi.Clans.OnClanVillagesLeft(fetched, fetchedClanVillages);

        public void OnDonation(Clan fetched, List<Donation> received, List<Donation> donated) => _cocApi.Clans.OnDonation(fetched, received, donated);

        public void OnLabelsChanged(Clan fetched, List<ClanLabel> added, List<ClanLabel> removed) => _cocApi.Clans.OnLabelsChanged(fetched, added, removed);

        public void OnQueueCompletedEvent() => _cocApi.Clans.OnQueuePopulated();
    }
}