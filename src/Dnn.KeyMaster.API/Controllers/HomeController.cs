using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.API.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace Dnn.KeyMaster.API.Controllers
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class HomeController : PersonaBarApiController
    {
        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Status()
        {
            PersonaBarResponse response = new PersonaBarResponse();
            if (!File.Exists(SecretsProvider.SecretsFile))
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!SecretsProvider.ValidateSecrets(secrets))
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");

            var connectionString = doc.Element("connectionStrings");

            response.Success = connectionString == null;
            return response.ToHttpResponseMessage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Toggle([FromBody] Status status)
        {
            PersonaBarResponse response = new PersonaBarResponse();
            if (!ModelState.IsValid || status == null)
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            if (status.IsEnabled)
            {
                return EnableKeyMaster();
            }

            return DisableKeyMaster();
        }

        private HttpResponseMessage EnableKeyMaster()
        {
            var response = new PersonaBarResponse();
            if (!File.Exists(SecretsProvider.SecretsFile))
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!SecretsProvider.ValidateSecrets(secrets))
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
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
            
            xml.Save(SecretsProvider.WebconfigFile);

            response.Success = true;
            return response.ToHttpResponseMessage();
        }

        public HttpResponseMessage DisableKeyMaster()
        {
            var response = new PersonaBarResponse();
            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);

            var connectionString = SecretsProvider.GetConnectionString(secrets);
            if (string.IsNullOrEmpty(connectionString))
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
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

            xml.Save(SecretsProvider.WebconfigFile);

            response.Success = true;
            return response.ToHttpResponseMessage();
        }
    }
}
