using AutoMapper;
using CocApiLibrary.Exceptions;
using CocApiLibrary.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CocApiLibrary
{
    internal class ClanUpdateService
    {
        public readonly List<string> clanStrings = new List<string>();

        private readonly CocApi _cocApi;
        private bool _update = false;
        
        public ClanUpdateService(CocApi cocApi)
        {
            _cocApi = cocApi;
        }

        public void Update(bool update)
        {
            _update = update;

            Task.Run(async () =>
            {
                await Update();
            });
        }

        private async Task Update()
        {
            while (_update)
            {
                for (int i = 0; i < clanStrings.Count; i++)
                {
                    ClanAPIModel storedClan = await _cocApi.GetClanAsync(clanStrings[i]);

                    ClanAPIModel downloadedClan = await _cocApi.GetClanAsync(clanStrings[i], true, false);

                    storedClan.Update(_cocApi, downloadedClan);

                    if (_cocApi.DownloadLeagueWars)
                    {
                        LeagueGroupAPIModel leagueGroupAPIModel = await _cocApi.GetLeagueGroupAsync(clanStrings[i], true, false);

                        foreach (var round in leagueGroupAPIModel.Rounds.Where(r => r != null))
                        {
                            foreach(var warTag in round.WarTags.Where(w => w != null))
                            {
                                ICurrentWarAPIModel leagueWar = await _cocApi.GetLeagueWarAsync(warTag, true, false);

                                if (leagueWar.Clans.Any(c => c.Tag == storedClan.Tag)) continue;
                            }
                        }
                    }

                    storedClan.AnnounceWars = true;

                    foreach (ICurrentWarAPIModel storedWar in storedClan.Wars.Values)
                    {
                        ICurrentWarAPIModel? downloadedWar = await _cocApi.GetCurrentWarAsync(storedWar);

                        if(storedWar is CurrentWarAPIModel currentWar)
                        {
                            currentWar.Update(_cocApi, storedClan, currentWar);
                        }
                        else if(storedWar is LeagueWarAPIModel leagueWar)
                        {
                            leagueWar.Update(_cocApi, storedClan, leagueWar);
                        }
                    }

                    if (_cocApi.DownloadVillages)
                    {
                        foreach(var village in storedClan.Members.Where(m => m != null))
                        {
                            try  //there is an api bug where some villages in the clan do not appear in the village end point
                            {
                                VillageAPIModel storedVillage = await _cocApi.GetVillageAsync(village.Tag);

                                VillageAPIModel downloadedVillage = await _cocApi.GetVillageAsync(village.Tag, true, false);

                                storedVillage.Update(_cocApi, downloadedVillage);
                            }
                            catch (Exception)
                            {
                                
                            }
                        }

                        if (!_update)
                        {
                            break;
                        }
                    }

                    await Task.Delay(100);

                    if (!_update)
                    {
                        break;
                    }
                }
            }
        }
    }
}




































































////////////////using AutoMapper;
////////////////using CocApiLibrary.Exceptions;
////////////////using CocApiLibrary.Models;
////////////////using System;
////////////////using System.Collections.Concurrent;
////////////////using System.Collections.Generic;
////////////////using System.Linq;
////////////////using System.Threading.Tasks;

////////////////namespace CocApiLibrary
////////////////{
////////////////    internal class ClanUpdateService
////////////////    {
////////////////        internal readonly List<string> clanStrings = new List<string>();
////////////////        private readonly CocApi _cocApi;
////////////////        private bool _update = false;
////////////////        private bool _firstPass = true;

////////////////        public ClanUpdateService(CocApi cocApi)
////////////////        {
////////////////            _cocApi = cocApi;
////////////////        }

////////////////        public void Update(bool update)
////////////////        {
////////////////            _update = update;

////////////////            Task.Run(async () =>
////////////////            {
////////////////                await Update();
////////////////            });
////////////////        }

////////////////        private async Task Update()
////////////////        {
////////////////            while (_update)
////////////////            {
////////////////                for (int i = 0; i < clanStrings.Count; i++)
////////////////                {
////////////////                    ClanAPIModel storedClan = await _cocApi.GetClanAsync(clanStrings[i]);

