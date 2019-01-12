using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.API.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
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
            if (!SecretsProvider.ValidateSecrets())
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
                KeyMasterProvider.SendAppSettings();
                response.Success = KeyMasterProvider.ToggleOn();
            }
            else
            {
                KeyMasterProvider.DownloadAppSettings();
                response.Success = KeyMasterProvider.ToggleOff();
            }

            return response.ToHttpResponseMessage();
        }
    }
}
