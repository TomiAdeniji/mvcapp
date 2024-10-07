using Qbicles.BusinessRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.Inventory;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderSpotCountController : BaseController
    {
        [HttpPost]
        public ActionResult SaveSpotCount(SpotCount spotCount)
        {

            spotCount.Domain = CurrentDomain();
            spotCount.Location = CurrentDomain().TraderLocations.FirstOrDefault(q => q.Id == spotCount.Location.Id);
            var result = new TraderSpotCountRules(dbContext).SaveSpotCount(spotCount, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateSportCountItems(SpotCount spotCount, string appStatus)
        {

            var result = new TraderSpotCountRules(dbContext).UpdateSportCountItems(spotCount, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdateDescription(SpotCount spotCount)
        {
            var result = new TraderSpotCountRules(dbContext).UpdateDescription(spotCount);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateSpotCountContent(int id)
        {
            try
            {
                var currentUser = CurrentUser();
                var accessTrader = false;
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUser.Id, CurrentDomainId());
                if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                    accessTrader = true;
                ViewBag.AccessTrader = accessTrader;

                var rule = new TraderSpotCountRules(dbContext);
                var spotCountModel = rule.GetById(id) ?? new SpotCount();
                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(spotCountModel.SpotCountApprovalProcess.ApprovalRequestDefinition.Id, currentUser.Id);



                var timeline = rule.SpotCountApprovalStatusTimeline(spotCountModel.Id, currentUser.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return PartialView("_SpotCountContent", spotCountModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult SpotCountReview(int id)
        {
            try
            {
                var currentUser = CurrentUser();
                var rule = new TraderSpotCountRules(dbContext);
                var spotCountModel = rule.GetById(id) ?? new SpotCount();
                var currentDomainId = spotCountModel?.Domain.Id ?? 0;
                ValidateCurrentDomain(spotCountModel?.Domain, spotCountModel.Workgroup?.Qbicle.Id ?? 0);
                var accessTrader = false;
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUser.Id, currentDomainId);
                if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                    accessTrader = true;
                ViewBag.AccessTrader = accessTrader;

                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                var approvalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(spotCountModel.SpotCountApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, currentUser.Id);
                if (!approvalRight.IsApprover && !approvalRight.IsInitiators && !approvalRight.IsReviewer)
                    return View("ErrorAccessPage");
                ViewBag.TraderApprovalRight = approvalRight;
                var timeline = rule.SpotCountApprovalStatusTimeline(spotCountModel.Id, currentUser.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;


                return View(spotCountModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        public ActionResult SpotCountMaster(int id)
        {

            var currentUser = CurrentUser();

            var accessTrader = false;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUser.Id, CurrentDomainId());
            if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                accessTrader = true;
            ViewBag.AccessTrader = accessTrader;

            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var rule = new TraderSpotCountRules(dbContext);
            var spotCountModel = rule.GetById(id) ?? new SpotCount();
            var approvalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(spotCountModel.SpotCountApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, currentUser.Id);
            if (!approvalRight.IsApprover && !approvalRight.IsInitiators && !approvalRight.IsReviewer)
                return View("ErrorAccessPage");
            ViewBag.TraderApprovalRight = approvalRight;

            var timeline = rule.SpotCountApprovalStatusTimeline(spotCountModel.Id, currentUser.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View(spotCountModel);

        }

        public ActionResult SpotCountMasterForMovenmentTrend(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            var currentUser = CurrentUser();

            var accessTrader = false;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUser.Id, CurrentDomainId());
            if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                accessTrader = true;
            ViewBag.AccessTrader = accessTrader;

            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var rule = new TraderSpotCountRules(dbContext);
            var spotCountModel = rule.GetById(id) ?? new SpotCount();
            var approvalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(spotCountModel.SpotCountApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, currentUser.Id);
            if (!approvalRight.IsApprover && !approvalRight.IsInitiators && !approvalRight.IsReviewer)
                return View("ErrorAccessPage");
            ViewBag.TraderApprovalRight = approvalRight;

            var timeline = rule.SpotCountApprovalStatusTimeline(spotCountModel.Id, currentUser.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View("~/Views/TraderSpotCount/SpotCountMaster.cshtml", spotCountModel);

        }

        public ActionResult TraderItemSpotCountContent()
        {
            var locationId = CurrentLocationManage();

            ViewBag.WorkgroupSpotCount = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderSpotCountProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            //var domain = CurrentDomain();
            //var traderItems = new TraderSpotCountRules(dbContext).GetByLocationId(locationId, domain.Id);

            return PartialView("_TraderItemSpotCountContent");
        }
        public ActionResult GetByLocationPagination([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datetime, SpotCountStatus[] status, int[] workgroups)
        {
            var result = new TraderSpotCountRules(dbContext).GetByLocationPagination(CurrentLocationManage(), CurrentDomainId(), requestModel, CurrentUser(), keyword.ToLower(), datetime, workgroups, status);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddEditSpotCount(int id = 0)
        {
            var spotRule = new TraderSpotCountRules(dbContext);
            var spotCount = new SpotCount();
            try
            {
                var domain = CurrentDomain();
                var locationId = CurrentLocationManage();
                var locations = domain.TraderLocations.ToList();

                ViewBag.WorkgroupSpotCount = domain.Workgroups.Where(q =>
                   q.Location.Id == locationId
                   && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderSpotCountProcessName))
                   && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
                ViewBag.Location = new TraderLocationRules(dbContext).GetById(locationId);
                ViewBag.Locations = id == 0 ? locations.Where(l => l.Id != locationId).ToList() : locations;

                if (id > 0)
                {
                    spotCount = spotRule.GetById(id);
                }
                return PartialView("_TraderItemSpotCountAddEdit", spotCount);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ShowSpotCountItemUnit(int id, int locationId, string unitName)
        {
            var spotCountItem = dbContext.TraderItems.Find(id);
            ViewBag.UnitName = unitName;
            var units = new List<UnitModel>();
            if (spotCountItem != null)
            {
                foreach (var con in spotCountItem.Units)
                {
                    var unitmodel = new UnitModel()
                    {
                        Id = con.Id,
                        QuantityOfBaseunit = con.QuantityOfBaseunit,
                        Group = "BaseUnit",
                        Name = con.Name
                    };
                    if (con.IsActive && !units.Any(q => q.Id == unitmodel.Id && q.Group == unitmodel.Group))
                        units.Add(unitmodel);
                }
            }

            ViewBag.BaseUnits = units;
            return PartialView("_TraderSpotCountUnitPartial", spotCountItem);
        }


        /// <summary>
        /// Using for case add/edit/remove item in a spotCountItem existed
        /// </summary>
        /// <param name="spotCountItem"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateSpotCountProduct(SpotCountItem spotCountItem, bool isDelete)
        {
            var result = new TraderSpotCountRules(dbContext).UpdateSpotCountProduct(spotCountItem, CurrentUser().Id, isDelete);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSpotCountItem([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int spotCountId)
        {
            var locationId = CurrentLocationManage();
            var result = new TraderSpotCountRules(dbContext).GetSpotCountItem(requestModel, spotCountId, locationId, CurrentDomainId(), CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<WasteReportItemModel>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
    }
}