using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.TraderApi;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderReportsController : BaseController
    {
        // GET: TraderReport
        public ActionResult LoadDefaultReportTab()
        {
            return PartialView("_LoadDefaultReportTab");
        }

        [HttpPost]
        public ActionResult GenarateReportTimeFrame(string daterange)
        {
            SetCurrentCurrentDateRangeCookies(daterange);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ReportPosOrders()
        {
            var pageRefreshTime = GetRefreshPageTime();
            SetPageRefreshTime(pageRefreshTime);
            ViewBag.pageRefreshTime = pageRefreshTime;

            string dateToDate = CurrentDateRange();
            ViewBag.TitleDate = dateToDate.Replace("-", "to");
            return View();
        }

        public ActionResult UpdatePageRefreshTime(int refreshTime)
        {
            SetPageRefreshTime(refreshTime);
            return Json(new ReturnJsonModel() { result = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PosPayment()
        {
            ViewBag.SourceItems = new PosSaleOrderRules(dbContext).GetSourceItems(CurrentDomainId());
            return View();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="requestModel"></param>
        /// <param name="keyword"></param>
        /// <param name="datelimit"></param>
        /// <returns></returns>
        public ActionResult TraderPosPaymentDataTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datelimit)
        {
            int[] locations = Request.Params["locations[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
            int[] methods = Request.Params["methods[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
            int[] accounts = Request.Params["accounts[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
            string[] cashiers = Request.Params["cashiers[]"]?.Split(',');
            int[] devices = Request.Params["devices[]"]?.Split(',').Select(s => Int32.Parse(s)).ToArray();
            //string dateFormat = CurrentUser().DateFormat;
            //string timeFormat = CurrentUser().TimeFormat;
            keyword = keyword.Trim().ToLower();
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            var result = new PosSaleOrderRules(dbContext).GetPosPaymentDataTable(requestModel, CurrentDomainId(), currencySettings, keyword, datelimit, locations, methods, accounts, cashiers, devices, CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<OrderQueueCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult TraderOrderHistoryDataTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, bool isCompleteShown)
        {
            string dateToDate = CurrentDateRange();
            var result = new PosSaleOrderRules(dbContext).GetTraderOrderHistoryDataTable(requestModel, CurrentLocationManage(), dateToDate, CurrentUser().DateFormat, isCompleteShown);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<OrderQueueCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddDiscussion(int id)
        {
            var domain = CurrentDomain();
            ViewBag.DomainUsers = domain.Users;
            var locationId = CurrentLocationManage();
            var queueOrder = new PosSaleOrderRules(dbContext).GetQueueOrderById(id);
            if (queueOrder.Discussion == null)
            {
                queueOrder.Discussion = new QbicleDiscussion();
            }
            var topics = new TopicRules(dbContext).GetTopicByQbicle(CurrentQbicleId());
            ViewBag.Topics = topics;
            if (queueOrder.Discussion.Topic == null)
            {
                queueOrder.Discussion.Topic = dbContext.PosSettings.FirstOrDefault(q => q.Location.Id == locationId)?.DefaultWorkGroup.Topic ?? topics.FirstOrDefault();
            }
            return PartialView("_AddDiscussion", queueOrder);
        }

        [HttpPost]
        public ActionResult SaveDiscussion(QueueOrder queueOrder)
        {
            var result = new PosSaleOrderRules(dbContext).SaveDiscussionFromTraderReport(queueOrder, CurrentUser().Id, CurrentQbicleId());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowOrderSummary(int queueOrderId)
        {
            var queue = dbContext.QueueOrders.Find(queueOrderId);
            ViewBag.IsPrepared = true;
            return PartialView("_ShowOrderSummary", queue);
        }

        // Method on Order Management page
        public ActionResult ShowQueueOrderSummary(int queueOrderId)
        {
            var queue = dbContext.QueueOrders.Find(queueOrderId);

            var tradeOrder = dbContext.TradeOrders.FirstOrDefault(e => e.LinkedOrderId == queue.LinkedOrderId);
            if (tradeOrder != null)
                ViewBag.Items = tradeOrder.OrderJson.ParseAs<Order>().Items;
            else
                ViewBag.Items = new List<Item>();
            ViewBag.IsPrepared = false;
            ViewBag.ShowIsNotForPrep = false;
            return PartialView("_ShowOrderSummary", queue);
        }
    }
}