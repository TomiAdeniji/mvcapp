using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using Qbicles.BusinessRules.Provider;
using System.IdentityModel.Tokens;


[assembly: OwinStartupAttribute(typeof(Qbicles.SignalR.Startup))]
namespace Qbicles.SignalR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Ensure that log4net is configured
            log4net.Config.XmlConfigurator.Configure();

            app.UseCors(CorsOptions.AllowAll);
            ConfigureAuth(app);
            ConfigureOAuthTokenConsumption(app);
            // Any connection or hub wire up and configuration should go here
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                map.RunSignalR(new HubConfiguration
                {
                    EnableJSONP = true,
                    EnableDetailedErrors = true,
                    EnableJavaScriptProxies = true,
                    //Resolver = GlobalHost.DependencyResolver
                });
            });
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            GlobalHost.HubPipeline.RequireAuthentication();
        }
        /// <summary>
        ///  This is function use to validate token from the IndentityProvider App
        /// </summary>
        /// <param name="app"></param>
        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {

            X509SecurityKey security = new X509SecurityKey(JwtProvider.LoadCertificate());
            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = security,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true
                    },
                    Provider = new TokenOAuthBearerProvider()
                });
        }
    }
}
