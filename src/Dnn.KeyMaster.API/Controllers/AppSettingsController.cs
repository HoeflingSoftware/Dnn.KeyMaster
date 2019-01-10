using Dnn.KeyMaster.API.Models;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using System.Net.Http;
using System.Web.Http;

namespace Dnn.KeyMaster.API.Controllers
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class AppSettingsController : PersonaBarApiController
    {
        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage List()
        {
            var response = new PersonaBarResponse<object>
            {
                Success = true,
                Result = new []
                {
                    new AppSetting
                    {
                        Key = "Test"
                    },
                    new AppSetting
                    {
                        Key = "Demo"
                    }
                }
            };

            return response.ToHttpResponseMessage();
        }
    }
}
