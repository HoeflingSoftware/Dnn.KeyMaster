using System.ComponentModel.DataAnnotations;

namespace Dnn.KeyMaster.API.Models
{
    public class Secrets
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }

        [Required]
        public string SecretName { get; set; }

        [Required]
        public string DirectoryId { get; set; }

        [Required]
        public string KeyVaultUrl { get; set; }
    }
}
