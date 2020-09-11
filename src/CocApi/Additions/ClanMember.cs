using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Model
{
    public partial class ClanMember
    {
        public string PlayerProfileUrl => Clash.PlayerProfileUrl(Tag);
    }
}
