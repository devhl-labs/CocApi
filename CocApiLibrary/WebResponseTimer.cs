using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CocApiLibrary
{
    public class WebResponseTimer
    {
        public EndPoint EndPoint { get; }
        public TimeSpan TimeSpan { get; }
        public DateTime DateTimeCreatedUtc { get; } = DateTime.UtcNow;
        public HttpStatusCode? HttpStatusCode { get; }

        public WebResponseTimer(EndPoint endPoint, TimeSpan timeSpan, HttpStatusCode? httpStatusCode = null)
        {
            EndPoint = endPoint;
            TimeSpan = timeSpan;
            HttpStatusCode = httpStatusCode;
        }

    }
}
