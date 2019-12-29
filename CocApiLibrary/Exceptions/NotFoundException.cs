using System;

using devhl.CocApi.Models;


namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class NotFoundException : ServerResponseException /*, ICocApiException*/
    {
        public NotFoundException(ResponseMessage responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }
    }
}
