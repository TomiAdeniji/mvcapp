using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.CoreCompat;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Qbicles.Models;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using System.Configuration;
using System.Web;
using Qbicles.BusinessRules.Provider;
using Qbicles.BusinessRules.Helper;

namespace Qbicles.Web
{
    public class EmailService : IIdentityMessageService
    {
        ApplicationDbContext dbContext = new ApplicationDbContext();
        public Task SendAsync(IdentityMessage message)
        {
            EmailRules er = new EmailRules(dbContext);
            var refModel = er.SendEmail(message);
            return Task.FromResult(refModel);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }



    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<BusinessRules.Model.ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = HelperClass.PasswordValidator();

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                      new DataProtectorTokenProvider<ApplicationUser>
                         (dataProtectionProvider.Create("ASP.NET Identity"))
                      {
                          TokenLifespan = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["TokenLifespan"]))                          
                      };
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
        /// <summary>
        /// Signin from Qbicle web
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="rememberMe"></param>
        /// <param name="shouldLockout"></param>
        /// <returns></returns>
        public override async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool rememberMe, bool shouldLockout)
        {
            string uri = ConfigManager.AuthHost;
            string clientId = "pos_user";
            //string scope = "web offline_access";
            string scope = "pos offline_access";
            var jwtProvider = JwtProvider.Create(uri);
            var stoken = await jwtProvider.GetTokenAsync(userName, password, clientId, scope);
            TokenResultDto tokenDto = null;
            if (!string.IsNullOrEmpty(stoken))
                tokenDto = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResultDto>(stoken);
            var validtoken = jwtProvider.ValidateToken(tokenDto?.access_token ?? "");
            if (tokenDto == null && !validtoken.IsTokenValid)
            {
                return SignInStatus.Failure;
            }
            else
            {
                //decode payload
                dynamic payload = jwtProvider.DecodePayload(stoken);
                //create an Identity Claim
                ClaimsIdentity claims = jwtProvider.CreateIdentity(true, userName, payload, tokenDto.access_token);

                //sign in
                var context = HttpContext.Current.Request.GetOwinContext();
                var authenticationManager = context.Authentication;
                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(tokenDto.expires_in) }, claims);

                return SignInStatus.Success;
            }
        }

        /// <summary>
        /// Signin from POS token
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<TokenResultDto> SignInAsync(string userName, string password)
        {
            string uri = ConfigManager.AuthHost;
            string clientId = "micro_app";
            string scope = "micro offline_access";
            var jwtProvider = JwtProvider.Create(uri);
            var stoken = await jwtProvider.GetTokenAsync(userName, password, clientId, scope);
            var tokenDto = new TokenResultDto();
            if (!string.IsNullOrEmpty(stoken))
                tokenDto = Newtonsoft.Json.JsonConvert.DeserializeObject<TokenResultDto>(stoken);
            var validtoken = jwtProvider.ValidateToken(tokenDto?.access_token ?? "");
            if (tokenDto == null && !validtoken.IsTokenValid)
            {
                tokenDto.status = SignInStatus.Failure;
                return tokenDto;
            }
            else
            {
                //decode payload
                dynamic payload = jwtProvider.DecodePayload(stoken);
                //create an Identity Claim
                ClaimsIdentity claims = jwtProvider.CreateIdentity(true, userName, payload, tokenDto.access_token);

                //sign in
                var context = HttpContext.Current.Request.GetOwinContext();
                var authenticationManager = context.Authentication;
                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(tokenDto.expires_in) }, claims);

                tokenDto.status = SignInStatus.Success;
                return tokenDto;
            }
        }
    }
}
