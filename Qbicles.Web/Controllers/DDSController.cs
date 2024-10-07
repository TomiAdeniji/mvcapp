using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.PoS;
using Qbicles.Models.Trader.SalesChannel;
namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class DDSController : BaseController
    {

        // -------- Delivery Queue Add Edit Delete -----------
        public ActionResult CreateDdsQueue(DeliveryQueue queue)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };
            try
            {
                if (queue.PrepQueue.Id == 0)
                {
                    result.msg = ResourcesManager._L("ERROR_MSG_298");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                queue.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());

                result = new DdsRules(dbContext).CreateDeliveryQueue(queue, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateDdsQueue(DeliveryQueue queue)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            queue.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            result = new DdsRules(dbContext).UpdateDeliveryQueue(queue);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDdsQueue(int id)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };
            try
            {
                result = new DdsRules(dbContext).DeleteDeliveryQueue(id);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // -------- Delivery Display system -----------
        // -------- Devices -----------
        public ActionResult DdsDevicesContent()
        {
            try
            {
                var domainId = CurrentDomainId();
                var locationId = CurrentLocationManage();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");
                ViewBag.UserRoleRights = userRoleRights;

                ViewBag.DdsQueues = new DdsRules(dbContext).GetDeliveriesQueueByLocation(CurrentLocationManage());

                var devices = new DdsRules(dbContext).GetDdsDevicesByLocationId(locationId);
                return PartialView("_DdsDevicesContent", devices ?? new List<DdsDevice>());
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return null;
            }

        }

        public ActionResult DdsDevicesSearch(string name, int ddsQueueId)
        {
            var model = new DdsRules(dbContext).SearchDevice(name, ddsQueueId, CurrentLocationManage());
            return PartialView("_DdsDevicesSearch", model);
        }


        public ActionResult DdsDeviceAddEdit(int id)
        {
            ViewBag.DdsQueue = new DdsRules(dbContext).GetDeliveryQueueByLocation(CurrentLocationManage());
            ViewBag.ddsAdministrators = CurrentDomain().Users;
            ViewBag.PosUsers = new DeviceUserRules(dbContext).GetAll(CurrentDomainId());
            ViewBag.OdsDeviceTypes = new PDSRules(dbContext).GetOdsDeviceTypeByLocation(CurrentLocationManage());
            var model = new DdsRules(dbContext).GetDdsDeviceById(id);
            return PartialView("_DdsDeviceAddEdit", model);
        }

        public ActionResult CreateDdsDevice(DdsDevice device, string adminIds, string userIds)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };
            try
            {
                if (device.Queue.Id == 0)
                {
                    result.msg = ResourcesManager._L("ERROR_MSG_298");
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                device.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());

                result = new DdsRules(dbContext).CreateDdsDevice(device, adminIds, userIds, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateDdsDevice(DdsDevice device, string adminIds, string userIds)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };

            device.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            result = new DdsRules(dbContext).UpdateDdsDevice(device, adminIds, userIds);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDdsDevice(int id)
        {
            var result = new ReturnJsonModel
            {
                result = false,
                actionVal = 0
            };
            try
            {
                result = new DdsRules(dbContext).DeleteDdsDevice(id);

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                result.msg = ex.Message;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // --------- General -------------
        public ActionResult DdsGeneralContent(int locationId = 0)
        {
            try
            {
                var id = locationId == 0 ? CurrentLocationManage() : locationId;
                var setting = new PosSettingRules(dbContext).GetByLocation(id, CurrentUser().Id, CurrentUser().Timezone);
                return PartialView("_DdsGeneralContent", setting ?? new PosSettings());
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return null;
            }
        }
        // -------- Drivers -----------
        public ActionResult DdsDriversContent()
        {
            try
            {
                var domainId = CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");
                ViewBag.UserRoleRights = userRoleRights;

                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);

                var rule = new DdsRules(dbContext);
                ViewBag.DdsLocationsQueue = rule.GetDdsLocationsQueue(domainId);

                var drivers = rule.GetDdsDriversByDomain(domainId, false);
                return PartialView("_DdsDriversContent", drivers ?? new List<Driver>());
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return null;
            }
        }

        public ActionResult DdsDriverSearch(string name, int statusId, int locationId, int currentLocationId)
        {
            var domainId = CurrentDomainId();
            var model = new DdsRules(dbContext).SearchDriver(name, locationId, statusId, domainId, false);
            //ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            ViewBag.CurrentLocationDomainId = new TraderLocationRules(dbContext).GetById(currentLocationId)?.Domain.Id ?? 0;
            ViewBag.DdsLocationsQueue = new DdsRules(dbContext).GetDdsLocationsQueue(domainId);
            return PartialView("_DdsDriversSearch", model);
        }

        public ActionResult UpdateDriverLocation(Driver driver)
        {
            var result = new DdsRules(dbContext).UpdateDriverLocation(driver);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDriverWorkLocation(Driver driver, bool idAdd)
        {
            var result = new DdsRules(dbContext).UpdateDriverWorkLocation(driver, idAdd);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDriverShift(Driver driver)
        {
            var result = new DdsRules(dbContext).UpdateDriverShift(driver);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDdsDriver(int id)
        {

            var result = new DdsRules(dbContext).DeleteDdsDriver(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


    }
}