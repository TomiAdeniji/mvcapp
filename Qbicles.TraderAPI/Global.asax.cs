using System.IO;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Qbicles.TraderAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));

            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            //set the antiforgery claim to user id
            AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
        }
        protected void Application_PreSendRequestHeaders()
        {
        //    HttpContext.Current.Response.Headers.Add("X-Frame-Options", "DENY");
        //    HttpContext.Current.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        //    HttpContext.Current.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        //    HttpContext.Current.Response.Headers.Remove("Server");
        }
    }
}
