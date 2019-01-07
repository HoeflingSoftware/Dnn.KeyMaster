using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using DotNetNuke.Security;
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
    [AllowAnonymous]
    public class HomeController : DnnApiController
    {
        private readonly string _secretsFile = $"{HostingEnvironment.MapPath("~/")}{SecretsProvider.SecretsFile}";
        private readonly string _webconfigFile = $"{HostingEnvironment.MapPath("~/")}web.config";

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [AllowAnonymous]
        public HttpResponseMessage IsKeyMasterOn()
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

            return connectionString == null ?
                new HttpResponseMessage(HttpStatusCode.OK) :
                new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [AllowAnonymous]
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

        //[HttpPost]
        //[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        //[AllowAnonymous]
        //public HttpResponseMessage DisableKeyMaster()
        //{
        //}

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [AllowAnonymous]
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [AllowAnonymous]
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
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [AllowAnonymous]
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
            var appsettings = new AppSettings
            {
                ClientId = secrets.ClientId,
                ClientSecret = secrets.ClientSecret,
                DirectoryId = secrets.DirectoryId,
                SecretName = secrets.SecretName,
                KeyVaultUrl = secrets.KeyVaultUrl
            };

            var connectionString = KeyVaultProvider.GetConnectionString(appsettings);

            return !string.IsNullOrWhiteSpace(connectionString);
        }
    }
}
