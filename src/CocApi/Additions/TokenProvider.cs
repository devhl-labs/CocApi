﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi
{
    public class TokenProvider
    {
        private readonly List<Token> _tokenObjects = new List<Token>();

        private readonly SemaphoreSlim _tokenSemaphore = new SemaphoreSlim(1, 1);

        public TokenProvider(List<string> tokens, TimeSpan tokenTimeOut)
        {
            foreach (string token in tokens)
                _tokenObjects.Add(new Token(token, tokenTimeOut));
        }

        internal async Task<string> GetTokenAsync()
        {
            try
            {
                await _tokenSemaphore.WaitAsync().ConfigureAwait(false);

                return await _tokenObjects.Where(x => !x.IsRateLimited).OrderBy(x => x.LastUsedUtc).First().GetTokenAsync().ConfigureAwait(false);
            }
            finally
            {
                _tokenSemaphore.Release();
            }
        }
    }
}