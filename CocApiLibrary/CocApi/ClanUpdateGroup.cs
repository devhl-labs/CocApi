using devhl.CocApi.Models.Clan;
using devhl.CocApi.Models.Village;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    internal sealed class ClanUpdateGroup
    {
        private readonly CocApi _cocApi;

        private bool _stopRequested = false;

        public ClanUpdateGroup(CocApi cocApi) => _cocApi = cocApi;

        public ConcurrentBag<string> ClanTags { get; } = new ConcurrentBag<string>();

        public bool QueueRunning { get; private set; } = false;

        public void StopUpdating() => _stopRequested = false;

        public Task StopUpdatingAsync()
        {
            _stopRequested = false;

            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

            Task task = tsc.Task;

            _ = Task.Run(async () =>
            {
                while (QueueRunning)
                    await Task.Delay(100).ConfigureAwait(false);

                tsc.SetResult(true);
            });

            return tsc.Task;
        }

        public void StartQueue()
        {
            _stopRequested = false;

            if (QueueRunning)
                return;

            QueueRunning = true;

            Task.Run(async () =>
            {
                try
                {
                    while (_stopRequested == false)
                    {
                        foreach (string clanTag in ClanTags)
                        {
                            Clan? queued = _cocApi.Clans.Queued.GetValueOrDefault(clanTag);

                            if (queued == null)
                            {
                                queued = await PopulateClanAsync(clanTag).ConfigureAwait(false);
                            }
                            else
                            {
                                queued = await UpdateClanAsync(queued).ConfigureAwait(false);
                            }

                            if (queued != null)
                                await UpdateClanVillagesAsync(queued);

                            if (_stopRequested)
                                break;

                            await Task.Delay(50);
                        }

                        _cocApi.Clans.OnQueueCompletedEvent();
                    }

                    QueueRunning = false;

                    _cocApi.LogEvent<CocApi>(logLevel: LogLevel.Information, loggingEvent: LoggingEvent.ClanUpdateEnded);
                }
                catch (Exception e)
                {
                    _stopRequested = false;

                    QueueRunning = false;

                    _cocApi.LogEvent<ClanUpdateGroup>(e, LogLevel.Critical, LoggingEvent.QueueCrashed);

                    _ = _cocApi.ClanQueueRestartAsync();

                    throw e;
                }
            });
        }

        private async Task<Clan> UpdateClanAsync(Clan queued)
        {
            if (queued.IsExpired() == false)
                return queued;

            Clan? fetched = await _cocApi.Clans.FetchAsync(queued.ClanTag).ConfigureAwait(false);

            if (fetched != null)
            {
                queued.Update(_cocApi, fetched);

                _cocApi.UpdateDictionary(_cocApi.Clans.Queued, fetched.ClanTag, fetched);
            }

            return fetched ?? queued;
        }

        private async Task<Clan?> PopulateClanAsync(string clanTag)
        {
            Clan? fetched = await _cocApi.Clans.FetchAsync(clanTag).ConfigureAwait(false);

            if (fetched != null)
                _cocApi.UpdateDictionary(_cocApi.Clans.Queued, fetched.ClanTag, fetched);

            return fetched;
        }

        private async Task UpdateClanVillageAsync(ClanVillage village)
        {
            //if this village is being watched, dont update it here to prevent events from firing twice
            if (_cocApi.Villages.QueueRunning && _cocApi.Villages.Get(village.VillageTag) != null)
                return;

            Village? queued = _cocApi.Clans.GetVillage(village.VillageTag);

            if (queued?.IsExpired() == false)
                return;

            Village? fetched = await _cocApi.Villages.FetchAsync(village.VillageTag).ConfigureAwait(false);

            if (queued != null && fetched != null)
                fetched.Update(_cocApi, queued);

            if (fetched != null)
                _cocApi.UpdateDictionary(_cocApi.Clans.QueuedVillage, fetched.VillageTag, fetched);
        }

        private async Task UpdateClanVillagesAsync(Clan queued)
        {
            if (_cocApi.Clans.QueueClanVillages == false || queued.QueueClanVillages == false)
                return;

            List<Task> tasks = new List<Task>();

            foreach (ClanVillage village in queued.Villages.EmptyIfNull())
                tasks.Add(UpdateClanVillageAsync(village));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}