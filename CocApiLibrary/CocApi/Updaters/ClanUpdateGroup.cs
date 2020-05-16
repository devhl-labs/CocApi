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
        private CocApi CocApi { get; }

        private bool StopRequested { get; set; }

        public ClanUpdateGroup(CocApi cocApi) => CocApi = cocApi;

        public ConcurrentBag<string> ClanTags { get; } = new ConcurrentBag<string>();

        public bool QueueRunning { get; private set; } = false;

        public void StopUpdating() => StopRequested = true;

        public Task StopUpdatingAsync()
        {
            StopRequested = true;

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

        public bool QueueIsPopulated { get; private set; }

        public void StartQueue()
        {
            StopRequested = false;

            if (QueueRunning)
                return;

            QueueRunning = true;

            Task.Run(async () =>
            {
                try
                {
                    while (StopRequested == false)
                    {
                        foreach (string clanTag in ClanTags)
                        {
                            if (StopRequested)
                                break;

                            Clan? queued = CocApi.Clans.Queued.GetValueOrDefault(clanTag);

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

                            await Task.Delay(50);
                        }

                        //if (QueueIsPopulated == false)
                        //{
                            QueueIsPopulated = true;

                            CocApi.Clans.OnQueuePopulated();
                        //}
                    }

                    QueueRunning = false;

                    CocApi.OnLog(new LogEventArgs(nameof(ClanUpdateGroup), nameof(StartQueue), LogLevel.Information, LoggingEvent.QueueExited.ToString()));
                }
                catch (Exception e)
                {
                    StopRequested = false;

                    QueueRunning = false;

                    CocApi.OnLog(new ExceptionEventArgs(nameof(ClanUpdateGroup), nameof(StartQueue), e));

                    _ = CocApi.ClanQueueRestartAsync();

                    throw e;
                }
            });
        }

        private async Task<Clan> UpdateClanAsync(Clan queued)
        {
            if (queued.IsExpired() == false)
                return queued;

            Clan? fetched = await CocApi.Clans.FetchAsync(queued.ClanTag).ConfigureAwait(false);

            if (fetched != null)
            {
                queued.Update(CocApi, fetched);

                CocApi.UpdateDictionary(CocApi.Clans.Queued, fetched.ClanTag, fetched);
            }

            return fetched ?? queued;
        }

        private async Task<Clan?> PopulateClanAsync(string clanTag)
        {
            Clan? fetched = await CocApi.Clans.FetchAsync(clanTag).ConfigureAwait(false);

            if (fetched != null)
                CocApi.UpdateDictionary(CocApi.Clans.Queued, fetched.ClanTag, fetched);

            return fetched;
        }

        private async Task UpdateClanVillageAsync(ClanVillage village)
        {
            //if this village is being watched, dont update it here to prevent events from firing twice
            if (CocApi.Villages.QueueRunning && CocApi.Villages.Get(village.VillageTag) != null)
                return;

            Village? queued = CocApi.Clans.GetVillage(village.VillageTag);

            if (queued?.IsExpired() == false)
                return;

            Village? fetched = await CocApi.Villages.FetchAsync(village.VillageTag).ConfigureAwait(false);

            if (queued != null && fetched != null)
                queued.Update(CocApi, fetched);

            if (fetched != null)
                CocApi.UpdateDictionary(CocApi.Clans.QueuedVillage, fetched.VillageTag, fetched);
        }

        private async Task UpdateClanVillagesAsync(Clan queued)
        {
            if (CocApi.Clans.QueueClanVillages == false || queued.QueueClanVillages == false)
                return;

            List<Task> tasks = new List<Task>();

            foreach (ClanVillage village in queued.Villages.EmptyIfNull())
                tasks.Add(UpdateClanVillageAsync(village));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}