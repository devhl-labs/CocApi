using System;

using devhl.CocApi.Models;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class ServerResponseException : CocApiException
    {
        public readonly string Reason;

        public readonly System.Net.HttpStatusCode? HttpStatusCode;


        public ServerResponseException(ResponseMessageApiModel responseMessage, System.Net.HttpStatusCode? httpStatusCode) : base(responseMessage.Reason)
        {
            Reason = responseMessage.Reason;
            HttpStatusCode = httpStatusCode;
        }
    }
}
