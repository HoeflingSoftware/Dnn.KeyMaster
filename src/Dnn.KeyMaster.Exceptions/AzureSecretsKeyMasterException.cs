using System;

namespace Dnn.KeyMaster.Exceptions
{
    public class AzureSecretsKeyMasterException : Exception
    {
        public string SecretName { get; private set; }
        public AzureSecretsKeyMasterException()
            : base("Unable to retrieve Azure Secrets") { }

        public AzureSecretsKeyMasterException(string secretName)
            : base("unable to read AppSetts from Azure Key Vault")
        {
            SecretName = secretName;
        }
    }
}
