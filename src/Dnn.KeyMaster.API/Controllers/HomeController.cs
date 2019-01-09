using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;
using System.Xml.Linq;

namespace Dnn.KeyMaster.API.Controllers
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class HomeController : DnnApiController
    {
        private readonly string _secretsFile = $"{HostingEnvironment.MapPath("~/")}{SecretsProvider.SecretsFile}";
        private readonly string _webconfigFile = $"{HostingEnvironment.MapPath("~/")}web.config";

        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Status()
        {
            if (!File.Exists(_secretsFile))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var json = File.ReadAllText(_secretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!ValidateSecrets(secrets))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var xml = XDocument.Load(_webconfigFile);
            var doc = xml.Element("configuration");

            var connectionString = doc.Element("connectionStrings");

            var response = new APIResponse<Status>
            {
                Result = new Status
                {
                    IsEnabled = connectionString == null
                }
            };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response.ToJson(), Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage EnableKeyMaster()
        {
            if (!File.Exists(_secretsFile))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var json = File.ReadAllText(_secretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!ValidateSecrets(secrets))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var xml = XDocument.Load(_webconfigFile);
            var doc = xml.Element("configuration");

            var connectionString = doc.Element("connectionStrings");
            if (connectionString != null)
            {
                connectionString.Remove();
            }

            var dnnDataProviders = doc.Element("dotnetnuke")
                ?.Elements("data")
                .Where(x => x.Attribute("defaultProvider").Value == "SqlDataProvider")
                .FirstOrDefault()
                ?.Element("providers");

            if (dnnDataProviders != null)
            {
                dnnDataProviders.Elements().Remove();
                dnnDataProviders.Add(XElement.Parse("<clear />"));
                dnnDataProviders.Add(XElement.Parse("<add name=\"SqlDataProvider\" type=\"Dnn.KeyMaster.Providers.AzureKeyVaultSqlDataProvider, Dnn.KeyMaster.Providers\" upgradeConnectionString=\"\" providerPath=\"~\\Providers\\DataProviders\\SqlDataProvider\\\" objectQualifier=\"\" databaseOwner=\"dbo\" />"));
            }

            var membershipProviders = doc.Element("system.web")
                ?.Element("membership")
                ?.Element("providers");

            if (membershipProviders != null)
            {
                membershipProviders.Elements().Remove();
                membershipProviders.Add(XElement.Parse("<clear />"));
                membershipProviders.Add(XElement.Parse("<add name=\"AspNetSqlMembershipProvider\" type=\"Dnn.KeyMaster.Providers.AzureKeyVaultSqlMembershipProvider, Dnn.KeyMaster.Providers\" enablePasswordReset=\"true\" requiresQuestionAndAnswer=\"false\" minRequiredPasswordLength=\"7\" minRequiredNonalphanumericCharacters=\"0\" requiresUniqueEmail=\"false\" passwordFormat=\"Hashed\" applicationName=\"DotNetNuke\" description=\"Stores and retrieves membership data from the local Microsoft SQL Server database\" />"));
            }
            
            xml.Save(_webconfigFile);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DisableKeyMaster()
        {
            var json = File.ReadAllText(_secretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);

            var connectionString = GetConnectionString(secrets);
            if (string.IsNullOrEmpty(connectionString))
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            var xml = XDocument.Load(_webconfigFile);
            var doc = xml.Element("configuration");

            doc.Add(XElement.Parse($"<connectionStrings><add name=\"SiteSqlServer\" connectionString=\"{connectionString}\" providerName=\"System.Data.SqlClient\" /></connectionStrings>"));

            var dnnDataProviders = doc.Element("dotnetnuke")
                ?.Elements("data")
                .Where(x => x.Attribute("defaultProvider").Value == "SqlDataProvider")
                .FirstOrDefault()
                ?.Element("providers");

            if (dnnDataProviders != null)
            {
                dnnDataProviders.Elements().Remove();
                dnnDataProviders.Add(XElement.Parse("<clear />"));
                dnnDataProviders.Add(XElement.Parse("<add name=\"SqlDataProvider\" type=\"DotNetNuke.Data.SqlDataProvider, DotNetNuke\" connectionStringName=\"SiteSqlServer\" upgradeConnectionString=\"\" providerPath=\"~\\Providers\\DataProviders\\SqlDataProvider\\\" objectQualifier=\"\" databaseOwner=\"dbo\" />"));
            }

            var membershipProviders = doc.Element("system.web")
                ?.Element("membership")
                ?.Element("providers");

            if (membershipProviders != null)
            {
                membershipProviders.Elements().Remove();
                membershipProviders.Add(XElement.Parse("<clear />"));
                membershipProviders.Add(XElement.Parse("<add name=\"AspNetSqlMembershipProvider\" type=\"System.Web.Security.SqlMembershipProvider\" connectionStringName=\"SiteSqlServer\" enablePasswordRetrieval=\"false\" enablePasswordReset=\"true\" requiresQuestionAndAnswer=\"false\" minRequiredPasswordLength=\"7\" minRequiredNonalphanumericCharacters=\"0\" requiresUniqueEmail=\"false\" passwordFormat=\"Hashed\" applicationName=\"DotNetNuke\" description=\"Stores and retrieves membership data from the local Microsoft SQL Server database\" />"));
            }

            xml.Save(_webconfigFile);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage GetSecrets()
        {
            if (!File.Exists(_secretsFile))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var json = File.ReadAllText(_secretsFile);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage SaveSecrets([FromBody] Secrets secrets)
        {
            if (!ModelState.IsValid || secrets == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }

            try
            {
                var isSecretsValid = ValidateSecrets(secrets);

                if (!isSecretsValid)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                File.WriteAllText(_secretsFile, JsonConvert.SerializeObject(secrets));

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage TestSecrets([FromBody] Secrets secrets)
        {
            if (!ModelState.IsValid || secrets == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }
            
            try
            {
                return ValidateSecrets(secrets) ?
                    new HttpResponseMessage(HttpStatusCode.OK) :
                    new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            catch(Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        private bool ValidateSecrets(Secrets secrets)
        {
            var connectionString = GetConnectionString(secrets);

            return !string.IsNullOrWhiteSpace(connectionString);
        }

        private string GetConnectionString(Secrets secrets)
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
