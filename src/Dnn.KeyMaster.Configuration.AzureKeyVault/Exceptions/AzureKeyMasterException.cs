using Dnn.KeyMaster.Configuration.AzureKeyVault.Models;
using System;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault.Exceptions
{
    public class AzureKeyMasterException : Exception
    {
        public TokenError TokenError { get; private set; }


        public AzureKeyMasterException(TokenError error)
            : base("Unable to retrieve Azure Access Token")
        {
            TokenError = error;
        }
    }
}
