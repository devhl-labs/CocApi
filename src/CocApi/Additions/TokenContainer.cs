using System.Collections.Generic;

namespace CocApi
{
    public sealed class TokenContainer
    {
        public List<TokenBuilder> Tokens { get; } = new();
    }
}
