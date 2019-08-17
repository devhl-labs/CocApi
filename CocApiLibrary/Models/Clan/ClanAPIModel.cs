using CocApiLibrary.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static CocApiLibrary.Enums;

namespace CocApiLibrary.Models
{
    public class ClanAPIModel : IClanAPIModel, IDownloadable
    {
        private CocApi? _cocApi;
        private bool _changed = false;

        public void Process(CocApi cocApi)
        {
            _cocApi = cocApi;

            //if (BadgeUrls != null)
            //{
            //    BadgeUrls.Process(_cocApi, this);
            //}

            //if (Location != null)
            //{
            //    Location.Process(_cocApi, this);
            //}
        }

        internal void FireEvent()
        {
            if (_changed && _cocApi != null)
            {
                _changed = false;
                //_cocApi.ClanChangedEvent(this);
            }
        }

        public void SetExpiration()
        {
            throw new NotImplementedException();
        }

        private string _tag = string.Empty;
        
        public string Tag
        {
            get
            {
                return _tag;
            }
        
            set
            {
        	    if(_tag != value)
        	    {
        		    _tag = value;
        	        
                    EncodedUrl = $"https://api.clashofclans.com/v1/clans/{Uri.EscapeDataString(_tag)}";
                }
            }
        }

        private string _name = string.Empty;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        public BadgeUrlModel? BadgeUrls { get; set; }

        //BadgeUrlModel? _badgeUrls;

        //public BadgeUrlModel? BadgeUrls
        //{
        //    get
        //    {
        //        return _badgeUrls;
        //    }

        //    set
        //    {
        //        //if(value != null && _cocApi != null)
        //        //{
        //        //    if ((_badgeUrls == null) || (_badgeUrls.Small != value.Small || _badgeUrls.Medium != value.Medium || _badgeUrls.Large != value.Large))
        //        //    {
        //        //        _cocApi.ClanChangedMethod(this);
        //        //    }
        //        //}
        //        if(_badgeUrls != value)
        //        {
        //            _badgeUrls = value;
        //        }
        //    }
        //}

        int _clanLevel;

        public int ClanLevel
        {
            get
            {
                return _clanLevel;
            }

            set
            {
                if (_clanLevel != value)
                {
                    _clanLevel = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        [JsonPropertyName("memberList")]
        public IList<MemberListAPIModel>? Members { get; set; }




        string _type = string.Empty;

        public string Type
        {
            get
            {
                return _type;
            }

            set
            {
                if (_type != value)
                {
                    _type = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        string _description = string.Empty;

        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                if (_description != value)
                {
                    _description = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        public LocationModel? Location { get; set; }



        int _clanPoints;

        public int ClanPoints
        {
            get
            {
                return _clanPoints;
            }

            set
            {
                if (_clanPoints != value)
                {
                    _clanPoints = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        int _clanVersusPoints;

        public int ClanVersusPoints
        {
            get
            {
                return _clanVersusPoints;
            }

            set
            {
                if (_clanVersusPoints != value)
                {
                    _clanVersusPoints = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        int _requiredTrophies;

        public int RequiredTrophies
        {
            get
            {
                return _requiredTrophies;
            }

            set
            {
                if (_requiredTrophies != value)
                {
                    _requiredTrophies = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        int _warWinStreak;

        public int WarWinStreak
        {
            get
            {
                return _warWinStreak;
            }

            set
            {
                if (_warWinStreak != value)
                {
                    _warWinStreak = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        int _warWins;

        public int WarWins
        {
            get
            {
                return _warWins;
            }

            set
            {
                if (_warWins != value)
                {
                    _warWins = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        int _warTies;

        public int WarTies
        {
            get
            {
                return _warTies;
            }

            set
            {
                if (_warTies != value)
                {
                    _warTies = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        int _warLosses;

        public int WarLosses
        {
            get
            {
                return _warLosses;
            }

            set
            {
                if (_warLosses != value)
                {
                    _warLosses = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        bool _isWarLogPublic;

        public bool IsWarLogPublic
        {
            get
            {
                return _isWarLogPublic;
            }

            set
            {
                if (_isWarLogPublic != value)
                {
                    _isWarLogPublic = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }

        [JsonPropertyName("members")]
        public int MemberCount { get; set; }

        WarFrequency _warFrequency;

        public WarFrequency WarFrequency
        {
            get
            {
                return _warFrequency;
            }

            set
            {
                if (_warFrequency != value)
                {
                    _warFrequency = value;

                    if (_cocApi != null)
                    {
                        _changed = true;
                    }
                }
            }
        }



        [JsonIgnore]
        public Dictionary<string, ICurrentWarAPIModel>? Wars { get; set; } = null;




        public DateTime DateTimeUTC { get; internal set; } = DateTime.UtcNow;

        public DateTime Expires { get; internal set; }

        public string EncodedUrl { get; internal set; } = string.Empty;

        public bool IsExpired()
        {
            if (DateTime.UtcNow > Expires)
            {
                return true;
            }
            return false;
        }

        //void IExpires.SetExpiration()
        //{
        //    DateTimeUTC = DateTime.UtcNow;

        //    Expires = DateTime.UtcNow.AddSeconds(15);

        //    //if (this is LeagueWarAPIModel leagueWarAPIModel)
        //    //{
        //    //    Expires = leagueWarAPIModel.StartTimeUTC;

        //    //    if (leagueWarAPIModel.State == Enums.State.InWar)
        //    //    {
        //    //        Expires = DateTime.UtcNow.AddSeconds(15);
        //    //    }
        //    //    else if (leagueWarAPIModel.State == Enums.State.WarEnded)
        //    //    {
        //    //        Expires = DateTime.UtcNow.AddDays(5);
        //    //    }
        //    //}
        //    //else if (this is CurrentWarAPIModel currentWarAPIModel)
        //    //{
        //    //    Expires = currentWarAPIModel.StartTimeUTC;

        //    //    if (currentWarAPIModel.State == Enums.State.InWar)
        //    //    {
        //    //        Expires = DateTime.UtcNow.AddSeconds(15);
        //    //    }
        //    //    else if (currentWarAPIModel.State == Enums.State.WarEnded)
        //    //    {
        //    //        Expires = DateTime.UtcNow.AddDays(5);
        //    //    }
        //    //}

        //    //if (this is LeagueGroupAPIModel)
        //    //{
        //    //    Expires = DateTime.UtcNow.AddMinutes(5);
        //    //}
        //}
    }
}
