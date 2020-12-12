using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi
{
    public class TokenProvider
    {
        private readonly Dictionary<int, TokenContainer> _tokenProviders = new Dictionary<int, TokenContainer>();

        private volatile int _index = 0;

        public TokenProvider(IEnumerable<string> tokens, TimeSpan tokenTimeOut)
        {
            int i = 0;

            foreach (string token in tokens)
            {
                _tokenProviders.Add(i, new TokenContainer(token, tokenTimeOut));
                i++;
            }
        }

        private readonly object _nextTokenLock = new object();

        private TokenContainer NextToken()
        {
            lock (_nextTokenLock)
            {
                TokenContainer result = _tokenProviders[_index];

                if (_index >= _tokenProviders.Count - 1)
                    _index = 0;
                else
                    _index++;

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

        /// <summary>
        /// Ignore the token timeout rate limit and just grab the token.
        /// </summary>
        /// <returns></returns>
        public string Get()
        {
            TokenContainer container = NextToken();

            return container.Get();
        }
    }
}
