using Dnn.KeyMaster.Configuration.AzureKeyVault.Exceptions;
using Dnn.KeyMaster.Configuration.AzureKeyVault.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault
{
    //https://medium.com/@anoopt/accessing-azure-key-vault-secret-through-azure-key-vault-rest-api-using-an-azure-ad-app-4d837fed747
    internal static class AccessTokenProvider
    {
        private class APIs
        {
            public const string AccessToken = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        }

        public static AccessToken GetToken()
        {
            using (var client = new HttpClient())
            {
                var endpoint = string.Format(
                    APIs.AccessToken, 

                    ConfigurationProvider.Instance.Configuration.Secrets[Keys.DirectoryId]);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", ConfigurationProvider.Instance.Configuration.Secrets[Keys.ClientId]),
                    new KeyValuePair<string, string>("client_secret", ConfigurationProvider.Instance.Configuration.Secrets[Keys.ClientSecret]),
                    new KeyValuePair<string, string>("scope", "https://vault.azure.net/.default")
                });

                var result = client.PostAsync(endpoint, content).Result;
                if (result.IsSuccessStatusCode)
                {
                    var json = result.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<AccessToken>(json);
                    return model;
                }

                var errorJson = result.Content.ReadAsStringAsync().Result;
                var error = JsonConvert.DeserializeObject<TokenError>(errorJson);
                throw new AzureKeyMasterException(error);
            }
        }
    }
}
