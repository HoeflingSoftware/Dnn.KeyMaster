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
            return Configuration.AppSettingsProvider.Instance.KeyMaster.GetConnectionString();
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


        // TODO - Figure out a way to dynamically load the configuration object
        //        Maybe we add some javascript to help with this
        internal static Secrets GetConfiguration()
        {
            return new Secrets
            {
                ClientId = ConfigurationProvider.Instance.Configuration.Secrets["ClientId"],
                ClientSecret = ConfigurationProvider.Instance.Configuration.Secrets["ClientSecret"],
                DirectoryId = ConfigurationProvider.Instance.Configuration.Secrets["DirectoryId"],
                KeyVaultUrl = ConfigurationProvider.Instance.Configuration.Secrets["KeyVaultUrl"],
                SecretName = ConfigurationProvider.Instance.Configuration.Secrets["SecretName"]
            };
        }

        internal static void SaveOrUpdateConfiguration(Secrets secrets)
        {
            ConfigurationProvider.Instance.Configuration.Secrets["ClientId"] = secrets.ClientId;
            ConfigurationProvider.Instance.Configuration.Secrets["ClientSecret"] = secrets.ClientSecret;
            ConfigurationProvider.Instance.Configuration.Secrets["DirectoryId"] = secrets.DirectoryId;
            ConfigurationProvider.Instance.Configuration.Secrets["KeyVaultUrl"] = secrets.KeyVaultUrl;
            ConfigurationProvider.Instance.Configuration.Secrets["SecretName"] = secrets.SecretName;
        }
    }
}
