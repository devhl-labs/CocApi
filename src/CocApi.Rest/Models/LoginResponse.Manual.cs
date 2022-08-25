using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CocApi.Rest.Models
{
    public partial class LoginResponse
    {
        public string IpAddress()
        {
            string part = TemporaryAPIToken.Split(".")[1];
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(part));
            var o = System.Text.Json.JsonSerializer.Deserialize<TokenDetails>(decoded)!;
            var result = o.Limits[1].Cidrs[0].Split("/")[0];
            return result;
        }
    }

    internal class TokenLimits
    {
        [JsonPropertyName("cidrs")]
        public List<string> Cidrs { get; set; } = new();
    }

    internal class TokenDetails
    {
        [JsonPropertyName("limits")]
        public List<TokenLimits> Limits { get; set; } = new();
    }
}
