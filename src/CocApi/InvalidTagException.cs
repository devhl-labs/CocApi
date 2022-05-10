using System;

namespace CocApi
{
    [Serializable]
    public sealed class InvalidTagException : Exception
    {
        public InvalidTagException(string tag) : base($"The provided tag {tag} is invalid. Valid tags must not be null nor empty and must start with a #.")
        {

        }
    }
}