////////////////                    if (_firstPass)
////////////////                    {
////////////////                        await InitialWarDownloadTryAsync(storedClan);
////////////////                    }
////////////////                    else
////////////////                    {
////////////////                        await ValidateClan(storedClan);

////////////////                        await UpdateWar(storedClan);
////////////////                    }

////////////////                    await Task.Delay(100);

////////////////                    if (!_update)
////////////////                    {
////////////////                        break;
////////////////                    }
////////////////                }
////////////////            }
////////////////        }

////////////////        private async Task ValidateClan(ClanAPIModel storedClan)
////////////////        {
////////////////            Console.WriteLine("ValidateClan");

////////////////            if (!storedClan.IsExpired())
////////////////            {
////////////////                return;
////////////////            }

////////////////            System.Console.WriteLine($"updating clan {storedClan.Tag}");

////////////////            ClanAPIModel newClan = await _cocApi.GetClanAsync(storedClan.Tag, false, false);

////////////////            UpdateClanTry(storedClan, newClan);

////////////////            UpdateBadgeTry(storedClan, newClan);

////////////////            UpdateLocationTry(storedClan, newClan);

////////////////            MembersLeftTry(storedClan, newClan);

////////////////            MembersJoinedTry(storedClan, newClan);
////////////////        }

////////////////        private async Task InitialWarDownloadTryAsync(ClanAPIModel storedClan)
////////////////        {
////////////////            Console.WriteLine("InitialWarDownloadTryAsync");

////////////////            try
////////////////            {
////////////////                //if (storedClan.Wars != null)
////////////////                //{
////////////////                //    return;
////////////////                //}

////////////////                //storedClan.Wars = new Dictionary<string, ICurrentWarAPIModel>();

////////////////                ICurrentWarAPIModel currentWarAPIModel = await _cocApi.GetWarAsync(storedClan.Tag, true, false);

////////////////                if (currentWarAPIModel.State != Enums.State.NotInWar)
////////////////                {
////////////////                    currentWarAPIModel.Clans.First(c => c.Tag == storedClan.Tag).WarIsAccessible = true;

////////////////                    storedClan.Wars?.TryAdd(currentWarAPIModel.WarID, currentWarAPIModel);
////////////////                }
////////////////                else if (currentWarAPIModel.State == Enums.State.NotInWar)
////////////////                {
////////////////                    await LeagueWarDownloadTryAsync(storedClan, false);
////////////////                }


////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }

////////////////        private async Task LeagueWarDownloadTryAsync(ClanAPIModel storedClan, bool announceWar)
////////////////        {
////////////////            Console.WriteLine("LeagueWarDownloadTryAsync");

////////////////            try
////////////////            {
////////////////                LeagueGroupAPIModel leagueGroupAPIModel = await _cocApi.GetLeagueGroupAsync(storedClan.Tag);

////////////////                if (leagueGroupAPIModel.Rounds == null)
////////////////                {
////////////////                    return;
////////////////                }

////////////////                foreach (RoundAPIModel round in leagueGroupAPIModel.Rounds)
////////////////                {
////////////////                    if (round.WarTags == null)
////////////////                    {
////////////////                        continue;
////////////////                    }

////////////////                    foreach (string warTag in round.WarTags)
////////////////                    {
////////////////                        ICurrentWarAPIModel leagueWar = await _cocApi.GetLeagueWarAsync(warTag);

////////////////                        if (leagueWar.Clans.Any(c => c.Tag == storedClan.Tag))
////////////////                        {
////////////////                            if (storedClan.Wars?.TryAdd(leagueWar.WarID, leagueWar) == true && announceWar)
////////////////                            {
////////////////                                _cocApi.NewWarEvent(storedClan, leagueWar);
////////////////                            }

////////////////                            break;
////////////////                        }
////////////////                    }
////////////////                }
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }



////////////////        private async Task UpdateWar(ClanAPIModel storedClan)
////////////////        {
////////////////            Console.WriteLine("UpdateWar");

