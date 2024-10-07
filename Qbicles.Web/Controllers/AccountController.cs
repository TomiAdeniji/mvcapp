using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Qbicles;
using Qbicles.Web.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static Qbicles.Models.EmailLog;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        private void ClearAllCookies()
        {
            int limit = Request.Cookies.Count; //Get the number of cookies and 
                                               //use that as the limit.
            HttpCookie aCookie;   //Instantiate a cookie placeholder
            string cookieName;
            Session.Abandon();

            //Loop through the cookies
            for (int i = 0; i < limit; i++)
            {
                cookieName = Request.Cookies[i].Name;    //get the name of the current cookie
                if (cookieName == "__RequestVerificationToken")
                    continue;
                aCookie = new HttpCookie(cookieName)
                {
                    Value = "",    //set a blank value to the cookie 
                    Expires = DateTime.Now.AddDays(-2)    //Setting the expiration date
                };
                aCookie.SameSite = SameSiteMode.Strict;
                Response.Cookies.Add(aCookie);    //Set the cookie to delete it.
            }
        }

        //
        // GET: /Account/Login
        //[AllowAnonymous]
        //public ActionResult Login(string returnUrl)
        //{
        //    ClearAllCookies();
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //    AuthenticationManager.SignOut();
        //    if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("AntiForgeryException"))
        //        ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_ANTIFORGERYEXCEPTION"));
        //    else
        //        ViewBag.ReturnUrl = returnUrl;

        //    //return View();
        //    return Redirect("~");
        //}

        // POST: /Account/Login
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        //{
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //    AuthenticationManager.SignOut();
        //    if (!ModelState.IsValid)
        //    {
        //        ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_21"));
        //        return View(model);
        //    }
        //    SetCurrentQbicleIdCookies();
        //    SetCurrentDomainIdCookies();
        //    // This doesn't count login failures towards account lockout
        //    // To enable password failures to trigger account lockout, change to shouldLockout: true
        //    var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            var user = await UserManager.FindAsync(model.Email, model.Password);
        //            returnUrl = returnUrl ?? "/Qbicles";
        //            //if (user != null)
        //            //{
        //            //    var identity = await UserManager.CreateIdentityAsync(
        //            //        user, DefaultAuthenticationTypes.ApplicationCookie);
        //            //}
        //            //var authorize = new AuthorizeModel
        //            //{
        //            //    Email = model.Email,
        //            //    PasswordHash = user.PasswordHash.Encrypt()
        //            //};
        //            //SetQbiclesToken(QbiclesTokenHelper.Create(authorize));

        //            var oldCurrentDomainId = CurrentDomainId();
        //            Request.Cookies.Clear();
        //            SetTimeZoneCookies(user.Timezone);
        //            SetDateFormatCookies(user.DateFormat);
        //            SetTimeFormatCookies(user.TimeFormat);
        //            SetDateTimeFormatCookies((user.DateFormat + ' ' + user.TimeFormat).Trim());
        //            if (oldCurrentDomainId == -1)
        //            {
        //                SetCurrentDomainIdCookies(user.Domains.Count > 0
        //                    ? user.Domains.FirstOrDefault().Id
        //                    : 0);
        //            }
        //            var _profilePic = user.ProfilePic ?? $"{ConfigManager.DefaultUserUrlGuid}";
        //            SetProfilePicCookies(_profilePic);

        //            var logRule = new QbicleLogRules(dbContext);
        //            QbicleLog log = new QbicleLog(QbicleLogType.Login, user.Id);
        //            logRule.SaveQbicleLog(log);
        //            return RedirectToLocal("/");
        //        case SignInStatus.LockedOut:
        //            ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_22"));
        //            return View(model);
        //        case SignInStatus.RequiresVerification:
        //            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, model.RememberMe });
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_21"));
        //            return View(model);
        //    }
        //}

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync()) return View("Error");
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe,
                model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_23"));
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, false, false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    //  string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Register", "Account");
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null) return View("Error");
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmPassword(string userId, string source, string code)
        {
            if (userId == null || code == null) return View("Error");
            //var u = new ApplicationUser();

            var u = await UserManager.FindByIdAsync(userId);
            if (u == null)
                return View("Error");

            ViewBag.UserId = userId;
            ViewBag.Code = code;
            ViewBag.Email = u.Email;
            ViewBag.IsDriver = dbContext.Drivers.Any(d => d.User.User.Id == userId) && source == "Driver";
            ViewBag.Source = source;
            return View("ConfirmPassword");
        }

        //




        //
        // GET: /Account/ForgotPasswordConfirmation 
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> CofirmGuestPasswords(string Email, string Password)
        {
            if (!ModelState.IsValid) return RedirectToAction("Login", "Account");
            var user = await UserManager.FindByEmailAsync(Email);

            var result = await UserManager.ChangePasswordAsync(user.Id,
                ConfigurationManager.AppSettings["CreateUserPasswordDefault"], Password);
            if (result.Succeeded)
            {
                if (user != null) await SignInManager.SignInAsync(user, false, false);
                return new LargeJsonResult
                { Data = new { status = true }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            return Json(new
            {
                status = false,
                message = result.Errors.FirstOrDefault()
            }, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation", "Account");
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded) return RedirectToAction("ResetPasswordConfirmation", "Account");
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider,
                Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null) return View("Error");
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose })
                .ToList();
            return View(new SendCodeViewModel
            { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid) return View();

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider)) return View("Error");
            return RedirectToAction("VerifyCode",
                new { Provider = model.SelectedProvider, model.ReturnUrl, model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null) return RedirectToAction("Login");

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation",
                        new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Manage");

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null) return View("ExternalLoginFailure");
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, false, false);
                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            var log = new QbicleLog(QbicleLogType.Logout, CurrentUser().Id)
            {
                SessionId = HelperClass.GetCurrentSessionID(),
                IPAddress = HelperClass.GetIPAddress()
            };
            new QbicleLogRules(dbContext).SaveQbicleLog(log);

            ClearAllCookies();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, OpenIdConnectAuthenticationDefaults.AuthenticationType);
            return Redirect("~");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        [Authorize]
        public ActionResult CheckUserEmailInSystem(string userEmail)
        {
            var refModel = new ReturnJsonModel();

            try
            {
                var user = new UserRules(_db).GetUserByEmail(userEmail);
                if (user != null)
                {
                    refModel.msg = user.UserName;
                    refModel.result = true;
                }
                else
                {
                    refModel.msg = "";
                    refModel.result = false;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                refModel.result = false;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// Check exist info register
        /// </summary>
        /// <param name="userDisplayName">user Display Name</param>
        /// <param name="email">email</param>
        /// <param name="accountName">account name</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult DuplicateCheck(string userDisplayName, string email, string accountName)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel = new AccountRules(_db).DuplicateCheck(userDisplayName, email, accountName);
                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.Object = ex;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsDuplicateAccountName(string accountName)
        {
            return Json(new AccountRules(_db).IsDuplicateAccountName(accountName).ToString(),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeNameAccount(string accountName)
        {
            var accountId = CurrentDomain().Account.Id;
            return Json(new AccountRules(dbContext).ChangeNameAccount(accountId, accountName, CurrentUser().Id),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeToPackage(int idPackage)
        {
            var accountId = CurrentDomain().Account.Id;
            var changeByUserId = CurrentUser().Id;
            return Json(new AccountPackageRules(dbContext).ChangeToPackage(idPackage, accountId, changeByUserId),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveAccountAdmin(string userId)
        {
            var account = CurrentDomain().Account;
            return Json(new AccountRules(_db).RemoveAccountAdmin(userId, account, CurrentUser().Id),
                JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddAccountAdmin(string userId)
        {
            var account = CurrentDomain().Account;
            var refModal = new AccountRules(_db).AddAccountAdmin(userId, account, CurrentUser().Id);
            if (refModal.result)
                refModal.Object = HelperClass.ConvertUserToAdminViewModal((ApplicationUser)refModal.Object,
                    AdministratorViewModal.AccountAdministrators);
            return Json(refModal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetAsAccountOwner(string userId)
        {
            var account = CurrentDomain().Account;
            var refModal = new AccountRules(_db).SetAsAccountOwner(userId, account, CurrentUser().Id);

            if (refModal.result)
                refModal.Object = HelperClass.ConvertUserToAdminViewModal((ApplicationUser)refModal.Object,
                    AdministratorViewModal.AccountOwner);
            return Json(refModal, JsonRequestBehavior.AllowGet);
        }

        // POST: /Account/ForgotPassword
        public HttpStatusCodeResult BadGateway()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadGateway, "There is something wrong. please contact admin.");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(string email, string source = "")
        {
            if (ModelState.IsValid)
            {
                var exist_email = 0;
                var user = await UserManager.FindByEmailAsync(email);
                try
                {
                    if (user != null)
                    {

                        var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

                        var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                        var callbackUrl = Url.Action("ConfirmPassword", "Account", new { userId = user.Id, source, code }, Request.Url.Scheme);

                        new EmailRules(dbContext).SendEmailBody2VerifitaionCreNewAcLoginOrPwReset(email, "", callbackUrl, "_EmailPasswordReset.html", ReasonSent.ForgotPassword.GetDescription());

                    }

                    // email has in the system and email has not in the system
                    exist_email = 1;
                }
                catch (Exception ex)
                {
                    LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, user.Id);
                }

                return Json(exist_email, JsonRequestBehavior.AllowGet);
            }

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public async Task<ActionResult> ResetPasswords(string Email, string Password, string Code)
        {
            if (!ModelState.IsValid) return null;
            var user = await UserManager.FindByEmailAsync(Email);
            if (user == null)
                return Json(new
                {
                    status = false
                }, JsonRequestBehavior.AllowGet);
            var result = await UserManager.ResetPasswordAsync(user.Id, Code, Password);
            if (result.Succeeded)
                return Json(new
                {
                    status = true
                }, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                status = false,
                message = result.Errors.FirstOrDefault()
            }, JsonRequestBehavior.AllowGet);
        }

        #region Helpers


        public class JsonHttpStatusResult : JsonResult
        {
            private readonly HttpStatusCode _httpStatus;

            public JsonHttpStatusResult(object data, HttpStatusCode httpStatus)
            {
                Data = data;
                _httpStatus = httpStatus;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                context.RequestContext.HttpContext.Response.StatusCode = (int)_httpStatus;
                base.ExecuteResult(context);
            }
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Login", "Account");
        }


        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null) properties.Dictionary[XsrfKey] = UserId;
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion

        public ActionResult Waiting()
        {
            return View("Waiting");
        }


        #region registration account/ confirm account

        [AllowAnonymous]
        public async Task<ActionResult> QbiclesWelcome(string userId, string code, int activityId,
            QbicleActivity.ActivityTypeEnum type, string sendByEmail)
        {
            if (userId == null || code == null) return View("Error");
            ViewBag.UserId = userId;
            ViewBag.Code = code;
            ViewBag.activityId = activityId;
            ViewBag.type = type;
            ViewBag.sendByEmail = sendByEmail;
            ViewBag.InvitedBy = HelperClass.GetFullNameOfUser(new UserRules(dbContext).GetUserByEmail(sendByEmail));
            var user = new UserRules(_db).GetUser(userId, 0);
            ViewBag.Email = user.Email;
            if (type == QbicleActivity.ActivityTypeEnum.Domain)
            {
                ViewBag.QbicleName = "";
            }
            else if (type == QbicleActivity.ActivityTypeEnum.QbicleActivity)
            {
                ViewBag.QbicleName = new QbicleRules(dbContext).GetQbicleById(activityId)?.Name;
            }
            else
            {
                var activity = new QbicleRules(_db).GetActivity(activityId);
                ViewBag.QbicleName = activity?.Qbicle?.Name;
            }


            IdentityResult result;
            try
            {
                result = await UserManager.ConfirmEmailAsync(userId, code);
            }
            catch (InvalidOperationException ioe)
            {
                // ConfirmEmailAsync throws when the userId is not found.
                ViewBag.errorMessage = ioe.Message;
                return View("Error");
            }

            if (result.Succeeded)
            {
                ViewBag.errorMessage = "";
                ViewBag.invalidtoken = "";
            }
            else
            {
                ViewBag.errorMessage = result.Errors.FirstOrDefault();
            }

            return View("QbiclesWelcome");
        }

        /// <summary>
        /// Create an account from Login
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult CreateAccount()
        {
            var listAccount = new AccountPackageRules(_db).GetAllAccountPackage();
            return View(listAccount);
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveAccount(CreateAccountMain acc)
        {
            try
            {
                var user = new UserRules(_db).GetUserByEmail(acc.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        DisplayUserName = acc.UserName.Trim(),
                        UserName = acc.Email,
                        Email = acc.Email,
                        Forename = acc.Forename,
                        Surname = acc.Surname,
                        DateBecomesMember = DateTime.UtcNow,
                        ProfilePic = ConfigManager.DefaultUserUrlGuid,
                        IsEnabled = true,
                        Timezone = ConfigurationManager.AppSettings["Timezone"],
                        TimeFormat = "HH:mm",
                        DateFormat = "dd/MM/yyyy",
                        Profile = ""
                    };
                    //The name for the SubscriptionAccount is based on the UserName:<<UserName>>_Account
                    acc.AccountName = user.UserName + "_Account";
                    //The name of the QbicleDomain is simply to be the DispalyUserName exactly.
                    acc.Domain = user.DisplayUserName;
                    var valid = new AccountRules(_db).DuplicateCheck(user.DisplayUserName, acc.Email, acc.AccountName);
                    if (!valid.result)
                        return Json(valid, JsonRequestBehavior.AllowGet);
                    var result = await UserManager.CreateAsync(user, acc.Password);
                    if (result.Succeeded)
                    {
                        UserManager.AddToRole(user.Id, SystemRoles.DomainUser);
                        UserManager.Update(user);

                        var res = new UserRules(_db).SetNormalizedUserNameToEmail(user.Id);

                    }
                    else
                    {
                        AddErrors(result);
                        return Json(new ReturnJsonModel { result = false, msg = string.Join(", ", result.Errors) }, JsonRequestBehavior.AllowGet);
                    }
                }

                var rs = new AccountRules(_db).SaveAccount(acc, user.Id);
                if (rs)
                {
                    var loginUser = await UserManager.FindByNameAsync(user.UserName);
                    await SignInManager.SignInAsync(loginUser, false, false);

                    acc.Id = user.Id;

                    // Sort out the cookies
                    Request.Cookies.Clear();

                    SetUserSettingsCookie(user);

                    SetCurrentQbicleIdCookies();

                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                    AuthenticationManager.SignOut();
                    //var redirectUrl = ConfigurationManager.AppSettings["AuthHost"];
                    //return Redirect(redirectUrl.TrimEnd('/') + "/Account/Login");

                    //Send email verification
                    acc.RegistrationType = RegistrationType.Web;
                    SendVerificationEmail(acc);

                    EncryptCookie("newaccreg", acc.ToJson(), 1);

                    return Json(new ReturnJsonModel { result = true }, JsonRequestBehavior.AllowGet);

                    //return RedirectToAction("VerifyStep", "Account", new { verification = acc.ToJson().Encrypt() });
                }

                return Json(new ReturnJsonModel { result = false, msg = "Internal server error!" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Json(new ReturnJsonModel { result = false, msg = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }


        private bool SendVerificationEmail(CreateAccountMain acc)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var callbackUrl = "";
            var tokenCode = userManager.GenerateEmailConfirmationToken(acc.Id);
            var userVerification = new UserVerification
            {
                Code = tokenCode,
                Id = acc.Id,
                Email = acc.Email,
                Password = acc.Password,
                RegistrationType = acc.RegistrationType,
                ConnectCode = acc.ConnectCode
            };

            if (Request.Url != null)
            {
                callbackUrl = Url.Action("interstitial", "Account", new
                {
                    code = userVerification.ToJson().Encrypt()
                }, Request.Url.Scheme);
            }
            var pinVerification = AddUpdatePinVerification(userVerification, callbackUrl.Encrypt());

            return new EmailRules(dbContext).SendEmailBody2VerifitaionCreNewAcLoginOrPwReset(acc.Email, pinVerification, callbackUrl, "_EmailVerify.html", ResourcesManager._L("EMAIL_VERIFICATION_SUBJECT"));
        }


        /// <summary>
        /// Method use when create a new account from Web app
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult VerifyStep()
        {
            try
            {
                var verification = DecryptCookie("newaccreg");
                var acc = verification.ParseAs<CreateAccountMain>();
                if (acc == null || string.IsNullOrEmpty(acc.Id))
                    return View("ErrorAccessPage");
                var verify = new AccountRules(dbContext).VerifyStep(acc.Email);
                if (verify.actionVal == 0)
                    return View("ErrorAccessPage");
                if (verify.actionVal == 1)
                {
                    var redirectUrl = ConfigurationManager.AppSettings["AuthHost"];
                    return Redirect(redirectUrl.TrimEnd('/') + "/Account/Login");
                }
                ViewBag.UserVerification = verification.Encrypt();
                ViewBag.Email = acc.Email;
                ViewBag.UserId = acc.Id.Encrypt();

                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("ErrorAccessPage");
            }

        }
        [AllowAnonymous]
        public ActionResult VerifyStepInvalidEmailConfirm(string key)
        {
            try
            {
                var base64EncodedBytes = Convert.FromBase64String(key);
                key = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                var userId = dbContext.QbicleUser.FirstOrDefault(e => e.Email == key).Id;

                var pinVerify = dbContext.PinVerifications.FirstOrDefault(e => e.UserId == userId);



                var verification = pinVerify.UserVerification.Decrypt().ParseAs<UserVerification>();
                var acc = new CreateAccountMain
                {
                    //Code = verification.Code,
                    Id = verification.Id,
                    Email = verification.Email,
                    Password = verification.Password,
                    RegistrationType = verification.RegistrationType,
                    ConnectCode = verification.ConnectCode

                };
                //if (acc == null || string.IsNullOrEmpty(acc.Id.Encrypt()))
                //    return View("ErrorAccessPage");
                var verify = new AccountRules(dbContext).VerifyStep(acc.Email);
                if (verify.actionVal == 0)
                    return View("ErrorAccessPage");
                if (verify.actionVal == 1)
                {
                    var redirectUrl = ConfigurationManager.AppSettings["AuthHost"];
                    return Redirect(redirectUrl.TrimEnd('/') + "/Account/Login");
                }
                ViewBag.UserVerification = acc.ToJson().Encrypt();
                ViewBag.Email = acc.Email;
                ViewBag.UserId = acc.Id.Encrypt();
                EncryptCookie("newaccreg", acc.ToJson(), 1);
                return View("VerifyStep");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("ErrorAccessPage");
            }

        }
        [AllowAnonymous]
        public ActionResult ResendVerificationEmail(string verification)
        {
            try
            {
                SendVerificationEmail(verification.Decrypt().ParseAs<CreateAccountMain>());
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
        }


        [AllowAnonymous]
        public async Task<ActionResult> Interstitial(string code)
        {
            var obj = code.Decrypt().ParseAs<UserVerification>();
            var verify = new AccountRules(dbContext).VerifyStep(obj.Email);

            if (verify.actionVal == 0)//user is null
                return View("ErrorAccessPage");

            if (verify.actionVal == 1)// EmailConfirmed = true
            {
                var redirectUrl = ConfigurationManager.AppSettings["AuthHost"];
                return Redirect(redirectUrl.TrimEnd('/') + "/Account/Login");
            }

            IdentityResult result;
            try
            {
                result = await UserManager.ConfirmEmailAsync(obj.Id, obj.Code);
            }
            catch (InvalidOperationException ioe)
            {
                ViewBag.errorMessage = ioe.Message;
                return View("ErrorAccessPage");
            }

            if (result == null)
                return View("ErrorAccessPage");

            ViewBag.Verification = code;
            ViewBag.RegistrationType = obj.RegistrationType;
            if (result.Succeeded)
            {
                /*
                When a new user is added to the Qbicles system (i.e. they register) they are now required to do email verification. When this verification process is complete (and only then) 
                                check if the new QbicleUser’s email address corresponds to any existing TraderContact
                                if it does, then link each such TraderContact to the new QbicleUser
                */
                //Get token response to the Micro auto-login
                new TraderContactRules(dbContext).LinkContactsToUser(obj.Email);
                var token = ""; var refresh = "";
                if (obj.RegistrationType == RegistrationType.Micro)
                {
                    var signIn = await SignInManager.SignInAsync(obj.Email, obj.Password);
                    if (signIn.status == SignInStatus.Success)
                    {
                        token = signIn.access_token;
                        refresh = signIn.refresh_token;
                    }
                }

                //Connect from obj.Connect2Code to current user
                if (!string.IsNullOrEmpty(obj.ConnectCode))
                    new C2CRules(dbContext).ConnectC2C(obj.ConnectCode.Decrypt(), obj.Id, 2);

                ViewBag.Token = token;
                ViewBag.Refresh = refresh;
                return View("Interstitial");
            }
            else
                return View("Invalidtoken");


        }

        [AllowAnonymous]
        public async Task<ActionResult> GoToQbicle(string verification)
        {
            try
            {
                var obj = verification.Decrypt().ParseAs<UserVerification>();

                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                AuthenticationManager.SignOut();

                SetCurrentQbicleIdCookies();
                SetCurrentDomainIdCookies();
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var result = await SignInManager.PasswordSignInAsync(obj.Email, obj.Password, true, false);
                var returnUrl = "";
                switch (result)
                {
                    case SignInStatus.Success:
                        var user = await UserManager.FindAsync(obj.Email, obj.Password);

                        Request.Cookies.Clear();
                        SetDateFormatCookies(user.DateFormat);
                        SetTimeFormatCookies(user.TimeFormat);
                        SetDateTimeFormatCookies((user.DateFormat + ' ' + user.TimeFormat).Trim());

                        var _profilePic = user.ProfilePic ?? $"{ConfigManager.DefaultUserUrlGuid}";
                        SetProfilePicCookies(_profilePic);

                        var logRule = new QbicleLogRules(dbContext);
                        var log = new QbicleLog(QbicleLogType.Login, user.Id);
                        logRule.SaveQbicleLog(log);
                        return Json(new { result = true }, JsonRequestBehavior.AllowGet);
                    case SignInStatus.LockedOut:
                        ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_22"));
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", ResourcesManager._L("ERROR_MSG_21"));
                        return View("Error");
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("ErrorAccessPage");
            }

        }
        #endregion

        #region Micro api
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SendEmailConfirmationToken(ReturnJsonModel account)
        {
            var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var callbackUrl = "";
            var acc = account.Object.ToString().Decrypt().ParseAs<CreateAccountMain>();

            var tokenCode = userManager.GenerateEmailConfirmationToken(acc.Id);

            var userVerification = new UserVerification
            {
                Code = tokenCode,
                Id = acc.Id,
                Email = acc.Email,
                Password = acc.Password,
                RegistrationType = acc.RegistrationType,
                ConnectCode = acc.ConnectCode
            };


            var pinVerified = dbContext.PinVerifications.FirstOrDefault(e => e.UserId == acc.Id);
            if(pinVerified != null)
            {
                var user = pinVerified.UserVerification.Decrypt().ParseAs<UserVerification>();
                userVerification = new UserVerification
                {
                    Code = tokenCode,
                    Id = user.Id,
                    Email = user.Email,
                    Password = user.Password,
                    RegistrationType = user.RegistrationType,
                    ConnectCode = user.ConnectCode
                };
            }


            if (Request.Url != null)
            {
                callbackUrl = Url.Action("interstitial", "Account", new
                {
                    code = userVerification.ToJson().Encrypt()
                }, Request.Url.Scheme);
            }

            var pinVerification = AddUpdatePinVerification(userVerification, callbackUrl.Encrypt());

            var send = new EmailRules(dbContext).SendEmailBody2VerifitaionCreNewAcLoginOrPwReset(acc.Email, pinVerification, callbackUrl, "_EmailVerify.html", ResourcesManager._L("EMAIL_VERIFICATION_SUBJECT"));
            if (send)
                return Json(new { Status = 200 }, JsonRequestBehavior.AllowGet);
            return Json(new { Status = 500 }, JsonRequestBehavior.AllowGet);
        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult VerificationEmailResend(ReturnJsonModel verification)
        {
            try
            {
                SendVerificationEmail(verification.Object.ToString().Decrypt().ParseAs<CreateAccountMain>());
                return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        /// <summary>
        /// Return new PIN
        /// </summary>
        /// <param name="userVerification"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        private string AddUpdatePinVerification(UserVerification userVerification, string callbackUrl)
        {
            var pin = new Random().Next(1, 999999).ToString("D6");
            var pinVerify = dbContext.PinVerifications.FirstOrDefault(e => e.UserId == userVerification.Id);
            if (pinVerify == null)
            {
                pinVerify = new Qbicles.Models.Qbicles.PinVerification
                {
                    UserVerification = userVerification.ToJson().Encrypt(),
                    PIN = pin,
                    CallbackUrl = callbackUrl,
                    UserId = userVerification.Id
                };
                dbContext.PinVerifications.Add(pinVerify);
                dbContext.SaveChanges();
            }
            else
            {
                pinVerify.UserVerification = userVerification.ToJson().Encrypt();
                pinVerify.PIN = pin;
                pinVerify.CallbackUrl = callbackUrl;
                dbContext.SaveChanges();
            }

            return pin;
        }

        [AllowAnonymous]
        public ActionResult ProceedPINVerifyWeb(string pin, string userId)
        {
            userId = userId.Decrypt();
            var pinVerify = dbContext.PinVerifications.FirstOrDefault(e => e.UserId == userId && e.PIN == pin);

            if (pinVerify == null)
                return Json(new { result = false, msg = "PIN is invalid." }, JsonRequestBehavior.AllowGet);

            return Json(new ReturnJsonModel { result = true, msg = pinVerify.CallbackUrl.Decrypt() }, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public async Task<ActionResult> ProceedPINVerifyMicro(PinVerification pinVerification)
        {
            // UserVerification user id base 64 from micro
            var base64EncodedBytes = Convert.FromBase64String(pinVerification.UserVerification);
            var userId = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

            var pinVerify = dbContext.PinVerifications.FirstOrDefault(e => e.UserId == userId && e.PIN == pinVerification.PIN);

            if (pinVerify == null)
                return Json(new VerificationPinModel { Status = HttpStatusCode.Unauthorized, access_token = "", refresh_token = "", message = "PIN is invalid." }, JsonRequestBehavior.AllowGet);

            var userVerification = pinVerify.UserVerification.Decrypt().ParseAs<UserVerification>();

            var verify = new AccountRules(dbContext).VerifyStep(userVerification.Email);

            if (verify.actionVal == 0)//user is null
                return Json(new VerificationPinModel { Status = HttpStatusCode.NotFound, access_token = "", refresh_token = "", message = "User not found." }, JsonRequestBehavior.AllowGet);


            if (verify.actionVal == 1)// EmailConfirmed = true
                return Json(new VerificationPinModel { Status = HttpStatusCode.NotAcceptable, access_token = "", refresh_token = "", message = "PIN has verified." }, JsonRequestBehavior.AllowGet);


            IdentityResult result = null;
            try
            {
                result = await UserManager.ConfirmEmailAsync(userVerification.Id, userVerification.Code);
            }
            catch (InvalidOperationException ioe)
            {
                return Json(new VerificationPinModel { Status = HttpStatusCode.Unauthorized, access_token = "", refresh_token = "", message = $"Cannot verify PIN.{ioe.Message}" }, JsonRequestBehavior.AllowGet);

            }

            if (result == null)
                return Json(new VerificationPinModel { Status = HttpStatusCode.Unauthorized, access_token = "", refresh_token = "", message = $"Cannot verify PIN." }, JsonRequestBehavior.AllowGet);


            if (result.Succeeded)
            {
                //Get token response to the Micro auto-login
                new TraderContactRules(dbContext).LinkContactsToUser(userVerification.Email);
                var token = ""; var refresh = "";

                var signIn = await SignInManager.SignInAsync(userVerification.Email, userVerification.Password);
                if (signIn.status == SignInStatus.Success)
                {
                    token = signIn.access_token;
                    refresh = signIn.refresh_token;
                }

                //Connect from obj.Connect2Code to current user
                if (!string.IsNullOrEmpty(userVerification.ConnectCode))
                    new C2CRules(dbContext).ConnectC2C(userVerification.ConnectCode.Decrypt(), userVerification.Id, 2);

                return Json(new VerificationPinModel { Status = HttpStatusCode.OK, access_token = token, refresh_token = refresh, message = "" }, JsonRequestBehavior.AllowGet);

            }
            else
                return Json(new VerificationPinModel { Status = HttpStatusCode.Unauthorized, access_token = "", refresh_token = "", message = string.Join(";", result.Errors) }, JsonRequestBehavior.AllowGet);

        }
    }
}