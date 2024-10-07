using Newtonsoft.Json;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class OrderManagementController : BaseController
    {
        // GET: OrderManagement
        public ActionResult Index()
        {
            ViewBag.CurrentPage = "OrderManagement";

            var currentDomain = CurrentDomain();
            var lstLocation = currentDomain.TraderLocations;
            var salesChannel = Enum.GetNames(typeof(SalesChannelEnum)).ToList();
            ViewBag.Locations = lstLocation;
            ViewBag.SaleChannels = salesChannel;
            ViewBag.CurrentDomain = currentDomain;

            return View();
        }

        public ActionResult GetOrderTableContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, 
            int locationId, string saleChannelStr, string daterange, string keyword, bool isCompletedShownOnly)
        {
            var saleChannels = JsonConvert.DeserializeObject<List<SalesChannelEnum>>(saleChannelStr);
            var settings = CurrentUser();
            var dateTimeFormat = settings.DateTimeFormat;
            var dateFormat = settings.DateFormat;
            var timeZone = settings.Timezone;
            var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            return Json(new OrderManagementRules(dbContext).GetListOrderPagination(requestModel, locationId, saleChannels, daterange,
                keyword, isCompletedShownOnly, dateFormat, dateTimeFormat, timeZone, currencySetting), JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePrepQueueOrderStatus(string lstOrderIdsStr, PrepQueueStatus upcomingStatus, string problemDescription)
        {
            var lstOrderIds = JsonConvert.DeserializeObject<List<int>>(lstOrderIdsStr);
            var currentUserId = CurrentUser().Id;
            var updateResult = new OrderManagementRules(dbContext).UpdatePrepQueueStatus(lstOrderIds, currentUserId, upcomingStatus, problemDescription);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }

    }
}