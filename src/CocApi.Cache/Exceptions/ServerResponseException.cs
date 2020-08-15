using System;

using CocApi.Cache.Models;

namespace CocApi.Cache.Exceptions
{
    [Serializable]
    public class ServerResponseException : CocApiException
    {
        public readonly string Reason;

        public readonly System.Net.HttpStatusCode? HttpStatusCode;


        public ServerResponseException(ResponseMessage responseMessage, System.Net.HttpStatusCode? httpStatusCode) : base(responseMessage.Reason)
        {
            Reason = responseMessage.Reason;
            HttpStatusCode = httpStatusCode;
        }
    }
}
