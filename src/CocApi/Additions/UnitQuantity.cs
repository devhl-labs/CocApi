using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CocApi.Additions
{
    public class UnitQuantity
    {
        public int Quantity { get; }

        public Unit? Unit { get; }

        internal UnitQuantity(int quantity, Unit? unit)
        {
            Quantity = quantity;
            Unit = unit;
        }
    }
}
