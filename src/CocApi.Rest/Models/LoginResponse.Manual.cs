using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class LoginResponse
    {
        internal class IpAddressLimit
        {
            [JsonPropertyName("cidrs")]
            public string[] Cidrs { get; set; } = Array.Empty<string>();

            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
        }

        public string[] IpAddresses()
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtSecurityToken = handler.ReadJwtToken(TemporaryAPIToken);
            System.Security.Claims.Claim limits = jwtSecurityToken.Claims.First(c => c.Value.Contains("cidrs"));
            IpAddressLimit? ipAddressLimit = System.Text.Json.JsonSerializer.Deserialize<IpAddressLimit>(limits.Value);
            return ipAddressLimit == null ? throw new Exception("Could not deserialize the response.") : ipAddressLimit.Cidrs;
        }
    }
}
