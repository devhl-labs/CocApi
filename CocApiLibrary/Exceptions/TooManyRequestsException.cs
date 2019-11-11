using System;

using devhl.CocApi.Models;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class TooManyRequestsException : ServerResponseException
    {
        public TooManyRequestsException(ResponseMessageApiModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }

    }
}
