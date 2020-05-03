using devhl.CocApi.Exceptions;
using devhl.CocApi.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace devhl.CocApi
{
    public delegate Task AsyncEventHandler(object sender, EventArgs e);
    public delegate Task AsyncEventHandler<T>(object sender, T e) where T : EventArgs;

    public delegate Task ApiIsAvailableChangedEventHandler(bool isAvailable);

    public delegate Task LogEventHandler(string source, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown, string? message = null);

    public sealed class CocApi : IDisposable
    {
        private readonly List<CancellationTokenSource> _cancellationTokenSources = new List<CancellationTokenSource>();
        private readonly object _isAvailableLock = new object();
        private volatile bool _isAvailable = true;

        public CocApi(CocApiConfiguration cfg)
        {
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

            WebResponse.Initialize(this, CocApiConfiguration, cfg.Tokens);

            Clans.CreateClanUpdateServices();
        }

        /// <summary>
        /// Fires if you query the Api during an outage.
        /// If the service is not available, you may still try to query the Api if you wish.
        /// </summary>
        public event ApiIsAvailableChangedEventHandler? ApiIsAvailableChanged;

        public event LogEventHandler? Log;

        public static Regex ValidTagCharacters { get; } = new Regex(@"^#[PYLQGRJCUV0289]+$");
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

                        ApiIsAvailableChanged?.Invoke(_isAvailable);
                    }
                }
            }
        }

        public Labels Labels { get; set; }
        public Leagues Leagues { get; set; }
        public Locations Locations { get; set; }
        public Villages Villages { get; set; }
        public Wars Wars { get; set; }

        internal CocApiConfiguration CocApiConfiguration { get; private set; } = new CocApiConfiguration();

        public static bool IsValidTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return false;
            }

            if (tag == "#0")
            {
                return false;
            }

            return ValidTagCharacters.IsMatch(tag);
        }

        public static bool TryGetValidTag(string userInput, out string formattedTag)
        {
            formattedTag = string.Empty;

            if (string.IsNullOrEmpty(userInput))
                return false;

            formattedTag = userInput.ToUpper();

            formattedTag = formattedTag.Replace("O", "0");

            if (!formattedTag.StartsWith("#"))
                formattedTag = $"#{formattedTag}";

            var result = IsValidTag(formattedTag);

            if (!result)
                formattedTag = string.Empty;

            return result;
        }

        /// <summary>
        /// Disposes all disposable items.  Pending tasks will be canceled.
        /// </summary>
        public void Dispose()
        {
            foreach (ClanUpdateService updateService in Clans._clanUpdateServices)
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
                LogEvent<CocApi>($"A clan queue crashed and could not be restarted.", LogLevel.Critical, LoggingEvent.QueueCrashed);
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

        internal void LogEvent(string source, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown, string? message = null) => Log?.Invoke(source, logLevel, loggingEvent, message);

        internal void LogEvent<T>(string? message, LogLevel logLevel = LogLevel.Trace, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(typeof(T).Name, logLevel, loggingEvent, message);

        internal void LogEvent<T>(Exception exception, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(typeof(T).Name, logLevel, loggingEvent, exception.Message);

        internal void LogEvent(string source, Exception exception, LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(source, logLevel, loggingEvent, exception.Message);

        internal void LogEvent<T>(LogLevel logLevel = LogLevel.Debug, LoggingEvent loggingEvent = LoggingEvent.Unknown) => LogEvent(typeof(T).Name, logLevel, loggingEvent, null);

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
                LogEvent<CocApi>($"The war queue crashed and could not be restarted.", LogLevel.Critical, LoggingEvent.QueueCrashed);
            }
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
                LogEvent<CocApi>($"The village queue crashed and could not be restarted.", LogLevel.Critical, LoggingEvent.QueueCrashed);
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