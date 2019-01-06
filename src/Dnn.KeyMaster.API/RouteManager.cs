using DotNetNuke.Web.Api;

namespace Dnn.KeyMaster.API
{
    public class RouteManager : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("Dnn.KeyMaster.API", "default", "{controller}/{action}", new[] { "Dnn.KeyMaster.API.Controllers" });
        }
    }
}
