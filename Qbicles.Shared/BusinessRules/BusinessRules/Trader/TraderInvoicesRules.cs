using Microsoft.Reporting.WebForms;
using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Trader
{
    public class TraderInvoicesRules
    {
        private readonly ApplicationDbContext dbContext;

        public TraderInvoicesRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public Invoice GetById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id);
                return dbContext.Invoices.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id);
                return new Invoice();
            }
        }

        public List<ApprovalStatusTimeline> InvoiceApprovalStatusTimeline(int id, string timezone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timezone);
                var timeline = new List<ApprovalStatusTimeline>();
                var logs = dbContext.InvoiceProcessLogs.Where(e => e.AssociatedInvoice.Id == id).OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.InvoiceStatus)
                    {
                        case TraderInvoiceStatusEnum.PendingReview:
                            icon = "fa fa-info bg-aqua";
                            break;

                        case TraderInvoiceStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case TraderInvoiceStatusEnum.InvoiceApproved:
                            icon = "fa fa-check bg-green";
                            break;

                        case TraderInvoiceStatusEnum.InvoiceDiscarded:
                            icon = "fa fa-warning bg-red";
                            break;

                        case TraderInvoiceStatusEnum.InvoiceDenied:
                            icon = "fa fa-warning bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timezone).ToShortTimeString(),
                            Status = log.InvoiceStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
                        }
                    );
                }

                return timeline;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<ApprovalStatusTimeline>();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="invoice"></param>
        /// <param name="token"></param>
        /// <param name="user">current user</param>
        /// <returns></returns>
        public ReturnJsonModel SaveSaleInvoice(Invoice invoice, string userId, string originatingConnectionId = "")
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, invoice);
                var user = dbContext.QbicleUser.Find(userId);
                invoice.Reference = new TraderReferenceRules(dbContext).GetById(invoice.Reference.Id);

                if (invoice.InvoiceItems != null && invoice.InvoiceItems.Count > 0)
                    invoice.TotalInvoiceAmount = invoice.InvoiceItems.Sum(e => e.InvoiceValue);

                int invoiceId;
                var sale = new TraderSaleRules(dbContext).GetById(invoice.Sale.Id);
                if (invoice.Id == 0)
                {
                    var sInvoice = new Invoice
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        DueDate = invoice.DueDate,
                        InvoiceAddress = invoice.InvoiceAddress?.TrimEnd(),
                        Workgroup = new TraderWorkGroupsRules(dbContext).GetById(invoice.Workgroup.Id),
                        PaymentDetails = invoice.PaymentDetails?.TrimEnd(),
                        Sale = sale,
                        Reference = invoice.Reference,
                        Status = invoice.Status,
                        TotalInvoiceAmount = invoice.TotalInvoiceAmount,
                        InvoicePDF = "",
                        InvoiceItems = new List<InvoiceTransactionItems>(),
                        //Payments = new List<CashAccountTransaction>(),//This is the number of CashAccountTransactions (Payments) that are associated with the Invoice (Invoice.Payments.Count)
                        Purchase = null
                    };
                    foreach (var item in invoice.InvoiceItems)
                    {
                        var transactionItem = dbContext.TraderSaleItems.Find(item.TransactionItem?.Id);
                        decimal? taxValue = null;
                        if (transactionItem?.Taxes != null && transactionItem?.Taxes.Count() > 0)
                        {
                            taxValue = transactionItem.Taxes.Sum(t => t.Value);
                        }

                        sInvoice.InvoiceItems.Add(new InvoiceTransactionItems
                        {
                            InvoiceValue = item.InvoiceValue,
                            InvoiceDiscountValue = item.InvoiceDiscountValue,
                            InvoiceItemQuantity = item.InvoiceItemQuantity,
                            InvoiceTaxValue = taxValue,
                            TransactionItem = transactionItem
                        });
                    }

                    dbContext.Invoices.Add(sInvoice);
                    dbContext.Entry(sInvoice).State = EntityState.Added;
                    dbContext.SaveChanges();

                    invoiceId = sInvoice.Id;
                    refModel.actionVal = 1;
                }
                else
                {
                    var uInvoice = GetById(invoice.Id);
                    if (uInvoice.Reference == null)
                    {
                        uInvoice.Reference = new TraderReferenceRules(dbContext).GetNewReference(invoice.Workgroup.Domain.Id, TraderReferenceType.Invoice);
                    }

                    uInvoice.DueDate = invoice.DueDate;
                    uInvoice.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(invoice.Workgroup.Id);
                    uInvoice.Status = invoice.Status;
                    uInvoice.InvoiceAddress = invoice.InvoiceAddress?.TrimEnd();
                    uInvoice.PaymentDetails = invoice.PaymentDetails?.TrimEnd();
                    uInvoice.TotalInvoiceAmount = invoice.TotalInvoiceAmount;
                    uInvoice.InvoiceItems = new List<InvoiceTransactionItems>();
                    uInvoice.Purchase = null;

                    uInvoice.InvoiceItems.Clear();
                    foreach (var item in invoice.InvoiceItems)
                        uInvoice.InvoiceItems.Add(new InvoiceTransactionItems
                        {
                            InvoiceValue = item.InvoiceValue,
                            InvoiceDiscountValue = item.InvoiceDiscountValue,
                            InvoiceItemQuantity = item.InvoiceItemQuantity,
                            InvoiceTaxValue = item.InvoiceTaxValue,
                            TransactionItem = dbContext.TraderSaleItems.Find(item.TransactionItem?.Id)
                        });

                    uInvoice.Payments.Clear();

                    if (dbContext.Entry(uInvoice).State == EntityState.Detached) dbContext.Invoices.Attach(uInvoice);

                    dbContext.Entry(uInvoice).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    refModel.actionVal = 2;
                    invoiceId = uInvoice.Id;
                }

                var tradInvoice = dbContext.Invoices.Find(invoiceId);

                if (tradInvoice.InvoiceApprovalProcess != null) return refModel;

                if (tradInvoice.Status != TraderInvoiceStatusEnum.PendingReview)
                    return refModel;

                tradInvoice.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;
                var refFull = tradInvoice.Reference == null ? "" : tradInvoice.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = dbContext.InvoiceApprovalDefinitions.FirstOrDefault(w => w.WorkGroup.Id == tradInvoice.Workgroup.Id),
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = tradInvoice.Workgroup.Qbicle,
                    Topic = tradInvoice.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Trader Approval for Invoice #{refFull}",
                    Invoice = new List<Invoice> { tradInvoice }
                };
                tradInvoice.InvoiceApprovalProcess = approval;
                approval.ActivityMembers.AddRange(tradInvoice.Workgroup.Members);
                if (dbContext.Entry(tradInvoice).State == EntityState.Detached) dbContext.Invoices.Attach(tradInvoice);

                dbContext.Entry(tradInvoice).State = EntityState.Modified;
                dbContext.SaveChanges();

                CreateInvoiceProcessLog(tradInvoice, approval);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, invoice);
                refModel.result = false;
                refModel.msg = e.Message;
                refModel.actionVal = 3;
            }
            return refModel;
        }

        public ReturnJsonModel SaveBillInvoice(Invoice invoice, string userId, string originatingConnectionId = "")
        {
            var refModel = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, userId, null, invoice);

                if (invoice.Reference != null)
                    invoice.Reference = new TraderReferenceRules(dbContext).GetById(invoice.Reference.Id);

                var s3Rules = new Azure.AzureStorageRules(dbContext);
                if (invoice.Id == 0)
                {
                    s3Rules.ProcessingMediaS3(invoice.InvoicePDF);
                }
                else
                {
                    var mediaValid = GetById(invoice.Id);
                    if (mediaValid.InvoicePDF != invoice.InvoicePDF)
                        s3Rules.ProcessingMediaS3(invoice.InvoicePDF);
                }
                if (invoice.InvoiceItems != null && invoice.InvoiceItems.Count > 0)
                    invoice.TotalInvoiceAmount = invoice.InvoiceItems.Sum(e => e.InvoiceValue);

                var user = dbContext.QbicleUser.Find(userId);

                int invoiceId;
                var purchase = new TraderPurchaseRules(dbContext).GetById(invoice.Purchase.Id);
                if (invoice.Id == 0)
                {
                    var sInvoice = new Invoice
                    {
                        CreatedBy = user,
                        CreatedDate = DateTime.UtcNow,
                        DueDate = invoice.DueDate,
                        Reference = invoice.Reference,
                        PaymentDetails = invoice.PaymentDetails,
                        Workgroup = new TraderWorkGroupsRules(dbContext).GetById(invoice.Workgroup.Id),
                        Purchase = purchase,
                        Status = invoice.Status,
                        TotalInvoiceAmount = invoice.TotalInvoiceAmount,
                        InvoicePDF = invoice.InvoicePDF,
                        InvoiceItems = new List<InvoiceTransactionItems>(),
                        Sale = null
                    };
                    foreach (var item in invoice.InvoiceItems)
                        sInvoice.InvoiceItems.Add(new InvoiceTransactionItems
                        {
                            InvoiceValue = item.InvoiceValue,
                            InvoiceDiscountValue = item.InvoiceDiscountValue,
                            InvoiceItemQuantity = item.InvoiceItemQuantity,
                            InvoiceTaxValue = item.InvoiceTaxValue,
                            TransactionItem = dbContext.TraderSaleItems.Find(item.TransactionItem?.Id)
                        });

                    dbContext.Invoices.Add(sInvoice);
                    dbContext.Entry(sInvoice).State = EntityState.Added;
                    dbContext.SaveChanges();

                    invoiceId = sInvoice.Id;
                    refModel.actionVal = 1;
                }
                else
                {
                    var uInvoice = GetById(invoice.Id);
                    uInvoice.DueDate = invoice.DueDate;
                    if (invoice.Reference != null)
                    {
                        uInvoice.Reference = invoice.Reference;
                    }

                    uInvoice.Workgroup = new TraderWorkGroupsRules(dbContext).GetById(invoice.Workgroup.Id);
                    uInvoice.Status = invoice.Status;
                    uInvoice.PaymentDetails = invoice.PaymentDetails?.TrimEnd();
                    uInvoice.TotalInvoiceAmount = invoice.TotalInvoiceAmount;
                    uInvoice.InvoiceItems = new List<InvoiceTransactionItems>();
                    uInvoice.Purchase = purchase;
                    uInvoice.InvoicePDF = invoice.InvoicePDF;
                    uInvoice.Sale = null;

                    uInvoice.InvoiceItems.Clear();
                    foreach (var item in invoice.InvoiceItems)
                        uInvoice.InvoiceItems.Add(new InvoiceTransactionItems
                        {
                            InvoiceValue = item.InvoiceValue,
                            InvoiceDiscountValue = item.InvoiceDiscountValue,
                            InvoiceItemQuantity = item.InvoiceItemQuantity,
                            InvoiceTaxValue = item.InvoiceTaxValue,
                            TransactionItem = dbContext.TraderSaleItems.Find(item.TransactionItem?.Id)
                        });

                    if (dbContext.Entry(uInvoice).State == EntityState.Detached) dbContext.Invoices.Attach(uInvoice);

                    dbContext.Entry(uInvoice).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    refModel.actionVal = 2;
                    invoiceId = uInvoice.Id;
                }

                refModel.msgId = invoiceId.ToString();

                var traderBill = dbContext.Invoices.Find(invoiceId);

                if (traderBill?.InvoiceApprovalProcess != null) return refModel;

                if (traderBill == null || traderBill.Status != TraderInvoiceStatusEnum.PendingReview) return refModel;

                traderBill.Workgroup.Qbicle.LastUpdated = DateTime.UtcNow;

                var appDef = dbContext.InvoiceApprovalDefinitions.FirstOrDefault(w =>
                    w.WorkGroup.Id == traderBill.Workgroup.Id);
                var refFull = traderBill.Reference == null ? "" : traderBill.Reference.FullRef;
                var approval = new ApprovalReq
                {
                    ApprovalRequestDefinition = appDef,
                    ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequest,
                    Priority = ApprovalReq.ApprovalPriorityEnum.High,
                    RequestStatus = ApprovalReq.RequestStatusEnum.Pending,
                    Qbicle = traderBill.Workgroup.Qbicle,
                    Topic = traderBill.Workgroup.Topic,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Notes = "",
                    IsVisibleInQbicleDashboard = true,
                    App = QbicleActivity.ActivityApp.Trader,
                    Name = $"Trader Approval for Bill #{refFull}",
                    Invoice = new List<Invoice> { traderBill }
                };
                approval.ActivityMembers.AddRange(traderBill.Workgroup.Members);
                dbContext.ApprovalReqs.Add(approval);
                dbContext.Entry(approval).State = EntityState.Added;
                traderBill.InvoiceApprovalProcess = approval;
                if (dbContext.Entry(traderBill).State == EntityState.Detached) dbContext.Invoices.Attach(traderBill);

                dbContext.Entry(traderBill).State = EntityState.Modified;
                dbContext.SaveChanges();

                CreateInvoiceProcessLog(traderBill, approval);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = HelperClass.GetFullNameOfUser(approval.StartedBy),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).Notification2Activity(activityNotification);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, userId, invoice);
                refModel.result = false;
                refModel.msg = e.Message;
                refModel.actionVal = 3;
            }

            return refModel;
        }

        private void CreateInvoiceProcessLog(Invoice tradInvoice, ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradInvoice, approval);
                var invoiceLog = new InvoiceLog
                {
                    CreatedBy = approval.StartedBy,
                    CreatedDate = DateTime.UtcNow,
                    Workgroup = tradInvoice.Workgroup,
                    Status = tradInvoice.Status,
                    Sale = tradInvoice.Sale,
                    Purchase = tradInvoice.Purchase,
                    AssociatedFiles = tradInvoice.AssociatedFiles,
                    DueDate = tradInvoice.DueDate,
                    Id = 0,
                    InvoiceAddress = tradInvoice.InvoiceAddress,
                    InvoiceApprovalProcess = tradInvoice.InvoiceApprovalProcess,
                    InvoiceItems = tradInvoice.InvoiceItems,
                    InvoicePDF = tradInvoice.InvoicePDF,
                    ParentInvoice = tradInvoice,
                    PaymentDetails = tradInvoice.PaymentDetails,
                    Payments = tradInvoice.Payments,
                    TotalInvoiceAmount = tradInvoice.TotalInvoiceAmount
                };
                var invoiceProcessLog = new InvoiceProcessLog()
                {
                    CreatedBy = invoiceLog.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = invoiceLog.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    },
                    AssociatedInvoice = tradInvoice,
                    AssociatedInvoiceLog = invoiceLog,
                    InvoiceStatus = tradInvoice.Status
                };

                dbContext.InvoiceProcessLogs.Add(invoiceProcessLog);
                dbContext.Entry(invoiceProcessLog).State = EntityState.Added;

                dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, tradInvoice, approval);
            }
        }

        public ReturnJsonModel UpdateInvoiceReview(Invoice invoice)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, invoice);
                var inv = GetById(invoice.Id);
                inv.DueDate = invoice.DueDate;
                inv.InvoiceAddress = invoice.InvoiceAddress.TrimEnd();
                inv.PaymentDetails = invoice.PaymentDetails.TrimEnd();
                dbContext.Entry(inv).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                refModel.result = false;
                refModel.msg = e.Message;
                refModel.actionVal = 3;
            }

            return refModel;
        }

        public ReturnJsonModel IssueInvoice(int id, string invoiceGuid)
        {
            var refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, invoiceGuid);
                var inv = GetById(id);
                inv.InvoicePDF = invoiceGuid;
                inv.Status = TraderInvoiceStatusEnum.InvoiceIssued;
                dbContext.Entry(inv).State = EntityState.Modified;
                dbContext.SaveChanges();
                refModel.result = true;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, id, invoiceGuid);
                refModel.result = false;
                refModel.msg = e.Message;
                refModel.actionVal = 3;
            }

            return refModel;
        }

        public void InvoiceApproval(ApprovalReq approval)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, approval);
                var tradInvoice = approval.Invoice.FirstOrDefault();
                if (tradInvoice == null) return;

                switch (approval.RequestStatus)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        tradInvoice.Status = TraderInvoiceStatusEnum.PendingReview;
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        tradInvoice.Status = TraderInvoiceStatusEnum.PendingApproval;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        tradInvoice.Status = TraderInvoiceStatusEnum.InvoiceApproved;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                        tradInvoice.Status = TraderInvoiceStatusEnum.InvoiceDenied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Discarded:
                        tradInvoice.Status = TraderInvoiceStatusEnum.InvoiceDiscarded;
                        break;
                }

                dbContext.Entry(tradInvoice).State = EntityState.Modified;
                dbContext.SaveChanges();

                var invoiceLog = new InvoiceLog
                {
                    CreatedBy = approval.ApprovedOrDeniedAppBy,
                    CreatedDate = DateTime.UtcNow,
                    Workgroup = tradInvoice.Workgroup,
                    Status = tradInvoice.Status,
                    Sale = tradInvoice.Sale,
                    Purchase = tradInvoice.Purchase,
                    AssociatedFiles = tradInvoice.AssociatedFiles,
                    DueDate = tradInvoice.DueDate,
                    Id = 0,
                    InvoiceAddress = tradInvoice.InvoiceAddress,
                    InvoiceApprovalProcess = tradInvoice.InvoiceApprovalProcess,
                    InvoiceItems = tradInvoice.InvoiceItems,
                    InvoicePDF = tradInvoice.InvoicePDF,
                    ParentInvoice = tradInvoice,
                    PaymentDetails = tradInvoice.PaymentDetails,
                    Payments = tradInvoice.Payments,
                    TotalInvoiceAmount = tradInvoice.TotalInvoiceAmount
                };

                var invoiceProcessLog = new InvoiceProcessLog()
                {
                    CreatedBy = invoiceLog.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    ApprovalReqHistory = new ApprovalReqHistory
                    {
                        ApprovalReq = approval,
                        UpdatedBy = invoiceLog.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        RequestStatus = approval.RequestStatus
                    },
                    AssociatedInvoice = tradInvoice,
                    AssociatedInvoiceLog = invoiceLog,
                    InvoiceStatus = tradInvoice.Status
                };

                dbContext.InvoiceProcessLogs.Add(invoiceProcessLog);
                dbContext.Entry(invoiceProcessLog).State = EntityState.Added;

                dbContext.SaveChanges();

                if (approval.RequestStatus != ApprovalReq.RequestStatusEnum.Approved)
                    return;

                if (tradInvoice.Sale != null)
                    new BookkeepingIntegrationRules(dbContext).AddSaleInvoiceJournalEntry(approval.ApprovedOrDeniedAppBy, tradInvoice);

                if (tradInvoice.Purchase != null)
                    new BookkeepingIntegrationRules(dbContext).AddPurchaseNonInventoryJournalEntry(approval.ApprovedOrDeniedAppBy, tradInvoice);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null, approval);
            }
        }

        public DataTablesResponse GetInvoicesByContact(int contactId, string timeZone, IDataTablesRequest requestModel, string type, string dateTimeFormat, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, contactId, timeZone, requestModel, type, dateTimeFormat, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                IQueryable<InvoiceCustome> query;
                if (type == "Sale")
                {
                    query = from ts in dbContext.TraderSales
                            join iv in dbContext.Invoices on ts.Id equals iv.Sale.Id
                            where ts.Purchaser.Id == contactId
                            select new InvoiceCustome
                            {
                                Id = iv.Id,
                                CreateDate = iv.CreatedDate,
                                Amount = iv.TotalInvoiceAmount,
                                Type = "Sale",
                                Status = iv.Status,
                                Ref = iv.Reference != null ? iv.Reference.FullRef : ""
                            };
                }
                else
                {
                    query = from ts in dbContext.TraderPurchases
                            join iv in dbContext.Invoices on ts.Id equals iv.Purchase.Id
                            where ts.Vendor.Id == contactId
                            select new InvoiceCustome
                            {
                                Id = iv.Id,
                                CreateDate = iv.CreatedDate,
                                Amount = iv.TotalInvoiceAmount,
                                Type = "Purchase",
                                Status = iv.Status,
                                Ref = iv.Reference != null ? iv.Reference.FullRef : ""
                            };
                }

                int totalSpot;

                #region Filter

                var keyword = requestModel.Search != null ? requestModel.Search.Value : "";
                if (!string.IsNullOrEmpty(keyword))
                {
                    //Check keyword is date and convert keyword date to DateTime.UTC
                    DateTime date = DateTime.UtcNow;
                    bool isDate = false;
                    Regex rx = new Regex("^([0]?[0-9]|[12][0-9]|[3][01])[./-]([0]?[0-9]|[12][0-9]|[3][01])[./-]([0-9]{4}|[0-9]{2})$");
                    var match = rx.Matches(keyword);
                    if (match.Count > 0)
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                        try
                        {
                            isDate = true;
                            date = keyword.ConvertDateFormat(dateTimeFormat).ConvertTimeToUtc(tz);
                        }
                        catch
                        {
                            date = DateTime.UtcNow;
                        }
                    }
                    //End
                    int _id;
                    var endate = date.AddDays(1);
                    int.TryParse(keyword.TrimStart('0'), out _id);
                    query = query.Where(q =>
                       q.Id == _id
                        || (isDate && q.CreateDate >= date && q.CreateDate < endate)
                    );
                }
                totalSpot = query.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Id" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreateDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Amount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Key = q.Id.Encrypt(),
                    Ref = q.Ref,
                    Date = q.CreateDate.ConvertTimeFromUtc(timeZone).ToString(dateTimeFormat),
                    Amount = q.Amount.ToDecimalPlace(currencySettings),
                    q.Type,
                    q.Status
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalSpot, totalSpot);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactId, timeZone, requestModel, type, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public List<ApprovalStatusTimeline> InvoiceBillApprovalStatusTimeline(int id, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, id, timeZone);
                var timeline = new List<ApprovalStatusTimeline>();
                var logs = dbContext.InvoiceProcessLogs.Where(e => e.AssociatedInvoice.Id == id)
                    .OrderByDescending(d => d.CreatedDate).ToList();
                string icon = "";

                foreach (var log in logs)
                {
                    switch (log.InvoiceStatus)
                    {
                        case TraderInvoiceStatusEnum.PendingReview:
                            icon = "fa fa-info bg-aqua";
                            break;

                        case TraderInvoiceStatusEnum.PendingApproval:
                            icon = "fa fa-truck bg-yellow";
                            break;

                        case TraderInvoiceStatusEnum.InvoiceApproved:
                            icon = "fa fa-check bg-green";
                            break;

                        case TraderInvoiceStatusEnum.Draft:
                            icon = "fa fa-warning bg-yellow";
                            break;

                        case TraderInvoiceStatusEnum.InvoiceDenied:
                            icon = "fa fa-warning bg-red";
                            break;

                        case TraderInvoiceStatusEnum.InvoiceDiscarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }

                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timeZone).ToShortTimeString(),
                            Status = log.InvoiceStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.CreatedBy.ProfilePic
                        }
                    );
                }
                return timeline;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<ApprovalStatusTimeline>();
            }
        }

        public byte[] ReportSaleInvoice(Invoice iv, string imageTop, string imageBottom, string timezone, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, iv, imageTop, imageBottom, timezone, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);

                #region Bind data Report

                var sale = iv.Sale;
                //product items
                var lst = new List<ReportProductItem>();
                decimal totalValue = 0;
                decimal totalTax = 0;
                if (iv.InvoiceItems != null && iv.InvoiceItems.Any())
                    foreach (var item in iv.InvoiceItems)
                    {
                        var productItem = new ReportProductItem
                        {
                            Item = item.TransactionItem?.TraderItem.Name,
                            Unit = item.TransactionItem?.Unit?.Name,
                            Quantity = item.InvoiceItemQuantity.ToString("N0"),
                            Discount = item.InvoiceDiscountValue.ToDecimalPlace(currencySettings) + currencySettings.CurrencySymbol
                        };

                        string taxrateName = "(Tax free)";
                        if (item.TransactionItem.Taxes != null)
                        {
                            var priceIvi = item.TransactionItem.SalePricePerUnit * item.InvoiceItemQuantity - item.InvoiceDiscountValue;
                            taxrateName = priceIvi.InvoiceTaxRatesHtml(item.TransactionItem.Taxes, currencySettings, false);
                        }
                        productItem.Tax = taxrateName;
                        productItem.Total = item.InvoiceValue.ToCurrencySymbol(currencySettings);
                        lst.Add(productItem);
                        totalTax = totalTax + (item.InvoiceTaxValue.HasValue ? item.InvoiceTaxValue.Value : 0) * (item.InvoiceItemQuantity);
                        totalValue = totalValue + item.InvoiceValue;
                    }
                //Order info
                var addressBilling = sale?.Purchaser?.Address;
                var lsOrderInfo = new List<ReportOrderInfo>();
                var orderInfo = new ReportOrderInfo
                {
                    FullRef = iv.Reference?.FullRef,
                    AdditionalInformation = iv.PaymentDetails,
                    OrderDate = iv.DueDate.ConvertTimeFromUtc(timezone).ToString("dd MM, yyyy"),
                    InvoiceAddress = iv.InvoiceAddress,
                    PurchaserName = sale?.Purchaser?.Name
                };
                if (addressBilling != null)
                {
                    if (!string.IsNullOrEmpty(addressBilling.AddressLine1))
                        orderInfo.BillingAddressLine = addressBilling.AddressLine1 + Environment.NewLine;
                    if (!string.IsNullOrEmpty(addressBilling.AddressLine2))
                        orderInfo.BillingAddressLine += (addressBilling.AddressLine2 + Environment.NewLine);
                    if (!string.IsNullOrEmpty(addressBilling.City))
                        orderInfo.BillingAddressLine += (addressBilling.City + Environment.NewLine);
                    if (!string.IsNullOrEmpty(addressBilling.State))
                        orderInfo.BillingAddressLine += (addressBilling.State + Environment.NewLine);
                    if (!string.IsNullOrEmpty(addressBilling.Country.ToString()))
                        orderInfo.BillingAddressLine += (addressBilling.Country + Environment.NewLine);
                }
                else if (iv.Purchase != null && iv.Purchase.Vendor != null)
                {
                    if (!string.IsNullOrEmpty(iv.Purchase.Vendor?.Address?.AddressLine1))
                        orderInfo.BillingAddressLine = iv.Purchase.Vendor?.Address?.AddressLine1 + Environment.NewLine;
                    if (!string.IsNullOrEmpty(iv.Purchase.Vendor?.Address?.AddressLine2))
                        orderInfo.BillingAddressLine += (iv.Purchase.Vendor?.Address?.AddressLine2 + Environment.NewLine);
                    if (!string.IsNullOrEmpty(iv.Purchase.Vendor?.Address?.City))
                        orderInfo.BillingAddressLine += (iv.Purchase.Vendor?.Address?.City + Environment.NewLine);
                    if (!string.IsNullOrEmpty(iv.Purchase.Vendor?.Address?.State))
                        orderInfo.BillingAddressLine += (iv.Purchase.Vendor?.Address?.State + Environment.NewLine);
                    if (!string.IsNullOrEmpty(iv.Purchase.Vendor?.Address?.Country.ToString()))
                        orderInfo.BillingAddressLine += (iv.Purchase.Vendor?.Address?.Country + Environment.NewLine);
                }
                orderInfo.SalesTax = totalTax.ToCurrencySymbol(currencySettings);
                orderInfo.Total = totalValue.ToCurrencySymbol(currencySettings);
                orderInfo.Subtotal = (totalValue - totalTax).ToCurrencySymbol(currencySettings);
                orderInfo.ImageTop = imageTop;
                orderInfo.ImageBottom = imageBottom;
                lsOrderInfo.Add(orderInfo);

                var dataSource = new List<ReportDataSource>
                {
                    new ReportDataSource {Name = "ProductItems", Value = lst},
                    new ReportDataSource {Name = "OrderInfo", Value = lsOrderInfo}
                };

                return ReportRules.RenderReport(dataSource, ReportFileName.Invoice);

                #endregion Bind data Report
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.InnerException.Message, ex.InnerException.InnerException.Message);
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                return null;
            }
        }

        public DataTablesResponse GetReportInvoices(IDataTablesRequest requestModel, string keyword, int contactId, string datetime, UserSetting dateTimeFormat, int domainId, string currentUserId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, contactId, keyword, datetime, dateTimeFormat, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                var query = dbContext.Invoices.Where(s => s.Workgroup.Domain.Id == domainId && (s.Sale != null || s.Purchase != null));
                int totalrecords;

                #region Filter

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(s => s.Reference.FullRef.Contains(keyword) || s.Sale.Purchaser.Name.Contains(keyword) || s.Purchase.Vendor.Name.Contains(keyword));
                }
                if (contactId > 0)
                {
                    query = query.Where(s => (s.Sale != null && s.Sale.Purchaser.Id == contactId));
                }
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(dateTimeFormat.Timezone);
                    datetime.ConvertDaterangeFormat(dateTimeFormat.DateTimeFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.minute);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                totalrecords = query.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Sale.Purchaser.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TotalInvoiceAmount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Key = q.Key,
                    Ref = q.Reference?.FullRef,
                    Contact = q.Sale?.Purchaser?.Name ?? q.Purchase?.Vendor?.Name?? "",
                    ContactId = q.Sale?.Purchaser?.Id?? q.Purchase?.Vendor?.Id??0,
                    Date = q.CreatedDate.ConvertTimeFromUtc(dateTimeFormat.Timezone).ToString(dateTimeFormat.DateFormat),
                    Amount = q.TotalInvoiceAmount.ToDecimalPlace(currencySettings),
                    q.Status,
                    AllowEdit = (q.Workgroup != null
                        && q.Workgroup.Processes.Any(s => s.Name.Equals(TraderProcessName.TraderPaymentProcessName))
                        && q.Workgroup.Members.Any(s => s.Id == currentUserId)
                        ),
                    IsBill = q.Purchase != null
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, contactId, keyword, datetime, dateTimeFormat, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public DataTablesResponse GetInvoicesInOutByLocation(IDataTablesRequest requestModel, bool isIN, string keyword, int locationId, string datetime, UserSetting userSetting, int domainId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, requestModel, locationId, keyword, datetime, userSetting, domainId);
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(domainId);
                IQueryable<Invoice> query = null;
                if (isIN)
                    query = dbContext.Invoices.Where(s => s.Sale != null && s.Sale.Location.Id == locationId);
                else
                    query = dbContext.Invoices.Where(s => s.Purchase != null && s.Purchase.Location.Id == locationId);

                int totalrecords;

                #region Filter

                if (!string.IsNullOrEmpty(keyword))
                {
                    if (isIN)
                        query = query.Where(s => s.Reference.FullRef.Contains(keyword) || s.Sale.Purchaser.Name.Contains(keyword));
                    else
                        query = query.Where(s => s.Reference.FullRef.Contains(keyword) || s.Purchase.Vendor.Name.Contains(keyword));
                }
                if (!string.IsNullOrEmpty(datetime.Trim()))
                {
                    var startDate = new DateTime();
                    var endDate = new DateTime();
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                    datetime.ConvertDaterangeFormat(userSetting.DateFormat, "", out startDate, out endDate, HelperClass.endDateAddedType.day);
                    startDate = startDate.ConvertTimeToUtc(tz);
                    endDate = endDate.ConvertTimeToUtc(tz);
                    query = query.Where(s => s.CreatedDate >= startDate && s.CreatedDate < endDate);
                }

                totalrecords = query.Count();

                #endregion Filter

                #region Sorting

                var sortedColumns = requestModel.Columns.GetSortedColumns();
                var orderByString = string.Empty;
                foreach (var column in sortedColumns)
                {
                    switch (column.Data)
                    {
                        case "Ref":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Reference.FullRef" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Contact":
                            orderByString += orderByString != string.Empty ? "," : "";
                            if (isIN)
                                orderByString += "Sale.Purchaser.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            else
                                orderByString += "Sale.Vendor.Name" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Date":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "CreatedDate" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Amount":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "TotalInvoiceAmount" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        case "Status":
                            orderByString += orderByString != string.Empty ? "," : "";
                            orderByString += "Status" + (column.SortDirection == TB_Column.OrderDirection.Ascendant ? " asc" : " desc");
                            break;

                        default:
                            orderByString = "CreatedDate asc";
                            break;
                    }
                }

                query = query.OrderBy(orderByString == string.Empty ? "CreatedDate desc" : orderByString);

                #endregion Sorting

                #region Paging

                var list = query.Skip(requestModel.Start).Take(requestModel.Length).ToList();

                #endregion Paging

                var dataJson = list.Select(q => new
                {
                    q.Id,
                    Ref = q.Reference?.FullRef,
                    Contact = isIN ? (q.Sale.Purchaser?.Name ?? "") : (q.Purchase.Vendor?.Name ?? ""),
                    ContactKey = isIN ? (q.Sale.Purchaser?.Key ?? "") : (q.Purchase.Vendor?.Key ?? ""),
                    Date = q.CreatedDate.ConvertTimeFromUtc(userSetting.Timezone).ToString(userSetting.DateFormat),
                    Amount = q.TotalInvoiceAmount.ToDecimalPlace(currencySettings),
                    q.Status,
                    Payments = q.Payments.Count()
                }).ToList();
                return new DataTablesResponse(requestModel.Draw, dataJson, totalrecords, totalrecords);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, requestModel, locationId, keyword, datetime, userSetting, domainId);
                return new DataTablesResponse(requestModel.Draw, null, 0, 0);
            }
        }

        public List<InvoiceItemModel> GetInvoiceItemsFromPurchase(int purchaseId, UserSetting userSetting)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, purchaseId);
                var purchase = dbContext.TraderPurchases.Find(purchaseId);
                if (purchase == null)
                    return new List<InvoiceItemModel>();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(purchase.Workgroup.Domain.Id);
                List<InvoiceItemModel> invoiceItems = new List<InvoiceItemModel>();
                var existed_bill = dbContext.Invoices.Where(p => p.Purchase.Id == purchaseId && p.Status == TraderInvoiceStatusEnum.InvoiceApproved).ToList();
                foreach (var item in purchase.PurchaseItems)
                {
                    InvoiceItemModel invoiceItem = new InvoiceItemModel();
                    invoiceItem.Id = 0;
                    invoiceItem.InvoiceQuantity = 0;
                    var quantity_used = existed_bill.Select(p => p.InvoiceItems.FirstOrDefault(x => x.TransactionItem.Id == item.Id)).Sum(c => c.InvoiceItemQuantity);
                    if (quantity_used < item.Quantity)
                    {
                        invoiceItem.InvoiceQuantity = item.Quantity - quantity_used;
                    }
                    invoiceItem.InvoiceDiscount = 0;
                    invoiceItem.InvoiceCost = 0;
                    invoiceItem.InvoiceChecked = true;
                    invoiceItem.InvoiceTaxes = 0;
                    invoiceItem.TransItemId = item.Id;
                    invoiceItem.TransItemUri = ConfigManager.ApiGetDocumentUri + item.TraderItem.ImageUri + "&size=T";
                    invoiceItem.TransItemName = item.TraderItem.Name;
                    invoiceItem.TransItemQuantity = item.Quantity;
                    invoiceItem.TransItemUnitName = item.Unit.Name;
                    invoiceItem.TransItemDiscount = item.Discount;
                    invoiceItem.TransItemSumValTaxRates = item.SumTaxRates();
                    invoiceItem.TransItemHtmlTaxRates = item.HtmlTaxRates(currencySettings, false);
                    invoiceItem.TransItemCost = item.Cost;
                    invoiceItem.PricePerUnit = item.CostPerUnit;
                    //invoiceItem.InvoiceTaxesInfo = string.Join(",", item.Taxes.Select(s => $"{(s.StaticTaxRate?.Rate ?? 0):N0}-{s.StaticTaxRate?.Name ?? ""}"));
                    invoiceItems.Add(invoiceItem);
                }
                return invoiceItems;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<InvoiceItemModel>();
            }
        }

        public List<InvoiceItemModel> GetInvoiceItemsFromSale(int purchaseId, UserSetting userSetting, int invoiceId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, purchaseId);
                var sale = dbContext.TraderSales.Find(purchaseId);
                if (sale == null)
                    return new List<InvoiceItemModel>();
                var currencySettings = new CurrencySettingRules(dbContext).GetCurrencySettingByDomain(sale.Workgroup.Domain.Id);
                Invoice invoice = null;
                List<InvoiceTransactionItems> ivTranstems = new List<InvoiceTransactionItems>();
                if (invoiceId > 0)
                {
                    invoice = dbContext.Invoices.Find(invoiceId);
                    ivTranstems = invoice.InvoiceItems;
                }
                List<InvoiceItemModel> invoiceItems = new List<InvoiceItemModel>();
                foreach (var item in sale.SaleItems)
                {
                    InvoiceItemModel invoiceItem = new InvoiceItemModel();
                    if (invoiceId == 0)
                    {
                        invoiceItem.Id = 0;
                        invoiceItem.InvoiceQuantity = 0;
                        invoiceItem.InvoiceDiscount = 0;
                        invoiceItem.InvoiceCost = 0;
                        invoiceItem.InvoiceChecked = true;
                        invoiceItem.InvoiceTaxes = 0;
                    }
                    else
                    {
                        var ivTransItem = ivTranstems.FirstOrDefault(s => s.TransactionItem.Id == item.Id);
                        invoiceItem.Id = ivTransItem?.Id ?? 0;
                        invoiceItem.InvoiceQuantity = ivTransItem?.InvoiceItemQuantity ?? 0;
                        invoiceItem.InvoiceDiscount = ivTransItem?.InvoiceDiscountValue ?? 0;
                        invoiceItem.InvoiceCost = ivTransItem?.InvoiceValue ?? 0;
                        invoiceItem.InvoiceChecked = ivTransItem != null ? true : false;
                        invoiceItem.InvoiceTaxes = (ivTransItem?.InvoiceTaxValue ?? 0) * (ivTransItem?.InvoiceItemQuantity ?? 0);
                    }

                    invoiceItem.TransItemId = item.Id;
                    invoiceItem.TransItemUri = ConfigManager.ApiGetDocumentUri + item.TraderItem.ImageUri + "&size=T";
                    invoiceItem.TransItemName = item.TraderItem.Name;
                    invoiceItem.TransItemQuantity = item.Quantity;
                    invoiceItem.TransItemUnitName = item.Unit.Name;
                    invoiceItem.TransItemDiscount = item.Discount;
                    invoiceItem.TransItemSumValTaxRates = item.SumTaxRates();
                    invoiceItem.TransItemHtmlTaxRates = item.HtmlTaxRates(currencySettings);
                    invoiceItem.TransItemCost = item.Price;
                    invoiceItem.PricePerUnit = item.SalePricePerUnit;
                    //invoiceItem.InvoiceTaxesInfo = string.Join(",", item.Taxes.Select(s => $"{(s.StaticTaxRate?.Rate ?? 0):N0}-{s.StaticTaxRate?.Name ?? ""}"));
                    invoiceItems.Add(invoiceItem);
                }
                return invoiceItems;
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e, null);
                return new List<InvoiceItemModel>();
            }
        }
    }
}