using System;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderSetupController : BaseController
    {
        public ActionResult ShowLocation()
        {
            var domain = CurrentDomain();
            ViewBag.TraderLocations = domain.TraderLocations.Any();
            return PartialView("_TraderSetupShowLocationPartial");
        }
        public ActionResult ShowProductGroup()
        {
            var domain = CurrentDomain();
            ViewBag.TraderGroups = domain.TraderGroups.Any();

            return PartialView("_TraderSetupShowProductGroupPartial");
        }
        public ActionResult ShowContactGroup()
        {
            var domain = CurrentDomain();
            ViewBag.ContactGroups = domain.ContactGroups.Any();

            return PartialView("_TraderSetupShowContactGroupPartial");
        }
        public ActionResult ShowWorkGroup()
        {
            ViewBag.Workgroups = CurrentDomain().Workgroups.Any();

            return PartialView("_TraderSetupShowWorkGroupPartial");
        }
        public ActionResult ShowAccounting()
        {
            var domainId = CurrentDomainId();
            ViewBag.JournalGroups = new JournalGroupRules(dbContext).GetByDomainId(domainId);
            ViewBag.TraderSetting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(domainId);
            ViewBag.TaxRates = new TaxRateRules(dbContext).GetByDomainId(domainId).Any();
            return PartialView("_TraderSetupShowAccountingPartial");
        }
        public ActionResult ShowComplete()
        {
            var domain = CurrentDomain();
            var traderLocations = domain.TraderLocations.Any();
            var traderGroups = domain.TraderGroups.Any();
            var workgroups = domain.Workgroups.Any();

            var isBkk = new TraderSettingRules(dbContext).GetTraderSettingByDomain(CurrentDomainId()).IsQbiclesBookkeepingEnabled;
            var taxRate = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId()).Any();
            var traderAccount = isBkk || taxRate;

            var isCompleted = traderLocations && traderGroups && workgroups && traderAccount;




            return PartialView("_TraderSetupShowCompletedPartial", isCompleted == true ? "" : "disabled");
        }


        [HttpPost]
        public ActionResult UpdateTraderIsSettingComplete(TraderSetupCurrent isComplete)
        {
            try
            {
                var traderLocation = new TraderSettingRules(dbContext);
                var result = traderLocation.UpdateTraderIsSettingComplete(CurrentDomainId(), isComplete);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        // GET: TraderItem
        [HttpPost]
        public ActionResult SaveLocation(TraderLocation location)
        {
            location.Domain = CurrentDomain();
            var result = new TraderLocationRules(dbContext).SaveOnly(location, CurrentUser().Id);
            result.result = CurrentDomain().TraderLocations.Any();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteLocation(int id)
        {
            var traderLocation = new TraderLocationRules(dbContext);
            var result = new ReturnJsonModel();
            if (traderLocation.DeleteLocation(id))
            {
                result.actionVal = 1;
            }
            else
            {
                result.actionVal = 3;
                result.msg = "Delete faild.";
            }
            result.result = CurrentDomain().TraderLocations.Any();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveGroup(TraderGroup group)
        {
            group.Domain = CurrentDomain();
            var refModel = new ReturnJsonModel();
            try
            {
                var tradergroup = new TraderGroupRules(dbContext);
                if (tradergroup.CheckExistName(group))
                {
                    refModel.actionVal = 3;
                    refModel.msg = group.Name + " already exists.";
                }
                else
                {
                    refModel = tradergroup.SaveGroup(group, CurrentUser().Id);

                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
            }
            refModel.result = CurrentDomain().TraderGroups.Any();
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveContactGroup(TraderContactGroup group)
        {
            group.Domain = CurrentDomain();

            var refModel = new ReturnJsonModel() { actionVal = 1, result = true };
            if (group.Id > 0) refModel.actionVal = 2;
            try
            {
                var traderContactGroup = new TraderContactRules(dbContext);
                if (traderContactGroup.TraderContactGroupNameCheck(group))
                {
                    refModel.actionVal = 3;
                    refModel.msg = group.Name + " already exists.";
                }
                else
                {
                    group = traderContactGroup.SaveTraderContactGroup(group, CurrentUser().Id);
                    refModel.msgId = group.Id.ToString();
                }

            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
            }
            refModel.result = CurrentDomain().ContactGroups.Any();
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteGroup(int id)
        {
            var traderGroup = new TraderGroupRules(dbContext);
            var result = new ReturnJsonModel();
            if (traderGroup.DeleteGroup(id))
            {
                result.actionVal = 1;
            }
            else
            {
                result.actionVal = 3;
                result.msg = "Delete faild.";
            }
            result.result = CurrentDomain().TraderGroups.Any();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteContactGroup(int id)
        {
            var traderContactGroup = new TraderContactRules(dbContext);
            var result = new ReturnJsonModel();
            if (traderContactGroup.DeleteContactGroup(id))
            {
                result.actionVal = 1;
            }
            else
            {
                result.actionVal = 3;
                result.msg = "Delete faild.";
            }
            result.result = CurrentDomain().ContactGroups.Any();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult UpdateSetting(int id = 0, bool value = false)
        {
            var result = new ReturnJsonModel();
            var _traderSetting = new TraderSettingRules(dbContext);
            result = _traderSetting.ChangeStatus(id, value, CurrentDomain());

            var isBkk = ((TraderSettings)result.Object).IsQbiclesBookkeepingEnabled;
            var taxRate = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId()).Any();
            if (!isBkk && !taxRate)
                result.result = false;
            else
                result.result = true;
            result.Object = null;
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveTaxRate(TaxRate taxrate)
        {
            var refModel = new TaxRateRules(dbContext).SaveTaxRate(taxrate, CurrentDomain());

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult DeleteWorkgroup(int id)
        {
            var refModel = new ReturnJsonModel();
            var rules = new TraderWorkGroupsRules(dbContext);
            refModel.result = rules.Delete(id);
            refModel.result = CurrentDomain().Workgroups.Any();
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult DeleteTaxRate(int id)
        {
            var refModel = new ReturnJsonModel
            {
                result = new TaxRateRules(dbContext).DeleteTaxRate(id)
            };

            var isBkk = new TraderSettingRules(dbContext).GetTraderSettingByDomain(CurrentDomainId()).IsQbiclesBookkeepingEnabled;
            var taxRate = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId()).Any();
            if (!isBkk && !taxRate)
                refModel.result = false;
            else
                refModel.result = true;

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

    }
}