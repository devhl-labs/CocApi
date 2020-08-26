using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi.Model
{
    public partial class Player
    {
        public static string Url(string villageTag)
        {
            if (Clash.TryFormatTag(villageTag, out string formattedTag) == false)
                throw new InvalidTagException(villageTag);

            return $"players/{Uri.EscapeDataString(formattedTag)}";
        }
    }
}
