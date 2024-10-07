using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.DDS;
using Qbicles.Models.Trader.ODS;
using Qbicles.Models.TraderApi;
using static Qbicles.BusinessRules.HelperClass;
using Qbicles.Models.Trader.SalesChannel;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class PDSController : BaseController
    {
        public ActionResult PDSContent(string value)
        {

            var domainId = CurrentDomainId();
            var locationId = CurrentLocationManage();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");

            ViewBag.UserRoleRights = userRoleRights;
            var model = new PDSRules(dbContext).ListCategoryExclutionSets(locationId, CurrentUser().Id);

            switch (value)
            {
                case "queues":
                    var queues = new PDSRules(dbContext).GetPrepAndDdsQueueByLocation(locationId);
                    return PartialView("_PDSQueue", queues ?? new List<PrepQueueModel>());
                case "pds-general":
                    var setting = new PosSettingRules(dbContext).GetByLocation(locationId, CurrentUser().Id, CurrentUser().Timezone);
                    return PartialView("_PDSPreparationGeneralDisplaySystem", setting ?? new PosSettings());
                case "pds-device-type":
                    var pdsDeviceTypes = new PDSRules(dbContext).GetOdsDeviceTypeByLocation(locationId);
                    ViewBag.PosOrderTypes = new PosSaleOrderRules(dbContext).GetOrderTypeInLocation(CurrentLocationManage());
                    return PartialView("_PDSDeviceType", pdsDeviceTypes ?? new List<OdsDeviceType>());
                case "pds-device":
                    var prepDisplayDevices1 = new PDSRules(dbContext).GetPrepDisplayDeviceByLocation(locationId);
                    return PartialView("_PDSPreparationDisplaySystem", prepDisplayDevices1 ?? new List<PrepDisplayDevice>());
                case "cds":
                    return PartialView("_PDSCustomerDisplaySystem");
                case "dds":
                    return PartialView("_PDSDeliveryDisplaySystem");
                case "mds":
                    return PartialView("_PDSManagementDisplaySystem");
                case "pds-category-exclusion":
                    return PartialView("_PDSCatExclusion", model);
            }

            return null;
        }


        // -------- PrepQueue -----------
        public ActionResult SearchQueue(string name, QueueType type)
        {
            var model = new PDSRules(dbContext).SearchQueue(name, CurrentLocationManage(), type);
            return PartialView("_SearchQueue", model);
        }

        public ActionResult AddEditPrepQueue(int id, QueueType type)
        {
            var prepQueue = new PrepQueue();
            var ddsQueue = new DeliveryQueue();

            switch (type)
            {
                case QueueType.All:
                    break;
                case QueueType.Order:
                    prepQueue = new PDSRules(dbContext).GetQueueById(id);
                    break;
                case QueueType.Delivery:
                    ddsQueue = new DdsRules(dbContext).GetDeliveriesQueueById(id);
                    break;
            }

            ViewBag.PrepQueue = prepQueue;
            ViewBag.DeliveryQueue = ddsQueue;
            ViewBag.PrepQueueList = new DdsRules(dbContext).GetPrepQueueFreeDelivery(CurrentLocationManage(), ddsQueue.Id);

            ViewBag.Id = id;
            ViewBag.QueueType = type;
            return PartialView("_AddEditQueue");
        }
        // -------- PrepQueue Add Edit Delete -----------
        public ActionResult CreatePrepQueue(PrepQueue queue)
        {
            queue.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            
            var result = new PDSRules(dbContext).CreatePrepQueue(queue, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreatePrepDeliveryQueue()
        {
            var result = new PDSRules(dbContext).CreatePrepDeliveryQueue(CurrentLocationManage(), CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePrepQueue(PrepQueue queue)
        {
            queue.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            var result = new PDSRules(dbContext).UpdatePrepQueue(queue);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeletePrepQueue(int id)
        {
            var result = new PDSRules(dbContext).DeletePrepQueue(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }




        // -------- PrepDisplayDevice -----------
        public ActionResult SearchPrepDisplayDevice(string name)
        {
            var model = new PDSRules(dbContext).SearchPrepDisplayDevice(name, CurrentLocationManage());
            return PartialView("_SearchPrepDisplayDevice", model);
        }


        public ActionResult AddEditPrepDisplayDevice(int id)
        {
            ViewBag.Users = CurrentDomain().Users;
            ViewBag.PrepQueue = new PDSRules(dbContext).GetPrepQueueByLocation(CurrentLocationManage());
            var model = new PDSRules(dbContext).GetPrepDisplayDeviceId(id);
            ViewBag.PosUsers = new DeviceUserRules(dbContext).GetAll(CurrentDomainId());
            ViewBag.OdsDeviceTypes = new PDSRules(dbContext).GetOdsDeviceTypeByLocation(CurrentLocationManage());
            ViewBag.ListCategoryExclutions = new PDSRules(dbContext).ListCategoryExclutionSets(CurrentLocationManage(), CurrentUser().Id);
            return PartialView("_AddEditPrepDisplayDevice", model ?? new PrepDisplayDevice());
        }

        public ActionResult CreatePrepDisplayDevice(PrepDisplayDevice prepDisplayDevice)
        {
            prepDisplayDevice.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());

            var result = new PDSRules(dbContext).CreatePrepDisplayDevice(prepDisplayDevice, CurrentDomain().Users);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePrepDisplayDevice(PrepDisplayDevice prepDisplayDevice)
        {
            prepDisplayDevice.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            var result = new PDSRules(dbContext).UpdatePrepDisplayDevice(prepDisplayDevice, CurrentDomain().Users);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeletePrepDisplayDevice(int id)
        {
            var result = new PDSRules(dbContext).DeletePrepDisplayDevice(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        // -------- OdsDeviceType -----------
        public ActionResult SearchOdsDeviceTypeName(string name)
        {
            var model = new PDSRules(dbContext).SearchOdsDeviceTypeByName(name, CurrentLocationManage());
            return PartialView("_SearchOdsDeviceType", model);
        }
        public ActionResult SearchOdsDeviceTypeAndName(string name, int orderTypeId)
        {
            
            var model = new PDSRules(dbContext).SearchOdsDeviceTypeByName(name, orderTypeId, CurrentLocationManage());
            return PartialView("_SearchOdsDeviceType", model);
        }


        public ActionResult AddEditOdsDeviceType(int id)
        {
            ViewBag.Users = CurrentDomain().Users;
            ViewBag.Status = EnumModel.GetEnumValuesAndDescriptions<PrepQueueStatus>();
            ViewBag.PosOrderTypes = new PosSaleOrderRules(dbContext).GetOrderTypeInLocation(CurrentLocationManage());

            var model = new PDSRules(dbContext).GetOdsDeviceTypeId(id);
            
            return PartialView("_AddEditOdsDeviceType", model);
        }

        public ActionResult CreateOdsDeviceType(OdsDeviceType odsDeviceType)
        {
            odsDeviceType.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());

            var result = new PDSRules(dbContext).CreateOdsDeviceType(odsDeviceType, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateOdsDeviceType(OdsDeviceType odsDeviceType)
        {
            odsDeviceType.Location = new TraderLocationRules(dbContext).GetById(CurrentLocationManage());
            var result = new PDSRules(dbContext).UpdateOdsDeviceType(odsDeviceType, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteOdsDeviceType(int id)
        {
            var result = new PDSRules(dbContext).DeleteOdsDeviceType(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //--------------Category exclusion--------------
        public ActionResult CreateCategoriesExclusion(int CategoryExclutionSetId = 0)
        {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            var prepDisplayDevices1 = new PDSRules(dbContext).GetPrepDisplayDeviceByLocation(CurrentLocationManage());
            ViewBag.PrepDisplayDevices = prepDisplayDevices1;

            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            if (CategoryExclutionSetId == 0)
            {
                return PartialView("_PDSCatExcSet");
            }
            else
            {
                var CategoryExclutionSet = new PDSRules(dbContext).ListCategoryExclutionSets(CurrentLocationManage(),CurrentUser().Id,CategoryExclutionSetId);
                ViewBag.CategoryExclutionSet = CategoryExclutionSet;
                return PartialView("_PDSCatExcSet");
            }
        }

        public ActionResult CreateNewCategoryExclustionSet(List<string>listCategories, string name, List<int>listPrepDevices)
        {
            var location = CurrentLocationManage();
            var user = CurrentUser().Id;
            var result = new PDSRules(dbContext).CreateNewCategoryExclustionSet(listCategories, name, user, location,listPrepDevices);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CategoryExclusionList(int currentExclusionSet = 0)
        {
            var location = CurrentLocationManage();
            var listCategories = new PDSRules(dbContext).ListCategoriesByTraderLocation(location, currentExclusionSet);
            ViewBag.ListCategories = listCategories;
            return PartialView("_PDSCatExcSelect");
        }

        public ActionResult IsUniqueNameCategoryExclsion(string name, int categoryExclutionSetId = 0)
        {
            var location = CurrentLocationManage();
            var result = new PDSRules(dbContext).IsNameExisted(location, name, categoryExclutionSetId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListCategoryExclutionSets()
        {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            var location = CurrentLocationManage();
            var userId = CurrentUser().Id;
            var model = new PDSRules(dbContext).ListCategoryExclutionSets(location, userId);
            return PartialView("_PDSCatExclusion",model);
        }
        public ActionResult UpdateCategoryExclutionSet(List<string> listCategoriesName, string name, int categoryExclutionSetId, List<int>listPrepDevices)
        {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            var result = new PDSRules(dbContext).UpdateCategoryExclusionSet(listCategoriesName, name, CurrentUser().Id, CurrentLocationManage(),categoryExclutionSetId, listPrepDevices);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteCategoryExclutionSet(int categoryExclutionSetId, string name) {
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                return View("ErrorAccessPage");
            var result = new PDSRules(dbContext).DeleteCategoryExclusionSet(categoryExclutionSetId, name);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}