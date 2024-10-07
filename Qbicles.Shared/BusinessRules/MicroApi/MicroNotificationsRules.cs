using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro.Model;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.Highlight;
using Qbicles.Models.MicroQbicleStream;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules.Micro
{
    public class MicroNotificationsRules : MicroRulesBase
    {
        public MicroNotificationsRules(MicroContext microContext) : base(microContext)
        {
        }

        public int GetCountNotifications()
        {
            return new NotificationRules(dbContext).GetNotificationByUser(CurrentUser.Id, false);
        }

        /// <summary>
        /// Micro api get list notificatons
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        public PaginationResponse GetNotifications(PaginationRequest pagination)
        {

            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "GetNotificationCusByUser", CurrentUser.Id, pagination);

            var order = int.Parse(pagination.orderBy);

            var notifications = dbContext.Notifications.Where(n => n.NotifiedUser.Id == CurrentUser.Id && !n.IsRead
                                                               && n.Event != NotificationEventEnum.TopicPost);

            if (order == 0)
                notifications = notifications.OrderByDescending(x => x.CreatedDate);
            else
                notifications = notifications.OrderBy(x => x.CreatedDate);

            var paginationResponse = new PaginationResponse { totalNumber = notifications.Count() };
            paginationResponse.totalPage = ((paginationResponse.totalNumber % pagination.pageSize) == 0) ? (paginationResponse.totalNumber / pagination.pageSize) : (paginationResponse.totalNumber / pagination.pageSize) + 1;

            //var notificationModels = notifications.Skip(pagination.pageNumber * pagination.pageSize).Take(pagination.pageSize).ToList()
            //    .Select(s => new MicroNotificationModel
            //    {
            //        Id = s.Id,
            //        CreatedBy = s.CreatedBy.GetFullName(),
            //        CreatedImage = s.CreatedBy.ProfilePic.ToUri(Enums.FileTypeEnum.Image,"T"),
            //        Activity = s.AssociatedAcitvity,
            //        CreatedDate = s.CreatedDate.ConvertTimeFromUtc(CurrentUser.Timezone),
            //        DomainName = s.AssociatedDomain?.Name,
            //        DomainId = s.AssociatedDomain?.Id,
            //        QbicleName = s.AssociatedQbicle?.Name,
            //        QbicleId = s.AssociatedQbicle?.Id,
            //        Event = s.Event,
            //        AssociatedPost = s.AssociatedPost,
            //        AssociatedUser = s.AssociatedUser?.GetFullName(),
            //        AssociateInvitation = s.AssociateInvitation != null,
            //        AssociatedHighlight = s.AssociatedHighlight,
            //        DomainRequested = s.AssociatedDomainRequest?.DomainRequestJSON,
            //        ExtensionRequest = s.AssociatedExtensionRequest,
            //        IsRead = s.IsRead
            //    }).ToList();

            var notificationModels = notifications.Skip(pagination.pageNumber * pagination.pageSize).Take(pagination.pageSize).ToList();

            var microNotifications = new List<MicroNotification>(pagination.pageSize);
            //var pages = new ConcurrentBag<MicroNotification>();
            notificationModels.ForEach(noti =>
            {
               // var activity = noti.AssociatedAcitvity;
                var qbicleName = noti.AssociatedAcitvity?.Qbicle?.Name;
                var domainName = noti.AssociatedDomain?.Name;
                var createdByName = noti.CreatedBy?.GetFullName();
                var createdDate = noti.CreatedDate.ConvertTimeFromUtc(CurrentUser.Timezone);
                var activityName = noti.AssociatedAcitvity?.Name;


                var microNotification = new MicroNotification
                {
                    NotificationId = noti.Id.Encrypt(),
                    EventId = noti.Event,
                    DomainId = noti.AssociatedDomain?.Id,
                    QbicleId = noti.AssociatedAcitvity?.Qbicle?.Id,
                    CreatedByName = noti.CreatedBy?.GetFullName(),
                    CreatedByImage = noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T"),
                    CreatedDateString = createdDate.GetTimeRelative(),
                    CreatedDate = createdDate,
                    ActivityKey = noti.AssociatedAcitvity?.Id,
                    IsRead = noti.IsRead
                };
                //Get Id, Key
                switch (noti.Event)
                {
                    case NotificationEventEnum.ApprovalCreation:
                    case NotificationEventEnum.ApprovalUpdate:
                    case NotificationEventEnum.ApprovalReviewed:
                    case NotificationEventEnum.ApprovalApproved:
                    case NotificationEventEnum.ApprovalDenied:
                        var app = (ApprovalReq)noti.AssociatedAcitvity;
                        switch (app?.ActivityType)
                        {
                            case QbicleActivity.ActivityTypeEnum.ApprovalRequest:
                                if (app?.Sale?.Count > 0)
                                    microNotification.ActivityKey = app?.Sale?.FirstOrDefault()?.Id;
                                if (app?.Purchase?.Count > 0)
                                    microNotification.ActivityKey = app?.Purchase?.FirstOrDefault()?.Id;
                                if (app?.TraderContact?.Count > 0)
                                    microNotification.ActivityKey = app?.TraderContact?.FirstOrDefault()?.Id;
                                if (app?.Transfer?.Count > 0)
                                    microNotification.ActivityKey = app?.Transfer?.FirstOrDefault()?.Id;
                                if (app?.Invoice?.Count > 0)
                                    microNotification.ActivityKey = app?.Invoice?.FirstOrDefault()?.Id;
                                if (app?.Payments?.Count > 0)
                                    microNotification.ActivityKey = app?.Payments?.FirstOrDefault()?.Id;
                                if (app?.SpotCounts?.Count > 0)
                                    microNotification.ActivityKey = app?.SpotCounts?.FirstOrDefault()?.Id;
                                if (app?.WasteReports?.Count > 0)
                                    microNotification.ActivityKey = app?.WasteReports?.FirstOrDefault()?.Id;
                                if (app?.Manufacturingjobs?.Count > 0)
                                    microNotification.ActivityKey = app?.Manufacturingjobs?.FirstOrDefault()?.Id;
                                break;
                        }

                        break;
                }

                switch (noti.Event)
                {
                    case NotificationEventEnum.QbicleInvited:
                        if (noti.AssociateInvitation != null) break;
                        microNotification.Title = $"Invited you to join their <b>{domainName}</b> Domain";
                        break;
                    case NotificationEventEnum.AlertCreation:
                    case NotificationEventEnum.AlertUpdate:
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.AlertUpdate ? "Updated" : "Created")} the Alert <b>{activityName}</b> in <b>{domainName}/ {qbicleName}</b>";
                        break;
                    case NotificationEventEnum.ApprovalCreation:
                    case NotificationEventEnum.ApprovalUpdate:
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.ApprovalUpdate ? "Updated" : "Created")} the Approval Request <b>{activityName}</b> in <b>{domainName}/ {qbicleName}</b>";
                        break;
                    case NotificationEventEnum.CreateMember:
                        if (noti.AssociatedUser == null) break;
                        microNotification.Title = $"Added the user <b>{noti.AssociatedUser}</b> into <b>{domainName}</b>";
                        break;
                    case NotificationEventEnum.EventCreation:
                    case NotificationEventEnum.EventUpdate:
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.EventUpdate ? "Updated" : "Created")} an Event <b>{activityName}</b> in <b>{domainName}/ {qbicleName}</b>";
                        break;
                    case NotificationEventEnum.EventWithdrawl:
                        microNotification.Title = $"Withdrew an Event <b>{activityName}</b> in <b>{domainName}/ {qbicleName}</b>";
                        break;
                    case NotificationEventEnum.InvitedMember:
                        if (noti.AssociatedUser != null) break;
                        microNotification.Title = $"Invited the guest in <b>{domainName}</b>";
                        break;
                    case NotificationEventEnum.MediaCreation:
                    case NotificationEventEnum.MediaUpdate:
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.MediaUpdate ? "Updated" : "Created")} a Media Item <b>{activityName}</b> in <b>{domainName}/ {qbicleName}</b>";
                        break;
                    case NotificationEventEnum.PostCreation:
                    case NotificationEventEnum.PostEdit:
                    case NotificationEventEnum.JournalPost:
                    case NotificationEventEnum.TransactionPost:
                    case NotificationEventEnum.ActivityComment:
                        var postCreation = noti.AssociatedPost;
                        if (postCreation != null && postCreation.Topic != null)
                        {
                            microNotification.ActivityKey = postCreation?.Id;
                            if (postCreation.JournalEntry != null)
                            {
                                microNotification.QbicleId = postCreation.JournalEntry?.WorkGroup?.Qbicle.Id;
                                qbicleName = postCreation.JournalEntry?.WorkGroup?.Qbicle.Name;
                                microNotification.Title = $"Added a comment to Journal Entry #<b>{postCreation.JournalEntry?.Number}</b> in <b>{domainName}/ {qbicleName}</b>";
                            }
                            else if (postCreation.BKTransaction != null)
                            {
                                microNotification.QbicleId = postCreation.BKTransaction?.JournalEntry?.WorkGroup?.Qbicle.Id;
                                qbicleName = postCreation.BKTransaction?.JournalEntry?.WorkGroup?.Qbicle.Name;
                                microNotification.Title = $"Added a comment to Transaction in Journal Entry #<b>{postCreation.BKTransaction?.JournalEntry?.Number}</b> in <b>{domainName}/ {qbicleName}</b>";
                            }
                            else
                            {
                                microNotification.QbicleId = postCreation.Topic?.Qbicle.Id;
                                qbicleName = postCreation.Topic?.Qbicle.Name;
                                if (noti.Event == NotificationEventEnum.PostCreation)
                                    microNotification.Title = $"Added a post to <b>{domainName}/ {qbicleName}</b>";
                                else
                                    microNotification.Title = $"Edited a post to <b>{domainName}/ {qbicleName}</b>";
                            }
                        }
                        else
                        {
                            if (noti.AssociatedAcitvity != null)
                            {
                                microNotification.Title = $"Added a comment in <b>{domainName}/ {qbicleName}</b>";
                            }
                        }
                        break;
                    case NotificationEventEnum.QbicleCreation:
                    case NotificationEventEnum.QbicleUpdate:
                        microNotification.QbicleId = noti.AssociatedQbicle?.Id;
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.QbicleUpdate ? "Updated" : "Created")} the Qbicle <b>{qbicleName}</b> in {domainName}";
                        break;
                    case NotificationEventEnum.TaskCompletion:
                        microNotification.Title = $"Completed the Task <b>{activityName}</b> in <b>{domainName}/{qbicleName}</b>";
                        break;
                    case NotificationEventEnum.TaskCreation:
                    case NotificationEventEnum.TaskUpdate:
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.TaskUpdate ? "Updated" : "Created")} the Task <b>{activityName}</b> in <b>{domainName}/{qbicleName}</b>";
                        break;
                    case NotificationEventEnum.DiscussionCreation:
                    case NotificationEventEnum.DiscussionUpdate:
                        //if (activity is QbicleDiscussion)
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.DiscussionUpdate ? "Updated" : "Created")} the Discussion <b>{activityName}</b>";
                        break;
                    case NotificationEventEnum.LinkCreation:
                    case NotificationEventEnum.LinkUpdate:
                        microNotification.Title = $"{(noti.Event == NotificationEventEnum.LinkUpdate ? "Updated" : "Created")} the Link {activityName} in <b>{domainName}/{qbicleName}</b>";
                        break;
                    case NotificationEventEnum.ApprovalReviewed:
                    case NotificationEventEnum.ApprovalApproved:
                    case NotificationEventEnum.ApprovalDenied:
                        var appTitle = "";
                        switch (noti.Event)
                        {
                            case NotificationEventEnum.ApprovalReviewed:
                                appTitle = "Reviewed";
                                break;
                            case NotificationEventEnum.ApprovalApproved:
                                appTitle = "Approved";
                                break;
                            case NotificationEventEnum.ApprovalDenied:
                                appTitle = "Denied";
                                break;
                        }
                        microNotification.Title = $"{appTitle} the Approval Request <b>{activityName}</b> in <b>{domainName}/{qbicleName}</b>";
                        break;

                    case NotificationEventEnum.RemoveUserOutOfDomain:
                        microNotification.Title = $"Removed you from the Domain <b>{domainName}</b>";
                        break;
                    case NotificationEventEnum.ReminderCampaignPost:
                        //if (activity is ApprovalReq)
                        //{

                        //}
                        var app = (ApprovalReq)noti.AssociatedAcitvity;
                        if (app != null)
                        {
                            var campaignApproval = app.CampaigPostApproval.FirstOrDefault();
                            var campaignPost = campaignApproval.CampaignPost;
                            microNotification.Title = $"{campaignPost.Reminder.Content} in <b>{domainName}/ {qbicleName}</b>";
                            if (campaignPost.AssociatedCampaign.CampaignType != Models.SalesMkt.CampaignType.Automated)
                                microNotification.CreatedByImage = $"icon_socialpost_manual.png".ToUri();
                        }
                        break;
                    case NotificationEventEnum.AssignTask:
                        microNotification.Title = $"Assigned you to the Task {activityName} in <b>{domainName}/ {qbicleName}</b>";
                        break;
                    case NotificationEventEnum.C2CConnectionIssued:
                        microNotification.Title = $"Wants to connect with you";
                        break;

                    case NotificationEventEnum.C2CConnectionAccepted:
                        microNotification.Title = $"You are now connected with {noti.CreatedBy}";
                        break;
                    case NotificationEventEnum.B2CConnectionCreated:
                        microNotification.Title = $"{noti.CreatedBy} has connected with you in B2C.";
                        break;
                    case NotificationEventEnum.ListingInterested:
                        //noti.AssociatedHighlight.Title
                        var listingPost = noti.AssociatedHighlight as ListingHighlight;
                        microNotification.Title = $"{noti.CreatedBy} has expressed interest in your <b>{listingPost.ListingHighlightType.GetDescription()}</b> listing <b>{listingPost.Reference}</b>";
                        break;
                    case NotificationEventEnum.RemoveUserOutOfQbicle:
                        microNotification.Title = $"<b>{noti.CreatedBy}</b> has removed you from the Qbicle <b>{qbicleName}<b>";
                        break;
                    case NotificationEventEnum.AddUserParticipants:
                    case NotificationEventEnum.RemoveUserParticipants:
                        //if (activity is QbicleDiscussion)
                        //{                            
                        //}
                        var actionType = noti.Event == NotificationEventEnum.AddUserParticipants ? "Add you to " : "Remove you from ";
                        microNotification.Title = $"{actionType} the participants in the discussion <b>{activityName}</b>";
                        break;
                    case NotificationEventEnum.AddUserToQbicle:
                        microNotification.Title = $"Added you to Qbicle{qbicleName}";
                        microNotification.QbicleId = noti.AssociatedQbicle?.Id;
                        break;
                    case NotificationEventEnum.ProcessDomainRequest:
                        if (noti.AssociatedDomainRequest == null) break;
                        var domainRequested = noti.AssociatedDomainRequest.DomainRequestJSON.ParseAs<DomainRequest>();
                        microNotification.Title = $"An administrator has reviewed your request for {domainRequested?.Name} Domain. Tap here to track the progress.";
                        microNotification.CreatedByImage = domainRequested?.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T");
                        break;
                    case NotificationEventEnum.ProcessExtensionRequest:
                        var extensionRequested = noti.AssociatedExtensionRequest;
                        microNotification.CreatedByImage = extensionRequested?.Domain.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T");
                        microNotification.DomainId = extensionRequested?.Domain.Id;
                        if (extensionRequested.Status == ExtensionRequestStatus.Approved)
                            microNotification.Title = $"<b>{extensionRequested?.Type.GetDescription()}</b> is now available for use.";
                        else if (extensionRequested.Status == ExtensionRequestStatus.Rejected)
                            microNotification.Title = $"<b>{extensionRequested?.Type.GetDescription()}</b>has been rejected. Click here for more information.";
                        break;
                    case NotificationEventEnum.TopicPost:
                    case NotificationEventEnum.RemoveQueue:
                    default:
                        break;
                }

                //yield return microNotification;
                //microNotifications.Add(microNotification);
                microNotifications.Add(microNotification);
            });

            //paginationResponse.items = microNotifications;
            //paginationResponse.items = GenerateList(notificationModels);
            paginationResponse.items = microNotifications;
            return paginationResponse;
        }


        private string GetLinkDiscussion(QbicleDiscussion discussion)
        {
            string link = "";
            if (discussion == null)
                return link;
            switch (discussion.DiscussionType)
            {
                case QbicleDiscussion.DiscussionTypeEnum.IdeaDiscussion:
                    link = "/SalesMarketingIdea/DiscussionIdea?disId=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.PlaceDiscussion:
                    link = "/SalesMarketingLocation/DiscussionPlace?disId=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.QbicleDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.PosOrderDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.PerformanceDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.OrderCancellation:
                    link = "/Qbicles/DiscussionQbicle?disKey=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion:
                    link = "/Operator/DiscussionGoal?disId=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion:
                    link = "/Operator/DiscussionComplianceTask?disId=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.CashManagement:
                    link = "/CashManagement/DiscussionCashManagementShow?disKey=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2CProductMenu:
                    link = "/B2C/DiscussionMenu?disKey=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2COrder:
                    link = "/B2C/DiscussionOrder?disKey=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2BOrder:
                    link = "/Commerce/DiscussionOrder?disKey=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2BPartnershipDiscussion:
                    var b2BPartnershipDiscussion = discussion as Models.B2B.B2BPartnershipDiscussion;
                    if (b2BPartnershipDiscussion != null)
                        link = "/Commerce/DiscussionPartner?rlid=" + (b2BPartnershipDiscussion.Relationship?.Id ?? 0);
                    break;
                default:
                    break;
            }
            return link;
        }

        public NotificationDetail GetNotificationDetail(int id, bool isRenderHtml)
        {
            return new NotificationRules(dbContext).GetNotificationById(id, isRenderHtml);

        }

        public bool MarkAsReadNotification(string notificationId)
        {
            var id = int.Parse(notificationId.Decrypt());
            var notification = dbContext.Notifications.Where(e => e.Id == id && e.IsRead == false).FirstOrDefault();
            if (notification != null)
                notification.IsRead = true;
            dbContext.SaveChanges();
            return true;
        }
    }
}
