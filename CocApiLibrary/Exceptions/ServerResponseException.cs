using System;
using System.Collections.Generic;
using System.Text;
using CocApiLibrary.Models;

namespace CocApiLibrary.Exceptions
{
    [Serializable]
    public class ServerResponseException : Exception
    {
        public readonly string Reason;

        public readonly System.Net.HttpStatusCode HttpStatusCode;


        public ServerResponseException(ResponseMessageApiModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage.Reason)
        {
            Reason = responseMessage.Reason;
            HttpStatusCode = httpStatusCode;
        }
    }
}
