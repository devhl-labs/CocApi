using System;

using devhl.CocApi.Models;


namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class NotFoundException : ServerResponseException
    {
        public NotFoundException(ResponseMessageApiModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }

    }
}