////////////////            ICurrentWarAPIModel? currentWar = await GetCurrentWarOrDefault(storedClan);

////////////////            if (currentWar == null || currentWar.State == Enums.State.NotInWar)
////////////////            {
////////////////                await LeagueWarDownloadTryAsync(storedClan, true);
////////////////            }

////////////////            if (storedClan.Wars == null)
////////////////            {
////////////////                return;
////////////////            }

////////////////            foreach (CurrentWarAPIModel storedWar in storedClan.Wars.Values)
////////////////            {
////////////////                if (!storedWar.Flags.WarStartingSoon && DateTime.UtcNow > storedWar.WarStartingSoonUTC)
////////////////                {
////////////////                    _cocApi.WarStartingSoonEvent(storedWar);
////////////////                    storedWar.Flags.WarEndingSoon = true;
////////////////                }

////////////////                if (!storedWar.Flags.WarEndingSoon && DateTime.UtcNow > storedWar.WarEndingSoonUTC)
////////////////                {
////////////////                    _cocApi.WarEndingSoonEvent(storedWar);
////////////////                    storedWar.Flags.WarEndingSoon = true;
////////////////                }

////////////////                ICurrentWarAPIModel? foundWar = null;

////////////////                if (storedWar.State == Enums.State.WarEnded || storedWar.EndTimeUTC.AddHours(6) < DateTime.UtcNow)
////////////////                {
////////////////                    continue; //dont update wars that we saw end or wars that have been ended for six hours
////////////////                }

////////////////                if (storedWar is LeagueWarAPIModel leagueWar && leagueWar.IsExpired())
////////////////                {
////////////////                    foundWar = await _cocApi.GetLeagueWarAsync(leagueWar.WarTag);
////////////////                }
////////////////                else if (currentWar != null && currentWar.WarID == storedWar.WarID)
////////////////                {
////////////////                    foundWar = currentWar;
////////////////                }
////////////////                else if (currentWar != null && currentWar.WarID != storedWar.WarID)
////////////////                {
////////////////                    storedWar.Clans.First(c => c.Tag == storedClan.Tag).WarIsAccessible = false; //todo make an event when both are false

////////////////                    foundWar = await _cocApi.GetWarAsync(storedWar.Clans.First(c => c.Tag != storedClan.Tag).Tag, true, false);
////////////////                }

////////////////                if (foundWar != null && foundWar.WarID == storedWar.WarID)
////////////////                {
////////////////                    UpdateWar(storedWar, foundWar);
////////////////                }
////////////////                else if (foundWar != null && foundWar.WarID != storedWar.WarID)
////////////////                {
////////////////                    foreach (var clan in storedWar.Clans)
////////////////                    {
////////////////                        clan.WarIsAccessible = false; //todo add event for both being false
////////////////                    }
////////////////                }
////////////////            }
////////////////        }

////////////////        private void UpdateWar(ICurrentWarAPIModel oldWar, ICurrentWarAPIModel newWar)
////////////////        {
////////////////            Console.WriteLine("UpdateWar2");

////////////////            if (oldWar.EndTimeUTC != newWar.EndTimeUTC ||
////////////////                oldWar.StartTimeUTC != newWar.StartTimeUTC ||
////////////////                oldWar.State != newWar.State
////////////////            )
////////////////            {
////////////////                _cocApi.WarChangedEvent(oldWar, newWar);

////////////////                oldWar.EndTimeUTC = newWar.EndTimeUTC;
////////////////                oldWar.StartTimeUTC = newWar.StartTimeUTC;
////////////////                oldWar.State = newWar.State;
////////////////            }

////////////////            List<AttackAPIModel> newAttacks = new List<AttackAPIModel>();

////////////////            foreach (AttackAPIModel attack in newWar.Attacks.Values)
////////////////            {
////////////////                if (oldWar.Attacks.TryAdd(attack.Order, attack))
////////////////                {
////////////////                    newAttacks.Add(attack); //todo there are more lists that contain attacks
////////////////                }
////////////////            }

