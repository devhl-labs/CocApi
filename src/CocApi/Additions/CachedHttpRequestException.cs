using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi
{
    [Serializable]
    public class CachedHttpRequestException : Exception
    {
        public CachedHttpRequestException(string messge) : base(messge)
        {

        }
    }
}