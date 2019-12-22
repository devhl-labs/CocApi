using System;

using devhl.CocApi.Models;


namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class InvalidTagException : CocApiException
    {
        public InvalidTagException() : base("Tags must not be null nor empty and must start with a #.")
        {
        }
    }
}
