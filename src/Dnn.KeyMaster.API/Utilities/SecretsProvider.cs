using Dnn.KeyMaster.API.Extensions;
using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Configuration;
using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace Dnn.KeyMaster.API.Utilities
{
    // todo - we need to separate Secrets and AppSettings providers
    internal static class SecretsProvider
    {
        internal readonly static string WebconfigFile = $"{HostingEnvironment.MapPath("~/")}web.config";

        internal static bool IsEnvVars()
        {
            return ConfigurationProvider.Instance.Configuration.Secrets["IsEnvVars"] == "true";
        }

        internal static bool ValidateSecrets(Secrets secrets = null)
        {
            try
            {
                var connectionString = GetConnectionString(secrets);

                return !string.IsNullOrWhiteSpace(connectionString);
            }
            catch (Exception ex)
            {
                ex.Handle(canContinue: true);
                return false;
            }
        }

        internal static string GetConnectionString(Secrets secrets = null)
        {
            return Configuration.AppSettingsProvider.Instance.KeyMaster.GetConnectionString(secrets?.ToNameValueCollection());
        }

        internal static IEnumerable<string> GetAppSettingsKeys()
        {
            return Configuration.AppSettingsProvider.Instance.AllKeys;
        }

        internal static string GetAppSettingValue(string key)
        {
            return Configuration.AppSettingsProvider.Instance[key];
        }

        internal static bool DeleteAppSetting(string key)
        {            
            return Configuration.AppSettingsProvider.Instance.KeyMaster.DeleteSecret(key);
        }

        internal static bool CreateOrUpdateAppSetting(string key, string value)
        {
            return Configuration.AppSettingsProvider.Instance.KeyMaster.CreateOrUpdate(key, value);
        }

        internal static Secrets GetConfiguration()
        {
            return new Secrets
            {
                ClientId = ConfigurationProvider.Instance["ClientId"],
                ClientSecret = ConfigurationProvider.Instance["ClientSecret"],
                DirectoryId = ConfigurationProvider.Instance["DirectoryId"],
                KeyVaultUrl = ConfigurationProvider.Instance["KeyVaultUrl"],
                SecretName = ConfigurationProvider.Instance["SecretName"]
            };
        }

        internal static void SaveOrUpdateConfiguration(Secrets secrets)
        {
            ConfigurationProvider.Instance["ClientId"] = secrets.ClientId;
            ConfigurationProvider.Instance["ClientSecret"] = secrets.ClientSecret;
            ConfigurationProvider.Instance["DirectoryId"] = secrets.DirectoryId;
            ConfigurationProvider.Instance["KeyVaultUrl"] = secrets.KeyVaultUrl;
            ConfigurationProvider.Instance["SecretName"] = secrets.SecretName;
        }
    }
}
