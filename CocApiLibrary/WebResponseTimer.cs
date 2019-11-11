using System;
using System.Net;

namespace devhl.CocApi
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
