using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.API.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace Dnn.KeyMaster.API.Controllers
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class AppSettingsController : PersonaBarApiController
    {
        // todo - add exception handling

        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage List()
        {
            var keys = SecretsProvider.GetAppSettingsKeys();

            var response = new PersonaBarResponse<IEnumerable<AppSetting>>
            {
                Success = true,
                Result = keys.Select(x => new AppSetting
                {
                    Key = x
                })
            };

            return response.ToHttpResponseMessage();
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Get(string key)
        {
            var value = SecretsProvider.GetAppSettingValue(key);

            var response = new PersonaBarResponse<AppSetting>
            {
                Success = true,
                Result = new AppSetting
                {
                    Key = key,
                    Value = value
                }
            };

            return response.ToHttpResponseMessage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Delete([FromBody] AppSetting appsetting)
        {
            var response = new PersonaBarResponse();
            if (string.IsNullOrEmpty(appsetting.Key))
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }
            
            response.Success = SecretsProvider.DeleteAppSetting(appsetting.Key);
            return response.ToHttpResponseMessage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Save([FromBody] AppSetting appsetting)
        {
            var response = new PersonaBarResponse();
            if (!ModelState.IsValid || appsetting == null)
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            };
        
            response.Success = SecretsProvider.CreateOrUpdateAppSetting(appsetting.Key, appsetting.Value);
            return response.ToHttpResponseMessage();
        }
    }
}
