using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules.Model;
using Qbicles.Models.Trader.Pricing;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models;
using System.IO;
using System.Collections;
using Qbicles.Models.Community;
using static log4net.Appender.RollingFileAppender;
using System.Threading.Tasks;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderSalesController : BaseController
    {
        // GET: TraderSales
        [HttpPost]
        public ActionResult SaveTraderSale(TraderSale traderSale, string countryName)
        {
            traderSale.Id = string.IsNullOrEmpty(traderSale.Key) ? 0 : int.Parse(traderSale.Key.Decrypt());
            var result = new TraderSaleRules(dbContext).SaveTraderSale(traderSale, countryName, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetContactById(int id)
        {
            var contact = new TraderSaleRules(dbContext).GetContactById(id);
            return Json(contact, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetPriceByLocationIdItemId(int locationId, int itemId)
        {
            var price = new TraderPriceRules(dbContext).GetPriceByLocationIdItemId(locationId, itemId, SalesChannelEnum.Trader);
            var priceResult = new Price()
            {
                Id = price.Id,
                NetPrice = price.NetPrice
            };
            return Json(priceResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TraderSaleRecordsTable(SaleFilterParameter saleFilter)
        {
            var domain = CurrentDomain();
            var traderSales = new TraderSaleRules(dbContext).GetByLocation(CurrentLocationManage(), domain.Id);

            ViewBag.WorkGroups = new TraderSaleRules(dbContext).GetWorkGroups(CurrentLocationManage());
            ViewBag.WorkGroupsOfMember = domain.Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage() && q.Processes.Any(p => p.Name == TraderProcessName.TraderSaleProcessName) && q.Members.Any(a => a.Id == CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.SaleFilter = saleFilter;
            return PartialView("_TraderSaleRecordsTablePartial", traderSales);
        }

        public ActionResult TraderSaleGetDataDashBoard(string keyword = "", int workGroupId = 0, string datetime = "", string channel = "")
        {
            var result = new TraderSaleRules(dbContext).GetDataDashBoard(CurrentDomainId(), CurrentLocationManage(), CurrentUser(), keyword, workGroupId, datetime, channel);
            return PartialView("_SaleDashBoard", result);
        }

        public ActionResult SaleReportGetDataDashBoard(string keyword = "", int workGroupId = 0, string datetime = "", string channel = "", int locationId = 0, int contactId = 0)
        {
            var userSetting = CurrentUser();
            var dateTimeFormat = new UserSetting
            {
                DateFormat = userSetting.DateFormat,
                TimeFormat = userSetting.TimeFormat,
                Timezone = userSetting.Timezone
            };
            var result = new TraderSaleRules(dbContext).GetDataDashBoard(CurrentDomainId(), locationId, dateTimeFormat, keyword, workGroupId, datetime, channel, contactId);
            return PartialView("_SaleDashBoard", result);
        }

        public ActionResult TraderSaleGetDetailTraderItems(string ids = "", string datetime = "")
        {
            try
            {
                List<int> lstId = new List<int>();
                if (!string.IsNullOrEmpty(ids))
                {
                    lstId = ids.Split('-').ToList().Select(s => Int32.Parse(s)).Distinct().ToList();
                }

                var result = new TraderSaleRules(dbContext).GetItemsByGroupId(lstId, CurrentDomainId(), CurrentLocationManage(), CurrentUser(), datetime, CurrentUser().Timezone, isApproved: true);
                return PartialView("_TraderItemDetail", result);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return PartialView("_TraderItemDetail", new List<TraerItemByGroup>());
            }
        }

        public ActionResult GetDataTableSales([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keyword, int workGroupId, string channel, string datetime, bool isApproved)
        {
            var result = new TraderSaleRules(dbContext).TraderSaleSearch(requestModel, CurrentUser().Id, CurrentLocationManage(), CurrentDomainId(), channel, keyword, workGroupId, datetime, CurrentUser().Timezone, isApproved, CurrentUser());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<TraderSaleCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaleReview(string key, bool content = false)
        {
            var rule = new TraderSaleRules(dbContext);
            var saleModel = rule.GetById(key.Decrypt2Int());
            if (saleModel == null)
                return View("Error");

            var userSetting = CurrentUser();
            var currentDomainId = saleModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            ValidateCurrentDomain(saleModel?.Workgroup?.Qbicle.Domain ?? CurrentDomain(), saleModel.Workgroup?.Qbicle.Id ?? 0);

            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");

            if (!content)
            {
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var approvalId = saleModel?.SaleApprovalProcess.Id ?? 0;
                SetCurrentApprovalIdCookies(approvalId);

                return View(saleModel ?? new TraderSale());
            }

            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(saleModel?.SaleApprovalProcess?.ApprovalRequestDefinition.Id ?? 0, userSetting.Id);

            var timeline = rule.SaleApprovalStatusTimeline(saleModel?.Id ?? 0, userSetting.Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return PartialView("_SaleReviewContent", saleModel ?? new TraderSale());
        }

        public ActionResult TraderSaleReviewItems(int id)
        {
            var result = new ReturnJsonModel();
            result.result = true;
            var saleModel = new TraderSaleRules(dbContext).GetById(id);
            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            ViewBag.locationId = saleModel.Location.Id;
            return PartialView("_TraderSaleReviewItems", saleModel);
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TraderSaleReviewContact(int id)
        {
            try
            {
                var deliveries = Enum.GetValues(typeof(DeliveryMethodEnum)).Cast<DeliveryMethodEnum>()
                    .Select(q => q.ToString()).ToList();
                ViewBag.LstEnum = deliveries;
                ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(CurrentDomainId());
                ViewBag.Contries = new CountriesRules().GetAllCountries();
                var saleModel = new TraderSaleRules(dbContext).GetById(id);
                return PartialView("_TraderSaleReviewContact", saleModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult TraderSaleReviewItemsPreview(int id)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(id);
            return PartialView("_TraderSaleReviewItemsPreview", saleModel);
        }

        public ActionResult TraderSaleReviewContactPreview(int id)
        {
            var saleModel = new TraderSaleRules(dbContext).GetById(id);
            return PartialView("_TraderSaleReviewContactPreview", saleModel);
        }

        [HttpPost]
        public ActionResult UpdateTraderSaleContact(TraderSale traderSale, string countryName)
        {
            var result = new TraderSaleRules(dbContext).UpdateTraderSaleContact(traderSale, countryName);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTraderSaleItems(TraderSale traderSale)
        {
            try
            {
                if (traderSale.Location == null)
                {
                    traderSale.Location = new TraderLocation() { Id = CurrentLocationManage() };
                }
                var result = new TraderSaleRules(dbContext).UpdateTraderSaleItems(traderSale, CurrentUser().Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public async Task<ActionResult> SaleMaster(string key)
        {
            ViewBag.GoBackPage = CurrentGoBackPage();
            this.SetCookieGoBackPage();
            var rule = new TraderSaleRules(dbContext);
            var saleModel = rule.GetById(key.Decrypt2Int());
            if (saleModel == null)
                return View("Error");
            var userSetting = CurrentUser();
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");
            var domain = saleModel.Workgroup?.Qbicle.Domain ?? CurrentDomain();
            var currentUserId = userSetting.Id;
            var allowAccessPage = domain.Administrators.Any(s => s.Id == currentUserId)
            || (saleModel.Workgroup != null
            && saleModel.Workgroup.Members.Any(q => q.Id == currentUserId)
            && saleModel.Workgroup.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
            );
            if (!allowAccessPage)
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var saleOrder = new TraderSalesOrder();
            if (saleModel.SalesOrders != null && saleModel.SalesOrders.Count > 0)
            {
                saleOrder = saleModel.SalesOrders[0];
            }
            if (saleOrder.Reference == null)
                saleOrder.Reference = new TraderReferenceRules(dbContext).GetNewReference(domain.Id, TraderReferenceType.SalesOrder);
            ViewBag.SaleOrder = saleOrder;
            var locationId = saleModel.Location.Id;
            var workgroupTransfer = domain.Workgroups.Where(q =>
                q.Location.Id == locationId &&
                q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderTransferProcessName))
                && q.Members.Select(u => u.Id).Contains(userSetting.Id)).ToList();

            ViewBag.IsMemberTransferWorkgroup =
                workgroupTransfer.Any(m => m.Members.Select(u => u.Id).Contains(currentUserId));

            decimal totalValue = 0;
            decimal totalTax = 0;

            ViewBag.imgTop = await GetMediaFileBase64Async(saleModel.Workgroup.Qbicle.Domain.LogoUri);
            ViewBag.imgBottom = await GetMediaFileBase64Async(saleModel.Workgroup.Qbicle.LogoUri);

            foreach (var item in saleModel.SaleItems)
            {
                var taxRate = item.TraderItem.SumTaxRatesPercent(true);
                var discount = item.Quantity * item.SalePricePerUnit * (item.Discount / 100);

                var taxValue = item.Quantity * item.SalePricePerUnit * taxRate / 100;
                totalTax += taxValue;
                totalValue += taxValue - discount + item.Quantity * item.SalePricePerUnit;
            }

            ViewBag.InvoiceTotal = totalValue;
            ViewBag.InvoiceSaleTax = totalTax;
            ViewBag.SubTotal = (totalValue - totalTax);

            var timeline = rule.SaleApprovalStatusTimeline(saleModel.Id, userSetting.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View(saleModel);
        }

        [HttpPost]
        public ActionResult SaveSaleInvoice(string saleKey, Invoice invoice)
        {
            var saleId = string.IsNullOrEmpty(saleKey) ? 0 : int.Parse(saleKey.Decrypt());
            invoice.Id = string.IsNullOrEmpty(invoice.Key) ? 0 : int.Parse(invoice.Key.Decrypt());
            invoice.Sale = new TraderSale { Id = saleId };
            var result = new TraderInvoicesRules(dbContext).SaveSaleInvoice(invoice, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InitSaleInvoice(int id, string saleKey)
        {
            var saleId = string.IsNullOrEmpty(saleKey) ? 0 : int.Parse(saleKey.Decrypt());
            var invoice = new Invoice();
            var saleModel = new TraderSaleRules(dbContext).GetById(saleId);
            invoice.Sale = saleModel;
            invoice.DueDate = DateTime.UtcNow;
            if (id > 0)
                invoice = new TraderInvoicesRules(dbContext).GetById(id);
            else
                invoice.Reference = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Invoice);

            ViewBag.Sale = saleModel;
            var locationId = saleModel.Location.Id;
            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderInvoiceProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.WorkgroupTransfer = workgroupTransfer;
            return PartialView("_TraderInvoiceAddPartial", invoice);
        }

        public ActionResult GetTableInvoice(string key, bool callBack = false)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            ViewBag.CallBack = callBack;
            var saleModel = new TraderSaleRules(dbContext).GetById(id);
            return PartialView("_TraderInvoiceTablePartial", saleModel);
        }

        //Sale Order ----------------------
        public ActionResult SaleOrder(int id)
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var saleModel = new TraderSaleOrderRules(dbContext).GetById(id);
                if (saleModel == null)
                    return View("Error");
                decimal totalValue = 0;
                decimal totalTax = 0;

                ViewBag.imgTop = saleModel.Sale.Workgroup.Qbicle.Domain.LogoUri;
                ViewBag.imgBottom = saleModel.Sale.Workgroup.Qbicle.LogoUri;

                foreach (var item in saleModel.Sale.SaleItems)
                {
                    var taxRate = item.TraderItem.SumTaxRates(true);
                    var discount = item.Quantity * item.SalePricePerUnit * (item.Discount / 100);

                    var taxValue = (item.Quantity * item.SalePricePerUnit - discount) * taxRate;
                    totalTax += taxValue;
                    totalValue += taxValue - discount + item.Quantity * item.SalePricePerUnit;
                }

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoiceSaleTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                return View(saleModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult SaveSaleOrder(TraderSalesOrder salesOrder)
        {
            try
            {
                var rule = new TraderSaleOrderRules(dbContext);
                var refModel = rule.SaveTraderSaleOrder(salesOrder, CurrentUser().Id);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> IssueSaleOrder(int id, string emails)
        {
            var qbicleUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port) + "Account/CreateAccount";
            var result = new ReturnJsonModel();
            var rule = new TraderSaleOrderRules(dbContext);
            var iv = rule.GetById(id);
            if (!string.IsNullOrEmpty(iv.SalesOrderPDF))
            {
                new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, await GetMediaFileBaseStreamAsync(iv.SalesOrderPDF), qbicleUri, IssueType.SaleOrder, emails);
                result.result = true;
                result.msg = iv.SalesOrderPDF;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            string imageTop = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.Domain?.LogoUri);
            string imageBottom = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.LogoUri);
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            var filePath = Server.MapPath($"~/App_Data/sale-order-{id}.pdf");
            var fileStreams = rule.ReportSaleOrder(iv, imageTop, imageBottom, CurrentUser().Timezone, currencySettings);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                fs.Write(fileStreams, 0, fileStreams.Length);
            }

            var uri = await UploadMediaFromPath($"sale-order-{id}.pdf", filePath);
            new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, new MemoryStream(fileStreams), qbicleUri, IssueType.SaleOrder, emails);
            result = rule.IssueSalesOrder(id, uri);
            result.msg = uri;

            System.IO.File.Delete(filePath);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DownloadFile(int saleOrderId)
        {
            try
            {
                var rule = new TraderSaleOrderRules(dbContext);
                var iv = rule.GetById(saleOrderId);
                string uri;
                if (!string.IsNullOrEmpty(iv.SalesOrderPDF))
                {
                    uri = iv.SalesOrderPDF;
                }
                else
                {
                    string imageTop = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.Domain?.LogoUri);
                    string imageBottom = await GetMediaFileBase64Async(iv.Sale.Workgroup?.Qbicle?.LogoUri);
                    var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
                    var filePath = Server.MapPath($"~/App_Data/sale-order-{saleOrderId}.pdf");
                    var fileStreams = rule.ReportSaleOrder(iv, imageTop, imageBottom, CurrentUser().Timezone, currencySettings);
                    //System.IO.File.WriteAllBytes(filePath, fileStreams);
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        fs.Write(fileStreams, 0, fileStreams.Length);
                    }

                    var uriKey = await UploadMediaFromPath($"sale-order-{saleOrderId}.pdf", filePath);
                    rule.IssueSalesOrder(saleOrderId, uriKey);
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

        public ActionResult TraderSaleAdd(int locationId, string tradersaleKey = "")
        {
            var tradersaleId = string.IsNullOrEmpty(tradersaleKey) ? 0 : int.Parse(tradersaleKey.Decrypt());
            var domain = CurrentDomain();
            var deliveries = Enum.GetValues(typeof(DeliveryMethodEnum)).Cast<DeliveryMethodEnum>()
                .Select(q => q.ToString()).ToList();
            ViewBag.LstEnum = deliveries;

            var traderSale = new TraderSale();
            var traderReferenceForSale = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Sale);
            if (tradersaleId > 0)
            {
                traderSale = new TraderSaleRules(dbContext).GetById(tradersaleId);
                if (traderSale.Reference == null)
                {
                    traderSale.Reference = traderReferenceForSale;
                }
            }
            else
            {
                // traderReferenceForSale = new TraderReferenceRules(dbContext).SaveReference(traderReferenceForSale);
                traderSale.Reference = traderReferenceForSale;
            }
            ViewBag.LocationId = locationId;
            ViewBag.Contries = new CountriesRules().GetAllCountries();
            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(CurrentDomainId()).Take(10).ToList();
            var currentLocationId = CurrentLocationManage();
            ViewBag.WorkGroups = domain.Workgroups.Where(q =>
                q.Location.Id == currentLocationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderSaleProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            return PartialView("_TraderSaleAddPartial", traderSale);
        }

        public ActionResult GetInvoiceItemsFromSale(string saleKey, int invoiceId = 0)
        {
            var saleId = string.IsNullOrEmpty(saleKey) ? 0 : int.Parse(saleKey.Decrypt());
            var rule = new TraderInvoicesRules(dbContext);
            var invoiceItems = rule.GetInvoiceItemsFromSale(saleId, CurrentUser(), invoiceId);

            return Json(new { data = invoiceItems }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowSaleDashboardReportDetail(string modalId = "", string keyword = "", int workGroupId = 0, string datetime = "", string channel = "", int locationId = 0, int contactId = 0)
        {
            var userSetting = CurrentUser();
            var dateTimeFormat = new UserSetting
            {
                DateFormat = userSetting.DateFormat,
                TimeFormat = userSetting.TimeFormat,
                Timezone = userSetting.Timezone
            };
            var result = new TraderSaleRules(dbContext).GetDataDashBoard(CurrentDomainId(), locationId, dateTimeFormat, keyword, workGroupId, datetime, channel, contactId);
            ViewBag.ModalId = modalId;
            return PartialView("_ShowSaleDashboardDetail", result);
        }

        public ActionResult ShowSaleDashboardDetail(string modalId = "", string keyword = "", int workGroupId = 0, string datetime = "", string channel = "")
        {
            var result = new TraderSaleRules(dbContext).GetDataDashBoard(CurrentDomainId(), CurrentLocationManage(), CurrentUser(), keyword, workGroupId, datetime, channel);
            ViewBag.ModalId = modalId;
            return PartialView("_ShowSaleDashboardDetail", result);
        }
    }
}