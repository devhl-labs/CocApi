using System;
using System.Timers;

namespace CocApi
{
    public sealed class TokenBuilder
    {
        public TimeSpan TokenTimeOut { get; }

        public string RawValue { get; }

        public TokenBuilder(string token, TimeSpan tokenTimeOut)
        {
            RawValue = token;
            TokenTimeOut = tokenTimeOut;
        }

        internal Token Build(TokenProvider tokenProvider) => new(RawValue, TokenTimeOut, tokenProvider);
    }

    internal sealed class Token
    {
        internal string RawValue { get; }
        private readonly TokenProvider _tokenProvider;
        private readonly System.Timers.Timer _timer = new();

        public Token(string token, TimeSpan tokenTimeOut, TokenProvider tokenProvider)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token must not be null or whitespace.", nameof(token));

            if (tokenTimeOut == null || tokenTimeOut == TimeSpan.MinValue)
                throw new ArgumentException("Invalid token timeout value.", nameof(tokenTimeOut));

            RawValue = token;
            _tokenProvider = tokenProvider;

            _timer.Interval = tokenTimeOut.TotalMilliseconds;
            _timer.Elapsed += OnTimer;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            _tokenProvider.AvailableTokens.Writer.TryWrite(this);
        }
    }
}
