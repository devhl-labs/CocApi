using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IApis;
using CocApi.Rest.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CocApi.Test
{
    public class TokenService : IHostedService
    {
        public Rest.Client.CookieContainer CookieContainer { get; }
        public IDeveloperApi DeveloperApi { get; }
        public IOptions<LoginCredentials> Options { get; }

        public TokenService(
            Rest.Client.CookieContainer cookieContainer,
            IDeveloperApi developerApi,
            IOptions<LoginCredentials> options)
        {
            CookieContainer = cookieContainer;
            DeveloperApi = developerApi;
            Options = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // login and save the cookie to the CookieContainer
            var login = await DeveloperApi.LoginResponseAsync(Options.Value);
            var rawValue = login.Headers.GetValues("set-cookie").Single();
            var encodedValue = System.Web.HttpUtility.UrlEncode(rawValue, Encoding.UTF8);
            var domain = new System.Uri("https://developer.clashofclans.com/api");
            var cookie = new System.Net.Cookie("session", encodedValue, null, domain.Host);
            CookieContainer.Value.Add(domain, cookie);

            // Create a testing token
            var token = new CreateTokenRequest(new List<string> { login.Content!.IpAddress() }, "test description", "test name");
            var createResponse = await DeveloperApi.CreateResponseAsync(token);

            // delete that testing token
            var deleteResponse = await DeveloperApi.RevokeResponseAsync(createResponse.Content!.Key!);

            // query all keys
            var keys = await DeveloperApi.KeysResponseAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
