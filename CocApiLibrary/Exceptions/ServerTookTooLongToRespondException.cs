using System;
using System.Collections.Generic;
using System.Text;
using CocApiLibrary.Models;

namespace CocApiLibrary.Exceptions
{
    [Serializable]
    public class ServerTookTooLongToRespondException : CocApiException
    {



        public ServerTookTooLongToRespondException(string message, Exception exception) : base(message, exception)
        {
        }


        public ServerTookTooLongToRespondException(string message) : base(message)
        {
        }
    }
}
