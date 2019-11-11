using System;

using devhl.CocApi.Models;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class ForbiddenException : ServerResponseException
    {
        public ForbiddenException(ResponseMessageApiModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }

    }
}
