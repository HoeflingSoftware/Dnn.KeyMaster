using Dnn.KeyMaster.API.Extensions;
using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace Dnn.KeyMaster.API.Utilities
{
    // todo - we need to separate Secrets and AppSettings providers
    internal static class SecretsProvider
    {
        internal readonly static string WebconfigFile = $"{HostingEnvironment.MapPath("~/")}web.config";

        internal static bool ValidateSecrets()
        {
            try
            {
                var connectionString = GetConnectionString();

                return !string.IsNullOrWhiteSpace(connectionString);
            }
            catch (Exception ex)
            {
                ex.Handle(canContinue: true);
                return false;
            }
        }

        internal static string GetConnectionString()
        {
            return KeyVaultProvider.GetConnectionString();
        }

        internal static IEnumerable<string> GetAppSettingsKeys()
        {
            return KeyVaultProvider.AppSettings.AllKeys;
        }

        internal static string GetAppSettingValue(string key)
        {
            return KeyVaultProvider.AppSettings[key];
        }

        internal static bool DeleteAppSetting(string key)
        {            
            return KeyVaultProvider.DeleteSecretAsync(key).Result;
        }

        internal static bool CreateOrUpdateAppSetting(string key, string value)
        {
            return KeyVaultProvider.CreateOrUpdateAppSettingAsync(key, value).Result;
        }

        internal static Secrets GetConfiguration()
        {
            var config = Configuration.SecretsProvider.Instance.Config;
            if (config == null) return null;

            return new Secrets
            {
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                DirectoryId = config.DirectoryId,
                KeyVaultUrl = config.KeyVaultUrl,
                SecretName = config.SecretName
            };
        }

        internal static void SaveOrUpdateConfiguration(Secrets secrets)
        {
            var newSecrets = new Configuration.Secrets(secrets.KeyVaultUrl, secrets.DirectoryId, secrets.ClientId, secrets.ClientSecret, secrets.SecretName);
            Configuration.SecretsProvider.Instance.SaveOrUpdate(newSecrets);
        }
    }
}
