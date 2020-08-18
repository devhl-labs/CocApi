//using System;
//using System.Threading.Tasks;
//using System.Timers;

//namespace CocApi.Cache
//{
//    internal sealed class TokenObject_old
//    {
//        private bool _isRateLimited = false;

//        private Timer ClearRateLimitTimer { get; } = new Timer();

//        private TimeSpan TokenTimeOut { get; set; }

//        private CocApiClient_old CocApiClient_old { get; }

//        public string Token { get; }

//        public DateTime LastUsedUtc { get; private set; } = DateTime.UtcNow.AddSeconds(-30);  //so it does not preemptive rate limit when the program starts

//        public bool IsRateLimited
//        {
//            get
//            {
//                return _isRateLimited;
//            }

//            set
//            {
//                _isRateLimited = value;

//                if (value)
//                {
//                    ClearRateLimitTimer.Start();

//                    CocApiClient_old.OnLog(new LogEventArgs(nameof(TokenObject_old), nameof(IsRateLimited), LogLevel.Warning, LoggingEvent.RateLimited.ToString()));
//                }
//            }
//        }

//        public TokenObject_old(CocApiClient_old cocApi, string token, TimeSpan tokenTimeOut)
//        {
//            CocApiClient_old = cocApi; 
//            Token = token;
//            TokenTimeOut = tokenTimeOut;

//            ClearRateLimitTimer.AutoReset = false;
//            ClearRateLimitTimer.Interval = 5000;
//            ClearRateLimitTimer.Elapsed += ClearRateLimit;
//        }

//        public async Task<TokenObject_old> GetTokenAsync()
//        {
//            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUtc;

//            while (timeSpan.TotalMilliseconds < TokenTimeOut.TotalMilliseconds)
//            {
//                await Task.Delay(50).ConfigureAwait(false);

//                timeSpan = DateTime.UtcNow - LastUsedUtc;
//            }

//            LastUsedUtc = DateTime.UtcNow;

//            return this;
//        }

//        private void ClearRateLimit(object sender, ElapsedEventArgs e) => IsRateLimited = false;
//    }
//}
