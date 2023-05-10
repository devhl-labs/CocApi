using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.IBaseApis;
using CocApi.Rest.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

/*
 * This shows how to query, delete, and create new tokens
 * This is useful if your IP address changes frequently.
 * The library does not update your tokens automatically,
 * but that is easy enough to setup.
 * 
 * Simply create a HostedService, inject the TokenContainer<ApiKeyToken>
 * and replace the tokens in the container with your newly created tokens.
 * 
 * ***WARNING***
 * Be aware that if the Clash API server is down, calls to it in HostedService#StartAsync will crash your program
 * If you do not want the program to crash on startup due to the Clash API server being down,
 * use a background service instead.
 */

namespace CocApi.Test;

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
        var login = await DeveloperApi.LoginAsync(Options.Value);
        var rawValue = login.Headers.GetValues("set-cookie").Single();
        var encodedValue = System.Web.HttpUtility.UrlEncode(rawValue, Encoding.UTF8);
        var domain = new System.Uri("https://developer.clashofclans.com/api");
        var cookie = new System.Net.Cookie("session", encodedValue, null, domain.Host);
        CookieContainer.Value.Add(domain, cookie);

        // Create a testing token
        var token = new CreateTokenRequest(new List<string> { login.ToModel()!.IpAddress() }, "test description", "test name");
        var createResponse = await DeveloperApi.CreateAsync(token);

        // delete that testing token
        var deleteResponse = await DeveloperApi.RevokeAsync(createResponse.ToModel()!.Key!);

        // query all keys
        var keys = await DeveloperApi.KeysAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
