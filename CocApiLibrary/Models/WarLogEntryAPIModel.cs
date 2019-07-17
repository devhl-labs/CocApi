using System;
using System.Text.Json.Serialization;

namespace CocApiStandardLibrary.Models
{
    public class WarLogEntryModel
    {

        private string _endTime = string.Empty;

        public string Result { get; set; } = string.Empty;


        public int TeamSize { get; set; }

        public WarClanAPIModel? Clan { get; set; }

        public WarClanAPIModel? Opponent { get; set; }

        public DateTime EndTimeUTC { get; set; }

        public string EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;

                EndTimeUTC = _endTime.ToDateTime();
            }
        }


        internal void Process()
        {
            Clan?.Process();
            Opponent?.Process();
        }
    }
}
