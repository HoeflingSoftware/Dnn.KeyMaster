﻿using Dnn.KeyMaster.Exceptions;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using DotNetNuke.Instrumentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dnn.KeyMaster.Web.Security.KeyVault.Utilities
{
    public static class KeyVaultProvider
    {
        private class API
        {
            public const string ApiVersion = "api-version=7.0";
            public const string Secret = "{0}/secrets/{1}?" + ApiVersion;
            public const string GetAllSecrets = "{0}/secrets?" + ApiVersion;
        }

        private static NameValueCollection _appsettings;
        public static NameValueCollection AppSettings
        {
            get
            {
                if (_appsettings == null)
                {
                    _appsettings = GetAppSettings();
                }

                return _appsettings;
            }
        }
        
        private static KeyValuePair<string, string> GetSecret(KeyVaultSecretListItem item, AccessTokenResponse token = null)
        {
            if (token == null)
            {
                token = AzureAccessTokenProvider.GetToken();
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());
                    var secretResponse = client.GetAsync($"{item.Id}?{API.ApiVersion}").Result;
                    if (secretResponse.IsSuccessStatusCode)
                    {
                        var secretJson = secretResponse.Content.ReadAsStringAsync().Result;
                        var secret = JsonConvert.DeserializeObject<KeyVaultSecret>(secretJson);

                        var name = item.Id
                            .Split('/').LastOrDefault()
                            .Replace($"{SecretsProvider.Instance.Config.SecretName}--AppSettings--", string.Empty)
                            .Replace("---", ".")
                            .Replace("--", ":");

                        return new KeyValuePair<string, string>(name, secret.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AzureSecretsKeyMasterException();
            }

            throw new AzureSecretsKeyMasterException(item.Id.Split('/').LastOrDefault());
        }

        private static NameValueCollection GetAppSettings()
        {
            try
            {
                var token = AzureAccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var secrets = string.Format(API.GetAllSecrets, SecretsProvider.Instance.Config.KeyVaultUrl);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());

                    var response = client.GetAsync(secrets).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var items = JsonConvert.DeserializeObject<KeyVaultSecretListResponse>(json);

                        if (items != null)
                        {
                            var appsettings = new NameValueCollection();

                            foreach (var secretListItem in items.Secrets
                                .Where(x => x.Id.Split('/').LastOrDefault().StartsWith($"{SecretsProvider.Instance.Config.SecretName}--AppSettings--")))
                            {

                                var current = GetSecret(secretListItem, token);
                                appsettings.Add(current.Key, current.Value);
                            }

                            return appsettings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new KeyMasterException("Unable to retrieve App Settings", ex);
            }

            throw new KeyMasterException("Azure Key Vault App Settings are empty");
        }

        public static async Task<bool> DeleteSecretAsync(string key)
        {
            try
            {
                var token = AzureAccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var name = key
                        .Replace(":", "--")
                        .Replace(".", "---");

                    name = $"{SecretsProvider.Instance.Config.SecretName}--AppSettings--{name}";

                    var secret = string.Format(API.Secret, SecretsProvider.Instance.Config.KeyVaultUrl, name);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());

                    var response = await client.DeleteAsync(secret);
                    if (response.IsSuccessStatusCode)
                    {
                        AppSettings.Remove(key);
                        return true;
                    }
                    else if (response.ReasonPhrase == "FORBIDDEN")
                    {
                        throw new ForbiddenKeyMasterException();
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Error(ex.Message, ex);
                throw new KeyMasterException("Unable to delete secret", ex);
            }

            return false;
        }

        public static async Task<bool> CreateOrUpdateAppSettingAsync(string key, string value)
        {
            try
            {
                var token = AzureAccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var name = key
                        .Replace(":", "--")
                        .Replace(".", "---");

                    name = $"{SecretsProvider.Instance.Config.SecretName}--AppSettings--{name}";

                    var secret = string.Format(API.Secret, SecretsProvider.Instance.Config.KeyVaultUrl, name);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());

                    var keyVaultSecret = new KeyVaultSecret
                    {
                        Id = secret,
                        Value = value
                    };
                    var json = JsonConvert.SerializeObject(keyVaultSecret);

                    var response = await client.PutAsync(secret, new StringContent(json, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        AppSettings[key] = value;
                        return true;
                    }
                    else if (response.ReasonPhrase == "FORBIDDEN")
                    {
                        throw new ForbiddenKeyMasterException();
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Error(ex.Message, ex);
                throw new KeyMasterException("Unable to delete secret", ex);
            }

            return false;
        }

        public static string GetConnectionString()
        {
            try
            {
                var token = AzureAccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var secretVersions = string.Format(API.Secret, SecretsProvider.Instance.Config.KeyVaultUrl, SecretsProvider.Instance.Config.SecretName);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());
                    var response = client.GetAsync(secretVersions).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        var secret = JsonConvert.DeserializeObject<KeyVaultSecret>(json);

                        if (secret != null)
                        {
                            return secret.Value;
                        }
                    }

                    throw new AzureSecretsKeyMasterException();
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new KeyMasterException("Unable to find Key Master Secrets file, check logs for more details", ex);
            }
            catch (AzureSecretsKeyMasterException ex)
            {
                throw new KeyMasterException("Unable to verify Key Master secrets with Azure, check logs for more details", ex);
            }
            catch (AzureKeyMasterException ex)
            {
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Error(JsonConvert.SerializeObject(ex.TokenError), ex);
                throw new KeyMasterException("Unable to verify Key Master secrets with Azure, check logs for more details", ex);
            }
            catch (Exception ex)
            {
                throw new KeyMasterException("Internal Key Master Error Occurred. Check the logs for more details", ex);
            }
        }
    }
}
