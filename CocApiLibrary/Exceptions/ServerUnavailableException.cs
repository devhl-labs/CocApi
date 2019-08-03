using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Exceptions
{
    [Serializable]
    public class ServerUnavailableException : ServerResponseException
    {
        public ServerUnavailableException(ResponseMessageAPIModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }

    }
}
