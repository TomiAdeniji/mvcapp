﻿using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Qbicles.Models;
using System.Configuration;

namespace Qbicles.SignalR
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {

            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(BusinessRules.Model.ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            var cookieOptions = new CookieAuthenticationOptions();

            #region Sort out the cookie options
            //Get the value of the cookie domain setting from the web.config
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
            #endregion

            app.UseCookieAuthentication(cookieOptions);

            //app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            //app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "",
            //   appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }
    }
}