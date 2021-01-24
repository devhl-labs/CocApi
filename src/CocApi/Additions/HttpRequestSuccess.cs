using System;
using System.Diagnostics;
using System.Net;
using System.Web;
using CocApi.Client;

namespace CocApi
{

    public class HttpRequestSuccess : IHttpRequestResult
    {
        public DateTime RequestedAt { get; } = DateTime.UtcNow;

        public TimeSpan Elapsed { get; }

        public string Path { get; }

        //public RequestOptions RequestOptions { get; }

        public HttpStatusCode? HttpStatusCode { get; }

        public HttpRequestSuccess(string path, TimeSpan elapsed, HttpStatusCode? httpStatusCode = null)
        {
            Elapsed = elapsed;

            Path = path;

            HttpStatusCode = httpStatusCode;

            //RequestOptions = requestOptions;
        }

        //public string Url()
        //{
        //    string result = Path;

        //    foreach (var kvp in RequestOptions.PathParameters)
        //        result = result.Replace("{" + kvp.Key + "}", kvp.Value);

        //    return result;
        //}

        //public string EncodedUrl()
        //{
        //    string result = Path;

        //    foreach (var kvp in RequestOptions.PathParameters)
        //        result = result.Replace("{" + kvp.Key + "}", HttpUtility.UrlEncode(kvp.Value));

        //    return result;
        //}
    }
}
