using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Utilities;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dnn.KeyMaster.API.Controllers
{
    [AllowAnonymous]
    public class HomeController : DnnApiController
    {
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Anonymous)]
        [AllowAnonymous]
        public HttpResponseMessage TestSecrets([FromBody] Secrets secrets)
        {
            if (!ModelState.IsValid || secrets == null)
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }

            var appsettings = new AppSettings
            {
                ClientId = secrets.ClientId,
                ClientSecret = secrets.ClientSecret,
                DirectoryId = secrets.DirectoryId,
                SecretName = secrets.SecretName,
                KeyVaultUri = secrets.KeyVaultUrl
            };

            try
            {
                var connectionString = KeyVaultProvider.GetConnectionString(appsettings);

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
            catch(Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
