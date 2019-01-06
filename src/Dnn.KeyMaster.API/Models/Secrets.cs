using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Dnn.KeyMaster.API.Models
{
    [JsonObject]
    public class Secrets
    {
        [Required]
        [JsonProperty]
        public string ClientId { get; set; }

        [Required]
        [JsonProperty]
        public string ClientSecret { get; set; }

        [Required]
        [JsonProperty]
        public string SecretName { get; set; }

        [Required]
        [JsonProperty]
        public string DirectoryId { get; set; }

        [Required]
        [JsonProperty]
        public string KeyVaultUrl { get; set; }
    }
}
