using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using CleanBooksData;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class AppsController : BaseController
    {

        public ActionResult AppManagement()
        {
            ClearAllCurrentActivities();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.Count == 0)
                return View("Error");
            ViewBag.UserRoleRights = userRoleRights;
            var currentUserId = CurrentUser().Id;
            var domainId = CurrentDomainId();
            var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
            var viewAccount = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountProcessName || p.Name == CBProcessName.AccountDataProcessName))
                .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);

            var viewTask = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskProcessName || p.Name == CBProcessName.TaskExecutionProcessName))
                .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);

            ViewBag.ViewAccount = viewAccount;
            ViewBag.ViewTask = viewTask;
            //Applications
            ViewBag.AllApplications = new QbicleApplicationsRules(dbContext).GetQbicleApplicationsNotCore();
            ViewBag.SubscribedApps = CurrentDomain().SubscribedApps.Where(e => !e.IsCore).ToList();
            ViewBag.AvailableApps = CurrentDomain().AvailableApps.Where(e => !e.IsCore).ToList();
            
            ViewBag.CurrentPage = "Apps";
            SetCurrentPage("Apps");
            ViewBag.PageTitle = "Bolt-ons";
            return View();
        }



        private ActionResult RemoveFileFromServer(string urlImage)
        {
            var refModel = new ReturnJsonModel();
            var fullPath = Request.MapPath(urlImage);
            if (!System.IO.File.Exists(fullPath)) refModel.result = false;
            ;

            try //Maybe error could happen like Access denied or Presses Already User used
            {
                System.IO.File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public string RenderViewToString(object model, string viewName, string appName)
        {
            ViewData.Model = model;
            ViewBag.UserRoleRights = ApplicationUserRoleRights(appName);
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }


        public ActionResult ChangePermissions(int appId, int rightId, bool isCheck, int roleId)
        {
            try
            {
                return Json(
                    new AppRules(dbContext).ChangePermissions(appId, rightId, isCheck, roleId, CurrentDomainId()));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult UpdateAppsForDomain(List<int> appsIdAdded, List<int> appsIdRemoved, int roleId)
        {
            try
            {
                return Json(new AppRules(dbContext).UpdateAppsForDomain(appsIdAdded, appsIdRemoved, CurrentUser().Id, roleId, CurrentDomainId()));
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SubscribeApp(int appId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                var appRules = new QbicleApplicationsRules(dbContext);
                refModel = appRules.SubscribeApp(appId, CurrentUser().Id, CurrentDomainId());

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
                refModel.msg = ex.Message;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UnSubscribeApp(int appId)
        {
            var refModel = new QbicleApplicationsRules(dbContext).UnSubscribeApp(appId, CurrentUser().Id, CurrentDomainId());

            return Json(refModel, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ApprovalsApp(int domainId)
        {
            try
            {
                ClearAllCurrentActivities();
                var user = CurrentUser().Id;
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                ViewBag.approvalGroups = new ApprovalAppsRules(dbContext)
                    .GetCurrentApprovalAppsGroup(CurrentUser().Id, CurrentDomainId()).ToList();
                ViewBag.UserRoleRights = ApplicationUserRoleRights(HelperClass.appTypeApprovals);
                ViewBag.ApprovalTypes = HelperClass.EnumModel
                    .GetEnumValuesAndDescriptions<ApprovalRequestDefinition.RequestTypeEnum>();
            }
            catch (Exception ex)
            {
                ViewBag.approvalGroups = new List<ApprovalGroup>();
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            ViewBag.CurrentPage = "ApprovalApps"; SetCurrentPage("ApprovalApps");
            ViewBag.PageTitle = "Approvals Application";

            return View();
        }


        public ActionResult ApprovalsApps(int domainId)
        {
            try
            {
                ClearAllCurrentActivities();
                var user = CurrentUser().Id;
                ViewBag.listFileType = new FileTypeRules(dbContext).GetExtension();
                ViewBag.approvalGroups = new ApprovalAppsRules(dbContext)
                    .GetCurrentApprovalAppsGroup(CurrentUser().Id, CurrentDomainId()).ToList();
                ViewBag.UserRoleRights = ApplicationUserRoleRights(HelperClass.appTypeApprovals);
                ViewBag.ApprovalTypes = HelperClass.EnumModel
                    .GetEnumValuesAndDescriptions<ApprovalRequestDefinition.RequestTypeEnum>();
                //ViewBag.AllFormByDomain = new FormDefinitionRules(dbContext).GetFormDefinitionsByDomainId(user.CurrentDomain.Id);
            }
            catch (Exception ex)
            {
                ViewBag.approvalGroups = new List<ApprovalGroup>();
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            ViewBag.CurrentPage = "ApprovalApps"; SetCurrentPage("ApprovalApps");
            ViewBag.PageTitle = "Approvals Application";

            return View();
        }

        /// <summary>
        ///     reload apps available
        /// </summary>
        /// <returns>html and append to modal</returns>
        public JsonResult ChangeAvailableApps(int roleId)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                refModel = new ApprovalAppsRules(dbContext).ChangeAvailableApps(CurrentUser().Id, roleId, CurrentDomainId());
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NavigationCleanBooksPartial(string tab = "account")
        {
            var domainId = CurrentDomainId();
            var currentUserId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, domainId);
            var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
            var memberAccount = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountProcessName))
                .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
            var memberAccountData = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountDataProcessName))
                .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
            var memberTaskExecution = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskExecutionProcessName))
                .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
            var memberTask = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskProcessName))
                .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);

            var visibleAccount = (memberAccount || memberAccountData);
            var visibleTask = (memberTask || memberTaskExecution);
            var visibleConfig = userRoleRights.Any(r => r == RightPermissions.CleanBooksConfig);

            ViewBag.Tab = tab;
            ViewBag.VisibleAccount = visibleAccount;
            ViewBag.VisibleTask = visibleTask;
            ViewBag.VisibleConfig = visibleConfig;
            ViewBag.UserRoleRights = userRoleRights;
            return PartialView("_NavigationCleanBooksPartial");
        }
        //Cleanbook Accounts

        public ActionResult Accounts()
        {
            try
            {
                ClearAllCurrentActivities();
                var currentUserId = CurrentUser().Id;
                var domainId = CurrentDomainId();
                var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
                var memberAccount = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                var memberAccountData = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.AccountDataProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);

                ViewBag.MemberAccountData = memberAccountData;
                ViewBag.MemberAccount = memberAccount;
                var accountRules = new CBAccountRules(dbContext);
                ViewBag.CurrentPage = "manageaccounts"; SetCurrentPage("manageaccounts");
                return View(accountRules.GetAccountGroup(CurrentDomainId()));

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }


        // GET: TaskManage
        public ActionResult Tasks()
        {
            try
            {
                var currentUserId = CurrentUser().Id;
                var domainId = CurrentDomainId();
                var wgs = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
                var memberTaskExecution = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskExecutionProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                var memberTask = wgs.Where(d => d.Processes.Any(p => p.Name == CBProcessName.TaskProcessName))
                    .SelectMany(q => q.Members).Distinct().ToList().Any(q => q.Id == currentUserId);
                ViewBag.MemberAssigns =
                    wgs.Where(q => q.Processes.Any(p => p.Name == CBProcessName.TaskExecutionProcessName))
                        .SelectMany(s => s.Members).Distinct().ToList();
                ViewBag.MemberTaskExecution = memberTaskExecution;
                ViewBag.MemberTask = memberTask;
                ViewBag.CBWorkGroups = wgs.Where(q =>
                    q.Qbicle.Domain.Id == domainId && q.Processes.Any(p => p.Name == CBProcessName.TaskProcessName)
                                                   && q.Members.Any(m => m.Id == currentUserId)).ToList();

                var taskRules = new CBTasksRules(dbContext);

                var model = taskRules.GetTaskgroup(CurrentDomainId());
                ViewBag.taskexecutioninterval = taskRules.GetTaskexecutioninterval();
                ViewBag.qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(CurrentDomainId());
                ViewBag.taskgroup = model;
                ViewBag.CurrentPage = "CBTasks"; SetCurrentPage("CBTasks");
                ViewBag.taskPrioritys =
                    HelperClass.EnumModel.GetEnumValuesAndDescriptions<QbicleTask.TaskPriorityEnum>();
                ViewBag.taskaccount = taskRules.GetAccounts(CurrentDomainId());
                ViewBag.transactionmatching = taskRules.GetTransactionmatchingtype();
                ViewBag.tastype = taskRules.GetTasktype();
                ViewBag.balanceanalysispredefactions = taskRules.GetBalanceanalysispredefaction();
                return View(model.ToList());
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult CleanBookConfig(string value = null)
        {
            var domainId = CurrentDomainId();
            switch (value)
            {
                case "workgroup":
                    var workGroups = new CBWorkGroupsRules(dbContext).GetAllCbWorkGroup(domainId);
                    ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
                    ViewBag.Process = new CBProcessRules(dbContext).GetAll();
                    return PartialView("_CleanBookWorkGroups", workGroups);
                case "bookkeeping":
                    return PartialView("_CleanBookBookeeping");
            }

            ViewBag.CurrentPage = "CBConfig"; SetCurrentPage("CBConfig");
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.CleanBooksConfig))
                return View("ErrorAccessPage");
            ViewBag.UserRoleRights = userRoleRights;
            return View();
        }

        public ActionResult CleanBookWorkGroupAddEdit(int id = 0)
        {
            var domainId = CurrentDomainId();
            var wg = new CBWorkGroup();
            var qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
            var wgRule = new CBWorkGroupsRules(dbContext);
            if (id > 0)
            {
                wg = wgRule.GetById(id);
            }
            else if (qbicles != null && qbicles.Count > 0)
            {
                wg.Qbicle = qbicles[0];
            }
            ViewBag.Topics = new TopicRules(dbContext).GetTopicByQbicle(wg.Qbicle.Id).ToList();
            ViewBag.Qbicles = qbicles;
            ViewBag.Process = new CBProcessRules(dbContext).GetAll();
            return PartialView("_CleanBookWorkGroupAddEdit", wg);
        }
        public ActionResult ValidateName(CBWorkGroup wg)
        {
            try
            {
                var refModel = new ReturnJsonModel();
                var rule = new CBWorkGroupsRules(dbContext);
                refModel.result = rule.WorkGroupNameCheck(wg, CurrentDomainId());

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult GetWorkGroupUser(int id)
        {
            try
            {
                var wg = new CBWorkGroupsRules(dbContext).GetWorkgroupUser(id);
                return Json(wg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new WorkgroupUser(), JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(int id)
        {
            try
            {
                var domainId = CurrentDomainId();
                var workGroup = new CBWorkGroupsRules(dbContext).GetById(id);
                ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
                ViewBag.Topics = new TopicRules(dbContext).GetTopicByQbicle(workGroup.Qbicle.Id);
                ViewBag.Process = new CBProcessRules(dbContext).GetAll();
                return PartialView("_CleanBookWorkGroupEdit", workGroup);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult SaveWorkgroup(CBWorkGroup wg)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                wg.Domain = CurrentDomain();
                refModel.result = new CBWorkGroupsRules(dbContext).SaveWorkgroup(wg, CurrentUser().Id);

            }
            catch (Exception ex)
            {
                refModel.result = false;
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var refModel = new ReturnJsonModel();
            try
            {
                var rules = new CBWorkGroupsRules(dbContext);
                refModel.result = rules.Delete(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
                refModel.msg = "The Workgroup does not allow deletion because related data has arisen!";
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReInitUsersEdit(int id)
        {
            try
            {
                var wg = new CBWorkGroupsRules(dbContext).ReInitUsersEdit(id, CurrentDomain());
                return Json(wg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return Json(new WorkgroupUser(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}