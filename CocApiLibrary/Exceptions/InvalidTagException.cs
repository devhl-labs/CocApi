using System;

using devhl.CocApi.Models;


namespace devhl.CocApi.Exceptions
{
    [Serializable]
    public class InvalidTagException : CocApiException
    {
        public InvalidTagException(string tag) : base($"Provided tag {tag} must not be null nor empty and must start with a #.")
        {
        }
    }
}
