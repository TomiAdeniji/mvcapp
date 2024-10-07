using Microsoft.AspNet.Identity.CoreCompat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.Owin;
using Owin;
using Qbicles.Models;
using System;

[assembly: OwinStartup(typeof(Qbicles.TraderAPI.Startup))]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Web.config", Watch = true)]

namespace Qbicles.TraderAPI
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
