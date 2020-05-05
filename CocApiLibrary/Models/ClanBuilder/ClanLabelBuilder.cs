namespace devhl.CocApi.Models.Clan
{
    public class ClanLabelBuilder
    {
        public string ClanTag { get; set; } = string.Empty;

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        //public LabelUrlBuilder? LabelUrl { get; set; }

        public override string ToString() => Name;

        internal ClanLabel Build()
        {
            ClanLabel clanLabel = new ClanLabel
            {
                ClanTag = ClanTag,
                Id = Id,
                Name = Name
            };

            return clanLabel;
        }
    }
}
