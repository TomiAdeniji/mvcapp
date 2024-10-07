using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.B2B;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.MyBankMate;
using Qbicles.Models.Trader.DDS;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderChannelsController : BaseController
    {
        public ActionResult SaveB2BConfig(B2bConfigModel model)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }

            model.LocationId = CurrentLocationManage();
            return Json(new CommerceRules(dbContext).SaveB2BConfig(model, CurrentUser().Id));
        }
        public ActionResult LoadModalPriceList(int id = 0)
        {
            var pricelist = new B2BPricelistRules(dbContext).PricelistById(id);
            if (CurrentLocationManage() == 0)
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId());
            return PartialView("_ModalB2bPriceList", pricelist != null ? pricelist : new PriceList());
        }
        public ActionResult LoadModalDeliveryChargeFramework(int priceListId, int id = 0)
        {
            var chargeframework = new B2BChargeFrameworkRules(dbContext).ChargeFrameworkById(id);
            if (id == 0)
                chargeframework.PriceList = new B2BPricelistRules(dbContext).PricelistById(priceListId);
            return PartialView("_ModalDeliveryChargeFramework", chargeframework);
        }
        public ActionResult LoadModalDeliveryDriver(string keyword)
        {
            var model = new DdsRules(dbContext).GetDomainUserNotDriver(CurrentDomainId(), keyword);
            return PartialView("_ModalDeliveryDriver", model);
        }
        public ActionResult LoadModalDeliveryVehicle(int id = 0)
        {
            var vehicle = new B2BVehicleRules(dbContext).VehicleById(id);
            return PartialView("_ModalDeliveryVehicle", vehicle != null ? vehicle : new Vehicle());
        }
        public ActionResult LoadContentPriceList(string keyword, int locationId = 0)
        {
            var pricelist = new List<PriceList>();
            if (locationId == 0)
                pricelist = new B2BPricelistRules(dbContext).SearchPricelist(keyword, CurrentLocationManage());
            else if (locationId < 0)
                pricelist = new B2BPricelistRules(dbContext).SearchPricelistByDomain(keyword, CurrentDomainId());
            else
                pricelist = new B2BPricelistRules(dbContext).SearchPricelist(keyword, locationId);
            return PartialView("_ContentPriceList", pricelist);
        }
        public ActionResult LoadModalLocationChange(int driverId)
        {
            ViewBag.Driver = new DdsRules(dbContext).GetDdsDriverById(driverId);
            return PartialView("_ModalDriverLocationChange", new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId()));
        }
        public ActionResult LoadModalChargeFrameworkPort(int priceId)
        {
            ViewBag.PricelistClone = new B2BPricelistRules(dbContext).PricelistById(priceId);
            return PartialView("_ModalChargeFrameworkPort", new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId()));
        }
        public ActionResult LoadContentMemberDetail(int posUId)
        {
            if (CurrentLocationManage() == 0)
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId());
            ViewBag.LocationDefaultId = 0;
            return PartialView("_ContentMemberDetail", new DeviceUserRules(dbContext).GetById(posUId));
        }
        public ActionResult LoadContentMemberDetailByUser(string posUId)
        {
            if (CurrentLocationManage() == 0)
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(CurrentDomainId());
            ViewBag.LocationDefaultId = 0;
            return PartialView("_ContentMemberDetail", new DeviceUserRules(dbContext).GetByUserId(posUId));
        }
        public ActionResult LoadContentChargeFramework(int priceId)
        {
            var pricelist = new B2BPricelistRules(dbContext).PricelistById(priceId);
            if (pricelist == null)
                return View("Error");
            ViewBag.PriceList = pricelist;
            return PartialView("_ContentChargeFramework", new B2BChargeFrameworkRules(dbContext).SearchChargeFramework(priceId));
        }
        public ActionResult LoadContentItemInfo(int itemId)
        {
            var deliverySettings = new DdsRules(dbContext).GetDeliverySettingsByLocationId(CurrentLocationManage());
            ViewBag.CurrentItem = deliverySettings.DeliveryService;
            return PartialView("_ContentItemInfo", new TraderItemRules(dbContext).GetById(itemId));
        }

        public ActionResult SavePriceList(B2bPriceListModel b2BPriceList,
            string mediaObjectKey)
        {
            var b2bPricelistRule = new B2BPricelistRules(dbContext);
            if (b2BPriceList.LocationId == 0)
                b2BPriceList.LocationId = CurrentLocationManage();
            if (b2bPricelistRule.CheckPricelistName(b2BPriceList.Id, b2BPriceList.Name, b2BPriceList.LocationId))
            {
                ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_DATA_EXISTED", b2BPriceList.Name);
                return Json(returnJson);
            }


            b2BPriceList.IconUri = mediaObjectKey;

            return Json(b2bPricelistRule.SavePricelist(b2BPriceList, CurrentUser().Id));
        }
        public ActionResult SaveChargeFramework(ChargeFramework chargeFramework)
        {
            var b2bchargeFrameworkRule = new B2BChargeFrameworkRules(dbContext);

            if (b2bchargeFrameworkRule.CheckPricelistName(chargeFramework.Id, chargeFramework.Name, chargeFramework.PriceList.Id))
            {
                ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_DATA_EXISTED", chargeFramework.Name);
                return Json(returnJson);
            }
            return Json(b2bchargeFrameworkRule.SaveChargeFramework(chargeFramework, CurrentUser().Id));
        }
        public ActionResult DeleteChargeFramework(int id)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            return Json(new B2BChargeFrameworkRules(dbContext).DeleteChargeFramework(id));
        }
        public ActionResult DeletePriceList(int id)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            return Json(new B2BPricelistRules(dbContext).DeletePricelist(id));
        }
        public ActionResult SearchVehicles([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword)
        {
            return Json(new B2BVehicleRules(dbContext).SearchVehicles(requestModel, CurrentDomainId(), keyword), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchDrivers([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int locationId = 0)
        {
            if (locationId == 0)
                locationId = CurrentLocationManage();
            return Json(new DdsRules(dbContext).SearchDrivers(requestModel, locationId, keyword, CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveVehicle(Vehicle vehicle)
        {
            vehicle.Domain = CurrentDomain();
            var b2bvehicleRule = new B2BVehicleRules(dbContext);
            if (b2bvehicleRule.CheckVehicleName(vehicle.Id, vehicle.Name, vehicle.Domain.Id))
            {
                ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_DATA_EXISTED", vehicle.Name);
                return Json(returnJson);
            }
            return Json(b2bvehicleRule.SaveVehicle(vehicle, CurrentUser().Id));
        }
        public ActionResult DeleteVehicle(int id)
        {
            return Json(new B2BVehicleRules(dbContext).DeleteVehicle(id));
        }
        public ActionResult GetVehiclesForSelect2()
        {
            return Json(new B2BVehicleRules(dbContext).GetVehiclesForSelect2(CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteDriver(int id)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            return Json(new DdsRules(dbContext).DeleteDdsDriver(id));
        }
        public ActionResult UpdateStatusDriver(int id, DriverStatus status)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            return Json(new DdsRules(dbContext).UpdateStatusDriver(id, status));
        }
        public ActionResult UpdateLocationDriver(Driver driver)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            return Json(new DdsRules(dbContext).UpdateDriverLocation(driver));
        }
        public ActionResult UpdateVehicleDriver(int id, int vehicleId)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            return Json(new DdsRules(dbContext).UpdateVehicleDriver(id, vehicleId));
        }
        public ActionResult AddDriver(int posUId, int accountId, int locationId = 0, string driverUserId = "")
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            if (accountId == 0)
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_BKREQUIRED");
                return Json(returnJson);
            }
            var cashBank = new DriverBankmateAccount
            {
                AssociatedBKAccount = new BKAccount { Id = accountId }
            };
            return Json(new DdsRules(dbContext).AddDriver(
                posUId, (locationId == 0 ? CurrentLocationManage() : locationId), CurrentDomainId(), cashBank, CurrentUser().Id, driverUserId));
        }
        public ActionResult SaveDeliverySettings(DeliverySettings settings)
        {
            if (settings.Location == null)
                settings.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            else
                settings.Location = new TraderLocationRules(dbContext).GetById(settings.Location.Id);
            return Json(new DdsRules(dbContext).SaveDeliverySettings(settings, CurrentUser().Id));
        }
        public ActionResult ClonePricelist(int cloneId, string cloneName, int locationId = 0)
        {
            var domainId = CurrentDomainId();
            var userId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userId, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            var location = new TraderLocationRules(dbContext).GetById(locationId > 0 ? locationId : CurrentLocationManage());
            return Json(new B2BPricelistRules(dbContext).ClonePricelist(cloneId, cloneName, location, userId));
        }


        public ActionResult SaveB2CConfig(B2cConfigModel model)
        {
            var domainId = CurrentDomainId();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            model.LocationId = CurrentLocationManage();
            return Json(new CommerceRules(dbContext).SaveB2CConfig(model, CurrentUser().Id));
        }


        public ActionResult SaveB2COrderConfigDefault(B2COrderSettingDefault model)
        {
            return Json(new CommerceRules(dbContext).B2CSaveOrderConfigDefault(model));
        }
        public ActionResult SaveB2BOrderConfigDefault(B2BOrderSettingDefault model)
        {
            return Json(new CommerceRules(dbContext).B2BSaveOrderConfigDefault(model));
        }
    }
}