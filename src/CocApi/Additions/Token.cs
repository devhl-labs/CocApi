using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CocApi
{
    internal sealed class Token
    {
        private bool _isRateLimited = false;

        private Timer ClearRateLimitTimer { get; } = new Timer();

        private TimeSpan TokenTimeOut { get; set; }

        private readonly string _token;

        public DateTime LastUsedUtc { get; private set; }

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
                }
            }
        }

        public Token(string token, TimeSpan tokenTimeOut)
        {
            _token = token;
            TokenTimeOut = tokenTimeOut;

            ClearRateLimitTimer.AutoReset = false;
            ClearRateLimitTimer.Interval = 5000;
            ClearRateLimitTimer.Elapsed += ClearRateLimit;
        }

        public async Task<string> GetTokenAsync()
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUtc;

            while (timeSpan.TotalMilliseconds < TokenTimeOut.TotalMilliseconds)
            {
                await Task.Delay(50).ConfigureAwait(false);

                timeSpan = DateTime.UtcNow - LastUsedUtc;
            }

            LastUsedUtc = DateTime.UtcNow;

            return _token;
        }

        private void ClearRateLimit(object sender, ElapsedEventArgs e) => IsRateLimited = false;
    }
}
