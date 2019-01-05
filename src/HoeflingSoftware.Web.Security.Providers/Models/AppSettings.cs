namespace HoeflingSoftware.Web.Security.Models
{
    internal class AppSettings
    {
        public string KeyVaultUri { get; set; }
        public string DirectoryId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SecretName { get; set; }

        public class Keys
        {
            public const string KeyVaultUri = "keyVaultUri";
            public const string DirectoryId = "directoryId";
            public const string ClientId = "clientId";
            public const string ClientSecret = "clientSecret";
            public const string SecretName = "secretName";
            public const string ConnectionString = "connectionString";
        }
    }
}
