using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace Dnn.KeyMaster.Web.Security.KeyVault.Utilities
{
    public static class KeyVaultProvider
    {
        private class API
        {
            public const string GetSecret = "{0}/secrets/{1}?api-version=7.0";
        }
        
        public static string GetConnectionString(AppSettings appsettings = null)
        {
            if (appsettings == null)
            {
                // TODO - Discuss if the configuration loading code should be pulled out or not
                appsettings = SecretsProvider.GetSecrets();
            }

            var token = AzureAccessTokenProvider.GetToken(appsettings);
            using (var client = new HttpClient())
            {
                var secretVersions = string.Format(API.GetSecret, appsettings.KeyVaultUrl, appsettings.SecretName);
                client.DefaultRequestHeaders.Add("Authorization", token.ToString());
                var response = client.GetAsync(secretVersions).Result;
                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsStringAsync().Result;
                    var secret = JsonConvert.DeserializeObject<KeyVaultSecretResponse>(json);

                    if (secret != null)
                    {
                        return secret.Value;
                    }
                }
            }

            return string.Empty;
        }
    }
}
