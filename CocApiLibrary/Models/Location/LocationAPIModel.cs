using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class LocationModel
    {
        //private CocApi? _cocApi;
        //private ClanAPIModel? _clanApiModel;

        //private bool _changed = false;

        //internal void Process(CocApi cocApi, ClanAPIModel clanApiModel)
        //{
        //    if (_cocApi == null || _clanApiModel == null)
        //    {
        //        _cocApi = cocApi;
        //        _clanApiModel = clanApiModel;
        //    }
        //}

        //internal void FireEvent()
        //{
        //    if (_changed && _cocApi != null && _clanApiModel != null)
        //    {
        //        _changed = false;
        //        _cocApi.ClanLocationChangedEvent(_clanApiModel);
        //    }
        //}



        int _id;
        
        public int Id
        {
            get
            {
                return _id;
            }
        
            set
            {
        	if (_id != value)
        	{
        		_id = value;
        	
        		//if (_cocApi != null)
        		//{
        		//	_changed = true;
        		//}
        	}
            }
        }

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
        	
        		//if (_cocApi != null)
        		//{
        		//	_changed = true;
        		//}
        	}
            }
        }

        bool _isCountry;
        
        public bool IsCountry
        {
            get
            {
                return _isCountry;
            }
        
            set
            {
        	if (_isCountry != value)
        	{
        		_isCountry = value;
        	
        		//if (_cocApi != null)
        		//{
        		//	_changed = true;
        		//}
        	}
            }
        }


        string _countryCode = string.Empty;

        /// <summary>
        /// This only appears when looking up the clantag
        /// It does not appear when looking up locations.
        /// </summary>
        public string CountryCode
        {
            get
            {
                return _countryCode;
            }
        
            set
            {
        	if (_countryCode != value)
        	{
        		_countryCode = value;
        	
        		//if (_cocApi != null)
        		//{
        		//	_changed = true;
        		//}
        	}
            }
        }
    }
}
