using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using static Qbicles.BusinessRules.Enums;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class CommonsController : BaseController
    {
        private ReturnJsonModel refModel;

        public ActionResult GetUserById(string id)
        {
            var rule = new UserRules(dbContext);
            var obj = rule.GetUserOnly(id);
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateModalPartial(string currentPage = "")
        {
            try
            {
                var user = new UserRules(dbContext).GetUser(CurrentUser().Id, 0);
                ViewBag.CurrentUser = user;

                // get data to modal add new aleart
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                // end alert
                ViewBag.listUserByQbicle = new UserRules(dbContext).GetListUserIdByQbicle(CurrentQbicleId());

                // get data to modal add new event
                var currentCube = new Qbicle();
                if (CurrentQbicleId() <= 0)
                {
                    ViewBag.UserCurrentQbicleAssing = new List<ApplicationUser>();
                }
                else
                {
                    currentCube = new QbicleRules(dbContext).GetQbicleById(CurrentQbicleId()) ?? new Qbicle();
                }

                ViewBag.UserCurrentQbicleAssing = currentCube.Members.ToList(); //The users displayed are the users associated with the current Qbicle.
                // end event
                //get data to modal add new Task
                ViewBag.taskPrioritys = EnumModel.GetEnumValuesAndDescriptions<QbicleTask.TaskPriorityEnum>();
                ViewBag.taskRepeats = EnumModel.GetEnumValuesAndDescriptions<QbicleTask.TaskRepeatEnum>();
                /*end get task*/
                //get data to modal add new discussion

                ViewBag.UserCurrentQbicleDiscussion = currentCube.Members.ToList();
                ViewBag.Domains = user.Domains;

                //get user not in a dicussion
                if (currentPage != "Discussion")
                {
                    ViewBag.UsersDomainNotInDiscussion = new List<ApplicationUser>();
                    return PartialView("_ModalPartial");
                }

                // if current page as Discussion, then get uses list in the Domain and not in discussion member and guest
                if (currentCube.Domain != null)
                {
                    var usersDomain = currentCube.Domain.Users;
                    var discussion = new DiscussionsRules(dbContext).GetDiscussionById(CurrentDiscussionId());
                    if (discussion == null)
                    {
                        ViewBag.UsersDomainNotInDiscussion = new List<ApplicationUser>();
                    }
                    else
                    {
                        var userNotIn = usersDomain.Except(discussion.ActivityMembers);
                        ViewBag.UsersDomainNotInDiscussion = userNotIn.ToList();
                    }
                }

                return PartialView("_ModalPartial");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        [HttpPost]
        public ActionResult BindingQbicleParameter(string key, QbicleModule moduleSelected)
        {
            if (!moduleSelected.TryParseEnum<QbicleModule>())
                throw new InvalidEnumArgumentException(nameof(moduleSelected), (int)moduleSelected,
                    typeof(QbicleModule));
            try
            {
                refModel = new ReturnJsonModel { result = false };
                var qbicleId = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                if (qbicleId != 0)
                {
                    if (qbicleId > 0)
                        try
                        {
                            var qRule = new QbicleRules(dbContext);
                            var cube = qRule.UpdateCurrentQbicle(qbicleId, User.Identity.GetUserId());
                            ValidateCurrentDomain(cube.Domain, cube.Id);
                        }
                        catch (Exception ex)
                        {
                            refModel.Object = ex;
                        }
                    SetModuleSelectedCookies((int)moduleSelected);

                    refModel.result = true;
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult SaveCookie(string key, string value, int expireTime)
        {
            var result = new ReturnJsonModel();
            //Security updates for cookie creation and cookie retrieval
            string[] keyAllows = new string[] { "CurrentLocationManage", "Qbicle-topic", "headercol", "cb_config_tab", "PreviousPageOfMediaFolder", "topic_stream" };
            if (keyAllows.Contains(key))
            {
                EncryptCookie(key, value, expireTime);
            }
            else
            {
                result.msg = "Access denied.";
                result.Object = "";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetCookie(string key)
        {
            var result = new ReturnJsonModel();
            //Security updates for cookie creation and cookie retrieval
            string[] keyAllows = new string[] { "CurrentLocationManage", "Qbicle-topic", "headercol", "cb_config_tab", "PreviousPageOfMediaFolder", "topic_stream" };
            if (keyAllows.Contains(key) || key.Contains("Qbicle-"))
            {
                result.Object = DecryptCookie(key);
            }
            else
            {
                result.msg = "Access denied.";
                result.Object = "";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult RemoveCookie(string key)
        {
            var result = new ReturnJsonModel();
            if (Request.Cookies[key] != null)
            {
                HttpContext.Response.Cookies[key].Expires = DateTime.Now.AddDays(-1);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Bind list select Domain and active current Domain
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserDomainChangeActive()
        {
            refModel = new ReturnJsonModel();
            try
            {
                var currentDomainId = CurrentDomainId();
                var currentUserId = CurrentUser().Id;
                var user = dbContext.QbicleUser.AsNoTracking().Any(u => u.Id == currentUserId && u.Domains.Any(d => d.Id == currentDomainId));
                if (user)
                    refModel.msgId = currentDomainId.ToString();

                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                refModel.result = false;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Update CurrentDomain when user selected change Domain
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateCurrentDomain(string currentDomainKey)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (currentDomainKey.IsNullOrEmpty())
                {
                    currentDomainKey = " ";
                }
                var currentDomainId = Int32.Parse(EncryptionService.Decrypt(currentDomainKey));
                if (currentDomainId != CurrentDomainId())
                {
                    refModel.result = true;
                    SetCurrentDomainIdCookies(currentDomainId);
                    SetCurrentQbicleIdCookies();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, currentDomainKey);
                refModel.Object = ex;
            }

            return Json(refModel);
        }
    }

    public class PermanentRedirectResult : ActionResult
    {
        public string Url { get; private set; }

        public PermanentRedirectResult(string url)
        {
            Url = url;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = 401;
            response.Status = "301 Moved Permanently";
            response.RedirectLocation = Url;
            response.End();
        }
    }

    [Authorize]
    public class BaseController : Controller
    {
        public ApplicationDbContext dbContext = new ApplicationDbContext();

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var authCookie = Request.Cookies["QbiclesAuth"];
                if (authCookie != null)
                {
                    var systemAuthCookie = new Cookie("QbiclesAuth", authCookie.Value);
                    if (ConfigurationManager.AppSettings["CookieDomain"] != null)
                    {
                        var cookieDomain = ConfigurationManager.AppSettings["CookieDomain"];
                        systemAuthCookie.Domain = cookieDomain;
                    }
                    else
                    {
                        if (Request.Url != null) systemAuthCookie.Domain = Request.Url.Host;
                    }

                    var sess = HttpContext.Session;
                    sess.Add("AuthCookie", systemAuthCookie);
                }

                if (CurrentDomainId() > 0)
                {
                    var currentDomain = dbContext.Domains.Find(CurrentDomainId());
                    var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomain.Id && p.IsArchived == false);
                    ViewBag.CurrentDomain = currentDomain;
                    ViewBag.CurrencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(currentDomain.Id);
                    ViewBag.CurrentDomainPlan = currentDomainPlan;
                }
                else
                {
                    ViewBag.CurrentDomain = new QbicleDomain
                    {
                        Name = "Guest Qbicles",
                        Id = -1
                    };
                    ViewBag.CurrencySettings = new CurrencySetting
                    {
                        SymbolDisplay = CurrencySetting.SymbolDisplayEnum.Prefixed,
                        DecimalPlace = CurrencySetting.DecimalPlaceEnum.Three,
                        CurrencySymbol = "₦"
                    };
                }
                var user = new UserRules(dbContext).GetUserById(CurrentUserIdIdentity());
                var userIdentity = CurrentUserIdIdentity();
                var userLogged = GetLoggedId();
                if (userIdentity != userLogged)
                {
                    SetUserSettingsCookie(user);
                    UpdateLoggedId(userIdentity);
                }

                var userSetting = CurrentUser();

                ViewBag.RightApps = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
                ViewBag.SystemRolesQBicles = new AppRightRules(dbContext).SystemRolesQBicles(userSetting.Id);
                ViewBag.IsUsingApprovals = new ApprovalAppsRules(dbContext).UserAsInitiators(userSetting.Id, CurrentDomainId());
				ViewBag.DefaultLocation = new TraderLocationRules(dbContext).GetTraderLocationDefault(CurrentDomainId());


				ViewBag.CurrentDomainId = CurrentDomainId();
                ViewBag.CurrentQbicleId = CurrentQbicleId();
                ViewBag.CurrentUserId = userSetting.Id;
                ViewBag.CurrentTimeZone = userSetting.Timezone;
                ViewBag.CurrentDateFormat = userSetting.DateFormat;
                ViewBag.CurrentTimeFormat = userSetting.TimeFormat;
                ViewBag.CurrentDateTimeFormat = userSetting.DateTimeFormat;
                ViewBag.CurrentUserAvatar = ProfilePicCookies();
                ViewBag.IsSysAdmin = userSetting.IsSysAdmin;
                ViewBag.CurrentLocationManage = CurrentLocationManage();
                ViewBag.DocRetrievalUrl = ConfigManager.ApiGetDocumentUri;
                ViewBag.VideoRetrievalUrl = ConfigManager.ApiGetVideoUri;
                ViewBag.VideoRetrievalScreenshotUrl = ConfigManager.ApiGetVideoScreenshot;
                ViewBag.Today = DateTime.UtcNow;
                ViewBag.CurrentUser = user;
                ViewBag.S3BucketName = ConfigManager.BucketName;
                ViewBag.S3BucketRegion = ConfigManager.BucketRegion;
                ViewBag.S3IdentityPoolId = ConfigManager.IdentityPoolId;

                var imgListFileType = new FileTypeRules(dbContext).GetExtensionsByType("Image File");
                ViewBag.ImageAcceptedExtensions = imgListFileType.Count() > 0 ? ("." + string.Join(",.", imgListFileType)) : "";

                ViewBag.AlertNotification = AlertNotificationCookieGet();
            }

            //base.OnActionExecuting(filterContext);
        }

        //protected override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    var currentDomain = dbContext.Domains.Find(CurrentDomainId());
        //    var cUser = new UserRules(dbContext).GetUser(_CurrentUserId(), 0);
        //    #region Check Access Permission
        //    var isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();
        //    if (!isAjaxRequest && cUser != null && cUser.IsEnabled.HasValue && !cUser.IsEnabled.Value)
        //    {
        //        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        //        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //        AuthenticationManager.SignOut();
        //    }
        //    else if (!isAjaxRequest && cUser != null && currentDomain != null && !CheckingDomainAccessPermission(cUser.Id, currentDomain))
        //    {
        //        SetCurrentDomainIdCookies();
        //        SetCurrentQbicleIdCookies();
        //        filterContext.Result = new RedirectResult("/");
        //    }
        //    else if (isAjaxRequest && cUser != null && cUser.IsEnabled.HasValue && !cUser.IsEnabled.Value)
        //    {
        //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Domain" }, { "action", "AccessUserDenied" } });
        //    }
        //    else if (isAjaxRequest && cUser != null && currentDomain != null && !CheckingDomainAccessPermission(cUser.Id, currentDomain))
        //    {
        //        SetCurrentDomainIdCookies();
        //        SetCurrentQbicleIdCookies();
        //        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Domain" }, { "action", "AccessDomainDenied" } });
        //    }
        //    #endregion
        //}

        [NonAction]
        private string CurrentUserIdIdentity()
        {
            var userId = "";
            var identity = (ClaimsIdentity)User.Identity;
            if (identity.IsAuthenticated)
            {
                //IEnumerable<Claim> claims = identity.Claims;
                try
                {
                    userId = identity.Claims.FirstOrDefault(s => s.Type == ClaimTypes.NameIdentifier)?.Value ?? "";
                }
                catch
                {
                    userId = "";
                }
            }

            return userId;
        }

        [NonAction]
        private void UpdateLoggedId(string id)
        {
            EncryptCookie("loggedId", id, 1);
        }

        [NonAction]
        public string GetLoggedId()
        {
            return DecryptCookie("loggedId");
        }

        [NonAction]
        public bool SystemRoleValidation(string userId, string roleName)
        {
            return new QbicleRules(dbContext).SystemRoleValidation(userId, roleName);
        }

        [NonAction]
        public Cookie AuthCookie()
        {
            return (Cookie)System.Web.HttpContext.Current.Session["AuthCookie"];
        }

        [NonAction]
        public void SetUserSettingsCookie(ApplicationUser user)
        {
            var userSetting = new UserSetting();
            if (user != null)
                userSetting = new UserSetting
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    DisplayName = user.GetFullName(),
                    ProfilePic = user.ProfilePic ?? ConfigManager.DefaultUserUrlGuid,
                    DateFormat = (string.IsNullOrEmpty(user.DateFormat) || (user.DateFormat != "dd/MM/yyyy" && user.DateFormat != "MM/dd/yyyy")) ? "dd/MM/yyyy" : user.DateFormat,
                    TimeFormat = string.IsNullOrEmpty(user.TimeFormat) ? "HH:mm" : user.TimeFormat,
                    Timezone = user.Timezone ?? ConfigurationManager.AppSettings["Timezone"],
                    IsSysAdmin = user.IsSystemAdmin
                };

            EncryptCookie("userSettingsCookie", userSetting.ToJson(), 1);
        }

        [NonAction]
        public UserSetting CurrentUser()
        {
            return DecryptCookie("userSettingsCookie").ParseAs<UserSetting>();
        }

        /// <summary>
        ///     Generate Url Token
        /// </summary>
        /// <param name="tokenToUserId">gen token to user Id</param>
        /// <param name="activityId"></param>
        /// <param name="type"></param>
        /// <param name="sendByEmail"></param>
        /// <returns></returns>
        [NonAction]
        public string GenerateUrlToken(string tokenToUserId, int activityId, QbicleActivity.ActivityTypeEnum type, string sendByEmail)
        {
            try
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                //var user = userManager.FindById(tokenToUserId);
                var code = userManager.GenerateEmailConfirmationToken(tokenToUserId);
                if (Request.Url != null)
                {
                    var callbackUrl = Url.Action("QbiclesWelcome", "Account", new
                    {
                        userId = tokenToUserId,
                        code,
                        activityId,
                        type,
                        sendByEmail
                    }, Request.Url.Scheme);
                    return callbackUrl;
                }

                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return "";
            }
        }

        [NonAction]
        public string GenerateCallbackUrl()
        {
            try
            {
                if (Request.Url != null)
                    return $"{ConfigManager.AuthHost}/Account/Login";

                return "";
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return "";
            }
        }

        /// <summary>
        ///     user reset password in application
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userPasswordChange"></param>
        /// <returns></returns>
        [NonAction]
        public bool ResetPassProfile(string userId, string userPasswordChange)
        {
            try
            {
                var userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var token = userManager.GeneratePasswordResetToken(userId);
                var result = userManager.ResetPassword(userId, token, userPasswordChange);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return false;
            }
        }

        [NonAction]
        public List<string> ApplicationUserRoleRights(string appName)
        {
            var rights = new List<string>();
            var roleRights = new List<RoleRightAppXref>();
            var currentDomainId = CurrentDomainId();
            var domainRoles = new UserRules(dbContext).GetUser(CurrentUser().Id, 0).DomainRoles.Where(d => d.Domain.Id == currentDomainId);
            foreach (var role in domainRoles)
                roleRights.AddRange(dbContext.RoleRightAppXref
                    .Where(d => d.Role.Id == role.Id && d.AppInstance.QbicleApplication.Name == appName).ToList());
            foreach (var right in roleRights) rights.Add(right.Right.Name);
            return rights.Distinct().ToList();
        }

        [NonAction]
        public void SetCurrentTopicIdCookies(int currentTopicId = 0)
        {
            EncryptCookie("CurrentTopicId", currentTopicId.ToString(), 1);
        }

        [NonAction]
        public int CurrentTopicId()
        {
            var CurrentTopicId = DecryptCookie("CurrentTopicId");
            return CurrentTopicId == null
                ? 0
                : Converter.Obj2Int(CurrentTopicId);
        }

        [NonAction]
        public AlertNotificationModel AlertNotificationCookieGet()
        {
            try
            {
                return DecryptCookie("AlertNotificationModel").ParseAs<AlertNotificationModel>();
            }
            catch
            {
                return new AlertNotificationModel();
            }
        }

        [NonAction]
        public void AlertNotificationCookieSet(AlertNotificationModel alertNotification)
        {
            EncryptCookie("AlertNotificationModel", alertNotification.ToJson(), 1);
        }

        public class LargeJsonResult : JsonResult
        {
            public LargeJsonResult()
            {
                MaxJsonLength = int.MaxValue;
            }
        }

        #region current activity id

        [NonAction]
        public int CurrentDiscussionId()
        {
            var CurrentDiscussionId = DecryptCookie("CurrentDiscussionId");
            if (CurrentDiscussionId == null)
                return 0;
            return Converter.Obj2Int(CurrentDiscussionId);
        }

        [NonAction]
        public void SetModuleSelectedCookies(int moduleSelected = 0)
        {
            EncryptCookie("ModuleSelected", moduleSelected.ToString(), 1);
        }

        [NonAction]
        public void SetCurrentQbicleIdCookies(int qbicleId = 0)
        {
            if (qbicleId != CurrentQbicleId())
            {
                ClearAllCurrentActivities();
                EncryptCookie("CurrentQbicleId", qbicleId.ToString(), 1);
            }
        }

        [NonAction]
        public void SetCurrentDomainIdCookies(int currentDomainId = 0)
        {
            //Check if the Domain is changing
            //If it is, then reset the Location
            if (currentDomainId != CurrentDomainId())
                SetCurrentLocationManage();

            EncryptCookie("CurrentDomainId", currentDomainId.ToString(), 1);
        }

        [NonAction]
        public void SetCurrentDomainIdCookies(string currentDomainKey)
        {
            //Check if the Domain is changing
            //If it is, then reset the Location
            var currentDomainId = Int32.Parse(EncryptionService.Decrypt(currentDomainKey));
            if (currentDomainId != CurrentDomainId())
                SetCurrentLocationManage();
            EncryptCookie("CurrentDomainId", currentDomainId.ToString(), 1);
        }

        [NonAction]
        public void ClearAllCurrentActivities()
        {
            SetCurrentTaskIdCookies();
            SetCurrentMediaIdCookies();
            SetCurrentEventIdCookies();
            SetCurrentAlertIdCookies();
            SetCurrentApprovalIdCookies();
            SetCurrentJournalEntryIdCookies();
            SetCurrentLinkIdCookies();
            SetCurrentDiscussionIdCookies();
            SetCurrentB2COrderIdCookies();
        }

        [NonAction]
        public void SetCurrentDiscussionIdCookies(int currentDiscussionId = 0)
        {
            EncryptCookie("CurrentDiscussionId", currentDiscussionId.ToString(), 1);
        }

        [NonAction]
        public void SetCurrentB2COrderIdCookies(int tradeOrderId = 0)
        {
            EncryptCookie("CurrentB2COrderId", tradeOrderId.ToString(), 1);
        }

        [NonAction]
        public int CurrentB2COrderIdCookies()
        {
            var tradeOrderId = DecryptCookie("CurrentB2COrderId");
            if (tradeOrderId == null)
                return 0;
            return Converter.Obj2Int(tradeOrderId);
        }

        [NonAction]
        public int CurrentLocationManage()
        {
            var currentLocationManage = DecryptCookie("CurrentLocationManage");
            if (currentLocationManage == null)
                return 0;
            return Converter.Obj2Int(currentLocationManage);
        }

        [NonAction]
        public void SetCurrentLocationManage(int id = 0)
        {
            CacheHelper.Delete($"itemUnitsChanged-{CurrentLocationManage()}");
            EncryptCookie("CurrentLocationManage", id.ToString(), 1);
        }

        [NonAction]
        public int CurrentDomainId()
        {
            var currentDomainId = DecryptCookie("CurrentDomainId");
            if (currentDomainId == null)
                return 0;
            return Converter.Obj2Int(currentDomainId);
        }

        [NonAction]
        public int CurrentQbicleId()
        {
            var currentQbicleId = DecryptCookie("CurrentQbicleId");
            if (currentQbicleId == null) return 0;

            var qbicleId = Converter.Obj2Int(currentQbicleId);
            var cubeId = CheckingQbicleAccessPermission(User.Identity.GetUserId(), qbicleId);
            return cubeId;
        }

        [NonAction]
        public QbicleDomain CurrentDomain()
        {
            return dbContext.Domains.Find(CurrentDomainId());
        }

        [ValidateAntiForgeryToken]
        public ActionResult Unauthorised()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        [NonAction]
        public int CurrentJournalEntryId()
        {
            var currentJournalEntryId = DecryptCookie("CurrentJournalEntryId");
            if (currentJournalEntryId == null)
                return 0;
            return Converter.Obj2Int(currentJournalEntryId);
        }

        [NonAction]
        public void SetCurrentJournalEntryIdCookies(int currentJournalEntryId = 0)
        {
            EncryptCookie("CurrentJournalEntryId", currentJournalEntryId.ToString(), 1);
        }

        [NonAction]
        public int CurrentTaskId()
        {
            var currentTaskId = DecryptCookie("CurrentTaskId");
            return currentTaskId == null ? 0 : Converter.Obj2Int(currentTaskId);
        }

        [NonAction]
        public string ProfilePicCookies()
        {
            return CurrentUser().ProfilePic;
        }

        [NonAction]
        public void SetCurrentTaskIdCookies(int currentTaskId = 0)
        {
            EncryptCookie("CurrentTaskId", currentTaskId.ToString(), 1);
        }

        [NonAction]
        public int CurrentMediaId()
        {
            var currentMediaId = DecryptCookie("CurrentMediaId");
            if (currentMediaId == null)
                return 0;
            return Converter.Obj2Int(currentMediaId);
        }

        [NonAction]
        public void SetCurrentMediaIdCookies(int currentMediaId = 0)
        {
            EncryptCookie("CurrentMediaId", currentMediaId.ToString(), 1);
        }

        [NonAction]
        public int GetRefreshPageTime()
        {
            var currentRefreshPageTime = DecryptCookie("PageRefreshTime");
            if (currentRefreshPageTime == null)
                return 20;
            return Converter.Obj2Int(currentRefreshPageTime);
        }

        [NonAction]
        public void SetPageRefreshTime(int refreshTime)
        {
            EncryptCookie("PageRefreshTime", refreshTime.ToString(), 1);
        }

        [NonAction]
        public int CurrentEventId()
        {
            var currentEventId = DecryptCookie("CurrentEventId");
            if (currentEventId == null)
                return 0;
            return Converter.Obj2Int(currentEventId);
        }

        [NonAction]
        public void SetCurrentEventIdCookies(int currentEventId = 0)
        {
            EncryptCookie("CurrentEventId", currentEventId.ToString(), 1);
        }

        [NonAction]
        public int CurrentLinkId()
        {
            var currentLinkId = DecryptCookie("CurrentLinkId");
            if (currentLinkId == null)
                return 0;
            return Converter.Obj2Int(currentLinkId);
        }

        [NonAction]
        public void SetCurrentLinkIdCookies(int currentLinkId = 0)
        {
            EncryptCookie("CurrentLinkId", currentLinkId.ToString(), 1);
        }

        [NonAction]
        public int CurrentAlertId()
        {
            var CurrentAlertId = DecryptCookie("CurrentAlertId");
            if (CurrentAlertId == null)
                return 0;
            return Converter.Obj2Int(CurrentAlertId);
        }

        [NonAction]
        public void SetCurrentAlertIdCookies(int currentAlertId = 0)
        {
            EncryptCookie("CurrentAlertId", currentAlertId.ToString(), 1);
        }

        [NonAction]
        public int CurrentApprovalId()
        {
            var currentApprovalId = DecryptCookie("CurrentApprovalId");
            if (currentApprovalId == null)
                return 0;
            return Converter.Obj2Int(currentApprovalId);
        }

        [NonAction]
        public void SetCurrentApprovalIdCookies(int currentApprovalId = 0)
        {
            EncryptCookie("CurrentApprovalId", currentApprovalId.ToString(), 1);
        }

        [NonAction]
        public string CurrentDateRange()
        {
            var currentDateRange = DecryptCookie("CurrentDateRange");
            if (currentDateRange == null)
                return "";
            return currentDateRange;
        }

        [NonAction]
        public void SetCurrentCurrentDateRangeCookies(string dateRange = "")
        {
            EncryptCookie("CurrentDateRange", dateRange, 1);
        }

        [NonAction]
        public int CurrentCommunityPageId()
        {
            var currentCommunityPageId = DecryptCookie("CurrentCommunityPageId");
            if (currentCommunityPageId == null)
                return 0;
            return Converter.Obj2Int(currentCommunityPageId);
        }

        [NonAction]
        public void SetCCommunityPageIdCookies(int communityPageId = 0)
        {
            EncryptCookie("CurrentCommunityPageId", communityPageId.ToString(), 1);
        }

        [NonAction]
        public string UserProfilePageId()
        {
            var userProfilePageId = DecryptCookie("UserProfilePageId");
            if (userProfilePageId == null)
                return "";
            return userProfilePageId;
        }

        [NonAction]
        public string EmailPreview()
        {
            var emailPreview = DecryptCookie("EmailPreview");
            if (emailPreview == null)
                return "";
            return emailPreview;
        }

        [NonAction]
        public void SetUserProfilePageId(string id = "")
        {
            EncryptCookie("UserProfilePageId", id.ToString(), 1);
        }

        [NonAction]
        public void SetProfilePicCookies(string profilePic)
        {
            EncryptCookie("ProfilePic", profilePic.ToString(), 1);
        }

        [NonAction]
        public void SetDateFormatCookies(string dateFormat)
        {
            if (string.IsNullOrEmpty(dateFormat))
                dateFormat = "dd/MM/yyyy";
            EncryptCookie("dateFormatCookie", dateFormat, 1);
        }

        [NonAction]
        public void SetTimeFormatCookies(string timeFormat)
        {
            if (string.IsNullOrEmpty(timeFormat))
                timeFormat = "HH:mm";
            EncryptCookie("timeFormatCookie", timeFormat, 1);
        }

        [NonAction]
        public void SetDateTimeFormatCookies(string dateTimeFormat)
        {
            if (string.IsNullOrEmpty(dateTimeFormat))
                dateTimeFormat = "dd/MM/yyyy HH:mm";
            EncryptCookie("dateTimeFormatCookie", dateTimeFormat, 1);
        }

        [NonAction]
        public string CurrentGoBackPage()
        {
            var goBackPage = DecryptCookie("goBackPage");
            if (goBackPage == null)
                return "";
            return goBackPage;
        }

        [NonAction]
        public void SetCookieGoBackPage(string goBack = "")
        {
            EncryptCookie("goBackPage", goBack, 1);
        }

        /// <summary>
        /// This property should have the default value of false,
        /// If the creator of the Activity or Post is the Customer in a B2CQBicle or B2COrder then this value must be set to true at the time the Activity/post is created
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public bool IsCreatorTheCustomer()
        {
            var isCreatorTheCustomer = DecryptCookie("isCreatorTheCustomer");
            if (isCreatorTheCustomer == SystemPageConst.C2C)// || isCreatorTheCustomer.Contains($"/{SystemPageConst.C2C}#comms-activities") || isCreatorTheCustomer.Contains($"/{SystemPageConst.C2C}"))
                return true;
            return false;
        }

        [NonAction]
        public string GetCreatorTheCustomer()
        {
            return DecryptCookie("isCreatorTheCustomer");
        }

        [NonAction]
        public void SetCreatorTheCustomer(string isCreatorTheCustomer = "")
        {
            EncryptCookie("isCreatorTheCustomer", isCreatorTheCustomer, 1);
        }

        [NonAction]
        public void SetCookieGoBackSubActivity(string goBack = "")
        {
            EncryptCookie("goBackSubActivityPage", goBack, 1);
        }

        [NonAction]
        public string CurrentPage()
        {
            var currentPage = DecryptCookie("CurrentPage");
            if (currentPage == null)
                return "Domain";
            return currentPage;
        }

        [NonAction]
        public void SetCurrentPage(string currentPage)
        {
            EncryptCookie("CurrentPage", currentPage, 1);
        }

        [NonAction]
        public void SetGoBackForMediaFolder(string gobackurl)
        {
            EncryptCookie("MediaFolderSelected", gobackurl, 1);
        }

        [NonAction]
        public void SetPreviewEmailCookie(string preview)
        {
            EncryptCookie("EmailPreview", preview, 1);
        }

        [NonAction]
        public void ValidateCurrentDomain(QbicleDomain domainValidate, int qbicleId)
        {
            try
            {
                if (domainValidate != null && domainValidate.Name != SystemDomainConst.BUSINESS2BUSINESS && domainValidate.Name != SystemDomainConst.BUSINESS2CUSTOMER && domainValidate.Name != SystemDomainConst.CUSTOMER2CUSTOMER)
                {
                    if (domainValidate.Id != CurrentDomainId())
                    {
                        ViewBag.CurrentDomainId = domainValidate.Id;
                        SetCurrentDomainIdCookies(domainValidate.Id);
                    }
                    SetCurrentQbicleIdCookies(qbicleId);
                }
                else
                    SetCurrentQbicleIdCookies(qbicleId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, domainValidate, qbicleId);
            }
        }

        #endregion current activity id

        public ActionResult LogAccess(string type)
        {
            var refModel = new ReturnJsonModel() { result = true };
            var userId = CurrentUser().Id;
            try
            {
                AppAccessLog log = null;
                var logRule = new QbicleLogRules(dbContext);
                switch (type)
                {
                    case "CleanBooks":
                        log = new AppAccessLog(userId, AppType.Cleanbooks, CurrentDomainId());
                        break;

                    case "Trader":
                        log = new AppAccessLog(userId, AppType.Trader, CurrentDomainId());
                        break;

                    case "Sales & Marketing":
                        log = new AppAccessLog(userId, AppType.SalesMarketing, CurrentDomainId());
                        break;

                    case "Bookkeeping":
                        log = new AppAccessLog(userId, AppType.Bookkeeping, CurrentDomainId());
                        break;

                    case "Spannered":
                        log = new AppAccessLog(userId, AppType.Spannered, CurrentDomainId());
                        break;
                }

                if (log != null) logRule.SaveAppAccessLog(log);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId);
                refModel.result = false;
                refModel.msg = ex.Message;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        [NonAction]
        public void EncryptCookie(string nameOfCookie, string valueOfCookie, int expiredTime)
        {
            var cookieObject = new HttpCookie(nameOfCookie, valueOfCookie.Encrypt())
            {
                SameSite = SameSiteMode.Strict
            };
            cookieObject.Expires.AddDays(expiredTime);

            HttpContext.Response.Cookies.Add(cookieObject);
        }

        [NonAction]
        public string DecryptCookie(string nameOfCookie)
        {
            var cookie = Request.Cookies[nameOfCookie];
            if (cookie != null)
            {
                return Request.Cookies[nameOfCookie].Value.Decrypt();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// get ErrorMessage by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [NonAction]
        public static string _L(string name)
        {
            return ResourcesManager._L(name);
        }

        /// <summary>
        /// get ErrorMessage by name
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="pars">paramaters string.format</param>
        /// <returns></returns>
        [NonAction]
        public static string _L(string name, object pars)
        {
            return ResourcesManager._L(name, pars);
        }

        /// <summary>
        /// get ErrorMessage by name
        /// </summary>
        /// <param name="name">key</param>
        /// <param name="pars">paramaters string.format</param>
        /// <returns></returns>
        [NonAction]
        public static string _L(string name, object[] pars)
        {
            return ResourcesManager._L(name, pars);
        }

        #region Qbicles.Doc api call

        /// <summary>
        /// Upload to S3 and Return Object Key
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<string> UploadMediaFromPath(string fileName, string filePath)
        {
            //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
            var mediaProcess = new MediaProcess
            {
                FileName = fileName,
                ObjectKey = Guid.NewGuid().ToString(),
                FilePath = filePath,
                IsPublic = false
            };
            await new AzureStorageRules(dbContext).UploadMediaFromPathByQbicleAsync(mediaProcess);
            return mediaProcess.ObjectKey;
        }

        /// <summary>
        ///     Return the URL for getting the document form the Qbicles.Doc instance
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        [NonAction]
        public string GetDocumentRetrievalUrl(string fileUri)
        {
            var documentRetrievalUrl = ConfigManager.ApiGetDocumentUri + fileUri;
            return documentRetrievalUrl;
        }

        /// <summary>
        ///     Return the URL for viewing document form the Qbicles.Doc instance
        /// </summary>
        /// <param name="fileUri"></param>
        /// <returns></returns>
        [NonAction]
        public string GetDocumentViewerUrl(string fileUri)
        {
            var documentViewerUrl = ConfigManager.DocumentsApi + "/retriever/viewfile?file=" + fileUri;
            return documentViewerUrl;
        }

        [NonAction]
        public string GetDocumentBase64(string fileUri)
        {
            try
            {
                var requestUri = $"{ConfigManager.DocumentsApi}/retriever/getdocumentbase64?file={fileUri}";
                return new BaseHttpClient().Get<string>(requestUri);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
                return "";
            }
        }

        [NonAction]
        public async Task<string> GetMediaFileBase64Async(string fileUri)
        {
            var fileStorageInformation = dbContext.StorageFiles.Find(fileUri);
            if (fileStorageInformation == null)
                return "";
            //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
            var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileStorageInformation.Id);

            var imageBase64 = HelperClass.GetBase64StringFromStream(s3Object.ObjectStream);
            return imageBase64;
        }

        [NonAction]
        public async Task<Stream> GetMediaFileBaseStreamAsync(string fileUri)
        {
            var fileStorageInformation = dbContext.StorageFiles.Find(fileUri);
            if (fileStorageInformation == null)
                return null;
            //var awsS3Bucket = AzureStorageHelper.AwsS3Client();
            var s3Object = await AzureStorageHelper.ReadObjectDataAsync(fileStorageInformation.Id);
            return s3Object.ObjectStream;
        }

        [NonAction]
        public Stream GetDocumentStream(string fileUri)
        {
            var requestUri = $"{ConfigManager.DocumentsApi}/retriever/getdocumentstream?file={fileUri}";
            return new BaseHttpClient().Get<Stream>(requestUri);
        }

        #endregion Qbicles.Doc api call

        [NonAction]
        public bool CanAccessBusiness()
        {
            var userSetting = CurrentUser();
            var isDomainAdmin = CurrentDomain().Administrators.Any(p => p.Id == userSetting.Id);
            var rightApps = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());

            if (!isDomainAdmin && !rightApps.Any(r => r == RightPermissions.QbiclesBusinessAccess))
                return false;
            return true;
        }

        [NonAction]
        public void SetDisplayUnitChangeCookies(string cookieName, object changes)
        {
            EncryptCookie(cookieName, changes.ToJson(), 1);
        }

        [NonAction]
        public List<ItemUnitChangeModel> GetDisplayUnitChangeCookies(string cookieName)
        {
            var obj = DecryptCookie(cookieName);
            if (obj == null)
                return new List<ItemUnitChangeModel>();
            return DecryptCookie(cookieName).ParseAs<List<ItemUnitChangeModel>>();
        }

        [NonAction]
        public void SetDeliveryManagementFilterCookies(string cookieName, DeliveryParameter values)
        {
            EncryptCookie(cookieName, values.ToJson(), 2);
        }

        [NonAction]
        public DeliveryParameter GetDeliveryManagementFilterCookies(string cookieName)
        {
            var obj = DecryptCookie(cookieName);
            if (obj == null)
                return null;
            return DecryptCookie(cookieName).ParseAs<DeliveryParameter>();
        }

        /// <summary>
        /// Set OriginatingConnectionId
        /// </summary>
        /// <param name="id"></param>
        [NonAction]
        public void SetOriginatingConnectionId2Cookies(string id)
        {
            EncryptCookie("hub-connect-id", id, 1);
        }

        /// <summary>
        /// Set originatingConnectionId
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string GetOriginatingConnectionIdFromCookies()
        {
            return DecryptCookie("hub-connect-id");
        }
    }

    public class HomeController : BaseController
    {
        [Authorize]
        public ActionResult Index()
        {
            var currentUserId = CurrentUser().Id;
            var user = dbContext.QbicleUser.Find(currentUserId);
            if (!user.IsUserProfileWizardRun)
                return RedirectToAction("UserProfileWizard", "UserInformation");

            var requests = dbContext.QbicleDomainRequests.Where(e => e.CreatedBy.Id == user.Id && e.Status == DomainRequestStatus.Pending)?.Select(e => e.DomainRequestJSON).ToList();

            var c2CNum = dbContext.C2CQbicles.Where(p => !p.IsHidden && p.Customers.Any(x => x.Id == currentUserId) && !p.RemovedForUsers.Any(x => x.Id == currentUserId)).Count();
            var b2CNum = dbContext.B2CQbicles.Where(p => !p.IsHidden && p.Customer.Id == currentUserId && !p.RemovedForUsers.Any(x => x.Id == currentUserId)).Count();

            var domainnRequests = new List<string>();
            requests.ForEach(r =>
            {
                domainnRequests.Add(r.ParseAs<DomainRequest>().Name);
            });

            ViewBag.ContactNumber = c2CNum + b2CNum;
            ViewBag.ListUserDomains = user.Domains;
            ViewBag.DomainRequests = string.Join(", ", domainnRequests);

            return View(user);
        }
    }
}