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
    internal class ClanStore
    {
        internal readonly List<string> clanStrings = new List<string>();
        internal readonly IMapper _mapper;
        private readonly CocApi _cocApi;
        private bool _update = false;

        public ClanStore(CocApi cocApi, IMapper mapper)
        {
            _cocApi = cocApi;
            _mapper = mapper;
        }



        public async Task<ClanAPIModel?> GetOrDownloadAsync(string clanTag, string encodedUrl)
        { 
            _cocApi.clans.TryGetValue(clanTag, out StoredItem2<ClanAPIModel> storedClan);

            if (storedClan != null && !storedClan.IsExpired())
            {
                return storedClan.DownloadedItem as ClanAPIModel;
            }

            var result = await WebResponse.GetWebResponse<ClanAPIModel>(_cocApi, encodedUrl);

            StoredItem2<ClanAPIModel> downloadedClan = new StoredItem2<ClanAPIModel>(result, encodedUrl);

            if (downloadedClan == null && storedClan != null)
            {
                return storedClan.DownloadedItem as ClanAPIModel;
            }

            if (downloadedClan != null)
            {
                if (storedClan != null)
                {
                    _cocApi.clans.TryRemove((storedClan.DownloadedItem as ClanAPIModel).Tag, out _);
                }

                _cocApi.clans.TryAdd((downloadedClan.DownloadedItem as ClanAPIModel).Tag, downloadedClan);

                return downloadedClan.DownloadedItem as ClanAPIModel;
            }
            else if (storedClan != null) //return the expired item
            {
                return storedClan.DownloadedItem as ClanAPIModel;
            }
            else
            {
                throw new CocApiException("No matching results found.");
            }
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
                    _cocApi.clans.TryGetValue(clanStrings[i], out StoredItem2<ClanAPIModel> clanKVP);

                    if (clanKVP == null)
                    {
                        string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanStrings[i])}";

                        ClanAPIModel download;

                        try
                        {
                            download = await WebResponse.GetWebResponse<ClanAPIModel>(_cocApi, url);
                        }
                        catch (NotFoundException)
                        {
                            clanStrings.RemoveAt(i);

                            i -= 1;

                            break;
                        }
                        catch
                        {
                            throw;
                        }

                        StoredItem2<ClanAPIModel> storedItem = new StoredItem2<ClanAPIModel>(download, url);

                        if (_cocApi.clans.TryAdd(clanStrings[i], storedItem))
                        {
                            clanKVP = storedItem;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    await UpdateClan(clanKVP);

                    //await UpdateWar(clanKVP);

                    await Task.Delay(100);

                    if (!_update)
                    {
                        break;
                    }
                }                      
            }
        }


        private async Task UpdateClan(StoredItem2<ClanAPIModel> clanKVP)
        {
            if (!clanKVP.IsExpired())
            {
                return;
            }

            System.Console.WriteLine($"updating clan {clanKVP.DownloadedItem.Tag}");

            ClanAPIModel download = await WebResponse.GetWebResponse<ClanAPIModel>(_cocApi, clanKVP.EncodedUrl);

            _mapper.Map<ClanAPIModel, ClanAPIModel>(download, clanKVP.DownloadedItem as ClanAPIModel);

            _mapper.Map<BadgeUrlModel, BadgeUrlModel>(download.BadgeUrls!, clanKVP.DownloadedItem.BadgeUrls!);

            _mapper.Map<LocationModel, LocationModel>(download.Location!, clanKVP.DownloadedItem.Location!);

            clanKVP.DownloadedItem.BadgeUrls?.Process(_cocApi, clanKVP.DownloadedItem);     //should not be needed, just incase it gets nulled at some point

            clanKVP.DownloadedItem.Location?.Process(_cocApi, clanKVP.DownloadedItem);      //should not be needed, just incase it gets nulled at some point

            clanKVP.DownloadedItem!.FireEvent();

            clanKVP.DownloadedItem!.BadgeUrls?.FireEvent();

            clanKVP.DownloadedItem!.Location?.FireEvent();

            MembersLeft(download, clanKVP.DownloadedItem!);

            MembersJoined(download, clanKVP.DownloadedItem!);

            clanKVP.DateTimeUTC = DateTime.UtcNow;

            clanKVP.Expires = DateTime.UtcNow.AddMinutes(1);  //todo

            StoredItem2<CurrentWarAPIModel> storedWar = await GetOrDownloadCurrentWarAsync(clanKVP.DownloadedItem);


        }

        private async Task<StoredItem2<CurrentWarAPIModel>> GetOrDownloadCurrentWarAsync(ClanAPIModel clanAPIModel)
        {
            _cocApi.wars.TryGetValue(clanAPIModel.Tag, out var warKVP);

            CurrentWarAPIModel currentWarAPIModel;

            string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanAPIModel.Tag)}/currentwar";

            if(warKVP == null || warKVP.IsExpired())
            {
                try
                {
                    currentWarAPIModel = await WebResponse.GetWebResponse<CurrentWarAPIModel>(_cocApi, url);
                }
                catch (ForbiddenException)
                {
                    if(warKVP != null)
                    {
                        url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(warKVP.DownloadedItem.Clans.First(c => c.Tag != clanAPIModel.Tag).Tag)}/currentwar";

                        currentWarAPIModel = await WebResponse.GetWebResponse<CurrentWarAPIModel>(_cocApi, url);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return warKVP;

            //StoredItem2<CurrentWarAPIModel> warKVP = new StoredItem2<CurrentWarAPIModel>(currentWarAPIModel, url);
        }

        private void MembersJoined(ClanAPIModel downloadedClan, ClanAPIModel storedClan)
        {
            List<MemberListAPIModel> newMembers = new List<MemberListAPIModel>();

            if(downloadedClan.Members == null)
            {
                return;
            }

            if(storedClan.Members == null)
            {
                storedClan.Members = new List<MemberListAPIModel>();
            }

            foreach (MemberListAPIModel member in downloadedClan.Members)
            {
                if (!storedClan.Members.Any(m => m.Tag == member.Tag))
                {
                    newMembers.Add(member);

                    storedClan.Members.Add(member);
                }
            }

            _cocApi.MembersJoinedEvent(storedClan, newMembers);
        }

        private void MembersLeft(ClanAPIModel downloadedClan, ClanAPIModel storedClan)
        {
            List<MemberListAPIModel> leftMembers = new List<MemberListAPIModel>();

            if(storedClan.Members == null)
            {
                return;
            }

            foreach (MemberListAPIModel member in storedClan.Members)
            {
                if (!downloadedClan.Members.Any(m => m.Tag == member.Tag))
                {
                    leftMembers.Add(member);
                }
            }

            _cocApi.MembersLeftEvent(storedClan, leftMembers);
        }


        private void UpdateWar(ClanAPIModel storedClan, CurrentWarAPIModel downloadedWar)
        {
            storedClan.Wars.TryGetValue(downloadedWar.WarID, out CurrentWarAPIModel storedWar);

            if(storedWar == null)
            {
                storedClan.Wars.TryAdd(downloadedWar.WarID, downloadedWar);

                _cocApi.NewWarEvent(downloadedWar);

                storedClan.Wars.TryGetValue(downloadedWar.WarID, out storedWar);
            }

            _mapper.Map<CurrentWarAPIModel, CurrentWarAPIModel>(downloadedWar, storedWar);

            storedWar.FireEvent();

            List<AttackAPIModel> newAttacks = new List<AttackAPIModel>();

            foreach (AttackAPIModel attackAPIModel in downloadedWar.Attacks)
            {
                if(!storedWar.Attacks.Any(a => a.Order == attackAPIModel.Order))
                {
                    newAttacks.Add(attackAPIModel);

                    storedWar.Attacks.Add(attackAPIModel);
                }
            }

            _cocApi.NewAttacksEvent(storedWar, newAttacks);
        }
    }
}
