using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderCashBankController : BaseController
    {
        public ActionResult TraderCashBankContents()
        {
            try
            {
                ViewBag.CurrentDomainId = CurrentDomainId();
                return PartialView("_TraderCashBankContent");
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return PartialView("_TraderCashBankContent", new List<TraderCashAccount>());
            }
        }
        public ActionResult GetCashBankContent([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keysearch = "")
        {
            keysearch = keysearch.ToLower().Trim();
            var domain = CurrentDomain();
            var result = new TraderCashBankRules(dbContext).TraderCashBankSearch(requestModel, keysearch, CurrentLocationManage(), CurrentDomainId(), CurrentUser().Id);

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<CashBankCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
        public ActionResult CashAccountTransactionContents(int id)
        {
            try
            {
                ViewBag.TraderCashAccountId = id;
                var destinationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashId(id, "Destination");
                var originAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashId(id, "Originating");
                destinationAccounts.AddRange(originAccounts);

                foreach (var item in destinationAccounts)
                {
                    item.CreatedDate = item.CreatedDate.ConvertTimeFromUtc(CurrentUser().Timezone);

                    if (item.AssociatedSale != null && item.AssociatedSale.SalesChannel == Qbicles.Models.Trader.SalesChannel.SalesChannelEnum.POS)
                    {
                        item.Reference = PaymentReferenceConst.PosCashPaymentReferenceString;
                    }
                }
                return PartialView("_TraderCashAccountTransactionContent", destinationAccounts);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return PartialView("_TraderCashAccountTransactionContent", new List<CashAccountTransaction>());
            }
        }

        public ActionResult TraderCashBankAddEdit(int id = 0)
        {
            try
            {
                var domainId = CurrentDomainId();
                var bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
                ViewBag.TreeView = bkGroup.Any() ? BKConfigurationRules.GenTreeView(bkGroup.ToList()) : "";
                var model = new TraderCashAccount();
                if (id > 0)
                    model = new TraderCashBankRules(dbContext).GeTraderCashAccountById(id);
                return PartialView("_TraderCashBankPartial", model);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult ShowCashAccountTransactionAttachments(int id)
        {
            try
            {
                //id: CashAccountTransactionId
                var attachments = new TraderCashBankRules(dbContext).ShowCashAccountTransactionAttachments(id);
                ViewBag.TransactionId = id;
                return PartialView("_TransactionAttachments", attachments);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }


        [HttpPost]
        public ActionResult SaveCashAccountPayment(CashAccountTransaction payment, CashAccountTransaction traderCashBankAssociatedFiles, List<MediaModel> traderCashBankAttachments)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            if (payment.Id > 0) refModel.actionVal = 2;
            try
            {
                var rule = new TraderCashBankRules(dbContext);
                var userId = CurrentUser().Id;
                var cashAccountTransaction = rule.SaveCashAccountPayment(payment, userId);


                var mediaRules = new MediasRules(dbContext);

                if (traderCashBankAssociatedFiles?.AssociatedFiles?.Count > 0)
                    mediaRules.UpdateAttachmentsCashAccountTransaction(traderCashBankAssociatedFiles, cashAccountTransaction);

                if (traderCashBankAttachments?.Count > 0)
                    mediaRules.SaveNewAttachmentsCashAccountTransaction(cashAccountTransaction, traderCashBankAttachments, userId);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RenderSelectSalePurchasePayment(int locationId)
        {
            var domainId = CurrentDomainId();
            var model = new List<SalePurchasePaymentModel>();
            if (locationId > 0)
                model = new TraderCashBankRules(dbContext).GeSalePurchasePaymentByLocationId(locationId, domainId);
            return PartialView("_SelectSalePurchasePayment", model);
        }

        public ActionResult TraderCashBankTransfer(int cashAccountId, int locationId)
        {
            try
            {
                var domainId = CurrentDomainId();
                var rule = new TraderCashBankRules(dbContext);
                ViewBag.LocationId = locationId;
                ViewBag.CashAccountCurrent = rule.GeTraderCashAccountById(cashAccountId) ?? new TraderCashAccount();
                ViewBag.TraderCashAccounts = rule.GetTraderCashAccounts(domainId);
                ViewBag.Destinations =
                    new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashId(cashAccountId, "Destination");
                ViewBag.Originatings =
                    new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashId(cashAccountId, "Originating");

                return PartialView("_TraderCashBankTransfer");
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult TraderCashBankFormTransfer(int index = 0)
        {
            try
            {
                var domainId = CurrentDomainId();
                var rule = new TraderCashBankRules(dbContext);
                ViewBag.TraderCashAccounts = rule.GetTraderCashAccounts(domainId);
                ViewBag.Index = index;
                var transaction = new CashAccountTransaction();

                return PartialView("_TraderCashBankFormTransfer", transaction);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult UpdateFilename(int idFile, string fileName, int idForm)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                if (idFile > 0 && idForm > 0)
                {
                    var formTrans = new TraderCashBankRules(dbContext).GetCashAccountTransactionById(idForm);
                }
            }
            catch (Exception ex)
            {
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public ActionResult TraderCashAccountNameCheck(TraderCashAccount cashBank)
        {
            var refModel = new ReturnJsonModel { result = true };
            try
            {
                cashBank.Domain = CurrentDomain();
                if (new TraderCashBankRules(dbContext).TraderCashAccountNameCheck(cashBank))
                {
                    refModel.actionVal = 3;
                    refModel.msg = "\"" + cashBank.Name + "\": already exists";
                    refModel.result = false;

                    return Json(refModel, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                refModel.result = false;
            }

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveTraderCashAccount(TraderCashAccount cashBank, string mediaObjectKey, int accountId)
        {

            cashBank.ImageUri = mediaObjectKey;
            cashBank.AssociatedBKAccount = new BKAccount
            {
                Id = accountId
            };
            cashBank.Domain = CurrentDomain();

            var refModel = new TraderCashBankRules(dbContext).SaveTraderCashAccount(cashBank, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteFile(int fileId, int accId)
        {
            var rules = new TraderCashBankRules(dbContext);
            return Json(rules.DeleteCashAccountTransactionFile(fileId, accId) ? "OK" : "Fail", JsonRequestBehavior.AllowGet);
        }
        [HttpDelete]
        public ActionResult DeleteTraderCashAccount(int id)
        {
            var rules = new TraderCashBankRules(dbContext);
            return Json(rules.DeleteCashBank(id) ? "OK" : "Fail", JsonRequestBehavior.AllowGet);
        }
        public ActionResult TraderCashAccount(int id = 0, int locationid = 0)
        {
            var associatedSafe = new CMsRules(dbContext).GetSafesByBankAccount(id);
            ViewBag.HasAssociatedSafe = associatedSafe.Count > 0 ? true : false;

            ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");
            var traderCashAccount = new TraderCashAccount();
            ViewBag.LocationId = locationid;
            var destinationAccounts = new List<CashAccountTransaction>();
            var originationAccounts = new List<CashAccountTransaction>();
            if (id > 0)
            {
                traderCashAccount = new TraderCashBankRules(dbContext).GeTraderCashAccountById(id);
                destinationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashAccountId(new List<int>() { id }, "Destination");
                originationAccounts = new TraderCashBankRules(dbContext).GetCashAccountTransactionByCashAccountId(new List<int>() { id }, "Originating");

            }

            var cashBankRules = new TraderCashBankRules(dbContext);
            var cashAccountTransactions = new List<CashAccountTransaction>();
            cashAccountTransactions.AddRange(destinationAccounts);
            cashAccountTransactions.AddRange(originationAccounts);

            var inTransactionsSum = cashAccountTransactions
                .Where(i => i.Status == TraderPaymentStatusEnum.PaymentApproved
                    && cashBankRules.GetTransactionDirection(i, id).ToLower() == "in")
                .Sum(a => a.Amount);

            var outTransactionsSum = cashAccountTransactions
                .Where(i => i.Status == TraderPaymentStatusEnum.PaymentApproved
                    && cashBankRules.GetTransactionDirection(i, id).ToLower() == "out")
                .Sum(a => a.Amount);

            ViewBag.InTransactionsSum = inTransactionsSum;
            ViewBag.OutTransactionsSum = outTransactionsSum;
            ViewBag.DestinationAccounts = destinationAccounts;
            ViewBag.OriginationAccounts = originationAccounts;
            
            var domainId = CurrentDomainId();
            var userId = CurrentUser().Id;
            var isMemberWorkGroup = dbContext.WorkGroups.Where(q => q.Location.Domain.Id == domainId && q.Processes.Any(p => p.Name == TraderProcessName.TraderPaymentProcessName)
                                                                            && q.Members.Select(u => u.Id).Contains(userId));
            if (locationid > 0)
                isMemberWorkGroup = isMemberWorkGroup.Where(q => q.Location.Id == locationid);

            ViewBag.IsMemberWorkGroup = isMemberWorkGroup.Any();

            ViewBag.GoBackPage = CurrentGoBackPage();
            return View(traderCashAccount);
        }

        [HttpPost]
        public ActionResult LoadPaymentTransaction([Bind(Prefix = "order[0][column]")] int column, [Bind(Prefix = "order[0][dir]")] string orderby, int id, string fromDate, string toDate, string search, int draw, int start, int length)
        {
            var totalRecord = 0;
            List<CashAccountTransactionModel> lstResult = new TraderCashBankRules(dbContext).GetListPaymentTransaction(column, orderby, CurrentLocationManage(), id, fromDate, toDate, search, CurrentDomain(), CurrentUser().Id, CurrentUser().Timezone, start, length, ref totalRecord, CurrentUser().DateFormat);
            var dataTableData = new DataTableModel
            {
                draw = draw,
                data = lstResult,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord
            };

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddPayment(int id = 0, int locationId = 0, int transactionId = 0)
        {
            var domainId = CurrentDomainId();
            ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(domainId).Where(q => q.Status == TraderContactStatusEnum.ContactApproved).ToList();
            var traderCashAccount = new TraderCashBankRules(dbContext).GeTraderCashAccountById(id);
            ViewBag.TraderCashAccount = traderCashAccount;
            ViewBag.Countries = new CountriesRules().GetAllCountries();
            ViewBag.LocationId = locationId;
            ViewBag.ContactGroups =
                    new TraderContactRules(dbContext).GetTraderContactsGroupByDomain(domainId, SalesChannelContactGroup.Trader);
            var bkGroup = dbContext.BKGroups.Where(e => e.Domain.Id == domainId).ToList();
            ViewBag.TreeView = bkGroup.Any() ? BKConfigurationRules.GenTreeView(bkGroup.ToList()) : "";
            var traderCashAccountTransaction = new TraderCashBankRules(dbContext).GeCashAccountTransactionById(transactionId) ??
                                               new CashAccountTransaction();
            
            var userId = CurrentUser().Id;
            var workgroupTransfer = dbContext.WorkGroups.Where(q => q.Location.Domain.Id == domainId && q.Processes.Any(p => p.Name == TraderProcessName.TraderPaymentProcessName)
                                                                            && q.Members.Select(u => u.Id).Contains(userId));
            if (locationId > 0)
                workgroupTransfer = workgroupTransfer.Where(q => q.Location.Id == locationId);

            ViewBag.WorkgroupTransfer = workgroupTransfer.ToList();
            ViewBag.PaymentMethods = dbContext.PaymentMethods.OrderBy(n => n.Name).ToList();
            return PartialView("_TraderCashBankAddPayment", traderCashAccountTransaction);
        }
        public ActionResult AddTransfer(int id = 0, int locationId = 0, int transactionId = 0)
        {
            var domainId = CurrentDomainId();
            var traderCashAccount = new TraderCashBankRules(dbContext).GeTraderCashAccountById(id);
            var traderCashAccountLst = new TraderCashBankRules(dbContext).GetTraderCashAccounts(domainId, false);
            ViewBag.TraderCashAccountDestination = traderCashAccountLst;
            ViewBag.TraderCashAccount = traderCashAccount;

            var paymentInTransaction = dbContext.CashAccountTransactions.Where(c => c.DestinationAccount.Id == id && c.Status == TraderPaymentStatusEnum.PaymentApproved).ToList();
            var paymentOutTransaction = dbContext.CashAccountTransactions.Where(c => c.OriginatingAccount.Id == id && c.Status == TraderPaymentStatusEnum.PaymentApproved).ToList();
            var paymentInAmount = paymentInTransaction.Sum(q => q.Amount);
            var paymentOutAmount = paymentOutTransaction.Sum(q => q.Amount);
            var chargesAmount = paymentInTransaction.Sum(q => q.Charges) + paymentOutTransaction.Sum(q => q.Charges);
            ViewBag.Amounts = (paymentInAmount - paymentOutAmount - chargesAmount);

            var traderCashAccountTransaction = new TraderCashBankRules(dbContext).GeCashAccountTransactionById(transactionId) ??
                                               new CashAccountTransaction();


            var userId = CurrentUser().Id;
            var workgroupTransfer = dbContext.WorkGroups.Where(q => q.Location.Domain.Id == domainId && q.Processes.Any(p => p.Name == TraderProcessName.TraderPaymentProcessName)
                                                                            && q.Members.Select(u => u.Id).Contains(userId));
            if (locationId > 0)
                workgroupTransfer = workgroupTransfer.Where(q => q.Location.Id == locationId);

            ViewBag.WorkgroupTransfer = workgroupTransfer.ToList();
            return PartialView("_TraderCashBankAddTransfer", traderCashAccountTransaction);
        }
        public ActionResult GetInvoicesInOutByLocation([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel,int locationId, bool isIN, string keyword, string searchDate)
        {
            var result = new TraderInvoicesRules(dbContext).GetInvoicesInOutByLocation(requestModel, isIN, keyword, locationId, searchDate, CurrentUser(), CurrentDomainId());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }
    }
}