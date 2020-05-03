using devhl.CocApi.Models.Clan;
using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.CocApi
{
    public class ClanDonationEventArgs : EventArgs
    {
#nullable disable

        public Clan Fetched { get; }

        public IReadOnlyList<Donation> Received { get; }
        
        public IReadOnlyList<Donation> Gave { get; }

        public ClanDonationEventArgs(Clan fetched, IReadOnlyList<Donation> received, IReadOnlyList<Donation> gave)
        {
            Fetched = fetched;

            Received = received;

            Gave = gave;
        }
    }
}
