﻿using System;
using System.Diagnostics;
using System.Web;
using CocApi.Client;

namespace CocApi
{
    public class HttpRequestException : Exception, IHttpRequestResult
    {
        public DateTime RequestedAt { get; } = DateTime.UtcNow;

        public TimeSpan Elapsed { get; }
        public string PathFormat { get; }
        public string Path { get; }

        //public Exception Exception { get; }

        //public System.Net.HttpStatusCode StatusCode { get; }

        //public string? Reason { get; }

        //public RequestOptions RequestOptions { get; }

        //public Exception Exception { get; }

        public HttpRequestException(string pathFormat, string path, /*RequestOptions requestOptions,*/ TimeSpan elapsed, Exception exception) : base (path, exception)
        {
            //InnerException = Exception;
            Elapsed = elapsed;
            PathFormat = pathFormat;
            //Exception = exception;
            //StatusCode = statusCode;
            //Reason = reason;
            Path = path;

            //Exception = exception;

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
