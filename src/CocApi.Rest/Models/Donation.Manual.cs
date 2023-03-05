namespace CocApi.Rest.Models
{
    public sealed class Donation
    {
        public ClanMember ClanMember { get; }
        public int OldValue { get; }
        public int NewValue { get; }
        public Donation(ClanMember clanMember, int oldValue, int newValue)
        {
            ClanMember = clanMember;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public int Quanity
        {
            get
            {
                return NewValue - OldValue;
            }
        }
    }
}
