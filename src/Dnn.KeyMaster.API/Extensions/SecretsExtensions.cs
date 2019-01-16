using Dnn.KeyMaster.API.Models;
using System.Collections.Specialized;

namespace Dnn.KeyMaster.API.Extensions
{
    public static class SecretsExtensions
    {
        public static NameValueCollection ToNameValueCollection(this Secrets secrets)
        {
            return new NameValueCollection
            {
                ["KeyVaultUrl"] = secrets.KeyVaultUrl,
                ["ClientId"] = secrets.ClientId,
                ["ClientSecret"] = secrets.ClientSecret,
                ["DirectoryId"] = secrets.DirectoryId,
                ["SecretName"] = secrets.SecretName
            };
        }
    }
}
