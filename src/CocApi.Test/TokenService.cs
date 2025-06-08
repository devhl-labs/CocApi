using System.Threading;
using System.Threading.Tasks;
using CocApi.Rest.Apis;
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
    public IDeveloperApi DeveloperApi { get; }
    public IOptions<LoginCredentials> Options { get; }

    public TokenService(
        IDeveloperApi developerApi,
        IOptions<LoginCredentials> options)
    {
        DeveloperApi = developerApi;
        Options = options;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var login = await DeveloperApi.LoginAsync(Options.Value, cancellationToken);

        // Create a testing token
        string ipAddressWithMask = login.Ok()!.IpAddresses()[0];
        string ipAddress = ipAddressWithMask[..ipAddressWithMask.IndexOf('/')];
        var token = new CreateTokenRequest([ipAddress], "test description", "test name");
        var createResponse = await DeveloperApi.CreateAsync(token, cancellationToken);
        var key = createResponse.Ok()!.Key!;

        // delete that testing token
        var deleteResponse = await DeveloperApi.RevokeAsync(key, cancellationToken);

        // query all keys
        var keys = await DeveloperApi.KeysAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
