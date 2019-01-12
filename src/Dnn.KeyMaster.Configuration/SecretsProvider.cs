using Dnn.KeyMaster.Configuration.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web.Hosting;

namespace Dnn.KeyMaster.Configuration
{
    public class SecretsProvider
    {
        private readonly string _secretsFile = $"{HostingEnvironment.MapPath("~/")}secrets.json.resources";
        public Secrets Config { get; private set; }
        private static object _lockobject = new object();
        private static SecretsProvider _instance;
        public static SecretsProvider Instance
        {
            get
            {
                lock(_lockobject)
                {
                    if (_instance == null)
                    {
                        _instance = new SecretsProvider();
                    }

                    return _instance;
                }
            }
        }

        public SecretsProvider()
        {
            Config = new Secrets
            {
                KeyVaultUrl = Environment.GetEnvironmentVariable(Secrets.Keys.KeyVaultUrl),
                DirectoryId = Environment.GetEnvironmentVariable(Secrets.Keys.DirectoryId),
                ClientId = Environment.GetEnvironmentVariable(Secrets.Keys.ClientId),
                ClientSecret = Environment.GetEnvironmentVariable(Secrets.Keys.ClientSecret),
                SecretName = Environment.GetEnvironmentVariable(Secrets.Keys.SecretName),
            };

            if (!Config.IsValid())
            {
                if (!File.Exists(_secretsFile))
                {
                    throw new FileNotFoundException($"Unable to find secrets file: {_secretsFile}");
                }

                using (var reader = File.OpenText(_secretsFile))
                {
                    var json = reader.ReadToEnd();
                    Config = JsonConvert.DeserializeObject<Secrets>(json);
                }
            }
        }

        public void SaveOrUpdate(Secrets secrets)
        {
            File.WriteAllText(_secretsFile, JsonConvert.SerializeObject(secrets));
            Config = secrets;
        }
    }
}