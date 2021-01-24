using System;

namespace CocApi.Cache
{
    public class ClientConfiguration
    {
        public string ConnectionString { get; }

        public TimeSpan DelayBetweenTasks { get; }

        public int ConcurrentClanDownloads { get; }

        public int ConcurrentClanWarDownloads { get; }

        public int ConcurrentCwlDownloads { get; }

        public int ConcurrentPlayerDownloads { get; }

        public int ConcurrentWarLogDownloads { get; }

        public ClientConfiguration(
            string connectionString = "Data Source=CocApi.Cache.sqlite", 
            TimeSpan? delayBetweenTasks = null,
            int concurrentClanWarDownloads = 10,
            int concurrentCwlDownloads = 10,
            int concurrentPlayerDownloads = 10,
            int concurrentWarLogDownloads = 10,
            int concurrentClanDownloads = 10)
        {
            ConnectionString = connectionString;

            DelayBetweenTasks = delayBetweenTasks ?? TimeSpan.FromMilliseconds(250);

            ConcurrentClanDownloads = concurrentClanDownloads;
            ConcurrentClanWarDownloads = concurrentClanWarDownloads;
            ConcurrentCwlDownloads = concurrentCwlDownloads;
            ConcurrentPlayerDownloads = concurrentPlayerDownloads;
            ConcurrentWarLogDownloads = concurrentWarLogDownloads;
        }
    }
}