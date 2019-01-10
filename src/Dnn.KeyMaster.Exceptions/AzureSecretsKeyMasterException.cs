using System;

namespace Dnn.KeyMaster.Exceptions
{
    public class AzureSecretsKeyMasterException : Exception
    {
        public AzureSecretsKeyMasterException()
            : base("Unable to retrieve Azure Secrets") { }
    }
}
