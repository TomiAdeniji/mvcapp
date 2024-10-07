using Qbicles.BusinessRules;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.MyBankMate;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class BankmateController : BaseController
    {
        // GET: Bankmate
        #region Calling to View
        public ActionResult Index()
        {
            var domainId = CurrentDomainId();
            var currentUserId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, domainId);
            if (userRoleRights.All(r => r != RightPermissions.MyBankMateAccess))
                return View("ErrorAccessPage");
            ViewBag.Banks = new MyBankMateRules(dbContext).GetBanks();
            ViewBag.Countries = new CountriesRules().GetAllCountries();

            var myBankMateRules = new MyBankMateRules(dbContext);
            ViewBag.DomainBankMateAccounts = myBankMateRules.GetBankMateAccountCustomByDomain(domainId, BankmateAccountType.Domain, true);
            ViewBag.ExternalBankMateAccounts = myBankMateRules.GetBankMateAccountCustomByDomain(domainId, BankmateAccountType.ExternalBank, true)
                                                    .OrderBy(p => p.Name).ToList();
            ViewBag.CurrentPage = "bankmate"; SetCurrentPage("bankmate");
            ViewBag.DriverBankMateAccounts = myBankMateRules.GetBankMateAccountCustomByDomain(domainId, BankmateAccountType.Driver, true);

            return View();
        }
        public ActionResult LoadModalAccountSetup(string elId)
        {
            ViewBag.elId = elId;
            return PartialView("_ModalAccountSetup");
        }
        public ActionResult DomainBMAccountDetail(int accountId)
        {
            var myBankMateRules = new MyBankMateRules(dbContext);

            var bankMateAccountCustom = myBankMateRules.GetBankMateAccountById(accountId);

            //var timeZone = UserSettings().Timezone;
            //var lstPendingTransactions = myBankMateRules.GetListAccountTransactionByStatus(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PendingApproval });
            //var lstFailedTransactions = myBankMateRules.GetListAccountTransactionByStatus(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PaymentDenied
            //                                , TraderPaymentStatusEnum.PaymentDiscarded});
            //var lstApprovedTransactions = myBankMateRules.GetListAccountTransactionByStatus(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PaymentApproved });
            //lstApprovedTransactions.ForEach(p => p.CreatedDate = p.CreatedDate.ConvertTimeFromUtc(timeZone));

            //var groupedAprrovedTransactions = lstApprovedTransactions.GroupBy(p => p.CreatedDate).ToList();

            //ViewBag.ListPendingTransactions = lstPendingTransactions;
            //ViewBag.ListFailedTransactions = lstFailedTransactions;
            //ViewBag.GroupedAprrovedTransactions = groupedAprrovedTransactions;

            var lstExternalAccounts = new MyBankMateRules(dbContext).GetBankMateAccountCustomByDomain(CurrentDomainId(), BankmateAccountType.ExternalBank, false);
            ViewBag.ListExternalAccounts = lstExternalAccounts;
            return View("DomainAccountDetail", bankMateAccountCustom);
        }
        public ActionResult AddFundModalShow(int accountId)
        {
            var cashAccountCustom = new MyBankMateRules(dbContext).GetBankMateAccountById(accountId);
            var lstExternalAccounts = new MyBankMateRules(dbContext).GetBankMateAccountCustomByDomain(CurrentDomainId(), BankmateAccountType.ExternalBank, false);
            ViewBag.ListExternalAccounts = lstExternalAccounts;

            return PartialView("_AddFundModal", cashAccountCustom);
        }
        public ActionResult WithdrawFundModalShow(int accountId)
        {
            var cashAccountCustom = new MyBankMateRules(dbContext).GetBankMateAccountById(accountId);
            var lstExternalAccounts = new MyBankMateRules(dbContext).GetBankMateAccountCustomByDomain(CurrentDomainId(), BankmateAccountType.ExternalBank, false);
            ViewBag.ListExternalAccounts = lstExternalAccounts;

            return PartialView("_WithdrawFundModal", cashAccountCustom);
        }
        public ActionResult TransactionDetailModalShow(int transactionId, int associatedAccountId)
        {
            var transactionItem = new TraderCashBankRules(dbContext).GetCashAccountTransactionById(transactionId);
            var timeZone = CurrentUser().Timezone;
            ViewBag.TimeZone = timeZone;
            ViewBag.AssociatedAccountId = associatedAccountId;
            return PartialView("_TransactionDetailModal", transactionItem);
        }
        public ActionResult DomainBMTransactionsListShow(int accountId, string daterangeString, string keysearch, string searchBankIdList = "", int showTypeId = 0, int takeNumber = 20)
        {
            var dateTimeFormat = CurrentUser().DateFormat;
            var timeZone = CurrentUser().Timezone;
            var BMRules = new MyBankMateRules(dbContext);
            var isAllApprovedTransactionShown = false;

            var bankmateAccountCustom = BMRules.GetBankMateAccountById(accountId);

            var lstPendingTransactions = BMRules.GetListTransactionForPagination(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PendingApproval },
                daterangeString, keysearch, searchBankIdList, showTypeId, dateTimeFormat, timeZone);
            var lstFailedTransactions = BMRules.GetListTransactionForPagination(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PaymentDenied,
            TraderPaymentStatusEnum.PaymentDiscarded}, daterangeString, keysearch, searchBankIdList, showTypeId, dateTimeFormat, timeZone);

            var lstApprovedTransactions = BMRules.GetListTransactionForPagination(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PaymentApproved },
                daterangeString, keysearch, searchBankIdList, showTypeId, dateTimeFormat, timeZone);
            if (lstApprovedTransactions.Count <= takeNumber)
            {
                isAllApprovedTransactionShown = true;
            }

            lstApprovedTransactions = lstApprovedTransactions.Take(takeNumber).ToList();
            lstApprovedTransactions.ForEach(p => p.CreatedDate = p.CreatedDate.ConvertTimeFromUtc(timeZone));

            var groupedAprrovedTransactions = lstApprovedTransactions.GroupBy(p => p.CreatedDate).ToList();

            ViewBag.isAllApprovedTransactionShown = isAllApprovedTransactionShown;
            ViewBag.ListPendingTransactions = lstPendingTransactions;
            ViewBag.ListFailedTransactions = lstFailedTransactions;
            ViewBag.GroupedAprrovedTransactions = groupedAprrovedTransactions;

            return PartialView("_DomainBMTransactionsList", bankmateAccountCustom);
        }
        public ActionResult GetBankmateTransactions([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, int cashAccountId, string userId = "", string keyword = "", string daterange = "", bool isPendingStatus = true)
        {
            if (SystemRoleValidation(CurrentUser().Id, SystemRoles.QbiclesBankManager))
                return Json(new MyBankMateRules(dbContext).GetBankmateTransactions(requestModel, cashAccountId, userId, keyword, daterange, CurrentUser().DateFormat, CurrentUser().Timezone, isPendingStatus), JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, string.Empty, 0, 0), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConfirmBankmateTransferModal(int transactionId)
        {
            if (SystemRoleValidation(CurrentUser().Id, SystemRoles.QbiclesBankManager))
            {
                var cashbanktransaction = new TraderCashBankRules(dbContext).GetCashAccountTransactionById(transactionId);
                ViewBag.ExternalAccounts = new MyBankMateRules(dbContext).GetBankMateAccountCustomByDomain(cashbanktransaction.OriginatingAccount.Domain.Id, BankmateAccountType.ExternalBank, false); ;
                return PartialView("~/Views/Administration/_BankmateTransferModal.cshtml", cashbanktransaction);
            }
            return View("Error");
        }
        public ActionResult GetExternalBankAccountInfo(int id)
        {
            return Json(null, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Business rules processing
        [HttpPost]
        public ActionResult ApproveCashBankAccountTransactions(int[] ids, TraderPaymentStatusEnum status, int externalBankAccountId, CashAccountTransactionTypeEnum type)
        {
            
            if (!SystemRoleValidation(CurrentUser().Id, SystemRoles.QbiclesBankManager))
            {
                ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = "ERROR_MSG_28";
                return Json(returnJson);
            }
            return Json(new MyBankMateRules(dbContext).UpdateCashBankAccountTransactions(ids, status, CurrentUser().Id, externalBankAccountId, type));
        }
        [HttpPost]
        public ActionResult RejectCashBankAccountTransactions(int[] ids, TraderPaymentStatusEnum status)
        {           
            if (!SystemRoleValidation(CurrentUser().Id, SystemRoles.QbiclesBankManager))
            {
                ReturnJsonModel returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = "ERROR_MSG_28";
                return Json(returnJson);
            }
            return Json(new MyBankMateRules(dbContext).UpdateCashBankAccountTransactions(ids, status, CurrentUser().Id));
        }
        public ActionResult CheckBankmateAccountSetup()
        {
            return Json(new MyBankMateRules(dbContext).CheckBankmateAccountSetup(CurrentDomainId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveBankmateAccountSetup(TraderCashAccount cashBank, int accountId)
        {
            var currentDomain = CurrentDomain();
            cashBank.ImageUri = currentDomain.LogoUri;
            cashBank.BankmateType = BankmateAccountType.Domain;
            cashBank.Domain = currentDomain;
            cashBank.CreatedDate = DateTime.UtcNow;
            cashBank.AssociatedBKAccount = new BKAccount { Id = accountId };
            var refModel = new TraderCashBankRules(dbContext).SaveTraderCashAccount(cashBank,CurrentUser().Id);
            return Json(refModel);
        }
        public ActionResult SaveBankmate(BankMateModel model)
        {
            var domainId = CurrentDomainId();
            var currentUserId = CurrentUser().Id;
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(currentUserId, domainId);
            if (userRoleRights.All(r => r != RightPermissions.MyBankMateAccess))
            {
                var returnJson = new ReturnJsonModel() { result = false };
                returnJson.msg = _L("ERROR_MSG_28");
                return Json(returnJson);
            }
            model.Domain = CurrentDomain();
            
            return Json(new MyBankMateRules(dbContext).SaveBankmate(model, currentUserId));
        }
        public ActionResult SaveBankmateTransaction(decimal amount, TraderCashAccount originalAccount, TraderCashAccount destinationAccount, string reference, string type, S3ObjectUploadModel s3ObjectUpload)
        {
            var description = "";

            if (originalAccount != null && originalAccount.Id > 0)
            {
                originalAccount = dbContext.TraderCashAccounts.FirstOrDefault(p => p.Id == originalAccount.Id);
                if (originalAccount == null)
                {
                    var failResult = new ReturnJsonModel()
                    {
                        result = false,
                        msg = "The Original Account does not exist."
                    };
                    return Json(failResult, JsonRequestBehavior.AllowGet);
                }
            }
            if (destinationAccount != null && destinationAccount.Id > 0)
            {
                destinationAccount = dbContext.TraderCashAccounts.FirstOrDefault(p => p.Id == destinationAccount.Id);
                if (destinationAccount == null)
                {
                    var failResult = new ReturnJsonModel()
                    {
                        result = false,
                        msg = "The Destination Account does not exist."
                    };
                    return Json(failResult, JsonRequestBehavior.AllowGet);
                }
            }

            var result = new ReturnJsonModel() { actionVal = 1, result = true };
            if (type == "fund")
            {
                description = "Transfer from: " + originalAccount.Name;
                result = new MyBankMateRules(dbContext).CreateDomainBankMateTransaction(amount, originalAccount, destinationAccount, reference,
                    description, CurrentUser().Id, CashAccountTransactionTypeEnum.PaymentIn, s3ObjectUpload, CurrentQbicleId());
            }
            else if (type == "withdraw")
            {
                description = "Transfer to: " + destinationAccount.Name;
                result = new MyBankMateRules(dbContext).CreateDomainBankMateTransaction(amount, originalAccount, destinationAccount, reference,
                    description, CurrentUser().Id, CashAccountTransactionTypeEnum.PaymentOut, s3ObjectUpload, CurrentQbicleId());
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult LoadMoreDomainBMAccountTransaction(int accountId, string daterangeString, string keysearch, int takeNumber, string searchBankIdList = "", int showTypeId = 0)
        {
            try
            {
                var cubeId = CurrentQbicleId();
                var BMRules = new MyBankMateRules(dbContext);
                var dateTimeFormat = CurrentUser().DateFormat;
                var timeZone = CurrentUser().Timezone;

                var lstApprovedTransactions = BMRules.GetListTransactionForPagination(accountId, new List<TraderPaymentStatusEnum>() { TraderPaymentStatusEnum.PaymentApproved },
                daterangeString, keysearch, searchBankIdList, showTypeId, dateTimeFormat, timeZone);
                var totalRecords = lstApprovedTransactions.Count;
                lstApprovedTransactions = lstApprovedTransactions.Take(takeNumber).ToList();

                var groupedAprrovedTransactions = lstApprovedTransactions.GroupBy(p => p.CreatedDate).ToList();

                var modelString = RenderLoadNextViewToString("_DomainApprovedTransactionsList", groupedAprrovedTransactions, accountId);
                var result = Json(new { ModelString = modelString, ModelCount = totalRecords },
                JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return null;
            }
        }

        public string RenderLoadNextViewToString(string viewName, object model, int accountId)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewBag.AccountId = accountId;
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);

                return sw.GetStringBuilder().ToString();
            }
        }
        #endregion
    }
}