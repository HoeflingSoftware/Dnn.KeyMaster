using Newtonsoft.Json;

namespace Dnn.KeyMaster.Web.Security.KeyVault.Models
{
    [JsonObject]
    public class AppSettings
    {
        [JsonProperty("KeyVaultUrl")]
        public string KeyVaultUrl { get; set; }

        [JsonProperty("DirectoryId")]
        public string DirectoryId { get; set; }

        [JsonProperty("ClientId")]
        public string ClientId { get; set; }

        [JsonProperty("ClientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("SecretName")]
        public string SecretName { get; set; }
    }
}
