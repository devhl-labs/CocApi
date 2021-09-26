using System;

namespace CocApi
{
    public class HttpRequestException : Exception, IHttpRequestResult
    {
        public DateTime RequestedAt { get; } = DateTime.UtcNow;
        public TimeSpan Elapsed { get; }
        public string PathFormat { get; }
        public string Path { get; }


        public HttpRequestException(string pathFormat, string path, TimeSpan elapsed, Exception exception) : base(path, exception)
        {
            Elapsed = elapsed;
            PathFormat = pathFormat;
            Path = path;
        }
    }
}
