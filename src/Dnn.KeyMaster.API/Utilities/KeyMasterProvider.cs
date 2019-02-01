using Dnn.KeyMaster.Configuration;
using Dnn.KeyMaster.Exceptions;
using DotNetNuke.Instrumentation;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace Dnn.KeyMaster.API.Utilities
{
    internal static class KeyMasterProvider
    {
        internal static bool ToggleOff()
        {
            var connectionString = SecretsProvider.GetConnectionString();
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

            var isDeleteSuccessful = AppSettingsProvider.Instance.KeyMaster.DeleteSecret("ConnectionString", false);
            if (!isDeleteSuccessful)
            {
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Error($"Key master couldn't delete connection string from Azure");
            }

            return true;
        }

        internal static void SendAppSettings()
        {
            var secretKeys = new[] { "ClientId", "ClientSecret", "DirectoryId", "KeyVaultUrl", "SecretName" };
            foreach (var key in ConfigurationManager.AppSettings.AllKeys.Where(x => !secretKeys.Contains(x)))
            {
                var isSuccessful = AppSettingsProvider.Instance.KeyMaster.CreateOrUpdate(key, ConfigurationManager.AppSettings[key]);
                if (!isSuccessful)
                {
                    var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                    logger.Error($"Key master couldn't send app secrets to azure: {key}. ** This app secret may be added by your hosting environment and not required **");
                }
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");

            var connectionStrings = doc.Element("connectionStrings");
            if (connectionStrings != null)
            {
                var siteConnection = connectionStrings.Elements().FirstOrDefault();
                var connectionValue = siteConnection.Attribute("connectionString")?.Value;

                var isSuccessful = AppSettingsProvider.Instance.KeyMaster.CreateOrUpdate("ConnectionString", connectionValue, false);
                if (!isSuccessful)
                {
                    var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                    logger.Error($"Key master couldn't send connection string to Azure");
                }
            }


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

            foreach (var key in AppSettingsProvider.Instance.AllKeys)
            {
                var secret = AppSettingsProvider.Instance[key];
                var isSuccessful = AppSettingsProvider.Instance.KeyMaster.DeleteSecret(key);
                if (!isSuccessful)
                {
                    var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                    logger.Error($"Key Master couldn't delete app secrets from azure: {key}. ** This app secret may be added by your hosting environment and not required **");
                }

                appSettings.Add(XElement.Parse($"<add key=\"{key}\" value=\"{secret}\" />"));
            }
            
            xml.Save(SecretsProvider.WebconfigFile);
        }

        internal static bool ToggleOn()
        {
            if (!SecretsProvider.ValidateSecrets())
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
