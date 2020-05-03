using System;
using System.Threading.Tasks;
using System.Timers;

namespace devhl.CocApi
{
    internal sealed class TokenObject
    {
        private bool _isRateLimited = false;
        private readonly System.Timers.Timer _clearRateLimitTimer = new System.Timers.Timer();
        private readonly TimeSpan _tokenTimeOut;
        private readonly CocApi _cocApi;

        public string Token { get; }

        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow.AddSeconds(-30);  //so it does not preemptive rate limit when the program starts

        public bool IsRateLimited
        {
            get
            {
                return _isRateLimited;
            }

            set
            {
                _isRateLimited = value;

                if (value)
                {
                    _clearRateLimitTimer.Start();

                    _cocApi.LogEvent<TokenObject>("Token is rate limited.", LogLevel.Critical, LoggingEvent.IsRateLimited);
                }
            }
        }




        public TokenObject(CocApi cocApi, string token, TimeSpan tokenTimeOut)
        {
            _cocApi = cocApi; 
            Token = token;
            _tokenTimeOut = tokenTimeOut;

            _clearRateLimitTimer.AutoReset = false;
            _clearRateLimitTimer.Interval = 5000;
            _clearRateLimitTimer.Elapsed += ClearRateLimit;
        }

        public async Task<TokenObject> GetTokenAsync()
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUtc;

            bool notified = false;

            while (timeSpan.TotalMilliseconds < _tokenTimeOut.TotalMilliseconds)
            {
                await Task.Delay(50).ConfigureAwait(false);

                if (!notified)
                {
                    _cocApi.LogEvent<TokenObject>($"Preemptive rate limit triggered.", LogLevel.Trace, LoggingEvent.IsPremptiveRateLimited);

                    notified = true;
                }

                timeSpan = DateTime.UtcNow - LastUsedUtc;
            }

            LastUsedUtc = DateTime.UtcNow;

            return this;
        }






        private void ClearRateLimit(object sender, ElapsedEventArgs e) => IsRateLimited = false;
    }
}
