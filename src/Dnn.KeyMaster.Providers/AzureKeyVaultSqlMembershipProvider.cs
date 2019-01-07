using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using System;
using System.Collections.Specialized;
using System.Web.Security;

namespace Dnn.KeyMaster.Providers
{
    public class AzureKeyVaultSqlMembershipProvider : SqlMembershipProvider
    {
        private const string ConnectionStringName = "connectionString";

        public override void Initialize(string name, NameValueCollection config)
        {
            try
            {
                var connectionString = KeyVaultProvider.GetConnectionString();

                config.Add(ConnectionStringName, connectionString);

                base.Initialize(name, config);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
