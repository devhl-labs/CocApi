using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CocApi.Cache.DelegatingHandlers
{
    public class PatchRealTimeResponse : DelegatingHandler
    {
        private readonly ILogger<PatchRealTimeResponse> _logger;

        public PatchRealTimeResponse(ILogger<PatchRealTimeResponse> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool realTime = request.RequestUri?.Query.Contains("realtime=true", StringComparison.OrdinalIgnoreCase) == true;

            if (realTime)
            {
                HttpResponseMessage result = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                result.Headers.CacheControl ??= new System.Net.Http.Headers.CacheControlHeaderValue();

                result.Headers.CacheControl.MaxAge = TimeSpan.Zero;

                return result;
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
