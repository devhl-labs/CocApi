using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static CocApiLibrary.Enums;


namespace CocApiLibrary
{
    internal class TokenObject
    {
        private bool _isRateLimited = false;
        private readonly Timer _clearRateLimitTimer = new Timer();
        internal readonly string Token;
        private readonly int _tokenTimeOut;
        //private readonly VerbosityType _verbosityType;
        private readonly CocApi _cocApi;

        public DateTime LastUsedUTC { get; private set; } = DateTime.UtcNow;

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
                    _cocApi.Logger.Invoke(new LogMessage(LogSeverity.Warning, nameof(TokenObject), "Token is rate limited"));
                }
            }
        }






        public TokenObject(CocApi cocApi, string token, int tokenTimeOut)
        {
            _cocApi = cocApi; 
            Token = token;
            _tokenTimeOut = tokenTimeOut;
            //_verbosityType = verbosityType;

            _clearRateLimitTimer.AutoReset = false;
            _clearRateLimitTimer.Interval = 5000;
            _clearRateLimitTimer.Elapsed += ClearRateLimit;
        }

        public async Task<TokenObject> GetToken(string url)
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUTC;

            bool notified = false;

            while (timeSpan.TotalMilliseconds < _tokenTimeOut)
            {
                await Task.Delay(50);

                if (!notified)
                {
                    _ = _cocApi.Logger.Invoke(new LogMessage(LogSeverity.Warning, nameof(TokenObject), "Preemptive rate limit"));
                    notified = true;
                }

                timeSpan = DateTime.UtcNow - LastUsedUTC;
            }

            LastUsedUTC = DateTime.UtcNow;
            return this;
        }






        private void ClearRateLimit(object sender, ElapsedEventArgs e)
        {
            IsRateLimited = false;
        }


    }
}
