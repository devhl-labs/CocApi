# Created with Openapi Generator

<a id="cli"></a>
## Run the following powershell command to generate the library

```ps1
$properties = @(
    'apiName=CocApi',
    'targetFramework=net7.0',
    'validatable=false',
    'nullableReferenceTypes=true',
    'hideGenerationTimestamp=false',
    'packageVersion=2.6.1',
    'packageAuthors=devhl',
    'packageCompany=devhl',
    'packageCopyright=No Copyright',
    'packageDescription=A wrapper for the Clash of Clans API',
    'packageName=CocApi.Rest',
    'packageTags=ClashOfClans SuperCell devhl',
    'packageTitle=CocApi.Rest'
) -join ","

$global = @(
    'apiDocs=true',
    'modelDocs=true',
    'apiTests=true',
    'modelTests=true'
) -join ","

java -jar "<path>/openapi-generator/modules/openapi-generator-cli/target/openapi-generator-cli.jar" generate `
    -g csharp-netcore `
    -i <your-swagger-file>.yaml `
    -o <your-output-folder> `
    --library generichost `
    --additional-properties $properties `
    --global-property $global `
    --git-host "github.com" `
    --git-repo-id "CocApi" `
    --git-user-id "devhl-labs" `
    --release-note "Moved rest methods to CocApi.Rest. Now using automation to generate rest methods from openapi yaml."
    # -t templates
```

<a id="usage"></a>
## Using the library in your project

```cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CocApi.Rest.Api;
using CocApi.Rest.Client;
using CocApi.Rest.Model;

namespace YourProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            var api = host.Services.GetRequiredService<IClansApi>();
            ApiResponse<ClanCapitalRaidSeasons> response = await api.GetCapitalRaidSeasonsAsync("todo");
            ClanCapitalRaidSeasons model = response.AsModel();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
          .ConfigureCocApi((context, options) =>
          {
              // the type of token here depends on the api security specifications
              ApiKeyToken token = new("<your token>");
              options.AddTokens(token);

              // optionally choose the method the tokens will be provided with, default is RateLimitProvider
              options.UseProvider<RateLimitProvider<ApiKeyToken>, ApiKeyToken>();

              // the type of token here depends on the api security specifications
              ApiKeyToken token = new("<your token>");
              options.AddTokens(token);

              // optionally choose the method the tokens will be provided with, default is RateLimitProvider
              options.UseProvider<RateLimitProvider<ApiKeyToken>, ApiKeyToken>();

              options.ConfigureJsonOptions((jsonOptions) =>
              {
                  // your custom converters if any
              });

              options.AddCocApiHttpClients(builder: builder => builder
                .AddRetryPolicy(2)
                .AddTimeoutPolicy(TimeSpan.FromSeconds(5))
                .AddCircuitBreakerPolicy(10, TimeSpan.FromSeconds(30))
                // add whatever middleware you prefer
              );
          });
    }
}
```
<a id="questions"></a>
## Questions

- What about HttpRequest failures and retries?
  If supportsRetry is enabled, you can configure Polly in the ConfigureClients method.
- How are tokens used?
  Tokens are provided by a TokenProvider class. The default is RateLimitProvider which will perform client side rate limiting.
  Other providers can be used with the UseProvider method.
- Does an HttpRequest throw an error when the server response is not Ok?
  It depends how you made the request. If the return type is ApiResponse<T> no error will be thrown, though the Content property will be null. 
  StatusCode and ReasonPhrase will contain information about the error.
  If the return type is T, then it will throw. If the return type is TOrDefault, it will return null.
- How do I validate requests and process responses?
  Use the provided On and After methods in the Api class from the namespace CocApi.Rest.Rest.DefaultApi.
  Or provide your own class by using the generic ConfigureCocApi method.

<a id="dependencies"></a>
## Dependencies

- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting/) - 5.0.0 or later
- [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http/) - 5.0.0 or later
- [Microsoft.Extensions.Http.Polly](https://www.nuget.org/packages/Microsoft.Extensions.Http.Polly/) - 5.0.1 or later

<a id="documentation-for-authorization"></a>
## Documentation for Authorization


Authentication schemes defined for the API:
<a id="JWT"></a>
### JWT

- **Type**: API key
- **API key parameter name**: authorization
- **Location**: HTTP header

<a id="cookieAuth"></a>
### cookieAuth

- **Type**: API key
- **API key parameter name**: session
- **Location**: 


## Build
- SDK version: 2.6.1
- Build date: 2023-07-09T16:02:31.795127800-04:00[America/New_York]
- Build package: org.openapitools.codegen.languages.CSharpClientCodegen

## Api Information
- appName: Clash of Clans API
- appVersion: v1
- appDescription: Check out &lt;a href&#x3D;\&quot;https://developer.clashofclans.com/#/getting-started\&quot; target&#x3D;\&quot;_parent\&quot;&gt;Getting Started&lt;/a&gt; for instructions and links to other resources. Clash of Clans API uses &lt;a href&#x3D;\&quot;https://jwt.io/\&quot; target&#x3D;\&quot;_blank\&quot;&gt;JSON Web Tokens&lt;/a&gt; for authorizing the requests. Tokens are created by developers on &lt;a href&#x3D;\&quot;https://developer.clashofclans.com/#/account\&quot; target&#x3D;\&quot;_parent\&quot;&gt;My Account&lt;/a&gt; page and must be passed in every API request in Authorization HTTP header using Bearer authentication scheme. Correct Authorization header looks like this: \&quot;Authorization: Bearer API_TOKEN\&quot;. 

## [OpenApi Global properties](https://openapi-generator.tech/docs/globals)
- generateAliasAsModel: 
- supportingFiles: 
- models: omitted for brevity
- apis: omitted for brevity
- apiDocs: true
- modelDocs: true
- apiTests: true
- modelTests: true
- withXml: 

## [OpenApi Generator Parameters](https://openapi-generator.tech/docs/generators/csharp-netcore)
- allowUnicodeIdentifiers: 
- apiName: CocApi
- caseInsensitiveResponseHeaders: 
- conditionalSerialization: false
- disallowAdditionalPropertiesIfNotPresent: 
- gitHost: github.com
- gitRepoId: CocApi
- gitUserId: devhl-labs
- hideGenerationTimestamp: false
- interfacePrefix: I
- library: generichost
- licenseId: 
- modelPropertyNaming: 
- netCoreProjectFile: false
- nonPublicApi: false
- nullableReferenceTypes: true
- optionalAssemblyInfo: 
- optionalEmitDefaultValues: false
- optionalMethodArgument: true
- optionalProjectFile: 
- packageAuthors: devhl
- packageCompany: devhl
- packageCopyright: No Copyright
- packageDescription: A wrapper for the Clash of Clans API
- packageGuid: 71B5E000-88E9-432B-BAEB-BB622EA7DC33
- packageName: CocApi.Rest
- packageTags: ClashOfClans SuperCell devhl
- packageTitle: CocApi.Rest
- packageVersion: 2.6.1
- releaseNote: Moved rest methods to CocApi.Rest. Now using automation to generate rest methods from openapi yaml.
- returnICollection: false
- sortParamsByRequiredFlag: 
- sourceFolder: src
- targetFramework: net7.0
- useCollection: false
- useDateTimeOffset: false
- useOneOfDiscriminatorLookup: false
- validatable: false

This C# SDK is automatically generated by the [OpenAPI Generator](https://openapi-generator.tech) project.
