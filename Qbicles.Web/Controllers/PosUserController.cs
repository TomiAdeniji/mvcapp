using Qbicles.BusinessRules;
using Qbicles.BusinessRules.PoS;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class PosUserController : BaseController
    {
        [HttpPost]
        public ActionResult CreatePosUserDevice(int deviceId, List<PooledUserModel> pooledUsers)
        {
            var result = new DeviceUserRules(dbContext).CreatePosUserDevice(pooledUsers, deviceId, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeletePosUserDevice(PosUserDeviceModel model)
        {
            var result = new DeviceUserRules(dbContext).DeletePosUserDevice(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePosUser(PosUserDeviceModel model, bool isDeleteAll)
        {
            var result = new DeviceUserRules(dbContext).UpdatePosUser(model);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePosUserAdminDevice(PosUserDeviceModel model)
        {
            var result = new DeviceUserRules(dbContext).UpdatePosUserAdminDevice(model, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}