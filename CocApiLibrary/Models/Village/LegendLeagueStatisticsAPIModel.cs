using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace CocApiLibrary
{
    public class LegendLeagueStatisticsAPIModel
    {
        [Key]
        public string VillageTag { get; set; } = string.Empty;

        public virtual VillageAPIModel? Village { get; set; } 

        public int LegendTrophies { get; set; }


        private LegendLeagueResultAPIModel? _bestSeason;

        private LegendLeagueResultAPIModel? _previousVersusSeason;

        private LegendLeagueResultAPIModel? _bestVersusSeason;

        private LegendLeagueResultAPIModel? _currentSeason;

        private LegendLeagueResultAPIModel? _previousSeason;

        private LegendLeagueResultAPIModel? _currentVersusSeason;





        [NotMapped]        
        public LegendLeagueResultAPIModel? BestSeason
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

        [NotMapped]       
        public LegendLeagueResultAPIModel? PreviousVersusSeason
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

        [NotMapped]        
        public LegendLeagueResultAPIModel? CurrentSeason
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

        [NotMapped]
        public LegendLeagueResultAPIModel? CurrentVersusSeason
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

        [NotMapped]
        public LegendLeagueResultAPIModel? BestVersusSeason
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

        [NotMapped]
        public LegendLeagueResultAPIModel? PreviousSeason
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






        [ForeignKey(nameof(VillageTag))]
        public virtual IList<LegendLeagueResultAPIModel> LegendLeagueStatistics { get; set; } = new List<LegendLeagueResultAPIModel>();
    }
}
