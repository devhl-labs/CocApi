using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi
{        
    [Serializable]
    public class InvalidTagException : Exception
    {
        public InvalidTagException(string tag) : base($"Provided tag {tag} must not be null nor empty and must start with a #.")
        {

        }        
    }
}
