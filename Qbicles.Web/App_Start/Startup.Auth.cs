using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Provider;
using Qbicles.Models;

namespace Qbicles.Web
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            //System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            var baseUrl = ConfigManager.QbiclesUrl.TrimEnd('/');



            var cookieOptions = new CookieAuthenticationOptions();

            #region Sort out the cookie options

            // Get the value of the cookie domain setting from the web.config
            if (ConfigurationManager.AppSettings["CookieDomain"] != null)
            {
                var cookieDomain = ConfigurationManager.AppSettings["CookieDomain"].ToString();
                if (!string.IsNullOrEmpty(cookieDomain))
                {
                    cookieOptions.CookieDomain = cookieDomain;
                }
            }
            cookieOptions.AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie;
            cookieOptions.CookieName = "QbiclesAuth";
            //cookieOptions.SameSite = SameSiteMode.Strict;
            cookieOptions.ExpireTimeSpan = TimeSpan.FromDays(30);
            cookieOptions.SlidingExpiration = true;
            cookieOptions.Provider = new CookieAuthenticationProvider
            {
                // Enables the application to validate the security stamp when the user logs in.
                // This is a security feature which is used when you change a password or add an external login to your account.  
                OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    validateInterval: TimeSpan.FromDays(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
            };

            # endregion

            app.UseCookieAuthentication(cookieOptions);

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = ConfigManager.AuthHost,
                ClientId = "web_app",
                ClientSecret = ConfigManager.ClientSecret,
                RedirectUri = baseUrl,//Net4MvcClient's URL
                //PostLogoutRedirectUri = baseUrl+ "/signout-callback-oidc",
                ResponseType = "id_token token",
                Scope = "openid profile",
                UseTokenLifetime = false,
                ProtocolValidator = new CustomOpenIdConnectProtocolValidator(true),
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new X509SecurityKey(JwtProvider.LoadCertificate()),
                },

                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                    {
                        //n.AuthenticationTicket.Properties.IsPersistent = true;
                        //n.AuthenticationTicket.Properties.ExpiresUtc = DateTime.Today.AddDays(30);
                        n.AuthenticationTicket.Identity.AddClaim(new Claim(SystemDomainConst.TOKENAUTH, n.ProtocolMessage.AccessToken));
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("expires_in", n.ProtocolMessage.ExpiresIn));
                        n.AuthenticationTicket.Identity.AddClaim(new Claim("workgroup", string.Empty));
                        //n.AuthenticationTicket.Identity.AddClaim(new Claim("refresh_token", n.ProtocolMessage.RefreshToken));
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = n =>
                    {
                        // Add return url of shared promotion or shared highlight post to cookie to use in wizard
                        var redirectUri = HttpContext.Current.Request.Url.AbsoluteUri;

                        if (redirectUri.ToLower().Contains("highlightpostdetail") || redirectUri.ToLower().Contains("promotiondetailview"))
                        {
                            var response = HttpContext.Current.Response;
                            response.Cookies.Remove("SharingPostUrlCookie");
                            HttpCookie cookie = new HttpCookie("SharingPostUrlCookie");
                            cookie.SameSite = SameSiteMode.Lax;
                            cookie.Value = redirectUri;
                            cookie.Expires = DateTime.Now.AddDays(1);
                            response.Cookies.Add(cookie);
                        }
                        if (n.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnectRequestType.LogoutRequest)
                        {
                            var id_token_claim = n.OwinContext.Authentication.User.Claims.FirstOrDefault(x => x.Type == "id_token");
                            if (id_token_claim != null)
                            {
                                n.ProtocolMessage.IdTokenHint = id_token_claim.Value;
                            }
                        }
                        return Task.FromResult(0);
                    }
                }
            });

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
        }

    }

    public class CustomOpenIdConnectProtocolValidator : OpenIdConnectProtocolValidator
    {
        public CustomOpenIdConnectProtocolValidator(bool shouldValidateNonce)
        {
            this.ShouldValidateNonce = shouldValidateNonce;
        }
        protected override void ValidateNonce(JwtSecurityToken jwt, OpenIdConnectProtocolValidationContext validationContext)
        {
            if (this.ShouldValidateNonce)
            {
                base.ValidateNonce(jwt, validationContext);
            }
        }

        private bool ShouldValidateNonce { get; set; }
    }
}