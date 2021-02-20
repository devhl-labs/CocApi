using System;

namespace CocApi
{
    public static class Library
    {
        public static readonly Version Version = typeof(Library).Assembly.GetName().Version;

        public const string REPOSITORY_URL = "https://github.com/devhl-labs/CocApi";

        public static event HttpRequestResultEventHandler? HttpRequestResult;

        public delegate System.Threading.Tasks.Task HttpRequestResultEventHandler(object sender, HttpRequestResultEventArgs log);

        internal static void OnHttpRequestResult(object sender, HttpRequestResultEventArgs log)
        {
            HttpRequestResult?.Invoke(sender, log);
        }
    }
}