////////////////            if (newAttacks.Count() > 0)
////////////////            {
////////////////                _cocApi.NewAttacksEvent(oldWar, newAttacks);
////////////////            }
////////////////        }






////////////////        private void UpdateLocationTry(ClanAPIModel oldClan, ClanAPIModel newClan)
////////////////        {
////////////////            try
////////////////            {
////////////////                if (oldClan.Location == null && newClan.Location != null)
////////////////                {
////////////////                    _cocApi.ClanLocationChangedEvent(oldClan, newClan);
////////////////                    return;
////////////////                }

////////////////                if (oldClan.Location?.CountryCode != newClan.Location?.CountryCode ||
////////////////                    oldClan.Location?.Id != newClan.Location?.Id ||
////////////////                    oldClan.Location?.IsCountry != newClan.Location?.IsCountry ||
////////////////                    oldClan.Location?.Name != newClan.Location?.Name)
////////////////                {
////////////////                    _cocApi.ClanBadgeUrlChangedEvent(oldClan, newClan);

////////////////                    oldClan.Location = newClan.Location;
////////////////                }
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }

////////////////        private void UpdateBadgeTry(ClanAPIModel oldClan, ClanAPIModel newClan)
////////////////        {
////////////////            try
////////////////            {
////////////////                if (oldClan.BadgeUrls == null && newClan.BadgeUrls != null)
////////////////                {
////////////////                    _cocApi.ClanBadgeUrlChangedEvent(oldClan, newClan);
////////////////                    return;
////////////////                }

////////////////                if (oldClan.BadgeUrls?.Large != newClan.BadgeUrls?.Large ||
////////////////                    oldClan.BadgeUrls?.Medium != newClan.BadgeUrls?.Medium ||
////////////////                    oldClan.BadgeUrls?.Small != newClan.BadgeUrls?.Small)
////////////////                {
////////////////                    _cocApi.ClanBadgeUrlChangedEvent(oldClan, newClan);

////////////////                    oldClan.BadgeUrls = newClan.BadgeUrls;
////////////////                }
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }

////////////////        private void UpdateClanTry(ClanAPIModel oldClan, ClanAPIModel newClan)
////////////////        {
////////////////            try
////////////////            {
////////////////                if (oldClan.ClanPoints != newClan.ClanPoints)
////////////////                {
////////////////                    _cocApi.ClanPointsChangedEvent(oldClan, newClan.ClanPoints);

////////////////                    oldClan.ClanPoints = newClan.ClanPoints;
////////////////                }

////////////////                if (oldClan.ClanVersusPoints != newClan.ClanVersusPoints)
////////////////                {
////////////////                    _cocApi.ClanVersusPointsChangedEvent(oldClan, newClan.ClanVersusPoints);

////////////////                    oldClan.ClanVersusPoints = newClan.ClanVersusPoints;
////////////////                }

////////////////                if (oldClan.ClanLevel != newClan.ClanLevel ||
////////////////                    oldClan.Description != newClan.Description ||
////////////////                    oldClan.IsWarLogPublic != newClan.IsWarLogPublic ||
////////////////                    oldClan.MemberCount != newClan.MemberCount ||
////////////////                    oldClan.Name != newClan.Name ||
////////////////                    oldClan.RequiredTrophies != newClan.RequiredTrophies ||
////////////////                    oldClan.Type != newClan.Type ||
////////////////                    oldClan.WarFrequency != newClan.WarFrequency ||
////////////////                    oldClan.WarLosses != newClan.WarLosses ||
////////////////                    oldClan.WarTies != newClan.WarTies ||
////////////////                    oldClan.WarWins != newClan.WarWins ||
////////////////                    oldClan.WarWinStreak != newClan.WarWinStreak
////////////////                )
////////////////                {
////////////////                    _cocApi.ClanChangedEvent(oldClan, newClan);

