using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using static CocApiStandardLibrary.Enums;
using CocApiStandardLibrary.Models;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics;
using CocApiStandardLibrary.Exceptions;
using System.Text.Json;
using System.Reflection.Emit;
using CocApiLibrary;

namespace CocApiStandardLibrary
{
    internal class ApiHelper
    {

        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly IList<TokenObject> _tokenObjects = new List<TokenObject>();
        private readonly HttpClient ApiClient = new HttpClient();
        private readonly ConcurrentDictionary<string, StoredItem> _cachedItems = new ConcurrentDictionary<string, StoredItem>();
        private readonly int _timeToWaitForWebRequest;
        private readonly VerbosityType _verbosityType;
        private int counter = 0;

        private readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiHelper(int timeToWaitForWebRequests, VerbosityType verbosityType, IEnumerable<string> tokens, int tokenTimeOutInMilliseconds)
        {
            _timeToWaitForWebRequest = timeToWaitForWebRequests;
            _verbosityType = verbosityType;

            _verbosityType = verbosityType;

            ApiClient.DefaultRequestHeaders.Accept.Clear();

            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            foreach (string token in tokens)
            {
                TokenObject tokenObject = new TokenObject(token, tokenTimeOutInMilliseconds, verbosityType);

                _tokenObjects.Add(tokenObject);
            }

            options.Converters.Add(new JsonStringEnumConverter());
        }





        public async Task<T> GetResponse<T>(CocApi cocApi, string encodedUrl, bool allowCachedItem = true) where T : class, new()
        {
            T? result = null;

            if (allowCachedItem)
            {
                result = GetStoredItem<T>(encodedUrl);
            }

            if (result == null)
            {
                result = await GetWebResponse<T>(cocApi, encodedUrl);
            }

            return result;
        }

        public string GetTokenStatus()
        {
            return $"{_tokenObjects.Count(x => x.IsRateLimited)} Rate Limited\n{_tokenObjects.Count(x => !x.IsRateLimited)} not rate limited";
        }









        private async Task<TokenObject> GetToken(string url)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (_verbosityType == VerbosityType.Verbose)
                {
                    Console.WriteLine($"counter: {counter};  {url}");
                }

                counter++;
                while (_tokenObjects.All(x => x.IsRateLimited))
                {
                    await Task.Delay(100);
                }

                return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUTC).FirstOrDefault().GetToken(url);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private T GetStoredItem<T>(string encodedUrl) where T : class, new()
        {
            var cachedItemKVP = _cachedItems.Where(c => c.Key == encodedUrl).OrderByDescending(c => c.Value.DateTimeUTC).FirstOrDefault();

            if (cachedItemKVP.Value != null && cachedItemKVP.Value.Expires > DateTime.UtcNow)
            {
                return (T)cachedItemKVP.Value.DownloadedItem;
            }
            else if (cachedItemKVP.Value != null)
            {
                _cachedItems.TryRemove(cachedItemKVP.Key, out StoredItem value);
            }

            return default;
        }

        private async Task<T> GetWebResponse<T>(CocApi cocApi, string encodedUrl) where T : new()
        {
            TokenObject token = await GetToken(encodedUrl);

            ApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(_timeToWaitForWebRequest);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            using HttpResponseMessage response = await ApiClient.GetAsync(encodedUrl, cancellationTokenSource.Token);

            stopwatch.Stop();

            string responseText = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                cocApi.IsAvailable = true;

                T result = JsonSerializer.Deserialize<T>(responseText, options); 

                StoredItem cachedItem = new StoredItem(result, stopwatch);

                _cachedItems.TryAdd(encodedUrl, cachedItem);

                return (T)cachedItem.DownloadedItem;
            }
            else
            {
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError || response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) //500 or 503
                {
                    cocApi.IsAvailable = false;
                }
                else if (response.StatusCode == (System.Net.HttpStatusCode) 429) //custom response for rate limited
                {
                    token.IsRateLimited = true;
                }

                ResponseMessage ex = JsonSerializer.Deserialize<ResponseMessage>(responseText, options);

                throw new ServerResponseException(ex, response.StatusCode);
            }
        }




    }
}


