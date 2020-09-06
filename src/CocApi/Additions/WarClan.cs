using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace CocApi.Model
{
    public partial class WarClan
    {
        public string ClanProfileUrl => Clash.ClanProfileUrl(Tag);


        [DataMember(Name = "result", EmitDefaultValue = false)]
        public Result? Result { get; internal set; }
    }
}
