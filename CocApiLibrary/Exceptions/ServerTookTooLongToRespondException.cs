using System;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class ServerTookTooLongToRespondException : CocApiException
    {



        public ServerTookTooLongToRespondException(string message, Exception exception) : base(message, exception)
        {
        }


        public ServerTookTooLongToRespondException(string message) : base(message)
        {
        }
    }
}