////////////////                    oldClan.ClanLevel = newClan.ClanLevel;
////////////////                    oldClan.Description = newClan.Description;
////////////////                    oldClan.IsWarLogPublic = newClan.IsWarLogPublic;
////////////////                    oldClan.MemberCount = newClan.MemberCount;
////////////////                    oldClan.Name = newClan.Name;
////////////////                    oldClan.RequiredTrophies = newClan.RequiredTrophies;
////////////////                    oldClan.Type = newClan.Type;
////////////////                    oldClan.WarFrequency = newClan.WarFrequency;
////////////////                    oldClan.WarLosses = newClan.WarLosses;
////////////////                    oldClan.WarTies = newClan.WarWins;
////////////////                    oldClan.WarWinStreak = newClan.WarWinStreak;

////////////////                    oldClan.DateTimeUTC = newClan.DateTimeUTC;
////////////////                    oldClan.Expires = newClan.Expires;
////////////////                }
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }

////////////////        private void MembersJoinedTry(ClanAPIModel oldClan, ClanAPIModel newClan)
////////////////        {
////////////////            try
////////////////            {
////////////////                List<MemberListAPIModel> newMembers = new List<MemberListAPIModel>();

////////////////                if (newClan.Members == null)
////////////////                {
////////////////                    return;
////////////////                }

////////////////                if (oldClan.Members == null)
////////////////                {
////////////////                    oldClan.Members = new List<MemberListAPIModel>();
////////////////                }

////////////////                foreach (MemberListAPIModel member in newClan.Members)
////////////////                {
////////////////                    if (!oldClan.Members.Any(m => m.Tag == member.Tag))
////////////////                    {
////////////////                        newMembers.Add(member);

////////////////                        oldClan.Members.Add(member);
////////////////                    }
////////////////                }

////////////////                _cocApi.MembersJoinedEvent(oldClan, newMembers);
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }

////////////////        private void MembersLeftTry(ClanAPIModel oldClan, ClanAPIModel newClan)
////////////////        {
////////////////            try
////////////////            {
////////////////                List<MemberListAPIModel> leftMembers = new List<MemberListAPIModel>();

////////////////                if (oldClan.Members == null)
////////////////                {
////////////////                    return;
////////////////                }

////////////////                foreach (MemberListAPIModel member in oldClan.Members)
////////////////                {
////////////////                    if (!newClan.Members.Any(m => m.Tag == member.Tag))
////////////////                    {
////////////////                        leftMembers.Add(member);
////////////////                    }
////////////////                }

////////////////                foreach (MemberListAPIModel member in leftMembers)
////////////////                {
////////////////                    oldClan.Members.Remove(member);
////////////////                }

////////////////                _cocApi.MembersLeftEvent(oldClan, leftMembers);
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////            }
////////////////        }










////////////////        private async Task<ICurrentWarAPIModel?> GetCurrentWarOrDefault(ClanAPIModel storedClan)
////////////////        {
////////////////            Console.WriteLine("GetCurrentWarOrDefault");

////////////////            try
////////////////            {
////////////////                if (storedClan.IsWarLogPublic && storedClan.Wars.All(w => w.Value.IsExpired()))
////////////////                {
////////////////                    ICurrentWarAPIModel currentWar = await _cocApi.GetWarAsync(storedClan.Tag, false);

////////////////                    if (currentWar.State != Enums.State.NotInWar && !storedClan.Wars.Any(w => w.Value.WarID == currentWar.WarID))
////////////////                    {
////////////////                        //AddNewWarToDictionaries(storedClan, currentWar);

////////////////                        storedClan.Wars?.TryAdd(currentWar.WarID, currentWar);

////////////////                        _cocApi.NewWarEvent(storedClan, currentWar);

////////////////                        if (currentWar.Attacks.Count() > 0)
////////////////                        {
////////////////                            _cocApi.NewAttacksEvent(currentWar, currentWar.Attacks.Values.ToList());
////////////////                        }
////////////////                    }

////////////////                    return currentWar;
////////////////                }

////////////////                return null;
////////////////            }
////////////////            catch (Exception)
////////////////            {
////////////////                return null;
////////////////            }
////////////////        }
////////////////    }
////////////////}

