using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.Trader;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderInvoicesController : BaseController
    {
        public ActionResult InvoiceReview(string key)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                var rule = new TraderInvoicesRules(dbContext);
                var invoiceModel = rule.GetById(id);

                if (invoiceModel == null)
                    return View("Error");

                var user = CurrentUser();

                var currentDomainId = invoiceModel?.Workgroup.Qbicle.Domain.Id ?? 0;
                ValidateCurrentDomain(invoiceModel?.Workgroup.Qbicle.Domain, invoiceModel?.Workgroup.Qbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                var traderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(invoiceModel.InvoiceApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, user.Id);

                ViewBag.TraderApprovalRight = traderApprovalRight;

                SetCurrentApprovalIdCookies(invoiceModel.InvoiceApprovalProcess?.Id ?? 0);

                var totalTax = invoiceModel.InvoiceItems.Sum(q => q.InvoiceTaxValue * q.InvoiceItemQuantity) ?? 0;
                var totalValue = invoiceModel.InvoiceItems.Sum(q => q.InvoiceValue);

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoiceSaleTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                var timeline = rule.InvoiceBillApprovalStatusTimeline(invoiceModel.Id, user.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(invoiceModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult InvoiceReviewContent(string key)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                var user = CurrentUser();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                var rule = new TraderInvoicesRules(dbContext);

                var invoiceModel = rule.GetById(id);

                ValidateCurrentDomain(invoiceModel.Workgroup.Qbicle.Domain, invoiceModel?.Workgroup.Qbicle.Id ?? 0);

                var traderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(invoiceModel.InvoiceApprovalProcess.ApprovalRequestDefinition.Id, user.Id);

                ViewBag.TraderApprovalRight = traderApprovalRight;

                SetCurrentApprovalIdCookies((int)invoiceModel.InvoiceApprovalProcess?.Id);

                var totalTax = invoiceModel.InvoiceItems.Sum(q => q.InvoiceTaxValue) ?? 0;
                var totalValue = invoiceModel.InvoiceItems.Sum(q => q.InvoiceValue);

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoiceSaleTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                var timeline = rule.InvoiceBillApprovalStatusTimeline(invoiceModel.Id, user.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return PartialView("_InvoiceReviewContent", invoiceModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult InvoiceManage(string key)
        {
            try
            {
                var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                ViewBag.GoBackPage = CurrentGoBackPage();
                var user = CurrentUser();
                this.SetCookieGoBackPage();
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(user.Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var rule = new TraderInvoicesRules(dbContext);

                var invoiceModel = rule.GetById(id);
                if (invoiceModel == null)
                    return View("Error");
                var domain = invoiceModel.Workgroup?.Qbicle.Domain ?? CurrentDomain();
                var currentUserId = user.Id;
                var allowAccessPage = domain.Administrators.Any(s => s.Id == currentUserId)
                || (invoiceModel.Workgroup != null
                && invoiceModel.Workgroup.Members.Any(q => q.Id == currentUserId)
                && invoiceModel.Workgroup.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                );
                if (!allowAccessPage)
                    return View("ErrorAccessPage");
                ValidateCurrentDomain(domain, invoiceModel?.Workgroup.Qbicle.Id ?? 0);

                var totalTax = invoiceModel.InvoiceItems.Sum(q => q.InvoiceTaxValue * q.InvoiceItemQuantity) ?? 0;
                var totalValue = invoiceModel.InvoiceItems.Sum(q => q.InvoiceValue);

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoiceSaleTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                ViewBag.imgTop = invoiceModel.Workgroup.Qbicle.Domain.LogoUri;
                ViewBag.imgBottom = invoiceModel.Workgroup.Qbicle.LogoUri;

                var timeline = rule.InvoiceBillApprovalStatusTimeline(invoiceModel.Id, user.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(invoiceModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateInvoiceReview(Invoice invoice)
        {
            invoice.Id = string.IsNullOrEmpty(invoice.Key) ? 0 : int.Parse(invoice.Key.Decrypt());
            var result = new TraderInvoicesRules(dbContext).UpdateInvoiceReview(invoice);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DownloadFile(int invoiceId)
        {
            try
            {
                var rule = new TraderInvoicesRules(dbContext);
                var iv = rule.GetById(invoiceId);
                string uri;
                if (!string.IsNullOrEmpty(iv.InvoicePDF))
                {
                    uri = iv.InvoicePDF;
                }
                else
                {
                    string imageTop = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.Domain?.LogoUri);
                    string imageBottom = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.LogoUri);

                    var fileStreams = rule.ReportSaleInvoice(iv, imageTop, imageBottom, CurrentUser().Timezone, CurrentDomainId());
                    var filePath = Server.MapPath($"~/App_Data/invoice-{invoiceId}.pdf");

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        fs.Write(fileStreams, 0, fileStreams.Length);
                    }

                    var uriKey = await UploadMediaFromPath($"invoice-{invoiceId}.pdf", filePath);
                    rule.IssueInvoice(invoiceId, uriKey);
                    uri = uriKey;
                    System.IO.File.Delete(filePath);
                }
                var fileString = GetDocumentRetrievalUrl(uri);
                return Json(fileString, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                if (ex.Message.Contains("Authorization has been denied for this request"))
                    return RedirectToAction("Login", "Account");
                return null;
            }
        }

        [HttpPost]
        public async Task<ActionResult> IssueInvoice(int id)
        {
            try
            {
                var qbicleUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port) + "Account/CreateAccount";
                var result = new ReturnJsonModel();
                var rule = new TraderInvoicesRules(dbContext);
                var iv = rule.GetById(id);
                if (!string.IsNullOrEmpty(iv.InvoicePDF))
                {
                    new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, await GetMediaFileBaseStreamAsync(iv.InvoicePDF), qbicleUri, IssueType.Invoice);
                    result.result = true;
                    result.msg = iv.InvoicePDF;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                string imageTop = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.Domain?.LogoUri);
                string imageBottom = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.LogoUri);

                var fileStreams = rule.ReportSaleInvoice(iv, imageTop, imageBottom, CurrentUser().Timezone, CurrentDomainId());
                var filePath = Server.MapPath($"~/App_Data/invoice-{id}.pdf");

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    fs.Write(fileStreams, 0, fileStreams.Length);
                }

                var uriKey = await UploadMediaFromPath($"invoice-{id}.pdf", filePath);

                result = rule.IssueInvoice(id, uriKey);
                result.msg = uriKey;
                new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, new MemoryStream(fileStreams), qbicleUri, IssueType.Invoice);

                System.IO.File.Delete(filePath);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(new ReturnJsonModel { result = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}