using Dnn.KeyMaster.Configuration.AzureKeyVault.Models;
using DotNetNuke.Instrumentation;
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
                [Keys.KeyVaultUrl] = /*"https://hoeflingsoftware.vault.azure.net/",*/Environment.GetEnvironmentVariable(Keys.KeyVaultUrl),
                [Keys.ClientId] = /*"7b3a0461-d5dd-48db-9d58-7ee931b335de",*/Environment.GetEnvironmentVariable(Keys.ClientId),
                [Keys.ClientSecret] = /*"=@*6#9)$/!.[?=M(p;[$!%;C#?+y&", */Environment.GetEnvironmentVariable(Keys.ClientSecret),
                [Keys.DirectoryId] = /*"dc431e77-b6a6-4540-a71c-ed34e769d5be",*/Environment.GetEnvironmentVariable(Keys.DirectoryId),
                [Keys.SecretName] = /*"DNN--SQL--RX--LOCALHOST"*/Environment.GetEnvironmentVariable(Keys.SecretName)
            };

            if (IsValid())
            {
                _secrets.Add("IsEnvVars", "true");
            }
            else
            {
                _secrets.Add("IsEnvVars", "false");

                if (!File.Exists(_secretsFile))
                {
                    var logger = LoggerSource.Instance.GetLogger("KeyMaster");
                    logger.Error($"Unable to find secrets file: {_secretsFile}");
                    return;
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
            var dictionary = Secrets.AllKeys
                .Where(k => k != "IsEnvVars")
                .ToDictionary(k => k, k => Secrets[k]);

            var json = JsonConvert.SerializeObject(dictionary);
            File.WriteAllText(_secretsFile, json);
        }
    }
}
