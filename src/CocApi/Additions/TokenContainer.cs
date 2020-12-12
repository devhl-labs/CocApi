using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace CocApi
{
    internal sealed class TokenContainer
    {
        //private bool _isRateLimited = false;

        //private System.Timers.Timer ClearRateLimitTimer { get; } = new System.Timers.Timer();

        private TimeSpan TokenTimeOut { get; set; }

        private readonly string _token;

        public DateTime LastUsedUtc { get; private set; }

        //public bool IsRateLimited
        //{
        //    get
        //    {
        //        return _isRateLimited;
        //    }

        //    set
        //    {
        //        _isRateLimited = value;

        //        if (value)
        //            ClearRateLimitTimer.Start();
        //    }
        //}

        public TokenContainer(string token, TimeSpan tokenTimeOut)
        {
            _token = token;
            TokenTimeOut = tokenTimeOut;

            //ClearRateLimitTimer.AutoReset = false;
            //ClearRateLimitTimer.Interval = 5000;
            //ClearRateLimitTimer.Elapsed += ClearRateLimit;
        }

        public async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            TimeSpan timeSpan = DateTime.UtcNow - LastUsedUtc;

            if (timeSpan.TotalMilliseconds < TokenTimeOut.TotalMilliseconds)            
                await Task.Delay((int)timeSpan.TotalMilliseconds, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);

            LastUsedUtc = DateTime.UtcNow;

            return _token;
        }

        //private void ClearRateLimit(object sender, ElapsedEventArgs e) => IsRateLimited = false;
    }
}
