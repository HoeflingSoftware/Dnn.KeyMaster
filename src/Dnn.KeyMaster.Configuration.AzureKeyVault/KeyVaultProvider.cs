using Dnn.KeyMaster.Configuration.AzureKeyVault.Exceptions;
using Dnn.KeyMaster.Configuration.AzureKeyVault.Models;
using Dnn.KeyMaster.Configurations.AzureKeyVault.Exceptions;
using Dnn.KeyMaster.Exceptions;
using DotNetNuke.Instrumentation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault
{
    public class KeyVaultProvider : IKeyMasterAppSettings
    {
        private class API
        {
            public const string ApiVersion = "api-version=7.0";
            public const string Secret = "{0}/secrets/{1}?" + ApiVersion;
            public const string GetAllSecrets = "{0}/secrets?" + ApiVersion;
        }

        private KeyValuePair<string,string> GetSecret(KeyVaultSecretListItem item, AccessToken token = null)
        {
            if (token == null)
            {
                token = AccessTokenProvider.GetToken();
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
                            .Replace($"{SecretsConfiguration._secrets["SecretName"]}--AppSettings--", string.Empty)
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

        public bool CreateOrUpdate(string key, string value)
        {
            try
            {
                var token = AccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var name = key
                        .Replace(":", "--")
                        .Replace(".", "---");

                    name = $"{SecretsConfiguration._secrets["SecretName"]}--AppSettings--{name}";

                    var secret = string.Format(API.Secret, SecretsConfiguration._secrets["KeyVaultUrl"], name);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());

                    var keyVaultSecret = new KeyVaultSecret
                    {
                        Id = secret,
                        Value = value
                    };
                    var json = JsonConvert.SerializeObject(keyVaultSecret);

                    var response = client.PutAsync(secret, new StringContent(json, Encoding.UTF8, "application/json")).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ConfigurationManager.AppSettings[key] = value;
                        return true;
                    }
                    else if (response.ReasonPhrase == "FORBIDDEN")
                    {
                        throw new ForbiddenKeyMasterException();
                    }

                    var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                    logger.Error("unable to Create or Update Secret, see response for details");
                    logger.Error(JsonConvert.SerializeObject(response));
                }
            }
            catch (Exception ex)
            {
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Error(ex.Message, ex);
                throw new KeyMasterException("Unable to Create or Update secret", ex);
            }

            return false;
        }

        public bool DeleteSecret(string key)
        {
            try
            {
                var token = AccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var name = key
                        .Replace(":", "--")
                        .Replace(".", "---");

                    name = $"{SecretsConfiguration._secrets["SecretName"]}--AppSettings--{name}";

                    var secret = string.Format(API.Secret, SecretsConfiguration._secrets["KeyVaultUrl"], name);
                    client.DefaultRequestHeaders.Add("Authorization", token.ToString());

                    var response = client.DeleteAsync(secret).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        ConfigurationManager.AppSettings.Remove(key);
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

        public NameValueCollection GetAppSettings()
        {
            try
            {
                var token = AccessTokenProvider.GetToken();
                using (var client = new HttpClient())
                {
                    var secrets = string.Format(API.GetAllSecrets, SecretsConfiguration._secrets["KeyVaultUrl"]);
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
                                .Where(x => x.Id.Split('/').LastOrDefault().StartsWith($"{SecretsConfiguration._secrets["SecretName"]}--AppSettings--")))
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
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Error("Unable to retrieve App Settings", ex);
                return ConfigurationManager.AppSettings;
            }

            throw new KeyMasterException("Azure Key Vault App Settings are empty");
        }

        public string GetConnectionString(NameValueCollection secrets = null)
        {
            try
            {
                secrets = secrets ?? ConfigurationProvider.Instance.Configuration.Secrets;
                var token = AccessTokenProvider.GetToken(secrets);
                using (var client = new HttpClient())
                {
                    var secretVersions = string.Format(API.Secret, secrets[Keys.KeyVaultUrl], secrets[Keys.SecretName]);
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

        public string GetSecret(string key)
        {
            throw new NotImplementedException();
        }
    }
}
