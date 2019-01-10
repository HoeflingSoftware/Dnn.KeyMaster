using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.API.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            PersonaBarResponse response = null;
            if (!File.Exists(SecretsProvider.SecretsFile))
            {
                response = new PersonaBarResponse
                {
                    Success = false
                };

                return response.ToHttpResponseMessage();
            }

            var json = File.ReadAllText(SecretsProvider.SecretsFile);
            var secrets = JsonConvert.DeserializeObject<Secrets>(json);
            var keys = SecretsProvider.GetAppSettings(secrets);

            response = new PersonaBarResponse<IEnumerable<AppSetting>>
            {
                Success = true,
                Result = keys.Select(x => new AppSetting
                {
                    Key = x
                })
            };

            return response.ToHttpResponseMessage();
        }
    }
}
