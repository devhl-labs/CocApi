using System;

using devhl.CocApi.Models;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class ServiceUnavailableException : ServerResponseException
    {
        public ServiceUnavailableException(ResponseMessageApiModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }
    }
}
