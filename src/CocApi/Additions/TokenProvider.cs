using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi
{
    public class TokenProviderBuilder
    {
        public List<string> Tokens { get; set; } = new();

        public TimeSpan TokenTimeout { get; set; }

        internal TokenProvider Build() => new(Tokens, TokenTimeout);
    }

    public class TokenProvider
    {
        private readonly Dictionary<int, TokenContainer> _tokenProviders = new();

        private volatile int _index;

        internal TokenProvider()
        {

        }

        public TokenProvider(string token, TimeSpan tokenTimeout) : this(new string[] { token }, tokenTimeout)
        {

        }

        public TokenProvider(IEnumerable<string> tokens, TimeSpan tokenTimeOut)
        {
            int i = 0;

            foreach (string token in tokens)
            {
                _tokenProviders.Add(i, new TokenContainer(token, tokenTimeOut));
                i++;
            }
        }

        private readonly object _nextTokenLock = new();

        private TokenContainer NextToken()
        {
            lock (_nextTokenLock)
            {
                TokenContainer result = _tokenProviders[_index];

                _index = _index >= _tokenProviders.Count - 1
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
            TokenContainer container = NextToken();

            return await container.GetAsync(cancellationToken);
        }
    }
}
