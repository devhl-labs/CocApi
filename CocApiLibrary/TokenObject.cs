using System;
using System.Threading.Tasks;
using System.Timers;

namespace devhl.CocApi
{
    internal sealed class TokenObject
    {
        private bool _isRateLimited = false;

        private Timer ClearRateLimitTimer { get; } = new Timer();

        private TimeSpan TokenTimeOut { get; set; }

        private CocApi CocApi { get; }

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
                    ClearRateLimitTimer.Start();

                    CocApi.OnLog(new LogEventArgs(nameof(TokenObject), nameof(IsRateLimited), LogLevel.Warning, LoggingEvent.RateLimited.ToString()));
                }
            }
        }

        public TokenObject(CocApi cocApi, string token, TimeSpan tokenTimeOut)
        {
            CocApi = cocApi; 
            Token = token;
            TokenTimeOut = tokenTimeOut;

            ClearRateLimitTimer.AutoReset = false;
            ClearRateLimitTimer.Interval = 5000;
            ClearRateLimitTimer.Elapsed += ClearRateLimit;
        }

        public async Task<TokenObject> GetTokenAsync()
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUtc;

            while (timeSpan.TotalMilliseconds < TokenTimeOut.TotalMilliseconds)
            {
                await Task.Delay(50).ConfigureAwait(false);

                timeSpan = DateTime.UtcNow - LastUsedUtc;
            }

            LastUsedUtc = DateTime.UtcNow;

            return this;
        }

        private void ClearRateLimit(object sender, ElapsedEventArgs e) => IsRateLimited = false;
    }
}
