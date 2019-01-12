using Newtonsoft.Json;

namespace Dnn.KeyMaster.Configuration
{
    [JsonObject]
    public class Secrets
    {
        public Secrets() { }
        public Secrets(string keyVaultUrl, string directoryId, string clientId, string clientSecret, string secretName)
        {
            KeyVaultUrl = keyVaultUrl;
            DirectoryId = directoryId;
            ClientId = clientId;
            ClientSecret = clientSecret;
            SecretName = secretName;
        }

        [JsonProperty(Keys.KeyVaultUrl)]
        public string KeyVaultUrl { get; internal set; }

        [JsonProperty(Keys.DirectoryId)]
        public string DirectoryId { get; internal set; }

        [JsonProperty(Keys.ClientId)]
        public string ClientId { get; internal set; }

        [JsonProperty(Keys.ClientSecret)]
        public string ClientSecret { get; internal set; }

        [JsonProperty(Keys.SecretName)]
        public string SecretName { get; internal set; }

        internal class Keys
        {
            public const string KeyVaultUrl = "KeyVaultUrl";
            public const string DirectoryId = "DirectoryId";
            public const string ClientId = "ClientId";
            public const string ClientSecret = "ClientSecret";
            public const string SecretName = "SecretName";
        }
    }
}