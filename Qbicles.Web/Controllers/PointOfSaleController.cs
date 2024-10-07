using Qbicles.BusinessRules;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Catalogs;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Qbicles.BusinessRules.HelperClass;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class PointOfSaleController : BaseController
    {
        public ActionResult PointOfSaleContent(string value)
        {
            var domainId = CurrentDomainId();
            var locationId = CurrentLocationManage();
            var user = CurrentUser();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            ViewBag.UserRoleRights = userRoleRights;

            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
            var today = DateTime.UtcNow.ConvertTimeFromUtc(timeZone);

            switch (value)
            {
                case "General":
                case "GeneralChannel":
                    var setting = new PosSettingRules(dbContext).GetByLocation(locationId, CurrentUser().Id, CurrentUser().Timezone);
                    var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(domainId)
                        .Where(q => q.Processes.Any(p => p.Name == "Point of Sale") && q.Location.Id == locationId && q.Members.Any(m => m.Id == user.Id)).OrderBy(n => n.Name).ToList();
                    var traderContacts = new TraderContactRules(dbContext).GetByDomainId(domainId);
                    ViewBag.Workgroups = workGroups;
                    ViewBag.Contacts = traderContacts;
                    if (string.IsNullOrEmpty(setting.ProductPlaceholderImage))
                        setting.ProductPlaceholderImage = ConfigManager.DefaultProductPlaceholderImageUrl;
                    ViewBag.SettingType = value;
                    return PartialView("_POSGeneral", setting ?? new PosSettings());
                case "Users":
                    var users = CurrentDomain().Users;
                    ViewBag.PosUsers = new DeviceUserRules(dbContext).GetAll(domainId);
                    return PartialView("_POSUsers", users);
                case "Printers":
                    return PartialView("_POSPrinters");
                case "OrderType":
                    return PartialView("_OrderType");
                case "DeviceType":
                    ViewBag.FilterOrderTypes = new PosSaleOrderRules(dbContext).GetFilterOrderTypeInLocation(locationId);
                    return PartialView("_DeviceType");
                case "Devices":
                    ViewBag.Location = new TraderLocationRules(dbContext).GetById(locationId);
                    var devices = new PosDeviceRules(dbContext).GetByLocation(locationId).ToList();
                    ViewBag.PrepQueue = new PDSRules(dbContext).GetPrepQueueByLocation(locationId);
                    return PartialView("_POSDevices", devices);
                case "Products":
                    var dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domainId);
                    ViewBag.Dimensions = dimensions == null ? new List<TransactionDimension>() : dimensions;
                    ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                    this.SetCookieGoBackPage("Trader");
                    return PartialView("_POSProducts");
                case "Loyalty":
                    return PartialView("_POSLoyalty");
                case "Tables":
                    var tableRules = new PosTableRules(dbContext);
                    
                    var listPosTables = tableRules.GetListPosTableByLocation(locationId);
                    var posTableLayout = tableRules.GetPosTableLayoutByLocation(locationId);
                    ViewBag.ListPosTables = listPosTables;
                    ViewBag.PosTableLayout = posTableLayout;
                    ViewBag.CurrentLocationId = locationId;
                    ViewBag.TimeZone = CurrentUser().Timezone;
                    ViewBag.DateFormat = CurrentUser().DateFormat;

                    var imageType = new FileTypeRules(dbContext).GetExtensionsByType("Image File");
                    var listImgAcceptedExtension = "." + string.Join(",.", imageType);
                    ViewBag.AcceptedImgExtensions = listImgAcceptedExtension;

                    return PartialView("_POSTables");
                case "Cancellations":
                    //ViewBag.Location = new TraderLocationRules(dbContext).GetById(locationId);
                    //var devices1 = new PosDeviceRules(dbContext).GetByLocation(locationId).ToList();
                    //ViewBag.PrepQueue = new PDSRules(dbContext).GetPrepQueueByLocation(locationId);
                    //return PartialView("_POSCancellations", devices1);

                    ViewBag.FromDateTime = today.Date.ToString("yyyy-MM-ddT00:00:00");
                    ViewBag.ToDateTime = today.Date.ToString("yyyy-MM-ddT23:59:59");
                    ViewBag.Users = new UserRules(dbContext).GetUserByDomain(CurrentDomainId());
                    return PartialView("_POSCancellations");
                case "PrintChecks":                    
                    ViewBag.FromDateTime = today.Date.ToString("yyyy-MM-ddT00:00:00");
                    ViewBag.ToDateTime = today.Date.ToString("yyyy-MM-ddT23:59:59");
                    ViewBag.Users = new UserRules(dbContext).GetUserByDomain(CurrentDomainId());
                    ViewBag.Devices = new PosDeviceRules(dbContext).GetByLocation(locationId);
                    return PartialView("_POSPrintCheck");
            }

            return null;
        }
        public ActionResult AddEditPosDevice(int id)
        {
            var posDevice = new PosDevice();
            if (id > 0)
            {
                posDevice = new PosDeviceRules(dbContext).GetById(id);
            }
            var locationId = CurrentLocationManage();
            ViewBag.PosDeviceTypes = new PosSaleOrderRules(dbContext).GetFilterDeviceTypeInLocation(locationId);
            ViewBag.Location = new TraderLocationRules(dbContext).GetById(locationId);
            ViewBag.PrepQueue = new PDSRules(dbContext).GetPrepQueueByLocation(locationId);
            return PartialView("_AddEditDevice", posDevice);
        }
        public ActionResult PoSDevice(int id)
        {
            var location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            ViewBag.Location = location;
            ViewBag.PrepQueue = new PDSRules(dbContext).GetPrepQueueByLocation(location.Id);
            ViewBag.PosMenu = new PosMenuRules(dbContext).GetByLocation(location.Id);
            ViewBag.UserType = EnumModel.GetEnumValuesAndDescriptions<PosUserType>();
            var posUsers = new DeviceUserRules(dbContext).GetDeviceUsersByDomain(CurrentDomainId(), id);

            ViewBag.PosUsersList = posUsers.Distinct().Where(e => e.Types.Count > 0).ToList();

            var usersModal = posUsers.Where(e => e.Types.Count == 0).ToList();

            ViewBag.PosUsersModal = usersModal;
            ViewBag.PosUsersGroupModal = usersModal.Select(e => e.ForenameGroup).Distinct().OrderBy(o => o).ToList();

            ViewBag.PosDeviceTypes = new PosSaleOrderRules(dbContext).GetFilterDeviceTypeInLocation(location.Id);

            var paymentMethods = new PosDeviceRules(dbContext).GetPaymentMethods();
            var accounts = new PosDeviceRules(dbContext).GetAccounts(CurrentDomainId());
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.Accounts = accounts;

            var safeByLocation = new CMsRules(dbContext).GetSafeByLocation(location.Id);
            ViewBag.Safe = safeByLocation;

            var model = new PosDeviceRules(dbContext).GetById(id);
            return View(model);

        }

        public ActionResult PoSMenu(int id)
        {
            var model = new PosMenuRules(dbContext).GetById(id);
            var dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            if (dimensions == null)
                ViewBag.Dimensions = new List<TransactionDimension>();
            else ViewBag.Dimensions = dimensions;
            ViewBag.GoBackPage = CurrentGoBackPage();
            ViewBag.CurrencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomain().Id); ;
            return View(model);
        }

        public ActionResult ShowCatalogPriceTab(int catalogId)
        {
            var model = new PosMenuRules(dbContext).GetById(catalogId);
            ViewBag.CurrencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomain().Id); ;
            return PartialView("_CatalogPriceTabContent", model);
        }

        public ActionResult LoadCatalogItemTableContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int catalogId, string keySearch, int categoryIdSearch,
            int start, int length, int draw)
        {
            var totalRecord = 0;
            var itemCustomList = new PosMenuRules(dbContext).GetCatalogItemsPagination(catalogId, keySearch, categoryIdSearch, ref totalRecord, requestModel, start, length);
            var dtModel = new DataTableModel()
            {
                data = itemCustomList,
                draw = draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };
            return Json(dtModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveOrderDisplayRefreshSetting(PosSettings setting)
        {
            var result = new PosSettingRules(dbContext).SaveOrderDisplayRefreshSetting(setting);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveDeliveryDisplayRefreshSetting(PosSettings setting)
        {
            var result = new PosSettingRules(dbContext).SaveDeliveryDisplayRefreshSetting(setting);
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult SaveDeliveryLingerTime(PosSettings setting)
        {
            var result = new PosSettingRules(dbContext).SaveDeliveryLingerTime(setting);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveDeliveryThresholdTimeInterval(PosSettings setting)
        {
            var result = new PosSettingRules(dbContext).SaveDeliveryThresholdTimeInterval(setting);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveSpeedDistance(PosSettings setting)
        {
            var result = new PosSettingRules(dbContext).SaveSpeedDistance(setting);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveSetting(PosSettings posSetting)
        {
            var refModel = new PosSettingRules(dbContext).SaveSetting(posSetting, CurrentUser().Timezone);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveDimensions(Catalog posMenu)
        {
            var result = new PosMenuRules(dbContext).SaveDimension(posMenu);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CreatePaymentMethod(PosDevice posDevice)
        {
            var result = new PosDeviceRules(dbContext).SavePaymentMethod(posDevice, CurrentUser().Id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult UpdatePaymentMethod(PosDevice posDevice)
        {
            var result = new PosDeviceRules(dbContext).SavePaymentMethod(posDevice, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeletePaymentMethod(PosDevice posDevice)
        {
            var result = new PosDeviceRules(dbContext).DeletePaymentMethod(posDevice);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetPaymentAccountTable(int idDevice)
        {
            var paymentMethods = new PosDeviceRules(dbContext).GetPaymentMethods();
            var accounts = new PosDeviceRules(dbContext).GetAccounts(CurrentDomainId());
            ViewBag.PaymentMethods = paymentMethods;
            ViewBag.Accounts = accounts;
            var device = new PosDeviceRules(dbContext).GetById(idDevice);
            var safeByLocation = new CMsRules(dbContext).GetSafeByLocation(CurrentLocationManage());
            ViewBag.SafeAccountId = safeByLocation?.CashAndBankAccount.Id ?? -1;
            return PartialView("_PosDeviceMethodAccounts", device);

        }
        public ActionResult GetDimensionManager(int idMenu)
        {
            var model = new PosMenuRules(dbContext).GetById(idMenu);
            var dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            if (dimensions == null)
                ViewBag.Dimensions = new List<TransactionDimension>();
            else ViewBag.Dimensions = dimensions;
            return PartialView("_PosManagerDimension", model);

        }

        // OrderType
        public ActionResult TraderPosOrderTypetDataTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword)
        {
            keyword = keyword.Trim().ToLower();
            var result = new PosSaleOrderRules(dbContext).GetPosOrderTypeDataTable(requestModel, keyword, CurrentLocationManage());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<PosOrderTypeCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddEditOrderType(int id = 0)
        {
            var model = new PosOrderType();
            if (id > 0)
            {
                model = new PosSaleOrderRules(dbContext).getOrderTypeById(id);
            }
            return PartialView("_AddEditOrderType", model);
        }
        [HttpPost]
        public ActionResult SaveOrderType(PosOrderType orderType)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 1
            };
            try
            {
                orderType.Location = new TraderLocation() { Id = CurrentLocationManage() };
                result = new PosSaleOrderRules(dbContext).SavePosOrderType(orderType, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpDelete]
        public ActionResult DeletePosOrderType(int id)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 1
            };
            if (new PosSaleOrderRules(dbContext).DeletePosOrderType(id))
            {
                result.actionVal = 1;
            }
            else
            {
                result.actionVal = 3;
                result.msg = "Delete faild.";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // DeviceType
        public ActionResult TraderPosDeviceTypetDataTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string orderTypeId)
        {
            keyword = keyword.Trim().ToLower();
            var result = new PosSaleOrderRules(dbContext).GetPosDeviceTypeDataTable(requestModel, keyword, CurrentLocationManage(), orderTypeId);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<PosDeviceTypeCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddEditDeviceType(int id = 0)
        {
            var model = new PosDeviceType();
            if (id > 0)
            {
                model = new PosSaleOrderRules(dbContext).getDeviceTypeById(id);
            }
            ViewBag.posOrderTypes = new PosSaleOrderRules(dbContext).GetOrderTypeInLocation(CurrentLocationManage());
            return PartialView("_AddEditDeviceType", model);
        }
        [HttpPost]
        public ActionResult SaveDeviceType(PosDeviceType deviceType)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 1
            };
            try
            {
                deviceType.Location = new TraderLocation() { Id = CurrentLocationManage() };
                result = new PosSaleOrderRules(dbContext).SavePosDeviceType(deviceType, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpDelete]
        public ActionResult DeletePosDeviceType(int id)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 1
            };
            if (new PosSaleOrderRules(dbContext).DeletePosDeviceType(id))
            {
                result.actionVal = 1;
            }
            else
            {
                result.actionVal = 3;
                result.msg = "Delete faild.";
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CatalogueQuickAddShow(CatalogType type = CatalogType.Sales)
        {
            ViewBag.lstTraderGroups = new PosMenuRules(dbContext).GetListTraderGroupByDomain(CurrentDomainId());
            ViewBag.catalogType = type;
            ViewBag.lstDimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            ViewBag.lstLocation = new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId());
            return PartialView("_CatalogueQuickAdd");
        }

        public ActionResult GetTraderGroupByLocation(int locaionId)
        {
            var lstGroups = new PosMenuRules(dbContext).GetListTraderGroupByLocation(locaionId);
            return Json(lstGroups.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListPriceItem([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string catalogKey,
            string traderItemSKU = "", List<int> lstCategoryId = null, string categoryItemNameKeySearch = "", string variantOrExtraName = "", int locationId = 0, int taxupdate = 0, int lastcostupdate = 0)
        {
            var catalogId = string.IsNullOrEmpty(catalogKey) ? 0 : int.Parse(catalogKey.Decrypt());

            var result = new PosMenuRules(dbContext)
                .GetPriceItemTableData(requestModel, catalogId, traderItemSKU, lstCategoryId, categoryItemNameKeySearch, variantOrExtraName, locationId, taxupdate, lastcostupdate, CurrentDomainId());
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
    }
}