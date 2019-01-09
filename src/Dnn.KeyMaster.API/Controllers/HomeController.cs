using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.API.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
            PersonaBarResponse<Status> response = null;
            if (!File.Exists(SecretsProvider.SecretsFile))
            {
                response = new PersonaBarResponse<Status>
                {
                    Success = true,
                    Result = new Status
                    {
                        IsEnabled = false
                    }
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(response.ToJson(), Encoding.UTF8, "application/json")
                };
            }

            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!SecretsProvider.ValidateSecrets(secrets))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var xml = XDocument.Load(SecretsProvider.WebconfigFile);
            var doc = xml.Element("configuration");

            var connectionString = doc.Element("connectionStrings");

            response = new PersonaBarResponse<Status>
            {
                Success = true,
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
        public HttpResponseMessage Toggle([FromBody] Status status)
        {
            if (!ModelState.IsValid || status == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            if (status.IsEnabled)
            {
                return EnableKeyMaster();
            }

            return DisableKeyMaster();
        }

        private HttpResponseMessage EnableKeyMaster()
        {
            if (!File.Exists(SecretsProvider.SecretsFile))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            if (!SecretsProvider.ValidateSecrets(secrets))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(new PersonaBarResponse { Success = true }.ToJson(), Encoding.UTF8, "application/json")
            };
        }

        public HttpResponseMessage DisableKeyMaster()
        {
            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);

            var connectionString = SecretsProvider.GetConnectionString(secrets);
            if (string.IsNullOrEmpty(connectionString))
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(new PersonaBarResponse { Success = true }.ToJson(), Encoding.UTF8, "application/json")
            };
        }
    }
}
