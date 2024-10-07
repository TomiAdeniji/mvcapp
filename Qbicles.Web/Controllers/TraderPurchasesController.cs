using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System.IO;
using Qbicles.Models.B2C_C2C;
using System.Web.UI.WebControls;
using System.Threading.Tasks;

namespace Qbicles.Web.Controllers
{
    [Authorize]
    public class TraderPurchasesController : BaseController
    {
        public ActionResult TraderPurchaseAdd(int locationId, int purchaseId = 0)
        {
            var domain = CurrentDomain();
            var deliveries = Enum.GetValues(typeof(DeliveryMethodEnum)).Cast<DeliveryMethodEnum>()
                .Select(q => q.ToString()).ToList();
            ViewBag.LstEnum = deliveries;

            var traderPurchase = new TraderPurchase();
            if (purchaseId > 0)
            {
                traderPurchase = new TraderPurchaseRules(dbContext).GetById(purchaseId);
            }
            else
            {
                traderPurchase.Reference = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Purchase);
            }

            ViewBag.LocationId = locationId;

            var workgroups = domain.Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPurchaseProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            ViewBag.WorkGroups = workgroups;
            var productGroupIds = workgroups.SelectMany(q => q.ItemCategories.Select(i => i.Id)).Distinct().ToList();

            ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
            //ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(CurrentDomainId());
            return PartialView("_TraderPurchasesAddPartial", traderPurchase);
        }

