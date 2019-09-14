using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CocApiLibrary
{
    public class WebResponseTimer
    {
        public readonly EndPoint EndPoint;
        public readonly TimeSpan TimeSpan;
        public readonly DateTime DateTimeCreatedUTC = DateTime.UtcNow;
        public readonly HttpStatusCode? HttpStatusCode;

        public WebResponseTimer(EndPoint endPoint, TimeSpan timeSpan, HttpStatusCode? httpStatusCode = null)
        {
            EndPoint = endPoint;
            TimeSpan = timeSpan;
            HttpStatusCode = httpStatusCode;
        }

    }
}
