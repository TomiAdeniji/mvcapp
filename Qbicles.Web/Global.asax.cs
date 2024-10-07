using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Web.config")));
#if !DEBUG
            PingApi();        
#endif
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            //set the antiforgery claim to user id
            AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
        }

        //protected void Application_Error()
        //{
        //    Exception ex = Server.GetLastError();
        //    if (ex is HttpAntiForgeryException)
        //    {
        //        Response.Clear();
        //        Server.ClearError(); //make sure you log the exception first
        //        Response.Redirect($"{ConfigManager.AuthHost}/Account/Login", true);
        //    }
        //}

        private static void PingApi()
        {
            try
            {
                //remove a previous job
                if (HttpContext.Current != null && HttpContext.Current.Cache["Refresh"] != null)
                {
                    var remove = HttpContext.Current.Cache["Refresh"] as Action;
                    if (remove is Action)
                    {
                        HttpContext.Current.Cache.Remove("Refresh");
                        remove.EndInvoke(null);
                    }
                }
            }
            finally
            {
                //get the worker
                Action work = () =>
                {
                    while (true)
                    {
                        Thread.Sleep(3600000);//1 hour
                        using (var apClient = new WebClient())
                        {
                            try
                            {
                                apClient.UploadString(ConfigManager.SignalRApi, string.Empty);
                                apClient.UploadString(ConfigManager.TraderApi, string.Empty);
                                apClient.UploadString(ConfigManager.QbiclesJobApi, string.Empty);
                                apClient.UploadString(ConfigManager.DocumentsApi, string.Empty);
                                apClient.UploadString(ConfigManager.AuthHost, string.Empty);
                            }

                            catch (Exception ex)
                            {
                                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                            }
                            finally
                            {
                                apClient.Dispose();
                            }
                        }
                    }
                };
                work.BeginInvoke(null, null);
                try
                {
                    //add this job to the cache
                    if (HttpContext.Current != null)
                        HttpContext.Current.Cache.Add(
                            "Refresh",
                            work,
                            null,
                            Cache.NoAbsoluteExpiration,
                            Cache.NoSlidingExpiration,
                            CacheItemPriority.Normal,
                            (s, o, r) => { PingApi(); }
                        );
                }
                catch (Exception ex)
                {
                    LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                }
            }

        }

        //protected void Application_BeginRequest(object sender, EventArgs e)
        //{
        //    if (Request.HttpMethod == "POST")
        //    {
        //        var cookieToken = Request.Cookies["__RequestVerificationToken"];
        //        var formToken = Request.Form["__RequestVerificationToken"];
        //        if (formToken != null && cookieToken != null)
        //        {
        //            try
        //            {
        //                AntiForgery.Validate(cookieToken.Value, formToken);
        //            }
        //            catch
        //            {
        //            }
        //        }
        //    }
        //    else
        //    {
        //        AntiForgery.GetTokens(null, out var cookieToken, out var formToken);
        //        Response.Cookies.Set(new HttpCookie("__RequestVerificationToken")
        //        {
        //            HttpOnly = true,
        //            Value = cookieToken,
        //            SameSite = SameSiteMode.Strict, // Set SameSite mode for the cookie
        //            Secure = Request.IsSecureConnection // Set Secure flag based on connection type
        //        });
        //    }
        //}
    }
}
