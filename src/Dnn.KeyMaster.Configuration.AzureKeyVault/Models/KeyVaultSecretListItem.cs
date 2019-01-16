using Newtonsoft.Json;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault.Models
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
