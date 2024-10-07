using Newtonsoft.Json;
using Qbicles.BusinessRules.BusinessRules.Spannered;
using Qbicles.BusinessRules.BusinessRules.TradeOrderLogging;
using Qbicles.BusinessRules.BusinessRules.Trader;
using Qbicles.BusinessRules.CMs;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class ApprovalsRules
    {
        private readonly ApplicationDbContext dbContext;

        public ApprovalsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public IEnumerable<ApprovalDocument> GetApprovalDocumentByApprovalId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval document by approval id", null, null, id);

                return dbContext.ApprovalDocument.Where(a => a.Approval.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new List<ApprovalDocument>();
            }
        }

        public List<AttachmentModel> GetAttachmentsTypeByApprovalId(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get attachments type by approval id", null, null, id);

                return HelperClass.MapAttachments(GetApprovalDocumentByApprovalId(id));
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new List<AttachmentModel>();
            }
        }

        public ApprovalReq SaveApproval(int approvalRequestDefinitionId, ApprovalReq approval, int qbicleId,
            string linkApproval, ApplicationUser currentUser, string topicName, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save approval", currentUser.Id, null, approvalRequestDefinitionId, approval, qbicleId, linkApproval, currentUser, topicName);

                var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                var approvalRequestDefinition =
                    new ApprovalAppsRules(dbContext).GetApprovalAppById(approvalRequestDefinitionId);
                approval.StartedBy = currentUser;
                approval.StartedDate = DateTime.UtcNow.AddMinutes(1);
                approval.State = QbicleActivity.ActivityStateEnum.Open;
                qbicle.LastUpdated = DateTime.UtcNow;
                approval.Qbicle = qbicle;
                approval.ActivityType = QbicleActivity.ActivityTypeEnum.ApprovalRequestApp;
                approval.TimeLineDate = DateTime.UtcNow;
                approval.ApprovalRequestDefinition = approvalRequestDefinition;
                approval.Name = approvalRequestDefinition.Title;
                if (!string.IsNullOrEmpty(linkApproval))
                {
                    var userRules = new UserRules(dbContext);
                    var linkApprovals = JsonConvert.DeserializeObject<List<string>>(linkApproval);
                    foreach (var member in linkApprovals) approval.ActivityMembers.Add(userRules.GetUser(member, 0));
                }

                approval.ActivityMembers.Add(currentUser); //user create
                var tRule = new TopicRules(dbContext);
                var topic = tRule.GetTopicByName(topicName, qbicleId);
                if (topic != null)
                {
                    approval.Topic = topic;
                }
                else
                {
                    var topicNew = tRule.SaveTopic(qbicleId, topicName);
                    approval.Topic = topicNew;
                }

                dbContext.ApprovalReqs.Add(approval);
                dbContext.Entry(approval).State = EntityState.Added;

                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approval.Id,
                    EventNotify = NotificationEventEnum.ApprovalCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = currentUser.Id,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);

                return approval;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUser.Id, approvalRequestDefinitionId, approval, qbicleId, linkApproval, currentUser, topicName);
                return null;
            }
        }

        public List<ApprovalReq> GetApprovalsOrderByQbicleId(int cubeId, Enums.OrderByDate orderBy, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approvals order by qbicle id", null, null, cubeId, orderBy, topicId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        if (topicId == 0)
                            return dbContext.ApprovalReqs.Where(c => c.Qbicle.Id == cubeId)
                                .OrderByDescending(d => d.TimeLineDate).ToList();
                        return dbContext.ApprovalReqs.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId)
                            .OrderByDescending(d => d.TimeLineDate).ToList();

                    case Enums.OrderByDate.OldestFirst:
                        if (topicId == 0)
                            return dbContext.ApprovalReqs.Where(c => c.Qbicle.Id == cubeId).OrderBy(d => d.TimeLineDate)
                                .ToList();
                        return dbContext.ApprovalReqs.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId)
                            .OrderBy(d => d.TimeLineDate).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, topicId);
            }

            return new List<ApprovalReq>();
        }

        public List<ApprovalReq> GetApprovalsByQbicleId(int cubeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approvals by qbicle id", null, null, cubeId);

                var approvals = dbContext.ApprovalReqs.Where(c => c.Qbicle.Id == cubeId).ToList();
                return approvals;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId);
                return new List<ApprovalReq>();
            }
        }

        public IEnumerable<DateTime> GetApprovalsDate(List<ApprovalReq> qbicleApprovals)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approvals date", null, null, qbicleApprovals);

                var approvalDates = from t in qbicleApprovals select t.TimeLineDate.Date;
                return approvalDates.Distinct();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleApprovals);
                return new List<DateTime>();
            }
        }

        public IEnumerable<DateTime> LoadMoreApprovals(int cubeId, int size,
            ref List<ApprovalReq> approvals, ref int activitiesDateCount, string currentTimeZone)
        {
            IEnumerable<DateTime> activitiesDate = null;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more approval", null, null, cubeId, size, approvals, activitiesDateCount, currentTimeZone);

                var qApproval = GetApprovalsByQbicleId(cubeId).BusinessMapping(currentTimeZone);

                var approvalDates = from d in qApproval select d.TimeLineDate.Date;

                var disDates = approvalDates.Distinct();
                activitiesDateCount = disDates.Count();

                disDates = disDates.OrderByDescending(d => d.Date.Date);
                activitiesDate = disDates.OrderByDescending(d => d.Date).Skip(size).Take(HelperClass.qbiclePageSize);

                approvals = qApproval.Where(o => activitiesDate.Contains(o.TimeLineDate.Date)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, approvals, activitiesDateCount, currentTimeZone);
            }

            return activitiesDate;
        }

        public ApprovalReq GetApprovalById(int approvalId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more approval", null, null, approvalId);

                return dbContext.ApprovalReqs.Find(approvalId) ?? new ApprovalReq();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, approvalId);
                return new ApprovalReq();
            }
        }

        public async Task<ReturnJsonModel> SetRequestStatusForApprovalRequest(string approvalReqId, ApprovalReq.RequestStatusEnum status, string userId, string originatingConnectionId = "")
        {
            var refModel = new ReturnJsonModel
            {
                actionVal = 0,
                result = true,
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set request status for approval request",
                        userId, null, approvalReqId, status);

                var appId = HelperClass.Converter.Obj2Int(approvalReqId.Decrypt());
                if (appId <= 0) return refModel;
                var app = GetApprovalById(appId);
                app.TimeLineDate = DateTime.UtcNow;
                var currentUser = dbContext.QbicleUser.Find(userId);
                switch (status)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        app.ReviewedBy.Remove(currentUser);
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        app.ReviewedBy.Add(currentUser);
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        app.ApprovedOrDeniedAppBy = currentUser;
                        app.ClosedBy = currentUser;
                        app.ClosedDate = DateTime.UtcNow;
                        app.State = QbicleActivity.ActivityStateEnum.Closed;
                        break;
                }

                if (app.ApprovalReqHistories == null)
                {
                    app.ApprovalReqHistories = new List<ApprovalReqHistory>();
                }

                app.ApprovalReqHistories.Add(new ApprovalReqHistory
                {
                    ApprovalReq = app,
                    CreatedDate = DateTime.UtcNow,
                    RequestStatus = status,
                    UpdatedBy = currentUser
                });

                app.ApprovedOrDeniedAppBy = currentUser;
                app.RequestStatus = status;
                app.Qbicle.LastUpdated = DateTime.UtcNow;
                //var startby = app.StartedBy;


                //Process Notification
                var eventApproval = Notification.NotificationEventEnum.ApprovalUpdate;
                switch (status)
                {
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        eventApproval = Notification.NotificationEventEnum.ApprovalReviewed;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalReviewed;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        eventApproval = Notification.NotificationEventEnum.ApprovalApproved;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalApproved;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        eventApproval = Notification.NotificationEventEnum.ApprovalDenied;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalDenied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Pending:
                        eventApproval = Notification.NotificationEventEnum.ApprovalUpdate;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalReviewed;
                        break;
                }

                //app.Transfer.FirstOrDefault().Logs.
                //var appHistory = app.ApprovalReqHistories[0].;

                dbContext.Entry(app).State = EntityState.Modified;
                dbContext.SaveChanges();

                //update sale status
                if (status == ApprovalReq.RequestStatusEnum.Approved && app.JournalEntries.FirstOrDefault() != null)
                    new JournalEntryRules(dbContext).JournalEntryApproval(app);

                if (app.StockAudits.Count > 0)
                    new TraderStockAuditRules(dbContext).ShiftAuditApproval(app);

                if (app.Sale.Count > 0)
                {
                    new TraderSaleRules(dbContext).SaleApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.SaleApproval, app.Sale.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }
                if (app.TraderReturns.Count > 0)
                    new TraderSalesReturnRules(dbContext).SaleReturnApproval(app);

                if (app.Purchase.Count > 0)
                {
                    new TraderPurchaseRules(dbContext).PurchaseApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.PurchaseApproval, app.Purchase.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }

                if (app.TraderContact.Count > 0)
                    new TraderContactRules(dbContext).ContactApproval(app);

                if (app.Transfer.Count > 0)
                {
                    new TraderTransfersRules(dbContext).TransferApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.TransferApproval, app.Transfer.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }

                if (app.Invoice.Count > 0)
                {
                    new TraderInvoicesRules(dbContext).InvoiceApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.InvoiceApproval, app.Invoice.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }

                if (app.Payments.Count > 0)
                {
                    new TraderCashBankRules(dbContext).PaymentApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.PaymentApproval, app.Payments.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }

                if (app.SpotCounts.Count > 0)
                    new TraderSpotCountRules(dbContext).SpotCountApproval(app);

                if (app.Manufacturingjobs.Count > 0)
                    await new TraderManufacturingRules(dbContext).ManufacturingApproval(app);

                if (app.WasteReports.Count > 0)
                    new TraderWasteReportRules(dbContext).WasteReportApproval(app);

                if (app.CreditNotes.Count > 0)
                    new TraderContactRules(dbContext).CreditNoteApproval(app);

                if (app.BudgetScenarioItemGroups.Count > 0)
                    new TraderBudgetMainRules(dbContext).BudgetScenarioItemGroupApproval(app);

                if (app.ConsumptionReports.Count() > 0)
                    new SpanneredConsumeReportRules(dbContext).ConsumeReportApproval(app);

                if (app.TillPayment.Count() > 0)
                    new CMsRules(dbContext).TillPaymentApproval(app);

                refModel.actionVal = 1;
                refModel.msg = status.GetDescription();
                switch (status)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        refModel.msgName = StatusLabelStyle.Pending;
                        break;

                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        refModel.msgName = StatusLabelStyle.Reviewed;
                        break;

                    case ApprovalReq.RequestStatusEnum.Approved:
                        refModel.msgName = StatusLabelStyle.Approved;
                        break;

                    case ApprovalReq.RequestStatusEnum.Denied:
                        refModel.msgName = StatusLabelStyle.Denied;
                        break;

                    case ApprovalReq.RequestStatusEnum.Discarded:
                        refModel.msgName = StatusLabelStyle.Discarded;
                        break;
                }

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = appId,
                    EventNotify = eventApproval,
                    AppendToPageName = ApplicationPageName.Activities,
                    CreatedByName = HelperClass.GetFullNameOfUser(currentUser),
                    CreatedById = currentUser.Id,
                    ReminderMinutes = 0
                };
                nRule.Notification2UpdateStatusApprovalCannotAttendEventCloseTask(activityNotification);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, approvalReqId, status);
                refModel.result = false;
                refModel.msg = ex.Message;
            }
            return refModel;
        }

        public List<ApprovalStatusTimeline> GetApprovalStatusTimeline(ApprovalReq contactApproval, string timezone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get approval status timeline", null, null, contactApproval, timezone);

                var timeline = new List<ApprovalStatusTimeline>();
                string icon = "";

                foreach (var log in contactApproval.ApprovalReqHistories)
                {
                    switch (log.RequestStatus)
                    {
                        case ApprovalReq.RequestStatusEnum.Pending:
                            icon = "fa fa-info bg-yellow";
                            break;

                        case ApprovalReq.RequestStatusEnum.Reviewed:
                            icon = "fa fa-truck bg-aqua";
                            break;

                        case ApprovalReq.RequestStatusEnum.Approved:
                            icon = "fa fa-check bg-green";
                            break;

                        case ApprovalReq.RequestStatusEnum.Denied:
                            icon = "fa fa-warning bg-red";
                            break;

                        case ApprovalReq.RequestStatusEnum.Discarded:
                            icon = "fa fa-trash bg-red";
                            break;
                    }
                    timeline.Add
                    (
                        new ApprovalStatusTimeline
                        {
                            LogDate = log.CreatedDate,
                            Time = log.CreatedDate.ConvertTimeFromUtc(timezone).ToShortTimeString(),
                            Status = log.RequestStatus.GetDescription(),
                            Icon = icon,
                            UserAvatar = log.UpdatedBy.ProfilePic
                        }
                    );
                }

                return timeline;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, contactApproval, timezone);
                return new List<ApprovalStatusTimeline>();
            }
        }

        public bool AddPostToApproval(bool isCreatorTheCustomer, string message, int approvalId, string currentUserId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add post to approval", currentUserId, null, message, approvalId, currentUserId);

                var approval = GetApprovalById(approvalId);
                var qbicleId = approval.Qbicle.Id;

                var post = new PostsRules(dbContext).SavePost(isCreatorTheCustomer, message, currentUserId, qbicleId);

                approval.TimeLineDate = DateTime.UtcNow;
                approval.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                approval.Posts.Add(post);
                //var startedBy = approval.StartedBy;
                approval.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.ApprovalReqs.Attach(approval);
                dbContext.Entry(approval).State = EntityState.Modified;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = approvalId,
                    PostId = post.Id,
                    EventNotify = Notification.NotificationEventEnum.ApprovalUpdate,
                    AppendToPageName = ApplicationPageName.Approval,
                    AppendToPageId = approvalId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, message, approvalId, currentUserId);
                return false;
            }
        }


        #region PurchaseTransferApprovalRequest

        /// <summary>
        /// Manage PurchaseTransferApprovalRequest
        /// </summary>
        /// <param name="approvalReqId"></param>
        /// <param name="status"></param>
        /// <param name="userId"></param>
        /// <param name="originatingConnectionId"></param>
        /// <returns></returns>
        public ReturnJsonModel SetPurchaseTransferApprovalRequest(string approvalReqId, ApprovalReq.RequestStatusEnum status, string userId, string originatingConnectionId = "")
        {
            var refModel = new ReturnJsonModel
            {
                actionVal = 0,
                result = true,
            };

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set request status for approval request",
                        userId, null, approvalReqId, status);

                var appId = HelperClass.Converter.Obj2Int(approvalReqId.Decrypt());
                if (appId <= 0) return refModel;
                var app = GetApprovalById(appId);
                app.TimeLineDate = DateTime.UtcNow;
                var currentUser = dbContext.QbicleUser.Find(userId);
                switch (status)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        app.ReviewedBy.Remove(currentUser);
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        app.ReviewedBy.Add(currentUser);
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        app.ApprovedOrDeniedAppBy = currentUser;
                        app.ClosedBy = currentUser;
                        app.ClosedDate = DateTime.UtcNow;
                        app.State = QbicleActivity.ActivityStateEnum.Closed;
                        break;
                }

                if (app.ApprovalReqHistories == null)
                {
                    app.ApprovalReqHistories = new List<ApprovalReqHistory>();
                }

                app.ApprovalReqHistories.Add(new ApprovalReqHistory
                {
                    ApprovalReq = app,
                    CreatedDate = DateTime.UtcNow,
                    RequestStatus = status,
                    UpdatedBy = currentUser
                });

                app.ApprovedOrDeniedAppBy = currentUser;
                app.RequestStatus = status;
                app.Qbicle.LastUpdated = DateTime.UtcNow;
                //var startby = app.StartedBy;


                //Process Notification
                var eventApproval = Notification.NotificationEventEnum.ApprovalUpdate;
                switch (status)
                {
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        eventApproval = Notification.NotificationEventEnum.ApprovalReviewed;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalReviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        eventApproval = Notification.NotificationEventEnum.ApprovalApproved;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalApproved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        eventApproval = Notification.NotificationEventEnum.ApprovalDenied;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalDenied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Pending:
                        eventApproval = Notification.NotificationEventEnum.ApprovalUpdate;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.ApprovalReviewed;
                        break;
                }

                //app.Transfer.FirstOrDefault().Logs.
                //var appHistory = app.ApprovalReqHistories[0].;

                dbContext.Entry(app).State = EntityState.Modified;
                dbContext.SaveChanges();


                if (app.Purchase.Count > 0)
                {
                    new TraderPurchaseRules(dbContext).PurchaseApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.PurchaseApproval, app.Purchase.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }

                if (app.Transfer.Count > 0)
                {
                    new TraderTransfersRules(dbContext).TransferApproval(app);
                    new TradeOrderLoggingRules(dbContext).TradeOrderLogging(TradeOrderLoggingType.TransferApproval, app.Transfer.FirstOrDefault().Id, app.GetCreatedBy().Id);
                }


                refModel.actionVal = 1;
                refModel.msg = status.GetDescription();


                switch (status)
                {
                    case ApprovalReq.RequestStatusEnum.Pending:
                        refModel.msgName = StatusLabelStyle.Pending;
                        break;
                    case ApprovalReq.RequestStatusEnum.Reviewed:
                        refModel.msgName = StatusLabelStyle.Reviewed;
                        break;
                    case ApprovalReq.RequestStatusEnum.Approved:
                        refModel.msgName = StatusLabelStyle.Approved;
                        break;
                    case ApprovalReq.RequestStatusEnum.Denied:
                        refModel.msgName = StatusLabelStyle.Denied;
                        break;
                    case ApprovalReq.RequestStatusEnum.Discarded:
                        refModel.msgName = StatusLabelStyle.Discarded;
                        break;
                }


                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = appId,
                    EventNotify = eventApproval,
                    AppendToPageName = ApplicationPageName.Activities,
                    CreatedByName = HelperClass.GetFullNameOfUser(currentUser),
                    CreatedById = currentUser.Id,
                    ReminderMinutes = 0
                };

                nRule.Notification2UpdateStatusApprovalCannotAttendEventCloseTask(activityNotification);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, approvalReqId, status);
                refModel.result = false;
                refModel.msg = ex.Message;
            }

            return refModel;
        }

        #endregion 

    }
}