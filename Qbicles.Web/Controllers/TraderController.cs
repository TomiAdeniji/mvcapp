using Qbicles.BusinessRules;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.SalesChannel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderController : BaseController
    {
        // GET: Trader
        [HttpGet]
        public ActionResult CheckTrader()
        {
            if (CurrentLocationManage() == 0)
            {
                var traderLocation = new TraderLocationRules(dbContext).GetTraderLocationDefault(CurrentDomainId());
                SetCurrentLocationManage(traderLocation?.Id ?? 0);
            }
            return Json(CheckTraderIsSetupComplete(), JsonRequestBehavior.AllowGet);
        }

        private bool CheckTraderIsSetupComplete()
        {
            return new TraderSettingRules(dbContext).CheckTraderIsSetupComplete(CurrentDomainId());
        }

        public ActionResult AppTrader()
        {
            var domain = CurrentDomain();
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            ViewBag.Locations = domain != null ? domain.TraderLocations : new List<TraderLocation>();
            return View();
        }

        public ActionResult TraderConfigMaster()
        {
            try
            {
                var domainId = CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");
                var groupRule = new TraderGroupRules(dbContext);
                MasterSetupModel masterSetup = new MasterSetupModel();
                masterSetup.MasterSetups = groupRule.GetMasterSetup(domainId);
                masterSetup.TraderGroups = groupRule.GetTraderGroupItem(domainId);
                var queryTaxRates = new TaxRateRules(dbContext).GetByDomainId(CurrentDomainId());
                masterSetup.PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).ToList();
                masterSetup.SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).ToList();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                ViewBag.SalesChannels = Enum.GetValues(typeof(SalesChannelEnum)).Cast<SalesChannelEnum>().ToList();
                return View(masterSetup);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult TraderReorder(int id)
        {
            var domain = CurrentDomain();
            var user = CurrentUser().Id;
            var locationid = CurrentLocationManage();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");
            if (!CheckTraderIsSetupComplete())
                return RedirectToAction("TraderSetup", "Trader");

            #region ViewBag

            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == locationid
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Reorder))
                && q.Members.Select(u => u.Id).Contains(user)).OrderBy(n => n.Name).ToList();
            ViewBag.Deliveries = Enum.GetValues(typeof(DeliveryMethodEnum)).Cast<DeliveryMethodEnum>().Select(q => q.ToString()).ToList();
            ViewBag.ProductGroups = new TraderGroupRules(dbContext).GetTraderGroupItemByLocation(domain.Id, locationid);
            ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(domain.Id);
            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(domain.Id);

            #endregion ViewBag

            var reorder = new TraderInventoryRules(dbContext).GetReorderById(id);
            return View(reorder);
        }

        public ActionResult TraderSaleTab()
        {
            try
            {
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
                var today = DateTime.UtcNow.ConvertTimeFromUtc(timeZone);
                ViewBag.FromDateTime = today.Date.ToString("yyyy-MM-ddT00:00:00");
                ViewBag.ToDateTime = today.Date.ToString("yyyy-MM-ddT23:59:59");
                ViewBag.CurrentPage = "Trader"; SetCurrentPage("Trader");
                return PartialView("_TraderSaleTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadPostComment(string type, int id, int page = 1)
        {
            List<QbiclePost> lstPost = new List<QbiclePost>();
            ViewBag.PageIndex = page;
            switch (type.ToLower())
            {
                case "stockAudit":
                    var stockAudit = new TraderStockAuditRules(dbContext).GetById(id);
                    if (stockAudit != null && stockAudit.StockAuditApproval != null)
                    {
                        lstPost = stockAudit.StockAuditApproval.Posts.OrderByDescending(x => x.StartedDate).ToList();
                    }
                    break;

                case "sales":
                    var sale = new TraderSaleRules(dbContext).GetById(id);
                    if (sale != null && sale.SaleApprovalProcess != null)
                    {
                        lstPost = sale.SaleApprovalProcess.Posts;
                    }
                    break;

                case "purchases":
                    var purchase = new TraderPurchaseRules(dbContext).GetById(id);
                    if (purchase != null && purchase.PurchaseApprovalProcess != null)
                    {
                        lstPost = purchase.PurchaseApprovalProcess.Posts;
                    }
                    break;
            }

            lstPost = lstPost.OrderByDescending(q => q.Id).ToList();
            return PartialView("_ListPostComment", lstPost);
        }

        [HttpGet]
        public ActionResult CheckStatusTrader(string type, int id)
        {
            var resultModel = new ReturnJsonModel();
            resultModel.actionVal = 1;
            resultModel.msg = "";
            resultModel.result = true;
            if (id == 0)
            {
                resultModel.Object = "Continue";
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            try
            {
                switch (type)
                {
                    case "Sale":
                        resultModel.Object = new TraderSaleRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "SaleReturn":
                        resultModel.Object = new TraderSalesReturnRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "Purchase":
                        resultModel.Object = new TraderPurchaseRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "SportCount":
                        resultModel.Object = new TraderSpotCountRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "ManuJob":
                        resultModel.Object = new TraderManufacturingRules(dbContext).GetManuJobById(id).Status.ToString();
                        break;

                    case "WasteItem":
                        resultModel.Object = new TraderWasteReportRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "Transfer":
                        resultModel.Object = new TraderTransfersRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "Contact":
                        resultModel.Object = new TraderContactRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "Invoice":
                        resultModel.Object = new TraderInvoicesRules(dbContext).GetById(id).Status.ToString();
                        break;

                    case "Payment":
                    case "Bill":
                        resultModel.Object = new TraderCashBankRules(dbContext).GetCashAccountTransactionById(id).Status.ToString();
                        break;

                    default:
                        resultModel.Object = "";
                        break;
                }
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                resultModel.actionVal = 3;
                resultModel.msg = ex.Message;
                resultModel.result = false;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CheckStatusApprovalReq(string appId)
        {
            var resultModel = new ReturnJsonModel { actionVal = 1, msg = "", Object = "", result = true };

            try
            {
                var approvalId = HelperClass.Converter.Obj2Int(appId.Decrypt());
                if (approvalId > 0)
                {
                    var app = new ApprovalsRules(dbContext).GetApprovalById(approvalId);
                    if (app != null)
                        resultModel.Object = app.RequestStatus.ToString();
                }
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                resultModel.actionVal = 3;
                resultModel.msg = ex.Message;
                resultModel.result = false;
                return Json(resultModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult TraderPurchasesTab()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                return PartialView("_PurchasesTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult TransfersTab()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");
                var domainId = CurrentDomainId();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);

                return PartialView("_TransfersTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ContactTab()
        {
            try
            {
                var domainId = CurrentDomainId();
                this.SetCookieGoBackPage("Trader");
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                var currentLocationId = CurrentLocationManage();
                ViewBag.CanCreate = CurrentDomain().Workgroups.Any(q => q.Location.Id == currentLocationId
                                                                           && q.Processes.Any(a => a.Name == TraderProcessesConst.Contact)
                                                                           && q.Members.Any(m => m.Id == user.Id));

                ViewBag.WorkGroups = dbContext.WorkGroups.Where(p => p.Domain.Id == domainId).OrderBy(n => n.Name).ToList();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                //ViewBag.ContactGroups = dbContext.TraderContactGroups.Where(p => p.Domain.Id == domainId).ToList();
                ViewBag.ContactGroups = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(domainId);
                ViewBag.ContactGroupFilter = new TraderContactRules(dbContext).GetTraderContactsGroupFilter(domainId);
                ViewBag.Countries = new CountriesRules().GetAllCountries();

                var bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                ViewBag.TreeView = bkGroup.Any() ? BKConfigurationRules.GenTreeView(bkGroup.ToList()) : "";

                return PartialView("_ContactsTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult CashBankTab()
        {
            try
            {
                var domainId = CurrentDomainId();
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                ViewBag.WorkgroupPayment = CurrentDomain().Workgroups.Where(q => q.Location.Id == CurrentLocationManage()
                                                                                 && q.Processes.Any(p => p.Name == TraderProcessName.TraderPaymentProcessName)
                                                                                 && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                ViewBag.CurrentDomainId = CurrentDomainId();
                SetCookieGoBackPage("Trader");
                return PartialView("_CashBankTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ItemsProductsTab()
        {
            try
            {
                var domain = CurrentDomain();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                ViewBag.Locations = domain.TraderLocations;

                return PartialView("_ItemsProductsTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ManufacturingTab()
        {
            try
            {
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                var currentLocationId = CurrentLocationManage();
                ViewBag.WorkGroups = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == currentLocationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.Manufacturing))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();
                ViewBag.Locations = CurrentDomain().TraderLocations;
                return PartialView("_ManufacturingTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult BudgetTab()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                return PartialView("_BudgetTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ReportsTab()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
                return PartialView("_ReportsTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ChannelsTab()
        {
            try
            {
                var domainId = CurrentDomainId();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, domainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                //var b2bconfig = new CommerceRules(dbContext).GetB2BConfigByDomainId(domainId);
                ViewBag.CurrentPage = "trader";
                SetCurrentPage("trader");
                return PartialView("_ChannelsTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ConfigTab()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                return PartialView("_ConfigTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DeviceTab()
        {
            try
            {
                var domain = CurrentDomain();

                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                var domainId = CurrentDomainId();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                var currentLocationId = CurrentLocationManage();
                ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                    q.Location.Id == currentLocationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.POS))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();
                var quere = new PDSRules(dbContext).GetPrepQueueByLocation(currentLocationId);
                var delivery = new DdsRules(dbContext).GetDeliveryQueueByLocation(currentLocationId);
                ViewBag.ShowButtonQuere = (quere?.Id > 0 && delivery?.Id > 0);
                return PartialView("_DeviceTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DeviceCMContent()
        {
            try
            {
                return PartialView("_DeviceSubCM");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DevicePOSContent()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                var domainId = CurrentDomainId();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                return PartialView("_DeviceSubPOS");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DeviceODContent()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                var domainId = CurrentDomainId();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                ViewBag.PrepQuere = new PDSRules(dbContext).GetPrepQueueByLocation(CurrentLocationManage());
                return PartialView("_DeviceSubOD");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult DeviceDDContent()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                var domainId = CurrentDomainId();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                ViewBag.DeliveryQueue = new DdsRules(dbContext).GetDeliveryQueueByLocation(CurrentLocationManage());
                return PartialView("_DeviceSubDD");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult ShiftManagementTab()
        {
            try
            {
                var domain = CurrentDomain();

                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                return PartialView("_ShiftManagementTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult TraderSetup()
        {
            //var domain = CurrentDomain();
            var domainId = CurrentDomainId();
            ViewBag.DomainRoles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(domainId);
            var traderSetting = new TraderSettingRules(dbContext).GetTraderSettingByDomain(domainId);

            var taxRates = new TaxRateRules(dbContext).GetByDomainId(domainId).Any();
            var traderSetupInit = new TraderSetupInit
            {
                LocationClass = traderSetting.Domain.TraderLocations.Any() ? "complete" : "incomplete",
                LocationCompleted = traderSetting.Domain.TraderLocations.Any(),

                ProductGroupClass = traderSetting.Domain.TraderGroups.Any() ? "complete" : "incomplete",
                ProductGroupCompleted = traderSetting.Domain.TraderGroups.Any(),

                ContactGroupClass = traderSetting.Domain.ContactGroups.Any() ? "complete" : "incomplete",
                ContactGroupCompleted = traderSetting.Domain.ContactGroups.Any(),

                WorkgroupClass = traderSetting.Domain.Workgroups.Any() ? "complete" : "incomplete",
                WorkgroupCompleted = traderSetting.Domain.Workgroups.Any(),

                AccountingClass = taxRates ? "complete" : "incomplete",
                AccountingCompleted = taxRates
            };

            if (taxRates && traderSetting.JournalGroupDefault != null)
            {
                traderSetupInit.AccountingClass = "complete";
                traderSetupInit.AccountingCompleted = true;
            }
            else
            {
                traderSetupInit.AccountingClass = "incomplete";
                traderSetupInit.AccountingCompleted = false;
            }

            switch (traderSetting.IsSetupCompleted)
            {
                case TraderSetupCurrent.Location:
                    traderSetupInit.LocationClass = "incomplete active";
                    break;

                case TraderSetupCurrent.ProductGroup:
                    traderSetupInit.ProductGroupClass = "incomplete active";
                    break;

                case TraderSetupCurrent.Contact:
                    traderSetupInit.ContactGroupClass = "incomplete active";
                    break;

                case TraderSetupCurrent.Workgroup:
                    traderSetupInit.WorkgroupClass = "incomplete active";
                    break;

                case TraderSetupCurrent.Accounting:
                    traderSetupInit.AccountingClass = "incomplete active";
                    break;

                case TraderSetupCurrent.Complete:
                    traderSetupInit.SetupCompleteClass = "incomplete active";
                    break;

                case TraderSetupCurrent.TraderApp:
                    traderSetupInit.SetupCompleteClass = "complete active";
                    break;

                default:
                    traderSetupInit.LocationClass = "incomplete active";
                    break;
            }

            ViewBag.TraderSetupInit = traderSetupInit;
            return View(traderSetting);
        }

        public ActionResult OrderDisplaySystemTab()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
                return PartialView("_OrderDisplaySystemTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult TraderSaleSelectUnit(int idTraderItem = 0, int idLocation = 0, int table = 0,
            int itemSaleId = 0, string valueUnit = "", bool spotcount = false, bool issale = false, bool isTraderItem = false)
        {
            ViewBag.IsSale = issale;
            ViewBag.IsTraderItem = isTraderItem;
            ViewBag.TraderItemId = idTraderItem;
            ViewBag.SpotCount = spotcount;
            ViewBag.Table = table;
            ViewBag.ValueUnit = valueUnit;
            ViewBag.ItemSale = itemSaleId > 0
                ? new TraderItemRules(dbContext).GetSaleItemById(itemSaleId)
                : new TraderTransactionItem();
            if (idTraderItem == 0) return PartialView("_TraderSaleUnitBySelectsItemPartial", new List<UnitModel>());
            var conversions = new List<UnitModel>();
            var traderItem = new TraderItemRules(dbContext).GetById(idTraderItem);
            var baseCost = new TraderInventoryRules(dbContext).GetAverageCost(idTraderItem, idLocation);
            if (traderItem.Units.Count > 0)
                conversions = traderItem.Units.Select(u => new UnitModel
                {
                    Id = u.Id,
                    QuantityOfBaseunit = u.QuantityOfBaseunit,
                    Quantity = u.Quantity,
                    Name = u.Name,
                    Group = "BaseUnit",
                    Selected = "",
                    IsBase = u.IsBase,
                    BaseUnitCost = baseCost
                }).ToList();

            //var tskHangfire = new Task(async () =>
            //{
            //    await new Hangfire.QbiclesJob().HangFireExcecuteAsync(job);
            //});
            //tskHangfire.Start();

            return PartialView("_TraderSaleUnitBySelectsItemPartial", conversions);
        }

        public ActionResult ListTraderItem(int locationId, bool callback = false)
        {
            ViewBag.CallBack = callback;
            var domain = CurrentDomain();
            ViewBag.TraderGroups = new TraderGroupRules(dbContext).GetTraderGroupItem(domain.Id);
            ViewBag.Location = domain.TraderLocations.FirstOrDefault(q => q.Id == locationId);
            ViewBag.AdditionalInfos = new TraderResourceRules(dbContext).GetListAdditionalInfos(domain.Id);
            return PartialView("_TraderListItem");
        }

        public ActionResult ListInventory(int locationId, bool callback = false)
        {
            //var domain = CurrentDomain();
            //ViewBag.CallBack = callback;
            //var location = domain.TraderLocations.FirstOrDefault(q => q.Id == locationId);
            //var inventories = new TraderInventoryRules(dbContext).GetInventoryDetails(location);
            //return PartialView("_TraderListInventory", inventories);
            return PartialView("_TraderListInventories");
        }

        public ActionResult TraderMovementTabShow(int locationId, bool callback = false)
        {
            return PartialView("_TraderMovementTab");
        }

        public ActionResult ListItemInventoryBatch(int locationId, bool callback = false, string datestring = "")
        {
            return PartialView("_TraderListInventoryBatch");
        }

        public ActionResult GetDataItemInventoryBatch([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keysearch, string stringdate = "")
        {
            var domain = CurrentDomain();
            var result = new TraderItemRules(dbContext).GetTraderItemsByDateRange(requestModel, domain.Id, CurrentLocationManage(), CurrentUser().Timezone, keysearch, CurrentUser().DateTimeFormat, stringdate);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);

            return Json(new DataTablesResponse(requestModel.Draw, new List<InventoryBatchCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ChangeUnitItemInMovement(int traderItemId, int unitId, string stringdate)
        {
            return Json(new TraderItemRules(dbContext).ChangeUnitItemInMovement(traderItemId, unitId, CurrentDomainId(), CurrentLocationManage(), CurrentUser().Timezone, CurrentUser().DateTimeFormat, stringdate), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataItemOverView([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel)
        {
            string keysearch = Request.Params["keysearch"];
            string groupIds = Request.Params["groups[]"];
            string types = Request.Params["types[]"];
            string brands = Request.Params["bands[]"];
            string needs = Request.Params["needs[]"];
            string rating = Request.Params["rating[]"];
            string tags = Request.Params["tags[]"];
            int currentLocation = !string.IsNullOrEmpty(Request.Params["clid"]) ? Convert.ToInt32(Request.Params["clid"]) : CurrentLocationManage();
            int activeInLocation = Convert.ToInt32(Request.Params["activeInLocation"]);
            var domain = CurrentDomain();
            var result = new TraderItemRules(dbContext).GetItemOverviewItemProduct(requestModel, domain.Id, currentLocation, keysearch, groupIds, types, brands, needs, rating, tags, activeInLocation);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<ItemOverview>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddTraderItem(int locationId, int traderItemId = 0, int type = 1, bool callback = false)
        {
            ViewBag.CallBack = callback;
            var currentDomainId = CurrentDomainId();
            var queryTaxRates = new TaxRateRules(dbContext).GetByDomainId(currentDomainId);
            ViewBag.PurchaseTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax).OrderBy(n => n.Name).ToList();
            ViewBag.SaleTaxRates = queryTaxRates.Where(s => s.IsPurchaseTax == false).OrderBy(n => n.Name).ToList();
            ViewBag.TraderGroups = new TraderGroupRules(dbContext).GetTraderGroupItem(currentDomainId);
            //ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(currentDomainId);
            ViewBag.avatar = CurrentUser().ProfilePic;
            ViewBag.Types = type;
            ViewBag.LocationId = locationId;
            ViewBag.AdditionalInfos = new TraderResourceRules(dbContext).GetListAdditionalInfos(currentDomainId);
            var traderItems = dbContext.TraderItems.AsNoTracking().Where(q => q.Domain.Id == currentDomainId).Where(p => p.Id != traderItemId).OrderBy(s => s.Name).Select(x => new TraderItemEditCustomModel
            {
                Id = x.Id,
                Name = x.Name,
                Inventory = x.InventoryDetails.FirstOrDefault(q => q.Location != null && q.Location.Id == locationId),
                AssociatedRecipes = x.AssociatedRecipes
            });

            var traderItem = new TraderItem
            {
                Group = new TraderGroup(),
                InventoryDetails = new List<InventoryDetail> { new InventoryDetail { Location = new TraderLocation { } } },
                AssociatedRecipes = new List<Recipe> { new Recipe { } }
            };
            if (traderItemId > 0)
                traderItem = new TraderItemRules(dbContext).GetById(traderItemId);

            if (type == 1)
            {
                ViewBag.TraderItems = traderItems.ToList();
                return PartialView("_AppTraderItemBuy", traderItem);
            }
            ViewBag.TraderItems = traderItems.Where(e => e.Inventory != null).ToList();
            return PartialView("_AppTraderItemSell", traderItem);
        }

        public ActionResult GetTraderItemsCustomModel(int locationId, string search, int page = 0, int traderItemId = 0, int numberResult = 10, int selectedItems = 0)
        {
            var currentDomainId = CurrentDomainId();

            var query = dbContext.TraderItems.AsNoTracking().Where(q => q.Domain.Id == currentDomainId).Where(p => p.Id != traderItemId);

            #region filter

            if (!search.IsNullOrEmpty())
            {
                search = search.ToLower();
                query = query.Where(e => e.Name.Contains(search));
            }

            #endregion filter

            //if (traderItemId == 0 && selectedItems == 0)
            //{
            //    query = query.Where(e => false);
            //}

            //get special to set default value in select
            if (selectedItems != 0)
            {
                query = query.Where(e => e.Id == selectedItems);
            }

            #region order

            query = query.OrderBy(s => s.Name);

            #endregion order

            #region pargination

            int skipItems = page * numberResult;
            var totalItems = query.Count();
            var totalPage = Math.Ceiling((double)totalItems / numberResult);

            var traderItems = query.Skip(skipItems).Take(numberResult).Select(x => new
            {
                Id = x.Id,
                Name = x.Name,
                AverageCost = x.InventoryDetails.Where(q => q.Location != null && q.Location.Id == locationId).Any() ? x.InventoryDetails.FirstOrDefault(q => q.Location != null && q.Location.Id == locationId).AverageCost : 0,
                LatestCost = x.InventoryDetails.Where(q => q.Location != null && q.Location.Id == locationId).Any() ? x.InventoryDetails.FirstOrDefault(q => q.Location != null && q.Location.Id == locationId).LatestCost : 0
            }).ToList();

            #endregion pargination

            object result = new
            {
                totalItem = totalItems,
                items = traderItems,
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddContactInTraderItem(int locationId, int traderItemId = 0, int type = 1,
            bool callback = false, int contactid = 0)
        {
            var contact = new TraderContact();
            if (contactid > 0) contact = new TraderContactRules(dbContext).GetById(contactid);
            ViewBag.Groups = new TraderContactRules(dbContext).GetTraderContactsGroupByDomain(CurrentDomainId(), SalesChannelContactGroup.Trader);
            ViewBag.Countries = new CountriesRules().GetAllCountries();
            return PartialView("_TraderItemAddTraderContact", contact);
        }

        public ActionResult ShowProductUnitByItem(int itemId = 0, int unitId = 0)
        {
            var traderItem = new TraderItem();
            if (itemId > 0)
            {
                traderItem = new TraderItemRules(dbContext).GetById(itemId);
            }
            ViewBag.IdSelected = unitId;
            return PartialView("_TraderListInventoryBatchChangeUnit", traderItem);
        }

        public ActionResult ShowTableViewTrend()
        {
            return PartialView("_TraderListInventoryViewTrendTable");
        }

        public ActionResult ShowTraderItemTrend(int itemId = 0, int unitId = 0, int locationId = 0, string datestring = "")
        {
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow;
            if (datestring != null && !string.IsNullOrEmpty(datestring.Trim()))
            {
                if (!datestring.Contains('-'))
                {
                    datestring += "-";
                }
                datestring.ConvertDaterangeFormat(CurrentUser().DateTimeFormat, "UTC", out startDate, out endDate);
                ViewBag.DateAndTime = startDate.AddTicks(1).ToString("dd/MM/yyyy HH:mm") + " - " + endDate.ToString("dd/MM/yyyy HH:mm");
            }
            else
            {
                ViewBag.DateAndTime = "";
            }

            var traderItem = new TraderItemRules(dbContext).GetById(itemId);
            var currency = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(traderItem.Domain.Id);
            var InventoryDetails = traderItem.InventoryDetails.Where(e => e.Location.Id == locationId).OrderByDescending(e => e.LastUpdatedDate).FirstOrDefault();
            ViewBag.IdSelected = unitId;
            ViewBag.SKU = traderItem.SKU;
            ViewBag.ProductGroup = traderItem.Group.Name;
            ViewBag.LastestCost = InventoryDetails.LatestCost;
            ViewBag.CurrentUnit = InventoryDetails.CurrentInventoryLevel;
            ViewBag.CurrentCurrency = currency.CurrencySymbol;

            return PartialView("_TraderListinventoryViewTrend", traderItem);
        }

        public ActionResult ShowTableViewTrendServer([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest dtRequestTable, int itemId = 0, int unitId = 0, int locationId = 0, bool isGenSystem = true, string datestring = "")
        {
            var dtData = new TraderItemRules(dbContext).GetTraderItemsByDateRangeServer(dtRequestTable, itemId, CurrentDomainId(), CurrentLocationManage(), CurrentUser().Timezone, CurrentUser().DateTimeFormat, datestring, isGenSystem, unitId);
            return Json(dtData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCurrentOnHandInventoryDetails(int traderItem = 0, int locationId = 0, int unitId = 0)
        {
            var currentValue = new TraderItemRules(dbContext).GetCurrentOnHandInventoryDetails(traderItem, locationId, unitId);

            return Json(currentValue, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowChartViewTrendServer(int itemId = 0, int unitId = 0, int locationId = 0, bool isGenSystem = true, string datestring = "")
        {
            try
            {
                var data = new TraderItemRules(dbContext).GetDataChartViewTrend(CurrentDomainId(), itemId, unitId, locationId, isGenSystem, datestring, CurrentUser().Timezone, CurrentUser().DateTimeFormat);
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(ex, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ListContactTraderItem()
        {
            var contacts = new TraderContactRules(dbContext).GetTraderContactsByDomain(CurrentDomainId(), SalesChannelContactGroup.Trader);
            ViewBag.Countries = new CountriesRules().GetAllCountries();
            return PartialView("_TraderItemListContact", contacts);
        }

        public ActionResult TraderSpecial()
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");
                return View();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult AddTraderLocation(int id)
        {
            var location = new TraderLocation();
            if (id > 0) location = new TraderLocationRules(dbContext).GetById(id);
            return PartialView("_TraderSetupAddLocationPartial", location);
        }

        public ActionResult AddTraderTableLocation()
        {
            return PartialView("_TraderSetupTableLocationPartial", CurrentDomain().TraderLocations);
        }

        public ActionResult AddTraderProductGroup(int id)
        {
            var group = new TraderGroup();
            if (id > 0) group = new TraderGroupRules(dbContext).GetById(id);
            return PartialView("_TraderSetupAddProductGroupPartial", group);
        }

        public ActionResult AddTraderTableProductGroup()
        {
            return PartialView("_TraderSetupTableProductGroupPartial", CurrentDomain().TraderGroups);
        }

        public ActionResult AddTraderContactGroup(int id)
        {
            var contactGroup = new TraderContactGroup();
            if (id > 0) contactGroup = new TraderContactRules(dbContext).GetTraderContactsGroupById(id);
            return PartialView("_TraderSetupAddContactGroupPartial", contactGroup);
        }

        public ActionResult AddTraderTableContactGroup()
        {
            return PartialView("_TraderSetupTableContactGroupPartial", CurrentDomain().ContactGroups);
        }

        public ActionResult AddTraderBaseUnit(int id)
        {
            ViewBag.MeasurementTypeEnums = Enum.GetValues(typeof(MeasurementTypeEnum)).Cast<MeasurementTypeEnum>()
                .Select(q => q.ToString()).ToList();
            return PartialView("_TraderSetupAddBaseUnitPartial");
        }

        public ActionResult AddTraderTableBaseUnit()
        {
            return PartialView("_TraderSetupTableBaseUnitPartial");
        }

        public ActionResult AddTraderWorkGroup(int id)
        {
            var workGroup = new WorkGroup();
            var domain = CurrentDomain();
            var topics = new List<Topic>();
            if (domain.Qbicles.Any(q => q.Domain.Id == domain.Id))
                topics = new TopicRules(dbContext).GetTopicByQbicle(domain.Qbicles.FirstOrDefault().Id);
            ViewBag.DefaultTopic = topics;
            ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domain.Id);
            ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domain.Id);
            ViewBag.Process = new TraderProcessRules(dbContext).GetAll();
            ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(domain.Id);
            if (id > 0) workGroup = new TraderWorkGroupsRules(dbContext).GetById(id);
            return PartialView("_TraderSetupAddWorkGroupPartial", workGroup);
        }

        public ActionResult AddTraderTableWorkGroup()
        {
            var domainId = CurrentDomainId();
            var workGroups = new TraderWorkGroupsRules(dbContext).GetWorkGroups(domainId);
            ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
            ViewBag.Qbicles = new QbicleRules(dbContext).GetQbicleByDomainId(domainId);
            ViewBag.Process = new TraderProcessRules(dbContext).GetAll();
            ViewBag.Groups = new TraderGroupRules(dbContext).GetTraderGroupItem(domainId);
            ViewBag.DomainRoles = new DomainRolesRules(dbContext).GetDomainRolesDomainId(domainId);
            return PartialView("_TraderSetupTableWorkGroupPartial", workGroups);
        }

        public ActionResult AddTraderTableTaxRate()
        {
            var domainId = CurrentDomainId();
            var taxRates = new TaxRateRules(dbContext).GetByDomainId(domainId).ToList();
            return PartialView("_TraderSetupTableTaxRatePartial", taxRates);
        }

        public ActionResult ShowListMemberForWorkGroup(int wgId = 0, string title = null)
        {
            if (wgId > 0)
            {
                var rules = new TraderWorkGroupsRules(dbContext);
                var wg = rules.GetById(wgId);
                ViewBag.workgroup = wg;
                ViewBag.TitleModel = title;
                return PartialView("_TraderTransferShowMember", wg.Members);
            }
            else
            {
                return PartialView("_TraderTransferShowMember", new List<ApplicationUser>());
            }
        }

        public ActionResult AddTraderTaxRate(int id)
        {
            var taxRate = new TaxRate();
            if (id > 0) taxRate = new TaxRateRules(dbContext).GetById(id);
            return PartialView("_TraderSetupAddTaxRatePartial", taxRate);
        }

        public ActionResult UpdateCurrentLocationManage(int id)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                SetCurrentLocationManage(id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult PointOfSale()
        {
            try
            {
                var domain = CurrentDomain();

                var user = CurrentUser();

                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess))
                    return View("ErrorAccessPage");

                if (!CheckTraderIsSetupComplete())
                    return RedirectToAction("TraderSetup", "Trader");

                var domainId = CurrentDomainId();
                ViewBag.Locations = new TraderLocationRules(dbContext).GetTraderLocation(domainId);
                var currentLocationId = CurrentLocationManage();
                ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                    q.Location.Id == currentLocationId
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();

                return PartialView("_DeviceTab");
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult NavigationTraderPartial()
        {
            var user = CurrentUser();
            var locationId = CurrentLocationManage();
            var isPos = CurrentDomain().Workgroups.Any(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.POS))
                && q.Members.Select(u => u.Id).Contains(user.Id));

            var showShiftManagement = CurrentDomain().Workgroups.Any(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderCashManagement))
                && q.Members.Select(u => u.Id).Contains(user.Id));

            ViewBag.ShowShiftManagement = showShiftManagement;
            ViewBag.IsPos = isPos;
            return PartialView("_NavigationTraderPartial");
        }

        public ActionResult GetPosMenusByLocationIds(int[] locationIds, int itemId)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                refModel = new TraderLocationRules(dbContext).GetPosMenusByLocationIds(locationIds, itemId);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult setIsActiveByLocations(int itemId, bool isActive, int[] locationIds)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                refModel = new TraderLocationRules(dbContext).setIsActiveByLocations(itemId, isActive, locationIds);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }
    }
}