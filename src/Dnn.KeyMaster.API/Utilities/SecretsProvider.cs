using Dnn.KeyMaster.API.Extensions;
using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using System;
using System.Web.Hosting;

namespace Dnn.KeyMaster.API.Utilities
{
    internal static class SecretsProvider
    {
        internal readonly static string SecretsFile = $"{HostingEnvironment.MapPath("~/")}{Dnn.KeyMaster.Web.Security.KeyVault.Utilities.SecretsProvider.SecretsFile}";
        internal readonly static string WebconfigFile = $"{HostingEnvironment.MapPath("~/")}web.config";

        internal static bool ValidateSecrets(Secrets secrets)
        {
            try
            {
                var connectionString = GetConnectionString(secrets);

                return !string.IsNullOrWhiteSpace(connectionString);
            }
            catch (Exception ex)
            {
                ex.Handle(canContinue: true);
                return false;
            }
        }

        internal static string GetConnectionString(Secrets secrets)
        {
            var appsettings = new AppSettings
            {
                ClientId = secrets.ClientId,
                ClientSecret = secrets.ClientSecret,
                DirectoryId = secrets.DirectoryId,
                SecretName = secrets.SecretName,
                KeyVaultUrl = secrets.KeyVaultUrl
            };

            return KeyVaultProvider.GetConnectionString(appsettings);
        }
    }
}
