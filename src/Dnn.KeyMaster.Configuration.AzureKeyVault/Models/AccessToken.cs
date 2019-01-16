using Newtonsoft.Json;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault.Models
{
    [JsonObject]
    internal class AccessToken
    {
        [JsonProperty("token_type")]
        public string Type { get; set; }

        [JsonProperty("access_token")]
        public string Token { get; set; }

        public override string ToString()
        {
            return $"{Type} {Token}";
        }
    }
}
