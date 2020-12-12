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

        public async ValueTask<string> GetAsync(CancellationToken? cancellationToken = null)
        {
            TokenContainer container = NextToken();

            return await container.GetAsync(cancellationToken);
        }
    }
}
