//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.Owin.Security;
//using Qbicles.BusinessRules;
//using Qbicles.BusinessRules.Helper;
//using Qbicles.TraderAPI.Models;

//namespace Qbicles.TraderAPI.Controllers
//{
//    public class AccountController : BaseController
//    {
//        //private readonly ApplicationDbContext _db = new ApplicationDbContext();
//        private ApplicationSignInManager _signInManager;
//        private ApplicationUserManager _userManager;

//        public AccountController()
//        {
//        }

//        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
//        {
//            UserManager = userManager;
//            SignInManager = signInManager;
//        }

//        public ApplicationSignInManager SignInManager
//        {
//            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
//            private set => _signInManager = value;
//        }

//        public ApplicationUserManager UserManager
//        {
//            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
//            private set => _userManager = value;
//        }


//        //
//        // GET: /Account/Login
//        [AllowAnonymous]
//        public ActionResult Login(string returnUrl)
//        {
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//            AuthenticationManager.SignOut();
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
//            ViewBag.ReturnUrl = returnUrl;
//            return View();
//        }

//        //
//        // POST: /Account/Login
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_8"));
//                return View(model);
//            }
//            var userLog = new UserRules(dbContext).GetUserByEmail(model.Email);

//            if (userLog != null)
//                model.Email = userLog.UserName;
//            // This doesn't count login failures towards account lockout
//            // To enable password failures to trigger account lockout, change to shouldLockout: true
//            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    var user = await UserManager.FindAsync(model.Email, model.Password);
//                    returnUrl = returnUrl ?? "/";
//                    if (user != null)
//                    {
//                        var identity = await UserManager.CreateIdentityAsync(
//                            user, DefaultAuthenticationTypes.ApplicationCookie);
//                    }

//                    return RedirectToLocal(returnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid login attempt. Email or Password incorrect!");
//                    return View(model);
//            }
//        }

//        //
//        // POST: /Account/LogOff
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult LogOff()
//        {
//            //return this.RedirectToLocal(null);
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
//            return RedirectToAction("Index", "Home");
//        }



//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (_userManager != null)
//                {
//                    _userManager.Dispose();
//                    _userManager = null;
//                }

//                if (_signInManager != null)
//                {
//                    _signInManager.Dispose();
//                    _signInManager = null;
//                }
//            }

//            base.Dispose(disposing);
//        }


//        #region Helpers

//        // Used for XSRF protection when adding external logins
//        private const string XsrfKey = "XsrfId";

//        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors) ModelState.AddModelError("", error);
//        }

//        private ActionResult RedirectToLocal(string returnUrl)
//        {
//            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
//            return RedirectToAction("Login", "Account");
//        }


//        internal class ChallengeResult : HttpUnauthorizedResult
//        {
//            public ChallengeResult(string provider, string redirectUri)
//                : this(provider, redirectUri, null)
//            {
//            }

//            public ChallengeResult(string provider, string redirectUri, string userId)
//            {
//                LoginProvider = provider;
//                RedirectUri = redirectUri;
//                UserId = userId;
//            }

//            public string LoginProvider { get; set; }
//            public string RedirectUri { get; set; }
//            public string UserId { get; set; }

//            public override void ExecuteResult(ControllerContext context)
//            {
//                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
//                if (UserId != null) properties.Dictionary[XsrfKey] = UserId;
//                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
//            }
//        }

//        #endregion
//    }
//}