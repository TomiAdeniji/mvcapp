using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Dds;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.Models;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class DeliveryController : BaseController
    {
        // Delivery Management
        public ActionResult Index()
        {
            var currentUserId = CurrentUser().Id;
            var domain = CurrentDomain();
            var user = CurrentUser();

            if (!CanAccessBusiness() || !Utility.CheckBusinessUserRole(domain.Id, currentUserId))
                return View("ErrorAccessPage");

            ViewBag.CurrentPage = SystemPageConst.DELIVERYMANAGEMENT;
            this.SetCookieGoBackPage(SystemPageConst.DELIVERYMANAGEMENT);

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);

            var deliveryRules = new DeliveryManagementRules(dbContext);

            var locations = deliveryRules.GetDeliveryLocations(domain.Id);
            var drivers = deliveryRules.GetDriversByDomain(domain.Id);

            var status = EnumModel.ConvertEnumToList<DeliveryStatus>();

            var today = DateTime.UtcNow.ConvertTimeFromUtc(timeZone);
            var dateStart = today.Date.ToString(user.DateFormat + " 00:00");
            var dateEnd = today.Date.ToString(user.DateFormat + " 23:59");

            var filterInit = GetDeliveryManagementFilterCookies($"Delivery-Management-{currentUserId}-{domain.Id}");

            if (filterInit == null)
            {
                filterInit = new DeliveryParameter
                {
                    DateStart = dateStart,
                    DateEnd = dateEnd,
                    Drivers = drivers.Select(d => d.Id).ToList(),
                    Keyword = "",
                    RefreshEvery = 60,
                    ShowCompleted = false,
                    Locations = locations.Select(l => l.Id).ToList(),
                    Status = status.Select(e => (DeliveryStatus)e.Key).ToList(),
                    Columns = new List<string> { "2", "3", "4", "5" }
                };

                ViewBag.DateStart = dateStart;
                ViewBag.DateEnd = dateEnd;

                SetDeliveryManagementFilterCookies($"Delivery-Management-{currentUserId}-{domain.Id}", filterInit);
            }
            else
            {                
                try
                {
                    if (!string.IsNullOrEmpty(filterInit.DateStart))
                        try
                        {
                            ViewBag.DateStart = DateTime.ParseExact(filterInit.DateStart, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm");
                        }
                        catch
                        {
                            ViewBag.DateStart = DateTime.ParseExact(filterInit.DateStart, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm");
                        }
                    else
                        ViewBag.DateStart = "";

                    if (!string.IsNullOrEmpty(filterInit.DateEnd))
                        try
                        {
                            ViewBag.DateEnd = DateTime.ParseExact(filterInit.DateEnd, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm");
                        }
                        catch
                        {
                            ViewBag.DateEnd = DateTime.ParseExact(filterInit.DateEnd, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm");
                        }
                    else
                        ViewBag.DateEnd = "";
                }
                catch
                {
                    ViewBag.DateStart = "";
                    ViewBag.DateEnd = "";
                }

            }

            ViewBag.FilterInit = filterInit;

            ViewBag.Locations = locations;
            ViewBag.Drivers = drivers;
            ViewBag.Status = status;
            ViewBag.DomainName = domain.Name;

            //config
            var traderLocations = dbContext.TraderLocations.Where(e => e.Domain.Id == domain.Id).OrderBy(o => o.Name).ToList();
            var firstLocationId = traderLocations.FirstOrDefault()?.Id ?? 0;

            ViewBag.DdsSetting = new PosSettingRules(dbContext).GetByLocation(firstLocationId, CurrentUser().Id, CurrentUser().Timezone);
            ViewBag.CurrentLocationManage = firstLocationId;

            var rule = new DdsRules(dbContext);
            ViewBag.DdsLocationsQueue = rule.GetDdsLocationsQueue(domain.Id);
            ViewBag.DDsDrivers = rule.GetDdsDriversByDomain(domain.Id, false);

            ViewBag.DdsLocations = traderLocations;
            return View();
        }


        public ActionResult GetDeliveries([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            var parameter = GetDeliveryManagementFilterCookies($"Delivery-Management-{CurrentUser().Id}-{CurrentDomainId()}");
            var deliveries = new DeliveryManagementRules(dbContext).GetDeliveries(requestModel, CurrentDomainId(), parameter, CurrentUser());
            if (deliveries != null)
                return Json(deliveries, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<object>(), 0, 0), JsonRequestBehavior.AllowGet);
        }


        public ActionResult CreateDelivery(int locationId)
        {
            new DdsApiRules(dbContext).CreateDelivery(CurrentUser().Id, locationId);
            return Json(new { Response = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RememberedFilter(DeliveryParameter parameter)
        {
            SetDeliveryManagementFilterCookies($"Delivery-Management-{CurrentUser().Id}-{CurrentDomainId()}", parameter);
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDeliveryStatus(string deliveryIds, DeliveryStatus upcomingStatus, string problemDescription)
        {
            var lstDeliveryIds = Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(deliveryIds);
            var currentUserId = CurrentUser().Id;
            var updateResult = new DeliveryManagementRules(dbContext).UpdateDeliveryStatus(lstDeliveryIds, currentUserId, upcomingStatus, problemDescription);
            return Json(updateResult, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddDeliveryDiscussion(int deliveryId)
        {
            var domain = CurrentDomain();

            ViewBag.DomainUsers = domain.Users;
            var delivery = new DeliveryManagementRules(dbContext).GetDelivery(deliveryId);
            var locationId = delivery.DeliveryQueue.Location.Id;

            var setting = dbContext.PosSettings.FirstOrDefault(q => q.Location.Id == locationId);

            var topics = new TopicRules(dbContext).GetTopicByQbicle(setting.DefaultWorkGroup?.Qbicle?.Id ?? 0);

            if (delivery.Discussion == null)
            {
                delivery.Discussion = new QbicleDiscussion
                {
                    Topic = setting.DefaultWorkGroup?.Topic ?? topics.FirstOrDefault()
                };
            }
            ViewBag.Topics = topics;

            return PartialView("_AddDeliveryDiscussion", delivery);
        }

        [HttpPost]
        public ActionResult SaveDeliveryDiscussion(Delivery delivery)
        {
            var result = new DeliveryManagementRules(dbContext).SaveDeliveryDiscussion(delivery, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ShowDeliveryOrder(int deliveryId)
        {
            var delivery = dbContext.Deliveries.FirstOrDefault(d => d.Id == deliveryId);
            return PartialView("_ShowDeliveryOrder", delivery);
        }

        //Management an Order
        public ActionResult Management(string deliveryKey)
        {
            var delivery = new DeliveryManagementRules(dbContext).GetDelivery(int.Parse(deliveryKey.Decrypt()));

            var locationId = delivery.DeliveryQueue?.Location.Id ?? 0;
            var posSetting = dbContext.PosSettings.AsNoTracking().FirstOrDefault(e => e.Location.Id == locationId);
            ViewBag.TitleStr = $"Delivery Management - Delivery #{delivery.Reference?.FullRef ?? delivery.Id.ToString()}";
            ViewBag.DeliveryDisplayRefreshInterval = posSetting?.DeliveryDisplayRefreshInterval ?? 0;
            ViewBag.CanRefreshDelivery = delivery.Status.GetId() < 4;
            ViewBag.DeliveryKey = deliveryKey;
            return View();
        }

        public ActionResult ManagementPartial(string deliveryKey)
        {
            var delivery = new DeliveryManagementRules(dbContext).GetDelivery(int.Parse(deliveryKey.Decrypt()));

            var driver = dbContext.DriverLogs.OrderByDescending(d => d.CreatedDate).FirstOrDefault(e => e.Delivery.Id == delivery.Id)?.Driver;
            if (driver == null)
                driver = delivery.Driver ?? delivery.DriverArchived;
            ViewBag.Driver = driver;
            return PartialView("_Management", delivery);
        }


        /// <summary>
        /// Get orders in delivery
        /// </summary>
        /// <param name="deliveryKey"></param>
        /// <returns></returns>
        public ActionResult GetDeliveryOrders(string deliveryKey)
        {
            var delivery = new DeliveryManagementRules(dbContext).GetDelivery(int.Parse(deliveryKey.Decrypt()));

            var driver = dbContext.DriverLogs.OrderByDescending(d => d.CreatedDate).FirstOrDefault(e => e.Delivery.Id == delivery.Id)?.Driver;
            if (driver == null)
                driver = delivery.Driver ?? delivery.DriverArchived;
            ViewBag.Driver = driver;
            return PartialView("_DeliveryOrders", delivery);
        }

        public ActionResult GetOrderInfo(string key)
        {
            var order = new DeliveryManagementRules(dbContext).GetOrder(int.Parse(key.Decrypt()));
            var tradeOrderRef = dbContext.TradeOrders.FirstOrDefault(e => e.LinkedOrderId == order.LinkedOrderId)?.OrderReference?.FullRef;

            var orderRef = order.OrderRef;
            if (!string.IsNullOrEmpty(tradeOrderRef))
                orderRef += $"/{tradeOrderRef}";

            ViewBag.OrderRef = orderRef;

            return PartialView("_OrderInfo", order);
        }

        //open modal dialog
        public ActionResult OpenOrderListModal()
        {
            return PartialView("_OrderList");
        }

        /// <summary>
        /// Get Orders available add to Delivery
        /// </summary>
        /// <param name="request"></param>
        /// <param name="deliveryKey">current delivery key</param>
        /// <returns></returns>
        public ActionResult GetOrdersAvailable(PaginationRequest request, string deliveryKey)
        {
            var prepQueue = new DeliveryManagementRules(dbContext).GetOrdersForDeliveryWeb(request, deliveryKey, CurrentDomainId(), CurrentUser().Id);
            return Json(prepQueue, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDriverInfo(string key, bool inDelivery)
        {
            var driverId = int.Parse(key.Decrypt());
            var driver = dbContext.Drivers.FirstOrDefault(d => d.Id == driverId);
            ViewBag.InDelivery = inDelivery;
            return PartialView("_DriverInfo", driver);
        }

        //open modal dialog
        public ActionResult OpenDriverListModal(string deliveryKey)
        {
            ViewBag.DeliveryKey = deliveryKey;
            return PartialView("_DriverList");
        }

        /// <summary>
        /// driver list server side - if existed retun only- else return page size, 
        /// input filter -> return page size, 
        /// change -> reload, assign -> reload
        /// </summary>
        /// <param name="request"></param>
        /// <param name="deliveryKey"></param>
        /// <returns></returns>
        public ActionResult GetDriverDelivery(PaginationRequest request, string deliveryKey)
        {
            var drivers = new DeliveryManagementRules(dbContext).GetDriverDeliveryWeb(request, deliveryKey);
            return Json(drivers, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Update delivery when add/remove order
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        public ActionResult DeliveryUpdate(DsDelivery delivery)
        {
            var request = new PosRequest { UserId = CurrentUser().Id, Status = System.Net.HttpStatusCode.OK };
            var result = new DdsApiRules(dbContext).DeliveryUpdate(request, delivery, true);

            if (result.result)
            {
                var currencySetting = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());

                var deliveryDb = (Delivery)result.Object2;

                var completed = deliveryDb.Orders.Count(e => e.Status == Qbicles.Models.Trader.ODS.PrepQueueStatus.Completed || e.Status == Qbicles.Models.Trader.ODS.PrepQueueStatus.CompletedWithProblems);
                var oComplete = $"{completed}/{deliveryDb.Orders.Count} complete";

                var duration = TimeSpan.FromSeconds(deliveryDb.EstimateTime ?? 0);
                var hourTxt = duration.Hours == 1 ? "hour" : "hours";
                var minuteTxt = duration.Minutes == 1 ? "minute" : "minutes";

                var deliveryInfo = new
                {
                    orderInfo = $"{deliveryDb.Orders.Count()} stops {oComplete}",
                    orderTotal = deliveryDb.Total.ToCurrencySymbol(currencySetting),
                    driverInfo = deliveryDb.Driver?.User.User.GetFullName() ?? "Unassigned",
                    driverStatus = deliveryDb.Driver?.DeliveryStatus.GetDescription(),
                    driverStatusCss = deliveryDb.Driver?.DeliveryStatus.GetClass(),
                    duration = $"{duration.Hours} {hourTxt} {duration.Minutes} {minuteTxt}",
                    distance = $"{((decimal)(deliveryDb.EstimateDistance ?? 0) / 1000).ToDecimalPlace(currencySetting)} km"
                };

                result.Object2 = deliveryInfo;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add/remove driver from Delivery
        /// </summary>
        /// <param name="delivery"></param>
        /// <returns></returns>
        public ActionResult DeliveryDriverChange(DeliveryDriverParameter delivery)
        {
            IPosResult result;
            var request = new PosRequest { UserId = CurrentUser().Id };
            if (!delivery.IsDelete)
                result = new DdsApiRules(dbContext).DriverAdd(request, delivery, true);
            else
                result = new DdsApiRules(dbContext).DriverRemove(request, delivery, true);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMessageDriverToCustomer(OrderMessageModel message)
        {
            new PosRules(dbContext).OrderComment(CurrentUser().Id, message);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Driver update status delivery on Accept/Reject delivery, change status new->start,... change order status
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public ActionResult DriverUpdateStatusDelivery(DsDeliveryParameter info)
        {
            var request = new PosRequest { UserId = CurrentUser().Id, Status = System.Net.HttpStatusCode.OK };
            var updateStatusDelivery = new DeliveryDriverApiRules(dbContext).DriverUpdateStatusDelivery(request, info, true);
            return Json(updateStatusDelivery, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadModalDeliveryDriver(string keyword)
        {
            var model = new DdsRules(dbContext).GetDomainUserNotDriver(CurrentDomainId(), keyword);
            return PartialView(@"~\Views\TraderChannels\_ModalDeliveryDriver.cshtml", model);
        }

        public ActionResult LoadContentMemberDetail(string userId, int locationDefaultId)
        {
            var domainId = CurrentDomainId();
            ViewBag.Locations = dbContext.TraderLocations.Where(e => e.Domain.Id == domainId).ToList();
            var deviceUser = new Qbicles.Models.Trader.PoS.DeviceUser
            {
                Id = 0,
                User = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId)
            };
            ViewBag.LocationDefaultId = locationDefaultId;
            return PartialView(@"~\Views\TraderChannels\_ContentMemberDetail.cshtml", deviceUser);
        }
    }
}