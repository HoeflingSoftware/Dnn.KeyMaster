using HoeflingSoftware.Web.Security.Models;
using HoeflingSoftware.Web.Security.Utilities;
using System;
using System.Collections.Specialized;
using System.Web.Security;

namespace HoeflingSoftware.Web.Security.Providers
{
    public class AzureKeyVaultSqlMembershipProvider : SqlMembershipProvider
    {
        private const string DefaultSecretName = "ConnectionString";
        private const string ConnectionStringName = "connectionString";

        public override void Initialize(string name, NameValueCollection config)
        {
            try
            {
                var appsettings = new AppSettings
                {
                    KeyVaultUri = config[AppSettings.Keys.KeyVaultUri],
                    DirectoryId = config[AppSettings.Keys.DirectoryId],
                    SecretName = config[AppSettings.Keys.SecretName] ?? DefaultSecretName,
                    ClientId = config[AppSettings.Keys.ClientId],
                    ClientSecret = config[AppSettings.Keys.ClientSecret]
                };

                if (string.IsNullOrEmpty(appsettings.ClientId) || string.IsNullOrEmpty(appsettings.ClientSecret))
                {
                    // search env settings instead
                }

                var connectionString = KeyVaultProvider.GetSecret(appsettings);

                config.Remove(AppSettings.Keys.KeyVaultUri);
                config.Remove(AppSettings.Keys.DirectoryId);
                config.Remove(AppSettings.Keys.SecretName);
                config.Remove(AppSettings.Keys.ClientId);
                config.Remove(AppSettings.Keys.ClientSecret);

                config.Add(ConnectionStringName, connectionString);

                base.Initialize(name, config);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
