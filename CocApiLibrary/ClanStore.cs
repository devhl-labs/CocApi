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
            _cocApi.clans.TryGetValue(clanTag, out StoredItem storedClan);

            if (storedClan != null && !storedClan.IsExpired())
            {
                return storedClan.DownloadedItem as ClanAPIModel;
            }

            var result = await WebResponse.GetWebResponse<ClanAPIModel>(_cocApi, encodedUrl);

            StoredItem downloadedClan = new StoredItem(result.Item2, result.Item1, encodedUrl);

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
                    //System.Console.WriteLine($"downloading war {clanStrings[i]}");

                    _cocApi.clans.TryGetValue(clanStrings[i], out StoredItem clanKVP);

                    if (clanKVP == null)
                    {
                        string url = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(clanStrings[i])}";

                        Tuple<System.Diagnostics.Stopwatch, ClanAPIModel> download;

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

                        StoredItem storedItem = new StoredItem(download.Item2, download.Item1, url);

                        if (_cocApi.clans.TryAdd(clanStrings[i], storedItem))
                        {
                            clanKVP = storedItem;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    //begin updating the clan!
                    await UpdateClan(clanKVP);

                    await Task.Delay(100);

                    if (!_update)
                    {
                        break;
                    }
                }                      
            }
        }

        private async Task UpdateClan(StoredItem clanKVP)
        {
            if (!clanKVP.IsExpired())
            {
                return;
            }

            System.Console.WriteLine($"updating clan");

            var download = await WebResponse.GetWebResponse<ClanAPIModel>(_cocApi, clanKVP.EncodedUrl);

            _mapper.Map<ClanAPIModel, ClanAPIModel>(download.Item2, clanKVP.DownloadedItem as ClanAPIModel);

            List<MemberListAPIModel> newMembers = new List<MemberListAPIModel>();

            foreach(MemberListAPIModel member in download.Item2.Members)
            {
                if(!(clanKVP.DownloadedItem as ClanAPIModel).Members.Any(m => m.Tag == member.Tag))
                {
                    newMembers.Add(member);

                    (clanKVP.DownloadedItem as ClanAPIModel).Members.Add(member);
                }
            }

            if(newMembers.Count() > 0)
            {
                _cocApi.MembersJoinedEvent((clanKVP.DownloadedItem as ClanAPIModel), newMembers);
            }

            clanKVP.DateTimeUTC = DateTime.UtcNow;

            clanKVP.TimeToDownload = download.Item1.Elapsed;

            clanKVP.Expires = DateTime.UtcNow.AddMinutes(1);
        }
    }
}
