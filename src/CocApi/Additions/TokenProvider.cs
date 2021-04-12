using System;
using System.Collections.Generic;
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
        private readonly Dictionary<int, Token> _tokens = new();

        private volatile int _index;

        internal TokenProvider()
        {

        }

        public TokenProvider(string token, TimeSpan tokenTimeout) : this(new TokenBuilder[] { new TokenBuilder(token, tokenTimeout) })
        {

        }

        public TokenProvider(IEnumerable<TokenBuilder> tokens)
        {
            int i = 0;

            foreach (TokenBuilder tokenBuilder in tokens)
            {
                _tokens.Add(i, tokenBuilder.Build());
                i++;
            }
        }

        private readonly object _nextTokenLock = new();

        private Token NextToken()
        {
            lock (_nextTokenLock)
            {
                Token result = _tokens[_index];

                _index = _index >= _tokens.Count - 1
                    ? 0
                    : _index++;

                return result;
            }
        }

        /// <summary>
        /// Returns the token after awaiting the token timeout rate limit.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            Token token = NextToken();

            return await token.GetAsync(cancellationToken);
        }
    }
}
