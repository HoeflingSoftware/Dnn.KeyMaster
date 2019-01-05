using Newtonsoft.Json;

namespace HoeflingSoftware.Web.Security.Models
{
    [JsonObject]
    internal class AppSettings
    {
        [JsonProperty("KeyVaultUri")]
        public string KeyVaultUri { get; set; }

        [JsonProperty("DirectoryId")]
        public string DirectoryId { get; set; }

        [JsonProperty("ClientId")]
        public string ClientId { get; set; }

        [JsonProperty("ClientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("SecretName")]
        public string SecretName { get; set; }

        internal class Keys
        {
            public const string KeyVaultUri = "KeyVaultUri";
            public const string DirectoryId = "DirectoryId";
            public const string ClientId = "ClientId";
            public const string ClientSecret = "ClientSecret";
            public const string SecretName = "SecretName";
            public const string ConnectionString = "connectionString";
        }
    }
}
