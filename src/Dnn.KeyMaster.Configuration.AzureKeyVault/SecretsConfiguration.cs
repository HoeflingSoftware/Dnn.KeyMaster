using Dnn.KeyMaster.Configuration.AzureKeyVault.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Web.Hosting;

namespace Dnn.KeyMaster.Configuration.AzureKeyVault
{
    public class SecretsConfiguration : IKeyMasterConfiguration
    {
        private readonly string _secretsFile = $"{HostingEnvironment.MapPath("~/")}secrets.json.resources";

        internal static NameValueCollection _secrets;
        public NameValueCollection Secrets
        {
            get
            {
                if (_secrets == null)
                {
                    throw new Exception("Secrets not initialized");
                }

                return _secrets;
            }
        }
        public void Initialize()
        {
            _secrets = new NameValueCollection
            {
                [Keys.KeyVaultUrl] = Environment.GetEnvironmentVariable(Keys.KeyVaultUrl),
                [Keys.ClientId] = Environment.GetEnvironmentVariable(Keys.ClientId),
                [Keys.ClientSecret] = Environment.GetEnvironmentVariable(Keys.ClientSecret),
                [Keys.DirectoryId] = Environment.GetEnvironmentVariable(Keys.DirectoryId),
                [Keys.SecretName] = Environment.GetEnvironmentVariable(Keys.SecretName)
            };
            
            if (!IsValid())
            {
                if (!File.Exists(_secretsFile))
                {
                    throw new FileNotFoundException($"Unable to find secrets file: {_secretsFile}");
                }

                using (var reader = File.OpenText(_secretsFile))
                {
                    var json = reader.ReadToEnd();
                    var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                    _secrets = new NameValueCollection();
                    foreach (var item in dictionary)
                    {
                        Secrets.Add(item.Key, item.Value);
                    }
                }
            }
        }

        private bool IsValid()
        {
            return
                !string.IsNullOrEmpty(Secrets[Keys.KeyVaultUrl]) &&
                !string.IsNullOrEmpty(Secrets[Keys.DirectoryId]) &&
                !string.IsNullOrEmpty(Secrets[Keys.ClientId]) &&
                !string.IsNullOrEmpty(Secrets[Keys.ClientSecret]) &&
                !string.IsNullOrEmpty(Secrets[Keys.SecretName]);
        }

        public void SaveOrUpdate()
        {
            var dictionary = Secrets.AllKeys.ToDictionary(k => Secrets[k]);
            var json = JsonConvert.SerializeObject(dictionary);
            File.WriteAllText(_secretsFile, json);
        }
    }
}
