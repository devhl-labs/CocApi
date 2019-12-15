using System;

using devhl.CocApi.Models;


namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class InvalidTagException : CocApiException
    {
        public InvalidTagException(string message) : base(message)
        {

        }
    }
}
