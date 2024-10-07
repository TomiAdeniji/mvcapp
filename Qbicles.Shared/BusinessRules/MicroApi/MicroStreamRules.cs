using Qbicles.BusinessRules.B2C;
using Qbicles.BusinessRules.BusinessRules.Social;
using Qbicles.BusinessRules.C2C;
using Qbicles.BusinessRules.Commerce;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Extensions;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Highlight;
using Qbicles.Models.Loyalty;
using Qbicles.Models.Manufacturing;
using Qbicles.Models.MicroQbicleStream;
using Qbicles.Models.SalesMkt;
using Qbicles.Models.Trader.Budgets;
using Qbicles.Models.Trader.Inventory;
using Qbicles.Models.Trader.Movement;
using Qbicles.Models.Trader.Payments;
using Qbicles.Models.Trader.Returns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Qbicles.BusinessRules.Enums;
using static Qbicles.Models.Notification;
using static Qbicles.Models.QbicleActivity;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroStreamRules : MicroRulesBase
    {
        public MicroStreamRules(MicroContext microContext) : base(microContext)
        {
        }

        public MicroQbicleStream QbiclesStream(QbicleFillterModel filter)
        {
            filter.Size *= HelperClass.activitiesPageSize;
            var model = new QbicleRules(dbContext).GetQbicleStreams(filter, CurrentUser.Timezone, CurrentUser.DateFormat);
            model.TotalCount /= HelperClass.activitiesPageSize;
            return model.ToMicro(CurrentUser.Timezone, CurrentUser.DateFormat, CurrentUser.Id);
        }

        public MicroDatesQbicleStream CalendarStream(CalendarFilterModel filter)
        {

            var totalRecords = 0;
            filter.PageSize = HelperClass.myDeskPageSize;
            var activities = new QbicleRules(dbContext).ActivitiesForCalendar(CurrentUser.Timezone,
                filter.QbicleId, filter.Type, filter.Day, filter.Keyword, filter.Orderby, filter.Types, filter.Topics, filter.Peoples, filter.Apps, filter.PageSize, filter.PageIndex,
                ref totalRecords, CurrentUser.DateFormat);

            var pinneds = (from pin in dbContext.MyPins
                           join desk in dbContext.MyDesks on pin.Desk.Id equals desk.Id
                           where desk.Owner.Id == CurrentUser.Id && pin.PinnedActivity != null
                           select pin.PinnedActivity.Id).ToList();


            return activities.ToMicro(pinneds, CurrentUser.DateFormat);
        }

        public List<CalendarColor> CalendarDataControl(int qbicleId, int? year, int? month)
        {
            return new QbicleRules(dbContext).ActivitiesRecursExistListDate(qbicleId, CurrentUser.Timezone, year, month).ToMicro();
        }

        public static MicroActivitesStream GenerateActivity(dynamic objActivity, DateTime objDate, List<int> pinneds, string currentUserId,
            string dateFormat, string timezone, bool isFilterDiscussionOrder, NotificationEventEnum notifyEvent = NotificationEventEnum.DiscussionCreation, Notification notification = null)
        {
            var today = DateTime.UtcNow;
            var isApproval = true;
            var streamActivity = new MicroActivitesStream();

            if (objActivity is TradeOrder)
            {

            }

            if (objActivity is QbiclePost post)
            {
                post = post.BusinessMapping(timezone);
                streamActivity.Id = post.Id;
                streamActivity.Key = post.Key;
                streamActivity.UserAvatar = post.CreatedBy.ProfilePic.ToUri();
                streamActivity.CreatedBy = post.CreatedBy.GetFullName();
                streamActivity.TimelineDate = post.StartedDate.ToShortDateString();
                streamActivity.CreatedDate = post.StartedDate.Date == today.Date ? "Today, " + post.StartedDate.ToString("hh:mmtt") : post.StartedDate.ToString("dd MMM yyyy, hh:mmtt");
                streamActivity.IsOwned = post.CreatedBy.Id == currentUserId;
                streamActivity.TopicId = post.Topic?.Id.ToString();
                streamActivity.TopicName = post.Topic?.Name;
                streamActivity.PostMessage = post.Message;
                streamActivity.StreamId = StreamType.Post.GetId();
                streamActivity.StreamName = StreamType.Post.GetDescription();
                streamActivity.Name = "Post";
            }

            if (objActivity is Qbicle qbicle)
            {
                #region Qbicle

                qbicle = qbicle.BusinessMapping(timezone);
                streamActivity.Id = qbicle.Id;
                streamActivity.Key = qbicle.Key;
                streamActivity.StreamId = StreamType.Qbicles.GetId();
                streamActivity.StreamName = StreamType.Qbicles.GetDescription();
                streamActivity.TimelineDate = qbicle.LastUpdated.ToShortDateString();
                streamActivity.IsOwned = qbicle.StartedBy.Id == currentUserId;
                streamActivity.CreatedDate = qbicle.StartedDate.Date == today.Date ? "Today, " + qbicle.StartedDate.ToString("hh:mm tt") : qbicle.StartedDate.ToString("dd MMM yyyy, hh:mm tt");
                streamActivity.Name = qbicle.Name;
                streamActivity.CreatedBy = qbicle.StartedBy.GetFullName();
                streamActivity.UserAvatar = qbicle.StartedBy.ProfilePic.ToUri();
                streamActivity.AppTitle = qbicle.Name;
                streamActivity.AppTitleMsg = qbicle.Description;
                streamActivity.MediaUri = qbicle.LogoUri.ToUri();
                #endregion

            }

            if (objActivity is QbicleActivity activity)
            {
                
                switch (activity.ActivityType)
                {
                    case ActivityTypeEnum.ApprovalRequest:
                        var appTrader = ((ApprovalReq)activity).BusinessMapping(timezone);
                        #region Trader Approval
                        streamActivity.Key = appTrader.Key;
                        streamActivity.TimelineDate = appTrader.TimeLineDate.ToShortDateString();
                        streamActivity.CreatedDate = appTrader.TimeLineDate.Date == today.Date ? "Today, " + appTrader.TimeLineDate.ToString("hh:mm tt") : appTrader.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = appTrader.Topic?.Id.ToString();
                        streamActivity.TopicName = appTrader.Topic?.Name;
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == appTrader.Id);

                        switch (appTrader.RequestStatus)
                        {
                            case ApprovalReq.RequestStatusEnum.Pending:
                                if (appTrader.ReviewedBy.Count == 0)
                                    if (appTrader.Transfer.Count > 0)
                                    {
                                        streamActivity.StatusName = TransferStatus.PendingPickup.GetDescription();
                                        streamActivity.StatusId = TransferStatus.PendingPickup.GetId();
                                    }
                                break;
                            case ApprovalReq.RequestStatusEnum.Reviewed:
                                if (appTrader.Transfer.Count > 0)
                                {
                                    streamActivity.StatusName = TransferStatus.PickedUp.GetDescription();
                                    streamActivity.StatusId = TransferStatus.PickedUp.GetId();
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Approved:
                                if (appTrader.Transfer.Count > 0)
                                {
                                    streamActivity.StatusName = TransferStatus.Delivered.GetDescription();
                                    streamActivity.StatusId = TransferStatus.Delivered.GetId();
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Denied:
                                streamActivity.StatusName = TransferStatus.Denied.GetDescription();
                                streamActivity.StatusId = TransferStatus.Denied.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Discarded:
                                streamActivity.StatusName = TransferStatus.Discarded.GetDescription();
                                streamActivity.StatusId = TransferStatus.Discarded.GetId();
                                break;
                        }

                        streamActivity.AppTitle = "Approval Request";
                        streamActivity.AppTitleMsg = appTrader.Name;
                        streamActivity.AppIcon = "";
                        streamActivity.TraderUri = "";
                        streamActivity.StreamName = "Trader";
                        streamActivity.CreatedBy = appTrader.StartedBy.GetFullName();
                        streamActivity.ConsumeCountTask = "";
                        if (appTrader.Transfer != null && appTrader.Transfer.Count > 0)
                        {
                            var transfer = appTrader.Transfer.FirstOrDefault();
                            streamActivity.Id = appTrader.Id;
                            streamActivity.Key = transfer.Key;
                            streamActivity.IsOwned = transfer.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.Transfer.GetId();
                            streamActivity.StreamName = StreamType.Transfer.GetDescription();
                            streamActivity.CreatedBy = transfer.CreatedBy.GetFullName();
                            streamActivity.AppIcon = "icon_delivery.png".ToUriString();
                            streamActivity.TraderUri = "/TraderTransfers/TransferReview?key=" + transfer.Key;

                            var saleTransfer = transfer.Sale;
                            var purchaseTransfer = transfer.Purchase;

                            if (saleTransfer == null && purchaseTransfer == null)
                                streamActivity.AppTitle = $"{transfer.OriginatingLocation?.Name} to {transfer.DestinationLocation?.Name}";
                            else if (saleTransfer != null)
                                streamActivity.AppTitle = $"{transfer.OriginatingLocation?.Name} to {saleTransfer.Purchaser.Name}";
                            else if (purchaseTransfer != null)
                                streamActivity.AppTitle = $"{purchaseTransfer.Vendor.Name} to {transfer.DestinationLocation?.Name}";
                        }
                        else if (appTrader.StockAudits != null && appTrader.StockAudits.Count > 0)//Audit or Purchase approval only
                        {
                            streamActivity.AppIcon = "icon_audit.png".ToUriString();
                            var stockAudit = appTrader.StockAudits.FirstOrDefault();
                            streamActivity.Id = stockAudit.Id;
                            streamActivity.Key = stockAudit.Key;
                            streamActivity.IsOwned = stockAudit.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.StockAudits.GetId();
                            streamActivity.StreamName = StreamType.StockAudits.GetDescription();
                            streamActivity.AppTitle = "Approval Request";
                            streamActivity.AppTitleMsg = stockAudit.Name;
                            streamActivity.TraderUri = "/TraderStockAudits/ShiftAuditReview?id=" + stockAudit.Id;
                        }
                        else if (appTrader.Sale != null && appTrader.Sale.Count > 0) //Sale or Purchase approval only
                        {
                            streamActivity.AppIcon = "icon_bookkeeping.png".ToUriString();

                            var sale = appTrader.Sale.FirstOrDefault();
                            streamActivity.IsOwned = sale.CreatedBy.Id == currentUserId;
                            streamActivity.Id = sale.Id;
                            streamActivity.Key = sale.Key;
                            streamActivity.StreamId = StreamType.Sale.GetId();
                            streamActivity.StreamName = StreamType.Sale.GetDescription();
                            streamActivity.CreatedBy = sale.CreatedBy.GetFullName();
                            streamActivity.TraderUri = "/TraderSales/SaleReview?key=" + sale.Key;
                        }
                        else if (appTrader.Purchase != null && appTrader.Purchase.Count > 0)
                        {
                            streamActivity.AppIcon = "icon_bookkeeping.png".ToUriString();
                            var purchase = appTrader.Purchase.FirstOrDefault();
                            streamActivity.IsOwned = purchase.CreatedBy.Id == currentUserId;
                            streamActivity.Id = purchase.Id;
                            streamActivity.Key = purchase.Key;
                            streamActivity.StreamId = StreamType.Purchase.GetId();
                            streamActivity.StreamName = StreamType.Purchase.GetDescription();
                            streamActivity.CreatedBy = purchase.CreatedBy.GetFullName();
                            streamActivity.TraderUri = "/TraderPurchases/PurchaseReview?id=" + purchase.Id;
                        }
                        else if (appTrader.TraderContact != null && appTrader.TraderContact.Count > 0)//approval contact
                        {
                            var contact = appTrader.TraderContact.FirstOrDefault();
                            streamActivity.IsOwned = contact.CreatedBy.Id == currentUserId;
                            streamActivity.Id = contact.Id;
                            streamActivity.Key = contact.Key;
                            streamActivity.StreamId = StreamType.TraderContact.GetId();
                            streamActivity.StreamName = StreamType.TraderContact.GetDescription();
                            streamActivity.AppTitle = contact.Name;
                            streamActivity.AppTitleMsg = contact.ContactGroup.Name + " Group";
                            streamActivity.AppIcon = "icon_contact.png".ToUriString();
                            streamActivity.TraderUri = "/TraderContact/ContactReview?id=" + contact.Id;
                        }
                        else if (appTrader.Invoice != null && appTrader.Invoice.Count > 0) // approval invoice
                        {
                            var invoice = appTrader.Invoice.FirstOrDefault();
                            streamActivity.CreatedBy = invoice.CreatedBy.GetFullName();
                            streamActivity.AppIcon = "icon_invoice.png".ToUriString();

                            streamActivity.IsOwned = invoice.CreatedBy.Id == currentUserId;
                            streamActivity.Id = invoice.Id;
                            streamActivity.Key = invoice.Key;
                            streamActivity.StreamId = StreamType.Invoice.GetId();
                            streamActivity.StreamName = StreamType.Invoice.GetDescription();
                            if (invoice.Purchase != null)
                            {
                                streamActivity.StreamId = StreamType.InvoicePurchase.GetId();
                                streamActivity.StreamName = StreamType.InvoicePurchase.GetDescription();
                                streamActivity.TraderUri = "/TraderBill/BillReview?id=" + invoice.Id;
                                streamActivity.AppTitle = $"Bill #{invoice.Reference?.FullRef ?? invoice.Id.ToString()}";
                                streamActivity.AppTitleMsg = $"For Purchase #{invoice.Purchase.Reference?.FullRef ?? ""}";
                            }

                            if (invoice.Sale != null)
                            {
                                streamActivity.TraderUri = "/TraderInvoices/InvoiceReview?key=" + invoice.Key;

                                streamActivity.StreamId = StreamType.InvoiceSale.GetId();
                                streamActivity.StreamName = StreamType.InvoiceSale.GetDescription();
                                streamActivity.AppTitle = $"Invoice #{invoice.Reference?.FullRef ?? invoice.Id.ToString()}";
                                streamActivity.AppTitleMsg = $"For Sale #{invoice.Sale.Reference?.FullRef ?? ""}";
                            }
                        }
                        else if (appTrader.Payments != null && appTrader.Payments.Count > 0) // approval payment
                        {
                            var payment = appTrader.Payments.FirstOrDefault();
                            streamActivity.Id = payment.Id;
                            streamActivity.Key = payment.Key;
                            streamActivity.IsOwned = payment.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.Payments.GetId();
                            streamActivity.StreamName = StreamType.Payments.GetDescription();
                            streamActivity.CreatedBy = payment.CreatedBy.GetFullName();
                            streamActivity.AppTitle = "Payment #" + (payment?.Reference ?? "");
                            streamActivity.AppIcon = "icon_payments.png".ToUriString();
                            streamActivity.TraderUri = "/TraderPayments/PaymentReview?id=" + payment.Id;

                            if (payment?.AssociatedInvoice != null && payment?.AssociatedInvoice?.Id != null && payment?.AssociatedInvoice?.Id > 0)
                            {
                                streamActivity.AppTitleMsg = $"For Invoice #{payment?.AssociatedInvoice?.Reference?.FullRef ?? ""}";
                            }
                            else
                            {
                                var fromStr = "";
                                var toStr = "";
                                if (payment?.OriginatingAccount?.Name != null)
                                    fromStr = $"From: {payment.OriginatingAccount.Name}";
                                if (payment?.DestinationAccount?.Name != null)
                                    toStr = $"To: {payment.DestinationAccount.Name}";

                                streamActivity.AppTitleMsg = $"{fromStr} {toStr}";

                                if (fromStr == "" && toStr == "")
                                    streamActivity.AppTitleMsg = "No account details available";
                            }

                        }
                        else if (appTrader.SpotCounts != null && appTrader.SpotCounts.Count > 0) // approval SpotCounts
                        {
                            var spotCount = appTrader.SpotCounts.FirstOrDefault();
                            streamActivity.Id = spotCount.Id;
                            streamActivity.Key = spotCount.Key;
                            streamActivity.IsOwned = spotCount.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.SpotCounts.GetId();
                            streamActivity.StreamName = StreamType.SpotCounts.GetDescription();
                            streamActivity.AppTitle = "Approval Request";
                            streamActivity.AppIcon = "icon_spotcount.png".ToUriString();
                            streamActivity.AppTitleMsg = streamActivity.AppTitleMsg.Replace("Spot Count:", "");
                            streamActivity.TraderUri = "/TraderSpotCount/SpotCountReview?id=" + spotCount.Id;
                            switch (spotCount.Status)
                            {
                                case SpotCountStatus.CountStarted:
                                    streamActivity.StatusName = SpotCountStatus.CountStarted.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.CountStarted.GetId();
                                    break;
                                case SpotCountStatus.CountCompleted:
                                    streamActivity.StatusName = SpotCountStatus.CountCompleted.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.CountCompleted.GetId();
                                    break;
                                case SpotCountStatus.StockAdjusted:
                                    streamActivity.StatusName = SpotCountStatus.StockAdjusted.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.StockAdjusted.GetId();
                                    break;
                                case SpotCountStatus.Denied:
                                    streamActivity.StatusName = SpotCountStatus.Denied.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.Denied.GetId();
                                    break;
                                case SpotCountStatus.Discarded:
                                    streamActivity.StatusName = SpotCountStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.WasteReports != null && appTrader.WasteReports.Count > 0) // approval SpotCounts
                        {
                            var wasteReport = appTrader.WasteReports.FirstOrDefault();

                            streamActivity.Id = wasteReport.Id;
                            streamActivity.Key = wasteReport.Key;
                            streamActivity.IsOwned = wasteReport.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.WasteReports.GetId();
                            streamActivity.StreamName = StreamType.WasteReports.GetDescription();
                            streamActivity.AppTitle = "Approval Request";
                            streamActivity.AppIcon = "icon_waste.png".ToUriString();
                            streamActivity.AppTitleMsg = wasteReport.Name;
                            streamActivity.TraderUri = "/TraderWasteReport/WasteReportReview?id=" + wasteReport.Id;
                            switch (wasteReport.Status)
                            {
                                case WasteReportStatus.Started:
                                    streamActivity.StatusName = WasteReportStatus.Started.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.Started.GetId();
                                    break;
                                case WasteReportStatus.Completed:
                                    streamActivity.StatusName = WasteReportStatus.Completed.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.Completed.GetId();
                                    break;
                                case WasteReportStatus.StockAdjusted:
                                    streamActivity.StatusName = WasteReportStatus.StockAdjusted.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.StockAdjusted.GetId();
                                    break;
                                case WasteReportStatus.Discarded:
                                    streamActivity.StatusName = WasteReportStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.Manufacturingjobs != null && appTrader.Manufacturingjobs.Count > 0) // approval Manufacturingjobs
                        {
                            var manufacturingJob = appTrader.Manufacturingjobs.FirstOrDefault();
                            streamActivity.Id = manufacturingJob.Id;
                            streamActivity.Key = manufacturingJob?.Key;
                            streamActivity.IsOwned = manufacturingJob.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.Manufacturingjobs.GetId();
                            streamActivity.StreamName = StreamType.Manufacturingjobs.GetDescription();
                            streamActivity.CreatedBy = manufacturingJob.CreatedBy.GetFullName();
                            streamActivity.AppTitle = "Compound Item Assembly";
                            streamActivity.AppIcon = "icon_manufacturing.png".ToUriString();
                            streamActivity.AppTitleMsg = (manufacturingJob.Reference != null ? manufacturingJob.Reference.FullRef : "") + " " + (manufacturingJob.Product.Name); //$"Items {appTrader.Manufacturingjobs.FirstOrDefault().SelectedRecipe.Ingredients.Count} manufacturing";
                            streamActivity.TraderUri = "/Manufacturing/ManuJobReview?id=" + manufacturingJob.Id;
                            switch (manufacturingJob.Status)
                            {
                                case ManuJobStatus.Pending:
                                    streamActivity.StatusName = ManuJobStatus.Pending.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Pending.GetId();
                                    break;
                                case ManuJobStatus.Reviewed:
                                    streamActivity.StatusName = ManuJobStatus.Reviewed.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Reviewed.GetId();
                                    break;
                                case ManuJobStatus.Approved:
                                    streamActivity.StatusName = ManuJobStatus.Approved.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Approved.GetId();
                                    break;
                                case ManuJobStatus.Denied:
                                    streamActivity.StatusName = ManuJobStatus.Denied.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Denied.GetId();
                                    break;
                                case ManuJobStatus.Discarded:
                                    streamActivity.StatusName = ManuJobStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.CreditNotes != null && appTrader.CreditNotes.Count > 0)// approval Credit
                        {

                            var creditNote = appTrader.CreditNotes.FirstOrDefault();
                            streamActivity.Id = creditNote.Id;
                            streamActivity.Key = creditNote.Key;
                            streamActivity.IsOwned = creditNote.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.CreditNotes.GetId();
                            streamActivity.StreamName = StreamType.CreditNotes.GetDescription();
                            streamActivity.CreatedBy = creditNote.CreatedBy.GetFullName();
                            streamActivity.AppTitle = $"{HelperClass.GetFullNameOfUser(creditNote.CreatedBy)}";
                            streamActivity.AppIcon = "icon_manufacturing.png".ToUriString();
                            if (appTrader.CreditNotes[0].Reason == CreditNoteReason.DebitNote
                               || appTrader.CreditNotes[0].Reason == CreditNoteReason.PriceIncrease)
                                streamActivity.AppTitleMsg = $"Debit note #" + appTrader.CreditNotes[0].Reference?.FullRef;
                            else
                                streamActivity.AppTitleMsg = $"Credit note #" + appTrader.CreditNotes[0].Reference?.FullRef;

                            streamActivity.TraderUri = "/TraderContact/CreditNoteReview?id=" + creditNote.Id;

                            switch (creditNote.Status)
                            {
                                case CreditNoteStatus.PendingReview:
                                    streamActivity.StatusName = CreditNoteStatus.PendingReview.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.PendingReview.GetId();
                                    break;
                                case CreditNoteStatus.Reviewed:
                                    streamActivity.StatusName = CreditNoteStatus.Reviewed.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Reviewed.GetId();
                                    break;
                                case CreditNoteStatus.Approved:
                                    streamActivity.StatusName = CreditNoteStatus.Approved.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Approved.GetId();
                                    break;
                                case CreditNoteStatus.Denied:
                                    streamActivity.StatusName = CreditNoteStatus.Denied.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Denied.GetId();
                                    break;
                                case CreditNoteStatus.Discarded:
                                    streamActivity.StatusName = CreditNoteStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.BudgetScenarioItemGroups != null && appTrader.BudgetScenarioItemGroups.Count > 0) // approval Budget group item
                        {
                            var budgetScenarioItemGroups = appTrader.BudgetScenarioItemGroups.FirstOrDefault();
                            streamActivity.Id = budgetScenarioItemGroups.Id;
                            streamActivity.Key = budgetScenarioItemGroups.Key;
                            streamActivity.StreamId = StreamType.BudgetScenarioItemGroups.GetId();
                            streamActivity.StreamName = StreamType.BudgetScenarioItemGroups.GetDescription();
                            streamActivity.CreatedBy = budgetScenarioItemGroups.CreatedBy.GetFullName();
                            streamActivity.IsOwned = budgetScenarioItemGroups.CreatedBy.Id == currentUserId;
                            streamActivity.AppTitle = $"{HelperClass.GetFullNameOfUser(budgetScenarioItemGroups.CreatedBy)}";
                            streamActivity.AppIcon = "icon_bookkeeping.png".ToUriString();
                            streamActivity.AppTitleMsg = $"Budget Scenario Items Group: {budgetScenarioItemGroups.BudgetScenario.Title}";
                            streamActivity.TraderUri = "/TraderBudget/ProcessApproval?id=" + budgetScenarioItemGroups.Id + "&oView=A";
                            switch (budgetScenarioItemGroups.Status)
                            {
                                case BudgetScenarioItemGroupStatus.Pending:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Pending.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Pending.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Reviewed:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Reviewed.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Reviewed.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Approved:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Approved.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Approved.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Draft:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Draft.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Draft.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Denied:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Denied.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Denied.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Discarded:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.TraderReturns != null && appTrader.TraderReturns.Count > 0) // approval Sale return
                        {

                            var traderReturn = appTrader.TraderReturns.FirstOrDefault();
                            streamActivity.Id = traderReturn.Id;
                            streamActivity.Key = traderReturn.Key;
                            streamActivity.StreamId = StreamType.TraderReturns.GetId();
                            streamActivity.StreamName = StreamType.TraderReturns.GetDescription();
                            streamActivity.IsOwned = traderReturn.CreatedBy.Id == currentUserId;
                            streamActivity.CreatedBy = traderReturn.CreatedBy.GetFullName();
                            streamActivity.AppTitle = $"Approval Request";
                            streamActivity.AppIcon = "icon_return.png".ToUriString();
                            streamActivity.AppTitleMsg = $"Reference #{traderReturn.Reference?.FullRef}";
                            streamActivity.TraderUri = "/TraderSalesReturn/SaleReturnReview?id=" + traderReturn.Id;
                            switch (traderReturn.Status)
                            {
                                case TraderReturnStatusEnum.PendingReview:
                                    streamActivity.StatusName = TraderReturnStatusEnum.PendingReview.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.PendingReview.GetId();
                                    break;
                                case TraderReturnStatusEnum.Reviewed:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Reviewed.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Reviewed.GetId();
                                    break;
                                case TraderReturnStatusEnum.Approved:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Approved.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Approved.GetId();
                                    break;
                                case TraderReturnStatusEnum.Draft:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Draft.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Draft.GetId();
                                    break;
                                case TraderReturnStatusEnum.Denied:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Denied.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Denied.GetId();
                                    break;
                                case TraderReturnStatusEnum.Discarded:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Discarded.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.ConsumptionReports != null && appTrader.ConsumptionReports.Count > 0) // Consume report
                        {
                            var consume = appTrader.ConsumptionReports.FirstOrDefault();
                            streamActivity.Id = consume.Id;
                            streamActivity.Key = consume.Key;
                            streamActivity.StreamId = StreamType.ConsumptionReports.GetId();
                            streamActivity.StreamName = StreamType.ConsumptionReports.GetDescription();
                            streamActivity.IsOwned = consume.CreatedBy.Id == currentUserId;
                            streamActivity.AppTitle = "Consumption Report";
                            streamActivity.AppIcon = "icon_spannered.png".ToUriString();
                            streamActivity.AppTitleMsg = consume?.Name;
                            streamActivity.TraderUri = "/Spanneredfree/ConsumeReportReview?id=" + consume?.Id;
                            streamActivity.ConsumeCountTask = consume.AssociatedTask != null ? "<li><i class=\"fa fa-link\"></i> 1 Asset Tasks</li>" : "";
                        }
                        else if (appTrader.TillPayment != null && appTrader.TillPayment.Count > 0) // Till payment
                        {
                            var tillPayment = appTrader.TillPayment.FirstOrDefault();
                            streamActivity.Id = tillPayment.Id;
                            streamActivity.Key = tillPayment.Key;
                            streamActivity.StreamId = StreamType.TillPayment.GetId();
                            streamActivity.StreamName = StreamType.TillPayment.GetDescription();
                            streamActivity.IsOwned = tillPayment.CreatedBy.Id == currentUserId;
                            var directionName = tillPayment.Direction == Models.Trader.CashMgt.TillPayment.TillPaymentDirection.InToTill ? "Pay In" : "Pay Out";
                            streamActivity.DiscussionDetail = $"Till {directionName} Approval Request";
                            streamActivity.AppTitle = $"Approval Request";
                            streamActivity.AppIcon = "icon_cash.png".ToUriString();
                            if (tillPayment.Direction == Models.Trader.CashMgt.TillPayment.TillPaymentDirection.InToTill)
                                streamActivity.AppTitleMsg = $"Till Payment from the Safe \"{tillPayment.AssociatedSafe.Name}\" to the Till \"{tillPayment.AssociatedTill.Name}\"";
                            else
                                streamActivity.AppTitleMsg = $"Till Payment from the Till \"{tillPayment.AssociatedTill.Name}\" to the Safe \"{tillPayment.AssociatedSafe.Name}\"";

                            streamActivity.TraderUri = "/CashManagement/TillPaymentReview?tillPaymentId=" + tillPayment.Id;
                        }
                        else
                            isApproval = false;
                        #endregion
                        break;

                    case ActivityTypeEnum.ApprovalRequestApp:
                        var app = ((ApprovalReq)activity).BusinessMapping(timezone);

                        streamActivity.Id = app.Id;
                        streamActivity.Key = app.Key;
                        streamActivity.IsOwned = app.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = app.TimeLineDate.ToShortDateString();
                        streamActivity.CreatedDate = app.TimeLineDate.Date == today.Date ? "Today, " + app.TimeLineDate.ToString("hh:mm tt") : app.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = app.Topic?.Id.ToString();
                        streamActivity.TopicName = app.Topic?.Name;
                        streamActivity.UserAvatar = app.StartedBy.ProfilePic.ToUri();
                        streamActivity.CreatedBy = app.StartedBy.GetFullName();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == app.Id);

                        var jounralApproval = app.JournalEntries.Count > 0;

                        if (app.ReviewedBy.Count > 0)
                        {
                            streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Reviewed.GetDescription();
                            streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                            if (jounralApproval)
                                streamActivity.StatusName = "Awaiting Approval";
                        }
                        switch (app.RequestStatus)
                        {
                            case ApprovalReq.RequestStatusEnum.Pending:
                                if (app.ReviewedBy.Count == 0)
                                {
                                    streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Pending.GetDescription();
                                    streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Pending.GetId();
                                    if (jounralApproval)
                                        streamActivity.StatusName = "Awaiting Review";
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Reviewed:

                                if (jounralApproval || app.CampaigPostApproval.Count > 0)
                                {
                                    streamActivity.StatusName = "Awaiting Approval";
                                    streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Approved:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Approved.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Denied:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Denied.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Denied.GetId();
                                break;
                        }
                        #region Journal Entry
                        if (jounralApproval)
                        {
                            var jounralEntry = (app.JournalEntries.FirstOrDefault()).BusinessMapping(timezone);

                            streamActivity.StreamId = StreamType.jounralEntries.GetId();
                            streamActivity.StreamName = StreamType.jounralEntries.GetDescription();
                            streamActivity.ActivityCreatedBy = jounralEntry.CreatedBy.GetFullName();
                            streamActivity.AppTitle = "Approval Request / Bookkeeping";
                            streamActivity.AppTitleMsg = $"Journal Entry #{jounralEntry.Number}";
                            streamActivity.Id = jounralEntry.Id;
                            streamActivity.Key = jounralEntry.Key;
                            streamActivity.IsOwned = jounralEntry.CreatedBy.Id == currentUserId;

                        }
                        #endregion
                        #region Campaign post
                        else if (app.CampaigPostApproval.Any())
                        {
                            var isManualCampaign = app.CampaigPostApproval.Any(c => c.CampaignPost.AssociatedCampaign.CampaignType == Qbicles.Models.SalesMkt.CampaignType.Manual);
                            if (!isManualCampaign)
                                streamActivity.AppIcon = "icon_socialpost.png".ToUriString();
                            else
                                streamActivity.AppIcon = "icon_socialpost_manual.png".ToUriString();

                            streamActivity.AppTitle = isManualCampaign ? "Manual Social Post Approval" : "Social Media Post Approval";
                            var campaignApproval = app.CampaigPostApproval.FirstOrDefault();
                            var campaignPost = campaignApproval.CampaignPost;
                            var media = campaignPost.ImageOrVideo;

                            if (media != null)
                            {
                                var mediaLastupdateS = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                                //streamActivity.ActivityHref = isManualCampaign ? "ManualSocialPostInApp" : "SocialPostInApp" + $"?id={campaignApproval.Id}";
                                if (media.FileType.Type.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                                    streamActivity.MediaUri = mediaLastupdateS.Uri.ToUri();
                                else if (media.FileType.Type.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                                    streamActivity.MediaUri = mediaLastupdateS.Uri.ToUri(FileTypeEnum.Video);
                                else
                                    streamActivity.MediaUri = media.FileType.IconPath.ToUri(FileTypeEnum.Document);

                            }

                            streamActivity.Id = campaignPost.Id;
                            streamActivity.Key = campaignPost.Key;
                            streamActivity.IsOwned = campaignPost.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.CampaigPost.GetId();
                            streamActivity.StreamName = StreamType.CampaigPost.GetDescription();
                            streamActivity.CampaignName = campaignPost.AssociatedCampaign.Name;
                            streamActivity.CampaignTitle = campaignPost.Title;
                            streamActivity.CampaignContent = campaignPost.Content;
                            streamActivity.ActivityCreatedBy = campaignPost.CreatedBy.GetFullName();
                        }
                        #endregion
                        #region Email post
                        else if (app.EmailPostApproval.Any())
                        {
                            var emailPostApproval = app.EmailPostApproval.FirstOrDefault().BusinessMapping(timezone);
                            var campaignEmail = emailPostApproval.CampaignEmail;

                            streamActivity.AppTitle = "Sales & Marketing Email Approval";

                            switch (emailPostApproval.ApprovalStatus)
                            {
                                case SalesMktApprovalStatusEnum.InReview:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.InReview.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.InReview.GetId();
                                    break;
                                case SalesMktApprovalStatusEnum.Approved:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.Approved.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.Approved.GetId();
                                    break;
                                case SalesMktApprovalStatusEnum.Denied:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.Denied.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.Denied.GetId();
                                    break;
                                case SalesMktApprovalStatusEnum.Queued:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.Queued.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.Queued.GetId();
                                    break;
                            }

                            //streamActivity.ActivityHref = $"/SalesMarketing/EmailPostInApp?id={emailPostApproval.Id}";
                            streamActivity.MediaUri = campaignEmail.FeaturedImageUri.ToUri(FileTypeEnum.Document);

                            streamActivity.Id = campaignEmail.Id;
                            streamActivity.Key = campaignEmail.Key;
                            streamActivity.IsOwned = campaignEmail.CreatedBy.Id == currentUserId;

                            streamActivity.StreamId = StreamType.EmailPost.GetId();
                            streamActivity.StreamName = StreamType.EmailPost.GetDescription();
                            streamActivity.CampaignName = campaignEmail.Campaign.Name;
                            streamActivity.CampaignTitle = campaignEmail.Title;
                            streamActivity.ActivityCreatedBy = campaignEmail.CreatedBy.GetFullName();
                        }
                        #endregion
                        #region Operator clock IN
                        else if (app.OperatorClockIn.Any())
                        {
                            streamActivity.AppTitle = app.Name;
                            var clockedInApproval = app.OperatorClockIn.FirstOrDefault();
                            streamActivity.Id = clockedInApproval.Id;
                            streamActivity.Key = clockedInApproval.Key;
                            streamActivity.IsOwned = clockedInApproval.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.OperatorClockIn.GetId();
                            streamActivity.StreamName = StreamType.OperatorClockIn.GetDescription();
                            streamActivity.MediaUri = clockedInApproval.People.ProfilePic.ToUri();
                            //streamActivity.ActivityHref = "/Operator/Clocked?id=@clockedInApproval.Id&type=clockin";
                            streamActivity.CampaignName = "Clock in /Operator";
                            streamActivity.CampaignTitle = clockedInApproval.Notes;
                            streamActivity.Location = clockedInApproval.WorkGroup.Location.Name;
                            streamActivity.ActivityEventDate = clockedInApproval.Date.ConvertTimeFromUtc(timezone).ToString(dateFormat);
                            streamActivity.ActivityEventTime = clockedInApproval.TimeIn.ConvertTimeFromUtc(timezone).ToString("hh:mmtt").ToLower();
                        }
                        #endregion
                        #region Operator clock OuT
                        else if (app.OperatorClockOut.Any())
                        {

                            streamActivity.AppTitle = app.Name;
                            var clockedOutApproval = app.OperatorClockOut.FirstOrDefault();
                            streamActivity.Id = clockedOutApproval.Id;
                            streamActivity.Key = clockedOutApproval.Key;
                            streamActivity.IsOwned = clockedOutApproval.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.OperatorClockOut.GetId();
                            streamActivity.StreamName = StreamType.OperatorClockOut.GetDescription();
                            streamActivity.MediaUri = clockedOutApproval.People.ProfilePic.ToUri();
                            //streamActivity.ActivityHref = "/Operator/Clocked?id=@clockedInApproval.Id&type=clockin";
                            streamActivity.CampaignName = "Clock out /Operator";
                            streamActivity.CampaignTitle = clockedOutApproval.Notes;
                            streamActivity.Location = clockedOutApproval.WorkGroup.Location.Name;
                            streamActivity.ActivityEventDate = clockedOutApproval.Date.ConvertTimeFromUtc(timezone).ToString(dateFormat);
                            streamActivity.ActivityEventTime = clockedOutApproval.TimeIn.ConvertTimeFromUtc(timezone).ToString("hh:mmtt").ToLower();
                        }
                        #endregion
                        #region Other approval
                        else
                        {
                            streamActivity.Id = app.Id;
                            streamActivity.Key = app.Key;
                            streamActivity.IsOwned = app.StartedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.ApprovalRequestApp.GetId();
                            streamActivity.StreamName = StreamType.ApprovalRequestApp.GetDescription();
                            streamActivity.AppTitle = $"Approval Request / {app.Name}";
                            streamActivity.AppTitleMsg = $"Journal Entry #{app.Notes}";
                        }
                        #endregion
                        break;
                    case ActivityTypeEnum.TaskActivity:
                        #region Task
                        var tk = ((QbicleTask)activity).BusinessMapping(timezone);
                        streamActivity.Id = tk.Id;
                        streamActivity.Key = tk.Key;

                        if (notifyEvent == NotificationEventEnum.TaskNotificationPoints)
                        {

                        }
                        else
                        {
                            streamActivity.StatusName = tk.isComplete == true ? "Complete" : "In progress";
                            if (tk.ActualStart == null)
                            {
                                streamActivity.StatusName = "Not started";
                            }
                            switch (streamActivity.StatusName)
                            {
                                case "Complete":
                                    streamActivity.StatusColor = StatusLabelStyle.SuccessColor;
                                    break;

                                case "In progress":
                                    streamActivity.StatusColor = StatusLabelStyle.WarningColor;
                                    break;
                                case "Not started":
                                    streamActivity.StatusColor = StatusLabelStyle.PrimaryColor;
                                    break;
                            }
                            streamActivity.StreamId = StreamType.Task.GetId();
                            streamActivity.StreamName = StreamType.Task.GetDescription();
                            streamActivity.IsOwned = tk.StartedBy.Id == currentUserId;
                            streamActivity.TimelineDate = tk.TimeLineDate.ToShortDateString();
                            streamActivity.CreatedDate = tk.TimeLineDate.Date == today.Date ? "Today, " + tk.TimeLineDate.ToString("hh:mm tt") : tk.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                            streamActivity.TopicId = tk.Topic?.Id.ToString();
                            streamActivity.TopicName = tk.Topic?.Name;
                            streamActivity.CreatedBy = tk.StartedBy.GetFullName();
                            streamActivity.UserAvatar = tk.StartedBy.ProfilePic.ToUri();
                            streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == tk.Id);
                            if (tk.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                                streamActivity.UpdateReason = tk.UpdateReason.GetDescription();
                            streamActivity.AppTitle = $"Task / {tk.Name}";
                            streamActivity.AppTitleMsg = tk.Description;

                            var _cbTask = tk.task;
                            if (_cbTask != null)
                            {
                                //streamActivity.Key =_cbTask.Key
                                streamActivity.UserAvatar = (tk.Asset?.FeaturedImageUri ?? tk.StartedBy.ProfilePic).ToUri();
                                streamActivity.StreamId = StreamType.Cleanbook.GetId();
                                streamActivity.StreamName = StreamType.Cleanbook.GetDescription();
                                streamActivity.AppTitle = $"Cleanbooks Task / {tk.Name}";
                                streamActivity.AppTitleMsg = tk.Description;
                                //streamActivity.ActivityHref = "/Apps/Tasks";
                                streamActivity.TaskType = _cbTask.tasktype?.Name;
                                streamActivity.TaskPriority = tk.Priority.GetDescription();
                            }
                            else if (tk.ComplianceTask != null)
                            {
                                //streamActivity.Key = tk.ComplianceTask.Key;
                                //streamActivity.ActivityHref = $"/Operator/ComplianceTask?id={tk.ComplianceTask.Id}&{tk.Id}";
                                streamActivity.TaskCompliance = tk.ComplianceTask.Type == Models.Operator.Compliance.TaskType.Repeatable ? "Repeating " : "" + "Compliance Task / Operator";
                                streamActivity.TaskForms = tk.ComplianceTask.OrderedForms.Count() + " Forms";

                                streamActivity.StreamId = StreamType.ComplianceTask.GetId();
                                streamActivity.StreamName = StreamType.ComplianceTask.GetDescription();
                                var minutes = tk.ComplianceTask.OrderedForms.Sum(s => s.FormDefinition.EstimatedTime);
                                streamActivity.TaskComplianceTotal = minutes > 60 ? $"{minutes / 60}h" : $"{minutes}m" + " total";
                                streamActivity.TaskRecurring = tk.isRecurs ? "Recurring" : "";
                            }
                            else
                            {
                                streamActivity.Key = tk.Key;
                                streamActivity.TaskPriority = tk.Priority.GetDescription();
                                streamActivity.TaskRecurring = tk.isRecurs ? "Recurring" : "";
                                if (tk.ProgrammedStart.HasValue && tk.ProgrammedEnd.HasValue)
                                {
                                    if (tk.ProgrammedStart.Value.Date == tk.ProgrammedEnd.Value.Date)
                                    {
                                        streamActivity.ActivityEventDate = tk.ProgrammedStart.Value.ToString(dateFormat);
                                        streamActivity.ActivityEventTime = tk.ProgrammedStart.Value.ToString("hh:mmtt") + " - " + tk.ProgrammedEnd.Value.ToString("hh:mmtt");
                                    }
                                    else
                                        streamActivity.ActivityEventDate = tk.ProgrammedStart.Value.ToString(dateFormat + " hh:mmtt") + " - " + tk.ProgrammedEnd.Value.ToString(dateFormat + " hh:mmtt");
                                }
                            }
                        }




                        #endregion
                        break;
                    case ActivityTypeEnum.DiscussionActivity:
                    case ActivityTypeEnum.OrderCancellation:
                        #region Discussion
                        var ds = ((QbicleDiscussion)activity).BusinessMapping(timezone);
                        streamActivity.Id = ds.Id;
                        streamActivity.Key = ds.Key;
                        streamActivity.StreamId = StreamType.Discussion.GetId();
                        streamActivity.StreamName = StreamType.Discussion.GetDescription();
                        streamActivity.IsOwned = ds.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = ds.TimeLineDate.ToShortDateString();
                        streamActivity.CreatedDate = ds.TimeLineDate.Date == today.Date ? "Today, " + ds.TimeLineDate.ToString("hh:mm tt") : ds.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = ds.Topic?.Id.ToString();
                        streamActivity.TopicName = ds.Topic?.Name;
                        streamActivity.CreatedBy = ds.StartedBy.GetFullName();
                        streamActivity.UserAvatar = ds.StartedBy.ProfilePic.ToUri();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == ds.Id);
                        streamActivity.MediaUri = ds.FeaturedImageUri?.ToUri();
                        streamActivity.CoveringNote = ds.Summary;
                        if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2CProductMenu || ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder)
                        {
                            if (ds is B2CProductMenuDiscussion)
                            {
                                MapCatalogDiscussion(streamActivity, ds);
                            }

                            else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder)
                            {
                                streamActivity.MediaUri = ConfigManager.Communitybuysell.ToDocumentUri();
                                streamActivity.Name = "View order";
                                //streamActivity.ActivityHref = $"/B2C/DiscussionOrder?disKey={ds.Key}";
                                if (ds is B2COrderCreation)
                                {
                                    var b2COrderCreation = (B2COrderCreation)ds;
                                    streamActivity.TradeOrderId = b2COrderCreation.TradeOrder.Id;
                                    streamActivity.TradeOrderKey = b2COrderCreation.TradeOrder.Key;
                                    streamActivity.StatusId = b2COrderCreation.TradeOrder.OrderStatus.GetId();
                                    streamActivity.StatusName = b2COrderCreation.TradeOrder.OrderStatus.GetDescription();
                                    streamActivity.TraderId = ds.Id;
                                    streamActivity.TraderKey = ds.Key;
                                    streamActivity.SalesChannel = b2COrderCreation.TradeOrder.SalesChannel;
                                    streamActivity.CoveringNote = ds.Summary;
                                }
                            }
                            else
                            {
                                streamActivity.MediaUri = ConfigManager.CommunityShop.ToDocumentUri();
                                streamActivity.Name = "View & manage";
                                //streamActivity.ActivityHref = $"/B2C/DiscussionMenu?disKey={ds.Key}";
                            }

                            streamActivity.StreamId = StreamType.DiscussionOrder.GetId();
                            streamActivity.StreamName = StreamType.DiscussionOrder.GetDescription();

                            streamActivity.AppTitleMsg = ds.Summary;
                            streamActivity.AppTitle = ds.Name;

                        }
                        else if (ds is B2CProductMenuDiscussion)
                        {
                            MapCatalogDiscussion(streamActivity, ds);
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder)
                        {
                            if (ds is B2COrderCreation)
                            {
                                var b2COrderCreation1 = (B2COrderCreation)ds;

                                streamActivity.TradeOrderId = b2COrderCreation1.TradeOrder.Id;
                                streamActivity.TradeOrderKey = b2COrderCreation1.TradeOrder.Key;
                                streamActivity.StatusId = b2COrderCreation1.TradeOrder.OrderStatus.GetId();
                                streamActivity.StatusName = b2COrderCreation1.TradeOrder.OrderStatus.GetDescription();
                                streamActivity.TraderId = ds.Id;
                                streamActivity.TraderKey = ds.Key;
                                streamActivity.SalesChannel = b2COrderCreation1.TradeOrder.SalesChannel;
                                streamActivity.CoveringNote = ds.Summary;
                            }
                            streamActivity.StreamId = StreamType.DiscussionOrder.GetId();
                            streamActivity.StreamName = StreamType.DiscussionOrder.GetDescription();
                            streamActivity.MediaUri = ConfigManager.Communitybuysell.ToDocumentUri();
                            streamActivity.Name = "View order";
                            streamActivity.AppTitleMsg = ds.Summary;
                            streamActivity.AppTitle = ds.Name;
                            //streamActivity.ActivityHref = $"/B2C/DiscussionOrder?disKey={ds.Key}";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.IdeaDiscussion)
                        {
                            //streamActivity.ActivityHref = "/SalesMarketingIdea/DiscussionIdea?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Sales & Marketing > Ideas/Themes";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Sales & Marketing Theme Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.PlaceDiscussion)
                        {
                            //streamActivity.ActivityHref = "/SalesMarketingLocation/DiscussionPlace?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Sales & Marketing > Places";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Sales & Marketing Place Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion)
                        {
                            //streamActivity.ActivityHref = "/Operator/DiscussionGoal?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Operator > Goal";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Operator Goal Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion)
                        {
                            //streamActivity.ActivityHref = "/Operator/DiscussionComplianceTask?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Operator > Compliance Task";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Operator Compliance Task Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.CashManagement)
                        {
                            //var nameStringList = ds.Name.Split(' ');
                            //streamActivity.ActivityHref = "/CashManagement/DiscussionCashManagementShow?disKey=" + ds.Key;
                            streamActivity.DiscussionBreadcrumb = "via Cash Management";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Cash Management Discussion";
                        }
                        else
                        {
                            //streamActivity.ActivityHref = "/Qbicles/DiscussionQbicle?disKey=" + ds.Key;
                            streamActivity.DiscussionDetail = $"{ds.ActivityMembers.Count} people";
                            streamActivity.AppTitle = ds.Name;
                            streamActivity.DiscussionBreadcrumb = "";
                        }
                        streamActivity.AppTitleMsg = ds.Summary;
                        #endregion
                        break;
                    case ActivityTypeEnum.AlertActivity:
                        #region Alert
                        var al = ((QbicleAlert)activity).BusinessMapping(timezone);

                        streamActivity.Id = al.Id;
                        streamActivity.Key = al.Key;
                        streamActivity.StreamId = StreamType.Alert.GetId();
                        streamActivity.StreamName = StreamType.Alert.GetDescription();
                        streamActivity.IsOwned = al.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = al.TimeLineDate.ToShortDateString();
                        streamActivity.CreatedDate = al.TimeLineDate.Date == today.Date ? "Today, " + al.TimeLineDate.ToString("hh:mm tt") : al.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = al.Topic?.Id.ToString();
                        streamActivity.TopicName = al.Topic?.Name;
                        streamActivity.CreatedBy = al.StartedBy.GetFullName();
                        streamActivity.UserAvatar = al.StartedBy.ProfilePic.ToUri();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == al.Id);
                        if (al.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = al.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Alert / {al.Name}";
                        streamActivity.AppTitleMsg = al.Content;
                        #endregion
                        break;
                    case ActivityTypeEnum.EventActivity:
                        #region Event
                        var ev = ((QbicleEvent)activity).BusinessMapping(timezone);
                        if (notifyEvent == NotificationEventEnum.EventNotificationPoints)
                        {

                        }
                        else
                        {
                            streamActivity.StreamId = StreamType.Event.GetId();
                            streamActivity.StreamName = StreamType.Event.GetDescription();
                            streamActivity.Id = ev.Id;
                            streamActivity.Key = ev.Key;
                            streamActivity.IsOwned = ev.StartedBy.Id == currentUserId;
                            streamActivity.TimelineDate = ev.TimeLineDate.ToShortDateString();
                            streamActivity.CreatedDate = ev.TimeLineDate.Date == today.Date ? "Today, " + ev.TimeLineDate.ToString("hh:mm tt") : ev.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                            streamActivity.TopicId = ev.Topic?.Id.ToString();
                            streamActivity.TopicName = ev.Topic?.Name;
                            streamActivity.CreatedBy = ev.StartedBy.GetFullName();
                            streamActivity.UserAvatar = ev.StartedBy.ProfilePic.ToUri();
                            streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == ev.Id);
                            streamActivity.AppTitle = $"Event / {ev.Name}";
                            streamActivity.AppTitleMsg = ev.Description;
                            if (ev.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                                streamActivity.UpdateReason = ev.UpdateReason.GetDescription();
                            if (ev.Start.Date == ev.End.Date)
                            {
                                streamActivity.ActivityEventDate = ev.Start.FormatDateTimeByUser(dateFormat);
                                streamActivity.ActivityEventTime = ev.Start.ToString("hh:mmtt") + " - " + ev.End.ToString("hh:mmtt");
                            }
                            else
                                streamActivity.ActivityEventDate = ev.Start.ToString(dateFormat + " hh:mmtt") + " - " + ev.End.ToString(dateFormat + " hh:mmtt");
                            streamActivity.Location = ev.Location;
                        }

                        #endregion
                        break;
                    case ActivityTypeEnum.MediaActivity:
                        #region Media
                        var me = ((QbicleMedia)activity).BusinessMapping(timezone);
                        var mediaLastupdate = me.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                        var mediatype = me.FileType?.Type ?? "";
                        streamActivity.Id = me.Id;
                        streamActivity.Key = me.Key;
                        streamActivity.StreamId = StreamType.Medias.GetId();
                        streamActivity.StreamName = StreamType.Medias.GetDescription();
                        streamActivity.IsOwned = me.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = me.TimeLineDate.ToShortDateString();
                        streamActivity.CreatedDate = me.TimeLineDate.Date == today.Date ? "Today, " + me.TimeLineDate.ToString("hh:mm tt") : me.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = me.Topic?.Id.ToString();
                        streamActivity.TopicName = me.Topic?.Name;
                        streamActivity.CreatedBy = me.StartedBy.GetFullName();
                        streamActivity.UserAvatar = me.StartedBy.ProfilePic.ToUri();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == me.Id);
                        streamActivity.AppTitle = $"Media / {me.Name}";
                        streamActivity.AppTitleMsg = me.Description;
                        var infoMedia = "";
                        if (mediaLastupdate != null)
                        {
                            if (mediatype.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                                streamActivity.MediaUri = mediaLastupdate.Uri.ToUri();
                            else if (mediatype.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                                streamActivity.MediaUri = mediaLastupdate.Uri.ToUri(FileTypeEnum.Video);
                            else
                                streamActivity.MediaUri = mediaLastupdate.Uri.ToUri(FileTypeEnum.Document);
                            infoMedia = mediaLastupdate.BusinessMapping(timezone).UploadedDate.ToString("d MMM yyyy, hh:mmtt");
                        }


                        streamActivity.MediaInfo = $"{Utility.GetFileTypeDescription(me.FileType?.Extension ?? "image")} | {infoMedia}";
                        streamActivity.MediaExtension = me.FileType?.Extension;
                        #endregion
                        break;
                    case ActivityTypeEnum.Link:
                        #region Link
                        var lk = ((QbicleLink)activity).BusinessMapping(timezone);

                        var mediaLinkupdate = lk.FeaturedImage?.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                        streamActivity.Id = lk.Id;
                        streamActivity.Key = lk.Key;
                        streamActivity.StreamId = StreamType.Link.GetId();
                        streamActivity.StreamName = StreamType.Link.GetDescription();
                        streamActivity.IsOwned = lk.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = lk.TimeLineDate.ToShortDateString();
                        streamActivity.CreatedDate = lk.TimeLineDate.Date == today.Date ? "Today, " + lk.TimeLineDate.ToString("hh:mm tt") : lk.TimeLineDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = lk.Topic?.Id.ToString();
                        streamActivity.TopicName = lk.Topic?.Name;
                        streamActivity.CreatedBy = lk.StartedBy.GetFullName();
                        streamActivity.UserAvatar = lk.StartedBy.ProfilePic.ToUri();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == lk.Id);
                        if (lk.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = lk.UpdateReason.GetDescription();
                        streamActivity.MediaUri = mediaLinkupdate?.Uri.ToUri(FileTypeEnum.Document);
                        streamActivity.AppTitle = lk.Name;
                        streamActivity.AppTitleMsg = lk.Description;
                        var myUri = new Uri(lk.URL);
                        streamActivity.LinkUri = !string.IsNullOrEmpty(myUri.Host) ? myUri.Host : lk.URL;
                        #endregion
                        break;
                    case ActivityTypeEnum.PostActivity:
                    case ActivityTypeEnum.QbicleActivity:
                    case ActivityTypeEnum.ApprovalActivity:
                    case ActivityTypeEnum.Domain:
                    case ActivityTypeEnum.RemoveQueue:
                        break;
                    case ActivityTypeEnum.SharedHLPost:
                        var sharedPost = ((HLSharedPost)activity).BusinessMapping(timezone);
                        streamActivity.Id = sharedPost.SharedPost.Id;
                        streamActivity.Key = sharedPost.SharedPost.Key;
                        streamActivity.StreamId = StreamType.HLSharedPost.GetId();
                        streamActivity.StreamName = StreamType.HLSharedPost.GetDescription();
                        streamActivity.IsOwned = sharedPost.SharedBy.Id == currentUserId;
                        streamActivity.TimelineDate = sharedPost.ShareDate.ToShortDateString();
                        streamActivity.CreatedDate = sharedPost.ShareDate.Date == today.Date ? "Today, " + sharedPost.ShareDate.ToString("hh:mm tt") : sharedPost.ShareDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = sharedPost.Topic?.Id.ToString();
                        streamActivity.TopicName = sharedPost.Topic?.Name;
                        streamActivity.CreatedBy = sharedPost.SharedWith.GetFullName();
                        streamActivity.UserAvatar = sharedPost.SharedBy.ProfilePic.ToUri();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == sharedPost.Id);
                        if (sharedPost.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = sharedPost.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Highlight / {sharedPost.SharedPost.Title}";
                        streamActivity.AppTitleMsg = sharedPost.SharedPost.Content;

                        break;
                    case ActivityTypeEnum.SharedPromotion:
                        var sharedPromotion = ((LoyaltySharedPromotion)activity).BusinessMapping(timezone);
                        streamActivity.Id = sharedPromotion.Id;
                        streamActivity.Key = sharedPromotion.Key;
                        streamActivity.StreamId = StreamType.LoyaltySharedPromotion.GetId();
                        streamActivity.StreamName = StreamType.LoyaltySharedPromotion.GetDescription();
                        streamActivity.IsOwned = sharedPromotion.SharedBy.Id == currentUserId;
                        streamActivity.TimelineDate = sharedPromotion.ShareDate.ToShortDateString();
                        streamActivity.CreatedDate = sharedPromotion.ShareDate.Date == today.Date ? "Today, " + sharedPromotion.ShareDate.ToString("hh:mm tt") : sharedPromotion.ShareDate.ToString("dd MMM yyyy, hh:mm tt");
                        streamActivity.TopicId = sharedPromotion.Topic?.Id.ToString();
                        streamActivity.TopicName = sharedPromotion.Topic?.Name;
                        streamActivity.CreatedBy = sharedPromotion.SharedWith.GetFullName();
                        streamActivity.UserAvatar = sharedPromotion.SharedBy.ProfilePic.ToUri();
                        streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == sharedPromotion.Id);
                        if (sharedPromotion.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = sharedPromotion.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Promotion / {sharedPromotion.SharedPromotion.Name}";
                        streamActivity.AppTitleMsg = sharedPromotion.SharedPromotion.CalRemainInfo(timezone, dateFormat, DateTime.UtcNow);

                        break;
                }
            }
            if (isApproval)
            {
                return streamActivity;
            }
            else
                return null;
        }


        private static void MapCatalogDiscussion(MicroActivitesStream streamActivity, QbicleDiscussion ds)
        {
            try
            {
                var discussion = ds as B2CProductMenuDiscussion;

                var catalog = discussion.ProductMenu;
                streamActivity.Key = discussion.Key;
                streamActivity.MediaUri = (string.IsNullOrEmpty(catalog.Image) ? ConfigManager.CatalogDefaultImage : catalog.Image).ToDocumentUri();
                streamActivity.CreatedDate = discussion.TimeLineDate.GetTimeRelative();
                streamActivity.DomainKey = discussion.ProductMenu.Location.Domain.Key;
                streamActivity.ActivityHref = $"/B2C/DiscussionMenu?disKey={discussion.Key}";
                streamActivity.StreamId = discussion.DiscussionType.GetId();
                streamActivity.StreamName = discussion.DiscussionType.GetDescription();
                streamActivity.Name = "Browse";
                streamActivity.AppTitleMsg = discussion.Summary;
                streamActivity.AppTitle = discussion.Name;
                streamActivity.CatalogId = catalog.Id;
                streamActivity.CatalogName = catalog.Name;
                streamActivity.CoveringNote = ds.Summary;
                using (var context = new ApplicationDbContext())
                {
                    var lstIds = catalog.Categories.Select(x => x.Id).ToList();
                    //TODO: QBIC-3927 Customers now can see the non-inventory items (additional service items)          
                    var query = from citem in context.PosCategoryItems
                                where lstIds.Contains(citem.Category.Id)
                                && citem.PosVariants.Any()
                                select citem;

                    streamActivity.CatalogItems = query.Count();
                }
            }
            catch
            {

            }
        }

        private static void MapCatalogDiscussion(MicroStream streamActivity, QbicleDiscussion ds)
        {
            try
            {
                var discussion = ds as B2CProductMenuDiscussion;

                var catalog = discussion.ProductMenu;
                streamActivity.Key = discussion.Key;
                streamActivity.MediaUri = (string.IsNullOrEmpty(catalog.Image) ? ConfigManager.CatalogDefaultImage : catalog.Image).ToDocumentUri();
                streamActivity.CreatedDate = discussion.TimeLineDate.GetTimeRelative();
                streamActivity.DomainKey = discussion.ProductMenu.Location.Domain.Key;
                //streamActivity.ActivityHref = $"/B2C/DiscussionMenu?disKey={discussion.Key}";
                streamActivity.StreamId = discussion.DiscussionType.GetId();
                streamActivity.StreamName = discussion.DiscussionType.GetDescription();
                streamActivity.Name = "Browse";
                streamActivity.AppTitleMsg = discussion.Summary;
                streamActivity.AppTitle = discussion.Name;
                streamActivity.CatalogId = catalog.Id;
                streamActivity.CatalogName = catalog.Name;
                streamActivity.CoveringNote = ds.Summary;
                using (var context = new ApplicationDbContext())
                {
                    var lstIds = catalog.Categories.Select(x => x.Id).ToList();
                    //TODO: QBIC-3927 Customers now can see the non-inventory items (additional service items)      
                    var query = from citem in context.PosCategoryItems
                                where lstIds.Contains(citem.Category.Id)
                                && citem.PosVariants.Any()
                                select citem;

                    streamActivity.CatalogItems = query.Count();
                }
            }
            catch
            {

            }
        }

        public MicroStreams MicroQbicleStreams(MicroStreamParameter filter)
        {
            filter.PageIndex *= HelperClass.activitiesPageSize;
            var tPage = 0;
            var model = GetMicroQbicleStreams(filter, CurrentUser.Timezone, CurrentUser.DateFormat, out tPage);
            //totalPage /= HelperClass.activitiesPageSize;
            return new MicroStreams { Streams = model, TotalPage = tPage };
        }


        public object MicroCommunityStreams(MicroStreamParameter fillterModel)
        {
            var response = new MicroStreams();

            var communityDefault = new MicroCommunityDefault();
            var businessName = "";

            var qbRule = new QbicleRules(dbContext);
            fillterModel.UserId = CurrentUser.Id;

            var isHidden = false;
            C2CQbicle c2cqbicle = null;
            B2CQbicle b2cqbicle = null;
            if (fillterModel.Type == 2)
            {
                c2cqbicle = new C2CRules(dbContext).GetC2CQbicleById(fillterModel.QbicleId);
                if (c2cqbicle == null)
                    return new { communityDefault, streams = response.Streams, response.TotalPage };
            }
            else if (fillterModel.Type == 1)
            {
                b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                if (b2cqbicle == null)
                    return new { communityDefault, streams = response.Streams, response.TotalPage };
            }
            else
            {
                c2cqbicle = new C2CRules(dbContext).GetC2CQbicleById(fillterModel.QbicleId);
                b2cqbicle = new B2CRules(dbContext).GetB2CQbicleById(fillterModel.QbicleId);
                if (c2cqbicle == null && b2cqbicle == null)
                    return new { communityDefault, streams = response.Streams, response.TotalPage };
            }


            response = MicroQbicleStreams(fillterModel);

            Models.ApplicationUser linkUser = null;
            string forename = "";

            if (c2cqbicle != null)
            {
                isHidden = c2cqbicle.IsHidden;

                linkUser = c2cqbicle.Customers.Where(s => s.Id != CurrentUser.Id).FirstOrDefault();
                forename = !string.IsNullOrEmpty(linkUser.Forename) ? linkUser.Forename : linkUser.DisplayUserName;

                communityDefault.CreatedDate = c2cqbicle.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");

                if (c2cqbicle.Status == CommsStatus.Pending)
                {
                    linkUser = c2cqbicle.Customers.Where(s => s.Id != CurrentUser.Id).FirstOrDefault();
                    forename = !string.IsNullOrEmpty(linkUser.Forename) ? linkUser.Forename : linkUser.DisplayUserName;
                    if (c2cqbicle.Source.Id == CurrentUser.Id)
                    {
                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_PENDING_C_TITLE", forename);
                        communityDefault.Description = ResourcesManager._L("COM_C2C_PENDING_C_DESCRIPTION", forename);
                        communityDefault.Action = new List<string> { "CancelRequest" };
                    }
                    else
                    {
                        communityDefault.Image = ConfigManager.CommunityLestTalk.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_PENDING_B_TITLE", forename);
                        communityDefault.Description = ResourcesManager._L("COM_C2C_PENDING_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Accept", "Decline" };
                    }
                    communityDefault.CreatedDate = c2cqbicle.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");
                    return new { communityDefault, streams = response.Streams, response.TotalPage };
                }

                if (c2cqbicle.Status == CommsStatus.Blocked && fillterModel.PageIndex == 0)
                {
                    if (c2cqbicle.Blocker.Id == CurrentUser.Id)
                    {
                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_BLOCK_B_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_C2C_BLOCK_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Unblock" };
                    }
                    else
                    {
                        communityDefault.Image = ConfigManager.CommunityPendingadd.ToDocumentUri();
                        communityDefault.Title = ResourcesManager._L("COM_C2C_BLOCK_C_TITLE", forename);
                        communityDefault.Description = ResourcesManager._L("COM_C2C_BLOCK_C_DESCRIPTION");
                        communityDefault.Action = new List<string> { "RemoveContact" };
                    }
                }
            }
            else if (b2cqbicle != null)
            {
                isHidden = b2cqbicle.IsHidden;
                communityDefault.CreatedDate = b2cqbicle.StartedDate.ConvertTimeFromUtc(CurrentUser.Timezone).ToString($"{CurrentUser.DateFormat} {CurrentUser.TimeFormat}");
                if (b2cqbicle.Status == CommsStatus.Blocked && fillterModel.PageIndex == 0)
                {
                    communityDefault.Image = ConfigManager.CommunityBlocked.ToDocumentUri();

                    if (b2cqbicle.Blocker != null)
                    {
                        communityDefault.Title = ResourcesManager._L("COM_B2C_BLOCK_B_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_B2C_BLOCK_B_DESCRIPTION");
                        communityDefault.Action = new List<string> { "Accept" };
                    }
                    else
                    {
                        businessName = new CommerceRules(dbContext).GetB2bBusinessNameById(b2cqbicle.Business?.Id ?? 0);

                        communityDefault.Title = ResourcesManager._L("COM_B2C_BLOCK_C_TITLE");
                        communityDefault.Description = ResourcesManager._L("COM_B2C_BLOCK_C_DESCRIPTION", businessName);
                        communityDefault.Action = new List<string> { "RemoveContact" };
                    }
                }
                else if (response.TotalPage == 0 && fillterModel.PageIndex == 0)
                {
                    businessName = new CommerceRules(dbContext).GetB2bBusinessNameById(b2cqbicle.Business?.Id ?? 0);

                    communityDefault.Title = ResourcesManager._L("COM_B2B_LETTALK_TITLE"); ;
                    communityDefault.Image = ConfigManager.CommunityLestTalk.ToDocumentUri();
                    communityDefault.Description = ResourcesManager._L("COM_B2B_LETTALK_DESCRIPTION", businessName);
                    communityDefault.Action = new List<string>();
                };
            }

            return new { communityDefault, streams = response.Streams, response.TotalPage };
        }




        private List<MicroStream> GetMicroQbicleStreams(MicroStreamParameter fillterModel, string timeZone, string dateFormat, out int totalPage)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get qbicle streams", null, null, fillterModel, timeZone);
                totalPage = 0;

                var qbicleFillterModel = new QbicleFillterModel
                {
                    ActivityTypes = fillterModel.ActivityTypes,
                    Apps = fillterModel.Apps,
                    Daterange = fillterModel.Daterange,
                    Key = fillterModel.Key,
                    QbicleId = fillterModel.QbicleId,
                    Size = fillterModel.PageIndex,
                    TopicIds = fillterModel.TopicIds,
                    Type = fillterModel.Type,
                    UserId = fillterModel.UserId,
                };
                var model = new QbicleRules(dbContext).GetQbicleStreams(qbicleFillterModel, timeZone, dateFormat);
                totalPage = model.TotalCount / HelperClass.activitiesPageSize;

                if (model.TotalCount % HelperClass.activitiesPageSize > 1)
                    totalPage++;

                if (model.Dates == null || !model.Dates.Any())
                    return new List<MicroStream>();

                var streams = new List<MicroStream>();
                foreach (var item in model.Dates)
                {
                    foreach (var activity in item.Activities)
                    {
                        streams.Add(GenerateMicroActivity(activity, CurrentUser));
                    }
                }

                return streams;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fillterModel, timeZone);
                totalPage = 0;
                return new List<MicroStream>();
            }
        }


        private MicroStream GenerateMicroActivity(object objActivity, ApplicationUser currentUser)
        {
            string currentUserId = currentUser.Id; string dateFormat = currentUser.DateFormat; string timezone = currentUser.Timezone;
            var today = DateTime.UtcNow;

            var streamActivity = new MicroStream();
            if (objActivity is QbiclePost post)
            {
                streamActivity.Id = post.Id;
                streamActivity.Key = post.Key;
                streamActivity.DomainKey = post.Topic.Qbicle.Domain.Key;
                streamActivity.UserAvatar = post.CreatedBy.ProfilePic.ToUri();
                streamActivity.CreatedBy = post.CreatedBy.GetFullName();
                streamActivity.TimelineDate = post.StartedDate.ConvertTimeFromUtc(timezone);
                streamActivity.CreatedDate = DateConvert(post.StartedDate, timezone);
                streamActivity.IsOwned = post.CreatedBy.Id == currentUserId;
                streamActivity.TopicId = post.Topic?.Id.ToString();
                streamActivity.TopicName = post.Topic?.Name;
                streamActivity.PostMessage = post.Message;
                streamActivity.StreamId = StreamType.Post.GetId();
                streamActivity.StreamName = StreamType.Post.GetDescription();
                streamActivity.Name = "Post";
            }

            else if (objActivity is Qbicle qbicle)
            {
                #region Qbicle
                streamActivity.DomainKey = qbicle.Domain.Key;
                streamActivity.Id = qbicle.Id;
                streamActivity.Key = qbicle.Key;
                streamActivity.StreamId = StreamType.Qbicles.GetId();
                streamActivity.StreamName = StreamType.Qbicles.GetDescription();
                streamActivity.TimelineDate = qbicle.LastUpdated.ConvertTimeFromUtc(timezone);
                streamActivity.IsOwned = qbicle.StartedBy.Id == currentUserId;
                streamActivity.CreatedDate = DateConvert(qbicle.StartedDate, timezone);
                streamActivity.Name = qbicle.Name;
                streamActivity.CreatedBy = qbicle.StartedBy.GetFullName();
                streamActivity.UserAvatar = qbicle.StartedBy.ProfilePic.ToUri();
                streamActivity.AppTitle = qbicle.Name;
                streamActivity.AppTitleMsg = qbicle.Description;
                streamActivity.MediaUri = qbicle.LogoUri.ToUri();

                #endregion

            }

            else if (objActivity is QbicleActivity activity)
            {
                streamActivity.DomainKey = activity.Qbicle?.Domain?.Key ?? "";
                switch (activity.ActivityType)
                {
                    case ActivityTypeEnum.ApprovalRequest:
                        var appTrader = (ApprovalReq)activity;
                        #region Trader Approval
                        streamActivity.Key = appTrader.Key;
                        streamActivity.TimelineDate = appTrader.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(appTrader.TimeLineDate, timezone);
                        streamActivity.TopicId = appTrader.Topic?.Id.ToString();
                        streamActivity.TopicName = appTrader.Topic?.Name;
                        //streamActivity.Pinned = pinneds != null && pinneds.Any(e => e == appTrader.Id);

                        switch (appTrader.RequestStatus)
                        {
                            case ApprovalReq.RequestStatusEnum.Pending:
                                if (appTrader.ReviewedBy.Count == 0)
                                    if (appTrader.Transfer.Count > 0)
                                    {
                                        streamActivity.StatusName = TransferStatus.PendingPickup.GetDescription();
                                        streamActivity.StatusId = TransferStatus.PendingPickup.GetId();
                                    }
                                break;
                            case ApprovalReq.RequestStatusEnum.Reviewed:
                                if (appTrader.Transfer.Count > 0)
                                {
                                    streamActivity.StatusName = TransferStatus.PickedUp.GetDescription();
                                    streamActivity.StatusId = TransferStatus.PickedUp.GetId();
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Approved:
                                if (appTrader.Transfer.Count > 0)
                                {
                                    streamActivity.StatusName = TransferStatus.Delivered.GetDescription();
                                    streamActivity.StatusId = TransferStatus.Delivered.GetId();
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Denied:
                                streamActivity.StatusName = TransferStatus.Denied.GetDescription();
                                streamActivity.StatusId = TransferStatus.Denied.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Discarded:
                                streamActivity.StatusName = TransferStatus.Discarded.GetDescription();
                                streamActivity.StatusId = TransferStatus.Discarded.GetId();
                                break;
                        }

                        streamActivity.AppTitle = "Approval Request";
                        streamActivity.AppTitleMsg = appTrader.Name;
                        streamActivity.AppIcon = "";
                        streamActivity.TraderUri = "";
                        streamActivity.StreamName = "Trader";
                        streamActivity.CreatedBy = appTrader.StartedBy.GetFullName();
                        streamActivity.ConsumeCountTask = "";
                        if (appTrader.Transfer != null && appTrader.Transfer.Count > 0)
                        {
                            var transfer = appTrader.Transfer.FirstOrDefault();
                            streamActivity.Id = appTrader.Id;
                            streamActivity.Key = transfer.Key;
                            streamActivity.IsOwned = transfer.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.Transfer.GetId();
                            streamActivity.StreamName = StreamType.Transfer.GetDescription();
                            streamActivity.CreatedBy = transfer.CreatedBy.GetFullName();
                            streamActivity.AppIcon = "icon_delivery.png".ToUriString();
                            //streamActivity.TraderUri = "/TraderTransfers/TransferReview?key=" + transfer.Key;

                            var saleTransfer = transfer.Sale;
                            var purchaseTransfer = transfer.Purchase;

                            if (saleTransfer == null && purchaseTransfer == null)
                                streamActivity.AppTitle = $"{transfer.OriginatingLocation?.Name} to {transfer.DestinationLocation?.Name}";
                            else if (saleTransfer != null)
                                streamActivity.AppTitle = $"{transfer.OriginatingLocation?.Name} to {saleTransfer.Purchaser.Name}";
                            else if (purchaseTransfer != null)
                                streamActivity.AppTitle = $"{purchaseTransfer.Vendor.Name} to {transfer.DestinationLocation?.Name}";
                        }
                        else if (appTrader.StockAudits != null && appTrader.StockAudits.Count > 0)//Audit or Purchase approval only
                        {
                            streamActivity.AppIcon = "icon_audit.png".ToUriString();
                            var stockAudit = appTrader.StockAudits.FirstOrDefault();
                            streamActivity.Id = stockAudit.Id;
                            streamActivity.Key = stockAudit.Key;
                            streamActivity.IsOwned = stockAudit.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.StockAudits.GetId();
                            streamActivity.StreamName = StreamType.StockAudits.GetDescription();
                            streamActivity.AppTitle = "Approval Request";
                            streamActivity.AppTitleMsg = stockAudit.Name;
                            //streamActivity.TraderUri = "/TraderStockAudits/ShiftAuditReview?id=" + stockAudit.Id;
                        }
                        else if (appTrader.Sale != null && appTrader.Sale.Count > 0) //Sale or Purchase approval only
                        {
                            streamActivity.AppIcon = "icon_bookkeeping.png".ToUriString();

                            var sale = appTrader.Sale.FirstOrDefault();
                            streamActivity.IsOwned = sale.CreatedBy.Id == currentUserId;
                            streamActivity.Id = sale.Id;
                            streamActivity.Key = sale.Key;
                            streamActivity.StreamId = StreamType.Sale.GetId();
                            streamActivity.StreamName = StreamType.Sale.GetDescription();
                            streamActivity.CreatedBy = sale.CreatedBy.GetFullName();
                            //streamActivity.TraderUri = "/TraderSales/SaleReview?key=" + sale.Key;
                        }
                        else if (appTrader.Purchase != null && appTrader.Purchase.Count > 0)
                        {
                            streamActivity.AppIcon = "icon_bookkeeping.png".ToUriString();
                            var purchase = appTrader.Purchase.FirstOrDefault();
                            streamActivity.IsOwned = purchase.CreatedBy.Id == currentUserId;
                            streamActivity.Id = purchase.Id;
                            streamActivity.Key = purchase.Key;
                            streamActivity.StreamId = StreamType.Purchase.GetId();
                            streamActivity.StreamName = StreamType.Purchase.GetDescription();
                            streamActivity.CreatedBy = purchase.CreatedBy.GetFullName();
                            //streamActivity.TraderUri = "/TraderPurchases/PurchaseReview?id=" + purchase.Id;
                        }
                        else if (appTrader.TraderContact != null && appTrader.TraderContact.Count > 0)//approval contact
                        {
                            var contact = appTrader.TraderContact.FirstOrDefault();
                            streamActivity.IsOwned = contact.CreatedBy.Id == currentUserId;
                            streamActivity.Id = contact.Id;
                            streamActivity.Key = contact.Key;
                            streamActivity.StreamId = StreamType.TraderContact.GetId();
                            streamActivity.StreamName = StreamType.TraderContact.GetDescription();
                            streamActivity.AppTitle = contact.Name;
                            streamActivity.AppTitleMsg = contact.ContactGroup.Name + " Group";
                            streamActivity.AppIcon = "icon_contact.png".ToUriString();
                            //streamActivity.TraderUri = "/TraderContact/ContactReview?id=" + contact.Id;
                        }
                        else if (appTrader.Invoice != null && appTrader.Invoice.Count > 0) // approval invoice
                        {
                            var invoice = appTrader.Invoice.FirstOrDefault();
                            streamActivity.CreatedBy = invoice.CreatedBy.GetFullName();
                            streamActivity.AppIcon = "icon_invoice.png".ToUriString();

                            streamActivity.IsOwned = invoice.CreatedBy.Id == currentUserId;
                            streamActivity.Id = invoice.Id;
                            streamActivity.Key = invoice.Key;
                            streamActivity.StreamId = StreamType.Invoice.GetId();
                            streamActivity.StreamName = StreamType.Invoice.GetDescription();
                            if (invoice.Purchase != null)
                            {
                                streamActivity.StreamId = StreamType.InvoicePurchase.GetId();
                                streamActivity.StreamName = StreamType.InvoicePurchase.GetDescription();
                                //streamActivity.TraderUri = "/TraderBill/BillReview?id=" + invoice.Id;
                                streamActivity.AppTitle = $"Bill #{invoice.Reference?.FullRef ?? invoice.Id.ToString()}";
                                streamActivity.AppTitleMsg = $"For Purchase #{invoice.Purchase.Reference?.FullRef ?? ""}";
                            }

                            if (invoice.Sale != null)
                            {
                                //streamActivity.TraderUri = "/TraderInvoices/InvoiceReview?key=" + invoice.Key;

                                streamActivity.StreamId = StreamType.InvoiceSale.GetId();
                                streamActivity.StreamName = StreamType.InvoiceSale.GetDescription();
                                streamActivity.AppTitle = $"Invoice #{invoice.Reference?.FullRef ?? invoice.Id.ToString()}";
                                streamActivity.AppTitleMsg = $"For Sale #{invoice.Sale.Reference?.FullRef ?? ""}";
                            }
                        }
                        else if (appTrader.Payments != null && appTrader.Payments.Count > 0) // approval payment
                        {
                            var payment = appTrader.Payments.FirstOrDefault();
                            streamActivity.Id = payment.Id;
                            streamActivity.Key = payment.Key;
                            streamActivity.IsOwned = payment.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.Payments.GetId();
                            streamActivity.StreamName = StreamType.Payments.GetDescription();
                            streamActivity.CreatedBy = payment.CreatedBy.GetFullName();
                            streamActivity.AppTitle = "Payment #" + (payment?.Reference ?? "");
                            streamActivity.AppIcon = "icon_payments.png".ToUriString();
                            //streamActivity.TraderUri = "/TraderPayments/PaymentReview?id=" + payment.Id;

                            if (payment?.AssociatedInvoice != null && payment?.AssociatedInvoice?.Id != null && payment?.AssociatedInvoice?.Id > 0)
                            {
                                streamActivity.AppTitleMsg = $"For Invoice #{payment?.AssociatedInvoice?.Reference?.FullRef ?? ""}";
                            }
                            else
                            {
                                var fromStr = "";
                                var toStr = "";
                                if (payment?.OriginatingAccount?.Name != null)
                                    fromStr = $"From: {payment.OriginatingAccount.Name}";
                                if (payment?.DestinationAccount?.Name != null)
                                    toStr = $"To: {payment.DestinationAccount.Name}";

                                streamActivity.AppTitleMsg = $"{fromStr} {toStr}";

                                if (fromStr == "" && toStr == "")
                                    streamActivity.AppTitleMsg = "No account details available";
                            }

                        }
                        else if (appTrader.SpotCounts != null && appTrader.SpotCounts.Count > 0) // approval SpotCounts
                        {
                            var spotCount = appTrader.SpotCounts.FirstOrDefault();
                            streamActivity.Id = spotCount.Id;
                            streamActivity.Key = spotCount.Key;
                            streamActivity.IsOwned = spotCount.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.SpotCounts.GetId();
                            streamActivity.StreamName = StreamType.SpotCounts.GetDescription();
                            streamActivity.AppTitle = "Approval Request";
                            streamActivity.AppIcon = "icon_spotcount.png".ToUriString();
                            streamActivity.AppTitleMsg = streamActivity.AppTitleMsg.Replace("Spot Count:", "");
                            //streamActivity.TraderUri = "/TraderSpotCount/SpotCountReview?id=" + spotCount.Id;
                            switch (spotCount.Status)
                            {
                                case SpotCountStatus.CountStarted:
                                    streamActivity.StatusName = SpotCountStatus.CountStarted.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.CountStarted.GetId();
                                    break;
                                case SpotCountStatus.CountCompleted:
                                    streamActivity.StatusName = SpotCountStatus.CountCompleted.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.CountCompleted.GetId();
                                    break;
                                case SpotCountStatus.StockAdjusted:
                                    streamActivity.StatusName = SpotCountStatus.StockAdjusted.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.StockAdjusted.GetId();
                                    break;
                                case SpotCountStatus.Denied:
                                    streamActivity.StatusName = SpotCountStatus.Denied.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.Denied.GetId();
                                    break;
                                case SpotCountStatus.Discarded:
                                    streamActivity.StatusName = SpotCountStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = SpotCountStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.WasteReports != null && appTrader.WasteReports.Count > 0) // approval SpotCounts
                        {
                            var wasteReport = appTrader.WasteReports.FirstOrDefault();

                            streamActivity.Id = wasteReport.Id;
                            streamActivity.Key = wasteReport.Key;
                            streamActivity.IsOwned = wasteReport.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.WasteReports.GetId();
                            streamActivity.StreamName = StreamType.WasteReports.GetDescription();
                            streamActivity.AppTitle = "Approval Request";
                            streamActivity.AppIcon = "icon_waste.png".ToUriString();
                            streamActivity.AppTitleMsg = wasteReport.Name;
                            //streamActivity.TraderUri = "/TraderWasteReport/WasteReportReview?id=" + wasteReport.Id;
                            switch (wasteReport.Status)
                            {
                                case WasteReportStatus.Started:
                                    streamActivity.StatusName = WasteReportStatus.Started.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.Started.GetId();
                                    break;
                                case WasteReportStatus.Completed:
                                    streamActivity.StatusName = WasteReportStatus.Completed.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.Completed.GetId();
                                    break;
                                case WasteReportStatus.StockAdjusted:
                                    streamActivity.StatusName = WasteReportStatus.StockAdjusted.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.StockAdjusted.GetId();
                                    break;
                                case WasteReportStatus.Discarded:
                                    streamActivity.StatusName = WasteReportStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = WasteReportStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.Manufacturingjobs != null && appTrader.Manufacturingjobs.Count > 0) // approval Manufacturingjobs
                        {
                            var manufacturingJob = appTrader.Manufacturingjobs.FirstOrDefault();
                            streamActivity.Id = manufacturingJob.Id;
                            streamActivity.Key = manufacturingJob?.Key;
                            streamActivity.IsOwned = manufacturingJob.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.Manufacturingjobs.GetId();
                            streamActivity.StreamName = StreamType.Manufacturingjobs.GetDescription();
                            streamActivity.CreatedBy = manufacturingJob.CreatedBy.GetFullName();
                            streamActivity.AppTitle = "Compound Item Assembly";
                            streamActivity.AppIcon = "icon_manufacturing.png".ToUriString();
                            streamActivity.AppTitleMsg = (manufacturingJob.Reference != null ? manufacturingJob.Reference.FullRef : "") + " " + (manufacturingJob.Product.Name); //$"Items {appTrader.Manufacturingjobs.FirstOrDefault().SelectedRecipe.Ingredients.Count} manufacturing";
                            //streamActivity.TraderUri = "/Manufacturing/ManuJobReview?id=" + manufacturingJob.Id;
                            switch (manufacturingJob.Status)
                            {
                                case ManuJobStatus.Pending:
                                    streamActivity.StatusName = ManuJobStatus.Pending.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Pending.GetId();
                                    break;
                                case ManuJobStatus.Reviewed:
                                    streamActivity.StatusName = ManuJobStatus.Reviewed.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Reviewed.GetId();
                                    break;
                                case ManuJobStatus.Approved:
                                    streamActivity.StatusName = ManuJobStatus.Approved.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Approved.GetId();
                                    break;
                                case ManuJobStatus.Denied:
                                    streamActivity.StatusName = ManuJobStatus.Denied.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Denied.GetId();
                                    break;
                                case ManuJobStatus.Discarded:
                                    streamActivity.StatusName = ManuJobStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = ManuJobStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.CreditNotes != null && appTrader.CreditNotes.Count > 0)// approval Credit
                        {

                            var creditNote = appTrader.CreditNotes.FirstOrDefault();
                            streamActivity.Id = creditNote.Id;
                            streamActivity.Key = creditNote.Key;
                            streamActivity.IsOwned = creditNote.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.CreditNotes.GetId();
                            streamActivity.StreamName = StreamType.CreditNotes.GetDescription();
                            streamActivity.CreatedBy = creditNote.CreatedBy.GetFullName();
                            streamActivity.AppTitle = $"{HelperClass.GetFullNameOfUser(creditNote.CreatedBy)}";
                            streamActivity.AppIcon = "icon_manufacturing.png".ToUriString();
                            if (appTrader.CreditNotes[0].Reason == CreditNoteReason.DebitNote
                               || appTrader.CreditNotes[0].Reason == CreditNoteReason.PriceIncrease)
                                streamActivity.AppTitleMsg = $"Debit note #" + appTrader.CreditNotes[0].Reference?.FullRef;
                            else
                                streamActivity.AppTitleMsg = $"Credit note #" + appTrader.CreditNotes[0].Reference?.FullRef;

                            //streamActivity.TraderUri = "/TraderContact/CreditNoteReview?id=" + creditNote.Id;

                            switch (creditNote.Status)
                            {
                                case CreditNoteStatus.PendingReview:
                                    streamActivity.StatusName = CreditNoteStatus.PendingReview.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.PendingReview.GetId();
                                    break;
                                case CreditNoteStatus.Reviewed:
                                    streamActivity.StatusName = CreditNoteStatus.Reviewed.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Reviewed.GetId();
                                    break;
                                case CreditNoteStatus.Approved:
                                    streamActivity.StatusName = CreditNoteStatus.Approved.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Approved.GetId();
                                    break;
                                case CreditNoteStatus.Denied:
                                    streamActivity.StatusName = CreditNoteStatus.Denied.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Denied.GetId();
                                    break;
                                case CreditNoteStatus.Discarded:
                                    streamActivity.StatusName = CreditNoteStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = CreditNoteStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.BudgetScenarioItemGroups != null && appTrader.BudgetScenarioItemGroups.Count > 0) // approval Budget group item
                        {
                            var budgetScenarioItemGroups = appTrader.BudgetScenarioItemGroups.FirstOrDefault();
                            streamActivity.Id = budgetScenarioItemGroups.Id;
                            streamActivity.Key = budgetScenarioItemGroups.Key;
                            streamActivity.StreamId = StreamType.BudgetScenarioItemGroups.GetId();
                            streamActivity.StreamName = StreamType.BudgetScenarioItemGroups.GetDescription();
                            streamActivity.CreatedBy = budgetScenarioItemGroups.CreatedBy.GetFullName();
                            streamActivity.IsOwned = budgetScenarioItemGroups.CreatedBy.Id == currentUserId;
                            streamActivity.AppTitle = $"{HelperClass.GetFullNameOfUser(budgetScenarioItemGroups.CreatedBy)}";
                            streamActivity.AppIcon = "icon_bookkeeping.png".ToUriString();
                            streamActivity.AppTitleMsg = $"Budget Scenario Items Group: {budgetScenarioItemGroups.BudgetScenario.Title}";
                            //streamActivity.TraderUri = "/TraderBudget/ProcessApproval?id=" + budgetScenarioItemGroups.Id + "&oView=A";
                            switch (budgetScenarioItemGroups.Status)
                            {
                                case BudgetScenarioItemGroupStatus.Pending:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Pending.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Pending.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Reviewed:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Reviewed.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Reviewed.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Approved:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Approved.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Approved.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Draft:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Draft.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Draft.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Denied:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Denied.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Denied.GetId();
                                    break;
                                case BudgetScenarioItemGroupStatus.Discarded:
                                    streamActivity.StatusName = BudgetScenarioItemGroupStatus.Discarded.GetDescription();
                                    streamActivity.StatusId = BudgetScenarioItemGroupStatus.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.TraderReturns != null && appTrader.TraderReturns.Count > 0) // approval Sale return
                        {

                            var traderReturn = appTrader.TraderReturns.FirstOrDefault();
                            streamActivity.Id = traderReturn.Id;
                            streamActivity.Key = traderReturn.Key;
                            streamActivity.StreamId = StreamType.TraderReturns.GetId();
                            streamActivity.StreamName = StreamType.TraderReturns.GetDescription();
                            streamActivity.IsOwned = traderReturn.CreatedBy.Id == currentUserId;
                            streamActivity.CreatedBy = traderReturn.CreatedBy.GetFullName();
                            streamActivity.AppTitle = $"Approval Request";
                            streamActivity.AppIcon = "icon_return.png".ToUriString();
                            streamActivity.AppTitleMsg = $"Reference #{traderReturn.Reference?.FullRef}";
                            //streamActivity.TraderUri = "/TraderSalesReturn/SaleReturnReview?id=" + traderReturn.Id;
                            switch (traderReturn.Status)
                            {
                                case TraderReturnStatusEnum.PendingReview:
                                    streamActivity.StatusName = TraderReturnStatusEnum.PendingReview.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.PendingReview.GetId();
                                    break;
                                case TraderReturnStatusEnum.Reviewed:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Reviewed.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Reviewed.GetId();
                                    break;
                                case TraderReturnStatusEnum.Approved:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Approved.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Approved.GetId();
                                    break;
                                case TraderReturnStatusEnum.Draft:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Draft.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Draft.GetId();
                                    break;
                                case TraderReturnStatusEnum.Denied:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Denied.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Denied.GetId();
                                    break;
                                case TraderReturnStatusEnum.Discarded:
                                    streamActivity.StatusName = TraderReturnStatusEnum.Discarded.GetDescription();
                                    streamActivity.StatusId = TraderReturnStatusEnum.Discarded.GetId();
                                    break;
                            }
                        }
                        else if (appTrader.ConsumptionReports != null && appTrader.ConsumptionReports.Count > 0) // Consume report
                        {
                            var consume = appTrader.ConsumptionReports.FirstOrDefault();
                            streamActivity.Id = consume.Id;
                            streamActivity.Key = consume.Key;
                            streamActivity.StreamId = StreamType.ConsumptionReports.GetId();
                            streamActivity.StreamName = StreamType.ConsumptionReports.GetDescription();
                            streamActivity.IsOwned = consume.CreatedBy.Id == currentUserId;
                            streamActivity.AppTitle = "Consumption Report";
                            streamActivity.AppIcon = "icon_spannered.png".ToUriString();
                            streamActivity.AppTitleMsg = consume?.Name;
                            //streamActivity.TraderUri = "/Spanneredfree/ConsumeReportReview?id=" + consume?.Id;
                            streamActivity.ConsumeCountTask = consume.AssociatedTask != null ? "<li><i class=\"fa fa-link\"></i> 1 Asset Tasks</li>" : "";
                        }
                        else if (appTrader.TillPayment != null && appTrader.TillPayment.Count > 0) // Till payment
                        {
                            var tillPayment = appTrader.TillPayment.FirstOrDefault();
                            streamActivity.Id = tillPayment.Id;
                            streamActivity.Key = tillPayment.Key;
                            streamActivity.StreamId = StreamType.TillPayment.GetId();
                            streamActivity.StreamName = StreamType.TillPayment.GetDescription();
                            streamActivity.IsOwned = tillPayment.CreatedBy.Id == currentUserId;
                            var directionName = tillPayment.Direction == Models.Trader.CashMgt.TillPayment.TillPaymentDirection.InToTill ? "Pay In" : "Pay Out";
                            streamActivity.DiscussionDetail = $"Till {directionName} Approval Request";
                            streamActivity.AppTitle = $"Approval Request";
                            streamActivity.AppIcon = "icon_cash.png".ToUriString();
                            if (tillPayment.Direction == Models.Trader.CashMgt.TillPayment.TillPaymentDirection.InToTill)
                                streamActivity.AppTitleMsg = $"Till Payment from the Safe \"{tillPayment.AssociatedSafe.Name}\" to the Till \"{tillPayment.AssociatedTill.Name}\"";
                            else
                                streamActivity.AppTitleMsg = $"Till Payment from the Till \"{tillPayment.AssociatedTill.Name}\" to the Safe \"{tillPayment.AssociatedSafe.Name}\"";

                            //streamActivity.TraderUri = "/CashManagement/TillPaymentReview?tillPaymentId=" + tillPayment.Id;
                        }

                        #endregion
                        break;

                    case ActivityTypeEnum.ApprovalRequestApp:
                        var app = (ApprovalReq)activity;

                        streamActivity.Id = app.Id;
                        streamActivity.Key = app.Key;
                        streamActivity.IsOwned = app.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = app.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(app.TimeLineDate, timezone);
                        streamActivity.TopicId = app.Topic?.Id.ToString();
                        streamActivity.TopicName = app.Topic?.Name;
                        streamActivity.UserAvatar = app.StartedBy.ProfilePic.ToUri();
                        streamActivity.CreatedBy = app.StartedBy.GetFullName();

                        var jounralApproval = app.JournalEntries.Count > 0;

                        if (app.ReviewedBy.Count > 0)
                        {
                            streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Reviewed.GetDescription();
                            streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                            if (jounralApproval)
                                streamActivity.StatusName = "Awaiting Approval";
                        }
                        switch (app.RequestStatus)
                        {
                            case ApprovalReq.RequestStatusEnum.Pending:
                                if (app.ReviewedBy.Count == 0)
                                {
                                    streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Pending.GetDescription();
                                    streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Pending.GetId();
                                    if (jounralApproval)
                                        streamActivity.StatusName = "Awaiting Review";
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Reviewed:

                                if (jounralApproval || app.CampaigPostApproval.Count > 0)
                                {
                                    streamActivity.StatusName = "Awaiting Approval";
                                    streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Reviewed.GetId();
                                }
                                break;
                            case ApprovalReq.RequestStatusEnum.Approved:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Approved.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Approved.GetId();
                                break;
                            case ApprovalReq.RequestStatusEnum.Denied:
                                streamActivity.StatusName = ApprovalReq.RequestStatusEnum.Denied.GetDescription();
                                streamActivity.StatusId = ApprovalReq.RequestStatusEnum.Denied.GetId();
                                break;
                        }
                        #region Journal Entry
                        if (jounralApproval)
                        {
                            var jounralEntry = app.JournalEntries.FirstOrDefault();

                            streamActivity.StreamId = StreamType.jounralEntries.GetId();
                            streamActivity.StreamName = StreamType.jounralEntries.GetDescription();
                            streamActivity.ActivityCreatedBy = jounralEntry.CreatedBy.GetFullName();
                            streamActivity.AppTitle = "Approval Request / Bookkeeping";
                            streamActivity.AppTitleMsg = $"Journal Entry #{jounralEntry.Number}";
                            streamActivity.Id = jounralEntry.Id;
                            streamActivity.Key = jounralEntry.Key;
                            streamActivity.IsOwned = jounralEntry.CreatedBy.Id == currentUserId;

                        }
                        #endregion
                        #region Campaign post
                        else if (app.CampaigPostApproval.Any())
                        {
                            var isManualCampaign = app.CampaigPostApproval.Any(c => c.CampaignPost.AssociatedCampaign.CampaignType == Qbicles.Models.SalesMkt.CampaignType.Manual);
                            if (!isManualCampaign)
                                streamActivity.AppIcon = "icon_socialpost.png".ToUriString();
                            else
                                streamActivity.AppIcon = "icon_socialpost_manual.png".ToUriString();

                            streamActivity.AppTitle = isManualCampaign ? "Manual Social Post Approval" : "Social Media Post Approval";
                            var campaignApproval = app.CampaigPostApproval.FirstOrDefault();
                            var campaignPost = campaignApproval.CampaignPost;
                            var media = campaignPost.ImageOrVideo;

                            if (media != null)
                            {
                                var mediaLastupdateS = media.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                                //streamActivity.ActivityHref = isManualCampaign ? "ManualSocialPostInApp" : "SocialPostInApp" + $"?id={campaignApproval.Id}";
                                if (media.FileType.Type.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                                    streamActivity.MediaUri = mediaLastupdateS.Uri.ToUri();
                                else if (media.FileType.Type.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                                    streamActivity.MediaUri = mediaLastupdateS.Uri.ToUri(FileTypeEnum.Video);
                                else
                                    streamActivity.MediaUri = media.FileType.IconPath.ToUri(FileTypeEnum.Document);

                            }

                            streamActivity.Id = campaignPost.Id;
                            streamActivity.Key = campaignPost.Key;
                            streamActivity.IsOwned = campaignPost.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.CampaigPost.GetId();
                            streamActivity.StreamName = StreamType.CampaigPost.GetDescription();
                            streamActivity.CampaignName = campaignPost.AssociatedCampaign.Name;
                            streamActivity.CampaignTitle = campaignPost.Title;
                            streamActivity.CampaignContent = campaignPost.Content;
                            streamActivity.ActivityCreatedBy = campaignPost.CreatedBy.GetFullName();
                        }
                        #endregion
                        #region Email post
                        else if (app.EmailPostApproval.Any())
                        {
                            var emailPostApproval = app.EmailPostApproval.FirstOrDefault();
                            var campaignEmail = emailPostApproval.CampaignEmail;

                            streamActivity.AppTitle = "Sales & Marketing Email Approval";

                            switch (emailPostApproval.ApprovalStatus)
                            {
                                case SalesMktApprovalStatusEnum.InReview:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.InReview.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.InReview.GetId();
                                    break;
                                case SalesMktApprovalStatusEnum.Approved:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.Approved.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.Approved.GetId();
                                    break;
                                case SalesMktApprovalStatusEnum.Denied:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.Denied.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.Denied.GetId();
                                    break;
                                case SalesMktApprovalStatusEnum.Queued:
                                    streamActivity.StatusName = SalesMktApprovalStatusEnum.Queued.GetDescription();
                                    streamActivity.StatusId = SalesMktApprovalStatusEnum.Queued.GetId();
                                    break;
                            }

                            //streamActivity.ActivityHref = $"/SalesMarketing/EmailPostInApp?id={emailPostApproval.Id}";
                            streamActivity.MediaUri = campaignEmail.FeaturedImageUri.ToUri(FileTypeEnum.Document);

                            streamActivity.Id = campaignEmail.Id;
                            streamActivity.Key = campaignEmail.Key;
                            streamActivity.IsOwned = campaignEmail.CreatedBy.Id == currentUserId;

                            streamActivity.StreamId = StreamType.EmailPost.GetId();
                            streamActivity.StreamName = StreamType.EmailPost.GetDescription();
                            streamActivity.CampaignName = campaignEmail.Campaign.Name;
                            streamActivity.CampaignTitle = campaignEmail.Title;
                            streamActivity.ActivityCreatedBy = campaignEmail.CreatedBy.GetFullName();
                        }
                        #endregion
                        #region Operator clock IN
                        else if (app.OperatorClockIn.Any())
                        {
                            streamActivity.AppTitle = app.Name;
                            var clockedInApproval = app.OperatorClockIn.FirstOrDefault();
                            streamActivity.Id = clockedInApproval.Id;
                            streamActivity.Key = clockedInApproval.Key;
                            streamActivity.IsOwned = clockedInApproval.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.OperatorClockIn.GetId();
                            streamActivity.StreamName = StreamType.OperatorClockIn.GetDescription();
                            streamActivity.MediaUri = clockedInApproval.People.ProfilePic.ToUri();
                            //streamActivity.ActivityHref = "/Operator/Clocked?id=@clockedInApproval.Id&type=clockin";
                            streamActivity.CampaignName = "Clock in /Operator";
                            streamActivity.CampaignTitle = clockedInApproval.Notes;
                            streamActivity.Location = clockedInApproval.WorkGroup.Location.Name;
                            streamActivity.ActivityEventDate = clockedInApproval.Date.ConvertTimeFromUtc(timezone, dateFormat);
                            streamActivity.ActivityEventTime = clockedInApproval.TimeIn.ConvertTimeFromUtc(timezone).ToString("hh:mmtt").ToLower();
                        }
                        #endregion
                        #region Operator clock OuT
                        else if (app.OperatorClockOut.Any())
                        {

                            streamActivity.AppTitle = app.Name;
                            var clockedOutApproval = app.OperatorClockOut.FirstOrDefault();
                            streamActivity.Id = clockedOutApproval.Id;
                            streamActivity.Key = clockedOutApproval.Key;
                            streamActivity.IsOwned = clockedOutApproval.CreatedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.OperatorClockOut.GetId();
                            streamActivity.StreamName = StreamType.OperatorClockOut.GetDescription();
                            streamActivity.MediaUri = clockedOutApproval.People.ProfilePic.ToUri();
                            //streamActivity.ActivityHref = "/Operator/Clocked?id=@clockedInApproval.Id&type=clockin";
                            streamActivity.CampaignName = "Clock out /Operator";
                            streamActivity.CampaignTitle = clockedOutApproval.Notes;
                            streamActivity.Location = clockedOutApproval.WorkGroup.Location.Name;
                            streamActivity.ActivityEventDate = clockedOutApproval.Date.ConvertTimeFromUtc(timezone, dateFormat);
                            streamActivity.ActivityEventTime = clockedOutApproval.TimeIn.ConvertTimeFromUtc(timezone).ToString("hh:mmtt").ToLower();
                        }
                        #endregion
                        #region Other approval
                        else
                        {
                            streamActivity.Id = app.Id;
                            streamActivity.Key = app.Key;
                            streamActivity.IsOwned = app.StartedBy.Id == currentUserId;
                            streamActivity.StreamId = StreamType.ApprovalRequestApp.GetId();
                            streamActivity.StreamName = StreamType.ApprovalRequestApp.GetDescription();
                            streamActivity.AppTitle = $"Approval Request / {app.Name}";
                            streamActivity.AppTitleMsg = $"Journal Entry #{app.Notes}";
                        }
                        #endregion
                        break;
                    case ActivityTypeEnum.TaskActivity:
                        #region Task
                        var tk = (QbicleTask)activity;
                        streamActivity.Id = tk.Id;
                        streamActivity.Key = tk.Key;
                        streamActivity.StreamId = StreamType.Task.GetId();
                        streamActivity.StreamName = StreamType.Task.GetDescription();
                        streamActivity.IsOwned = tk.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = tk.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(tk.TimeLineDate, timezone);
                        streamActivity.TopicId = tk.Topic?.Id.ToString();
                        streamActivity.TopicName = tk.Topic?.Name;
                        streamActivity.CreatedBy = tk.StartedBy.GetFullName();
                        streamActivity.UserAvatar = tk.StartedBy.ProfilePic.ToUri();
                        if (tk.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = tk.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Task / {tk.Name}";
                        streamActivity.AppTitleMsg = tk.Description;

                        var _cbTask = tk.task;
                        if (_cbTask != null)
                        {
                            //streamActivity.Key =_cbTask.Key
                            streamActivity.UserAvatar = (tk.Asset?.FeaturedImageUri ?? tk.StartedBy.ProfilePic).ToUri();
                            streamActivity.StreamId = StreamType.Cleanbook.GetId();
                            streamActivity.StreamName = StreamType.Cleanbook.GetDescription();
                            streamActivity.AppTitle = $"Cleanbooks Task / {tk.Name}";
                            streamActivity.AppTitleMsg = tk.Description;
                            //streamActivity.ActivityHref = "/Apps/Tasks";
                            streamActivity.TaskType = _cbTask.tasktype?.Name;
                            streamActivity.TaskPriority = tk.Priority.GetDescription();
                        }
                        else if (tk.ComplianceTask != null)
                        {
                            //streamActivity.Key = tk.ComplianceTask.Key;
                            //streamActivity.ActivityHref = $"/Operator/ComplianceTask?id={tk.ComplianceTask.Id}&{tk.Id}";
                            streamActivity.TaskCompliance = tk.ComplianceTask.Type == Models.Operator.Compliance.TaskType.Repeatable ? "Repeating " : "" + "Compliance Task / Operator";
                            streamActivity.TaskForms = tk.ComplianceTask.OrderedForms.Count() + " Forms";

                            streamActivity.StreamId = StreamType.ComplianceTask.GetId();
                            streamActivity.StreamName = StreamType.ComplianceTask.GetDescription();
                            var minutes = tk.ComplianceTask.OrderedForms.Sum(st => st.FormDefinition.EstimatedTime);
                            streamActivity.TaskComplianceTotal = minutes > 60 ? $"{minutes / 60}h" : $"{minutes}m" + " total";
                            streamActivity.TaskRecurring = tk.isRecurs ? "Recurring" : "";
                        }
                        else
                        {
                            streamActivity.Key = tk.Key;
                            streamActivity.TaskPriority = tk.Priority.GetDescription();
                            streamActivity.TaskRecurring = tk.isRecurs ? "Recurring" : "";
                            if (tk.ProgrammedStart.HasValue && tk.ProgrammedEnd.HasValue)
                            {
                                if (tk.ProgrammedStart.Value.Date == tk.ProgrammedEnd.Value.Date)
                                {
                                    streamActivity.ActivityEventDate = tk.ProgrammedStart.Value.ConvertTimeFromUtc(timezone, dateFormat);
                                    streamActivity.ActivityEventTime = tk.ProgrammedStart.Value.ConvertTimeFromUtc(timezone).ToString("hh:mmtt") + " - " + tk.ProgrammedEnd.Value.ToString("hh:mmtt");
                                }
                                else
                                    streamActivity.ActivityEventDate = tk.ProgrammedStart.Value.ConvertTimeFromUtc(timezone).ToString(dateFormat + " hh:mmtt") + " - " + tk.ProgrammedEnd.Value.ToString(dateFormat + " hh:mmtt");
                            }
                        }
                        #endregion
                        break;
                    case ActivityTypeEnum.DiscussionActivity:
                    case ActivityTypeEnum.OrderCancellation:
                        #region Discussion
                        var ds = (QbicleDiscussion)activity;
                        streamActivity.Id = ds.Id;
                        streamActivity.Key = ds.Key;
                        streamActivity.StreamId = StreamType.Discussion.GetId();
                        streamActivity.StreamName = StreamType.Discussion.GetDescription();
                        streamActivity.IsOwned = ds.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = ds.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(ds.TimeLineDate, timezone);
                        streamActivity.TopicId = ds.Topic?.Id.ToString();
                        streamActivity.TopicName = ds.Topic?.Name;
                        streamActivity.CreatedBy = ds.StartedBy.GetFullName();
                        streamActivity.UserAvatar = ds.StartedBy.ProfilePic.ToUri();
                        streamActivity.MediaUri = ds.FeaturedImageUri?.ToUri();
                        streamActivity.CoveringNote = ds.Summary;
                        if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2CProductMenu || ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder)
                        {
                            if (ds is B2CProductMenuDiscussion)
                            {
                                MapCatalogDiscussion(streamActivity, ds);
                            }

                            else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder)
                            {
                                streamActivity.MediaUri = ConfigManager.Communitybuysell.ToDocumentUri();
                                streamActivity.Name = "View order";
                                //streamActivity.ActivityHref = $"/B2C/DiscussionOrder?disKey={ds.Key}";
                                if (ds is B2COrderCreation)
                                {
                                    var b2COrderCreation = (B2COrderCreation)ds;
                                    streamActivity.TradeOrderId = b2COrderCreation.TradeOrder.Id;
                                    streamActivity.TradeOrderKey = b2COrderCreation.TradeOrder.Key;
                                    streamActivity.StatusId = b2COrderCreation.TradeOrder.OrderStatus.GetId();
                                    streamActivity.StatusName = b2COrderCreation.TradeOrder.OrderStatus.GetDescription();
                                    streamActivity.TraderId = ds.Id;
                                    streamActivity.TraderKey = ds.Key;
                                    streamActivity.SalesChannel = b2COrderCreation.TradeOrder.SalesChannel;
                                    streamActivity.CoveringNote = ds.Summary;
                                }
                            }
                            else
                            {
                                streamActivity.MediaUri = ConfigManager.CommunityShop.ToDocumentUri();
                                streamActivity.Name = "View & manage";
                                //streamActivity.ActivityHref = $"/B2C/DiscussionMenu?disKey={ds.Key}";
                            }

                            streamActivity.StreamId = StreamType.DiscussionOrder.GetId();
                            streamActivity.StreamName = StreamType.DiscussionOrder.GetDescription();

                            streamActivity.AppTitleMsg = ds.Summary;
                            streamActivity.AppTitle = ds.Name;

                        }
                        else if (ds is B2CProductMenuDiscussion)
                        {
                            MapCatalogDiscussion(streamActivity, ds);

                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.B2COrder)
                        {
                            if (ds is B2COrderCreation)
                            {
                                var b2COrderCreation1 = (B2COrderCreation)ds;

                                streamActivity.TradeOrderId = b2COrderCreation1.TradeOrder.Id;
                                streamActivity.TradeOrderKey = b2COrderCreation1.TradeOrder.Key;
                                streamActivity.StatusId = b2COrderCreation1.TradeOrder.OrderStatus.GetId();
                                streamActivity.StatusName = b2COrderCreation1.TradeOrder.OrderStatus.GetDescription();
                                streamActivity.TraderId = ds.Id;
                                streamActivity.TraderKey = ds.Key;
                                streamActivity.SalesChannel = b2COrderCreation1.TradeOrder.SalesChannel;
                                streamActivity.CoveringNote = ds.Summary;
                            }
                            streamActivity.StreamId = StreamType.DiscussionOrder.GetId();
                            streamActivity.StreamName = StreamType.DiscussionOrder.GetDescription();
                            streamActivity.MediaUri = ConfigManager.Communitybuysell.ToDocumentUri();
                            streamActivity.Name = "View order";
                            streamActivity.AppTitleMsg = ds.Summary;
                            streamActivity.AppTitle = ds.Name;
                            streamActivity.CoveringNote = ds.Summary;
                            //streamActivity.ActivityHref = $"/B2C/DiscussionOrder?disKey={ds.Key}";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.IdeaDiscussion)
                        {
                            //streamActivity.ActivityHref = "/SalesMarketingIdea/DiscussionIdea?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Sales & Marketing > Ideas/Themes";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Sales & Marketing Theme Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.PlaceDiscussion)
                        {
                            //streamActivity.ActivityHref = "/SalesMarketingLocation/DiscussionPlace?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Sales & Marketing > Places";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Sales & Marketing Place Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion)
                        {
                            //streamActivity.ActivityHref = "/Operator/DiscussionGoal?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Operator > Goal";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Operator Goal Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion)
                        {
                            //streamActivity.ActivityHref = "/Operator/DiscussionComplianceTask?disId=" + ds.Id;
                            streamActivity.DiscussionBreadcrumb = "via Operator > Compliance Task";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Operator Compliance Task Discussion";
                        }
                        else if (ds.DiscussionType == QbicleDiscussion.DiscussionTypeEnum.CashManagement)
                        {
                            //var nameStringList = ds.Name.Split(' ');
                            //streamActivity.ActivityHref = "/CashManagement/DiscussionCashManagementShow?disKey=" + ds.Key;
                            streamActivity.DiscussionBreadcrumb = "via Cash Management";
                            streamActivity.DiscussionDetail = ds.Name;
                            streamActivity.AppTitle = "Cash Management Discussion";
                        }
                        else
                        {
                            //streamActivity.ActivityHref = "/Qbicles/DiscussionQbicle?disKey=" + ds.Key;
                            streamActivity.DiscussionDetail = $"{ds.ActivityMembers.Count} people";
                            streamActivity.AppTitle = ds.Name;
                            streamActivity.DiscussionBreadcrumb = "";
                        }
                        streamActivity.AppTitleMsg = ds.Summary;
                        #endregion
                        break;
                    case ActivityTypeEnum.AlertActivity:
                        #region Alert
                        var al = (QbicleAlert)activity;

                        streamActivity.Id = al.Id;
                        streamActivity.Key = al.Key;
                        streamActivity.StreamId = StreamType.Alert.GetId();
                        streamActivity.StreamName = StreamType.Alert.GetDescription();
                        streamActivity.IsOwned = al.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = al.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(al.TimeLineDate, timezone);
                        streamActivity.TopicId = al.Topic?.Id.ToString();
                        streamActivity.TopicName = al.Topic?.Name;
                        streamActivity.CreatedBy = al.StartedBy.GetFullName();
                        streamActivity.UserAvatar = al.StartedBy.ProfilePic.ToUri();
                        if (al.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = al.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Alert / {al.Name}";
                        streamActivity.AppTitleMsg = al.Content;
                        #endregion
                        break;
                    case ActivityTypeEnum.EventActivity:
                        #region Event
                        var ev = (QbicleEvent)activity;

                        streamActivity.StreamId = StreamType.Event.GetId();
                        streamActivity.StreamName = StreamType.Event.GetDescription();
                        streamActivity.Id = ev.Id;
                        streamActivity.Key = ev.Key;
                        streamActivity.IsOwned = ev.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = ev.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(ev.TimeLineDate, timezone);
                        streamActivity.TopicId = ev.Topic?.Id.ToString();
                        streamActivity.TopicName = ev.Topic?.Name;
                        streamActivity.CreatedBy = ev.StartedBy.GetFullName();
                        streamActivity.UserAvatar = ev.StartedBy.ProfilePic.ToUri();
                        streamActivity.AppTitle = $"Event / {ev.Name}";
                        streamActivity.AppTitleMsg = ev.Description;
                        if (ev.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = ev.UpdateReason.GetDescription();
                        if (ev.Start.Date == ev.End.Date)
                        {
                            streamActivity.ActivityEventDate = ev.Start.FormatDateTimeByUser(dateFormat);
                            streamActivity.ActivityEventTime = ev.Start.ToString("hh:mmtt") + " - " + ev.End.ToString("hh:mmtt");
                        }
                        else
                            streamActivity.ActivityEventDate = ev.Start.ToString(dateFormat + " hh:mmtt") + " - " + ev.End.ToString(dateFormat + " hh:mmtt");
                        streamActivity.Location = ev.Location;
                        #endregion
                        break;
                    case ActivityTypeEnum.MediaActivity:
                        #region Media
                        var me = (QbicleMedia)activity;
                        var mediaLastupdate = me.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                        var mediatype = me.FileType?.Type ?? "";
                        streamActivity.Id = me.Id;
                        streamActivity.Key = me.Key;
                        streamActivity.StreamId = StreamType.Medias.GetId();
                        streamActivity.StreamName = StreamType.Medias.GetDescription();
                        streamActivity.IsOwned = me.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = me.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(me.TimeLineDate, timezone);
                        streamActivity.TopicId = me.Topic?.Id.ToString();
                        streamActivity.TopicName = me.Topic?.Name;
                        streamActivity.CreatedBy = me.StartedBy.GetFullName();
                        streamActivity.UserAvatar = me.StartedBy.ProfilePic.ToUri();
                        streamActivity.AppTitle = $"Media / {me.Name}";
                        streamActivity.AppTitleMsg = me.Description;
                        var infoMedia = "";
                        if (mediaLastupdate != null)
                        {
                            if (mediatype.Equals("Image File", StringComparison.OrdinalIgnoreCase))
                                streamActivity.MediaUri = mediaLastupdate.Uri.ToUri();
                            else if (mediatype.Equals("Video File", StringComparison.OrdinalIgnoreCase))
                                streamActivity.MediaUri = mediaLastupdate.Uri.ToUri(FileTypeEnum.Video);
                            else
                                streamActivity.MediaUri = mediaLastupdate.Uri.ToUri(FileTypeEnum.Document);
                            infoMedia = mediaLastupdate.UploadedDate.ConvertTimeFromUtc(timezone).ToString("d MMM yyyy, hh:mmtt");
                        }


                        streamActivity.MediaInfo = $"{Utility.GetFileTypeDescription(me.FileType?.Extension ?? "image")} | {infoMedia}";
                        streamActivity.MediaExtension = me.FileType?.Extension;
                        #endregion
                        break;
                    case ActivityTypeEnum.Link:
                        #region Link
                        var lk = (QbicleLink)activity;

                        var mediaLinkupdate = lk.FeaturedImage?.VersionedFiles.Where(e => !e.IsDeleted).OrderByDescending(x => x.UploadedDate).FirstOrDefault();
                        streamActivity.Id = lk.Id;
                        streamActivity.Key = lk.Key;
                        streamActivity.StreamId = StreamType.Link.GetId();
                        streamActivity.StreamName = StreamType.Link.GetDescription();
                        streamActivity.IsOwned = lk.StartedBy.Id == currentUserId;
                        streamActivity.TimelineDate = lk.TimeLineDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(lk.TimeLineDate, timezone);
                        streamActivity.TopicId = lk.Topic?.Id.ToString();
                        streamActivity.TopicName = lk.Topic?.Name;
                        streamActivity.CreatedBy = lk.StartedBy.GetFullName();
                        streamActivity.UserAvatar = lk.StartedBy.ProfilePic.ToUri();
                        if (lk.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = lk.UpdateReason.GetDescription();
                        streamActivity.MediaUri = mediaLinkupdate?.Uri.ToUri(FileTypeEnum.Document);
                        streamActivity.AppTitle = lk.Name;
                        streamActivity.AppTitleMsg = lk.Description;
                        var myUri = new Uri(lk.URL);
                        streamActivity.LinkUri = !string.IsNullOrEmpty(myUri.Host) ? myUri.Host : lk.URL;
                        #endregion
                        break;
                    case ActivityTypeEnum.PostActivity:
                    case ActivityTypeEnum.QbicleActivity:
                    case ActivityTypeEnum.ApprovalActivity:
                    case ActivityTypeEnum.Domain:
                    case ActivityTypeEnum.RemoveQueue:
                        break;
                    case ActivityTypeEnum.SharedHLPost:
                        var sharedPost = (HLSharedPost)activity;
                        streamActivity.Id = sharedPost.SharedPost.Id;
                        streamActivity.Key = sharedPost.SharedPost.Key;
                        streamActivity.StreamId = StreamType.HLSharedPost.GetId();
                        streamActivity.StreamName = StreamType.HLSharedPost.GetDescription();
                        streamActivity.IsOwned = sharedPost.SharedBy.Id == currentUserId;
                        streamActivity.TimelineDate = sharedPost.ShareDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(sharedPost.ShareDate, timezone);
                        streamActivity.TopicId = sharedPost.Topic?.Id.ToString();
                        streamActivity.TopicName = sharedPost.Topic?.Name;
                        streamActivity.CreatedBy = sharedPost.SharedWith.GetFullName();
                        streamActivity.UserAvatar = sharedPost.SharedBy.ProfilePic.ToUri();
                        if (sharedPost.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = sharedPost.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Highlight / {sharedPost.SharedPost.Title}";
                        streamActivity.AppTitleMsg = sharedPost.SharedPost.Content;

                        var lstConnectedBusinessIdWithCurrentUser = dbContext.B2CQbicles
                            .Where(p => p.Customer.Id == currentUser.Id
                            && !p.RemovedForUsers.Any(x => x.Id == currentUser.Id)
                            && p.Business.Status != QbicleDomain.DomainStatusEnum.Closed
                            && p.Status != CommsStatus.Blocked).Select(p => p.Business.Id).ToList();


                        streamActivity.ObjectData = new HighlightPostRules(dbContext).MapHighlightPost2HighlightModel(sharedPost.SharedPost, currentUser.Id, timezone, dateFormat + ' ' + currentUser.TimeFormat, lstConnectedBusinessIdWithCurrentUser);
                        //streamActivity.ObjectData = ActivityPostHtmlTemplateRules.getHighLigthPostHtml(sharedPost, currentUserId, timezone, dateFormat, false);
                        break;
                    case ActivityTypeEnum.SharedPromotion:
                        var sharedPromotion = (LoyaltySharedPromotion)activity;
                        streamActivity.Id = sharedPromotion.Id;
                        streamActivity.Key = sharedPromotion.Key;
                        streamActivity.StreamId = StreamType.LoyaltySharedPromotion.GetId();
                        streamActivity.StreamName = StreamType.LoyaltySharedPromotion.GetDescription();
                        streamActivity.IsOwned = sharedPromotion.SharedBy.Id == currentUserId;
                        streamActivity.TimelineDate = sharedPromotion.ShareDate.ConvertTimeFromUtc(timezone);
                        streamActivity.CreatedDate = DateConvert(sharedPromotion.StartedDate, timezone);
                        streamActivity.TopicId = sharedPromotion.Topic?.Id.ToString();
                        streamActivity.TopicName = sharedPromotion.Topic?.Name;
                        streamActivity.CreatedBy = sharedPromotion.SharedWith.GetFullName();
                        streamActivity.UserAvatar = sharedPromotion.SharedBy.ProfilePic.ToUri();
                        if (sharedPromotion.UpdateReason != ActivityUpdateReasonEnum.NoUpdates)
                            streamActivity.UpdateReason = sharedPromotion.UpdateReason.GetDescription();
                        streamActivity.AppTitle = $"Promotion / {sharedPromotion.SharedPromotion.Name}";
                        streamActivity.AppTitleMsg = sharedPromotion.SharedPromotion.CalRemainInfo(timezone, dateFormat, DateTime.UtcNow);
                        //streamActivity.ObjectData = ActivityPostHtmlTemplateRules.getSharedPromotionHtml(sharedPromotion, currentUserId, timezone, dateFormat, false);


                        var sPromotion = sharedPromotion.SharedPromotion;
                        var businessProfileId = sPromotion.Domain.Id.BusinesProfile()?.Id;
                        //The user should be connected to the Business automatically if they are not already

                        var b2cqbicles = dbContext.B2CQbicles.AsNoTracking().Where(e => !e.IsHidden && e.Customer.Id == currentUserId).Select(b => b.Business.Id).ToList();
                        var b2bProfileConnected = dbContext.B2BProfiles.Any(e => e.Id == businessProfileId && b2cqbicles.Contains(e.Domain.Id));

                        streamActivity.ObjectData = new PromotionModel
                        {
                            PromotionKey = sPromotion.Key,
                            Name = sPromotion.Name,
                            Description = sPromotion.Description,
                            DisplayDate = sPromotion.DisplayDate.ConvertTimeFromUtc(timezone),
                            VoucherExpiryDate = sPromotion.VoucherInfo.VoucherExpiryDate.ConvertTimeFromUtc(timezone) ?? sPromotion.EndDate.ConvertTimeFromUtc(timezone),
                            StartDate = sPromotion.StartDate.ConvertTimeFromUtc(timezone),
                            EndDate = sPromotion.EndDate.ConvertTimeFromUtc(timezone),
                            IsHalted = sPromotion.IsHalted,
                            IsArchived = sPromotion.IsArchived,
                            FeaturedImageUri = sPromotion.FeaturedImageUri.ToDocumentUri(),
                            BusinessName = sPromotion.Domain.Id.BusinesProfile()?.BusinessName ?? sPromotion.Domain.Name,
                            BusinessKey = sPromotion.Domain.Id.BusinesProfile()?.Key ?? sPromotion.Domain.Key,
                            BusinessProfileId = sPromotion.Domain.Id.BusinesProfile()?.Id ?? sPromotion.Domain.Id,
                            IsLiked = sPromotion.LikingUsers.Any(l => l.Id == currentUserId),
                            AllowClaimNow = sPromotion.CheckAllowClaimNow(currentUserId, today),
                            RemainHtmlInfo = sPromotion.CalRemainPromotionInfo(timezone, dateFormat, today),
                            RemainInfo = sPromotion.CalRemainInfo(timezone, dateFormat, today),
                            DomainLogo = sPromotion.Domain.Id.BusinesProfile().LogoUri.ToDocumentUri().ToString(),
                            IsMarkedLiked = sPromotion.LikedBy.Any(l => l.Id == currentUserId),
                            MarkedLikedCount = sPromotion.LikedBy?.Count ?? 0,
                            TotalClaimed = sPromotion.Vouchers.Count(e => e.ClaimedBy != null && !e.IsRedeemed),
                            IsConnected = b2bProfileConnected
                        };

                        break;
                }
            }

            return streamActivity;
        }

        private static string DateConvert(DateTime date, string timezone)
        {
            var dateUtc = date.ConvertTimeFromUtc(timezone);
            return dateUtc == DateTime.UtcNow.Date ? "Today, " + dateUtc.ToString("hh:mm tt") : dateUtc.ToString("dd MMM yyyy, hh:mm tt");
        }

        public object ChatVisibilityAlert(int id)
        {
            return new NotificationRules(dbContext).GetAlertNotificationMicro(CurrentUser.Timezone, id);
        }
        public object ChatVisibilityAlertList(AlertNotificationParameter parameter)
        {
            var pagination = new PaginationRequest { pageNumber = parameter.pageNumber, pageSize = parameter.pageSize };
            var alertNotification = new AlertNotificationModel { Ids = parameter.Ids, IsShowAlertBusiness = parameter.IsShowAlertBusiness, IsShowAlertCustomer = parameter.IsShowAlertCustomer };
            return new NotificationRules(dbContext).GetListAlertNotificationMicro(pagination, CurrentUser.Timezone, alertNotification);
        }
    }
}
