using CocApi.Cache.Models;
using System;

namespace CocApi.Cache.Exceptions
{
    [Serializable]
    public class ServerTookTooLongToRespondException : ServerResponseException
    {
        public ServerTookTooLongToRespondException(ResponseMessage responseMessage, System.Net.HttpStatusCode? httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }
    }
}
