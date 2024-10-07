using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.Budgets;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderBudgetController : BaseController
    {
        // Budget Group
        public ActionResult BudgetGroupContent()
        {
            var domain = CurrentDomain();

            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage()
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            return PartialView("_BudgetGroupContent");
        }
        public ActionResult BudgetGroupMaster(int id)
        {
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var budget = new TraderBudgetGroupRules(dbContext).GetBudGroupById(id);
            return View(budget);
        }
        public ActionResult BudgetGroupMasterContent(int id)
        {
            var domain = CurrentDomain();
            ViewBag.TraderGroups = new TraderBudgetGroupRules(dbContext).GetTraderGroups(domain.Id);
            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage()
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            var budget = new TraderBudgetGroupRules(dbContext).GetBudGroupById(id);
            return PartialView("_BudgetGroupMasterContent", budget);
        }

        public ActionResult BudgetGroupAddEditModal(int id = 0)
        {
            var domain = CurrentDomain();
            var budgetGroup = new BudgetGroup();
            if (id > 0)
                budgetGroup = new TraderBudgetGroupRules(dbContext).GetBudGroupById(id);

            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage()
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            ViewBag.PaymentTerms = new TraderBudgetGroupRules(dbContext).GetPaymentTerms();
            ViewBag.TraderGroups = new TraderBudgetGroupRules(dbContext).GetTraderGroups(domain.Id);

            return PartialView("_BudgetGroupAddEditModal", budgetGroup);
        }
        public ActionResult BudgetGroupItems()
        {
            var domain = CurrentDomain();
            var budgetGroups = new TraderBudgetGroupRules(dbContext).GetBudgetGroupsByLocation(CurrentLocationManage(), domain.Id);
            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage()
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            return PartialView("_BudgetGroupItems", budgetGroups);
        }
        public ActionResult BudgetGroupItemProductsDialog(int idBudget = 0)
        {
            var budgetGroup = new TraderBudgetGroupRules(dbContext).GetBudGroupById(idBudget);
            ViewBag.BudgetGroupTitle = budgetGroup.Title;
            List<TraderItem> lstTraderItems;

            if (budgetGroup.Type == BudgetGroupType.Expenditure)
            {
                lstTraderItems = budgetGroup.ExpenditureGroups.Where(q => q.Items.Any()).Select(q => q.Items.Where(x => x.Locations.Select(i => i.Id).Contains(CurrentLocationManage()) && x.IsBought)).SelectMany(q => q).ToList();
            }
            else
            {
                lstTraderItems = budgetGroup.RevenueGroups.Where(q => q.Items.Any()).Select(q => q.Items.Where(x => x.Locations.Select(i => i.Id).Contains(CurrentLocationManage()) && x.IsSold)).SelectMany(q => q).ToList();
            }
            return PartialView("_BudgetGroupItemProductsDialog", lstTraderItems);
        }
        [HttpDelete]
        public ActionResult DeleteBudgetGroup(int id)
        {
            ReturnJsonModel refModel = new TraderBudgetGroupRules(dbContext).DeleteBudgetGroup(id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveBudgetGroup(BudgetGroup budgetGroup)
        {
            budgetGroup.Domain = CurrentDomain();
            budgetGroup.Location = new TraderLocation() { Id = CurrentLocationManage() };
            var refModel = new TraderBudgetGroupRules(dbContext).SaveBudgetGroup(budgetGroup, CurrentUser().Id);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        // Budget Scenarios
        public ActionResult BudgetScenariosContent()
        {
            var domain = CurrentDomain();
            var budgetGroups = new TraderBudgetGroupRules(dbContext).GetBudgetGroupsByLocation(CurrentLocationManage(), domain.Id);

            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage()
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            var abc = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage()
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget)));


            return PartialView("_BudgetScenariosContent", budgetGroups);
        }
        public ActionResult BudgetScenariosItems(string key = "", string sortBy = "")
        {
            var domain = CurrentDomain();
            var budgetScenarios = new TraderBudgetGroupRules(dbContext).GetBudgetScenariosByLocation(CurrentLocationManage(), domain.Id);
            if (!string.IsNullOrEmpty(key))
                budgetScenarios = budgetScenarios.Where(q => (q.Title + " " + q.Description).Contains(key)).ToList();
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "0":
                        budgetScenarios = budgetScenarios.OrderBy(q => q.Title).ToList();
                        break;
                    case "1":
                        budgetScenarios = budgetScenarios.OrderByDescending(q => q.Title).ToList();
                        break;
                    case "2":
                        budgetScenarios = budgetScenarios.OrderByDescending(q => q.CreatedDate).ToList();
                        break;
                    case "3":
                        budgetScenarios = budgetScenarios.OrderBy(q => q.CreatedDate).ToList();
                        break;
                    case "4":
                        budgetScenarios = budgetScenarios.OrderBy(q => q.IsActive).ToList();
                        break;
                }
            }
            return PartialView("_BudgetScenariosItems", budgetScenarios);
        }
        public ActionResult BudgetScenarioAddEditModal(int id = 0)
        {
            var domain = CurrentDomain();
            var budgetScenario = new BudgetScenario();
            if (id > 0)
                budgetScenario = new TraderBudgetGroupRules(dbContext).GetBudgetScenarioById(id);

            ViewBag.BudgetGroups = new TraderBudgetGroupRules(dbContext).GetBudgetGroupsByLocation(CurrentLocationManage(), domain.Id);

            return PartialView("_BudgetScenariosAddEditModal", budgetScenario);
        }
        [HttpGet]
        public ActionResult CheckExistsTitle(string title = "", int id = 0)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = false, actionVal = 1 };
            try
            {
                refModel.result = new TraderBudgetGroupRules(dbContext).CheckExistsTitle(title, id, CurrentLocationManage(), CurrentDomainId());
            }
            catch (Exception ex)
            {
                refModel.msg = ex.Message;
                refModel.actionVal = 3;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveBudgetScenario(BudgetScenario budgetScenaio, string startdate = "", string enddate = "")
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            try
            {
                budgetScenaio.Domain = CurrentDomain();
                if (!string.IsNullOrEmpty(startdate) && !string.IsNullOrEmpty(enddate))
                {
                    budgetScenaio.FiscalStartPeriod = DateTime.ParseExact(startdate, "d/M/yyyy", null);
                    budgetScenaio.FiscalEndPeriod = DateTime.ParseExact(enddate, "d/M/yyyy", null);
                }
                budgetScenaio.Location = new TraderLocation() { Id = CurrentLocationManage() };
                refModel = new TraderBudgetGroupRules(dbContext).SaveBudgetScenario(budgetScenaio, CurrentUser().Id);
            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult SetActive(int id)
        {
            ReturnJsonModel refModel = new ReturnJsonModel() { result = true, actionVal = 1 };
            try
            {
                refModel = new TraderBudgetGroupRules(dbContext).SetActiveBudgetScenario(id, CurrentLocationManage(), CurrentDomainId());
            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                refModel.result = false;
            }
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BudgetMain(int id)
        {
            try
            {
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var rule = new TraderBudgetGroupRules(dbContext);
                var model = rule.GetBudgetScenarioById(id);
                ViewBag.BudgetGroups = rule.GetBudgetGroupsByLocation(CurrentLocationManage(), CurrentDomainId());
                ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), model.Location.Id);
                return View(model);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// Budget starting quantities content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BudgetStartingQuantities(int id)
        {
            try
            {
                var rule = new TraderBudgetGroupRules(dbContext);
                var model = rule.GetBudgetScenarioById(id);
                ViewBag.BudgetGroups = rule.GetBudgetGroupsByLocation(CurrentLocationManage(), CurrentDomainId());
                ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), model.Location.Id);
                return PartialView("_BudgetStartingQuantities", model);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// Budget process content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BudgetProcess(int id)
        {
            try
            {
                var domain = CurrentDomain();
                ViewBag.AddNewRight = domain.Workgroups.Any(q => q.Location.Id == CurrentLocationManage()
                                                                 && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                                                                 && q.Members.Select(u => u.Id).Contains(CurrentUser().Id));

                ViewBag.BudgetScenarioId = id;

                var rule = new TraderBudgetGroupRules(dbContext);
                var model = rule.GetBudgetScenarioItemGroupsByScenarioId(id);
                return PartialView("_BudgetProcess", model);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// Budget Cash flow content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BudgetCashflow(int id)
        {
            try
            {
                var rule = new TraderBudgetGroupRules(dbContext);
                var model = rule.GetBudgetScenarioById(id);
                ViewBag.BudgetGroups = rule.GetBudgetGroupsByLocation(CurrentLocationManage(), CurrentDomainId());
                ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), model.Location.Id);
                return PartialView("_BudgetCashflow", model);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }
        /// <summary>
        /// Budget vs Actual content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BudgetVsActual(int id)
        {
            try
            {
                var rule = new TraderBudgetGroupRules(dbContext);
                var model = rule.GetBudgetScenarioById(id);
                ViewBag.BudgetGroups = rule.GetBudgetGroupsByLocation(CurrentLocationManage(), CurrentDomainId());
                ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(CurrentDomainId(), model.Location.Id);
                return PartialView("_BudgetVsActual", model);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        /// <summary>
        /// Display modal items associated budget on budget process
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BudgetProcessItemsPreview(int id)
        {

            var model = new TraderBudgetMainRules(dbContext).GetBudgetScenarioItemByBudgetScenarioItemGroupId(id);

            return PartialView("_BudgetProcessItemsPreview", model);
        }
        /// <summary>
        /// Add item(s) to Budget
        /// </summary>
        /// <param name="budgetScenarioItemGroupId"></param>
        /// <para name="budgetScenarioId"></para>
        /// <returns></returns>
        public ActionResult BudgetAddEditItem(int budgetScenarioItemGroupId, int budgetScenarioId, string oView)
        {
            ViewBag.Workgroups = CurrentDomain().Workgroups.Where(q => q.Location.Id == CurrentLocationManage()
                                                             && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Budget))
                                                             && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name);

            ViewBag.GroupTypes = HelperClass.EnumModel.GetEnumValuesAndDescriptions<ItemGroupType>();
            ViewBag.BudgetScenarioId = budgetScenarioId;
            ViewBag.oView = oView;
            var model = new TraderBudgetMainRules(dbContext).GetBudgetScenarioItemGroupById(budgetScenarioItemGroupId);
            return PartialView("_BudgetAddEditItem", model);
        }

        public ActionResult BudgetAddEditChoseItem(int budgetScenarioItemGroupId, ItemGroupType itemGroupType, int budgetScenarioId)
        {
            ViewBag.ItemGroupType = itemGroupType;

            ViewBag.TraderItems = new TraderBudgetMainRules(dbContext).GetAllByItemType(budgetScenarioId, itemGroupType);


            ViewBag.BudgetScenarioItems = new TraderBudgetMainRules(dbContext).GetBudgetScenarioItemByBudgetScenarioItemGroupId(budgetScenarioItemGroupId).Any();


            return PartialView("_BudgetAddEditChoseItem");
        }




        /// <summary>
        /// Update unit
        /// </summary>
        /// <param name="startingQuantity"></param>
        /// <returns></returns>
        public ActionResult UpdateScenarioItemStartingUnit(ScenarioItemStartingQuantity startingQuantity)
        {
            var refModel = new TraderBudgetGroupRules(dbContext).UpdateScenarioItemStartingUnit(startingQuantity);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Update quantity
        /// </summary>
        /// <param name="startingQuantity"></param>
        /// <returns></returns>
        public ActionResult UpdateScenarioItemStartingQuantity(ScenarioItemStartingQuantity startingQuantity)
        {
            var refModel = new TraderBudgetGroupRules(dbContext).UpdateScenarioItemStartingQuantity(startingQuantity);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Add Budget Scenario Item when click Add now or click Period breakdown
        /// </summary>
        /// <param name="budgetScenarioGroup"></param>
        /// <returns></returns>
        public ActionResult AddBudgetScenarioItem(BudgetScenarioItemGroup budgetScenarioGroup)
        {
            var refModel = new TraderBudgetMainRules(dbContext).AddBudgetScenarioItem(budgetScenarioGroup, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Remove Budget Scenario Item from table list
        /// </summary>
        /// <param name="budgetScenarioGroupId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ActionResult RemoveBudgetScenarioItem(int budgetScenarioGroupId, int itemId)
        {
            var refModel = new TraderBudgetMainRules(dbContext).RemoveBudgetScenarioItem(budgetScenarioGroupId, itemId);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult PeriodBreakdownItemManagement(int budgetScenarioItemId, int type)
        {
            var model = new TraderBudgetMainRules(dbContext).PeriodBreakdownItemManagement(budgetScenarioItemId);
            ViewBag.Type = type;
            return PartialView("_PeriodBreakdownItemManagement", model);
        }

        public ActionResult ValidBudgetGroupItemStatus(int id)
        {
            var refModel = new TraderBudgetMainRules(dbContext).ValidBudgetGroupItemStatus(id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendToReviewBudgetScenario(int budgetScenarioGroupId)
        {
            var refModel = new TraderBudgetMainRules(dbContext).SendToReviewBudgetScenarioItemGroup(budgetScenarioGroupId, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProcessApproval(int id, string oView)
        {
            try
            {
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var rule = new TraderBudgetMainRules(dbContext);
                var model = rule.GetBudgetScenarioItemGroupById(id);
                var currentDomainId = model?.WorkGroup.Qbicle.Domain.Id ?? 0;
                ValidateCurrentDomain(model?.WorkGroup?.Qbicle.Domain ?? CurrentDomain(), model.WorkGroup?.Qbicle.Id ?? 0);
                var userSetting = CurrentUser();

                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(model?.ApprovalRequest?.ApprovalRequestDefinition?.Id ?? 0, userSetting.Id);

                SetCurrentApprovalIdCookies(model.ApprovalRequest?.Id ?? 0);

                ViewBag.oView = oView;

                var timeline = rule.BudgetGroupItemApprovalStatusTimeline(model?.Id ?? 0, userSetting.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(model ?? new BudgetScenarioItemGroup());
                //return PartialView("_SaleReviewContent", saleModel ?? new TraderSale());

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult ConfirmPeriodChange(List<ItemProjection> itemProjections)
        {
            var refModel = new TraderBudgetMainRules(dbContext).ConfirmPeriodChange(itemProjections);
            return Json(refModel, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ReloadBudgetStatusApproval(int id)
        {
            try
            {
                var userSetting = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var rule = new TraderBudgetMainRules(dbContext);
                var model = rule.GetBudgetScenarioItemGroupById(id);

                if (model != null)
                    ValidateCurrentDomain(model.WorkGroup?.Qbicle.Domain ?? CurrentDomain(), model.WorkGroup?.Qbicle.Id ?? 0);
                ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(model?.ApprovalRequest?.ApprovalRequestDefinition?.Id ?? 0, userSetting.Id);


                SetCurrentApprovalIdCookies(model.ApprovalRequest?.Id ?? 0);

                ViewBag.oView = "A";

                var timeline = rule.BudgetGroupItemApprovalStatusTimeline(model?.Id ?? 0, userSetting.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return PartialView("_ReloadBudgetStatusApproval", model ?? new BudgetScenarioItemGroup());

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }



        /// <summary>
        /// Budget report content
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BudgetReport(int id)
        {
            var model = new TraderBudgetGroupRules(dbContext).BudgetPanelReport(id, CurrentDomainId(), CurrentLocationManage());
            return PartialView("_BudgetReport", model);
        }

        public ActionResult BudgetGroupReport(int sId, int gId)
        {
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            ViewBag.WorkGroups = new TraderSaleRules(dbContext).GetWorkGroups(CurrentLocationManage());
            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());

            ViewBag.BudgetScenarioId = sId;
            ViewBag.BudgetGroupId = gId;

            var budgetGroup = new TraderBudgetGroupRules(dbContext).GetBudGroupById(gId);

            return View(budgetGroup);
        }


        public ActionResult GetDataTableBudgetGroupReport([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int budgetScenarioId, int budgetGroupId, string dimensions, int workGroupId, string dateRange)
        {
            var result = new TraderBudgetGroupRules(dbContext).GetDataTableBudgetGroupReport(requestModel, CurrentDomainId(), CurrentLocationManage(), CurrentUser().Timezone, CurrentUser().DateTimeFormat, budgetScenarioId, budgetGroupId, dimensions, workGroupId, dateRange);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderBudgetGroupReportCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
    }

}