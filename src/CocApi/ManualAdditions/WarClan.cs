using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CocApi.Model
{
    public partial class WarClan
    {
        private List<ClanWarAttack> _allAttacks;

        public List<ClanWarAttack> AllAttacks
        {
            get
            {
                if (_allAttacks == null)
                {
                    _allAttacks = new List<ClanWarAttack>();
                    foreach (var member in Members)
                        foreach (var attack in member.Attacks.EmptyIfNull())
                            _allAttacks.Add(attack);
                }

                return _allAttacks;
            }
        }
    }
}
