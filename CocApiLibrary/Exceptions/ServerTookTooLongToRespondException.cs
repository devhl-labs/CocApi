using devhl.CocApi.Models;
using System;

namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class ServerTookTooLongToRespondException : ServerResponseException // CocApiException
    {



        //public ServerTookTooLongToRespondException(string message, Exception exception) : base(message, exception)
        //{
        //}


        //public ServerTookTooLongToRespondException(string message) : base(message)
        //{
        //}

        public ServerTookTooLongToRespondException(ResponseMessage responseMessage, System.Net.HttpStatusCode? httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }
    }
}
