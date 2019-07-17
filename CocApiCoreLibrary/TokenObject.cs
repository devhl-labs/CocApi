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
        private Timer _clearRateLimitTimer = new Timer();
        internal readonly string Token;
        private readonly int _tokenTimeOut;
        private readonly VerbosityType _verbosityType;


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
                    //if(_verbosityType > VerbosityType.None)
                    //{
                    Console.WriteLine("CocAPI Token is Rate Limited");
                    //}
                }
            }
        }






        public TokenObject(string token, int tokenTimeOut, VerbosityType verbosityType)
        {
            Token = token;
            _tokenTimeOut = tokenTimeOut;
            _verbosityType = verbosityType;

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

                if (!notified && _verbosityType >= VerbosityType.PreemptiveRateLimits)
                {
                    Console.WriteLine($"CoC API preemptive rate limiting {url}");
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
