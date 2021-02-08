using System;
using System.Diagnostics;
using CocApi.Client;

namespace CocApi
{
    public interface IHttpRequestResult
    {
        DateTime RequestedAt { get; }

        TimeSpan Elapsed { get; }

        string Path { get; }

        string PathFormat { get; }

        //RequestOptions RequestOptions { get; }

        //string Url();

        //string EncodedUrl();
    }
}
