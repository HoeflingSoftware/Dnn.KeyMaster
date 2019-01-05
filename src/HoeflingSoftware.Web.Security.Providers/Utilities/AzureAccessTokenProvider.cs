using HoeflingSoftware.Web.Security.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace HoeflingSoftware.Web.Security.Utilities
{
    internal static class AzureAccessTokenProvider
    {
        private class APIs
        {
            public const string AccessToken = "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";
        }

        public static string GetToken(AppSettings appsettings)
        {
            using (var client = new HttpClient())
            {
                var endpoint = string.Format(APIs.AccessToken, appsettings.DirectoryId);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", appsettings.ClientId),
                    new KeyValuePair<string, string>("client_secret", appsettings.ClientSecret),
                    new KeyValuePair<string, string>("scope", "https://vault.azure.net/.default")
                });

                var result = client.PostAsync(endpoint, content).Result;
                if (result.IsSuccessStatusCode)
                {
                    var json = result.Content.ReadAsStringAsync().Result;
                    var model = JsonConvert.DeserializeObject<AccessTokenResponse>(json);
                    return model.Token;
                }

                return string.Empty;
            }
        }
    }
}
