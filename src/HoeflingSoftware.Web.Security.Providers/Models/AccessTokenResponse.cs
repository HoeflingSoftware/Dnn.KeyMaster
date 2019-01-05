using Newtonsoft.Json;

namespace HoeflingSoftware.Web.Security.Models
{
    [JsonObject]
    internal class AccessTokenResponse
    {
        [JsonProperty("token_type")]
        public string Type { get; set; }

        [JsonProperty("access_token")]
        public string Token { get; set; }
    }
}
