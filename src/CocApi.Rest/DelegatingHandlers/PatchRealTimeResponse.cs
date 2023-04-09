using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CocApi.Rest.DelegatingHandlers
{
    public class PatchRealTimeResponse : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine("here!");

            string[] realTimeEligible = new string[] { "currentwar", "warTag" };

            if (realTimeEligible.Any(word => request.RequestUri?.ToString().Contains(word) == true))
            {
                HttpResponseMessage result = await base.SendAsync(request, cancellationToken);

                result.Headers.CacheControl ??= new System.Net.Http.Headers.CacheControlHeaderValue();

                result.Headers.CacheControl.MaxAge = TimeSpan.Zero;

                return result;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
