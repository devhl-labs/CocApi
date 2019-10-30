using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LocationApiModel
    {
        public int Id { get; set; }

        string _name = string.Empty;
        
        public string Name
        {
            get
            {
                return _name;
            }
        
            set
            {
        	    if (_name != value)
        	    {
        		    _name = value;
        	    }
            }
        }

        public bool IsCountry { get; set; }

        /// <summary>
        /// This only appears when looking up the clantag
        /// It does not appear when looking up locations.
        /// </summary>
        public string CountryCode { get; set; } = string.Empty;
    }
}
