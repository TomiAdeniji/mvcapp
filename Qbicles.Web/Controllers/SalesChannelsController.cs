using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.PoS;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader.ODS;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class SalesChannelsController : BaseController
    {

        public ActionResult SalesChannelB2CContent()
        {
            try
            {
                var userSetting = CurrentUser();
                var isDomainAdmin = CurrentDomain().Administrators.Any(p => p.Id == userSetting.Id);
                var rightApps = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
                var visibleMenu = isDomainAdmin || rightApps.Any(r => r == RightPermissions.QbiclesBusinessAccess);

                if (!visibleMenu)
                    return View("ErrorAccessPage");

                var b2cSettings = new CommerceRules(dbContext).GetB2CConfigByLocationId(CurrentLocationManage());


                return PartialView("_B2CChannelSettings", b2cSettings);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SalesChannelB2BContent()
        {
            try
            {
                var userSetting = CurrentUser();
                var isDomainAdmin = CurrentDomain().Administrators.Any(p => p.Id == userSetting.Id);
                var rightApps = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
                var visibleMenu = isDomainAdmin || rightApps.Any(r => r == RightPermissions.QbiclesBusinessAccess);

                if (!visibleMenu)
                    return View("ErrorAccessPage");

                var b2bSettings = new CommerceRules(dbContext).GetB2BConfigByLocationId(CurrentLocationManage());


                return PartialView("_B2BChannelSettings", b2bSettings);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult LoadB2C_WG_AC_ST_Default(int locationId)
        {
            var currentDomainId = CurrentDomainId();
            var orderSettings = new CommerceRules(dbContext).GetB2COrderSettingDefault(currentDomainId, CurrentUser().Id, locationId);
            var orderStatus = HelperClass.EnumModel.GetEnumValuesAndDescriptions<PrepQueueStatus>();
            var listPaymentAccount = new TraderCashBankRules(dbContext).GetTraderCashAccounts(currentDomainId).Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPaymentAccountId ? true : false) }).Add(new Select2Option { id = "0", text = " ", selected = false }).OrderBy(o => o.text).ToList();
            var listWorkgroup = new TraderWorkGroupsRules(dbContext).GetWorkGroupsByLocationId(locationId);
            var listSaleWG = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Sale")).Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings?.DefaultSaleWorkGroupId?true:false)}).Add(new Select2Option { id = "0", text = " ", selected = false }).OrderBy(o=>o.text).ToList();
            var listInvoiceWG = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Invoice")).Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultInvoiceWorkGroupId? true : false) }).Add(new Select2Option { id = "0", text = " ", selected = false }).OrderBy(o => o.text).ToList();
            var listPaymentWG = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Payment")).Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPaymentWorkGroupId  ? true : false) }).Add(new Select2Option { id = "0", text = " ", selected = false }).OrderBy(o => o.text).ToList();
            var listTransferWG = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Transfer")).Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultTransferWorkGroupId ? true : false) }).Add(new Select2Option { id = "0", text = " ", selected = false }).OrderBy(o => o.text).ToList();
            var listb2cOrderStatus = orderStatus.Select(h => new Select2Option { id = h.Key.ToString(), text = h.Value, selected = h.Key == (int)orderSettings.B2cOrder }).ToList();
            return Json(new
            {
                PaymentAccounts = listPaymentAccount,
                SaleWorkgroups = listSaleWG,
                InvoiceWorkgroups = listInvoiceWG,
                PaymentWorkgroups = listPaymentWG,
                TransferWorkgroups = listTransferWG,
                B2cOrderStatus = listb2cOrderStatus,
                SaveSettings = orderSettings.SaveSettings,
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadB2B_WG_AC_ST_Default(int locationId)
        {
            var currentDomainId = CurrentDomainId();
            var orderSettings = new CommerceRules(dbContext).GetB2BOrderSettingDefault(currentDomainId, CurrentUser().Id, locationId);
            var orderStatus = HelperClass.EnumModel.GetEnumValuesAndDescriptions<PrepQueueStatus>();
            var listPaymentAccount = new TraderCashBankRules(dbContext).GetTraderCashAccounts(currentDomainId);
            var listWorkgroup = new TraderWorkGroupsRules(dbContext).GetWorkGroupsByLocationId(locationId);
            #region Workgroups by processes
            var _wgSale = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Sale"));
            var _wgInvoice = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Invoice"));
            var _wgPayment = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Payment"));
            var _wgTransfer = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Transfer"));
            var _wgPurchase = listWorkgroup.Where(p => p.Processes.Any(x => x.Name == "Purchase"));
            #endregion
            var listb2bOrderStatus = orderStatus.Select(h => new Select2Option { id = h.Key.ToString(), text = h.Value, selected = h.Key == (int)orderSettings.B2bOrder }).ToList();
            //Sale
            var listSaleWG = _wgSale.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultSaleWorkGroupId ? true : false) }).ToList();
            var listInvoiceWG = _wgInvoice.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultInvoiceWorkGroupId ? true : false) }).ToList();
            var listPaymentWG = _wgPayment.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPaymentWorkGroupId ? true : false) }).ToList();
            var listTransferWG = _wgTransfer.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultTransferWorkGroupId ? true : false) }).ToList();
            var listSalePaymentAccount = listPaymentAccount.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPaymentAccountId ? true : false) }).ToList();
            //Purchase
            var listPurchaseWG = _wgSale.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPurchaseWorkGroupId ? true : false) }).ToList();
            var listBillWG = _wgInvoice.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultBillWorkGroupId ? true : false) }).ToList();
            var listPurchasePaymentWG = _wgPayment.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPurchasePaymentWorkGroupId ? true : false) }).ToList();
            var listPurchaseTransferWG = _wgTransfer.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPurchaseTransferWorkGroupId ? true : false) }).ToList();
            var listPurchasePaymentAccount = listPaymentAccount.Select(s => new Select2Option { id = s.Id.ToString(), text = s.Name, selected = (s.Id == orderSettings.DefaultPurchasePaymentAccountId ? true : false) }).ToList();
            return Json(new
            {
                B2bOrderStatus = listb2bOrderStatus,
                //Sale
                SaleWorkgroups = listSaleWG,
                InvoiceWorkgroups = listInvoiceWG,
                PaymentWorkgroups = listPaymentWG,
                TransferWorkgroups = listTransferWG,
                SalePaymentAccounts = listSalePaymentAccount,
                //Purchase
                PurchaseWorkgroups = listPurchaseWG,
                BillWorkgroups= listBillWG,
                PurchasePaymentWorkgroups= listPurchasePaymentWG,
                PurchaseTransfertWorkgroups= listPurchaseTransferWG,
                PurchasePaymentAccounts= listPurchasePaymentAccount
            }, JsonRequestBehavior.AllowGet);
        }
    }
}