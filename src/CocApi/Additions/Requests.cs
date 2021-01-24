using static CocApi.Api.ClansApi;

namespace CocApi
{
    public static class Requests
    {
        public static event HttpRequestResultEventHandler? HttpRequestResult;

        internal static void OnHttpRequestResult(object sender, HttpRequestResultEventArgs log)
        {
            HttpRequestResult?.Invoke(sender, log);
        }
    }
}
