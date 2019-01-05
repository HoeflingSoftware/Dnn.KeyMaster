using HoeflingSoftware.Web.Security.Models;
using System.Net.Http;

namespace HoeflingSoftware.Web.Security.Utilities
{
    internal static class KeyVaultProvider
    {
        private class API
        {
            public const string GetSecretVersions = "{0}/secrets/{1}/versions?api-version=7.0";
            public const string GetSecret = "{0}/secrets/{1}";
        }

        public static string GetSecret(AppSettings appsettings)
        {
            var token = AzureAccessTokenProvider.GetToken(appsettings);
            using (var client = new HttpClient())
            {
                var secretVersions = string.Format(API.GetSecretVersions, appsettings.KeyVaultUri, appsettings.SecretName);
                client.GetAsync(secretVersions);
                return string.Empty;
            }
        }
    }
}
