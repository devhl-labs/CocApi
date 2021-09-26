using System;
using System.Collections.Generic;
using System.Text;

namespace CocApi
{
    public class HttpRequestResultEventArgs : EventArgs
    {
        public IHttpRequestResult HttpRequestResult { get; }

        public HttpRequestResultEventArgs(IHttpRequestResult httpRequestResult)
        {
            HttpRequestResult = httpRequestResult;
        }
    }
}