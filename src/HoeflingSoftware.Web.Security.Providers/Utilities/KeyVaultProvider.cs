﻿using HoeflingSoftware.Web.Security.Models;
using Newtonsoft.Json;
using System.Net.Http;

namespace HoeflingSoftware.Web.Security.Utilities
{
    internal static class KeyVaultProvider
    {
        private class API
        {
            public const string GetSecret = "{0}/secrets/{1}?api-version=7.0";
        }

        public static string GetSecret(AppSettings appsettings)
        {
            var token = AzureAccessTokenProvider.GetToken(appsettings);
            using (var client = new HttpClient())
            {
                var secretVersions = string.Format(API.GetSecret, appsettings.KeyVaultUri, appsettings.SecretName);
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
