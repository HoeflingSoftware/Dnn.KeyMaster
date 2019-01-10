using Dnn.KeyMaster.Exceptions;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Newtonsoft.Json;
using System;
using System.IO;
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
            try
            {
                if (appsettings == null)
                {
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

                    throw new AzureSecretsKeyMasterException();
                }
            }
            catch (FileNotFoundException ex)
            {
                // TODO - add additional logging about the file not being found
                throw new KeyMasterException("Unable to find Key Master Secrets file, check logs for more details", ex);
            }
            catch (AzureSecretsKeyMasterException ex)
            {
                // TODO - handle any specific logging
                throw new KeyMasterException("Unable to verify Key Master secrets with Azure, check logs for more details", ex);
            }
            catch (AzureKeyMasterException ex)
            {
                // TODO - add additional logging about the file not being found
                throw new KeyMasterException("Unable to verify Key Master secrets with Azure, check logs for more details", ex);
            }
            catch (Exception ex)
            {
                // todo - add additional logging
                throw new KeyMasterException("Internal Key Master Error Occurred. Check the logs for more details", ex);
            }
        }
    }
}
