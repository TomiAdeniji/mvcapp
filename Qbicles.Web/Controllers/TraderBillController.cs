using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderBillController : BaseController
    {
        public async Task<ActionResult> BillManage(int id = 0, string key = "")
        {
            try
            {
                if (!key.IsNullOrEmpty())
                {
                    id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
                }
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "bookkeeping"; SetCurrentPage("bookkeeping");

                var rule = new TraderInvoicesRules(dbContext);

                var billModel = rule.GetById(id);
                if (billModel == null)
                    return View("Error");

                if (billModel.Purchase == null)
                    return View("Error");
                ValidateCurrentDomain(billModel.Workgroup?.Qbicle.Domain ?? CurrentDomain(), billModel.Workgroup?.Qbicle.Id ?? 0);

                decimal totalValue = 0;
                decimal totalTax = 0;
                foreach (var item in billModel.InvoiceItems)
                {
                    var taxRatePercent = item.TransactionItem.TraderItem.SumTaxRatesPercent(false);
                    var taxValue = (item.InvoiceValue * taxRatePercent) / 100;
                    totalTax += taxValue;
                    totalValue += item.InvoiceValue;
                }
                totalTax = billModel.InvoiceItems.Sum(q => q.InvoiceTaxValue ?? 0);
                totalValue = billModel.InvoiceItems.Sum(q => q.InvoiceValue);

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoiceSaleTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                ViewBag.imgTop = await GetMediaFileBase64Async(billModel.Workgroup.Qbicle.Domain.LogoUri);
                ViewBag.imgBottom = await GetMediaFileBase64Async(billModel.Workgroup.Qbicle.LogoUri);

                var billImage = "/Content/DesignStyle/img/icon_file_pdf.png";
                var billUpload = dbContext.StorageFiles.FirstOrDefault(e => e.Id == billModel.InvoicePDF);
                var fileType = Path.GetExtension(billUpload?.Name ?? "pdf").Replace(".", "");
                switch (fileType)
                {
                    case "ppt":
                        billImage = "/Content/DesignStyle/img/icon_file_ppt.png";
                        break;

                    case "pptx":
                        billImage = "/Content/DesignStyle/img/icon_file_pptx.png";
                        break;

                    case "doc":
                        billImage = "/Content/DesignStyle/img/icon_file_doc.png";
                        break;

                    case "docx":
                        billImage = "/Content/DesignStyle/img/icon_file_docx.png";
                        break;

                    case "xls":
                        billImage = "/Content/DesignStyle/img/icon_file_xls.png";
                        break;

                    case "xlsx":
                        billImage = "/Content/DesignStyle/img/icon_file_xlsx.png";
                        break;

                    case "jpg":
                        billImage = "/Content/DesignStyle/img/icon_file_jpg.png";
                        break;

                    case "jpeg":
                        billImage = "/Content/DesignStyle/img/icon_file_jpeg.png";
                        break;

                    case "png":
                        billImage = "/Content/DesignStyle/img/icon_file_png.png";
                        break;

                    case "zip":
                        billImage = "/Content/DesignStyle/img/icon_file_zip.png";
                        break;

                    case "pdf":
                        billImage = "/Content/DesignStyle/img/icon_file_pdf.png";
                        break;

                    case "gif":
                        billImage = "/Content/DesignStyle/img/icon_file_gif.png";
                        break;

                    case "mp4":
                        billImage = "/Content/DesignStyle/img/media-item-video.jpg";
                        break;

                    case "webm":
                        billImage = "/Content/DesignStyle/img/media-item-video.jpg";
                        break;

                    case "ogv":
                        billImage = "/Content/DesignStyle/img/media-item-video.jpg";
                        break;
                }
                ViewBag.BillImage = billImage;
                //Timeline
                var timeline = rule.InvoiceBillApprovalStatusTimeline(billModel.Id, CurrentUser().Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                return View(billModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult BillReview(int id, bool isReload = false)
        {
            try
            {
                var rule = new TraderInvoicesRules(dbContext);
                var billModel = rule.GetById(id);
                if (billModel == null)
                    return View("Error");
                var userSetting = CurrentUser();
                var currentDomainId = billModel?.Workgroup.Qbicle.Domain.Id ?? 0;
                ValidateCurrentDomain(billModel?.Workgroup?.Qbicle.Domain ?? CurrentDomain(), billModel.Workgroup?.Qbicle.Id ?? 0);
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

                if (billModel.Purchase == null)
                    return View("Error");

                var traderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(billModel.InvoiceApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, userSetting.Id);

                ViewBag.TraderApprovalRight = traderApprovalRight;

                SetCurrentApprovalIdCookies(billModel.InvoiceApprovalProcess?.Id ?? 0);

                decimal totalValue = 0;
                decimal totalTax = 0;
                foreach (var item in billModel.InvoiceItems)
                {
                    var taxRatePercent = item.TransactionItem.TraderItem.SumTaxRatesPercent(false);
                    var taxValue = (item.InvoiceValue * taxRatePercent) / 100;
                    totalTax += taxValue;
                    totalValue += item.InvoiceValue;
                }

                totalTax = billModel.InvoiceItems.Sum(q => q.InvoiceTaxValue) ?? 0;
                totalValue = billModel.InvoiceItems.Sum(q => q.InvoiceValue);

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoiceSaleTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                var billImage = "/Content/DesignStyle/img/icon_file_pdf.png";
                var billUpload = dbContext.StorageFiles.FirstOrDefault(e => e.Id == billModel.InvoicePDF);
                var fileType = Path.GetExtension(billUpload?.Name ?? "pdf").Replace(".", "");
                switch (fileType)
                {
                    case "ppt":
                        billImage = "/Content/DesignStyle/img/icon_file_ppt.png";
                        break;

                    case "pptx":
                        billImage = "/Content/DesignStyle/img/icon_file_pptx.png";
                        break;

                    case "doc":
                        billImage = "/Content/DesignStyle/img/icon_file_doc.png";
                        break;

                    case "docx":
                        billImage = "/Content/DesignStyle/img/icon_file_docx.png";
                        break;

                    case "xls":
                        billImage = "/Content/DesignStyle/img/icon_file_xls.png";
                        break;

                    case "xlsx":
                        billImage = "/Content/DesignStyle/img/icon_file_xlsx.png";
                        break;

                    case "jpg":
                        billImage = "/Content/DesignStyle/img/icon_file_jpg.png";
                        break;

                    case "jpeg":
                        billImage = "/Content/DesignStyle/img/icon_file_jpeg.png";
                        break;

                    case "png":
                        billImage = "/Content/DesignStyle/img/icon_file_png.png";
                        break;

                    case "zip":
                        billImage = "/Content/DesignStyle/img/icon_file_zip.png";
                        break;

                    case "pdf":
                        billImage = "/Content/DesignStyle/img/icon_file_pdf.png";
                        break;

                    case "gif":
                        billImage = "/Content/DesignStyle/img/icon_file_gif.png";
                        break;

                    case "mp4":
                        billImage = "/Content/DesignStyle/img/media-item-video.jpg";
                        break;

                    case "webm":
                        billImage = "/Content/DesignStyle/img/media-item-video.jpg";
                        break;

                    case "ogv":
                        billImage = "/Content/DesignStyle/img/media-item-video.jpg";
                        break;
                }
                ViewBag.BillImage = billImage;

                var timeline = rule.InvoiceApprovalStatusTimeline(billModel.Id, userSetting.Timezone);
                var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

                ViewBag.TimelineDate = timelineDate;
                ViewBag.Timeline = timeline;

                if (isReload)
                {
                    return PartialView("_BillReviewContent", billModel);
                }
                else
                    return View(billModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }
    }
}