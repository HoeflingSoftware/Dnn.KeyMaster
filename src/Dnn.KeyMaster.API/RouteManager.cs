using DotNetNuke.Web.Api;

namespace Dnn.KeyMaster.API
{
    public class RouteManager : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Admin/Dnn.PersonaBar/Modules/KeyMaster", "default", "{controller}/{action}", new[] { "Dnn.KeyMaster.API.Controllers" });
        }
    }
}
