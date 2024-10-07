using System.Web;
using System.Web.Mvc;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Qbicles.BusinessRules.Provider;

//[assembly: PreApplicationStartMethod(typeof(Qbicles.Web.InitTempData), "Start")]

namespace Qbicles.Web
{
    public class InitTempData
    {
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(SetFactoryModule));
        }

        public class SetFactoryModule : IHttpModule
        {
            static SetFactoryModule()
            {
                var currentFactory = ControllerBuilder.Current.GetControllerFactory();
                if (!(currentFactory is CookieTempDataControllerFactory))
                {
                    //ControllerBuilder.Current.SetControllerFactory(new CookieTempDataControllerFactory(currentFactory));
                }
            }

            public void Init(HttpApplication app)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}