﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CocApi
{
    internal sealed class TokenContainer
    {
        private TimeSpan TokenTimeOut { get; set; }

        private readonly string _token;

        private DateTime _lastUsedUtc;

        public TokenContainer(string token, TimeSpan tokenTimeOut)
        {
            _token = token;
            TokenTimeOut = tokenTimeOut;
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            await _semaphore.WaitAsync();

            try
            {
                DateTime now = DateTime.UtcNow;

                DateTime available = _lastUsedUtc.Add(TokenTimeOut);

                if (now < available)            
                    await Task.Delay(available - now, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

                _lastUsedUtc = now;
            }
            finally
            {
                _semaphore.Release();
            }

            return _token;
        }

        public string Get() => _token;        
    }
}
