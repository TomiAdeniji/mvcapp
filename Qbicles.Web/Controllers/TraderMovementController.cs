using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Movement;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class TraderMovementController : BaseController
    {
        public ActionResult TraderMovementSetting(int alertGroupId = 0)
        {
            var _mvntSettingsGroup = new TraderMovementRules(dbContext).GetAlertGroupById(alertGroupId);

            var _currentDomainId = CurrentDomainId();
            var _currentLocationid = CurrentLocationManage();
            var _listTraderGroup = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(_currentDomainId, _currentLocationid);
            var _listAlertSettingByLocation = new TraderMovementRules(dbContext).GetAlertGroupByLocation(_currentLocationid);

            var _lstProductGroups = new List<TraderGroup>();
            if (_mvntSettingsGroup != null)
            {
                _lstProductGroups.AddRange(_mvntSettingsGroup.ProductGroups);
            }

            foreach (var traderGroupItem in _listTraderGroup)
            {
                if (!_listAlertSettingByLocation.Any(p => p.ProductGroups.Any(gr => gr.Id == traderGroupItem.Id)))
                {
                    _lstProductGroups.Add(traderGroupItem);
                }
            }
            ViewBag.ListProductGroups = _lstProductGroups;
            return View("MovementAlertSettings", _mvntSettingsGroup);
        }

        public ActionResult GetListMovementAlertSettings([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string key, AlertGroupStatusShown statusShown, string dateRange = "")
        {
            var domain = CurrentDomain();
            var result = new TraderMovementRules(dbContext).GetMovementAlertSettingDTContent(requestModel, domain.Id, CurrentLocationManage(),
                CurrentUser().Timezone, key, CurrentUser().DateTimeFormat, statusShown, dateRange);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);

            return Json(new DataTablesResponse(requestModel.Draw, new List<AlertSettingCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetAlertProductGroups(int alertGroupId, List<int> lstProductGroupIds)
        {
            var _movementAlertRules = new TraderMovementRules(dbContext);
            if (alertGroupId <= 0)
            {
                var newAlertGroupResult = _movementAlertRules.CreateNewAlertGroup(CurrentUser().Id, CurrentLocationManage());
                if (newAlertGroupResult.result)
                {
                    alertGroupId = (int)newAlertGroupResult.Object;
                }
                else
                {
                    return Json(newAlertGroupResult, JsonRequestBehavior.AllowGet);
                }
            }

            var result = _movementAlertRules.SetAlertProductGroup(alertGroupId, lstProductGroupIds, CurrentUser().Id);
            result.msgId = alertGroupId.ToString();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetNoMovementThresholds(int alertGroupId, string daterangeString)
        {
            var result = new TraderMovementRules(dbContext).SetNoMovementThresholds(alertGroupId, daterangeString, CurrentUser());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetMinMaxThresholds(int alertGroupId, string daterangeString)
        {
            var result = new TraderMovementRules(dbContext).SetMinMaxThresholds(alertGroupId, daterangeString, CurrentUser());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetAccumulationThresholds(int alertGroupId, string daterangeString, CheckEvent checkPeriod)
        {
            var result = new TraderMovementRules(dbContext).SetAccumulationThresholds(alertGroupId, daterangeString, checkPeriod, CurrentUser());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // Processing with hangfire job
        public ActionResult EnableNoMovementJob(int alertGroupId, CheckEvent checkPeriod)
        {
            var result = new TraderMovementRules(dbContext).ScheduleNoMovementCheck(alertGroupId, CurrentUser().Id, checkPeriod);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EnableMinMaxJob(int alertGroupId, CheckEvent checkPeriod)
        {
            var result = new TraderMovementRules(dbContext).ScheduleMinMaxCheck(alertGroupId, CurrentUser().Id, checkPeriod);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EnableAccumulationJob(int alertGroupId, CheckEvent checkPeriod)
        {
            var result = new TraderMovementRules(dbContext).ScheduleAccumulationCheck(alertGroupId, CurrentUser().Id, checkPeriod);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DisableScheduleJob(int alertConstraintId)
        {
            var result = new TraderMovementRules(dbContext).RemoveScheduleCheck(alertConstraintId, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        // End processing with hangfire

        public ActionResult ProductGroupItemsShow(int productGroupId)
        {
            var productGroup = dbContext.TraderGroups.Find(productGroupId);
            var lstItems = productGroup?.Items ?? new List<TraderItem>();
            ViewBag.ListItems = lstItems;

            var lstAlertGroupItemXrefs = new List<Item_AlertGroup_Xref>();
            foreach (var item in lstItems)
            {
                var xrefItem = dbContext.Item_AlertGroup_Xrefs.Where(p => p.Item.Id == item.Id).FirstOrDefault();
                if (xrefItem != null)
                    lstAlertGroupItemXrefs.Add(xrefItem);
            }
            ViewBag.ListXrefs = lstAlertGroupItemXrefs;

            return PartialView("_ProductGroupItemsShow");
        }


        public ActionResult UpdateItemAlertGroup(Item_AlertGroup_Xref item)
        {
            var result = new TraderMovementRules(dbContext).UpdateItemAlertGroup(item);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}