namespace CocApi.Cache
{
    public class ClanServiceOptions : ServiceOptions
    {
        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadClan { get; set; } = true;

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadGroup { get; set; } = true;

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadCurrentWar { get; set; } = true;

        /// <summary>
        /// Default value is true.
        /// </summary>
        public bool DownloadWarLog { get; set; } = true;
    }
}
