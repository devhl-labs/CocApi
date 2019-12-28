using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace devhl.CocApi.Models.Village
{
    public class LegendLeagueStatisticsApiModel
    {

        public string VillageTag { get; set; } = string.Empty;

        public VillageApiModel? Village { get; set; } 

        public int LegendTrophies { get; set; }


        private LegendLeagueResultApiModel? _bestSeason;

        private LegendLeagueResultApiModel? _previousVersusSeason;

        private LegendLeagueResultApiModel? _bestVersusSeason;

        private LegendLeagueResultApiModel? _currentSeason;

        private LegendLeagueResultApiModel? _previousSeason;

        private LegendLeagueResultApiModel? _currentVersusSeason;





        
        public LegendLeagueResultApiModel? BestSeason
        {
            get
            {
                return _bestSeason;
            }
        
            set
            {
                _bestSeason = value;

                if (_bestSeason != null)
                {
                    if (!LegendLeagueStatistics.Any(l => l.Id == _bestSeason.Id))
                    {
                        LegendLeagueStatistics.Add(_bestSeason);
                    }
                }

            }
        } 

       
        public LegendLeagueResultApiModel? PreviousVersusSeason
        {
            get
            {
                return _previousVersusSeason;
            }
        
            set
            {
                _previousVersusSeason = value;

                if (_previousVersusSeason != null)
                {
                    _previousVersusSeason.Village = Enums.VillageType.BuilderBase;

                    if (!LegendLeagueStatistics.Any(l => l.Id == _previousVersusSeason.Id))
                    {
                        LegendLeagueStatistics.Add(_previousVersusSeason);
                    }
                }
            }
        }

        
        public LegendLeagueResultApiModel? CurrentSeason
        {
            get
            {
                return _currentSeason;
            }
        
            set
            {
                _currentSeason = value;

                if(_currentSeason != null)
                {
                    if (!LegendLeagueStatistics.Any(l => l.Id == _currentSeason.Id))
                    {
                        LegendLeagueStatistics.Add(_currentSeason);
                    }
                }
            }
        }


        public LegendLeagueResultApiModel? CurrentVersusSeason
        {
            get
            {
                return _currentVersusSeason;
            }

            set
            {
                _currentVersusSeason = value;

                if (_currentVersusSeason != null)
                {
                    _currentVersusSeason.Village = Enums.VillageType.BuilderBase;

                    if (!LegendLeagueStatistics.Any(l => l.Id == _currentVersusSeason.Id))
                    {
                        LegendLeagueStatistics.Add(_currentVersusSeason);
                    }
                }
            }
        }


        public LegendLeagueResultApiModel? BestVersusSeason
        {
            get
            {
                return _bestVersusSeason;
            }

            set
            {
                _bestVersusSeason = value;

                if (_bestVersusSeason != null)
                {
                    _bestVersusSeason.Village = Enums.VillageType.BuilderBase;

                    if (!LegendLeagueStatistics.Any(l => l.Id == _bestVersusSeason.Id))
                    {
                        LegendLeagueStatistics.Add(_bestVersusSeason);
                    }
                }
            }
        }


        public LegendLeagueResultApiModel? PreviousSeason
        {
            get
            {
                return _previousSeason;
            }
        
            set
            {
                _previousSeason = value;

                if (_previousSeason != null)
                {
                    if (!LegendLeagueStatistics.Any(l => l.Id == _previousSeason.Id))
                    {
                        LegendLeagueStatistics.Add(_previousSeason);
                    }
                }
            }
        } 







        public IList<LegendLeagueResultApiModel> LegendLeagueStatistics { get; set; } = new List<LegendLeagueResultApiModel>();
    }
}
