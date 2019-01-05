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
                var appsettings = SecretsProvider.GetSecrets();
                var connectionString = KeyVaultProvider.GetSecret(appsettings);

                config.Add(ConnectionStringName, connectionString);

                base.Initialize(name, config);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
