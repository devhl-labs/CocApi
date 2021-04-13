using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CocApi
{
    public class TokenBuilder
    {
        public TimeSpan TokenTimeOut { get; }

        public string RawValue { get; }

        public TokenBuilder(string token, TimeSpan tokenTimeOut)
        {
            RawValue = token;
            TokenTimeOut = tokenTimeOut;
        }

        internal Token Build() => new(RawValue, TokenTimeOut);
    }

    internal sealed class Token
    {
        private TimeSpan TokenTimeOut { get; set; }

        private readonly string _rawValue;

        private DateTime _lastUsedUtc;

        public Token(string token, TimeSpan tokenTimeOut)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token must not be null or whitespace.", nameof(token));

            if (tokenTimeOut == null || tokenTimeOut == TimeSpan.MinValue)
                throw new ArgumentException("Invalid token timeout value.", nameof(tokenTimeOut));

            _rawValue = token;
            TokenTimeOut = tokenTimeOut;
        }

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            await _semaphore.WaitAsync(cancellationToken.GetValueOrDefault());

            try
            {
                DateTime now = DateTime.UtcNow;

                DateTime available = _lastUsedUtc.Add(TokenTimeOut);

                if (now < available)            
                    await Task.Delay(available - now, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

                _lastUsedUtc = now;

                return _rawValue;
            }
            finally
            {
                _semaphore.Release();
            }
        } 
    }
}
