using System;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.Models.Bookkeeping;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class BkWorkGroupsController : BaseController
    {
        public ActionResult Edit(int id)
        {
            try
            {
                var domainId = CurrentDomainId();
                var workGroup = new BKWorkGroupsRules(dbContext).GetById(id);
                ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
                ViewBag.Topics = new TopicRules(dbContext).GetTopicByQbicle(workGroup.Qbicle.Id);
                ViewBag.Process = new BKWorkGroupsRules(dbContext).GetKBWorkGroupProcesss();
                return PartialView("_BKWorkGroupEdit", workGroup);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), e, CurrentUser().Id, id);
                return View("Error");
            }
        }

        public ActionResult GetWorkGroupUser(int id)
        {
            var wg = new BKWorkGroupsRules(dbContext).GetWorkgroupUser(id);
            return Json(wg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReInitUsersEdit(int id)
        {
            var wg = new BKWorkGroupsRules(dbContext).ReInitUsersEdit(id, CurrentDomain());
            return Json(wg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateName(BKWorkGroup wg)
        {
            var refModel = new ReturnJsonModel();
            var rule = new BKWorkGroupsRules(dbContext);
            refModel.result = rule.WorkGroupNameCheck(wg, CurrentDomainId());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            var rules = new BKWorkGroupsRules(dbContext);
            var refModel = rules.DeleteBKWorkGroup(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(BKWorkGroup wg)
        {
            wg.Domain = CurrentDomain();
            var refModel = new BKWorkGroupsRules(dbContext).CreateBKWorkGroup(wg, CurrentUser().Id, "icon_bookkeeping.png");

            refModel.actionVal = CurrentDomain().Workgroups.Any() ? 1 : 2;
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(BKWorkGroup wg)
        {
            wg.Domain = CurrentDomain();
            var refModel = new BKWorkGroupsRules(dbContext).UpdateBKWorkGroup(wg, CurrentUser().Id, "icon_bookkeeping.png");

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BookkeepingNavigatePartial()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.BKBookkeepingView))
                    return View("ErrorAccessPage");
                var bkWorkGroups = new BKWorkGroupsRules(dbContext).GetKBWorkGroupsOfUser(CurrentDomainId(), CurrentUser().Id, "");

                ViewBag.rightShowJournalTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.JournalEntry || e.Name == BookkeepingProcessName.ViewJournalEntries));
                ViewBag.rightShowAccountTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Accounts || e.Name == BookkeepingProcessName.ViewChartOfAccounts));
                ViewBag.rightShowConfigurationTab = userRoleRights.Any(r => r == RightPermissions.BKManageAppSettings);
                ViewBag.rightShowReportsTab = bkWorkGroups.Any(p => p.Processes.Any(e => e.Name == BookkeepingProcessName.Reports));

                ViewBag.tabSelected = 4;
                return PartialView("_BookkeepingNavigatePartial");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }
    }
}