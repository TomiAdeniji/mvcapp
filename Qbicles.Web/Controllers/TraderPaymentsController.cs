using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderPaymentsController : BaseController
    {
        public ActionResult PaymentReview(int id, bool reload = false)
        {
            try
            {
                var rule = new TraderCashBankRules(dbContext);
                var paymentModel = rule.GetCashAccountTransactionById(id);
                var currentDomainId = paymentModel?.Workgroup.Qbicle.Domain.Id ?? 0;
                ValidateCurrentDomain(paymentModel?.Workgroup?.Qbicle.Domain ?? CurrentDomain(), paymentModel.Workgroup?.Qbicle.Id ?? 0);
                if (!reload)
                {
                    ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                    SetCurrentApprovalIdCookies(paymentModel.PaymentApprovalProcess?.Id ?? 0);
                    
                    return View(paymentModel);
                }
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");

                var traderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(paymentModel.PaymentApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

                ViewBag.TraderApprovalRight = traderApprovalRight;

                var timeline = rule.PaymentApprovalStatusTimeline(id, user.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                if (paymentModel.AssociatedSale?.Id > 0)
                {
                    ViewBag.TraderTitle = "Trader - Sale #" + paymentModel.AssociatedSale.Reference.FullRef;
                }
                else if (paymentModel.AssociatedPurchase?.Id > 0)
                {
                    ViewBag.TraderTitle = "Trader - Purchase #" + paymentModel.AssociatedPurchase.Reference.FullRef;
                }
                else
                {
                    ViewBag.TraderTitle = "Trader - Payment #" + id;
                }

                return PartialView("_PaymentReviewContent", paymentModel);

            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult PaymentManage(int id)
        {
            try
            {
                ViewBag.GoBackPage = CurrentGoBackPage();
                this.SetCookieGoBackPage();
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                ViewBag.TraderTitle = "Trader - Sale#";

                var user = CurrentUser();

                var rule = new TraderCashBankRules(dbContext);
                var paymentModel = rule.GetCashAccountTransactionById(id);
                var domain = paymentModel.Workgroup != null ? paymentModel.Workgroup.Domain : CurrentDomain();
                ValidateCurrentDomain(domain, paymentModel.Workgroup?.Qbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, domain.Id);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                if (paymentModel.AssociatedSale?.Id > 0)
                {
                    ViewBag.TraderTitle = "Trader - Sale #" + paymentModel.AssociatedSale.Reference.FullRef;
                }
                else if (paymentModel.AssociatedPurchase?.Id > 0)
                {
                    ViewBag.TraderTitle = "Trader - Purchase #" + paymentModel.AssociatedPurchase.Reference.FullRef;
                }
                else
                {
                    ViewBag.TraderTitle = "Trader - Payment #" + id;
                }


                var timeline = rule.PaymentApprovalStatusTimeline(id, user.Timezone);
                var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(paymentModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),ex, CurrentUser().Id);
                return View("Error");
            }
        }


        public ActionResult InvoicePaymentContent(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            var paymentModel = new TraderCashBankRules(dbContext).GetByInvoice(id);
            var invoiceModel = dbContext.Invoices.FirstOrDefault(e => e.Id == id)?.TotalInvoiceAmount;
            ViewBag.PaymentFull = invoiceModel == paymentModel.Sum(e => e.Amount);
            ViewBag.InvoiceId = id;
            ViewBag.IsBusiness = !IsCreatorTheCustomer();
            return PartialView("_TraderInvoicePaymentContent", paymentModel);
        }

        public ActionResult TraderInvoicePaymentAdd(int invoiceId)
        {
            try
            {
                var model = new TraderInvoicesRules(dbContext).GetById(invoiceId);
                ViewBag.Payment = new CashAccountTransaction();

                var locationId = model.Sale?.Location?.Id ?? model.Purchase?.Location?.Id ?? 0;
                var user = CurrentUser();
                ViewBag.Workgroups = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == locationId &&
                    q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPaymentProcessName))
                    && q.Members.Select(u => u.Id).Contains(user.Id)).OrderBy(n => n.Name).ToList();

                var cashBankRule = new TraderCashBankRules(dbContext);
                ViewBag.TraderAccounts = cashBankRule.GetTraderCashAccounts(CurrentDomainId());
                ViewBag.PaymentMethods = dbContext.PaymentMethods.ToList();
                return PartialView("_TraderInvoicePaymentAddEdit", model);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return View("Error");
            }

        }


        public ActionResult TraderInvoicePaymentEdit(int paymentId)
        {
            var payment = new TraderCashBankRules(dbContext).GeCashAccountTransactionById(paymentId);
            ViewBag.Payment = payment;
            var model = payment.AssociatedInvoice;
            var locationId = model.Sale?.Location?.Id ?? model.Purchase?.Location?.Id ?? 0;

            ViewBag.Workgroups = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId &&
                q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPaymentProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            var cashBankRule = new TraderCashBankRules(dbContext);
            ViewBag.TraderAccounts = cashBankRule.GetTraderCashAccounts(CurrentDomainId());
            ViewBag.PaymentMethods = dbContext.PaymentMethods.ToList();
            return PartialView("_TraderInvoicePaymentAddEdit", model);
        }


        [HttpPost]
        public ActionResult SaveInvoicePayment(CashAccountTransaction invoicePayment, CashAccountTransaction traderInvoiceAssociatedFiles, List<MediaModel> traderInvoiceAttachments)
        {
            try
            {                
                //1. save payment
                var payment = new TraderCashBankRules(dbContext).SaveCashAccountPayment(invoicePayment, CurrentUser().Id);
                var mediaRules = new MediasRules(dbContext);

                if (traderInvoiceAssociatedFiles?.AssociatedFiles?.Count > 0)
                    mediaRules.UpdateAttachmentsInvoice(traderInvoiceAssociatedFiles, payment);

                if (traderInvoiceAttachments?.Count > 0)
                    mediaRules.SaveNewAttachmentsInvoice(payment, traderInvoiceAttachments, payment.CreatedBy.Id);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(),e, CurrentUser().Id);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
    }
}