using System.Web.Mvc;
using System.Web.Routing;

namespace Qbicles.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            //The address for registration is to change to
            routes.MapRoute(
              name: "CreateAccount",
              url: "Registration",
              defaults: new { controller = "Account", action = "CreateAccount" }
          );
            routes.MapRoute(
             name: "LoadingPage",
             url: "waiting",
             defaults: new { controller = "Account", action = "Waiting" }
         );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            
        }
    }
}
