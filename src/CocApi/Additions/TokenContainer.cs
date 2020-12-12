using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CocApi
{
    internal sealed class TokenContainer
    {
        private TimeSpan TokenTimeOut { get; set; }

        private readonly string _token;

        public DateTime LastUsedUtc { get; private set; }

        public TokenContainer(string token, TimeSpan tokenTimeOut)
        {
            _token = token;
            TokenTimeOut = tokenTimeOut;
        }

        private readonly object _getLock = new object();

        public async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUtc;

            if (timeSpan.TotalMilliseconds < TokenTimeOut.TotalMilliseconds)            
                await Task.Delay((int)timeSpan.TotalMilliseconds, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

            lock (_getLock)
            {
                LastUsedUtc = DateTime.UtcNow;
            }

            return _token;
        }

        public string Get()
        {
            lock (_getLock)
            {
                LastUsedUtc = DateTime.UtcNow;
            }

            return _token;
        }
    }
}
