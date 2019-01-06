using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Newtonsoft.Json;
using System.IO;
using System.Web.Hosting;

namespace Dnn.KeyMaster.Web.Security.KeyVault.Utilities
{
    public static class SecretsProvider
    {
        public const string SecretsFile = "secrets.json";

        public static AppSettings GetSecrets()
        {
            var file = $"{HostingEnvironment.MapPath("~/")}{SecretsFile}";
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"Unable to find secrets file: {file}");
            }

            using (var reader = File.OpenText(file))
            {
                var json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<AppSettings>(json);
            }
        }
    }
}
