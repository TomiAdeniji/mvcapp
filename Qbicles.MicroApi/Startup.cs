using Microsoft.IdentityModel.Logging;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Qbicles.MicroApi.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]

namespace Qbicles.MicroApi
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Ensure that log4net is configured
            log4net.Config.XmlConfigurator.Configure();

            ConfigureAuth(app);
            IdentityModelEventSource.ShowPII = true;

        }
    }
}
