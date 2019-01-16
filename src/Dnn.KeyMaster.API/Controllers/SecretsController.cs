using Dnn.KeyMaster.API.Extensions;
using Dnn.KeyMaster.API.Models;
using Dnn.KeyMaster.API.Utilities;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Dnn.KeyMaster.API.Controllers
{
    [MenuPermission(Scope = ServiceScope.Host)]
    public class SecretsController : PersonaBarApiController
    {
        [HttpGet]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Get()
        {
            var response = new PersonaBarResponse();
            try
            {
                var secrets = SecretsProvider.GetConfiguration();

                response = new PersonaBarResponse<Secrets>
                {
                    Success = true,
                    Result = secrets
                };

                return response.ToHttpResponseMessage();
            }
            catch (Exception ex)
            {
                ex.Handle();
                response.Success = false;
            }

            return response.ToHttpResponseMessage();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Test([FromBody] Secrets secrets)
        {
            PersonaBarResponse response = new PersonaBarResponse();

            if (!ModelState.IsValid || secrets == null)
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            try
            {
                response.Success = SecretsProvider.ValidateSecrets(secrets);
                return response.ToHttpResponseMessage();
            }
            catch (Exception ex)
            {
                ex.Handle();
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage Save([FromBody] Secrets secrets)
        {
            PersonaBarResponse response = new PersonaBarResponse();

            if (!ModelState.IsValid || secrets == null)
            {
                response.Success = false;
                return response.ToHttpResponseMessage();
            }

            try
            {
                var isValid = SecretsProvider.ValidateSecrets(secrets);
                if (!isValid)
                {
                    response.Success = false;
                    return response.ToHttpResponseMessage();
                }

                SecretsProvider.SaveOrUpdateConfiguration(secrets);

                response.Success = true;
                return response.ToHttpResponseMessage();
            }
            catch (Exception ex)
            {
                ex.Handle();
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
