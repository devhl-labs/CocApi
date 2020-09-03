using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using CocApi.Client;

namespace CocApi
{
    public interface IQueryResult
    {
        Stopwatch Stopwatch { get; }

        string Path { get; }

        RequestOptions RequestOptions { get; }

        string Url();

        string EncodedUrl();
    }

    public class QuerySuccess : IQueryResult
    {
        public Stopwatch Stopwatch { get; }

        public string Path { get; }

        public RequestOptions RequestOptions { get; }

        public HttpStatusCode? HttpStatusCode { get; }

        public QuerySuccess(string path, RequestOptions requestOptions, Stopwatch stopwatch, HttpStatusCode? httpStatusCode = null)
        {
            Stopwatch = stopwatch;

            Path = path;

            HttpStatusCode = httpStatusCode;

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

    public class QueryException : IQueryResult
    {
        public Stopwatch Stopwatch { get; }

        public string Path { get; }

        public RequestOptions RequestOptions { get; }

        public Exception Exception { get; }

        public QueryException(string path, RequestOptions requestOptions, Stopwatch stopwatch, Exception exception)
        {
            Stopwatch = stopwatch;

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
