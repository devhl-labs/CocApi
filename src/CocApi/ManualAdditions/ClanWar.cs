using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanWar
    {
        public static string Url(string clanTag)
        {
            if (Clash.TryGetValidTag(clanTag, out string formattedTag) == false)
                throw new InvalidTagException(clanTag);

            return $"clans/{Uri.EscapeDataString(formattedTag)}/currentwar";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PreparationStartTime, Clans.Values.First().Tag, Clans.Values.Skip(1).First().Tag);
        }

        private SortedDictionary<string, WarClan> _clans;
        
        public SortedDictionary<string, WarClan> Clans
        {
            get
            {
                if (_clans != null)
                    return _clans;

                if (Clan == null || Clan.Tag == null || Opponent == null || Opponent.Tag == null)
                    return null;

                _clans = new SortedDictionary<string, WarClan>
                {
                    { Clan.Tag, Clan },

                    { Opponent.Tag, Opponent }
                };

                return _clans;
            }
        }

        public bool Equals(ClanWar input)
        {
            if (input == null)
                return false;

            return
                //(
                //    this.Clan == input.Clan ||
                //    (this.Clan != null &&
                //    this.Clan.Equals(input.Clan))
                //) &&
                (
                    this.TeamSize == input.TeamSize ||
                    this.TeamSize.Equals(input.TeamSize)
                ) &&
                //(
                //    this.Opponent == input.Opponent ||
                //    (this.Opponent != null &&
                //    this.Opponent.Equals(input.Opponent))
                //) &&
                (
                    this.StartTime == input.StartTime ||
                    (this.StartTime != null &&
                    this.StartTime.Equals(input.StartTime))
                ) &&
                (
                    this.State == input.State ||
                    this.State.Equals(input.State)
                ) &&
                (
                    this.EndTime == input.EndTime ||
                    (this.EndTime != null &&
                    this.EndTime.Equals(input.EndTime))
                ) &&
                (
                    this.PreparationStartTime == input.PreparationStartTime ||
                    (this.PreparationStartTime != null &&
                    this.PreparationStartTime.Equals(input.PreparationStartTime))
                ) &&
                (
                    this.Clans.First().Value.Attacks == input.Clans.First().Value.Attacks
                ) &&
                (
                    this.Clans.Skip(1).First().Value.Attacks == input.Clans.Skip(1).First().Value.Attacks
                )              
                ;
        }
    }
}
