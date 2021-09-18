using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CocApi
{
    public sealed class TokenProvider
    {
        private readonly Token[] _tokens;

        internal System.Threading.Channels.Channel<Token> AvailableTokens { get; }
        internal ILogger<TokenProvider> Logger { get; }

        public TokenProvider(ILogger<TokenProvider> logger, TokenContainer container)
        {
            if (container.Tokens.Count == 0)
                throw new ArgumentException("You did not provide any tokens.", nameof(container));

            _tokens = new Token[container.Tokens.Count];

            AvailableTokens = System.Threading.Channels.Channel.CreateBounded<Token>(new System.Threading.Channels.BoundedChannelOptions(_tokens.Length)
            {
                FullMode = System.Threading.Channels.BoundedChannelFullMode.DropWrite
            });

            int i = 0;

            foreach (TokenBuilder tokenBuilder in container.Tokens)
            {
                if (container.Tokens.Count(t => t.RawValue == tokenBuilder.RawValue) > 1)
                    throw new ArgumentException($"Duplicate token provided - {tokenBuilder.RawValue}", nameof(container));

                _tokens[i] = tokenBuilder.Build(this);

                i++;
            }

            Logger = logger;
        }

        internal async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            CancellationToken cancellation = cancellationToken.GetValueOrDefault();

            AvailableTokens.Reader.TryRead(out Token? token);

            if (token != null)
                return token.RawValue;
            else
                Logger.LogTrace("Waiting on a token.");

            token = await AvailableTokens.Reader.ReadAsync(cancellation).ConfigureAwait(false);

            return token.RawValue;
        }
    }
}
