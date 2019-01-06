using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Hosting;
using System.Web.Http;

namespace Dnn.KeyMaster.API.Controllers
{
    [AllowAnonymous]
    public class HomeController : DnnApiController
    {
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [AllowAnonymous]
        public HttpResponseMessage GetSecrets()
        {
            var secretsFile = $"{HostingEnvironment.MapPath("~/")}{SecretsProvider.SecretsFile}";
            if (!File.Exists(secretsFile))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var json = File.ReadAllText(secretsFile);

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

                var secretsFile = $"{HostingEnvironment.MapPath("~/")}{SecretsProvider.SecretsFile}";
                File.WriteAllText(secretsFile, JsonConvert.SerializeObject(secrets));

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
                KeyVaultUri = secrets.KeyVaultUrl
            };

            var connectionString = KeyVaultProvider.GetConnectionString(appsettings);

            return !string.IsNullOrWhiteSpace(connectionString);
        }
    }
}
