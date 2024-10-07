using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderWasteReportController : BaseController
    {
        public ActionResult WasteUnitSelect(int unitId, int itemId)
        {
            ViewBag.ItemId = itemId;
            ViewBag.UnitSelectedId = unitId;
            var conversions = new List<UnitModel>();

            var item = dbContext.WasteItems.FirstOrDefault(e => e.Id == itemId)?.Product.Id ?? 0;

            if (unitId == 0 && item == 0)
                item = itemId;


            var traderItem = new TraderItemRules(dbContext).GetById(item);
            if (traderItem.Units.Count > 0)
                conversions.AddRange(traderItem.Units.Select(u => new UnitModel
                {
                    Id = u.Id,
                    QuantityOfBaseunit = u.QuantityOfBaseunit,
                    Quantity = u.Quantity,
                    Name = u.Name,
                    Group = "BaseUnit",
                    Selected = "",
                    IsBase = u.IsBase
                }));

            return PartialView("_WasteUnitBySelectsItemPartial", conversions);
        }

        [HttpPost]
        public ActionResult SaveWasteReport(WasteReport wasteReport)
        {
            wasteReport.CreatedDate = DateTime.UtcNow;
            wasteReport.Domain = CurrentDomain();
            wasteReport.Location = CurrentDomain().TraderLocations.FirstOrDefault(q => q.Id == wasteReport.Location.Id);
            var result = new TraderWasteReportRules(dbContext).SaveWasteReport(wasteReport, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateWasteReportItems(WasteReport wasteReport, string appStatus)
        {
            var result = new TraderWasteReportRules(dbContext).UpdateWasteReportItems(wasteReport, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateDescription(WasteReport wasteReport)
        {
            var result = new TraderWasteReportRules(dbContext).UpdateDescription(wasteReport);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WasteReportReview(int id)
        {
            var rule = new TraderWasteReportRules(dbContext);
            var wasteReportModel = rule.GetById(id) ?? new WasteReport();
            var currentDomainId = wasteReportModel?.Domain.Id ?? 0;
            ValidateCurrentDomain(wasteReportModel?.Domain, wasteReportModel.Workgroup?.Id ?? 0);
            var user = CurrentUser();
            var accessTrader = false;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
            if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                accessTrader = true;
            ViewBag.AccessTrader = accessTrader;

            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");

            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(wasteReportModel.WasteApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, user.Id);

            var timeline = rule.WasteApprovalStatusTimeline(wasteReportModel.Id, user.Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View(wasteReportModel);
        }

        public ActionResult UpdateWasteReportContent(int id)
        {
            var user = CurrentUser();
            var accessTrader = false;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
            if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                accessTrader = true;
            ViewBag.AccessTrader = accessTrader;
            var rule = new TraderWasteReportRules(dbContext);
            var wasteReportModel = rule.GetById(id) ?? new WasteReport();
            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(wasteReportModel.WasteApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

            var timeline = rule.WasteApprovalStatusTimeline(wasteReportModel.Id, user.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return PartialView("_WasteReportContent", wasteReportModel);
        }

        public ActionResult WasteReportMaster(int id)
        {
            var user = CurrentUser();
            var accessTrader = false;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
            if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                accessTrader = true;
            ViewBag.AccessTrader = accessTrader;

            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var rule = new TraderWasteReportRules(dbContext);
            var wasteReportModel = rule.GetById(id) ?? new WasteReport();
            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(wasteReportModel.WasteApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

            var timeline = rule.WasteApprovalStatusTimeline(wasteReportModel.Id, user.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;


            return View(wasteReportModel);
        }

        public ActionResult WasteReportMasterForMovenmentTrend(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            var user = CurrentUser();
            var accessTrader = false;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
            if (userRoleRights.Any(r => r == RightPermissions.TraderAccess))
                accessTrader = true;
            ViewBag.AccessTrader = accessTrader;

            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var rule = new TraderWasteReportRules(dbContext);
            var wasteReportModel = rule.GetById(id) ?? new WasteReport();
            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(wasteReportModel.WasteApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

            var timeline = rule.WasteApprovalStatusTimeline(wasteReportModel.Id, user.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;


            return View("~/Views/TraderWasteReport/WasteReportMaster.cshtml", wasteReportModel);
        }
        public ActionResult LoadWorkGroup()
        {
            ViewBag.WorkGroupFilter =
                new TraderWasteReportRules(dbContext).GetTraderWasteReportGroupFilter(CurrentLocationManage(),
                    CurrentDomainId());

            return PartialView("_WorkGroupWasteReport");
        }
        public ActionResult TraderItemWasteReportContent()
        {
            var locationId = CurrentLocationManage();

            var workgroups = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderWasteReportProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            ViewBag.WorkgroupWasteReport = workgroups;
            ViewBag.WorkGroupFilter =
                new TraderWasteReportRules(dbContext).GetTraderWasteReportGroupFilter(CurrentLocationManage(),
                    CurrentDomainId());

            return PartialView("_TraderItemWasteReportContent");
        }
        public ActionResult GetByLocationPagination([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datetime, WasteReportStatus[] status, int[] workgroups)
        {
            var result = new TraderWasteReportRules(dbContext).GetByLocationPagination(CurrentLocationManage(), CurrentDomainId(), requestModel, CurrentUser(), keyword.ToLower(), datetime, workgroups, status);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddEditWasteReport(int id = 0)
        {
            var wasteReport = new WasteReport();

            var domain = CurrentDomain();
            var locationId = CurrentLocationManage();
            var locations = domain.TraderLocations.ToList();
            var workgroups = domain.Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderWasteReportProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            ViewBag.WorkgroupWasteReport = workgroups;
            ViewBag.Location = new TraderLocationRules(dbContext).GetById(locationId);
            ViewBag.Locations = id == 0 ? locations.Where(l => l.Id != locationId).ToList() : locations;

            if (id > 0)
            {
                wasteReport = new TraderWasteReportRules(dbContext).GetById(id);
            }
            return PartialView("_TraderItemWasteReportAddEdit", wasteReport);
        }

        public ActionResult ShowWasteReportItemUnit(int id, int locationId, string unitName)
        {
            var wasteReportItem = dbContext.TraderItems.Find(id);
            ViewBag.UnitName = unitName;
            var units = new List<UnitModel>();
            if (wasteReportItem != null)
            {
                foreach (var con in wasteReportItem.Units)
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
            return PartialView("_TraderWasteReportUnitPartial", wasteReportItem);
        }

        /// <summary>
        /// Using for case add/edit/remove item in a wasteReport existed
        /// </summary>
        /// <param name="wasteItem"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateWasteReportProduct(WasteItem wasteItem, bool isDelete)
        {
            var result = new TraderWasteReportRules(dbContext).UpdateWasteReportProduct(wasteItem, CurrentUser().Id, isDelete);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWasteReportItem([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int wasteReportId)
        {
            var locationId = CurrentLocationManage();
            var result = new TraderWasteReportRules(dbContext).GetWasteReportItem(requestModel, wasteReportId, locationId, CurrentDomainId(), CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<WasteReportItemModel>(), 0, 0), JsonRequestBehavior.AllowGet);
        }



    }
}