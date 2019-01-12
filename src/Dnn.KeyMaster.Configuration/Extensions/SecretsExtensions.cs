using Dnn.KeyMaster.Configuration;

namespace Dnn.KeyMaster.Configuration.Extensions
{
    public static class SecretsExtensions
    {
        public static bool IsValid(this Secrets secrets)
        {
            return 
                !string.IsNullOrEmpty(secrets.KeyVaultUrl) &&
                !string.IsNullOrEmpty(secrets.DirectoryId) &&
                !string.IsNullOrEmpty(secrets.ClientId) &&
                !string.IsNullOrEmpty(secrets.ClientSecret) &&
                !string.IsNullOrEmpty(secrets.SecretName);
        }
    }
}