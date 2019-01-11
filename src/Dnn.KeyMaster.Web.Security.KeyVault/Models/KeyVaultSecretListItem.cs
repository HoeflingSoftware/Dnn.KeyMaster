using Newtonsoft.Json;

namespace Dnn.KeyMaster.Web.Security.KeyVault.Models
{
    [JsonObject]
    internal class KeyVaultSecretListItem
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
