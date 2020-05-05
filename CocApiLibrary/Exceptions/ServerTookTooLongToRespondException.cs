using devhl.CocApi.Models;
using System;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class ServerTookTooLongToRespondException : ServerResponseException
    {
        public ServerTookTooLongToRespondException(ResponseMessage responseMessage, System.Net.HttpStatusCode? httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }
    }
}
