using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Exceptions;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Dnn.KeyMaster.API.Utilities
{
    internal static class KeyMasterProvider
    {
        internal static bool ToggleOff()
        {
            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);

            var connectionString = SecretsProvider.GetConnectionString(secrets);
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");

            doc.Add(XElement.Parse($"<connectionStrings><add name=\"SiteSqlServer\" connectionString=\"{connectionString}\" providerName=\"System.Data.SqlClient\" /></connectionStrings>"));

            var dnnDataProviders = doc.Element("dotnetnuke")
                ?.Elements("data")
                .Where(x => x.Attribute("defaultProvider").Value == "SqlDataProvider")
                .FirstOrDefault()
                ?.Element("providers");

            if (dnnDataProviders != null)
            {
                dnnDataProviders.Elements().Remove();
                dnnDataProviders.Add(XElement.Parse("<clear />"));
                dnnDataProviders.Add(XElement.Parse("<add name=\"SqlDataProvider\" type=\"DotNetNuke.Data.SqlDataProvider, DotNetNuke\" connectionStringName=\"SiteSqlServer\" upgradeConnectionString=\"\" providerPath=\"~\\Providers\\DataProviders\\SqlDataProvider\\\" objectQualifier=\"\" databaseOwner=\"dbo\" />"));
            }

            var membershipProviders = doc.Element("system.web")
                ?.Element("membership")
                ?.Element("providers");

            if (membershipProviders != null)
            {
                membershipProviders.Elements().Remove();
                membershipProviders.Add(XElement.Parse("<clear />"));
                membershipProviders.Add(XElement.Parse("<add name=\"AspNetSqlMembershipProvider\" type=\"System.Web.Security.SqlMembershipProvider\" connectionStringName=\"SiteSqlServer\" enablePasswordRetrieval=\"false\" enablePasswordReset=\"true\" requiresQuestionAndAnswer=\"false\" minRequiredPasswordLength=\"7\" minRequiredNonalphanumericCharacters=\"0\" requiresUniqueEmail=\"false\" passwordFormat=\"Hashed\" applicationName=\"DotNetNuke\" description=\"Stores and retrieves membership data from the local Microsoft SQL Server database\" />"));
            }

            xml.Save(SecretsProvider.WebconfigFile);

            return true;
        }

        internal static void SendAppSettings()
        {
            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                var result = KeyVaultProvider.CreateOrUpdateAppSetting(key, ConfigurationManager.AppSettings[key]);
                if (!result)
                {
                    throw new KeyMasterException("Key master couldn't send app secrets to azure");
                }
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");

            var appSettings = doc.Element("appSettings");
            if (appSettings != null)
            {
                appSettings.Remove();
            }

            xml.Save(SecretsProvider.WebconfigFile);
        }

        internal static void DownloadAppSettings()
        {
            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");


            var appSettings = doc.Element("appSettings");
            if (appSettings == null)
            {
                doc.Add(XElement.Parse("<appSettings></appSettings>"));
                appSettings = doc.Element("appSettings");
            }

            appSettings.Elements().Remove();

            foreach (var key in ConfigurationManager.AppSettings.AllKeys)
            {
                var secret = ConfigurationManager.AppSettings[key];
                appSettings.Add(XElement.Parse($"<add key=\"{key}\" value=\"{secret}\" />"));
            }

            xml.Save(SecretsProvider.WebconfigFile);
        }

        internal static bool ToggleOn()
        {
            if (!File.Exists(SecretsProvider.SecretsFile))
            {
                return false;
            }

            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!SecretsProvider.ValidateSecrets(secrets))
            {
                return false;
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");

            var connectionString = doc.Element("connectionStrings");
            if (connectionString != null)
            {
                connectionString.Remove();
            }

            var dnnDataProviders = doc.Element("dotnetnuke")
                ?.Elements("data")
                .Where(x => x.Attribute("defaultProvider").Value == "SqlDataProvider")
                .FirstOrDefault()
                ?.Element("providers");

            if (dnnDataProviders != null)
            {
                dnnDataProviders.Elements().Remove();
                dnnDataProviders.Add(XElement.Parse("<clear />"));
                dnnDataProviders.Add(XElement.Parse("<add name=\"SqlDataProvider\" type=\"Dnn.KeyMaster.Providers.AzureKeyVaultSqlDataProvider, Dnn.KeyMaster.Providers\" upgradeConnectionString=\"\" providerPath=\"~\\Providers\\DataProviders\\SqlDataProvider\\\" objectQualifier=\"\" databaseOwner=\"dbo\" />"));
            }

            var membershipProviders = doc.Element("system.web")
                ?.Element("membership")
                ?.Element("providers");

            if (membershipProviders != null)
            {
                membershipProviders.Elements().Remove();
                membershipProviders.Add(XElement.Parse("<clear />"));
                membershipProviders.Add(XElement.Parse("<add name=\"AspNetSqlMembershipProvider\" type=\"Dnn.KeyMaster.Providers.AzureKeyVaultSqlMembershipProvider, Dnn.KeyMaster.Providers\" enablePasswordReset=\"true\" requiresQuestionAndAnswer=\"false\" minRequiredPasswordLength=\"7\" minRequiredNonalphanumericCharacters=\"0\" requiresUniqueEmail=\"false\" passwordFormat=\"Hashed\" applicationName=\"DotNetNuke\" description=\"Stores and retrieves membership data from the local Microsoft SQL Server database\" />"));
            }

            xml.Save(SecretsProvider.WebconfigFile);

            return true;
        }
    }
}
