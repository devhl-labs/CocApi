using System;
using System.Collections.Generic;
using System.Text;
using CocApiStandardLibrary.Models;

namespace CocApiStandardLibrary.Exceptions
{
    [Serializable]
    public class CocApiException : Exception
    {



        public CocApiException(string message, Exception exception) : base(message, exception)
        {
        }


        public CocApiException(string message) : base(message)
        {
        }
    }
}
