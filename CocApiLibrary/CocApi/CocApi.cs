using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
//using Microsoft.Data.Sqlite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace devhl.CocApi
{
    public delegate Task ApiIsAvailableChangedEventHandler(object sender, bool isAvailable);
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;
    public delegate Task LogEventHandler(object sender, LogEventArgs log);

    public sealed partial class CocApi : IDisposable
    {
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();
        private readonly object _isAvailableLock = new object();
        private volatile bool _isAvailable = true;

        public CocApi(CocApiConfiguration cfg)
        {
            //SQLiteConnection.CreateFile("MyDatabase.sqlite");

            //SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");

            //m_dbConnection.Open();

            //string sql = "create table highscores (name varchar(20), score int)";

            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

            //command.ExecuteNonQuery();

            //string sql = "insert into highscores (name, score) values ('Me', 9001)";

            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //command.ExecuteNonQuery();

            //m_dbConnection.Close();






            if (cfg != null)
            {
                CocApiConfiguration = cfg;
            }

            if (cfg == null || cfg.Tokens.Count == 0)
            {
                throw new CocApiException("You did not provide any tokens to access the SC Api.");
            }

            Villages = new Villages(this);

            Clans = new Clans(this);

            Wars = new Wars(this);

            Labels = new Labels(this);

            Leagues = new Leagues(this);

            Locations = new Locations(this);

            Test = new Test(this);

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            Clans.CreateClanUpdateServices();
        }

        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;

        public event LogEventHandler? Log;


        public Clans Clans { get; set; }

        public bool IsAvailable
        {
            get
            {
                lock (_isAvailableLock)
                {
                    return _isAvailable;
                }
            }

            internal set
            {
                lock (_isAvailableLock)
                {
                    if (_isAvailable != value)
                    {
                        _isAvailable = value;

                        ApiIsAvailableChanged?.Invoke(this, _isAvailable);
                    }
                }
            }
        }

        public Labels Labels { get; private set; }
        public Leagues Leagues { get; private set; }
        public Locations Locations { get; private set; }
        public Villages Villages { get; private set; }
        public Wars Wars { get; private set; }
        public Test Test { get; private set; }

        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        /// <summary>
        /// Disposes all disposable items.  Pending tasks will be canceled.
        /// </summary>
        public void Dispose()
        {
            foreach (ClanUpdateGroup updateService in Clans.ClanUpdateGroups)
            {
                updateService.StopUpdating();
            }

            foreach (CancellationTokenSource cancellationTokenSource in _cancellationTokenSources)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception)
                {
                }

                cancellationTokenSource.Dispose();
            }

            WebResponse.HttpClient.Dispose();

            WebResponse.SemaphoreSlim.Dispose();
        }

        /// <summary>
        /// Use this to get statistics on how long the Api takes to respond for diffent and points.
        /// </summary>
        /// <returns></returns>
        public ConcurrentBag<WebResponseTimer> GetTimers() => WebResponse.GetTimers();

        public string GetTokenStatus() => WebResponse.GetTokenStatus();

        internal async Task ClanQueueRestartAsync()
        {
            try
            {
                //wait to allow the updater to finish crashing
                await Task.Delay(5000).ConfigureAwait(false);

                Clans.StartQueue();
            }
            catch (Exception)
            {
                OnLog(new LogEventArgs(nameof(CocApi), nameof(ClanQueueRestartAsync), LogLevel.Critical, LoggingEvent.QueueRestartFailed.ToString()));
            }
        }

        internal async Task<IDownloadable?> FetchAsync<TResult>(string url, CancellationToken? cancellationToken = null) where TResult : class, IDownloadable /*, new()*/
        {
            try
            {
                using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken ?? new CancellationToken());

                TokenObject token = await WebResponse.GetTokenAsync();

                AddCancellationTokenSource(cts);

                if (cancellationToken == null)
                    cts.CancelAfter(CocApiConfiguration.TimeToWaitForWebRequests);

                IDownloadable? result = await WebResponse.GetDownloadableAsync<TResult>(url, token, cts.Token).ConfigureAwait(false);

                RemoveCancellationTokenSource(cts);

                return result;
            }
            catch (Exception e)
            {
                if (e is CocApiException)
                    throw;

                throw new CocApiException(e.Message, e);
            }
        }

        internal void OnLog(LogEventArgs log) => Log?.Invoke(this, log);

        internal void UpdateDictionary<T>(ConcurrentDictionary<string, T> dictionary, string key, T downloadable) where T : class?, IDownloadable?
        {
            dictionary.AddOrUpdate(key, downloadable, (_, existingItem) =>
            {
                if (existingItem == null)
                    return downloadable;

                if (downloadable == null)
                    return existingItem;

                if (existingItem.ServerExpirationUtc > downloadable.ServerExpirationUtc)
                    return existingItem;

                return downloadable;
            });
        }

        internal async Task VillageQueueRestartAsync()
        {
            try
            {
                //wait to allow the updater to finish crashing
                await Task.Delay(5000).ConfigureAwait(false);

                Villages.StartQueue();
            }
            catch (Exception)
            {
                OnLog(new LogEventArgs(nameof(CocApi), nameof(VillageQueueRestartAsync), LogLevel.Critical, LoggingEvent.QueueRestartFailed.ToString()));
            }
        }

        internal async Task WarQueueRestartAsync()
        {
            try
            {
                //wait to allow the updater to finish crashing
                await Task.Delay(5000).ConfigureAwait(false);

                Wars.StartQueue();
            }
            catch (Exception)
            {
                OnLog(new LogEventArgs(nameof(CocApi), nameof(WarQueueRestartAsync), LogLevel.Critical, LoggingEvent.QueueRestartFailed.ToString()));
            }
        }

        private void AddCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Add(cts);
            }
        }

        private void RemoveCancellationTokenSource(CancellationTokenSource cts)
        {
            lock (_cancellationTokenSources)
            {
                _cancellationTokenSources.Remove(cts);
            }
        }
    }
}