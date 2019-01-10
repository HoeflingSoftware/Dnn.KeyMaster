using Dnn.KeyMaster.Exceptions.Models;
using System;

namespace Dnn.KeyMaster.Exceptions
{
    public class AzureKeyMasterException : Exception
    {
        public AzureTokenError TokenError { get; private set; }

        
        public AzureKeyMasterException(AzureTokenError error)
            : base("Unable to retrieve Azure Access Token")
        {
            TokenError = error;
        }
    }
}


