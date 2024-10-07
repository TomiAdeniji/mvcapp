using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Trader.Inventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    public class BusinessReportsController : BaseController
    {
        // GET: BusinessReports
        public ActionResult Index()
        {
            if (!CanAccessBusiness())
                return View("ErrorAccessPage");

            var currentDomainId = CurrentDomainId();
            var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == currentDomainId && p.IsArchived == false);
            ViewBag.CurrentDomainPlan = currentDomainPlan;

            ViewBag.CurrentPage = SystemPageConst.BUSINESSREPORTS;
            this.SetCookieGoBackPage(SystemPageConst.BUSINESSREPORTS);
            return View();
        }

        public ActionResult LoadBusinessReportTab(string tab, bool reload = false)
        {
            try
            {
                var domainId = CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
                ViewBag.UserRoleRights = userRoleRights;
                var currentDomainPlan = dbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainId && p.IsArchived == false);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomain().Id);
                ViewBag.CurrencySettings = currencySettings;
                //var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(domainId);
                switch (tab)
                {
                    case "purchase":
                        ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                        ViewBag.TraderContacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(domainId);
                        return PartialView("_PurchaseTabContent");

                    case "sales":
                        ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                        ViewBag.TraderContacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(domainId);
                        return PartialView("_SalesTabContent");

                    case "invoices":
                        ViewBag.TraderContacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(domainId);
                        return PartialView("_InvoicesTabContent");

                    case "payments":
                        ViewBag.TraderContacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(domainId);
                        return PartialView("_PaymentsTabContent");

                    case "transfers":
                        return PartialView("_TransfersTabContent");

                    case "orders":
                        ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                        ViewBag.TraderContacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(domainId);
                        ViewBag.CurrentDomainPlan = currentDomainPlan;
                        return PartialView("_OrderTabContent");

                    case "bookkeeping":
                        ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                        return PartialView("_BookkeepingTabContent");

                    case "inventory":
                        var currentDomain = CurrentDomain();
                        var lstLocationIds = currentDomain.TraderLocations;
                        ViewBag.ListTraderLocation = lstLocationIds;
                        return PartialView("_InventoryTabContent");
                }
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), e, CurrentUser().Id);
                return null;
            }
            return null;
        }

        public ActionResult GetPurchase([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int locationId, int contactId, string channel, string datetime)
        {
            var result = new TraderPurchaseRules(dbContext).GetReportPurchase(requestModel, CurrentUser().Id, locationId, CurrentDomainId(), channel, contactId, keyword, datetime, false, CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSales([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int locationId, int contactId, string channel, string datetime)
        {
            var result = new TraderSaleRules(dbContext).GetReportSales(requestModel, CurrentUser().Id, locationId, CurrentDomainId(), channel, contactId, keyword, datetime, false, CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInvoices([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int contactId, string datetime)
        {
            var result = new TraderInvoicesRules(dbContext).GetReportInvoices(requestModel, keyword, contactId, datetime, CurrentUser(), CurrentDomainId(), CurrentUser().Id);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPayments([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int contactId, string datetime)
        {
            var result = new TraderCashBankRules(dbContext).GetReportPayments(requestModel, keyword, datetime, contactId, CurrentUser(), CurrentDomainId(), CurrentUser().Id);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTransfers([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datetime)
        {
            var result = new TraderTransfersRules(dbContext).GetReportTransfers(requestModel, keyword, datetime, CurrentUser(), CurrentUser().Id, CurrentDomainId());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetOrders([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string datetime, int locationId,
            int contactId, int channel, B2CFilterInvoiceType filterInvoice, B2CFilterPaymentType filterPayment, B2CFilterDeliveryType filterDelivery, bool isSaleOrderShow = false)
        {
            var result = new B2CRules(dbContext).GetB2COrders(requestModel, keyword, locationId, contactId,
                datetime, channel, filterInvoice, filterPayment, filterDelivery, CurrentUser(), CurrentDomainId(), isSaleOrderShow);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkManufacturing([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int locationId = 0)
        {
            var userSettings = CurrentUser();
            var result = new B2CRules(dbContext).GetBkManuDatas(requestModel, CurrentDomainId(), keyword, daterange, locationId, userSettings.DateTimeFormat, userSettings.DateFormat, userSettings.Timezone);
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkPaymentTblData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int locationId = 0)
        {
            var userSettings = CurrentUser();
            var result = new B2CRules(dbContext).GetBkPaymentDatas(requestModel, CurrentDomainId(), keyword, daterange, locationId, userSettings.DateTimeFormat, userSettings.DateFormat, userSettings.Timezone);
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkInvPurchase([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int locationId = 0)
        {
            var userSettings = CurrentUser();
            var result = new B2CRules(dbContext).GetBkInvPurchaseDatas(requestModel, CurrentDomainId(), keyword, daterange, locationId, userSettings.DateTimeFormat, userSettings.DateFormat, userSettings.Timezone);
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInventoryTblData([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,
            int locationId, string keySearch = "", string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1, bool hasSymbol = true)
        {
            var userId = CurrentUser().Id;
            var cookieName = $"itemUnitsChanged-{locationId}-{userId}";

            var unitsChange = GetDisplayUnitChangeCookies(cookieName);

            var result = new TraderInventoryRules(dbContext).GetInventoryServerSide(requestModel.ToIDataTablesRequest(), locationId, CurrentUser(), unitsChange,
               keySearch, inventoryBasis, maxDayToLast, days2Last, dayToLastOperator, hasSymbol);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);

            return Json(new DataTablesResponse(requestModel.Draw, new List<InventoryModel>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkNonInvPurchase([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int locationId = 0)
        {
            var userSettings = CurrentUser();
            var result = new B2CRules(dbContext).GetBkNonInvPurchaseDatas(requestModel, CurrentDomainId(), keyword, daterange, locationId, userSettings.DateTimeFormat, userSettings.DateFormat, userSettings.Timezone);
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkSInvoice([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int locationId = 0)
        {
            var userSettings = CurrentUser();
            var result = new B2CRules(dbContext).GetBkSaleInvoiceDatas(requestModel, CurrentDomainId(), keyword, daterange, locationId, userSettings.DateTimeFormat, userSettings.DateFormat, userSettings.Timezone);
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBkSTransfer([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, string daterange, int locationId)
        {
            var userSettings = CurrentUser();
            var result = new B2CRules(dbContext).GetBkSaleTransferDatas(requestModel, CurrentDomainId(), keyword, daterange, locationId, userSettings.DateTimeFormat, userSettings.DateFormat, userSettings.Timezone);
            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> ReProcessOrder(int tradeOderId)
        {
            if (!CanAccessBusiness())
                return Json(new ReturnJsonModel { msg = "Sorry, you are not allowed to access this page", result = false }, JsonRequestBehavior.AllowGet);
            var result = await new ProcessOrderRules(dbContext).ReProcessOrder(tradeOderId);
            return Json(result);
        }

        public async Task<ActionResult> ReProcessOrderInProcessing(int tradeOderId)
        {
            if (!CanAccessBusiness())
                return Json(new ReturnJsonModel { msg = "Sorry, you are not allowed to access this page", result = false }, JsonRequestBehavior.AllowGet);
            var tradeOrder = await dbContext.TradeOrders.FindAsync(tradeOderId);
            tradeOrder.OrderStatus = TradeOrderStatusEnum.AwaitingProcessing;
            tradeOrder.OrderProblem = TradeOrderProblemEnum.Non;
            dbContext.Entry(tradeOrder).State = EntityState.Modified;
            dbContext.SaveChanges();
            return await ReProcessOrder(tradeOderId);
        }

        public ActionResult UpdateInventoryUnit(int locationId, int unitId, int itemId,
            string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1, bool hasSymbol = true)
        {
            var userId = CurrentUser().Id;
            var cookieName = $"itemUnitsChanged-{locationId}-{userId}";

            var unitsChange = GetDisplayUnitChangeCookies(cookieName);

            var refModel = new TraderInventoryRules(dbContext).UpdateChangeItemUnit(userId, unitId, itemId, locationId, unitsChange,
                 inventoryBasis, maxDayToLast, days2Last, dayToLastOperator, hasSymbol);

            SetDisplayUnitChangeCookies(cookieName, refModel.Object2);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInventoryReportItemDetailModal(int unitId, int locationId,
            string inventoryBasis = "", int maxDayToLast = 0, string days2Last = "", int dayToLastOperator = 1)
        {
            var unitItem = dbContext.ProductUnits.FirstOrDefault(p => p.Id == unitId);
            if (unitItem == null)
                return null;

            var inventoryDetail = unitItem.Item.InventoryDetails.FirstOrDefault(e => e.Location.Id == locationId);
            if (inventoryDetail == null)
                return null;

            var currentUserSettings = CurrentUser();
            var dateFormat = string.IsNullOrEmpty(currentUserSettings.DateFormat) ? "dd/MM/yyyy" : currentUserSettings.DateFormat;

            int associatedCount = 0;
            var isCompoundProduct = true;
            var nOfDay = 1;

            if (inventoryDetail.CurrentRecipe != null && inventoryDetail.CurrentRecipe.Ingredients.Any() && unitItem.Item.IsCompoundProduct)
            {
                associatedCount = inventoryDetail.CurrentRecipe.Ingredients.Count;
            }
            else
            {
                isCompoundProduct = false;
                associatedCount = dbContext.Ingredients.Count(e => e.SubItem.Id == inventoryDetail.Item.Id);
            }

            var days2LastFrom = DateTime.UtcNow;
            var days2LastTo = DateTime.UtcNow;
            switch (dayToLastOperator)
            {
                case 1:
                    days2LastFrom = days2LastTo.AddDays(-7);
                    break;

                case 2:
                    days2LastTo = days2LastFrom.AddMonths(1);
                    break;

                case 3:
                    if (!string.IsNullOrEmpty(days2Last.Trim()))
                    {
                        if (!days2Last.Contains('-'))
                        {
                            days2Last += "-";
                        }

                        days2Last.ConvertDaterangeFormat(dateFormat, currentUserSettings.Timezone, out days2LastFrom, out days2LastTo);
                        days2LastFrom.AddTicks(1);
                        days2LastTo.AddDays(1).AddTicks(-1);
                    }
                    break;
            }

            var quantityOut = inventoryDetail.InventoryBatches.Where(e => e.Direction == BatchDirection.Out
                                                                && e.CreatedDate <= days2LastTo
                                                                && e.CreatedDate >= days2LastFrom).Sum(u => u.UnusedQuantity) / nOfDay;
            if (quantityOut == 0) quantityOut = 1;
            var dayToLast = (inventoryDetail.CurrentInventoryLevel / quantityOut);

            var quantityOfBaseUnit = unitItem.QuantityOfBaseunit;
            var unitName = unitItem.Name;
            var resultObject = new InventoryReportDetailModel
            {
                ItemName = unitItem.Item.Name,
                ImageUrl = unitItem.Item.ImageUri,
                Barcode = unitItem.Item.Barcode,
                SKU = unitItem.Item.SKU,
                AverageCost = (inventoryDetail.AverageCost * quantityOfBaseUnit).ToString("F2"),
                LatestCost = (inventoryDetail.LatestCost * quantityOfBaseUnit).ToString("F2"),
                CurrentInventory = (inventoryDetail.CurrentInventoryLevel / quantityOfBaseUnit).ToString("F2"),
                MinInventory = $"{(inventoryDetail.MinInventorylLevel / quantityOfBaseUnit):F2} {unitName}",
                MaxInventory = $"{(inventoryDetail.MaxInventoryLevel / quantityOfBaseUnit):F2} {unitName}",
                InventoryTotal = inventoryBasis == "average"
                            ? (inventoryDetail.CurrentInventoryLevel * inventoryDetail.AverageCost).ToString("F2")
                            : (inventoryDetail.CurrentInventoryLevel * inventoryDetail.LatestCost).ToString("F2"),
                InventoryBasis = inventoryBasis == "latest" ? "Latest cost" : "Average cost (FIFO)",
                DayToLastOperator = "Last one week sales",
                DayToLast = dayToLast.ToString("N0")
            };
            switch (dayToLastOperator)
            {
                case 1:
                    resultObject.DayToLastOperator = "Last one week sales";
                    break;
                case 2:
                    resultObject.DayToLastOperator = "Last one month sales";
                    break;
                case 3:
                    resultObject.DayToLastOperator = "Custom range";
                    break;
            }
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomain().Id);
            ViewBag.CurrencySettings = currencySettings;

            return PartialView("_InventoryReportItemModal", resultObject);
        }

        public ActionResult ShowInventoryAssociatedIngredientsItem(int itemId, int locationId)
        {
            var itemsAssociated = new TraderInventoryRules(dbContext).ShowIngredientsItemAssociated(itemId, locationId);
            ViewBag.IngredientId = itemId;
            return PartialView("~/Views/TraderInventory/_IngredientsItemAssociated.cshtml", itemsAssociated);
        }
    }
}