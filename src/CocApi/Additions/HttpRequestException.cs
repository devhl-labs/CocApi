using System;
using System.Diagnostics;
using System.Web;
using CocApi.Client;

namespace CocApi
{
    public class HttpRequestException : IHttpRequestResult
    {
        public DateTime RequestedAt { get; } = DateTime.UtcNow;

        public TimeSpan Elapsed { get; }

        public string Path { get; }

        public RequestOptions RequestOptions { get; }

        public Exception Exception { get; }

        public HttpRequestException(string path, RequestOptions requestOptions, Stopwatch stopwatch, Exception exception)
        {
            Elapsed = stopwatch.Elapsed;

            Path = path;

            Exception = exception;

            RequestOptions = requestOptions;
        }

        public string Url()
        {
            string result = Path;

            foreach (var kvp in RequestOptions.PathParameters)
                result = result.Replace("{" + kvp.Key + "}", kvp.Value);

            return result;
        }

        public string EncodedUrl()
        {
            string result = Path;

            foreach (var kvp in RequestOptions.PathParameters)
                result = result.Replace("{" + kvp.Key + "}", HttpUtility.UrlEncode(kvp.Value));

            return result;
        }
    }
}
