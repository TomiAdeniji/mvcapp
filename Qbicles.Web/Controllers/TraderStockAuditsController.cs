using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderStockAuditsController : BaseController
    {
        // GET: StockAudit
        [HttpPost]
        public ActionResult SaveStockAudit(StockAudit stockAudit)
        {
            stockAudit.Domain = CurrentDomain();
            var result = new ReturnJsonModel { actionVal = 3, msgId = "0" };
            // check name;
            if (new TraderStockAuditRules(dbContext).ExistsName(stockAudit))
                result.msg = ResourcesManager._L("ERROR_MSG_631", stockAudit.Name);
            else
                result = new TraderStockAuditRules(dbContext).SaveStockAudit(stockAudit, CurrentUser().Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult StartStockAudit(StockAudit stockAudit)
        {
            stockAudit.Domain = CurrentDomain();
            var result = new TraderStockAuditRules(dbContext).SaveStockAudit(stockAudit, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult FinishStockAudit(StockAudit stockAudit, string status = "")
        {
            var result = new TraderStockAuditRules(dbContext).FinishStockAudit(stockAudit, status, CurrentDomainId(), CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InventoryAuditTab(int locationId = 0)
        {
            ViewBag.WorkgroupStockAudit = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.ShiftAudits))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            return PartialView("_TraderItemInventoryAuditTab");
        }

        public ActionResult ListStockAudit([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int workgroupId)
        {
            var itemImports = new TraderStockAuditRules(dbContext).
                GetStockAuditServerSide(requestModel, keyword.ToLower(), workgroupId, CurrentLocationManage(), CurrentUser());
            if (itemImports != null)
                return Json(itemImports, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        //public ActionResult ListStockAudit(int locationId, bool callback = false)
        //{
        //    var traderItems = new TraderStockAuditRules(dbContext).GetByLocationId(locationId, CurrentDomainId());
        //    ViewBag.CallBack = callback;
        //    return PartialView("_TraderListStockAudit", traderItems);
        //}

        public ActionResult AddStockAudit(int id = 0, string view = "")
        {
            var stockRule = new TraderStockAuditRules(dbContext);
            var spotAudit = new StockAudit();
            try
            {
                ViewBag.ViewAudit = view;
                var locationId = CurrentLocationManage();

                ViewBag.WorkgroupStockAudit = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == locationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.ShiftAudits))
                    && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

                if (id > 0)
                {
                    spotAudit = stockRule.GetById(id);
                }
                return PartialView("_TraderItemAddEditStockAudit", spotAudit);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), e);
                return View("Error");
            }
        }

        public ActionResult GetItemByWorkGroupId(int wgId = 0, int locationId = 0)
        {
            var items = new TraderStockAuditRules(dbContext).GetTraderItem2StockAudit(CurrentDomainId(), locationId, wgId);
            //var itemWorkGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), locationId);
            return PartialView("_TraderProductGroupSelected", items);
        }

        public ActionResult ShiftAuditMaster(int id)
        {
            try
            {
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                var stockAuditModel = new TraderStockAuditRules(dbContext).GetStockAuditModel(id, CurrentUser(), CurrentDomainId());

                var timeline = new TraderStockAuditRules(dbContext).ShiftAuditApprovalStatusTimeline(stockAuditModel.Id, user.Timezone).OrderByDescending(q => q.LogDate).ToList();
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();
                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(stockAuditModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateStockitem(int id, decimal closingCount)
        {
            var result = new TraderStockAuditRules(dbContext).UpdateStockAuditItem(id, closingCount);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShiftAuditReview(int id, bool content = false)
        {
            try
            {
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");
                var rule = new TraderStockAuditRules(dbContext);
                var stockAuditModel = rule.GetById(id);
                if (!content)
                {
                    ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                    SetCurrentApprovalIdCookies(stockAuditModel.StockAuditApproval?.Id ?? 0);
                    return View(stockAuditModel ?? new StockAudit());
                }
                if (stockAuditModel != null)
                    ValidateCurrentDomain(stockAuditModel.WorkGroup.Qbicle.Domain, stockAuditModel.WorkGroup?.Qbicle.Id ?? 0);
                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(stockAuditModel?.StockAuditApproval.ApprovalRequestDefinition?.Id ?? 0, user.Id);

                var timeline = new TraderStockAuditRules(dbContext).ShiftAuditApprovalStatusTimeline(stockAuditModel.Id, user.Timezone).OrderByDescending(q => q.LogDate).ToList();
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();
                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;
                ViewBag.CurrentUserAvatar = user.ProfilePic;

                ViewBag.StockAudit = rule.GetStockAuditModel(id, user, CurrentDomainId(), stockAuditModel);

                stockAuditModel.ProductList = null;

                return PartialView("_ShiftAuditReviewContent", stockAuditModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }
    }
}