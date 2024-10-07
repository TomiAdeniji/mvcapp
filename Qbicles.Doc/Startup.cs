using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Owin;
using Qbicles.BusinessRules.Provider;
using System.IdentityModel.Tokens;

[assembly: OwinStartup(typeof(Qbicles.Doc.Startup))]

namespace Qbicles.Doc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Ensure that log4net is configured
            log4net.Config.XmlConfigurator.Configure();

            ConfigureAuth(app);
            ConfigureOAuthTokenConsumption(app);
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