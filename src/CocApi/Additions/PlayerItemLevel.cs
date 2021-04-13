using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CocApi.Model
{
    public partial class PlayerItemLevel
    {
        /// <summary>
        /// Gets or Sets SuperTroopIsActive
        /// </summary>
        [DataMember(Name = "superTroopIsActive", EmitDefaultValue = false)]
        public bool? SuperTroopIsActive { get; private set; }
    }
}
