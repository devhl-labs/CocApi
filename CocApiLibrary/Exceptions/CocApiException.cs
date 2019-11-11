using System;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class CocApiException : Exception
    {
        public CocApiException(string message, Exception exception) : base(message, exception)
        {
        }

        public CocApiException(string message) : base(message)
        {
        }
    }
}
