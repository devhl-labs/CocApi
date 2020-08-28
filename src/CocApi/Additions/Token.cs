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

                    ////todo fix this
                    //CocApiClient.OnLog(this, new LogEventArgs(nameof(IsRateLimited), LogLevel.Warning, "Rate Limited"));
                }
            }
        }

        public Token(/*CocApiClientBase cocApiClient,*/ string token, TimeSpan tokenTimeOut)
        {
            //CocApiClient = cocApiClient;
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