        /// <summary>
        /// Get buy item I buy (IsBought = true)
        /// </summary>
        /// <param name="workGroupId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        public ActionResult GetItemProductByWorkgroupIsBought(int workGroupId, int locationId)
        {
            var result = new ReturnJsonModel();
            result.result = true;
            ViewBag.locationId = locationId;

            var workgroups = CurrentDomain().Workgroups.Where(q =>
                q.Id == workGroupId
                && q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPurchaseProcessName))
                && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            var productGroupIds = workgroups.SelectMany(q => q.ItemCategories.Select(i => i.Id)).Distinct().ToList();
            var traderItems = CurrentDomain().TraderItems.Where(q => q.IsBought && q.Locations.Any(z => z.Id == locationId) && productGroupIds.Contains(q.Group.Id)
            ).Select(i => new TraderItemModel
            {
                Id = i.Id,
                Name = i.Name,
                ImageUri = i.ImageUri,
                CostUnit = 0,
                TaxRateName = i.StringItemTaxRates(false),
                TaxRateValue = i.SumTaxRatesPercent(false),
                WgIds = i.Group.WorkGroupCategories.Select(g => g.Id).ToList()
            }).OrderBy(n => n.Name).ToList();
            result.Object = traderItems;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // GET: TraderSales
        [HttpPost]
        public ActionResult SaveTraderPurchase(TraderPurchase traderPurchase)
        {
            traderPurchase.Id = string.IsNullOrEmpty(traderPurchase.Key) ? 0 : int.Parse(traderPurchase.Key.Decrypt());
            var result = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            result = new TraderPurchaseRules(dbContext).SaveTraderPurchase(traderPurchase, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetContactById(int id)
        {
            var contact = new TraderContactRules(dbContext).GetById(id);
            return Json(new TraderContact() { Id = contact.Id, Name = contact.Name, Address = contact.Address }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TraderPurchaseTable()
        {
            ViewBag.WorkGroups = new TraderPurchaseRules(dbContext).GetWorkGroupsByLocation(CurrentLocationManage());
            ViewBag.WorkGroupsOfMember = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == CurrentLocationManage() && q.Processes.Any(p => p.Name == TraderProcessName.TraderPurchaseProcessName) && q.Members.Any(a => a.Id == CurrentUser().Id)).OrderBy(n => n.Name).ToList();
            return PartialView("_TraderPurchaseTablePartial");
        }

        public ActionResult TraderPurchaseDataTable([ModelBinder(typeof(DataTablesBinder))] IDataTablesRequest requestModel, string keysearch = "", string groupid = "0")
        {
            var result = new TraderPurchaseRules(dbContext).GetTraderPurchaseDataTable(requestModel, CurrentUser(), CurrentLocationManage(), keysearch, int.Parse(groupid), CurrentDomainId());

            if (result != null)
                return Json(result, JsonRequestBehavior.AllowGet);
            else
                return Json(new DataTablesResponse(requestModel.Draw, new List<InventoryBatchCustom>(), 0, 0), JsonRequestBehavior.AllowGet);
        }

        public ActionResult PurchaseReview(int id)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            if (purchaseModel == null)
                return View("ErrorAccessPage");

            var userSetting = CurrentUser();

            var currentDomainId = purchaseModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            ValidateCurrentDomain(purchaseModel?.Workgroup?.Qbicle.Domain ?? CurrentDomain(), purchaseModel.Workgroup?.Qbicle.Id ?? 0);
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(purchaseModel.PurchaseApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, userSetting.Id);

            SetCurrentApprovalIdCookies(purchaseModel.PurchaseApprovalProcess?.Id ?? 0);

            var timeline = new TraderPurchaseRules(dbContext).PurchaseApprovalStatusTimeline(purchaseModel.Id, userSetting.Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View(purchaseModel);
        }

        public ActionResult PurchaseReviewContent(int id)
        {
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            if (purchaseModel == null)
                return View("Error");

            var userSetting = CurrentUser();

            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");
            ValidateCurrentDomain(purchaseModel.Workgroup?.Qbicle.Domain ?? CurrentDomain(), purchaseModel.Workgroup?.Qbicle.Id ?? 0);
            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(purchaseModel.PurchaseApprovalProcess.ApprovalRequestDefinition.Id, userSetting.Id);

            var timeline = new TraderPurchaseRules(dbContext).PurchaseApprovalStatusTimeline(purchaseModel.Id, userSetting.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return PartialView("_PurchaseReviewContent", purchaseModel);
        }

        public ActionResult TraderPurchaseReviewItems(int id)
        {
            try
            {
                var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
                ViewBag.Dimensions = new TransactionDimensionRules(dbContext).GetByDomainId(CurrentDomainId());
                return PartialView("_TraderPurchaseReviewItems", purchaseModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult TraderPurchaseReviewContact(int id)
        {
            try
            {
                var deliveries = Enum.GetValues(typeof(DeliveryMethodEnum)).Cast<DeliveryMethodEnum>()
                    .Select(q => q.ToString()).ToList();
                ViewBag.LstEnum = deliveries;
                ViewBag.Contries = new CountriesRules().GetAllCountries();
                var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);

                ViewBag.Contacts = new TraderContactRules(dbContext).GetByDomainId(CurrentDomainId());

                return PartialView("_TraderPurchaseReviewContact", purchaseModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult TraderPurchaseReviewItemsPreview(int id)
        {
            try
            {
                var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
                return PartialView("_TraderPurchaseReviewItemsPreview", purchaseModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                return View("Error");
            }
        }

        public ActionResult TraderPurchaseReviewContactPreview(int id, string display = "")
        {
            ViewBag.Display = display;
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            return PartialView("_TraderPurchaseReviewContactPreview", purchaseModel);
        }

        [HttpPost]
        public ActionResult UpdateTraderPurchaseContact(TraderPurchase traderPurchase, string countryName)
        {
            var result = new TraderPurchaseRules(dbContext).UpdateTraderPurchaseContact(traderPurchase, countryName);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateTraderPurchaseItems(TraderPurchase traderPurchase)
        {
            var result = new TraderPurchaseRules(dbContext).UpdateTraderPurchaseItems(traderPurchase, CurrentUser().Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> PurchaseMaster(int id)
        {
            ViewBag.GoBackPage = CurrentGoBackPage();
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            if (purchaseModel == null)
                return View("Error");

            var userSetting = CurrentUser();

            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, CurrentDomainId());
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
            var purchaseOrder = new TraderPurchaseOrder();
            var traderReferenceForSale = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.PurchaseOrder);
            // traderReferenceForSale = new TraderReferenceRules(dbContext).SaveReference(traderReferenceForSale);
            purchaseOrder.Reference = traderReferenceForSale;
            if (purchaseModel.PurchaseOrder != null && purchaseModel.PurchaseOrder.Count > 0)
            {
                purchaseOrder = purchaseModel.PurchaseOrder[0];
            }
            ViewBag.PurchaseOrder = purchaseOrder;

            var locationId = purchaseModel.Location.Id;

            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                q.Location.Id == locationId
                && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderPurchaseProcessName))
                && q.Members.Select(u => u.Id).Contains(userSetting.Id)).OrderBy(n => n.Name).ToList();

            ViewBag.WorkgroupTransfer = workgroupTransfer;

            ViewBag.IsMemberTransferWorkgroup =
                workgroupTransfer.Any(m => m.Members.Select(u => u.Id).Contains(userSetting.Id));

            ViewBag.imgTop = await GetMediaFileBase64Async(purchaseModel.Workgroup.Qbicle.Domain.LogoUri);
            ViewBag.imgBottom = await GetMediaFileBase64Async(purchaseModel.Workgroup.Qbicle.LogoUri);

            decimal totalValue = 0;
            decimal totalTax = 0;
            foreach (var item in purchaseModel.PurchaseItems)
            {
                var taxRate = item.TraderItem.SumTaxRatesPercent(false);
                var discount = item.Quantity * item.CostPerUnit * (item.Discount / 100);

                var taxValue = item.Quantity * item.CostPerUnit * taxRate / 100;
                totalTax += taxValue;
                totalValue += taxValue - discount + item.Quantity * item.CostPerUnit;
            }

            ViewBag.InvoiceTotal = totalValue;
            ViewBag.InvoiceSaleTax = totalTax;
            ViewBag.SubTotal = (totalValue - totalTax);

            var timeline = new TraderPurchaseRules(dbContext).PurchaseApprovalStatusTimeline(purchaseModel.Id, userSetting.Timezone);
            var timelineDate = timeline.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View(purchaseModel);
        }

        public ActionResult PurchaseReviewForMovenmentTrend(string key)
        {
            var id = string.IsNullOrEmpty(key) ? 0 : int.Parse(key.Decrypt());
            var purchaseModel = new TraderPurchaseRules(dbContext).GetById(id);
            if (purchaseModel == null)
                return View("ErrorAccessPage");

            var userSetting = CurrentUser();

            var currentDomainId = purchaseModel?.Workgroup.Qbicle.Domain.Id ?? 0;
            ValidateCurrentDomain(purchaseModel?.Workgroup?.Qbicle.Domain ?? CurrentDomain(), purchaseModel.Workgroup?.Qbicle.Id ?? 0);
            var userRoleRights = new AppRightRules(dbContext).UserRoleRights(userSetting.Id, currentDomainId);
            if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                return View("ErrorAccessPage");
            ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");

            ViewBag.TraderApprovalRight = new ApprovalAppsRules(dbContext).GetTraderApprovalRight(purchaseModel.PurchaseApprovalProcess?.ApprovalRequestDefinition?.Id ?? 0, userSetting.Id);

            SetCurrentApprovalIdCookies(purchaseModel.PurchaseApprovalProcess?.Id ?? 0);

            var timeline = new TraderPurchaseRules(dbContext).PurchaseApprovalStatusTimeline(purchaseModel.Id, userSetting.Timezone);
            var timelineDate = timeline?.Select(d => d.LogDate.Date).Distinct().ToList();

            ViewBag.TimelineDate = timelineDate;
            ViewBag.Timeline = timeline;

            return View("~/Views/TraderPurchases/PurchaseReview.cshtml", purchaseModel);
        }

        //Purchase Order ----------------------
        public ActionResult PurchaseOrder(int id)
        {
            try
            {
                var userRoleRights = new AppRightRules(dbContext).UserRoleRights(CurrentUser().Id, CurrentDomainId());
                if (userRoleRights.All(r => r != RightPermissions.TraderAccess && r != RightPermissions.QbiclesBusinessAccess))
                    return View("ErrorAccessPage");
                ViewBag.CurrentPage = "trader"; SetCurrentPage("trader");
                var purchaseModel = new TraderPurchaseOrderRules(dbContext).GetById(id);

                decimal totalValue = 0;
                decimal totalTax = 0;

                ViewBag.imgTop = purchaseModel.Purchase.Workgroup.Qbicle.Domain.LogoUri;
                ViewBag.imgBottom = purchaseModel.Purchase.Workgroup.Qbicle.LogoUri;

                foreach (var item in purchaseModel.Purchase.PurchaseItems)
                {
                    var taxRate = item.TraderItem.SumTaxRates(false);
                    var discount = item.Quantity * item.CostPerUnit * (item.Discount / 100);

                    var taxValue = (item.Quantity * item.CostPerUnit - discount) * taxRate;
                    totalTax += taxValue;
                    totalValue += taxValue - discount + item.Quantity * item.CostPerUnit;
                }

                ViewBag.InvoiceTotal = totalValue;
                ViewBag.InvoicePurchaseTax = totalTax;
                ViewBag.SubTotal = (totalValue - totalTax);

                SetCurrentApprovalIdCookies(purchaseModel.Purchase.PurchaseApprovalProcess?.Id ?? 0);

                return View(purchaseModel);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return View("Error");
            }
        }

        public ActionResult SavePurchaseOrder(TraderPurchaseOrder purchaseOrder)
        {
            var rule = new TraderPurchaseOrderRules(dbContext);
            var refModel = rule.SaveTraderPurchaseOrder(purchaseOrder, CurrentUser().Id);

            return Json(refModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> DownloadFile(int saleOrderId)
        {
            try
            {
                var rule = new TraderPurchaseOrderRules(dbContext);
                var iv = rule.GetById(saleOrderId);
                string uri;
                if (!string.IsNullOrEmpty(iv.PurchaseOrderPDF))
                {
                    uri = iv.PurchaseOrderPDF;
                }
                else
                {
                    string imageTop = await GetMediaFileBase64Async(iv.Purchase.Workgroup?.Qbicle?.Domain?.LogoUri);
                    string imageBottom = await GetMediaFileBase64Async(iv.Purchase.Workgroup?.Qbicle?.LogoUri);
                    var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
                    var filePath = Server.MapPath($"~/App_Data/purchase-order-{saleOrderId}.pdf");
                    var fileStreams = rule.ReportSalePurchase(iv, imageTop, imageBottom, CurrentUser().Timezone, currencySettings);

                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        fs.Write(fileStreams, 0, fileStreams.Length);
                    }

                    var uriKey = await UploadMediaFromPath($"purchase-order-{saleOrderId}.pdf", filePath);

                    rule.IssuepurchaseOrder(saleOrderId, uriKey);
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
        public async Task<ActionResult> IssuePurchaseOrder(int id)
        {
            try
            {
                var qbicleUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port) + "Account/CreateAccount";
                var result = new ReturnJsonModel();
                var rule = new TraderPurchaseOrderRules(dbContext);
                var iv = rule.GetById(id);
                if (!string.IsNullOrEmpty(iv.PurchaseOrderPDF))
                {
                    new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, await GetMediaFileBaseStreamAsync(iv.PurchaseOrderPDF), qbicleUri, IssueType.PurchaseOrder);
                    result.result = true;
                    result.msg = iv.PurchaseOrderPDF;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                string imageTop = await GetMediaFileBase64Async(iv.Purchase.Workgroup?.Qbicle?.Domain?.LogoUri);
                string imageBottom = await GetMediaFileBase64Async(iv.Purchase.Workgroup?.Qbicle?.LogoUri);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
                var filePath = Server.MapPath($"~/App_Data/purchase-order-{id}.pdf");
                var fileStreams = rule.ReportSalePurchase(iv, imageTop, imageBottom, CurrentUser().Timezone, currencySettings);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    fs.Write(fileStreams, 0, fileStreams.Length);
                }

                var uriKey = await UploadMediaFromPath($"purchase-order-{id}.pdf", filePath);

                result = rule.IssuepurchaseOrder(id, uriKey);
                result.msg = uriKey;

                new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, new MemoryStream(fileStreams), qbicleUri, IssueType.PurchaseOrder);
                System.IO.File.Delete(filePath);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
                return Json(new ReturnJsonModel { result = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // Purchase Bill
        public ActionResult ShowTableBillByPurchase(int id = 0)
        {
            var purchase = new TraderPurchase();
            if (id > 0)
            {
                purchase = new TraderPurchaseRules(dbContext).GetById(id);
            }
            return PartialView("_TraderPurchasesTableBillPartial", purchase);
        }

        public ActionResult AddEditTraderPurchaseBill(int id = 0, int purchaseId = 0)
        {
            Invoice invoice = new Invoice();
            if (id > 0)
            {
                invoice = new TraderInvoicesRules(dbContext).GetById(id);
            }
            else
            {
                if (purchaseId > 0)
                {
                    invoice.Purchase = new TraderPurchaseRules(dbContext).GetById(purchaseId);
                }
                var traderReferenceForSale = new TraderReferenceRules(dbContext).GetNewReference(CurrentDomainId(), TraderReferenceType.Bill);
                // traderReferenceForSale = new TraderReferenceRules(dbContext).SaveReference(traderReferenceForSale);
                invoice.Reference = traderReferenceForSale;
            }

            var workgroupTransfer = CurrentDomain().Workgroups.Where(q =>
                    q.Location.Id == CurrentLocationManage()
                    && q.Processes.Any(p => p.Name.Equals(TraderProcessName.TraderInvoiceProcessName))
                    && q.Members.Select(u => u.Id).Contains(CurrentUser().Id)).OrderBy(n => n.Name).ToList();

            ViewBag.WorkgroupTransfer = workgroupTransfer;
            return PartialView("_TraderPurchasesAddBillPartial", invoice);
        }

        [HttpPost]
        public ActionResult SaveBillInvoice(Invoice invoice, Invoice traderBillAssociatedFiles, List<MediaModel> traderBillAttachments)
        {
            var refModel = new ReturnJsonModel
            {
                result = true,
                actionVal = 1
            };
            if (invoice.Id > 0) refModel.actionVal = 2;
            try
            {
                var currentuser = CurrentUser();
                refModel = new TraderInvoicesRules(dbContext).SaveBillInvoice(invoice, currentuser.Id);

                var mediaRules = new MediasRules(dbContext);

                if (traderBillAssociatedFiles?.AssociatedFiles?.Count > 0)
                    mediaRules.UpdateAttachmentsBill(traderBillAssociatedFiles, int.Parse(refModel.msgId));

                if (traderBillAttachments?.Count > 0)
                    mediaRules.SaveNewAttachmentsBill(int.Parse(refModel.msgId), traderBillAttachments, currentuser.Id);

                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);
                refModel.result = false;
                refModel.actionVal = 3;
                refModel.msg = ex.Message;
                return Json(refModel, JsonRequestBehavior.AllowGet);
            }
        }

        // End Purchase Bill
        public ActionResult GetInvoiceItemsFromPurchase(int purchaseId)
        {
            var rule = new TraderInvoicesRules(dbContext);
            var invoiceItems = rule.GetInvoiceItemsFromPurchase(purchaseId, CurrentUser());

            return Json(new { data = invoiceItems }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult IssuePurchaseOrder(int id)
        //{
        //    try
        //    {
        //        var qbicleUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port) + "Account/CreateAccount";
        //        var result = new ReturnJsonModel();
        //        var rule = new TraderPurchaseOrderRules(dbContext);
        //        var iv = rule.GetById(id);
        //        if (!string.IsNullOrEmpty(iv.PurchaseOrderPDF))
        //        {
        //            new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, GetMediaFileBaseStream(iv.PurchaseOrderPDF), qbicleUri, IssueType.PurchaseOrder);
        //            result.result = true;
        //            result.msg = iv.PurchaseOrderPDF;
        //            return Json(result, JsonRequestBehavior.AllowGet);
        //        }

        //        string imageTop = GetMediaFileBase64(iv.Purchase.Workgroup?.Qbicle?.Domain?.LogoUri);
        //        string imageBottom = GetMediaFileBase64(iv.Purchase.Workgroup?.Qbicle?.LogoUri);
        //        var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
        //        var filePath = Server.MapPath($"~/App_Data/purchase-order-{id}.pdf");
        //        var fileStreams = rule.ReportSalePurchase(iv, imageTop, imageBottom, CurrentUser().Timezone, currencySettings);

        //        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        //        {
        //            fs.Write(fileStreams, 0, fileStreams.Length);
        //        }

        //        var uriKey = UploadMediaFromPath($"purchase-order-{id}.pdf", filePath);

        //        result = rule.IssuepurchaseOrder(id, uriKey);
        //        result.msg = uriKey;

        //        new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, new MemoryStream(fileStreams), qbicleUri, IssueType.PurchaseOrder);
        //        System.IO.File.Delete(filePath);
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        LogManager.Error(System.Reflection.MethodBase.GetCurrentMethod(), ex, CurrentUser().Id);
        //        return Json(new ReturnJsonModel { result = false }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public async Task<ActionResult> IssuePurchaseOrder(int id, string emails)
        {
            var qbicleUri = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port) + "Account/CreateAccount";
            var result = new ReturnJsonModel();
            var rule = new TraderPurchaseOrderRules(dbContext);
            var iv = rule.GetById(id);
            if (!string.IsNullOrEmpty(iv.PurchaseOrderPDF))
            {
                new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, await GetMediaFileBaseStreamAsync(iv.PurchaseOrderPDF), qbicleUri, IssueType.PurchaseOrder, emails);
                result.result = true;
                result.msg = iv.PurchaseOrderPDF;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            string imageTop = await GetMediaFileBase64Async(iv.Purchase.Workgroup?.Qbicle?.Domain?.LogoUri);
            string imageBottom = await GetMediaFileBase64Async(iv.Purchase.Workgroup?.Qbicle?.LogoUri);
            var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(CurrentDomainId());
            var filePath = Server.MapPath($"~/App_Data/sale-order-{id}.pdf");
            var fileStreams = rule.ReportSalePurchase(iv, imageTop, imageBottom, CurrentUser().Timezone, currencySettings);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                fs.Write(fileStreams, 0, fileStreams.Length);
            }

            var uri = await UploadMediaFromPath($"sale-order-{id}.pdf", filePath);
            new EmailRules(dbContext).SendEmailQbicleIssue(iv, null, new MemoryStream(fileStreams), qbicleUri, IssueType.PurchaseOrder, emails);
            result = rule.IssuepurchaseOrder(id, uri);
            result.msg = uri;

            System.IO.File.Delete(filePath);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}