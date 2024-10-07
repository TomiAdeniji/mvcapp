using System;
using System.Linq;
using System.Web.Mvc;
using Qbicles.Models.Trader;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using System.Collections.Generic;
using Qbicles.Models.Trader.Resources;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.SalesChannel;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderConfigurationController : BaseController
    {
        // GET: TraderConfiguration
        public ActionResult TraderConfigurationContent(string value)
        {
            try
            {
                var domainId = CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");
                ViewBag.UserRoleRights = userRoleRights;

                switch (value)
                {
                    case "settings":
                        return PartialView("_TraderSettingsConfig", new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId));
                    case "Workgroups":
                        var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroupsConfig(domainId);
                        ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocationBase(domainId);
                        ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleBase(domainId);
                        ViewBag.Process = new TraderProcessRules(dbContext).GetAllBase();
                        ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItemBase(domainId);
                        ViewBag.DomainRoles = new DomainRolesRules(dbContext).GetDomainRolesBase(domainId);
                        return PartialView("_TraderWorkGroups", workGroups);
                    case "location":
                        var location = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                        return PartialView("_TraderLocationConfig", location);
                    case "dimension":
                        var dimension = new TransactionDimensionRules(dbContext).GetTransactionDimension2TraderReportingFilters(domainId);
                        return PartialView("_TraderDimmensionConfig", dimension);
                    case "accounting":
                        ViewBag.traderSetting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(CurrentDomainId());
                        var bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                        if (bkGroup.Any())
                            ViewBag.TreeView = BKConfigurationRules.GenTreeView(bkGroup.ToList());
                        else
                            ViewBag.TreeView = "";
                        var accounting = new TaxRateRules(dbContext).GetTaxRateByDomainId(domainId).ToList();

                        ViewBag.JournalGroups = new JournalGroupRules(dbContext).GetByDomainId(domainId);

                        return PartialView("_TraderAccountingConfig", accounting);
                    case "group":
                        ViewBag.LstGroupItems = new TraderGroupRules(dbContext).GetTraderGroupItemConfig(domainId);
                        ViewBag.LstContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupConfig(domainId);
                        return PartialView("_TraderGroupConfig");
                    case "resourcecategory":
                        return PartialView("_TraderResourceCategoryConfig");
                    case "references":
                        ViewBag.traderSetting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(CurrentDomainId());
                        return PartialView("_TraderReferenceConfig");
                    case "mastersetup":
                        ViewBag.traderSetting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(CurrentDomainId());
                        return PartialView("_TraderMasterSetup");

                }
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return null;
            }
            return null;
        }
        [HttpPost]
        public ActionResult UpdateSetting(TraderSettings setting)
        {
            setting.Domain = CurrentDomain();
            var refModel = new TraderSettingRules(dbContext).UpdateSetting(setting);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetResouceCategoryDocumentData()
        {
            var categoryDocuments = new TraderSettingRules(dbContext).GetResouceCategoriesByType(CurrentDomainId(), ResourceCategoryType.Document);
            return PartialView("_TraderResourceCategoryDocumentDataConfig", categoryDocuments);
        }
        public ActionResult GetResouceCategoryImageData()
        {
            var categoryImages = new TraderSettingRules(dbContext).GetResouceCategoriesByType(CurrentDomainId(), ResourceCategoryType.Image);
            return PartialView("_TraderResourceCategoryImageDataConfig", categoryImages);
        }

        [HttpPost]
        public ActionResult UpdateResourceCategory(ResourceCategory category)
        {
            category.Domain = CurrentDomain();
            var result = new TraderSettingRules(dbContext).SaveCategory(category, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult DeleteResourceCategory(int id)
        {
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            new TraderSettingRules(dbContext).DeleteReCategory(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowListMemberForContactGroup(int contactGroupId = 0)
        {
            if (contactGroupId > 0)
            {
                var contactGroups = CurrentDomain().ContactGroups.FirstOrDefault(q => q.Id == contactGroupId);

                return PartialView("_TraderContactGroupShowMember", contactGroups?.Contacts ?? new List<TraderContact>());
            }
            else
            {
                return PartialView("_TraderContactGroupShowMember", new List<TraderContact>());
            }
        }
        public ActionResult ChangeIsQbiclesBookkeepingEnabled(bool isCheck, int traderId)
        {
            var refModel = new ReturnJsonModel();
            refModel.result = new TraderSettingRules(dbContext).ChangeIsQbiclesBookkeepingEnabled(isCheck, traderId);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LocationAddEdit(int id = 0)
        {
            var location = new TraderLocation();

            ViewBag.Countries = new CountriesRules().GetAllCountries();
            if (id > 0)
                location = new TraderLocationRules(dbContext).GetById(id);

            if (location.Address == null) location.Address = new TraderAddress();
            return PartialView("_TraderLocationAddEditPartial", location);
        }
        //location
        [HttpGet]
        public ActionResult GetEditLocationById(int id)
        {
            var location = new TraderLocationRules(dbContext).GetOnlyById(id);

            if (location != null)
            {
                return Json(location, JsonRequestBehavior.AllowGet);
            }
            else return Json(null, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveLocation(TraderLocation location, string country)
        {

            var refModel = new ReturnJsonModel();
            location.Address.Country = new CountriesRules().GetCountryByName(country);

            location.Domain = CurrentDomain();
            if (new TraderLocationRules(dbContext).CheckExistName(location))
            {
                refModel.actionVal = 3;
                refModel.msg = ResourcesManager._L("ERROR_MSG_632", location.Name);
                refModel.msgId = location.Id.ToString();
                refModel.msgName = location.Name;
                refModel.result = true;

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }

            refModel = new TraderLocationRules(dbContext).SaveLocation(location, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteLocation(int id)
        {
            try
            {
                var rules = new TraderLocationRules(dbContext);
                if (rules.DeleteLocation(id))
                {
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Fail", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        //Group Item
        [HttpGet]
        public ActionResult GetEditGroupById(int id)
        {
            var group = new TraderGroupRules(dbContext).GetOnlyById(id);

            if (group != null)
            {
                return Json(group, JsonRequestBehavior.AllowGet);
            }
            else return Json(null, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowTableContactGroup()
        {
            return PartialView("_TraderGroupConfigContactGroupTablePartial", CurrentDomain().ContactGroups);
        }
        [HttpPost]
        public ActionResult SaveContactGroup(TraderContactGroup group)
        {
            var refModel = new ReturnJsonModel();
            if (group.Id > 0)
            {
                refModel.actionVal = 2;
            }
            else
            {
                refModel.actionVal = 1;
            }

            refModel.result = true;
            group.Domain = CurrentDomain();
            try
            {
                var rule = new TraderContactRules(dbContext);
                if (group.Id > 0 && rule.TraderContactGroupNameCheck(group))
                {
                    refModel.actionVal = 3;
                    refModel.msg = ResourcesManager._L("ERROR_MSG_632", group.Name);
                    refModel.result = false;
                }
                else
                {
                    rule.SaveTraderContactGroup(group, CurrentUser().Id);
                    group.Domain = null;
                    group.Creator = null;
                    group.Contacts = null;
                    refModel.Object = group;
                }
            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DelteContactGroup(int id = 0)
        {
            var result = new ReturnJsonModel();
            result.msgId = id.ToString();
            result.result = new TraderContactRules(dbContext).DeleteContactGroup(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveGroup(TraderGroup group)
        {

            var refModel = new ReturnJsonModel();
            group.Domain = CurrentDomain();
            if (new TraderGroupRules(dbContext).CheckExistName(group))
            {
                refModel.actionVal = 3;
                refModel.msg = "\"" + group.Name + "\": already exists";
                refModel.msgId = group.Id.ToString();
                refModel.msgName = group.Name;
                refModel.result = true;

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }

            var rule = new TraderGroupRules(dbContext);
            refModel = rule.SaveGroup(group, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteGroup(int id)
        {
            try
            {
                var rules = new TraderGroupRules(dbContext);
                if (rules.DeleteGroup(id))
                {
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Fail", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        [HttpDelete]
        public ActionResult DeleteContactGroup(int id)
        {
            try
            {
                var rules = new TraderContactRules(dbContext);
                if (rules.DeleteContactGroup(id))
                {
                    return Json("OK", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Fail", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }



        public ActionResult UpdateJournalGroupDefault(int journalGroupId, int traderSettingId)
        {
            var refModel = new ReturnJsonModel();
            refModel.result = new TraderSettingRules(dbContext).UpdateJournalGroupDefault(journalGroupId, traderSettingId);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadGroupConfigTab(int groupid)
        {
            var domainId = CurrentDomainId();
            var grouprule = new TraderGroupRules(dbContext);
            TabGroupConfigModel model = new TabGroupConfigModel();
            model.TraderGroup = grouprule.GetById(groupid);
            model.MasterSetup = grouprule.GetMasterSetupByGroupId(groupid, domainId);
            if (model.MasterSetup == null)
            {
                grouprule.SaveMasterSetup(groupid, domainId, CurrentUser().Id);
            }
            var queryTaxRates = new TaxRateRules(dbContext).GetByDomainId(domainId);
            model.PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
            model.SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
            model.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            model.SalesChannels = Enum.GetValues(typeof(SalesChannelEnum)).Cast<SalesChannelEnum>().ToList();
            return PartialView("~/Views/Trader/_AddGroupConfigTab.cshtml", model);
        }
        public ActionResult LoadAccountingConfigItem(int traderitemid)
        {
            var domainId = CurrentDomainId();
            var productrule = new TraderItemRules(dbContext);
            var queryTaxRates = new TaxRateRules(dbContext).GetByDomainId(domainId);
            ViewBag.PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
            ViewBag.SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
            return PartialView("~/Views/Trader/_EditAccountingConfigItem.cshtml", productrule.GetById(traderitemid));
        }
        public ActionResult RemoveMasterSetup(int groupid)
        {
            new TraderGroupRules(dbContext).RemoveMasterSetup(groupid, CurrentDomainId());
            return Json(true);
        }
        public ActionResult ItemsTraderGroupMaster([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int groupid, string keyword, int type)
        {
            return Json(new TraderGroupRules(dbContext).ItemsTraderGroupMaster(requestModel, groupid, keyword, type), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveApplyMasterSetup(ApplyTypeSettingsModel model)
        {
            return Json(new TraderGroupRules(dbContext).ApplyMasterSettingsByGroupId(CurrentDomainId(), CurrentUser().Id, model));
        }
        public ActionResult UpdateAccountingItemSettings(AccountingItemSettingsModel model)
        {
            return Json(new TraderGroupRules(dbContext).AccountSettingsByItemId(CurrentDomainId(), CurrentUser().Id, model));
        }
        public ActionResult ApplyConfigPriceByGroup(ConfigsPriceModel model)
        {
            return Json(new TraderGroupRules(dbContext).ApplyConfigPriceByGroup(CurrentDomainId(), CurrentUser().Id, model));
        }
        public ActionResult PricesTraderGroupMaster([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int groupid, string keyword, int locationid, int saleschannel)
        {
            return Json(new TraderGroupRules(dbContext).PricesTraderGroupMaster(requestModel, CurrentDomainId(), groupid, keyword, locationid, saleschannel), JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateValuePrice(int id, bool isInclusiveTax, decimal value)
        {
            return Json(new TraderGroupRules(dbContext).UpdateValuePrice(id, isInclusiveTax, value, CurrentDomainId(), CurrentUser().Id));
        }
    }
}