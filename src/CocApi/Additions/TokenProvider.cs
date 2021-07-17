using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi
{
    public class TokenProviderBuilder
    {
        public List<TokenBuilder> Tokens { get; } = new();

        internal TokenProvider Build() => new(Tokens);
    }

    public class TokenProvider
    {
        private readonly Token[] _tokens;

        internal System.Threading.Channels.Channel<Token> AvailableTokens { get; }

        internal TokenProvider()
        {
            
        }

        public TokenProvider(string token, TimeSpan tokenTimeout) : this(new TokenBuilder[] { new TokenBuilder(token, tokenTimeout) })
        {
        }

        public TokenProvider(IEnumerable<TokenBuilder> tokens)
        {
            _tokens = new Token[tokens.Count()];

            AvailableTokens = System.Threading.Channels.Channel.CreateBounded<Token>(new System.Threading.Channels.BoundedChannelOptions(_tokens.Length)
            {
                FullMode = System.Threading.Channels.BoundedChannelFullMode.DropWrite
            });
            
            int i = 0;

            foreach (TokenBuilder tokenBuilder in tokens)
            {
                if (tokens.Count(t => t.RawValue == tokenBuilder.RawValue) > 1)
                    throw new Exception($"Duplicate token provided - {tokenBuilder.RawValue}");

                _tokens[i] = tokenBuilder.Build(this);

                i++;
            }
        }

        internal async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            CancellationToken cancellation = cancellationToken.GetValueOrDefault();

            AvailableTokens.Reader.TryRead(out Token? token);

            if (token != null)
                return token.RawValue;            
            else
                Library.OnLog(this, new LogEventArgs(LogLevel.Trace, message: "Waiting on a token"));

            token = await AvailableTokens.Reader.ReadAsync(cancellation).ConfigureAwait(false);

            return token.RawValue;
        }
    }
}
