using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.Web.Security.KeyVault.Models;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using System.Net.Http;
using System.Text;
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
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{ message: \"Hello World\" }", Encoding.UTF8, "application/json")
            };
        }
    }
}
