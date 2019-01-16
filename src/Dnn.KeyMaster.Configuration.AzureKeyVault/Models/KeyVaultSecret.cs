using Newtonsoft.Json;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault.Models
{
    [JsonObject]
    internal class KeyVaultSecret
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
