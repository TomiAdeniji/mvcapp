using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Returns;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderSalesReturnController : BaseController
    {
        public ActionResult TraderSaleReturnTable(SaleFilterParameter saleFilter)
        {
            var domain = CurrentDomain();
            var traderSalesReturn = new TraderSalesReturnRules(dbContext).GetByLocation(CurrentLocationManage(), domain.Id);

            ViewBag.WorkGroups = new TraderSaleRules(dbContext).GetWorkGroups(CurrentLocationManage());
            ViewBag.WorkGroupsOfMember = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage() && q.Processes.Any(p => p.Name == TraderProcessName.TraderSaleProcessName) && q.Members.Any(a => a.Id == CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.SaleFilter = saleFilter;
            return PartialView("_TraderSaleReturnTablePartial", traderSalesReturn);
        }



        public ActionResult GetDataTableSalesReturn([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int workGroupId)
        {
            var result = new TraderSalesReturnRules(dbContext).TraderSaleReturnSearch(requestModel, CurrentUser(), CurrentLocationManage(), keyword, workGroupId, CurrentUser().Timezone);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }


        public ActionResult TraderSaleReturnAdd(int traderSaleReturnId = 0)
        {
            var domain = CurrentDomain();
            var saleReturn = new TraderReturn();
            var traderReferenceForSale = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.SaleReturn);
            if (traderSaleReturnId > 0)
            {
                saleReturn = new TraderSalesReturnRules(dbContext).GetById(traderSaleReturnId);
                if (saleReturn.Reference == null)
                {
                    saleReturn.Reference = traderReferenceForSale;
                }
            }
            else
            {
                saleReturn.Reference = traderReferenceForSale;
            }

            var currentLocationId = CurrentLocationManage();
            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == currentLocationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderSaleReturnProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.TraderSaleReturnId = traderSaleReturnId;
            return PartialView("_TraderSaleReturnAddPartial", saleReturn);
        }


        /// <summary>
        /// Get sale selected to display sale transaction items for return
        /// </summary>
        /// <param name="saleId"></param>
        /// <returns></returns>
        public ActionResult TraderSaleSelected2Return(string saleKey)
        {
            var saleId = string.IsNullOrEmpty(saleKey) ? 0 : int.Parse(saleKey.Decrypt());
            var rule = new TraderSaleRules(dbContext);
            var saleModel = rule.GetById(saleId);

            return PartialView("_TraderSaleSelected2Return", saleModel);
        }


        [HttpPost]
        public ActionResult SaveTraderSaleReturn(TraderReturn traderSaleReturn)
        {
            var result = new TraderSalesReturnRules(dbContext).SaveTraderSaleReturn(traderSaleReturn, CurrentUser().Id, CurrentDomainId());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaleReturnReview(int id)
        {
            var rule = new TraderSalesReturnRules(dbContext);
            var saleReturnModel = rule.GetById(id);
            if (saleReturnModel?.ReturnApprovalProcess == null)
                return View("Error");
            var user = CurrentUser();
            var currentDomainId = saleReturnModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            ValidateCurrentDomain(saleReturnModel?.Workgroup?.Qbicle.Domain ?? CurrentDomain(), saleReturnModel.Workgroup?.Qbicle.Id ?? 0);
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(saleReturnModel.ReturnApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

            var timeline = rule.SaleReturnApprovalStatusTimeline(saleReturnModel.Id, user.Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;


            return View(saleReturnModel);

        }


        public ActionResult SaleReturnReviewContent(int id)
        {
            var rule = new TraderSalesReturnRules(dbContext);
            var saleReturnModel = rule.GetById(id);
            if (saleReturnModel?.ReturnApprovalProcess == null)
                return View("Error");

            var user = CurrentUser();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

            ValidateCurrentDomain(saleReturnModel.Workgroup?.Qbicle.Domain?? CurrentDomain(), saleReturnModel.Workgroup?.Qbicle.Id ?? 0);
            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(saleReturnModel.ReturnApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

            var timeline = rule.SaleReturnApprovalStatusTimeline(saleReturnModel.Id, user.Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;


            return PartialView("_SaleReturnReviewContent", saleReturnModel);
        }

        public ActionResult SaleReturnMaster(int id)
        {
            try
            {
                var rule = new TraderSalesReturnRules(dbContext);
                var saleReturnModel = rule.GetById(id);
                if (saleReturnModel?.ReturnApprovalProcess == null)
                    return View("Error");
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                var timeline = rule.SaleReturnApprovalStatusTimeline(saleReturnModel.Id, user.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(saleReturnModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return View("Error");
            }
        }


        [HttpPost]
        public ActionResult UpdateReturnItemQuantity(ReturnItem returnItem)
        {
            var result = new TraderSalesReturnRules(dbContext).UpdateReturnItemQuantity(returnItem, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UpdateReturnItemCredit(ReturnItem returnItem)
        {
            var result = new TraderSalesReturnRules(dbContext).UpdateReturnItemCredit(returnItem, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UpdateReturnItemIsReturnedToInventory(ReturnItem returnItem)
        {
            var result = new TraderSalesReturnRules(dbContext).UpdateReturnItemIsReturnedToInventory(returnItem, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteReturnItem(ReturnItem returnItem)
        {
            var result = new TraderSalesReturnRules(dbContext).DeleteReturnItem(returnItem, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}