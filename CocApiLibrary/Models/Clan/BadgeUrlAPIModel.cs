using System;
using System.Text.Json.Serialization;

namespace CocApiLibrary.Models
{
    public class BadgeUrlModel
    {
        private CocApi? _cocApi;
        private ClanAPIModel? _clanApiModel;

        private bool _changed = false;

        internal void Process(CocApi cocApi, ClanAPIModel clanApiModel)
        {
            if(_cocApi == null || _clanApiModel == null)
            {
                _cocApi = cocApi;
                _clanApiModel = clanApiModel;
            }
        }

        internal bool IsProcessed()
        {
            if(_cocApi == null || _clanApiModel == null)
            {
                return false;
            }
            return true;
        }

        internal void FireEvent()
        {
            if (_changed && _cocApi != null && _clanApiModel != null)
            {
                _changed = false;
                _cocApi.ClanBadgeUrlChangedEvent(_clanApiModel);
            }
        }



        private string _small = string.Empty;
        
        public string Small
        {
            get
            {
                return _small;
            }
        
            set
            {
        	    if(_small != value)
        	    {
        		    _small = value;
        	
        		    if(_cocApi != null && _clanApiModel  != null)
        		    {
                         _changed = true;
        		    }
        	    }
            }
        }

        private string _large = string.Empty;
        
        public string Large
        {
            get
            {
                return _large;
            }
        
            set
            {
        	    if(_large != value)
        	    {
        		    _large = value;
        	
        		    if(_cocApi != null && _clanApiModel != null)
        		    {
                        _changed = true;
        		    }
        	    }
            }
        }

        private string _medium = string.Empty;
        
        public string Medium
        {
            get
            {
                return _medium;
            }
        
            set
            {
        	    if(_medium != value)
        	    {
        		    _medium = value;
        	
        		    if(_cocApi != null && _clanApiModel != null)
        		    {
                        _changed = true;
        		    }
        	    }
            }
        }
    }
}
