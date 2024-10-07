using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.PoS;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class PointOfSaleDeviceController : BaseController
    {

        public ActionResult CreateDevice(PosDevice device)
        {
            device.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            
            var result = new PosDeviceRules(dbContext).CreateDevice(device, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckExistsTabletPrefix(string tabletPrefix, int deviceId)
        {
            var result =
                new PosDeviceRules(dbContext).existsTabletPrefix(tabletPrefix, deviceId, CurrentLocationManage());
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateDeviceMenu(PosDevice device)
        {
            var result = new PosDeviceRules(dbContext).UpdateDeviceMenu(device);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateDeviceQueue(PosDevice device)
        {
            var result = new PosDeviceRules(dbContext).UpdateDeviceQueue(device);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDevice(PosDevice device)
        {
            var result = new PosDeviceRules(dbContext).UpdateDevice(device);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActiveDevice(PosDevice device)
        {
            var result = new PosDeviceRules(dbContext).ActiveDevice(device);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchDevice(string name, int order)
        {
            var model = new PosDeviceRules(dbContext).SearchDevice(name, order, CurrentLocationManage());
            return PartialView("_SearchDevice", model);
        }

        [HttpPost]
        public ActionResult GeneratePin(CreatePosUserViewModel posUser)
        {
            posUser.Domain = CurrentDomain();
            var result = new DeviceUserRules(dbContext).GeneratePin(posUser);            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreatePosUser(CreatePosUserViewModel posUser)
        {
            posUser.Domain = CurrentDomain();
            var result = new DeviceUserRules(dbContext).Create(posUser, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult DeletePosUser(CreatePosUserViewModel posUser)
        {
            posUser.Domain = CurrentDomain();
            var result = new DeviceUserRules(dbContext).Delete(posUser);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteDevice(int id)
        {
            var result = new PosDeviceRules(dbContext).DeleteDevice(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}