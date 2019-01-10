using Dnn.KeyMaster.Exceptions;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using DotNetNuke.Instrumentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;

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

        private static KeyValuePair<string, string> GetSecret(KeyVaultSecretListItem item, AccessTokenResponse token = null, AppSettings config = null)
        {
            if (config == null)
            {
                config = SecretsProvider.GetSecrets();
            }

            if (token == null)
            {
                token = AzureAccessTokenProvider.GetToken(config);
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
                        var secret = JsonConvert.DeserializeObject<KeyVaultSecretResponse>(secretJson);

                        var name = item.Id
                            .Split('/').LastOrDefault()
                            .Replace($"{config.SecretName}--AppSettings--", string.Empty)
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

        private static NameValueCollection GetAppSettings(AppSettings config = null)
        {
            try
            {
                if (config == null)
                {
                    config = SecretsProvider.GetSecrets();
                }

                var token = AzureAccessTokenProvider.GetToken(config);
                using (var client = new HttpClient())
                {
                    var secrets = string.Format(API.GetAllSecrets, config.KeyVaultUrl);
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
                                .Where(x => x.Id.Split('/').LastOrDefault().StartsWith($"{config.SecretName}--AppSettings--")))
                            {

                                var current = GetSecret(secretListItem, token, config);
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

        public static bool DeleteSecret(string key, AppSettings config = null)
        {
            try
            {
                if (config == null)
                {
                    config = SecretsProvider.GetSecrets();
                }

                var token = AzureAccessTokenProvider.GetToken(config);
                using (var client = new HttpClient())
                {
                    var name = key
                        .Replace(":", "--")
                        .Replace(".", "---");

                    name = $"{config.SecretName}--AppSettings--{name}";

                    var secret = string.Format(API.Secret, config.KeyVaultUrl, name);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());

                    var response = client.DeleteAsync(secret).Result;
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

        public static string GetConnectionString(AppSettings appsettings = null)
        {
            try
            {
                if (appsettings == null)
                {
                    appsettings = SecretsProvider.GetSecrets();
                }

                var token = AzureAccessTokenProvider.GetToken(appsettings);
                using (var client = new HttpClient())
                {
                    var secretVersions = string.Format(API.Secret, appsettings.KeyVaultUrl, appsettings.SecretName);
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
