using Dnn.KeyMaster.Exceptions;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using DotNetNuke.Instrumentation;
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
                // this should be moved to another entry point hook, but this is the easiest way for now.
                AzureKeyVaultConfigurationProvider.Initialize();

                var connectionString = KeyVaultProvider.GetConnectionString();

                config.Add(ConnectionStringName, connectionString);
                                
                base.Initialize(name, config);
            }
            catch (KeyMasterException ex)
            {
                var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                logger.Fatal("Unrecoverable Key Master error", ex);
                throw ex;
            }
        }
    }
}
