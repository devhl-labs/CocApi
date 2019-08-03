using CocApiLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CocApiLibrary.Exceptions
{
    [Serializable]
    public class TokenTimeOutException : ServerResponseException
    {
        public TokenTimeOutException(ResponseMessageAPIModel responseMessage, System.Net.HttpStatusCode httpStatusCode) : base(responseMessage, httpStatusCode)
        {

        }

    }
}
