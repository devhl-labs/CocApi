using Newtonsoft.Json;
using System;
using System.Net;

namespace CocApi.Cache
{
    public class WebResponseTimer
    {
        [JsonProperty]
        public EndPoint EndPoint { get; private set; }

        [JsonProperty]
        public TimeSpan TimeSpan { get; private set; }

        [JsonProperty]
        public DateTime DateTimeCreatedUtc { get; private set; } = DateTime.UtcNow;

        [JsonProperty]
        public HttpStatusCode? HttpStatusCode { get; private set; }

        public WebResponseTimer(EndPoint endPoint, TimeSpan timeSpan, HttpStatusCode? httpStatusCode = null)
        {
            EndPoint = endPoint;
            TimeSpan = timeSpan;
            HttpStatusCode = httpStatusCode;
        }

    }
}
