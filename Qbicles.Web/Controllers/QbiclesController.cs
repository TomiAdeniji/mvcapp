using Microsoft.AspNet.Identity;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Community;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Form;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static Qbicles.BusinessRules.HelperClass;
using static Qbicles.Models.QbicleActivity;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class QbiclesController : BaseController
    {
        private static List<QbiclePost> topicPost = new List<QbiclePost>();
        private static List<QbicleTask> activitiesTasks = new List<QbicleTask>();
        private static List<QbicleAlert> activitiesAlerts = new List<QbicleAlert>();
        private static List<QbicleMedia> activitiesMedias = new List<QbicleMedia>();
        private static List<QbicleEvent> activitiesEvents = new List<QbicleEvent>();
        private static List<ApprovalReq> activitiesApprovals = new List<ApprovalReq>();
        private static List<QbicleLink> activitiesLinks = new List<QbicleLink>();
        private static List<QbicleDiscussion> activitiesDiscussions = new List<QbicleDiscussion>();

        private static List<object> subActivities = new List<object>();

        // pinned activities
        private static List<QbiclePost> pinnedTopicPosts = new List<QbiclePost>();
        private static List<QbicleActivity> myPinnedTasks = new List<QbicleActivity>();
        private static List<QbicleActivity> myPinnedAlerts = new List<QbicleActivity>();
        private static List<QbicleActivity> myPinnedEvents = new List<QbicleActivity>();
        private static List<QbicleActivity> myPinnedMedias = new List<QbicleActivity>();
        private static List<QbicleActivity> myPinnedApprovals = new List<QbicleActivity>();
        private static List<QbicleActivity> myPinnedLinks = new List<QbicleActivity>();
        private static List<QbicleActivity> myPinnedDiscussions = new List<QbicleActivity>();

        private static QbicleDiscussion discussionSelected = new QbicleDiscussion();
        private int acivitiesDateCount;
        private ReturnJsonModel refModel;

        public ActionResult Index()
        {
            try
            {
                var logRule = new QbicleLogRules(dbContext);
                var log = new DomainAccessLog(CurrentUser().Id, CurrentDomainId());
                logRule.SaveDomainAccessLog(log);

                SetCurrentPage("Qbicles");
                ViewBag.CurrentPage = "Qbicles";
                var dr = new DomainRules(dbContext);
                var user = dr.GetUser(CurrentUser().Id);
                var currentDomain = CurrentDomain();
                if (CurrentDomainId() <= 0 && currentDomain != null)
                    SetCurrentDomainIdCookies(currentDomain.Id);

                ViewBag.DomainUsers = currentDomain.Users;
                ViewBag.currentQbicle = new Qbicle();
                ViewBag.Topics = new TopicRules(dbContext).GetTopicByDomain(CurrentDomainId());
                var existedQbicleNames = currentDomain.Qbicles.Select(p => p.Name).ToList();
                ViewBag.ExistedQbicleNameStr = String.Join(",", existedQbicleNames);

                if (currentDomain != null)
                {
                    var lstQbicles = currentDomain.Administrators.Any(u => u.Id == user.Id) ?
                    new QbicleRules(dbContext).GetQbicleByDomainId(currentDomain.Id) :
                    new QbicleRules(dbContext).GetQbicleByUserId(currentDomain.Id, user.Id);
                    ViewBag.QbicleList = lstQbicles.Where(q => !q.IsHidden).ToList();

                    var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomain.Id && p.IsArchived == false);
                    ViewBag.CurrentDomainPlan = currentDomainPlan;
                }
                else
                {
                    ViewBag.QbicleList = new List<Qbicle>();
                }

                ViewBag.ActivityType = Request["ActivityType"] ?? "";


                if (CurrentDomainId() > 0 && currentDomain != null)
                {
                    var domain = currentDomain.BusinessMapping(CurrentUser().Timezone);

                    if (domain != null)
                        ViewBag.UserCurrentDomain = domain.Users;
                    else
                        domain = new QbicleDomain
                        {
                            Name = "All Domains",
                            Id = 0
                        };

                    ViewBag.PageTitle = "Qbicles List";

                    return View(domain);
                }
                else
                {
                    ViewBag.PageTitle = "Qbicles List";

                    var domain = new QbicleDomain
                    {
                        Name = "Guest Qbicles",
                        Id = -1
                    };
                    return View(domain);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                var AuthenticationManager = HttpContext.GetOwinContext().Authentication;
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Login", "Account");
            }

        }

        /// <summary>
        ///     Default order by Latest activity
        /// </summary>
        /// <returns></returns>
        //public ActionResult Qbicle()
        //{
        //    try
        //    {
        //        ViewBag.CurrentPage = "Qbicles"; SetCurrentPage("Qbicles");
        //        var dr = new DomainRules(dbContext);
        //        var user = dr.GetUser(CurrentUser().Id);

        //        ViewBag.CommunityPages =
        //            new CommunityPageRules(dbContext).GetCommunityPagesDisplayOnQbicle(CurrentDomainId());

        //        ViewBag.Topics = new TopicRules(dbContext).GetTopicByDomain(CurrentDomainId());

        //        if (user == null)
        //            return RedirectToAction("Login", "Account");

        //        if (CurrentDomainId() > 0)
        //        {
        //            var currentDomain = CurrentDomain();
        //            var domain = currentDomain.BusinessMapping(CurrentUser().Timezone);

        //            if (domain != null)
        //                ViewBag.UserCurrentDomain = domain.Users;
        //            else
        //                domain = new QbicleDomain
        //                {
        //                    Name = "All Domains",
        //                    Id = 0
        //                };

        //            ViewBag.PageTitle = "Welcome to Qbicles";
        //            ViewBag.Domains = user.Domains;

        //            return View(domain);
        //        }
        //        else
        //        {
        //            ViewBag.PageTitle = "Welcome to Qbicles";
        //            ViewBag.Domains = user.Domains;

        //            var domain = new QbicleDomain
        //            {
        //                Name = "Guest Qbicles",
        //                Id = -1
        //            };
        //            return View(domain);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
        //        return RedirectToAction("Login", "Account");
        //    }
        //}

        public ActionResult ApplyFilterQbicle(QbicleSearchParameter cubeParameter)
        {
            refModel = new ReturnJsonModel();
            try
            {
                cubeParameter.UserId = CurrentUser().Id;
                cubeParameter.DomainId = CurrentDomainId();
                var qbicles = new QbicleRules(dbContext).FilterQbicle(cubeParameter);
                return PartialView("~/Views/Qbicles/_QbicleList.cshtml", qbicles);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ShowOrHideQbicle(string key, bool isHidden)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var qbicleId = int.Parse(key.Decrypt());
                var qRules = new QbicleRules(dbContext);
                refModel.result = qRules.ShowOrHideQbicle(qbicleId, isHidden);


            }
            catch (Exception ex)
            {

                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);

                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCubeUsers(int id)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var qRules = new QbicleRules(dbContext);
                var users = qRules.GetQbicleById(id).Members.ToList();
                var userIds = users.Select(u => u.Id).ToList();
                ViewBag.UserProfiles = dbContext.UserProfilePages.Where(x => userIds.Contains(x.AssociatedUser.Id))
                    .ToList();


                return PartialView("_QbicleUsers", users);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult DuplicateQbicleNameCheck(string key, string qbicName)
        {
            try
            {
                refModel = new ReturnJsonModel();
                try
                {
                    var qbicId = int.Parse(key.Decrypt());
                    var qb = new QbicleRules(dbContext);
                    refModel.result = qb.DuplicateQbicleNameCheck(qbicId, qbicName, CurrentDomainId());
                }
                catch (Exception ex)
                {
                    refModel.result = false;
                    refModel.Object = ex;
                }

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult SaveQbicle(Qbicle qbicle, string[] userQbicle, string[] guestsQbicle, string managerId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                if (qbicle.Name.Length > 50 || (qbicle.Description != null && qbicle.Description.Length > 350))
                {
                    refModel.result = false;
                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }

                if (qbicle.Id == 0 && string.IsNullOrEmpty(qbicle.LogoUri))
                    qbicle.LogoUri = QbicleLogoDefault;


                var userSetting = CurrentUser();
                qbicle.Name = qbicle.Name.Trim();

                // exclude new guest from guestsQbicle???
                refModel = new QbicleRules(dbContext).SaveQbicle(qbicle, userQbicle, guestsQbicle, CurrentDomainId(), userSetting.Id, managerId);

                var newUserGuests = (List<ApplicationUser>)refModel.Object;

                //send email to guest invited
                foreach (var guet in newUserGuests)
                {
                    var callbackUrl = GenerateUrlToken(guet.Id, qbicle.Id,
                        ActivityTypeEnum.QbicleActivity, userSetting.Email);
                    new EmailRules(dbContext).SendEmailInvitedGuest(userSetting.Id, guet.Email,
                        callbackUrl, ActivityTypeEnum.QbicleActivity, qbicle.Domain?.Name, qbicle.Name);
                }

                refModel.result = true;
            }
            catch (Exception ex)
            {
                refModel.result = false;
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.Object = ex;
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("/");
        }

        /// <summary>
        ///     get the Qbicle by qbicle Id
        /// </summary>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public ActionResult GetQbicle(string key)
        {
            try
            {
                var qbicleId = int.Parse(key.Decrypt());
                var qRules = new QbicleRules(dbContext);
                var cube = qRules.GetQbicleToEditView(qbicleId, CurrentUser().Id);
                cube.LogoUri = GetDocumentRetrievalUrl(cube.LogoUri);
                return Json(cube, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        ///// <summary>
        /////     Close a Qbicle by QbicleId
        ///// </summary>
        ///// <param name="closeQbicleId">QbicleId</param>
        ///// <returns></returns>
        //public ActionResult CloseQbicle(string key)
        //{
        //    try
        //    {
        //        refModel = new ReturnJsonModel();
        //        try
        //        {
        //            var closeQbicleId = int.Parse(key.Decrypt());
        //            refModel.result = new QbicleRules(dbContext).CloseQbicle(closeQbicleId, CurrentUser().Id);
        //        }
        //        catch (Exception ex)
        //        {
        //            refModel.result = false;
        //            refModel.Object = ex;
        //        }

        //        return Json(refModel, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
        //        return View("Error");
        //    }
        //}

        ///// <summary>
        /////     Re-Open the Qbicle has Closed
        ///// </summary>
        ///// <param name="openQbicleId"></param>
        ///// <returns></returns>
        //public ActionResult ReOpenQbicle(int openQbicleId)
        //{
        //    try
        //    {
        //        refModel = new ReturnJsonModel();
        //        try
        //        {
        //            refModel.result = new QbicleRules(dbContext).ReOpenQbicle(openQbicleId, CurrentUser().Id);
        //        }
        //        catch (Exception ex)
        //        {
        //            refModel.result = false;
        //            refModel.Object = ex;
        //        }

        //        return Json(refModel, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
        //        return View("Error");
        //    }
        //}

        ///// <summary>
        /////     Get Qbicle with order by
        ///// </summary>
        ///// <param name="orderBy">
        /////     1.Latest activity:
        /////     All Public Qbicles (Qbicle.Scope) associated with the Domain
        /////     All Private Qbicles associated with the Domain where the user is associated with the Qbicle
        /////     The order is to be based on the latest QbicleActivity.StartedDate of each Qbicle QbicleActivity.
        /////     2.Open Qbicles:
        /////     All Public Qbicles (Qbicle.Scope) associated with the Domain
        /////     All Private Qbicles associated with the Domain where the user is associated with the Qbicle
        /////     The order is to be based on the latest QbicleActivity.StartedDate of each Qbicle QbicleActivity.
        /////     No Closed Qbicles are to be displayed
        /////     3.Closed Qbicles:
        /////     All Public Qbicles (Qbicle.Scope) associated with the Domain
        /////     All Private Qbicles associated with the Domain where the user is associated with the Qbicle
        /////     The order is to be based on the latest QbicleActivity.StartedDate of each Qbicle QbicleActivity.
        /////     No Open Qbicles are to be displayed
        ///// </param>
        ///// <returns></returns>
        //public ActionResult FilterQbicle(bool cubePublic, bool cubePrivate, bool cubeOpen,
        //    bool cubeClosed, string cubeSearch)
        //{
        //    refModel = new ReturnJsonModel();
        //    try
        //    {
        //        //var cubeParameterSearch = new QbicleSearchParameter
        //        //{
        //        //    Public = cubePublic,
        //        //    Private = cubePrivate,
        //        //    Open = cubeOpen,
        //        //    Closed = cubeClosed,
        //        //    Name = cubeSearch.Replace("₩", " ")
        //        //};
        //        //var qRules = new QbicleRules(dbContext);
        //        //refModel.Object = qRules.FilterQbicle(currentUser(0), cubeParameterSearch, CurrentDomain());
        //        //if (refModel.Object != null)
        //        //{
        //        //    var cubes = (List<Qbicle>)refModel.Object;
        //        //    cubes = cubes.BusinessMapping(UserSettings().Timezone).OrderByDescending(u => u.LastUpdated).ToList();
        //        //    return PartialView("../Domain/_DomainPartial", cubes);
        //        //}
        //        //else
        //        return PartialView("../Domain/_DomainPartial", new List<Qbicle>());
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(MethodBase.GetCurrentMethod(),ex, UserSettings().Id);
        //        return View("Error");
        //    }
        //}

        #region render view to string helper

        /// <summary>
        ///     Render view to string for Load next
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="model"></param>
        /// <param name="moduleSelected"></param>
        /// <returns></returns>
        public string RenderLoadNextViewToString(string viewName, object model, Enums.QbicleModule moduleSelected)
        {
            ViewData.Model = model;
            var dateCollection = (IList)model;
            switch (moduleSelected)
            {
                case Enums.QbicleModule.Dashboard:
                case Enums.QbicleModule.Topic:
                    ViewBag.cubeActivities = new QbicleRules(dbContext).GetActivities((IEnumerable<DateTime>)model,
                        topicPost, activitiesTasks, activitiesAlerts, activitiesMedias, activitiesEvents,
                        activitiesApprovals, activitiesLinks, activitiesDiscussions);
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.pinnedTopicPosts = pinnedTopicPosts;
                    ViewBag.myPinnedAlerts = myPinnedAlerts;
                    ViewBag.myPinnedEvents = myPinnedEvents;
                    ViewBag.myPinnedMedias = myPinnedMedias;
                    ViewBag.myPinnedTasks = myPinnedTasks;
                    ViewBag.myPinnedApprovals = myPinnedApprovals;
                    ViewBag.myPinnedDiscussions = myPinnedDiscussions;
                    ViewBag.currentTimeZone = CurrentUser().Timezone;
                    break;
                case Enums.QbicleModule.Discussions:
                    ViewBag.acitvitiesDiscussion =
                        topicPost.Where(d => dateCollection.Contains(d.StartedDate.Date)).ToList();
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinneds = pinnedTopicPosts;
                    break;
                case Enums.QbicleModule.Tasks:
                    ViewBag.activitiesTask =
                        activitiesTasks.Where(d => dateCollection.Contains(d.StartedDate.Date)).ToList();
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinneds = myPinnedTasks;
                    break;
                case Enums.QbicleModule.Alerts:
                    ViewBag.activitiesAlert = activitiesAlerts.Where(d => dateCollection.Contains(d.StartedDate.Date))
                        .ToList();
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinneds = myPinnedAlerts;
                    break;
                case Enums.QbicleModule.Media:
                    ViewBag.activitiesMedia = activitiesMedias.Where(d => dateCollection.Contains(d.StartedDate.Date))
                        .ToList();
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinneds = myPinnedMedias;
                    break;
                case Enums.QbicleModule.Events:
                    ViewBag.activitiesEvent = activitiesEvents.Where(d => dateCollection.Contains(d.StartedDate.Date))
                        .ToList();
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinneds = myPinnedEvents;
                    break;
                case Enums.QbicleModule.Settings:
                    break;
                case Enums.QbicleModule.SubActivities:
                    ViewBag.subActivities = subActivities;
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinnedDiscussions = pinnedTopicPosts;
                    ViewBag.myPinnedAlerts = myPinnedAlerts;
                    ViewBag.myPinnedEvents = myPinnedEvents;
                    ViewBag.myPinnedMedias = myPinnedMedias;
                    ViewBag.myPinnedTasks = myPinnedTasks;
                    ViewBag.myPinnedApprovals = myPinnedApprovals;
                    break;
                case Enums.QbicleModule.Approvals:
                    ViewBag.activitiesApprovals = activitiesApprovals
                        .Where(d => dateCollection.Contains(d.StartedDate.Date)).ToList();
                    ViewBag.cubeActivities = activitiesApprovals.Where(d => dateCollection.Contains(d.StartedDate.Date))
                        .Cast<object>().ToList();
                    ViewBag.AcivitiesDateCount = acivitiesDateCount;
                    ViewBag.myPinneds = pinnedTopicPosts;
                    break;
            }


            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
        public string RenderLoadNextViewToString(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
        #endregion

        /// <summary>
        ///     View Dashboard
        /// </summary>
        /// <returns></returns>
        public ActionResult Dashboard()
        {
            try
            {
                if (CurrentQbicleId() == 0)
                    return Redirect("/");
                var currentUserId = CurrentUser().Id;
                var log = new QbicleAccessLog(currentUserId, CurrentQbicleId(), CurrentDomainId());
                new QbicleLogRules(dbContext).SaveQbicleAccessLog(log);

                ClearModuleSelected();

                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var cubeId = CurrentQbicleId();

                var currentCube = new QbicleRules(dbContext).GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);
                if (!currentCube.Members.Any(u => u.Id == currentUserId) && !CurrentDomain().Administrators.Any(a => a.Id == currentUserId))
                    return View("ErrorAccessPage");

                switch (currentCube.Domain.Name)
                {
                    case SystemDomainConst.BUSINESS2BUSINESS:
                        return Redirect("~/Commerce");
                    case SystemDomainConst.BUSINESS2CUSTOMER:
                        var b2cqbicle = currentCube as Qbicles.Models.B2C_C2C.B2CQbicle;
                        var isDomainAdmin = b2cqbicle.Business.Administrators.Any(p => p.Id == currentUserId);
                        var isMemberOfDomain = b2cqbicle.Business.Users.Any(p => p.Id == currentUserId);
                        var isMemberOfQbicle = b2cqbicle.Members.Any(p => p.Id == currentUserId);
                        var isCustomerOfBusiness = b2cqbicle.Customer.Id == currentUserId;
                        var goBack = CurrentGoBackPage();

                        if ((isDomainAdmin || (isMemberOfDomain && isMemberOfQbicle)) && (!isCustomerOfBusiness || (goBack == "B2C" && isCustomerOfBusiness)))
                        {
                            ValidateCurrentDomain(b2cqbicle.Business, cubeId);
                            return Redirect("~/B2C");
                        }
                        else
                            return Redirect("~/C2C");
                    case SystemDomainConst.CUSTOMER2CUSTOMER:
                        return Redirect("~/C2C");
                }


                ViewBag.currentQbicle = currentCube;

                if (currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else
                    ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);
                ViewBag.mediaFolders = new MediaFolderRules(dbContext).GetMediaFoldersByQbicleIdAndUserId(cubeId, currentUserId);
                ViewBag.qbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(cubeId);
                ViewBag.lstUser = new QbicleRules(dbContext).GetUsersCustomByQbicleId(cubeId);
                ViewBag.CurrentPage = "Dasboard"; SetCurrentPage("Dashboard");
                ViewBag.PageTitle = "All Activity";
                ViewBag.ActivityType = Request["ActivityType"] ?? "";
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
                ViewBag.FileTypes = new FileTypeRules(dbContext).GetFileTypes();
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        /// <summary>
        ///     Return view Calendar Content
        /// </summary>
        /// <param name="type">today;week;month</param>
        /// <param name="day"></param>
        /// <param name="keyword"></param>
        /// <param name="orderby">StartDate asc;StartDate desc;Status asc</param>
        /// <param name="types">0:Approval,1:Event,2:Media,3:Task,4:Alert</param>
        /// <param name="topics"></param>
        /// <param name="status"></param>
        /// <param name="peoples"></param>
        /// <param name="apps">Bookkeeping;Cleanbooks;Operator;Trader</param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateCalendarActivities(string type, string day, string keyword, string orderby,
            short[] types, int[] topics, string[] peoples, string[] apps, int pageSize, int pageIndex)
        {
            refModel = new ReturnJsonModel() { result = false, msg = "An error" };
            var userId = CurrentUser().Id;
            try
            {
                var totalRecords = 0;
                var activities = new QbicleRules(dbContext).ActivitiesForCalendar(CurrentUser().Timezone,
                    CurrentQbicleId(), type, day, keyword, orderby, types, topics, peoples, apps, pageSize, pageIndex,
                    ref totalRecords, CurrentUser().DateFormat);

                ViewBag.type = type;
                ViewBag.Pinneds = (from pin in dbContext.MyPins
                                   join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                                   where desk.Owner.Id == userId && pin.PinnedActivity != null
                                   select pin.PinnedActivity.Id).ToList();
                var partialView = RenderViewToString("_CalendarContent", activities);
                refModel.result = true;
                refModel.Object = new
                {
                    strResult = partialView,
                    totalRecord = totalRecords
                };
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId);
                refModel.result = false;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadDotActivities(int? Year, int? Month)
        {
            var lstDot = new QbicleRules(dbContext).ActivitiesRecursExistListDate(CurrentQbicleId(), CurrentUser().Timezone, Year, Month);
            return Json(lstDot, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadDotMyDeskActivities(int? Year, int? Month)
        {
            var lstDate = new List<DateTime>();
            var currentDate = DateTime.Now;
            DateTime firstDate;
            if (Year.HasValue && Month.HasValue)
                firstDate = new DateTime(Year.Value, Month.Value, 1);
            else
                firstDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            var lastDayOfMonth = firstDate.AddMonths(1).AddDays(-1);
            for (var current = firstDate; current <= lastDayOfMonth; current = current.AddDays(1)) lstDate.Add(current);
            var lstDot =
                new QbicleRules(dbContext).ActivitiesListDateDotMyDesk(lstDate, CurrentUser().Timezone, CurrentUser().Id);
            return Json(lstDot, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        ///     Discussions Panel
        /// </summary>
        /// <returns></returns>
        public ActionResult Discussions()
        {
            try
            {
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var cubeId = CurrentQbicleId();
                var cubeRules = new QbicleRules(dbContext);
                var currentCube = cubeRules.GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;

                ViewBag.PageTitle = "Discussions";
                ViewBag.CurrentPage = "Discussions"; SetCurrentPage("Discussions");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///     View Discussion
        /// </summary>
        /// <returns></returns>
        public ActionResult Discussion()
        {
            try
            {
                SetCurrentTaskIdCookies();
                SetCurrentAlertIdCookies();
                SetCurrentEventIdCookies();
                SetCurrentMediaIdCookies();
                SetCurrentApprovalIdCookies();

                /*get Discussion*/
                var disRules = new DiscussionsRules(dbContext);
                discussionSelected =
                    disRules.GetDiscussionById(CurrentDiscussionId()).BusinessMapping(CurrentUser().Timezone);
                /*end get Discussion*/
                if (discussionSelected == null)
                    return View(new List<object>());

                var currentCube = discussionSelected.Qbicle.BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;
                if (currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else
                    ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);

                ViewBag.ActivityName = discussionSelected.Name;
                if (CurrentDiscussionId() == 0)
                    return View(new List<object>());
                ViewBag.CurrentPage = "Discussion";
                SetCurrentPage("Discussion");
                ViewBag.PageTitle = "Discussion: " + discussionSelected.Name;
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
                return View(subActivities);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///     Tasks Panel
        /// </summary>
        /// <returns></returns>
        public ActionResult Tasks()
        {
            try
            {
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var cubeId = CurrentQbicleId();
                var cubeRules = new QbicleRules(dbContext);
                var currentCube = cubeRules.GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;

                ViewBag.PageTitle = "Tasks";
                ViewBag.CurrentPage = "Tasks"; SetCurrentPage("Tasks");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///     View Task
        /// </summary>
        /// <returns></returns>
        public ActionResult Task()
        {
            try
            {
                var task = new TasksRules(dbContext).GetTaskById(CurrentTaskId()).BusinessMapping(CurrentUser().Timezone);
                if (task.Qbicle != null)
                {
                    ValidateCurrentDomain(task?.Qbicle.Domain, task.Qbicle.Id);
                    var currentCube = task.Qbicle.BusinessMapping(CurrentUser().Timezone);
                    var closedTitle = currentCube.ClosedDate == null
                        ? ""
                        : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                    ViewBag.QbicleName = currentCube.Name + closedTitle;
                    ViewBag.TaskPriority = EnumModel.GetEnumValuesAndDescriptions<QbicleTask.TaskPriorityEnum>();

                    if (currentCube.LogoUri == "")
                        ViewBag.LogoUri = ImageNotFoundUrl;
                    else
                        ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);
                }

                var taskAssignee = task.AssociatedSet?.Peoples?.FirstOrDefault(m => m.Type == QbiclePeople.PeopleTypeEnum.Assignee);
                var currentUserId = CurrentUser().Id;
                var currentDomain = CurrentDomain();
                var taskQbicle = task.Qbicle;
                var showReview = false;
                if ((currentUserId == task.StartedBy.Id || taskQbicle.Members.Any(mem => mem.Id == currentUserId)) && (taskAssignee?.User == null || taskAssignee.User.Id != currentUserId))
                {
                    showReview = true;
                }

                ViewBag.IsAbleToReview = showReview;
                ViewBag.UserRoleRights = ApplicationUserRoleRights(appTypeProcessDocumentation);
                ViewBag.CurrentPage = "Task"; SetCurrentPage("Task");
                ViewBag.PageTitle = "Task: " + task.Name;

                var b2cMember = false;
                b2cMember = task.Qbicle.Members.Any(e => e.Id == currentUserId);

                ViewBag.B2CMember = b2cMember;

                if (task.task != null)
                    return View("CBTask", task);
                return View(task);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        /// <summary>
        ///     Alerts Panel
        /// </summary>
        /// <returns></returns>
        public ActionResult Alerts()
        {
            try
            {
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);
                var cubeId = CurrentQbicleId();
                var cubeRules = new QbicleRules(dbContext);
                var currentCube = cubeRules.GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;


                ViewBag.PageTitle = "Alerts";
                ViewBag.CurrentPage = "Alerts"; SetCurrentPage("Alerts");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///     Alert page
        /// </summary>
        /// <returns></returns>
        public ActionResult Alert()
        {
            try
            {
                var alert = new AlertsRules().GetAlertById(CurrentAlertId());
                if (alert == null)
                    return View("Error");
                ValidateCurrentDomain(alert.Qbicle.Domain, alert.Qbicle.Id);
                ViewBag.Alert = alert.BusinessMapping(CurrentUser().Timezone);
                ViewBag.CurrentPage = "Alert"; SetCurrentPage("Alert");
                ViewBag.PageTitle = "Alert: " + alert.Name;
                ViewBag.alertPriority = EnumModel.GetEnumValuesAndDescriptions<QbicleAlert.AlertPriorityEnum>();
                var currentCube = alert.Qbicle.BusinessMapping(CurrentUser().Timezone);
                ViewBag.QbicleName = currentCube.Name;
                if (currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else
                    ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);

                ViewBag.ActivityName = alert.Name;
                if (alert.Qbicle.Domain.Users.Any(u => u.Id == CurrentUser().Id)) return View();
                var commPageId = CurrentCommunityPageId();
                if (commPageId <= 0) return View();
                ViewBag.IsMember = "Follower";
                ViewBag.CommEmail = new CommunityPageRules(dbContext).GetCommunityPageById(commPageId)
                    .PublicContactEmail;

                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        /// <summary>
        ///     Medias Panel
        /// </summary>
        /// <returns></returns>
        public ActionResult Medias()
        {
            try
            {
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var cubeId = CurrentQbicleId();
                var cubeRules = new QbicleRules(dbContext);
                var currentCube = cubeRules.GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;

                ViewBag.PageTitle = "Medias";
                ViewBag.CurrentPage = "Medias"; SetCurrentPage("Medias");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///     Media page
        /// </summary>
        /// <returns></returns>
        public ActionResult Media(string key)
        {
            try
            {

                var currentUser = CurrentUser();

                var media = new MediasRules(dbContext).GetMediaById(int.Parse(key.Decrypt())).BusinessMapping(currentUser.Timezone);
                ValidateCurrentDomain(media?.Qbicle.Domain, media?.Qbicle.Id ?? 0);

                ViewBag.CurrentPage = "Media"; SetCurrentPage("Media");
                ViewBag.PageTitle = "Media: " + media.Name;

                media.VersionedFiles = media.VersionedFiles.Where(e => !e.IsDeleted).ToList();

                ViewBag.ListMediaFolders = new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(media.Qbicle.Id, "");

                ViewBag.Topics = new TopicRules(dbContext).GetTopicByQbicle(media.Qbicle.Id);

                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();

                ViewBag.Media = media;

                var b2cMember = false;
                //if (media.Qbicle is B2CQbicle)
                b2cMember = media.Qbicle.Members.Any(e => e.Id == currentUser.Id);

                ViewBag.B2CMember = b2cMember;

                if (media.Qbicle.Domain.Users.Any(u => u.Id == currentUser.Id))
                    return View();

                var commPageId = CurrentCommunityPageId();
                if (commPageId <= 0)
                    return View();

                ViewBag.IsMember = "Follower";
                ViewBag.CommEmail = new CommunityPageRules(dbContext).GetCommunityPageById(commPageId).PublicContactEmail;

                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        /// <summary>
        ///     Events Panel
        /// </summary>
        /// <returns></returns>
        public ActionResult Events()
        {
            try
            {
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var cubeId = CurrentQbicleId();
                var cubeRules = new QbicleRules(dbContext);
                var currentCube = cubeRules.GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);
                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;
                ViewBag.LogoUri = currentCube.LogoUri == ""
                    ? ImageNotFoundUrl
                    : currentCube.LogoUri;
                ViewBag.DomainLogo = currentCube.Domain.LogoUri;

                ViewBag.PageTitle = "Events";
                ViewBag.CurrentPage = "Event"; SetCurrentPage("Events");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        ///     Event page
        /// </summary>
        /// <returns></returns>
        public ActionResult Event()
        {
            try
            {
                var ev = new EventsRules(dbContext).GetEventById(CurrentEventId()).BusinessMapping(CurrentUser().Timezone);
                ValidateCurrentDomain(ev?.Qbicle.Domain, ev?.Qbicle.Id ?? 0);
                if (ev.Qbicle.Id == CurrentQbicleId())
                {
                    ViewBag.CurrentPage = "Events"; SetCurrentPage("Events");
                }
                ViewBag.PageTitle = "Event: " + ev.Name;
                ViewBag.eventType = EnumModel.GetEnumValuesAndDescriptions<QbicleEvent.EventTypeEnum>();
                //Invites
                var qbicleSet = ev.AssociatedSet;
                ViewBag.Invites = qbicleSet != null ? new TasksRules(dbContext).GetPeoples(qbicleSet.Id) : null;
                //end
                var currentCube = ev.Qbicle.BusinessMapping(CurrentUser().Timezone);
                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                if (currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else
                    ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);
                ViewBag.QbicleName = currentCube.Name + closedTitle;

                return View(ev);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        public ActionResult Link()
        {
            try
            {
                var lk = new LinksRules(dbContext).GetLinkById(CurrentLinkId()).BusinessMapping(CurrentUser().Timezone);
                ValidateCurrentDomain(lk?.Qbicle.Domain, lk?.Qbicle.Id ?? 0);
                ViewBag.CurrentPage = "Link"; SetCurrentPage("Link");
                ViewBag.PageTitle = "Link: " + lk.Name;
                var currentCube = lk.Qbicle.BusinessMapping(CurrentUser().Timezone);
                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                if (currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else
                    ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);
                ViewBag.QbicleName = currentCube.Name + closedTitle;
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);

                return View(lk);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        /// <summary>
        ///     Clear all cookies activityId and go back url of the activity
        /// </summary>
        private void ClearModuleSelected()
        {
            ClearAllCurrentActivities();
        }


        public ActionResult GetInviteGuestsByQbicleId(int qbicleId)
        {
            var result = new List<string>();
            try
            {
                //var qRules = new QbicleRules(dbContext);
                //var qbicle = qRules.GetQbicleById(qbicleId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult MyDesk()
        {
            try
            {
                //curr
                var logRule = new QbicleLogRules(dbContext);
                QbicleLog log = new QbicleLog(QbicleLogType.MyDeskAccess, CurrentUser().Id);
                logRule.SaveQbicleLog(log);

                var userId = CurrentUser().Id;
                ViewBag.Domains = new DomainRules(dbContext).GetDomainsByUserId(userId);

                SetCurrentPage("MyDesk");
                ViewBag.CurrentPage = "MyDesk";

                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var myDesk = dbContext.MyDesks.FirstOrDefault(u => u.Owner.Id == userId);
                if (myDesk == null)
                {
                    myDesk = new MyDesksRules(dbContext).CreateMyDesk(userId);
                    if (myDesk == null)
                        return null;
                }

                var uiSettings = new QbicleRules(dbContext).LoadUiSettings("MyDesk", userId);
                var currentDate = DateTime.UtcNow;
                var lsttoday =
                    new MyDesksRules(dbContext).GetActivityByDueDate(currentDate, currentDate.AddDays(1), userId, 0, 1);
                var lstWeek =
                    new MyDesksRules(dbContext).GetActivityByDueDate(currentDate, currentDate.AddDays(7), userId, 0, 2);
                lstWeek = lstWeek.Where(p => !lsttoday.Any(a => a.Id == p.Id)).OrderBy(o => o.TimeLineDate).ToList();
                ViewBag.lstWeek = lstWeek.BusinessMapping(CurrentUser().Timezone);
                var lstUser = new List<UserCustom>();

                var currentTimeZone = CurrentUser().Timezone;
                var querryActivity = from pin in dbContext.MyPins
                                     join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                                     where desk.Owner.Id == userId && pin.PinnedActivity != null
                                     select pin.PinnedActivity;
                var myPinnedActivities = querryActivity.ToList().BusinessMapping(currentTimeZone);
                ViewBag.myPinned = myPinnedActivities;

                ViewBag.Tags = new MyDesksRules(dbContext).GetAllTags(myDesk.Id);
                ViewBag.MyDeskId = myDesk.Id;
                ViewBag.PageTitle = "My Desk";
                ViewBag.UiSetting = uiSettings;
                return View(myDesk);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GenerateModalTask(string taskKey)
        {
            try
            {
                var taskId = string.IsNullOrEmpty(taskKey) ? 0 : int.Parse(taskKey.Decrypt());
                //var user = currentUser(0);
                //ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                //ViewBag.qbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(user.CurrentQbicle.Id);
                var recurrance = new RecurranceRules(dbContext).GetRecurranceById(0);
                ViewBag.lstMonth = Utility.GetListMonth(DateTime.UtcNow);
                ViewBag.Recurrance = recurrance;
                ViewBag.taskId = taskId;
                ViewBag.taskKey = taskKey;
                return PartialView("_ModalTask");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        public ActionResult GenerateModalEvent(int eventId)
        {
            try
            {
                //ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                // get data to modal add new event
                var currentCube = new Qbicle();
                if (CurrentQbicleId() <= 0)
                {
                    ViewBag.UserCurrentQbicleAssing = new List<ApplicationUser>();
                }
                else
                {
                    var cubeId = CurrentQbicleId();
                    var cubeRules = new QbicleRules(dbContext);
                    currentCube = cubeRules.GetQbicleById(cubeId) ?? new Qbicles.Models.Qbicle();
                }

                var recurrance = new RecurranceRules(dbContext).GetRecurranceById(0);

                ViewBag.lstMonth = Utility.GetListMonth(DateTime.UtcNow);
                ViewBag.UserCurrentQbicleAssing = currentCube?.Members?.ToList() ?? new List<ApplicationUser>();

                ViewBag.eventId = eventId;
                ViewBag.EventRecurrance = recurrance;
                return PartialView("_ModalEvent");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        [HttpPost]
        public JsonResult GetListDate(string dayofweek, int type, int Pattern, int customDate, string LastOccurenceDate,
            string firstOccurenceDate)
        {
            try
            {
                string formatDatetime = CurrentUser().DateTimeFormat;
                var dtLastOccurenceDate =
                    DateTime.ParseExact(LastOccurenceDate, CurrentUser().DateFormat, CultureInfo.InvariantCulture);
                var dtCalculator =
                    DateTime.ParseExact(firstOccurenceDate, formatDatetime, CultureInfo.InvariantCulture);
                switch (type)
                {
                    case 0:
                        {
                            var lstDay = Utility.GetListDayToTable(dtCalculator, dayofweek, dtLastOccurenceDate);
                            return Json(lstDay, JsonRequestBehavior.AllowGet);
                        }

                    case 1:
                        {
                            var lstWeek = Utility.GetListWeekToTable(dtCalculator, dayofweek, dtLastOccurenceDate);
                            return Json(lstWeek, JsonRequestBehavior.AllowGet);
                        }
                }

                dtCalculator = new DateTime(dtCalculator.Year, dtCalculator.Month + 1, 1, dtCalculator.Hour,
                    dtCalculator.Minute, dtCalculator.Second);
                var lstMonth =
                    Utility.GetListMonthToTable(dtCalculator, Pattern, customDate, dayofweek, dtLastOccurenceDate, formatDatetime);
                return Json(lstMonth, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult GenerateModalMedia()
        {
            try
            {
                var user = CurrentUser().Id;
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                ViewBag.listMediaFolder =
                    new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(CurrentQbicleId(), "");
                // get data to modal add new event
                var currentCube = new Qbicle();
                if (CurrentQbicleId() <= 0)
                {
                    ViewBag.UserCurrentQbicleAssing = new List<ApplicationUser>();
                }
                else
                {
                    var cubeId = CurrentQbicleId();
                    var cubeRules = new QbicleRules(dbContext);
                    currentCube = cubeRules.GetQbicleById(cubeId) ?? new Qbicle();
                }


                ViewBag.UserCurrentQbicleAssing = currentCube?.Members?.ToList() ?? new List<ApplicationUser>();
                return PartialView("_ModalMedia");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        public ActionResult GenerateModalMediaByQbicleId(int cubeId)
        {
            try
            {
                var user = CurrentUser().Id;
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                ViewBag.listMediaFolder = new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(cubeId, "");
                var currentCube = new Qbicle();
                var cubeRules = new QbicleRules(dbContext);
                currentCube = cubeRules.GetQbicleById(cubeId);
                ViewBag.UserCurrentQbicleAssing = currentCube?.Members?.ToList() ?? new List<ApplicationUser>();
                ViewBag.CurrentQbicleId = cubeId;
                return PartialView("_ModalMedia");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        public ActionResult GenerateModalAlert()
        {
            try
            {
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                // get data to modal add new event
                var currentCube = new Qbicle();
                if (CurrentQbicleId() <= 0)
                {
                    ViewBag.UserCurrentQbicleAssing = new List<ApplicationUser>();
                }
                else
                {
                    var cubeId = CurrentQbicleId();
                    var cubeRules = new QbicleRules(dbContext);
                    currentCube = cubeRules.GetQbicleById(cubeId) ?? new Qbicles.Models.Qbicle();
                }

                ViewBag.UserCurrentQbicleAssing = currentCube.Members
                    .ToList();
                return PartialView("_ModalAlert");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        public ActionResult GenerateModalTopic()
        {
            try
            {
                return PartialView("_ModalTopic");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }
        public ActionResult GenerateModalDiscussion(int disId)
        {
            try
            {
                var qbicleId = CurrentQbicleId();
                var discussion = disId > 0 ? new DiscussionsRules(dbContext).GetDiscussionById(disId) : new QbicleDiscussion();
                ViewBag.CurrentQbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(qbicleId);
                var domainLink = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath;
                ViewBag.DefaultMedia = HelperClass.GetListDefaultMedia(domainLink);
                ViewBag.FeaturedImageId = (dbContext.StorageFiles.FirstOrDefault(s => s.Id == discussion.FeaturedImageUri)?.Name ?? "0").Replace(".jpg", "");
                return PartialView("_ModalDiscussion", discussion);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        /// <summary>
        ///     Re-send email token to guest
        /// </summary>
        /// <param name="tokenToUserId"></param>
        /// <param name="tokenToEmail"></param>
        /// <param name="activityId"></param>
        /// <param name="type"></param>
        /// <param name="sendByEmail"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ReSendInvited(string tokenToUserId, string tokenToEmail, int activityId,
            ActivityTypeEnum type, string sendByEmail)
        {
            try
            {
                string callbackUrl;
                bool emailInvited;
                var currentUser = dbContext.QbicleUser.FirstOrDefault(u => u.Email == sendByEmail);
                if (type == ActivityTypeEnum.Domain)
                {
                    var activity = new DomainRules(dbContext).GetDomainById(activityId);

                    callbackUrl = GenerateUrlToken(tokenToUserId, activity.Id, type, sendByEmail);

                    emailInvited = new EmailRules(dbContext).SendEmailInvitedGuest(currentUser.Id, tokenToEmail,
                        callbackUrl, type, activity.Name, "");
                }
                else if (type == ActivityTypeEnum.QbicleActivity)
                {
                    var activity = new QbicleRules(dbContext).GetQbicleById(activityId);

                    callbackUrl = GenerateUrlToken(tokenToUserId, activity.Id, type, sendByEmail);
                    emailInvited = new EmailRules(dbContext).SendEmailInvitedGuest(currentUser.Id, tokenToEmail,
                        callbackUrl, type, activity.Name, activity.Name);
                }
                else
                {
                    var activity = new QbicleRules(dbContext).GetActivity(activityId);

                    callbackUrl = GenerateUrlToken(tokenToUserId, activity.Id, type, sendByEmail);
                    emailInvited = new EmailRules(dbContext).SendEmailInvitedGuest(currentUser.Id, tokenToEmail,
                        callbackUrl, activity.ActivityType, activity.Name, activity.Qbicle.Name);
                }

                refModel = new ReturnJsonModel
                {
                    result = emailInvited
                };
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, "");
                return View("Error");
            }
        }


        public ActionResult Approvals()
        {
            try
            {
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var currentCube = new QbicleRules(dbContext).GetQbicleById(CurrentQbicleId()).BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;

                ViewBag.PageTitle = "Approvals";
                ViewBag.CurrentPage = "Approvals"; SetCurrentPage("Approvals");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult Approval()
        {
            try
            {
                var approval = new ApprovalsRules(dbContext).GetApprovalById(CurrentApprovalId())
                    .BusinessMapping(CurrentUser().Timezone);
                ValidateCurrentDomain(approval.Qbicle.Domain, approval.Qbicle.Id);

                ViewBag.Approval = approval;
                if (approval.ApprovalRequestDefinition != null)
                    ViewBag.CurrentReviewerAndApprover =
                        new ApprovalAppsRules(dbContext).GetIsReviewerAndApprover(
                            (int)approval.ApprovalRequestDefinition?.Id,
                            CurrentUser().Id);
                else
                    ViewBag.CurrentReviewerAndApprover = new IsReviewerAndApproverModel();
                ViewBag.Title = "Approval";
                ViewBag.CurrentPage = "Approval"; SetCurrentPage("Approval");
                var formEdited =
                    new ApprovalReqFormRefRules(dbContext).GetApprovalDefinitionRefsByApprovalId(CurrentApprovalId());
                var allForm = approval.ApprovalRequestDefinition?.Forms ?? new List<FormDefinition>();
                if (allForm.Any())
                    allForm.ForEach(x => x.Definition =
                        formEdited.FirstOrDefault(f => f.FormDefinition.Id == x.Id) != null
                            ? formEdited.FirstOrDefault(f => f.FormDefinition.Id == x.Id)?.FormBuilder
                            : x.Definition);
                ViewBag.FormDefinition = allForm;

                var currentCube = approval.Qbicle.BusinessMapping(CurrentUser().Timezone);
                ViewBag.QbicleName = currentCube?.Name;

                if (currentCube != null && currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else if (currentCube != null) ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);

                ViewBag.ActivityName = approval.Name;
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult SetRequestStatusForApprovalRequest(string appKey, ApprovalReq.RequestStatusEnum status)
        {
            try
            {
                var result = new ApprovalsRules(dbContext).SetRequestStatusForApprovalRequest(appKey, status, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public ActionResult SetPurchaseTransferApprovalRequest(string appKey, ApprovalReq.RequestStatusEnum status)
        {
            try
            {
                var result = new ApprovalsRules(dbContext).SetPurchaseTransferApprovalRequest(appKey, status, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult OurPeople()
        {
            try
            {
                var currentDomain = CurrentDomain();
                if (currentDomain == null)
                    return View(new List<OurPeopleModel>());
                var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomain.Id && p.IsArchived == false);
                ViewBag.CurrentDomainPlan = currentDomainPlan;
                var userSetting = CurrentUser();
                var model = new OurPeopleRules(dbContext).GetAllOurPeopleByDomain(CurrentDomainId(), userSetting.Timezone, userSetting.Id);
                var dr = new DomainRules(dbContext);
                var user = dr.GetUser(userSetting.Id);
                var lstDomain = user.Domains;
                var lstQbicleNotNull = lstDomain?.Where(p => p.Qbicles.Count > 0).ToList();
                if (lstQbicleNotNull != null && lstQbicleNotNull.Any())
                {
                    lstDomain = lstDomain.Where(p => !lstQbicleNotNull.Any(a => a.Id == p.Id)).ToList();
                    lstQbicleNotNull = lstQbicleNotNull
                        .OrderBy(o => o.Qbicles.OrderBy(a => a.LastUpdated).FirstOrDefault().LastUpdated).ToList();
                    lstDomain.AddRange(lstQbicleNotNull);
                }

                var lstInvitation = new OurPeopleRules(dbContext).GetAllInvitationByDomain(currentDomain);
                //App Permissions
                ViewBag.Roles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(currentDomain.Id);
                ViewBag.Domains = lstDomain;
                ViewBag.Invitation = lstInvitation;

                ViewBag.Users = currentDomain.Users.ToList();
                ViewBag.Domain = currentDomain;
                ViewBag.CurrentUserId = userSetting.Id;
                ViewBag.CurrentPage = "OurPeople"; SetCurrentPage("OurPeople");
                ViewBag.PageTitle = "Our People";

                var userSlots = HelperClass.GetDomainUsersAllowed(currentDomain.Id);
                ViewBag.CanSendInvite = userSlots.ActualMembers < userSlots.UsersAllowed;
                //ViewBag.UserSlots = $"{userSlots.ActualMembers}/{userSlots.UsersAllowed}";

                ViewBag.ActualMembers = userSlots.ActualMembers;
                ViewBag.UsersAllowed = userSlots.UsersAllowed;
                return View(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }


        [HttpPost]
        public ActionResult LoadAllUserBySystemAdmin([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var totalRecord = 0;

            var lstUser =
                new UserRules(dbContext).GetAllBySystemAdmin(requestModel, ref totalRecord) ??
                new List<UserCustom>();
            if (lstUser.Any())
            // lstUser.All(c => { c.Domain = string.Join("<br />", c.lstDomain.ToArray()); return true; });
            {
                //var roles = dbContext.Roles.ToList();
                foreach (var item in lstUser)
                {
                    item.UserName = (string.IsNullOrEmpty(item.Forename) || string.IsNullOrEmpty(item.Surname))
                        ? item.UserName : item.Forename + " " + item.Surname;
                    foreach (var domain in item.Domains)
                        item.Domain += "<li>" + domain + "</li>";
                    //foreach (var roleId in item.SystemRoles)
                    //    item.Roles += "<li>" + roles.FirstOrDefault(e => e.Id == roleId).Name + "</li>";                    
                }
            }
            var dataTableData = new DataTableModel
            {
                draw = requestModel.Draw,
                //draw = draw,
                data = lstUser,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SuspendOrActive(string userId, int type)
        {
            refModel = new ReturnJsonModel();
            try
            {
                refModel.result = new UserRules(dbContext).SuspendOrActive(userId, type);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.result = false;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetUsersByQbicle(int qbicleId)
        {
            refModel = new ReturnJsonModel();
            try
            {
                var dRules = new QbicleRules(dbContext);
                var users = dRules.GetUsersByQbicleId(qbicleId);
                var str = new StringBuilder();
                foreach (var item in users)
                    str.AppendFormat("<option value='{0}'>{1}</option>", item.Id, item.Forename + " " + item.Surname);
                refModel.Object = str.ToString();
                refModel.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                refModel.result = false;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        [ChildActionOnly]
        public ActionResult GeneratesTaskFormDefinitionPartial(TaskFormDefinitionRef taskTormDefinition, string css,
            bool closedDate)
        {
            try
            {
                ViewBag.css = css;
                ViewBag.closed = closedDate;
                return PartialView("_GeneratesTaskFormDefinitionPartial", taskTormDefinition);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GennarateAllReportByTaskIdToTabs(int taskId, int currentFormId)
        {
            try
            {
                //var fbuilderAndName = new TasksRules(dbContext).GetFormBuilderAndFormNameByTaskId(taskId);
                var task = new TasksRules(dbContext).GetTaskById(taskId);
                ViewBag.CurrentFormId = currentFormId;
                ViewBag.closed = task.ClosedBy != null ? "true" : "false";
                //return PartialView("_GennarateAllReportByTaskIdToTabsPartial", fbuilderAndName);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult Topic()
        {
            try
            {
                var tRules = new TopicRules(dbContext);
                ClearModuleSelected();
                if (Request.Url != null) SetCookieGoBackPage(Request.Url.PathAndQuery);

                var cubeId = CurrentQbicleId();
                var cubeRules = new QbicleRules(dbContext);
                var currentCube = cubeRules.GetQbicleById(cubeId).BusinessMapping(CurrentUser().Timezone);

                var closedTitle = currentCube.ClosedDate == null
                    ? ""
                    : "(Closed on " + ((DateTime)currentCube.ClosedDate).DatetimeToOrdinal() + ")";
                ViewBag.QbicleName = currentCube.Name + closedTitle;

                if (currentCube.LogoUri == "")
                    ViewBag.LogoUri = ImageNotFoundUrl;
                else
                    ViewBag.LogoUri = GetDocumentRetrievalUrl(currentCube.LogoUri);


                ViewBag.Topic = tRules.GetTopicById(CurrentTopicId()).BusinessMapping(CurrentUser().Timezone);
                ViewBag.PageTitle = "Topic";
                ViewBag.CurrentPage = "Topic"; SetCurrentPage("Topic");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return View("Error");
            }
        }


        public ActionResult MediaFolder()
        {
            try
            {
                ViewBag.CurrentPage = "MediaFolder"; SetCurrentPage("MediaFolder");
                var qbicle = new QbicleRules(dbContext).GetQbicleById(CurrentQbicleId());
                ViewBag.QbicleName = qbicle.Name;
                var listMediaFolderBy =
                    new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(CurrentQbicleId(), "");
                return View(listMediaFolderBy);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult InsertOrUpdateMediaFolder(int mediaFolderId, string mediaFolderName)
        {
            refModel = new ReturnJsonModel { result = false };


            if (mediaFolderName == GeneralName)
            {
                refModel.msg = ResourcesManager._L("ERROR_MSG_322");
                return Json(refModel, JsonRequestBehavior.DenyGet);
            }

            var mfRules = new MediaFolderRules(dbContext);
            MediaFolder mFoler = null;
            if (!mfRules.IsDuplicateFolderName(mediaFolderId, mediaFolderName, CurrentQbicleId()))
            {
                if (mediaFolderId > 0)
                    mFoler = mfRules.UpdateMediaFolder(mediaFolderId, mediaFolderName, CurrentQbicleId());
                else
                    mFoler = mfRules.InsertMediaFolder(mediaFolderName, CurrentUser().Id, CurrentQbicleId());
            }
            else
            {
                refModel.msg = ResourcesManager._L("ERROR_MEDIANAME_EXISTS");
            }


            if (mFoler != null && mFoler.Id > 0)
            {
                refModel.result = true;
                refModel.Object = new { mFoler.Id, mFoler.Name };
            }

            return Json(refModel, JsonRequestBehavior.DenyGet);
        }


        public ActionResult GetMediaItemByFolderId(int mediaFolderId)
        {
            try
            {
                var listMedia = new MediaFolderRules(dbContext).GetMediaItemByFolderId(mediaFolderId, CurrentQbicleId(), CurrentUser().Timezone);
                var result = listMedia.Select(x => new
                {
                    x.Id,
                    x.Key,
                    x.Name,
                    x.Description,
                    ImgPath = CheckimgPath(x.FileType, x.VersionedFiles.Where(e => !e.IsDeleted)
                        .OrderByDescending(f => f.UploadedDate).FirstOrDefault()),
                    x.FileType.Type,
                    MediaFolderId = x.MediaFolder.Id,
                    LastUpdate = x.TimeLineDate.ToString(CurrentUser().DateFormat + " hh:mmtt")
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        [NonAction]
        private string CheckimgPath(QbicleFileType fType, VersionedFile file)
        {
            var url = "";
            if (fType != null)
                switch (fType.Extension)
                {
                    case "jpg":
                    case "gif":
                    case "png":
                        if (file != null)
                            url = ConfigManager.ApiGetDocumentUri + file.Uri;
                        else
                            url = fType.ImgPath;
                        break;
                    default:
                        url = fType.ImgPath;
                        break;
                }

            return url;
        }

        public ActionResult DeleteMediaFolderById(int mFolderId)
        {
            try
            {
                var result = new MediaFolderRules(dbContext).DeleteMediaFolderById(mFolderId, CurrentQbicleId());
                return Json(result);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SaveMoveMediasToOtherFolder(int toFolder, List<int> listMedias)
        {
            try
            {
                var result =
                    new MediaFolderRules(dbContext).SaveMoveMediasToOtherFolder(toFolder, CurrentQbicleId(), listMedias);
                return Json(result);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult MediaReview(int id)
        {
            try
            {
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                var versionFile = new MediasRules(dbContext).GetVersionedFileById(id);
                ViewBag.ViewerUrl = GetDocumentViewerUrl(versionFile.Uri);
                ViewBag.VersionFile = versionFile;
                return PartialView("_MediaReview");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        // Load more older Activities
        [HttpPost]
        public JsonResult LoadMoreEvents(int size)
        {
            try
            {
                var evRules = new EventsRules(dbContext);
                var cubeId = CurrentQbicleId();
                var model = evRules.LoadMoreEvents(cubeId, size, ref activitiesEvents, ref acivitiesDateCount,
                    CurrentUser().Timezone).ToList();
                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    var modelString = RenderLoadNextViewToString("_Events", model, Enums.QbicleModule.Events);
                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        [HttpPost]
        public JsonResult LoadMoreMedias(int size)
        {
            try
            {
                var mRules = new MediasRules(dbContext);
                var cubeId = CurrentQbicleId();
                var model = mRules.LoadMoreMedias(cubeId, size, ref activitiesMedias, ref acivitiesDateCount,
                    CurrentUser().Timezone).ToList();
                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    var modelString = RenderLoadNextViewToString("_Medias", model, Enums.QbicleModule.Media);
                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        ///     Load more old post, sub activities of a Discussion when scroll on top
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadMoreSubActivities(int size)
        {
            try
            {
                var dRule = new DiscussionsRules(dbContext);

                var model = dRule.LoadMoreSubActivities(discussionSelected, size,
                    ref subActivities, ref acivitiesDateCount, CurrentUser().Timezone).ToList();
                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    var modelString = RenderLoadNextViewToString("_DiscussionSubActivities", model,
                        Enums.QbicleModule.SubActivities);
                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        ///     Load more old Activities  when scroll on top/ Dashboard page
        /// </summary>
        /// <param name="size">Count Activities * Activity Page Size</param>
        /// <param name="appFilters"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadMoreActivities(QbicleFillterModel fillterModel)
        {
            try
            {
                var user = CurrentUser();
                var cubeId = CurrentQbicleId();
                var qbRule = new QbicleRules(dbContext);
                fillterModel.QbicleId = cubeId;
                fillterModel.UserId = user.Id;
                var model = qbRule.GetQbicleStreams(fillterModel, user.Timezone, user.DateFormat);
                if (model != null)
                {
                    //var modelString = RenderLoadNextViewToString("_Dashboard", model);

                    var result = Json(new { ModelString = ActivityPostHtmlTemplateRules.getQbicleStreamsHtml(model, user.Id, user.Timezone, user.DateFormat), ModelCount = model.TotalCount },
                        JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue;
                    return result;
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        /// <summary>
        ///     Load more old Taks when scroll on top
        /// </summary>
        /// <param name="size">Count tasks * task Page Size</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LoadMoreTasks(int size)
        {
            try
            {
                var tkRule = new TasksRules(dbContext);
                var cubeId = CurrentQbicleId();

                var model = tkRule.LoadMoreTasks(cubeId, size,
                    ref activitiesTasks, ref acivitiesDateCount, CurrentUser().Timezone).ToList();
                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    var modelString = RenderLoadNextViewToString("_Tasks", model, Enums.QbicleModule.Tasks);
                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        [HttpPost]
        public JsonResult LoadMoreApprovals(int size)
        {
            try
            {
                var cubeId = CurrentQbicleId();
                var appRule = new ApprovalsRules(dbContext);

                var model = appRule.LoadMoreApprovals(cubeId, size,
                    ref activitiesApprovals, ref acivitiesDateCount, CurrentUser().Timezone).ToList();
                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    var modelString =
                        RenderLoadNextViewToString("_Approvals", model, Enums.QbicleModule.Approvals);

                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }
        [HttpPost]
        public JsonResult LoadMoreTopic(int size, string[] activityFilters = null, string[] topicFilters = null)
        {
            try
            {
                var cubeId = CurrentQbicleId();
                var qbRule = new QbicleRules(dbContext);
                DateTime[] model;
                if (activityFilters == null && topicFilters == null)
                {
                    model = qbRule.LoadMoreQbicleActivities(cubeId, size,
                        ref topicPost, ref activitiesTasks,
                        ref activitiesAlerts, ref activitiesMedias, ref activitiesEvents,
                        ref acivitiesDateCount, ref activitiesApprovals, ref activitiesLinks, ref activitiesDiscussions, CurrentUser().Timezone,
                        CurrentTopicId()).ToArray();
                }
                else
                {
                    if (activityFilters != null && activityFilters.Length > 0 &&
                        (activityFilters[0] == "0" || activityFilters[0] == ""))
                        activityFilters = null;
                    if (topicFilters != null && topicFilters.Length > 0 &&
                        (topicFilters[0] == "0" || topicFilters[0] == ""))
                        topicFilters = null;
                    model = qbRule.LoadMoreQbicleActivitiesFilter(cubeId, size,
                        ref topicPost, ref activitiesTasks,
                        ref activitiesAlerts, ref activitiesMedias, ref activitiesEvents,
                        ref acivitiesDateCount, ref activitiesApprovals, ref activitiesLinks, CurrentUser().Timezone,
                        activityFilters,
                        topicFilters).ToArray();
                }

                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    ViewBag.currentTimeZone = CurrentUser().Timezone;
                    var modelString = RenderLoadNextViewToString("_Dashboard", model, Enums.QbicleModule.Topic);

                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        [HttpPost]
        public JsonResult LoadMoreAlerts(int size)
        {
            try
            {
                var alertRules = new AlertsRules(dbContext);
                var cubeId = CurrentQbicleId();
                var model = alertRules.LoadMoreAlerts(cubeId, size, ref activitiesAlerts, ref acivitiesDateCount,
                    CurrentUser().Timezone).ToList();
                var modelCount = acivitiesDateCount;
                if (model.Any())
                {
                    var modelString = RenderLoadNextViewToString("_Alerts", model, Enums.QbicleModule.Alerts);
                    return Json(new { ModelString = modelString, ModelCount = modelCount });
                }

                return Json(model);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        //// Load more older Activity

        public ActionResult LoadMoreActivityMedias(int activityId, int size)
        {
            try
            {
                var endOfOlder = false;
                var medias = new MediasRules(dbContext).GetActivityMedias(activityId, size, out endOfOlder);
                //medias = medias.OrderByDescending(d => d.TimeLineDate).Skip(size).Take(activitiesPageSize).ToList();
                //if (medias.Count < size) endOfOlder = true;
                //ViewBag.EndOfOlder = endOfOlder;
                if (medias.Count > 0)
                    return PartialView("_ActivityMedias", medias);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult LoadMoreActivityPosts(string activityKey, int size, bool isDiscussionOrder = false)
        {
            try
            {
                var activityId = string.IsNullOrEmpty(activityKey) ? 0 : int.Parse(activityKey.Decrypt());
                var endOfOlder = false;
                //var totalSize = 0;
                var posts = new PostsRules(dbContext).GetActivityPosts(activityId, size, out endOfOlder);
                //var totalSize = posts.Count;
                //posts = posts.OrderByDescending(d => d.TimeLineDate).Skip(size).Take(activitiesPageSize).ToList();
                //if (totalSize <= (size + activitiesPageSize)) 
                //    endOfOlder = true;
                ViewBag.EndOfOlder = endOfOlder;
                ViewBag.IsDiscussionOrder = isDiscussionOrder;

                if (posts.Count > 0)
                    return PartialView("_ActivityPosts", posts);
                return null;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return null;
            }
        }

        public string RenderViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult GenerateTopicManager(string view, int[] topics)
        {
            try
            {
                ViewBag.view = view;
                var currentQbicleId = CurrentQbicleId();
                if (view == "list")
                    ViewBag.ListTopics = new TopicRules(dbContext).ListTopic(currentQbicleId);
                else
                    ViewBag.Topics = new TopicRules(dbContext).GetTopicByQbicle(currentQbicleId, topics);
                return PartialView("_TopicManager");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }

        public ActionResult LoadMediasByFolderId(int folderId, string name, string fileType)
        {
            try
            {
                var listMedia = new MediaFolderRules(dbContext).MediaFilter(folderId, CurrentQbicleId(), name, fileType, CurrentUser().Timezone);
                return PartialView("_MediasContent", listMedia);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }
        public ActionResult DiscussionQbicle(string disKey)
        {
            try
            {
                var disId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(EncryptionService.Decrypt(disKey));

                var discussionQbicle = new DiscussionsRules(dbContext).GetDiscussionById(disId);
                if (discussionQbicle == null)
                    return View("Error");
                ValidateCurrentDomain(discussionQbicle.Qbicle.Domain, discussionQbicle.Qbicle.Id);
                ViewBag.CurrentPage = "DiscussionQbicle"; SetCurrentPage("DiscussionQbicle");

                SetCurrentDiscussionIdCookies(disId);
                return View(discussionQbicle);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }
        public ActionResult DiscussionParticipants(int disId, string keyword)
        {
            try
            {
                keyword = keyword.ToLower();
                var discussion = new DiscussionsRules(dbContext).GetDiscussionById(disId);

                ViewBag.Creator = discussion.StartedBy;
                var members = discussion.ActivityMembers != null ? discussion.ActivityMembers.Where(s => s.Surname.ToLower().Contains(keyword) || s.Forename.ToLower().Contains(keyword)).ToList() : new List<ApplicationUser>();
                return PartialView("_DiscussionParticipants", members);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult DisContactDetail(int disId, string uId)
        {
            try
            {
                var discussion = new DiscussionsRules(dbContext).GetDiscussionById(disId);
                ViewBag.Creator = discussion.StartedBy;
                ViewBag.DiscussionId = disId;
                return PartialView("_ParticipantsDetail", new UserRules().GetById(uId));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult RemoveContactDiscussion(int disId, string uId)
        {
            refModel = new ReturnJsonModel
            {
                result = new DiscussionsRules(dbContext).RemoveContactDiscussion(disId, uId, CurrentUser())
            };
            return Json(refModel);
        }
        public ActionResult SearchQbicleUsersInviteDiscussion(int disId, string keyword)
        {

            try
            {
                var disRule = new DiscussionsRules(dbContext);
                var discussion = disRule.GetDiscussionById(disId);
                ViewBag.Creator = discussion.StartedBy;
                return PartialView("_DiscussionParticipantsAdd", disRule.SearchQbicleUsersInviteDiscussion(disId, keyword, CurrentQbicleId(), discussion));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// Detail invite User  Discussion
        /// </summary>
        /// <param name="disId">Discussion Id</param>
        /// <param name="uId">User Id</param>
        /// <returns></returns>
        public ActionResult DisContactDetailAdd(int disId, string uId)
        {
            try
            {
                var discussion = new DiscussionsRules(dbContext).GetDiscussionById(disId);
                ViewBag.Creator = discussion.StartedBy;
                ViewBag.DiscussionId = disId;
                return PartialView("_ParticipantsDetailAdd", new UserRules().GetById(uId));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, this.CurrentUser().Id);
                return View("Error");
            }
        }
        [HttpPost]
        public ActionResult AddContactDiscussion(int disId, string uId)
        {
            refModel = new ReturnJsonModel
            {
                result = new DiscussionsRules(dbContext).AddContactDiscussion(disId, uId, CurrentUser())
            };
            return Json(refModel);
        }
        public JsonResult getFileTypeInfo(string ext)
        {
            return Json(new FileTypeRules(dbContext).GetFileTypeByExtension(ext), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveCurrencySettings(CurrencySetting currencySetting)
        {
            currencySetting.Domain = CurrentDomain();
            return Json(new CurrencySettingRules(dbContext).SaveCurrencyConfiguration(currencySetting));
        }
        public ActionResult GetCurrencySettings()
        {
            char currencyGroupSeparator = Convert.ToChar(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyGroupSeparator);
            var cs = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            if (cs != null)
                return Json(new { cs.CurrencySymbol, cs.SymbolDisplay, cs.DecimalPlace, currencyGroupSeparator }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                currencyGroupSeparator = ",",
                CurrencySymbol = "",
                SymbolDisplay = 0,
                DecimalPlace = 2
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCurrencySettingsByDomain(int domainId)
        {
            char currencyGroupSeparator = Convert.ToChar(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyGroupSeparator);
            var cs = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
            return Json(new { cs.CurrencySymbol, cs.SymbolDisplay, cs.DecimalPlace, currencyGroupSeparator }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckActivityDetailPageAccessibility(string activityKey)
        {
            var activityId = string.IsNullOrEmpty(activityKey) ? 0 : int.Parse(activityKey.Decrypt());
            var checkResult = new QbicleRules(dbContext).CheckActivityAccibility(activityId, CurrentUser().Id);
            return Json(checkResult, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GenerateForwardPostModal()
        {
            try
            {
                var currentQbicleId = CurrentQbicleId();
                var qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(CurrentDomainId());
                if (qbicles != null && qbicles.Any())
                    qbicles = qbicles.Where(s => s.Id != currentQbicleId).ToList();
                return PartialView("_ForwardPost", qbicles);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }
        public ActionResult GenerateEditPostModal(string postKey)
        {
            try
            {
                var postId = 0;
                if (!string.IsNullOrEmpty(postKey?.Trim()))
                {
                    postId = Int32.Parse(EncryptionService.Decrypt(postKey));
                }
                var post = new PostsRules(dbContext).GetPostById(postId);
                ViewBag.QbicleTopics = new TopicRules(dbContext).GetTopicByQbicle(CurrentQbicleId());
                return PartialView("_EditPost", post);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }
        public ActionResult ShowB2BPromoteCatalogModal()
        {
            var currentBusinessDomainId = CurrentDomainId();
            var currentListLocationId = CurrentDomain().TraderLocations.Select(p => p.Id).ToList();
            var lstCatalogs = new PosMenuRules(dbContext).FiltersCatalog(currentListLocationId, "", true, (int)SalesChannelEnum.B2B);
            return PartialView("_B2BPromoteCatalogModal", lstCatalogs);
        }

        public ActionResult SetHubConnect(string id)
        {
            SetOriginatingConnectionId2Cookies(id);
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChatAlertsModal()
        {
            try
            {
                //var user = CurrentUser().Id;
                //ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                //ViewBag.listMediaFolder =
                //    new MediaFolderRules(dbContext).GetMediaFoldersByQbicleId(CurrentQbicleId(), "");
                //// get data to modal add new event
                //var currentCube = new Qbicle();
                //if (CurrentQbicleId() <= 0)
                //{
                //    ViewBag.UserCurrentQbicleAssing = new List<ApplicationUser>();
                //}
                //else
                //{
                //    var cubeId = CurrentQbicleId();
                //    var cubeRules = new QbicleRules(dbContext);
                //    currentCube = cubeRules.GetQbicleById(cubeId) ?? new Qbicles.Models.Qbicle();
                //}


                //ViewBag.UserCurrentQbicleAssing = currentCube?.Members?.ToList() ?? new List<ApplicationUser>();
                return PartialView("_ChatAlertsModal");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, User.Identity.GetUserId());
                return null;
            }
        }


        public ActionResult ShowMediaFolderPanelOnDashboard(int folderId)
        {
            var currentUserId = CurrentUser().Id;
            var cubeId = CurrentQbicleId();
            var mediaFolders = new MediaFolderRules(dbContext).GetMediaFoldersByQbicleIdAndUserId(cubeId, currentUserId);

            ViewBag.FolderId = folderId;
            return PartialView("_MediaFolderPanelOnDashboard", mediaFolders);
        }
    }
}