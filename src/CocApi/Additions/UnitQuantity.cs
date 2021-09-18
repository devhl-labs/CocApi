namespace CocApi.Additions
{
    public sealed class UnitQuantity
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
