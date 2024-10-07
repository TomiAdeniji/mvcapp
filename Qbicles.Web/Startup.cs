using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Qbicles.Web.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]
namespace Qbicles.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Ensure that log4net is configured
            log4net.Config.XmlConfigurator.Configure();

            ConfigureAuth(app);
            //app.UseCors(CorsOptions.AllowAll);

        }

    }
}
