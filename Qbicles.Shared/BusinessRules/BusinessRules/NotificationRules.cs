using Qbicles.BusinessRules.Hangfire;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Highlight;
using Qbicles.Models.MicroQbicleStream;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static Qbicles.Models.EmailLog;
using static Qbicles.Models.Notification;
using Notification = Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class NotificationRules : ISignalRNotification
    {
        private ApplicationDbContext dbContext;

        public NotificationRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        #region Notification data action : MarkAsRead,GetNotification,


        public int MarkAsReadNotification(int notificationId, int activityId = 0)
        {
            var notification = dbContext.Notifications.Where(e => e.Id == notificationId && e.IsRead == false).FirstOrDefault();
            if (activityId > 0)
                notification = dbContext.Notifications.Where(e => e.AssociatedAcitvity.Id == activityId && e.IsRead == false).FirstOrDefault();
            if (notification != null)
                notification.IsRead = true;
            dbContext.SaveChanges();
            return 1;
        }


        public int GetNotificationByUser(string userId, bool isRead)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetbNotificationbBybUser", userId, isRead);

                return dbContext.Notifications.AsNoTracking().Count(n => n.NotifiedUser.Id == userId
                                                                   && n.IsRead == isRead &&
                                                                   n.Event != NotificationEventEnum.TopicPost);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, isRead);
                return 0;
            }

        }

        /// <summary>
        /// Get notifications and display to model on bell
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isRead"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public PaginationResponse ShowNotificationsModal(PaginationRequest pagination, string userId, bool isRead, string timezone)
        {
            PaginationResponse paginationResponse = new PaginationResponse();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetNotificationCusByUser", userId, isRead, pagination);
                var order = int.Parse(pagination.orderBy);
                var notifications = dbContext.Notifications.Where(n => n.NotifiedUser.Id == userId
                                                                   && n.IsRead == isRead &&
                                                                   n.Event != NotificationEventEnum.TopicPost);



                if (order == 0)
                    notifications = notifications.OrderByDescending(x => x.CreatedDate);
                else
                    notifications = notifications.OrderBy(x => x.CreatedDate);

                paginationResponse.totalNumber = notifications.Count();
                paginationResponse.totalPage = ((paginationResponse.totalNumber % pagination.pageSize) == 0) ? (paginationResponse.totalNumber / pagination.pageSize) : (paginationResponse.totalNumber / pagination.pageSize) + 1;

                var notificationModels = notifications.Skip((pagination.pageNumber - 1) * pagination.pageSize).Take(pagination.pageSize).ToList().Select(s => new NotificationModel
                {
                    Id = s.Id,
                    CreatedBy = s.CreatedBy,
                    Activity = s.AssociatedAcitvity,
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timezone),
                    AssociatedDomain = s.AssociatedDomain,
                    AssociatedQbicle = s.AssociatedQbicle,
                    Event = s.Event,
                    AssociatedPost = s.AssociatedPost,
                    AssociatedUser = s.AssociatedUser,
                    AssociateInvitation = s.AssociateInvitation,
                    AssociatedHighlight = s.AssociatedHighlight,
                    DomainRequested = s.AssociatedDomainRequest?.DomainRequestJSON,
                    ExtensionRequest = s.AssociatedExtensionRequest,
                    IsRead = s.IsRead,
                    IsCreatorTheCustomer = s.IsCreatorTheCustomer,
                    AssociatedWaitList = s.AssociatedWaitList
                });

                var items = new List<string>();


                if (notificationModels != null && notificationModels.Any())
                {
                    var approvalType = "";
                    var approvalName = "";
                    foreach (var noti in notificationModels)
                    {
                        var activity = noti.Activity;
                        var qbicleName = activity?.Qbicle != null ? " / " + activity?.Qbicle.Name : "";
                        switch (noti.Event)
                        {
                            case NotificationEventEnum.ApprovalCreation:
                            case NotificationEventEnum.ApprovalUpdate:
                            case NotificationEventEnum.ApprovalReviewed:
                            case NotificationEventEnum.ApprovalApproved:
                            case NotificationEventEnum.ApprovalDenied:
                                var app = (ApprovalReq)activity;
                                approvalName = app?.Name;
                                switch (app?.ActivityType)
                                {
                                    case QbicleActivity.ActivityTypeEnum.ApprovalRequest:
                                        if (app?.Sale?.Count > 0)
                                        {
                                            approvalType = "/TraderSales/SaleReview?key=" + app?.Sale?.FirstOrDefault()?.Key;
                                        }
                                        if (app?.Purchase?.Count > 0)
                                        {
                                            approvalType = "/TraderPurchases/PurchaseReview?id=" + app?.Purchase?.FirstOrDefault()?.Id;
                                        }
                                        if (app?.TraderContact?.Count > 0)
                                        {
                                            approvalType = "/TraderContact/ContactReview?id=" + app?.TraderContact?.FirstOrDefault()?.Id;
                                        }
                                        if (app?.Transfer?.Count > 0)
                                        {
                                            approvalType = "/TraderTransfers/TransferReview?key=" + app?.Transfer?.FirstOrDefault()?.Key;
                                        }
                                        if (app?.Invoice?.Count > 0)
                                        {
                                            approvalType = "/TraderInvoices/InvoiceReview?key=" + app?.Invoice?.FirstOrDefault()?.Key;
                                        }
                                        break;
                                    case QbicleActivity.ActivityTypeEnum.ApprovalRequestApp:
                                        if (app?.JournalEntries != null && app?.JournalEntries?.Count > 0)
                                            approvalType = "journal";
                                        else
                                            approvalType = "approval";
                                        break;
                                }

                                break;
                        }

                        var trHtml = "<tr id=\"notification-id-" + noti.Id + "\">";
                        switch (noti.Event)
                        {
                            case NotificationEventEnum.JoinToWaitlist:
                                if (noti.AssociatedWaitList == null) break;
                                trHtml += "<td style=\"width:30px; \">" +
                                    "<input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_qbicleinviteId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" " +
                                    $"style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{noti.CreatedBy.GetFullName()}</strong> has joined the <strong>" +
                                    $"<a href=\"javascript:void(0)\" onclick=\"ShowAdminJoinToWaitlist(); \">waitlist</a></strong></p><small>{noti.CreatedDate.GetTimeRelative()}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.QbicleInvited:
                                if (noti.AssociateInvitation == null) break;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_qbicleinviteId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> has invited you to join their <strong><a href=\"javascript:void(0)\"  onclick=\"ShowDomanInvited(); \">{noti.AssociatedDomain.Name}</a></strong> Domain</p><small>{(noti.CreatedDate.GetTimeRelative() + " in " + noti.AssociatedDomain.Name)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.AlertCreation:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> created the Alert <a href=\"javascript:void(0)\" onclick=\"ShowAlertPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.AlertUpdate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated the Alert <a href=\"javascript:void(0)\" onclick=\"ShowAlertPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ApprovalCreation:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> created the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}', 'false','{approvalType}');\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ApprovalUpdate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}', 'false','{approvalType}');\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.CreateMember:
                                var CreateMember = noti.AssociatedUser;
                                if (CreateMember == null) break;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_userId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> added the user {HelperClass.GetFullNameOfUser(CreateMember)}</p><small>{(noti.CreatedDate.GetTimeRelative() + " in " + noti.AssociatedDomain.Name)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.EventCreation:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> created the Event <a href=\"javascript:void(0)\" onclick=\"ShowEventPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.EventUpdate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated the Event <a href=\"javascript:void(0)\" onclick=\"ShowEventPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.EventWithdrawl:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> withdrew from the Event <a href=\"javascript:void(0)\" onclick=\"ShowEventPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.InvitedMember:
                                var invitedMember = noti.AssociatedUser;
                                if (invitedMember != null) break;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_userId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> invited the guest</p><small>{(noti.CreatedDate.GetTimeRelative() + " in " + noti.AssociatedDomain.Name)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.MediaCreation:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> uploaded the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;

                            case NotificationEventEnum.MediaUpdate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.MediaRemoveVersion:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> removed version in the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.MediaAddVersion:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> added version in to the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.PostCreation:
                            case NotificationEventEnum.PostEdit:
                            case NotificationEventEnum.JournalPost:
                            case NotificationEventEnum.TransactionPost:
                            case NotificationEventEnum.ActivityComment:
                                var postCreation = noti.AssociatedPost;
                                // post for dashboard - topic_id != null
                                // Post for activity - qbicleactivity_id != null
                                if (postCreation != null && postCreation.Topic != null)
                                {
                                    var _contentHtml = "";
                                    if (postCreation.JournalEntry != null)
                                    {
                                        _contentHtml = "<p><strong>" + HelperClass.GetFullNameOfUser(noti.CreatedBy)
                                            + $"</strong> has been added a comment to Journal Entry " +
                                            "<a href=\"javascript:void(0)\"  " +
                                            "onclick=\"postSelelected('" + (noti.AssociatedQbicle != null ? noti.AssociatedQbicle.Id.Encrypt() : "") + "', '" + postCreation?.Id.Encrypt() + "', '" + Enums.QbicleModule.Dashboard + "'); \"> #" + postCreation.JournalEntry?.Number +
                                            "</a></p>";

                                    }
                                    else if (postCreation.BKTransaction != null)
                                    {
                                        _contentHtml = "<p><strong>" + HelperClass.GetFullNameOfUser(noti.CreatedBy)
                                            + $"</strong> has been added a comment to Transaction in Journal Entry " +
                                            "<a href=\"javascript:void(0)\"  " +
                                            "onclick=\"postSelelected('" + (noti.AssociatedQbicle != null ? noti.AssociatedQbicle.Id.Encrypt() : "") + "', '" + postCreation?.Id.Encrypt() + "', '" + Enums.QbicleModule.Dashboard + "'); \"> #" + postCreation.BKTransaction?.JournalEntry?.Number +
                                            "</a></p>";

                                    }
                                    else
                                    {
                                        _contentHtml = "<p><strong>" + HelperClass.GetFullNameOfUser(noti.CreatedBy) + "</strong> added a post to <a href=\"javascript:void(0)\"  onclick=\"postSelelected('" + (noti.AssociatedQbicle != null ? noti.AssociatedQbicle.Id.Encrypt() : "") + "', '" + postCreation?.Id.Encrypt() + "', '" + Enums.QbicleModule.Dashboard + "'); \"> " + postCreation?.Topic?.Qbicle?.Name + "</a></p>";

                                    }
                                    trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_postId\" name=\"notifications[]\"></td>";
                                    trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                    trHtml += $"<td class=\"notification-detail\">{_contentHtml}<small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                    trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                }
                                else
                                {
                                    var functionshow = "";
                                    if (activity != null)
                                    {
                                        if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.AlertActivity)
                                        {
                                            functionshow = "ShowAlertPage('" + activity.Key + "', false); ";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp)
                                        {
                                            functionshow = $"ShowApprovalPage('{activity.Key}', false,'{approvalType}');";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.EventActivity)
                                        {
                                            functionshow = "ShowEventPage('" + activity.Key + "', false);";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity)
                                        {
                                            functionshow = "ShowMediaPage('" + activity.Key + "', false);";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity)
                                        {
                                            functionshow = "ShowTaskPage('" + activity.Key + "', false); ";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.DiscussionActivity)
                                        {
                                            var discussion = activity as B2COrderCreation;
                                            if (discussion == null)
                                                functionshow = "ShowDiscussionPage('" + activity.Key + "', false); ";
                                            else
                                                functionshow = "ShowDiscussionB2CPage('" + activity.Key + "', false); ";
                                        }
                                    }
                                    trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_postId\" name=\"notifications[]\"></td>";
                                    trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";

                                    if (noti.Event == NotificationEventEnum.PostEdit)
                                        trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated a comment to <a href=\"javascript:void(0)\"  onclick=\"{functionshow}\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                    else
                                        trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> added a comment to <a href=\"javascript:void(0)\"  onclick=\"{functionshow}\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";

                                    trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                }
                                break;
                            case NotificationEventEnum.QbicleCreation:
                                var qbicleCreation = noti.AssociatedQbicle;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> created the Qbicle <a href=\"javascript:void(0)\"  onclick=\"MarkAsReadNotification({noti.Id},event);QbicleSelected('{qbicleCreation?.Key}','{Enums.QbicleModule.Dashboard}');\">{qbicleCreation?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + (qbicleCreation != null ? (" / " + qbicleCreation.Name) : ""))}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.QbicleUpdate:
                                var qbicleUpdate = noti.AssociatedQbicle;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated the Qbicle <a href=\"javascript:void(0)\"  onclick=\"MarkAsReadNotification({noti.Id},event);QbicleSelected('{qbicleUpdate?.Key}','{Enums.QbicleModule.Dashboard}');\">{qbicleUpdate?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + (qbicleUpdate != null ? (" / " + qbicleUpdate.Name) : ""))}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.TaskCompletion:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> completed the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.TaskCreation:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> created the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.TaskUpdate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> updated the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.DiscussionCreation:
                                if (activity is QbicleDiscussion)
                                {
                                    var ds = (QbicleDiscussion)activity;
                                    var discussionUrl = GetDiscussionLink(ds);
                                    trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                    trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                    trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> Created the Discussion <a href=\"{discussionUrl}\" onclick=\"MarkAsReadNotification({noti.Id},event);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                    trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                }

                                break;
                            case NotificationEventEnum.DiscussionUpdate:
                                if (activity is QbicleDiscussion)
                                {
                                    var ds = (QbicleDiscussion)activity;
                                    var discussionUrl = GetDiscussionLink(ds);
                                    trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                    trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                    trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> Updated the Discussion <a href=\"{discussionUrl}\" onclick=\"MarkAsReadNotification({noti.Id},event);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                    trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                }

                                break;
                            case NotificationEventEnum.TopicPost:
                                break;
                            case NotificationEventEnum.LinkCreation:
                            case NotificationEventEnum.LinkUpdate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> {(noti.Event == NotificationEventEnum.LinkUpdate ? "Updated" : "Created")} the Discussion <a href=\"javascript:void(0)\" onclick=\"ShowLinkPage('{activity?.Key}',false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ApprovalReviewed:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong>  reviewed the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}','{approvalType}');\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ApprovalApproved:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong>  approved the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}','false','{approvalType}');\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ApprovalDenied:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> denied the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}','false','{approvalType}');\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.RemoveUserOutOfDomain:
                                var domain = noti.AssociatedDomain;
                                var user = noti.AssociatedUser;
                                var name = userId == user.Id ? "You" : HelperClass.GetFullNameOfUser(user);
                                if (domain != null)
                                {
                                    trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_domainId\" name=\"notifications[]\"></td>";
                                    trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                    trHtml += $"<td class=\"notification-detail\"><p><strong>{name}</strong> has removed you from the Domain <strong>{activity?.Name}</strong></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : ""))}</small></td>";
                                    trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                }
                                break;
                            case NotificationEventEnum.ReminderCampaignPost:
                                if (activity is ApprovalReq)
                                {
                                    var app = (ApprovalReq)activity;
                                    var campaignApproval = app.CampaigPostApproval.FirstOrDefault();
                                    var campaignPost = campaignApproval.CampaignPost;
                                    if (app != null)
                                    {
                                        var _urlBg = "";
                                        if (campaignPost.AssociatedCampaign.CampaignType == Qbicles.Models.SalesMkt.CampaignType.Automated)
                                            _urlBg = $"{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}";
                                        else
                                            _urlBg = $"/Content/DesignStyle/img/icon_socialpost_manual.png";
                                        trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                        trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{_urlBg}');\"></div></td>";
                                        trHtml += $"<td class=\"notification-detail\"><p>{campaignPost.Reminder.Content}</p><small>{(noti.CreatedDate.GetTimeRelative() + " in " + activity?.Qbicle?.Domain?.Name + " / " + activity?.Qbicle?.Name)}</small></td>";
                                        trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                    }
                                }

                                break;
                            case NotificationEventEnum.AssignTask:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> assigned you to the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;

                            case NotificationEventEnum.C2CConnectionIssued:
                                var associatedQbicle = noti.AssociatedQbicle;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><a href=\"/Community/UserProfilePage?uId={noti.CreatedBy.Id}\"><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong></a> wants to connect with you.</p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;

                            case NotificationEventEnum.C2CConnectionAccepted:
                                associatedQbicle = noti.AssociatedQbicle;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>You are now connected with <a href=\"/Community/UserProfilePage?uId={noti.CreatedBy.Id}\"><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong></a>.</p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.B2CConnectionCreated:
                                associatedQbicle = noti.AssociatedQbicle;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><a href=\"/Community/UserProfilePage?uId={noti.CreatedBy.Id}\"><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong></a> has connected with you in B2C.</p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ListingInterested:
                                var listingPost = noti.AssociatedHighlight as ListingHighlight;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_listingId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{HelperClass.GetFullNameOfUser(noti.CreatedBy)}</strong> has expressed interest in your <strong>{listingPost.ListingHighlightType.GetDescription()}</strong> listing <strong>{listingPost.Reference}</strong></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : ""))} / Highlight posts</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.RemoveUserOutOfQbicle:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_domainId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{noti.CreatedBy.GetFullName()}</strong> has removed you from the Qbicle <strong>{noti.AssociatedQbicle?.Name}<strong></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : ""))}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.AddUserParticipants:
                            case NotificationEventEnum.RemoveUserParticipants:
                                if (activity is QbicleDiscussion)
                                {
                                    var dsParticipants = (QbicleDiscussion)activity;
                                    var discussionUrl = GetDiscussionLink(dsParticipants);
                                    var actionType = noti.Event == NotificationEventEnum.AddUserParticipants ? "Add you to " : "Remove you from ";
                                    trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_activityId\" name=\"notifications[]\"></td>";
                                    trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                    trHtml += $"<td class=\"notification-detail\"><p><strong>{noti.CreatedBy.GetFullName()}</strong> {actionType} the participants in the discussion <a href=\"{discussionUrl}\" onclick=\"MarkAsReadNotification({noti.Id},event)\">{activity?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName)}</small></td>";
                                    trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                }
                                break;
                            case NotificationEventEnum.AddUserToQbicle:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p><strong>{noti.CreatedBy.GetFullName()}</strong> has added you to Qbicle <a href=\"javascript:void(0)\"  onclick=\"MarkAsReadNotification({noti.Id},event);QbicleSelected('{noti.AssociatedQbicle?.Key}', '{Enums.QbicleModule.Dashboard}'); \">{noti.AssociatedQbicle?.Name}</a></p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : ""))}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ProcessDomainRequest:
                                if (string.IsNullOrEmpty(noti.DomainRequested)) break;
                                var domainRequested = noti.DomainRequested.ParseAs<DomainRequest>();

                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{domainRequested?.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>An administrator has reviewed your request for <strong><a href=\"javascript:void(0)\"  onclick=\"MarkAsReadNotification({noti.Id},event);showDomainRequestHistory();\">{domainRequested?.Name ?? ""}</a></strong> Domain. Tap here to track the progress.</p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + (noti.AssociatedQbicle != null ? (" / " + noti.AssociatedQbicle.Name) : ""))}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ProcessExtensionRequest:
                                var extensionRequested = noti.ExtensionRequest;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{extensionRequested?.Domain.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                if (extensionRequested.Status == ExtensionRequestStatus.Approved)
                                    trHtml += $"<td class=\"notification-detail\"><p>Your requested extension, <strong><a href=\"javascript:void(0)\"  onclick=\"MarkAsReadNotification({noti.Id},event);showDomainExtensionHistoryTab('{extensionRequested.Domain.Key}');\">{(extensionRequested?.Type.GetDescription() ?? "")}</a></strong> is now available for use.</p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + (noti.AssociatedQbicle != null ? (" / " + noti.AssociatedQbicle.Name) : ""))}</small></td>";
                                else if (extensionRequested.Status == ExtensionRequestStatus.Rejected)
                                    trHtml += $"<td class=\"notification-detail\"><p>Your request for <strong><a href=\"javascript:void(0)\"  onclick=\"MarkAsReadNotification({noti.Id},event);showDomainExtensionHistoryTab('{extensionRequested.Domain.Key}');\">{(extensionRequested?.Type.GetDescription() ?? "")}</a></strong> has been rejected. Click here for more information.</p><small>{(noti.CreatedDate.GetTimeRelative() + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + (noti.AssociatedQbicle != null ? (" / " + noti.AssociatedQbicle.Name) : ""))}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;

                            case NotificationEventEnum.DomainSubTrialEnd:
                                var associatedDomain = noti.AssociatedDomain;
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.AssociatedDomain.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>Your trial will expire soon!</p><small>Your free trial period for <strong>{associatedDomain.Name}</strong> expires soon, after which you will be automatically billed the first month's subscription fee. Manage this in <a href=\"/Administration/AdminPermissions\">Domain Adminstration</a>.</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.DomainSubNextPaymentDate:
                                trHtml += "<td style=\"width:30px; \"><input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_cubeId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{noti.AssociatedDomain.LogoUri.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>Reminder of subscription payment</p><small>Your monthly subscription charge for <strong data-renderer-mark=\"true\">{noti.AssociatedDomain.Name}</strong> is due to be paid soon. Track and manage this in <a href=\"/Administration/AdminPermissions\">Domain Adminstration</a>.</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.TaskWorkLog:
                            case NotificationEventEnum.TypingChat:
                            case NotificationEventEnum.EndTypingChat:
                            case NotificationEventEnum.CreateRequest:
                            case NotificationEventEnum.B2COrderUpdated:
                            case NotificationEventEnum.B2COrderInvoiceCreationCompleted:
                            case NotificationEventEnum.B2COrderBeginProcess:
                            case NotificationEventEnum.B2COrderCompleted:
                            case NotificationEventEnum.B2BOrderCompleted:
                            case NotificationEventEnum.B2COrderPaymentApproved:
                            case NotificationEventEnum.EventNotificationPoints:
                            case NotificationEventEnum.TaskNotificationPoints:
                            case NotificationEventEnum.TaskStart:
                            case NotificationEventEnum.TaskComplete:
                            case NotificationEventEnum.RemoveQueue:
                                break;
                            case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                                trHtml += "<td style=\"width:30px; \">" +
                                    "<input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_qbicleinviteId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" " +
                                    $"style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>Congratulations! You have been approved from the Waitlist, and can now begin adding Subscription Domains and Custom Domains.</p><small>{noti.CreatedDate.GetTimeRelative()}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                            case NotificationEventEnum.ApprovalCustomWaitlist:
                                trHtml += "<td style=\"width:30px; \">" +
                                   "<input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_qbicleinviteId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" " +
                                    $"style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>Congratulations! You have been approved from the Waitlist, and can now begin adding Subscription Domains.</p><small>{noti.CreatedDate.GetTimeRelative()}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";

                                break;
                            case NotificationEventEnum.RejectWaitlist:
                                trHtml += "<td style=\"width:30px; \">" +
                                   "<input type=\"checkbox\" class=\"cb-element\" onchange=\"CheckNotifications(this)\" value=\"" + noti.Id + "_qbicleinviteId\" name=\"notifications[]\"></td>";
                                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" " +
                                    $"style=\"background-image: url('{noti.CreatedBy?.ProfilePic.ToUri(Enums.FileTypeEnum.Image, "T")}');\"></div></td>";
                                trHtml += $"<td class=\"notification-detail\"><p>We regret to inform you that your application for access to Subscription Domain creation has been unsuccessful, and you are no longer on the Waitlist. You are welcome to retry, or contact support if you’re stuck.</p><small>{noti.CreatedDate.GetTimeRelative()}</small></td>";
                                trHtml += $"<td style=\"width:30px;\"><button type=\"button\" class=\"btn btn-soft\" onclick=\"MarkAsReadNotification({noti.Id},event)\"><i class=\"fa fa-trash\"></i></button></td>";
                                break;
                            default:
                                break;
                        }
                        trHtml += "</tr>";
                        items.Add(trHtml);
                    }

                }
                else
                {
                    items.Add("<tr><td colspan=\"4\">You have no new messages!</td></tr>");
                }
                paginationResponse.items = items;
                return paginationResponse;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, isRead, pagination);
                paginationResponse.totalNumber = 0;
                var items = new List<string>();
                items.Add("<tr><td colspan=\"4\">Can't load the messages!</td></tr>");
                paginationResponse.items = items;
                return paginationResponse;
            }
        }


        public ReturnJsonModel DeleteAllNotificationByUser(string userId)
        {
            var result = new ReturnJsonModel() { actionVal = 3, result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, userId);

                dbContext.Notifications.Where(p => p.NotifiedUser.Id == userId).ToList()
                                        .ForEach(notiItem =>
                                        {
                                            notiItem.IsRead = true;
                                        });
                dbContext.SaveChanges();
                result.result = true;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                result.result = false;
                result.msg = ResourcesManager._L("ERROR_MSG_5");
                return result;
            }
        }
        #endregion


        /// <summary>
        /// Notification comment on Dashboard
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2PostOnDashboard(SignalRParameter parameter)
        {
            var notifications = CreateNotifications(parameter, parameter.Post, parameter.Qbicle, null);
            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
                switch (notify.SentMethod)
                {
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notify, MapReasonSent(parameter.EventNotify), notify.AssociatedPost?.Message, notify.AssociatedPost?.Topic.Qbicle.Name ?? "");

                        break;
                }
            }
        }

        /// <summary>
        /// Notification when Create or update or close/open q Qbicle
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2AddUpdateQbicle(SignalRParameter parameter)
        {
            var reasonSent = MapReasonSent(parameter.EventNotify);
            var notifications = CreateNotifications(parameter, null, parameter.Qbicle, null);

            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
                switch (notify.SentMethod)
                {
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notify, reasonSent, parameter.Qbicle.Name, parameter.Qbicle.Name);
                        break;
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                }
            }

        }

        /// <summary>
        /// Notification when inviter/create/remove user from domain
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2UserCreateRemoveFromDomain(SignalRParameter parameter)
        {
            var notifications = CreateNotificationNewUser(parameter);

            var reasonSent = MapReasonSent(parameter.EventNotify);

            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
                switch (notify.SentMethod)
                {
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notify, reasonSent, parameter.Domain.Name, "");
                        break;
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                }
            }
        }

        private void Notify2TaskAssignee(SignalRParameter parameter)
        {
            var notification = CreateNotification2TaskAssignee(parameter.Domain, parameter.Activity, parameter.CreatedById, (ApplicationUser)parameter.ActivityBroadcast, parameter.EventNotify, parameter.AppendToPageName);
            notification.OriginatingConnectionId = parameter.OriginatingConnectionId;
            notification.OriginatingCreationId = parameter.OriginatingCreationId;
            var resonSent = MapReasonSent(parameter.EventNotify);

            SendBroadcastNotification(notification);
            switch (notification.SentMethod)
            {
                case NotificationSendMethodEnum.Both:
                case NotificationSendMethodEnum.Email:
                    SendNotificationEmail(parameter.CreatedById, notification, resonSent, parameter.Domain.Name, "");
                    break;
                case NotificationSendMethodEnum.Broadcast:
                    break;
            }
        }

        private void NotifyOnC2CConnectionProcess(SignalRParameter parameter)
        {
            var notification = NotifyOnC2CConnectionProcess(parameter, (ApplicationUser)parameter.ActivityBroadcast);

            var resonSent = MapReasonSent(parameter.EventNotify);
            notification.OriginatingConnectionId = parameter.OriginatingConnectionId;
            notification.OriginatingCreationId = parameter.OriginatingCreationId;
            parameter.QbicleNotification = notification;

            SendBroadcastNotification(notification);
            switch (notification.SentMethod)
            {
                case NotificationSendMethodEnum.Both:
                case NotificationSendMethodEnum.Email:
                    SendNotificationEmail(parameter.CreatedById, notification, resonSent, parameter.Domain.Name, "");
                    break;
                case NotificationSendMethodEnum.Broadcast:
                    break;
            }
        }

        private void NotifyOnB2CConnectionProcess(SignalRParameter parameter, B2BProfile b2bprofile)
        {
            var createdQbicle = dbContext.Qbicles.Find(parameter.CurrentQbicleId);
            var lstNotifications = CreateNotificationOnB2CConnectionCreated(parameter, createdQbicle, b2bprofile.DefaultB2CRelationshipManagers);

            var resonSent = MapReasonSent(parameter.EventNotify);

            foreach (var notification in lstNotifications)
            {
                notification.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notification.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notification);
                switch (notification.SentMethod)
                {
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notification, resonSent, parameter.Domain.Name, "");
                        break;
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                }
            }
        }

        private void NotifyOnFlaggingListing(SignalRParameter parameter)
        {
            var lstNotifications = CreateNotificationOnFlagListing(parameter.HLPost, parameter.CreatedById, parameter.EventNotify);

            var resonSent = MapReasonSent(parameter.EventNotify);

            foreach (var notification in lstNotifications)
            {
                notification.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notification.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notification);
                switch (notification.SentMethod)
                {
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notification, resonSent, parameter.HLPost.Title, "");
                        break;
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                }
            }
        }

        private void NotifyOnB2CProcessForBusiness(SignalRParameter parameter)
        {
            var notification = CreateNotification2TaskAssignee(parameter.Domain, parameter.Activity, parameter.CreatedById, (ApplicationUser)parameter.ActivityBroadcast, parameter.EventNotify, parameter.AppendToPageName);
            notification.OriginatingConnectionId = parameter.OriginatingConnectionId;
            notification.OriginatingCreationId = parameter.OriginatingCreationId;
            var resonSent = MapReasonSent(parameter.EventNotify);
            SendBroadcastNotification(notification);
            switch (notification.SentMethod)
            {
                case NotificationSendMethodEnum.Both:
                case NotificationSendMethodEnum.Email:
                    SendNotificationEmail(parameter.CreatedById, notification, resonSent, parameter.Domain.Name, "");
                    break;
                case NotificationSendMethodEnum.Broadcast:
                    break;
            }
        }

        /// <summary>
        /// Notification when activity create
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2Activity(SignalRParameter parameter)
        {
            var notifications = CreateNotifications(parameter, null, null, parameter.Activity);
            var reasonSent = MapReasonSent(parameter.EventNotify);
            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
                switch (notify.SentMethod)
                {
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notify, reasonSent, parameter.Activity.Name, parameter.Activity.Qbicle.Name);
                        break;
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                }
            }
        }

        /// <summary>
        /// Notification when add comment on a Activity
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2CommentOnActivity(SignalRParameter parameter)
        {
            var notifications = CreateNotificationsCommentOnActivity(parameter);
            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
            }
        }

        /// <summary>
        /// Notification when
        /// Update status approval
        /// Cannot Attend a Event
        /// Close a Task
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2UpdateStatusApprovalCannotAttendEventCloseTask(SignalRParameter parameter)
        {
            var notifications = CreateNotifications(parameter, null, null, parameter.Activity);
            var reasonSent = MapReasonSent(parameter.EventNotify);
            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                parameter.QbicleNotification = notify;
                parameter.AppendToPageId = parameter.Activity.Id;

                SendBroadcastNotification(notify);

                switch (notify.SentMethod)
                {
                    case NotificationSendMethodEnum.Both:
                    case NotificationSendMethodEnum.Email:
                        SendNotificationEmail(parameter.CreatedById, notify, reasonSent, parameter.Activity.Name, parameter.Activity.Qbicle.Name);
                        break;
                    case NotificationSendMethodEnum.Broadcast:
                        break;
                }
            }
        }

        /// <summary>
        /// Notification when add media to a Activity
        /// </summary>
        /// <param name="parameter"></param>
        private void Notify2ActivityMedia(SignalRParameter parameter)
        {
            var notifications = CreateNotifications(parameter, null, null, parameter.Activity);
            //var reasonSent = MapReasonSent(eventNotify);
            foreach (var notify in notifications)
            {
                notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
            }
        }

        /// <summary>
        /// Only send email notification from queue hangfire
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="notifyUsers"></param>
        /// <param name="createBy"></param>
        /// <param name="eventNotify"></param>
        public void NotifyReminderCampaignPost(QbicleActivity activity, List<ApplicationUser> notifyUsers, ApplicationUser createBy, NotificationEventEnum eventNotify)
        {
            var notifications = new List<Notification>();

            var startDate = DateTime.UtcNow;
            foreach (var user in notifyUsers)
            {
                var notify = new Notification
                {
                    CreatedBy = createBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = eventNotify,
                    NotifiedUser = user,
                    AssociatedAcitvity = activity,
                    SentMethod = user.ChosenNotificationMethod,
                    IsRead = false,
                    IsCreatorTheCustomer = activity.IsCreatorTheCustomer,
                };
                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                notifications.Add(notify);
            }

            dbContext.SaveChanges();

            var reasonSent = MapReasonSent(eventNotify);

            foreach (var notify in notifications)
            {
                SendNotificationEmail(createBy.Id, notify, reasonSent, activity.Name, activity.Qbicle?.Name);
            }

        }

        private void SendNotificationEmail(string startedById, Notification notification, ReasonSent sentReason, string activityName, string qbicleName)
        {
            if (startedById == notification.NotifiedUser.Id) return;
            var el = new EmailRules(dbContext);
            var emailLog = el.SendEmailNotification(notification, sentReason, activityName, qbicleName);
            if (emailLog != null)
            {
                notification.EmailSent = emailLog;
                notification.SentDate = DateTime.UtcNow;
            }

            dbContext.Notifications.Attach(notification);
            dbContext.Entry(notification).State = EntityState.Modified;
            dbContext.SaveChanges();
        }

        private void Notify2InvitedQbicle(SignalRParameter parameter)
        {
            SendBroadcastNotification(CreateNotificationsInvitedQbicle(parameter));
        }

        #region Private method

        private List<Notification> CreateNotificationsCommentOnActivity(SignalRParameter parameter)
        {
            var notifications = new List<Notification>();

            List<ApplicationUser> notifyUsers;
            if (parameter.Activity != null)
            {
                var startDate = DateTime.UtcNow;
                var qb = parameter.Activity.Qbicle;

                if (parameter.Activity is ApprovalReq
                ) // Saves the ApprovalReq to the system and notifies the users who have been indicated as Reviewers (location (2)2 in Figure 2) for this type of Approval Request Definition.
                {
                    var approvalRequest = (ApprovalReq)parameter.Activity;
                    notifyUsers = approvalRequest.ActivityMembers.Distinct().ToList();
                    if (approvalRequest.ApprovalRequestDefinition != null)
                    {
                        notifyUsers.AddRange(approvalRequest.ApprovalRequestDefinition.Reviewers);
                        notifyUsers.AddRange(approvalRequest.ApprovalRequestDefinition.Approvers);
                    }
                }
                else
                {
                    notifyUsers = parameter.Activity.ActivityMembers.Distinct().ToList();
                }

                var startBy = parameter.Post.CreatedBy;
                if (notifyUsers.Any())
                    notifyUsers = notifyUsers.Where(p => p.IsEnabled == true).ToList();
                //Get All Member by Qbicle
                var notificeUsersByQBicle = qb.Members.Where(p => p.IsEnabled == true).Distinct().ToList();
                notifyUsers.AddRange(notificeUsersByQBicle);
                //end
                notifyUsers = (from d in notifyUsers select d).Distinct().ToList();
                //fix problems SignalR communications will not occur correctly if the Domain Admin is not a member of the B2C QBicle
                AddDomainAdminsToNotifyUsers(notifyUsers, parameter.Activity.Qbicle.Domain, null, parameter.Activity);
                //end
                TradeOrder tradeOrder = null;
                if (parameter.Activity is B2COrderCreation)
                    tradeOrder = ((B2COrderCreation)parameter.Activity).TradeOrder;
                else if (parameter.Activity is B2BOrderCreation)
                    tradeOrder = ((B2BOrderCreation)parameter.Activity).TradeOrder;

                var creater = notifyUsers.FirstOrDefault(x => x.Email == startBy.Email);
                notifyUsers = notifyUsers.Where(s => s.Id != creater.Id).ToList();
                foreach (var use in notifyUsers)
                {
                    #region Create a Notification is Post on Activity
                    var notify = new Notification
                    {
                        AssociatedPost = parameter.Post,
                        AssociatedAcitvity = parameter.Activity,
                        AssociatedTradeOrder = tradeOrder,
                        CreatedBy = startBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = NotificationEventEnum.ActivityComment,
                        NotifiedUser = use,
                        AssociatedDomain = qb.Domain,
                        AssociatedQbicle = qb,
                        SentMethod = use.ChosenNotificationMethod,
                        AppendToPageName = parameter.AppendToPageName,
                        HasActionToHandle = parameter.HasActionToHandle,
                        IsRead = false,
                        IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false,
                    };
                    dbContext.Notifications.Add(notify);
                    dbContext.Entry(notify).State = EntityState.Added;
                    notifications.Add(notify);
                    #endregion

                    #region Create a Notification updated of parent Activity on Stream
                    //Sent a notification updated for the Activity 
                    var notifyActivityUpdated = new Notification
                    {
                        AssociatedPost = null,
                        AssociatedAcitvity = parameter.Activity,
                        AssociatedTradeOrder = tradeOrder,
                        CreatedBy = startBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = parameter.EventNotify,
                        NotifiedUser = use,
                        AssociatedDomain = qb.Domain,
                        AssociatedQbicle = qb,
                        SentMethod = use.ChosenNotificationMethod,
                        AppendToPageName = ApplicationPageName.Activities,
                        HasActionToHandle = parameter.HasActionToHandle,
                        IsRead = true,
                        IsCreatorTheCustomer = parameter.Activity.IsCreatorTheCustomer,
                    };
                    dbContext.Notifications.Add(notifyActivityUpdated);
                    dbContext.Entry(notifyActivityUpdated).State = EntityState.Added;
                    //isread = true -> do not notification
                    notifications.Add(notifyActivityUpdated);
                    #endregion
                }
                if (creater != null)
                {
                    var notifyCreater = new Notification
                    {
                        AssociatedPost = parameter.Post,
                        AssociatedAcitvity = parameter.Activity,
                        AssociatedTradeOrder = tradeOrder,
                        CreatedBy = startBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = parameter.EventNotify,
                        NotifiedUser = creater,
                        AssociatedDomain = qb.Domain,
                        AssociatedQbicle = qb,
                        AppendToPageName = parameter.AppendToPageName,
                        SentMethod = NotificationSendMethodEnum.Broadcast,
                        HasActionToHandle = parameter.HasActionToHandle,
                        IsRead = true,
                        IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
                    };
                    dbContext.Notifications.Add(notifyCreater);
                    dbContext.Entry(notifyCreater).State = EntityState.Added;
                    //isread = true -> do not notification
                    notifications.Add(notifyCreater);
                }
                //Save Notify for Ativity
                dbContext.SaveChanges();
            }

            return notifications;
        }

        /// <summary>
        /// Create notification db
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="post"></param>
        /// <param name="cube"></param>
        /// <param name="activity"></param>
        /// <returns></returns>
        private List<Notification> CreateNotifications(SignalRParameter parameter,
            QbiclePost post, Qbicle cube, QbicleActivity activity)
        {
            var notifications = new List<Notification>();

            var domain = new QbicleDomain();
            var notification2Users = new List<ApplicationUser>();
            var createdBy = dbContext.QbicleUser.Find(parameter.CreatedById);
            var startDate = DateTime.UtcNow;

            if (post != null)
            {
                notification2Users = post.Topic.Qbicle.Members.Where(p => p.IsEnabled == true).Distinct().ToList();
                domain = post.Topic.Qbicle.Domain;
            }
            else if (cube != null)
            {
                notification2Users = cube.Members.Where(p => p.IsEnabled == true).Distinct().ToList();
                domain = cube.Domain;
            }
            else if (activity != null)
            {
                if (activity is ApprovalReq) // Saves the ApprovalReq to the system and notifies the users who have been indicated as Reviewers (location (2)2 in Figure 2) for this type of Approval Request Definition.
                {
                    var approvalRequest = (ApprovalReq)activity;
                    notification2Users = approvalRequest.ActivityMembers.Where(p => p.IsEnabled == true).Distinct().ToList();
                    if (approvalRequest.ApprovalRequestDefinition != null)
                    {
                        notification2Users.AddRange(approvalRequest.ApprovalRequestDefinition.Reviewers.Where(p => p.IsEnabled == true));
                        notification2Users.AddRange(approvalRequest.ApprovalRequestDefinition.Approvers.Where(p => p.IsEnabled == true));
                    }
                    //Add in the QBicle members to ensure that they get their pages updated.
                    notification2Users = notification2Users.Concat(activity.Qbicle.Members).ToList();
                }
                else
                {
                    notification2Users = activity.ActivityMembers.Where(p => p.IsEnabled == true).Distinct().ToList();
                    //Add in the QBicle members to ensure that they get their pages updated.
                    notification2Users = notification2Users.Concat(activity.Qbicle.Members).ToList();
                }

                if (activity is QbicleMedia)
                    notification2Users.AddRange(activity.Qbicle.Members.Where(p => p.IsEnabled == true).Distinct().ToList());
                domain = activity.Qbicle.Domain;
                cube = activity.Qbicle;
            }

            notification2Users = (from d in notification2Users select d).Distinct().ToList();
            if (notification2Users.Count == 0)
                return notifications;
            // fix problems SignalR communications will not occur correctly if the Domain Admin is not a member of the B2C QBicle
            AddDomainAdminsToNotifyUsers(notification2Users, domain, post, activity);

            TradeOrder tradeOrder = null;
            if (parameter.EventNotify == NotificationEventEnum.DiscussionCreation || parameter.EventNotify == NotificationEventEnum.DiscussionUpdate
                || parameter.EventNotify == NotificationEventEnum.B2COrderUpdated)
            {
                if (activity is B2COrderCreation)
                {
                    tradeOrder = ((B2COrderCreation)activity).TradeOrder;
                }
                else if (activity is B2BOrderCreation) 
                { 
                    tradeOrder = ((B2BOrderCreation)activity).TradeOrder;
                }
            }
            foreach (var user in notification2Users)
            {
                var notify = new Notification
                {
                    AssociatedDomain = domain,
                    AssociatedQbicle = cube,
                    AssociatedTradeOrder = tradeOrder,
                    AssociatedPost = post,
                    AssociatedAcitvity = activity,
                    CreatedBy = createdBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = parameter.EventNotify,
                    NotifiedUser = user,
                    SentMethod = user.ChosenNotificationMethod,
                    IsRead = user.Id == createdBy.Id,
                    AppendToPageName = parameter.AppendToPageName,
                    IsCreatorTheCustomer = post?.IsCreatorTheCustomer ?? activity?.IsCreatorTheCustomer ?? false,
                };

                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                //if (notify.IsRead == false)
                notifications.Add(notify);
            }

            dbContext.SaveChanges();
            return notifications;

        }

        private ReasonSent MapReasonSent(NotificationEventEnum eventEnum)
        {
            switch (eventEnum)
            {
                case NotificationEventEnum.QbicleCreation:
                    return ReasonSent.QbicleCreation;
                case NotificationEventEnum.QbicleUpdate:
                    return ReasonSent.QbicleUpdate;
                case NotificationEventEnum.DiscussionCreation:
                    return ReasonSent.DiscussionCreation;
                case NotificationEventEnum.DiscussionUpdate:
                    return ReasonSent.DiscussionUpdate;
                case NotificationEventEnum.TaskCreation:
                    return ReasonSent.TaskCreation;
                case NotificationEventEnum.TaskCompletion:
                    return ReasonSent.TaskCompletion;
                case NotificationEventEnum.AlertCreation:
                    return ReasonSent.AlertCreation;
                case NotificationEventEnum.EventCreation:
                    return ReasonSent.EventCreation;
                case NotificationEventEnum.EventWithdrawl:
                    return ReasonSent.EventWithdrawl;
                case NotificationEventEnum.TopicPost:
                    return ReasonSent.TopicPost;
                case NotificationEventEnum.PostCreation:
                    return ReasonSent.PostCreation;
                case NotificationEventEnum.PostEdit:
                    return ReasonSent.PostEdit;
                case NotificationEventEnum.ApprovalCreation:
                    return ReasonSent.ApprovalCreation;
                case NotificationEventEnum.CreateMember:
                    return ReasonSent.CreateMember;
                case NotificationEventEnum.InvitedMember:
                    return ReasonSent.InvitedMember;
                case NotificationEventEnum.AlertUpdate:
                    return ReasonSent.AlertUpdate;
                case NotificationEventEnum.TaskUpdate:
                    return ReasonSent.TaskUpdate;
                case NotificationEventEnum.EventUpdate:
                    return ReasonSent.EventUpdate;
                case NotificationEventEnum.LinkCreation:
                    return ReasonSent.LinkCreation;
                case NotificationEventEnum.ApprovalUpdate:
                    return ReasonSent.ApprovalUpdate;
                case NotificationEventEnum.MediaCreation:
                    return ReasonSent.MediaCreation;
                case NotificationEventEnum.MediaUpdate:
                    return ReasonSent.MediaUpdate;
                case NotificationEventEnum.MediaRemoveVersion:
                    return ReasonSent.MediaRemoveVersion;
                case NotificationEventEnum.MediaAddVersion:
                    return ReasonSent.MediaAddVersion;
                case NotificationEventEnum.ApprovalReviewed:
                    return ReasonSent.ApprovalReviewed;
                case NotificationEventEnum.ApprovalApproved:
                    return ReasonSent.ApprovalApproved;
                case NotificationEventEnum.ApprovalDenied:
                    return ReasonSent.ApprovalDenied;
                case NotificationEventEnum.JournalPost:
                    return ReasonSent.JournalPost;
                case NotificationEventEnum.TransactionPost:
                    return ReasonSent.TransactionPost;
                case NotificationEventEnum.RemoveUserOutOfDomain:
                    return ReasonSent.RemoveUserOutOfDomain;
                case NotificationEventEnum.ReminderCampaignPost:
                    return ReasonSent.ReminderCampaignPost;
                case NotificationEventEnum.AssignTask:
                    return ReasonSent.AssignTask;
                case NotificationEventEnum.C2CConnectionIssued:
                    return ReasonSent.CreateC2CConnectionRequest;
                case NotificationEventEnum.C2CConnectionAccepted:
                    return ReasonSent.AcceptC2CConnectionRequest;
                case NotificationEventEnum.B2CConnectionCreated:
                    return ReasonSent.CreateB2CConnection;
                case NotificationEventEnum.ListingInterested:
                    return ReasonSent.FlagListingPost;
                case NotificationEventEnum.LinkUpdate:
                    return ReasonSent.LinkCreation;
                case NotificationEventEnum.ActivityComment:
                    return ReasonSent.ActivityComment;
                case NotificationEventEnum.RemoveUserOutOfQbicle:
                    return ReasonSent.RemoveUserOutOfQbicle;
                case NotificationEventEnum.AddUserParticipants:
                    return ReasonSent.AddUserParticipants;
                case NotificationEventEnum.RemoveUserParticipants:
                    return ReasonSent.RemoveUserParticipants;
                case NotificationEventEnum.RemoveQueue:
                    return ReasonSent.RemoveQueue;
                case NotificationEventEnum.QbicleInvited:
                    return ReasonSent.QbicleInvited;
                case NotificationEventEnum.AddUserToQbicle:
                    return ReasonSent.QbicleInvited;
                case NotificationEventEnum.ProcessDomainRequest:
                    return ReasonSent.ProcessDomainRequest;
                case NotificationEventEnum.EventNotificationPoints:
                    return ReasonSent.EventNotificationPoints;
                case NotificationEventEnum.TaskNotificationPoints:
                    return ReasonSent.TaskNotificationPoints;
                case NotificationEventEnum.TaskComplete:
                    return ReasonSent.TaskComplete;
                case NotificationEventEnum.TaskStart:
                    return ReasonSent.TaskStart;
                default:
                    return ReasonSent.QbicleCreation;
            }
        }

        private List<Notification> CreateNotificationNewUser(SignalRParameter parameter)
        {

            var userNew = (ApplicationUser)parameter.ActivityBroadcast;

            var notifications = new List<Notification>();

            var startDate = DateTime.UtcNow;
            var listUserForNotify = parameter.Domain.Users;
            listUserForNotify = listUserForNotify.Where(u => u.Id != parameter.CreatedById).ToList();
            var createdBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == parameter.CreatedById);
            foreach (var user in listUserForNotify)
            {
                var notify = new Notification
                {
                    CreatedBy = createdBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = parameter.EventNotify,
                    NotifiedUser = user,
                    AssociatedDomain = parameter.Domain,
                    AssociatedUser = userNew,
                    SentMethod = user.ChosenNotificationMethod,
                    AssociatedQbicle = dbContext.Qbicles.Find(parameter.CurrentQbicleId),
                    IsRead = false,
                    AppendToPageName = parameter.AppendToPageName,
                    IsCreatorTheCustomer = false
                };

                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                notifications.Add(notify);
            }
            if (listUserForNotify.Any(u => u.Id != userNew.Id)
                &&
                (parameter.EventNotify == NotificationEventEnum.RemoveUserOutOfDomain
                || parameter.EventNotify == NotificationEventEnum.RemoveUserOutOfQbicle
                ))
            {
                var notify = new Notification
                {
                    CreatedBy = createdBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = parameter.EventNotify,
                    NotifiedUser = userNew,
                    AssociatedDomain = parameter.Domain,
                    AssociatedUser = userNew,
                    SentMethod = userNew.ChosenNotificationMethod,
                    AssociatedQbicle = dbContext.Qbicles.Find(parameter.CurrentQbicleId),
                    IsRead = false,
                    AppendToPageName = parameter.AppendToPageName,
                    IsCreatorTheCustomer = false,
                };
                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                notifications.Add(notify);
            }
            dbContext.SaveChanges();
            return notifications;

        }

        private Notification CreateNotification2TaskAssignee(QbicleDomain domain, QbicleActivity associatedTask, string createById,
            ApplicationUser taskAssignee, NotificationEventEnum ev, ApplicationPageName appendToPageName)
        {
            var startDate = DateTime.UtcNow;

            var notify = new Notification
            {
                CreatedBy = dbContext.QbicleUser.FirstOrDefault(e => e.Id == createById),
                CreatedDate = startDate,
                SentDate = startDate,
                Event = ev,
                NotifiedUser = taskAssignee,
                AssociatedDomain = domain,
                AssociatedUser = taskAssignee,
                SentMethod = taskAssignee.ChosenNotificationMethod,
                IsRead = false,
                AssociatedAcitvity = associatedTask,
                AppendToPageName = appendToPageName,
                IsCreatorTheCustomer = false
            };
            dbContext.Notifications.Add(notify);
            dbContext.Entry(notify).State = EntityState.Added;
            dbContext.SaveChanges();
            return notify;
        }

        private Notification NotifyOnC2CConnectionProcess(SignalRParameter parameter, ApplicationUser userReceived)
        {
            var startDate = DateTime.UtcNow;

            var notify = new Notification
            {
                CreatedBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == parameter.CreatedById),
                CreatedDate = startDate,
                SentDate = startDate,
                Event = parameter.EventNotify,
                NotifiedUser = userReceived,
                AssociatedDomain = parameter.Domain,
                AssociatedUser = userReceived,
                SentMethod = userReceived.ChosenNotificationMethod,
                IsRead = false,
                AssociatedQbicle = parameter.Qbicle,
                AppendToPageName = parameter.AppendToPageName,
                IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
            };
            dbContext.Notifications.Add(notify);
            dbContext.Entry(notify).State = EntityState.Added;
            dbContext.SaveChanges();
            return notify;
        }

        private List<Notification> CreateNotificationOnB2CConnectionCreated(SignalRParameter parameter, Qbicle associatedQbicle, List<ApplicationUser> notiReceivers)
        {
            var lstNotifications = new List<Notification>();
            var startDate = DateTime.UtcNow;
            foreach (var receiver in notiReceivers)
            {
                var notification = new Notification
                {
                    CreatedBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == parameter.CreatedById),
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = parameter.EventNotify,
                    NotifiedUser = receiver,
                    AssociatedDomain = parameter.Domain,
                    AssociatedUser = receiver,
                    SentMethod = receiver.ChosenNotificationMethod,
                    IsRead = false,
                    AssociatedQbicle = associatedQbicle,
                    AppendToPageName = parameter.AppendToPageName,
                    IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
                };
                dbContext.Notifications.Add(notification);
                dbContext.Entry(notification).State = EntityState.Added;
                dbContext.SaveChanges();
                lstNotifications.Add(notification);

            }
            return lstNotifications;
        }

        private List<Notification> CreateNotificationOnFlagListing(HighlightPost associatedHighlight, string createdById, NotificationEventEnum ev)
        {
            //Listing Domain
            var hlDomain = associatedHighlight.Domain;
            //B2C Relationship Managers in Listing Domain
            var lstDomainMemIds = hlDomain.Users.Select(p => p.Id).Distinct().ToList();
            var lstManagers = dbContext.B2CQbicles.Where(p => p.Business.Id == hlDomain.Id).SelectMany(p => p.Members).Where(p => lstDomainMemIds.Contains(p.Id) && p.Id != createdById).Distinct().ToList();
            //Get List receivers
            var lstReceiver = new List<ApplicationUser>();
            if (lstManagers != null && lstManagers.Count > 0)
            {
                lstReceiver.AddRange(lstManagers);
            }
            else
            {
                lstReceiver.Add(associatedHighlight.CreatedBy);
            }
            var createdBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == createdById);
            var lstNotifications = new List<Notification>();
            var startDate = DateTime.UtcNow;
            foreach (var receiver in lstManagers)
            {
                var noti = new Notification
                {
                    CreatedBy = createdBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = ev,
                    NotifiedUser = receiver,
                    AssociatedUser = receiver,
                    SentMethod = receiver.ChosenNotificationMethod,
                    IsRead = false,
                    AssociatedHighlight = associatedHighlight,
                    AppendToPageName = ApplicationPageName.Qbicle,
                    IsCreatorTheCustomer = false,
                };
                dbContext.Notifications.Add(noti);
                dbContext.Entry(noti).State = EntityState.Added;
                dbContext.SaveChanges();
                lstNotifications.Add(noti);
            }
            return lstNotifications;
        }

        private Notification CreateNotificationsInvitedQbicle(SignalRParameter invited)
        {
            var notify = new Notification
            {
                OriginatingConnectionId = invited.OriginatingConnectionId,
                CreatedBy = dbContext.QbicleUser.Find(invited.CreatedById),
                CreatedDate = DateTime.UtcNow,
                SentDate = DateTime.UtcNow,
                Event = invited.EventNotify,
                NotifiedUser = dbContext.QbicleUser.FirstOrDefault(u => u.Email == invited.CreatedByName),
                AssociateInvitation = dbContext.Invitations.Find(invited.CurrentQbicleId),
                AssociatedDomain = dbContext.Domains.Find(invited.Domain.Id),
                IsRead = false,
                IsCreatorTheCustomer = false
            };

            dbContext.Notifications.Add(notify);
            dbContext.Entry(notify).State = EntityState.Added;

            dbContext.SaveChanges();
            return notify;

        }
        #endregion



        #region Notification on Stream Activity

        /// <summary>
        /// Call from Rules
        /// Send Activity created into HangFire schedule
        /// </summary>
        /// <param name="activityNotification"></param>
        public void Notification2Activity(ActivityNotification activityNotification)
        {
            try
            {

                var job = new QbicleJobParameter
                {
                    EndPointName = "scheduleqbicleactivity",
                    ActivityNotification = activityNotification
                };
                //execute SignalR2Activity
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }

        /// <summary>
        /// HangFire execute queue Activity
        /// </summary>
        /// <param name="job"></param>
        public void SignalR2Activity(QbicleJobParameter job)
        {
            try
            {

                var activity = dbContext.Activities.Find(job.ActivityNotification.Id);


                if (activity == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {job.ActivityNotification.Id}. Can not execute queue of Activity.", null, job);
                    return;
                }

                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = activity,
                    ActivityBroadcast = job.ActivityNotification.ActivityBroadcast,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CurrentQbicleId = activity.Qbicle?.Id ?? 0,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    AppendToPageId = job.ActivityNotification.AppendToPageId,
                    ObjectById = job.ActivityNotification.ObjectById
                };
                Notify2Activity(parameter);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }
        }



        #endregion

        #region Notification add comment on Activity
        /// <summary>
        /// Call from rule
        /// Send comment on activity to HangFire schedule
        /// </summary>
        /// <param name="activityNotification"></param>
        public void NotificationComment2Activity(ActivityNotification activityNotification, bool sendToQueue = true)
        {

            if (sendToQueue)
            {

                var job = new QbicleJobParameter
                {
                    EndPointName = "schedulecommentonactivity",
                    ActivityNotification = activityNotification,

                };
                //execute SignalRComment2Activity
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
                return;
            }

            SignalRComment2Activity(activityNotification);


        }

        /// <summary>
        /// HangFire execute queue Activity
        /// </summary>
        /// <param name="job"></param>
        public void SignalRComment2Activity(ActivityNotification activityNotification)
        {
            try
            {
                var post = dbContext.Posts.Find(activityNotification.PostId);
                if (activityNotification.AppendToPageName == ApplicationPageName.Activities || activityNotification.EventNotify == NotificationEventEnum.TransactionPost)
                {
                    var parameter = new SignalRParameter
                    {
                        OriginatingConnectionId = activityNotification.OriginatingConnectionId,
                        OriginatingCreationId = activityNotification.OriginatingCreationId,
                        Activity = null,
                        Post = post,
                        //ActivityBroadcast = job.ActivityNotification.ActivityBroadcast,
                        EventNotify = activityNotification.EventNotify,
                        CreatedById = post?.CreatedBy.Id,
                        CurrentQbicleId = post != null && post.Topic != null ? post.Topic.Qbicle.Id : 0,
                        CreatedByName = activityNotification.CreatedByName,
                        AppendToPageName = activityNotification.AppendToPageName,
                        AppendToPageId = activityNotification.AppendToPageId,
                        Qbicle = post.Topic.Qbicle,
                        HasActionToHandle = activityNotification.HasActionToHandle
                    };
                    Notify2PostOnDashboard(parameter);
                }
                else
                {
                    var activity = dbContext.Activities.Find(activityNotification.Id);
                    if (activity == null)
                    {
                        LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {activityNotification.Id}. Can not execute queue of Activity.", null, activityNotification);
                        return;
                    }
                    var parameter = new SignalRParameter
                    {
                        OriginatingConnectionId = activityNotification.OriginatingConnectionId,
                        OriginatingCreationId = activityNotification.OriginatingCreationId,
                        Activity = activity,
                        Post = post,
                        //ActivityBroadcast = job.ActivityNotification.ActivityBroadcast,
                        EventNotify = activityNotification.EventNotify,
                        CreatedById = post?.CreatedBy.Id,
                        CurrentQbicleId = activity.Qbicle.Id,
                        CreatedByName = activityNotification.CreatedByName,
                        AppendToPageName = activityNotification.AppendToPageName,
                        AppendToPageId = activityNotification.AppendToPageId,
                        HasActionToHandle = activityNotification.HasActionToHandle,
                    };
                    Notify2CommentOnActivity(parameter);
                }

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }


        #endregion

        #region Notification action on Qbicle

        /// <summary>
        /// Call from rule
        /// Qbicles notification to HangFire schedule
        /// </summary>
        /// <param name="activityNotification"></param>
        public void Notification2Qbicle(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "scheduleonqbicle",
                ActivityNotification = activityNotification
            };
            //execute SignalRComment2Activity
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        /// <summary>
        /// HangFire execute queue Qbicles notification
        /// </summary>
        /// <param name="job"></param>
        public void SignalR2Qbicle(QbicleJobParameter job)
        {
            try
            {

                var qbicle = dbContext.Qbicles.Find(job.ActivityNotification.Id);


                if (qbicle == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found qbicle id {job.ActivityNotification.Id}. Can not execute queue of qbicle.", null, job);
                    return;
                }

                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    Qbicle = qbicle,
                    CurrentQbicleId = qbicle.Id
                };

                Notify2AddUpdateQbicle(parameter);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }
        public void SignalR2InvitedQbicle(QbicleJobParameter job)
        {
            try
            {
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    Domain = dbContext.Domains.Find(job.ActivityNotification.DomainId),
                    CurrentQbicleId = job.ActivityNotification.Id
                };

                Notify2InvitedQbicle(parameter);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }

        #endregion

        #region Notification on Media upload

        /// <summary>
        /// Call from Rules
        /// Media upload into HangFire schedule
        /// </summary>
        /// <param name="activityNotification"></param>
        public void Notification2MediaUpload(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "schedulemediaupload",
                ActivityNotification = activityNotification
            };
            //execute SignalR2Activity
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        /// <summary>
        /// HangFire execute queue Media upload
        /// </summary>
        /// <param name="job"></param>
        public void SignalRMediaUpload(QbicleJobParameter job)
        {
            try
            {
                var activity = dbContext.Activities.Find(job.ActivityNotification.Id);
                if (activity == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {job.ActivityNotification.Id}. Can not execute queue of Activity.", null, job);
                    return;
                }

                var mediaId = int.Parse(job.ActivityNotification.ObjectById);
                var media = activity.SubActivities.FirstOrDefault(e => e.Id == mediaId) ?? activity;

                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = media,
                    AppendToPageId = mediaId,
                    ActivityBroadcast = null,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CurrentQbicleId = activity.Qbicle.Id,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,

                };
                Notify2ActivityMedia(parameter);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }



        #endregion

        #region Notification on Update Status Approval/ Cannot Attend Event/ Close Task

        /// <summary>
        /// Call from Rules Notify2UserCreateRemoveFromDomain
        /// Notification on Update Status Approval/ Cannot Attend Event/ Close Task
        /// </summary>
        /// <param name="activityNotification"></param>
        public void Notification2UpdateStatusApprovalCannotAttendEventCloseTask(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "updatestatusapprovalcannotattendeventclosetask",
                ActivityNotification = activityNotification
            };
            //execute SignalRUpdateStatusApprovalCannotAttendEventCloseTask
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        /// <summary>
        /// HangFire execute queue Notification on Update Status Approval/ Cannot Attend Event/ Close Task
        /// </summary>
        /// <param name="job"></param>
        public void SignalRUpdateStatusApprovalCannotAttendEventCloseTask(QbicleJobParameter job)
        {
            try
            {

                var activity = dbContext.Activities.Find(job.ActivityNotification.Id);


                if (activity == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {job.ActivityNotification.Id}. Can not execute queue of Activity.", null, job);
                    return;
                }
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = activity,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    CurrentQbicleId = activity.Qbicle.Id,
                    AppendToPageName = job.ActivityNotification.AppendToPageName
                };

                Notify2UpdateStatusApprovalCannotAttendEventCloseTask(parameter);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }



        #endregion

        #region Notification on Notify2UserCreateRemoveFromDomain


        public void Notification2UserCreateRemoveFromDomain(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "notification2usercreateremovefromdomain",
                ActivityNotification = activityNotification
            };
            //execute SignalRUpdateStatusApprovalCannotAttendEventCloseTask
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }


        public void SignalRUserCreateRemoveFromDomain(QbicleJobParameter job)
        {
            try
            {
                var associatedUser = new UserRules(dbContext).GetById(job.ActivityNotification.ObjectById);


                if (associatedUser == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {job.ActivityNotification.Id}. Can not execute queue of Activity.", null, job);
                    return;
                }
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    EventNotify = job.ActivityNotification.EventNotify,
                    ActivityBroadcast = associatedUser,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    Domain = new DomainRules(dbContext).GetDomainById(job.ActivityNotification.DomainId),// activity.CurrentDomain,
                    CurrentQbicleId = job.ActivityNotification.QbicleId
                };
                Notify2UserCreateRemoveFromDomain(parameter);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }

        }



        #endregion

        #region Notification invite qbicles
        public void Notification2InvitedQbicle(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "scheduleoninvitedqbicle",
                ActivityNotification = activityNotification
            };
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }
        #endregion

        #region Notification on Assign an user to a Task - Notification2TasAssignee
        public void Notification2TaskAssignee(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "notification2taskassignee",
                ActivityNotification = activityNotification
            };
            var jobExected = new QbiclesJob().HangFireExcecuteAsync(job);
        }

        /// <summary>
        /// HangFire execute queue Notification on Assign Task to User
        /// </summary>
        /// <param name="job"></param>
        public void SignalRAssignTask(QbicleJobParameter job)
        {
            try
            {
                var assignedUser = new UserRules(dbContext).GetById(job.ActivityNotification.ObjectById);
                var activity = dbContext.QbicleTasks.Find(job.ActivityNotification.Id);

                if (assignedUser == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {job.ActivityNotification.Id}. Can not execute queue of Activity.", null, job);
                    return;
                }
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    EventNotify = job.ActivityNotification.EventNotify,
                    ActivityBroadcast = assignedUser,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    Domain = new DomainRules(dbContext).GetDomainById(job.ActivityNotification.DomainId),// activity.CurrentDomain,
                    CurrentQbicleId = job.ActivityNotification.QbicleId,
                    Activity = activity
                };
                Notify2TaskAssignee(parameter);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }

        }
        #endregion

        #region Get discussion link 
        private string GetDiscussionLink(QbicleDiscussion discussion)
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
                    link = "/Qbicles/DiscussionQbicle?disKey=" + discussion.Key;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion:
                    link = "/Operator/DiscussionGoal?disId=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion:
                    link = "/Operator/DiscussionComplianceTask?disId=" + discussion.Id;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.CashManagement:
                    link = "/CashManagement/DiscussionCashManagementShow?disKey=" + discussion.Key;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2CProductMenu:
                    link = "/B2C/DiscussionMenu?disKey=" + discussion.Key;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2COrder:
                    link = "/B2C/DiscussionOrder?disKey=" + discussion.Key;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2BOrder:
                    link = "/Commerce/DiscussionOrder?disKey=" + discussion.Key;
                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2BPartnershipDiscussion:
                    var b2BPartnershipDiscussion = discussion as B2BPartnershipDiscussion;
                    if (b2BPartnershipDiscussion != null)
                        link = "/Commerce/DiscussionPartner?rlid=" + (b2BPartnershipDiscussion.Relationship?.Id ?? 0);
                    break;
                default:
                    break;
            }
            return link;
        }

        private string GetDiscussionType(QbicleDiscussion.DiscussionTypeEnum type, string action)
        {

            switch (type)
            {
                case QbicleDiscussion.DiscussionTypeEnum.IdeaDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.PlaceDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.QbicleDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.PosOrderDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.PerformanceDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.CashManagement:
                    return $"{action} discussion";
                case QbicleDiscussion.DiscussionTypeEnum.B2CProductMenu:
                    return $"had added a message to";
                case QbicleDiscussion.DiscussionTypeEnum.B2COrder:
                case QbicleDiscussion.DiscussionTypeEnum.B2BPartnershipDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.B2BOrder:
                case QbicleDiscussion.DiscussionTypeEnum.B2BCatalogDiscussion:
                    return $"{action} order";
                case QbicleDiscussion.DiscussionTypeEnum.OrderCancellation:
                    return "Canceled an order";
                default:
                    return "";
            }
        }

        private NavigateEnum GetDiscussionNavigate(QbicleDiscussion discussion)
        {
            switch (discussion.DiscussionType)
            {
                case QbicleDiscussion.DiscussionTypeEnum.IdeaDiscussion:
                    return NavigateEnum.IdeaDiscussion;

                case QbicleDiscussion.DiscussionTypeEnum.PlaceDiscussion:
                    return NavigateEnum.PlaceDiscussion;
                case QbicleDiscussion.DiscussionTypeEnum.QbicleDiscussion:
                    return NavigateEnum.Discussion;
                case QbicleDiscussion.DiscussionTypeEnum.PosOrderDiscussion:
                    return NavigateEnum.PosOrderDiscussion;
                case QbicleDiscussion.DiscussionTypeEnum.PerformanceDiscussion:
                    return NavigateEnum.PerformanceDiscussion;
                case QbicleDiscussion.DiscussionTypeEnum.OrderCancellation:
                    return NavigateEnum.OrderCancellation;
                case QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion:
                    return NavigateEnum.GoalDiscussion;
                case QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion:
                case QbicleDiscussion.DiscussionTypeEnum.CashManagement:

                    break;
                case QbicleDiscussion.DiscussionTypeEnum.B2CProductMenu:
                    return NavigateEnum.B2COrder;
                case QbicleDiscussion.DiscussionTypeEnum.B2COrder:
                    return NavigateEnum.B2COrder;
                case QbicleDiscussion.DiscussionTypeEnum.B2BOrder:
                    return NavigateEnum.B2BOrder;
                case QbicleDiscussion.DiscussionTypeEnum.B2BPartnershipDiscussion:
                    var b2BPartnershipDiscussion = discussion as B2BPartnershipDiscussion;
                    if (b2BPartnershipDiscussion != null)
                        return NavigateEnum.B2BPartnershipDiscussion;
                    break;
            }
            return NavigateEnum.Discussion;
        }
        #endregion

        #region Add users is Domain Admin into list notification Users
        private void AddDomainAdminsToNotifyUsers(List<ApplicationUser> notifyUsers, QbicleDomain domain, QbiclePost post, QbicleActivity activity)
        {
            try
            {
                //SignalR communications will not occur correctly if the Domain Admin is not a member of the B2C QBicle
                if (domain != null && domain.Name == SystemDomainConst.BUSINESS2CUSTOMER)
                {
                    QbicleDomain businessDomain = null;
                    if (post != null)
                    {
                        var b2cQbicle = post.Topic.Qbicle as B2CQbicle;
                        businessDomain = b2cQbicle.Business;
                    }
                    if (activity != null)
                    {
                        var b2cQbicle = activity.Qbicle as B2CQbicle;
                        businessDomain = b2cQbicle.Business;
                    }
                    if (businessDomain != null)
                    {
                        foreach (var user in businessDomain.Administrators)
                        {
                            if (!notifyUsers.Any(s => s.Id == user.Id))
                                notifyUsers.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, notifyUsers, domain, post, activity);
            }

        }
        #endregion

        #region Send notification for C2C processes
        /// <summary>
        /// Call to API to run Hangfire job to create C2C Connection Issued notification
        /// </summary>
        /// <param name="activityNotification"></param>
        public void NotificationOnC2CConnectionIssued(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "notificationonc2cconnectionissued",
                ActivityNotification = activityNotification
            };
            var jobExected = new QbiclesJob().HangFireExcecuteAsync(job);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        public void SignalRC2CConnection(QbicleJobParameter job)
        {
            try
            {
                var requestedUser = new UserRules(dbContext).GetById(job.ActivityNotification.ObjectById);
                var qbicle = dbContext.Qbicles.Find(job.ActivityNotification.Id);

                if (requestedUser == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found user id {job.ActivityNotification.ObjectById}. Can not execute queue of connect to qbicle.", null, job);
                    return;
                }
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    EventNotify = job.ActivityNotification.EventNotify,
                    ActivityBroadcast = requestedUser, //User that will receive notification
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    Domain = new DomainRules(dbContext).GetDomainById(job.ActivityNotification.DomainId),// activity.CurrentDomain,
                    CurrentQbicleId = job.ActivityNotification.QbicleId,
                    Qbicle = qbicle
                };
                NotifyOnC2CConnectionProcess(parameter);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }

        }

        public void SignalRB2CConnection(QbicleJobParameter job)
        {
            try
            {
                var requestedUser = new UserRules(dbContext).GetById(job.ActivityNotification.ObjectById);
                var qbicle = dbContext.Qbicles.Find(job.ActivityNotification.QbicleId);
                var b2bProfile = dbContext.B2BProfiles.Find(job.ActivityNotification.Id);

                if (qbicle == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found qbicle id {job.ActivityNotification.QbicleId}. Can not execute queue of qbicle.", null, job);
                    return;
                }
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    EventNotify = job.ActivityNotification.EventNotify,
                    ActivityBroadcast = requestedUser, //User that will receive notification
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    Domain = new DomainRules(dbContext).GetDomainById(job.ActivityNotification.DomainId),// activity.CurrentDomain,
                    CurrentQbicleId = job.ActivityNotification.QbicleId,
                    Qbicle = qbicle
                };
                NotifyOnB2CConnectionProcess(parameter, b2bProfile);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }

        }

        public void FlagListing(QbicleJobParameter job)
        {
            try
            {
                var hlPost = dbContext.HighlightPosts.Find(job.ActivityNotification.PostId);
                var domain = dbContext.Domains.Find(job.ActivityNotification.DomainId);
                if (hlPost == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Not found Activity id {job.ActivityNotification.PostId}. Can not execute queue of qbicle.", null, job);
                    return;
                }
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    EventNotify = job.ActivityNotification.EventNotify,
                    HLPost = hlPost
                };
                NotifyOnFlaggingListing(parameter);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
                return;
            }
        }

        /// <summary>
        /// Call to API to run Hangfire job to create C2C Connection Accepted notification
        /// </summary>
        /// <param name="activityNotification"></param>
        public void NotificationOnC2CConnectionAccepted(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "notificationonc2cconnectionaccepted",
                ActivityNotification = activityNotification
            };
            var jobExected = new QbiclesJob().HangFireExcecuteAsync(job);
        }

        public void NotificationOnB2CConnectionAccepted(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "notificationb2cconnectioncreated",
                ActivityNotification = activityNotification
            };
            var jobExecuted = new QbiclesJob().HangFireExcecuteAsync(job);
        }

        #endregion

        #region Flag Listing Highlight Post
        public void NotifyOnFlagListing(ActivityNotification notificationObject)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "notifyonflaglisting",
                ActivityNotification = notificationObject
            };
            var jobExecuted = new QbiclesJob().HangFireExcecuteAsync(job);
        }
        #endregion

        #region Notification add remove Discussion Participants
        /// <summary>
        /// Call from rule
        /// Send comment on activity to HangFire schedule
        /// </summary>
        /// <param name="activityNotification"></param>
        public void NotificationDiscussionParticipants(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "discussionparticipants",
                ActivityNotification = activityNotification,

            };
            //execute SignalRComment2Activity
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        /// <summary>
        /// HangFire execute queue Activity
        /// </summary>
        /// <param name="job"></param>
        public void SignalRDiscussionParticipants(QbicleJobParameter job)
        {
            try
            {
                var activity = dbContext.Discussions.Find(job.ActivityNotification.Id);

                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = activity,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CurrentQbicleId = activity.Qbicle.Id,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    AppendToPageId = job.ActivityNotification.AppendToPageId,
                    HasActionToHandle = job.ActivityNotification.HasActionToHandle,
                    ObjectById = job.ActivityNotification.ObjectById
                };


                var notifications = CreateNotificationsDiscussionParticipants(parameter);
                foreach (var notify in notifications)
                {
                    notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                    notify.OriginatingCreationId = parameter.OriginatingCreationId;
                    SendBroadcastNotification(notify);
                }

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }

        private List<Notification> CreateNotificationsDiscussionParticipants(SignalRParameter parameter)
        {
            var notifications = new List<Notification>();

            List<ApplicationUser> notifyUsers;
            if (parameter.Activity != null)
            {
                var startDate = DateTime.UtcNow;
                var qb = parameter.Activity.Qbicle;

                notifyUsers = parameter.Activity.ActivityMembers.Distinct().ToList();

                if (notifyUsers.Any())
                    notifyUsers = notifyUsers.Where(p => p.IsEnabled == true).ToList();
                //Get All Member by Qbicle
                var notificeUsersByQBicle = qb.Members.Where(p => p.IsEnabled == true).Distinct().ToList();
                notifyUsers.AddRange(notificeUsersByQBicle);
                //end
                notifyUsers = (from d in notifyUsers select d).Distinct().ToList();
                //fix problems SignalR communications will not occur correctly if the Domain Admin is not a member of the B2C QBicle
                AddDomainAdminsToNotifyUsers(notifyUsers, parameter.Activity.Qbicle.Domain, null, parameter.Activity);
                //end                
                var startedBy = dbContext.QbicleUser.Find(parameter.CreatedById);

                Notification notification;
                //Sent a notification associated to user discussion Participants
                var associatedUser = dbContext.QbicleUser.Find(parameter.ObjectById);
                if (associatedUser != null)
                {
                    notification = new Notification
                    {
                        AssociatedUser = associatedUser,
                        AssociatedAcitvity = parameter.Activity,
                        CreatedBy = startedBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = parameter.EventNotify,
                        NotifiedUser = associatedUser,
                        AssociatedDomain = qb.Domain,
                        AssociatedQbicle = parameter.Activity.Qbicle,
                        SentMethod = associatedUser.ChosenNotificationMethod,
                        AppendToPageName = ApplicationPageName.Activities,
                        IsRead = false,
                        IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
                    };
                    dbContext.Notifications.Add(notification);
                    dbContext.Entry(notification).State = EntityState.Added;
                    notifications.Add(notification);
                }
                //Sent a notification updated for the Activity 
                foreach (var use in notifyUsers.Where(u => u.Id != parameter.ObjectById).ToList())
                {
                    notification = new Notification
                    {
                        AssociatedPost = null,
                        AssociatedAcitvity = parameter.Activity,
                        CreatedBy = startedBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = NotificationEventEnum.DiscussionUpdate,
                        NotifiedUser = use,
                        AssociatedDomain = qb.Domain,
                        AssociatedQbicle = parameter.Activity.Qbicle,
                        SentMethod = use.ChosenNotificationMethod,
                        AppendToPageName = ApplicationPageName.Activities,
                        IsRead = use.Id == startedBy.Id,
                        IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
                    };
                    dbContext.Notifications.Add(notification);
                    dbContext.Entry(notification).State = EntityState.Added;
                    //if (notification.IsRead == false)
                    notifications.Add(notification);
                }


                //Save Notify for Ativity
                dbContext.SaveChanges();
            }

            return notifications;

        }

        private List<Notification> CreateNotificationsDomainSubscription(SignalRParameter parameter)
        {
            var notifications = new List<Notification>();

            var associatedDomain = parameter.Domain;
            var creator = dbContext.QbicleUser.Find(parameter.CreatedById);
            var startDate = DateTime.UtcNow;

            foreach (var user in associatedDomain.Administrators)
            {
                var notification = new Notification
                {
                    AssociatedPost = null,
                    AssociatedAcitvity = parameter.Activity,
                    CreatedBy = creator,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = parameter.EventNotify,
                    NotifiedUser = user,
                    AssociatedDomain = parameter.Domain,
                    AssociatedQbicle = null,
                    SentMethod = NotificationSendMethodEnum.Broadcast,
                    AppendToPageName = ApplicationPageName.Domain,
                    IsRead = user.Id == creator.Id,
                    IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
                };
                dbContext.Notifications.Add(notification);
                dbContext.Entry(notification).State = EntityState.Added;
                //if (notification.IsRead == false)
                notifications.Add(notification);
            }

            //Save Notify for Ativity
            dbContext.SaveChanges();

            return notifications;

        }
        #endregion

        #region Qbicle Stream notification
        /// <summary>
        /// Get and generate notidication response to Web or App
        /// </summary>
        /// <param name="id">notification id</param>
        /// <param name="isRenderHTML">true: return HTML, false: return Json</param>
        /// <returns></returns>
        public NotificationDetail GetNotificationById(int id, bool isRenderHTML = false, bool isCustomerView = false)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Notification By id");


            try
            {
                var notify = dbContext.Notifications.Find(id);
                if (notify != null)
                {
                    // Customer pull notification from business, so !IsCreatorTheCustomer = isCustomerView; Business creates notification, customer gets notification.
                    isCustomerView = !notify.IsCreatorTheCustomer;

                    var notifyDetail = new NotificationDetail
                    {
                        CreatedById = notify.CreatedBy.Id,
                        AppendToPageType = notify.AppendToPageName,
                        AppendToPageName = notify.AppendToPageName.GetDescription(),
                        CurrentDomainId = notify.AssociatedDomain?.Id ?? 0,
                        HasActionToHandle = notify.HasActionToHandle.HasValue ? notify.HasActionToHandle.Value : false,
                        Event = notify.Event,
                        EventName = notify.Event.GetDescription(),
                        AssociatedById = notify.AssociatedUser?.Id,
                        IsAlertDisplay = notify.IsAlertDisplay
                    };
                    #region Load the Object(Qbicle/QbicleDomain/QbicleActivity/QbiclePost) from Notification
                    object activitiy = null;

                    if (notify.AssociatedAcitvity != null && notify.AssociatedPost != null)//if post in activity
                    {
                        activitiy = notify.AssociatedPost;
                        if (notify.AssociatedPost.BKTransaction != null)
                            notifyDetail.ElementId = notify.AssociatedPost.BKTransaction.Id;
                        else if (notify.AssociatedPost.JournalEntry != null)
                            notifyDetail.ElementId = notify.AssociatedPost.JournalEntry.Id;
                        else
                            notifyDetail.ElementId = notify.AssociatedPost.Id;
                        notifyDetail.CurrentQbicleId = notify.AssociatedQbicle?.Id ?? 0;

                        notifyDetail.AppendToPageId = notify.AssociatedAcitvity.Id;
                        notifyDetail.IsCreatorTheCustomer = notify.AssociatedPost.IsCreatorTheCustomer;
                        notifyDetail.CreatorTheQbcile = notify.AssociatedPost.Topic.Qbicle.GetCreatorTheQbcile();
                    }
                    else if (notify.AssociatedAcitvity != null)//if activity in Qbicle
                    {

                        //if (notify.AssociatedAcitvity.IsVisibleInQbicleDashboard)
                        activitiy = notify.AssociatedAcitvity;

                        notifyDetail.ElementId = notify.AssociatedAcitvity.Id;
                        notifyDetail.CurrentQbicleId = notify.AssociatedAcitvity.Qbicle?.Id ?? 0;
                        notifyDetail.AppendToPageId = notify.AssociatedAcitvity.Qbicle?.Id ?? 0;
                        notifyDetail.IsCreatorTheCustomer = notify.AssociatedAcitvity.IsCreatorTheCustomer;
                        notifyDetail.CreatorTheQbcile = notify.AssociatedAcitvity.Qbicle.GetCreatorTheQbcile();
                    }
                    else if (notify.AssociatedPost != null)//if post in Qbicle
                    {
                        activitiy = notify.AssociatedPost;
                        if (notify.AssociatedPost.BKTransaction != null)
                            notifyDetail.ElementId = notify.AssociatedPost.BKTransaction.Id;
                        else if (notify.AssociatedPost.JournalEntry != null)
                            notifyDetail.ElementId = notify.AssociatedPost.JournalEntry.Id;
                        else
                            notifyDetail.ElementId = notify.AssociatedPost.Id;
                        notifyDetail.CurrentQbicleId = notify.AssociatedQbicle?.Id ?? 0;
                        notifyDetail.IsCreatorTheCustomer = notify.AssociatedPost.IsCreatorTheCustomer;
                        notifyDetail.CreatorTheQbcile = notify.AssociatedPost.Topic.Qbicle.GetCreatorTheQbcile();
                    }
                    else if (notify.AssociatedQbicle != null)
                    {
                        activitiy = notify.AssociatedQbicle;
                        notifyDetail.ElementId = notify.AssociatedQbicle.Id;
                        notifyDetail.CurrentQbicleId = notify.AssociatedQbicle.Id;
                        notifyDetail.AppendToPageId = notify.AssociatedQbicle.Domain?.Id ?? 0;
                        notifyDetail.IsCreatorTheCustomer = false;
                        notifyDetail.CreatorTheQbcile = notify.AssociatedQbicle.GetCreatorTheQbcile();
                    }

                    if (notify.AssociatedTradeOrder != null)
                    {
                        if (notify.AssociatedPost != null && (notify.Event == NotificationEventEnum.ActivityComment || notify.Event == NotificationEventEnum.DiscussionUpdate))
                            activitiy = notify.AssociatedPost;
                        else
                        {
                            var b2COrderCreation = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByTradeorderId(notify.AssociatedTradeOrder.Id);
                                if(b2COrderCreation == null)
                            {
                                //get from B2Borders
                                var b2bOrderCreation = new DiscussionsRules(dbContext).GetB2BDiscussionOrderByTradeorderId(notify.AssociatedTradeOrder.Id);
                                var discussion = dbContext.Discussions.FirstOrDefault(e => e.Id == b2bOrderCreation.Id);
                                activitiy = discussion;// notify.AssociatedTradeOrder;
                            }
                            else
                            {
                            var discussion = dbContext.Discussions.FirstOrDefault(e => e.Id == b2COrderCreation.Id);
                            activitiy = discussion;// notify.AssociatedTradeOrder;
                            }
                            //notifyDetail.ElementId = notify.AssociatedTradeOrder.Id;
                            //notifyDetail.AppendToPageId = notify.AssociatedTradeOrder.Id;
                        }
                    }

                    switch (notifyDetail.Event)
                    {
                        case NotificationEventEnum.JoinToWaitlist:
                        case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                        case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                        case NotificationEventEnum.ApprovalCustomWaitlist:
                        case NotificationEventEnum.RejectWaitlist:
                        case NotificationEventEnum.UpdateMembersList:
                            break;
                        default:
                            var dateFormat = string.IsNullOrEmpty(notify.NotifiedUser.DateFormat) ? "dd/MM/yyyy" : notify.NotifiedUser.DateFormat;
                            if (isRenderHTML)
                                notifyDetail.HtmlNotification = HtmlRender(activitiy, notify.NotifiedUser, notify.AppendToPageName, notify.Event, notify, isCustomerView);
                            else
                                notifyDetail.MicroNotification = MicroStreamRules.GenerateActivity(activitiy, notify.CreatedDate, null, notify.NotifiedUser.Id, dateFormat, notify.NotifiedUser.Timezone, false, notify.Event, notify);

                            dbContext.Entry(activitiy).State = EntityState.Unchanged;
                            break;
                    }

                    #endregion
                    return notifyDetail;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
            }
            return new NotificationDetail();
        }


        #endregion

        #region Domain Request
        /// <summary>
        /// Send Notification to Domain Request Creator when the request is Approved or Rejected
        /// </summary>
        /// <param name="activityNotification"></param>
        public void RaiseDomainRequestProcessNotification(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "processdomainrequest",
                ActivityNotification = activityNotification
            };
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public void SignalRProcessDomainRequest(QbicleJobParameter job)
        {
            try
            {
                var domainRequest = dbContext.QbicleDomainRequests.Find(job.ActivityNotification.Id);
                if (domainRequest == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Can not find Domain Request with Id {job.ActivityNotification.Id}. Can not execute queue of Qbicle.", null, job);
                    return;
                }
                else
                {
                    var parameter = new SignalRParameter
                    {
                        OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                        OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                        CreatedById = job.ActivityNotification.CreatedById,
                        CreatedByName = job.ActivityNotification.CreatedByName,
                        EventNotify = job.ActivityNotification.EventNotify,
                        DomainRequested = job.ActivityNotification.DomainRequest
                    };
                    NotifyOnProcessDomainRequest(parameter, domainRequest);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }
        }

        private void NotifyOnProcessDomainRequest(SignalRParameter param, QbicleDomainRequest dmRequest)
        {
            var notification = CreateProcessDomainRequestNotification(param, dmRequest);
            var resonSent = MapReasonSent(param.EventNotify);
            notification.OriginatingConnectionId = param.OriginatingConnectionId;
            notification.OriginatingCreationId = param.OriginatingCreationId;
            switch (notification.SentMethod)
            {
                case NotificationSendMethodEnum.Both:
                case NotificationSendMethodEnum.Email:
                    SendNotificationEmail(param.CreatedById, notification, resonSent, dmRequest.DomainRequestJSON.ParseAs<DomainRequest>().Name, "");
                    SendBroadcastNotification(notification);
                    break;
                case NotificationSendMethodEnum.Broadcast:
                    SendBroadcastNotification(notification);
                    break;
            }

        }

        private Notification CreateProcessDomainRequestNotification(SignalRParameter parameter, QbicleDomainRequest domainRequest)
        {
            var startDate = DateTime.UtcNow;

            var notify = new Notification
            {
                CreatedBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == parameter.CreatedById),
                CreatedDate = startDate,
                SentDate = startDate,
                Event = parameter.EventNotify,
                NotifiedUser = domainRequest.CreatedBy,
                AssociatedDomain = parameter.Domain,
                AssociatedUser = domainRequest.CreatedBy,
                SentMethod = domainRequest.CreatedBy.ChosenNotificationMethod,
                IsRead = false,
                AssociatedQbicle = parameter.Qbicle,
                AppendToPageName = parameter.AppendToPageName,
                AssociatedDomainRequest = domainRequest,
                IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
            };
            dbContext.Notifications.Add(notify);
            dbContext.Entry(notify).State = EntityState.Added;
            dbContext.SaveChanges();
            return notify;
        }

        public void RaiseNotificationOnDomainRequestCreated(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "domainrequestcreated",
                ActivityNotification = activityNotification
            };
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public void SignalROnDomainRequestCreated(QbicleJobParameter job)
        {
            try
            {
                var domainRequest = dbContext.QbicleDomainRequests.Find(job.ActivityNotification.Id);
                if (domainRequest == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Can not find Domain Request with Id {job.ActivityNotification.Id}. Can not execute queue of Qbicle.", null, job);
                    return;
                }
                else
                {
                    var lstReceivers = dbContext.QbicleUser.Where(p => p.IsSystemAdmin).ToList();
                    foreach (var receiver in lstReceivers)
                    {
                        SendBroadcastNotification(new Notification()
                        {
                            CreatedBy = domainRequest.CreatedBy,
                            NotifiedUser = receiver,
                            Event = NotificationEventEnum.CreateRequest
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }
        }


        #endregion

        #region Domain Extension Request
        /// <summary>
        /// Send Notification to Domain Request Creator when the request is Approved or Rejected
        /// </summary>
        /// <param name="activityNotification"></param>
        public void RaiseExtensionRequestProcessNotification(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "processextensionrequest",
                ActivityNotification = activityNotification
            };
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public void SignalRProcessExtensionRequest(QbicleJobParameter job)
        {
            try
            {
                var extensionRequest = dbContext.DomainExtensionRequests.Find(job.ActivityNotification.Id);
                if (extensionRequest == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Can not find Domain Extension Request with Id {job.ActivityNotification.Id}. Can not execute queue of Qbicle.", null, job);
                    return;
                }
                else
                {
                    var parameter = new SignalRParameter
                    {
                        OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                        OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                        CreatedById = job.ActivityNotification.CreatedById,
                        CreatedByName = job.ActivityNotification.CreatedByName,
                        EventNotify = job.ActivityNotification.EventNotify,
                    };
                    NotifyOnProcessExtensionRequest(parameter, extensionRequest);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }
        }

        private void NotifyOnProcessExtensionRequest(SignalRParameter param, DomainExtensionRequest extensionRequest)
        {
            var notification = CreateProcessExtensionRequestNotification(param, extensionRequest);
            var resonSent = MapReasonSent(param.EventNotify);
            notification.OriginatingConnectionId = param.OriginatingConnectionId;
            notification.OriginatingCreationId = param.OriginatingCreationId;
            switch (notification.SentMethod)
            {
                case NotificationSendMethodEnum.Both:
                case NotificationSendMethodEnum.Email:
                    SendNotificationEmail(param.CreatedById, notification, resonSent, extensionRequest.Domain.Name, "");
                    SendBroadcastNotification(notification);
                    break;
                case NotificationSendMethodEnum.Broadcast:
                    SendBroadcastNotification(notification);
                    break;
            }

        }

        private Notification CreateProcessExtensionRequestNotification(SignalRParameter parameter, DomainExtensionRequest extensionRequest)
        {
            var startDate = DateTime.UtcNow;

            var notify = new Notification
            {
                CreatedBy = dbContext.QbicleUser.FirstOrDefault(u => u.Id == parameter.CreatedById),
                CreatedDate = startDate,
                SentDate = startDate,
                Event = parameter.EventNotify,
                NotifiedUser = extensionRequest.CreatedBy,
                AssociatedDomain = extensionRequest.Domain,
                AssociatedUser = extensionRequest.CreatedBy,
                SentMethod = extensionRequest.CreatedBy.ChosenNotificationMethod,
                IsRead = false,
                AssociatedQbicle = parameter.Qbicle,
                AppendToPageName = parameter.AppendToPageName,
                AssociatedExtensionRequest = extensionRequest,
                IsCreatorTheCustomer = parameter.Post?.IsCreatorTheCustomer ?? parameter.Activity?.IsCreatorTheCustomer ?? false
            };
            dbContext.Notifications.Add(notify);
            dbContext.Entry(notify).State = EntityState.Added;
            dbContext.SaveChanges();
            return notify;
        }

        public void RaiseNotificationOnExtensionRequestCreated(ActivityNotification activityNotification)
        {
            var job = new QbicleJobParameter
            {
                EndPointName = "extensionrequestcreated",
                ActivityNotification = activityNotification
            };
            Task tskHangfire = new Task(async () =>
            {
                await new QbiclesJob().HangFireExcecuteAsync(job);
            });
            tskHangfire.Start();
        }

        public void SignalROnExtensionRequestCreated(QbicleJobParameter job)
        {
            try
            {
                var extensionRequest = dbContext.DomainExtensionRequests.Find(job.ActivityNotification.Id);
                if (extensionRequest == null)
                {
                    LogManager.Warn(MethodBase.GetCurrentMethod(), $"Can not find Domain Extension Request with Id {job.ActivityNotification.Id}. Can not execute queue of Qbicle.", null, job);
                    return;
                }
                else
                {
                    SendBroadCastForDomainExtensionRequest(extensionRequest);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, job);
            }
        }

        public void SendBroadCastForDomainExtensionRequest(DomainExtensionRequest request)
        {
            var lstReceivers = dbContext.QbicleUser.Where(p => p.IsSystemAdmin).ToList();
            foreach (var receiver in lstReceivers)
            {
                SendBroadcastNotification(new Notification()
                {
                    CreatedBy = request.CreatedBy,
                    NotifiedUser = receiver,
                    Event = NotificationEventEnum.CreateRequest
                });
            }
        }
        #endregion

        #region SignalR Typing
        /// <summary>
        /// 
        /// </summary>
        /// <param name="createdBy">need Id, ProfilePic</param>
        /// <param name="toEmails">List of user to recived signalr</param>
        /// <param name="typing">true: typing - false: end typing</param>
        /// <param name="discussionId">If chatting from Order then the dicussion has value >0 </param>
        public void TypingChat(UserSetting createdBy, string toEmails, NotificationEventEnum notificationEvent, int discussionId)
        {
            try
            {
                var job = new ChattingJobParameter
                {
                    EndPointName = "chatting",
                    Chatting = new ChattingModel
                    {
                        ChatFromEmail = createdBy.Email,
                        ChatFromId = createdBy.Id,
                        ChatFromImg = createdBy.ProfilePic,
                        ChatToEmails = toEmails.Split(',').ToList().Where(e => e != createdBy.Email).ToList(),
                        ChatFromName = createdBy.DisplayName,
                        Event = notificationEvent,
                        DiscussionId = discussionId
                    }
                };

                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();


            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }

        public void SignalR2UserChatting(ChattingJobParameter job)
        {
            job.Chatting.ChatToEmails.ForEach(chatTo =>
            {
                var notify = new Notification
                {
                    //use the dicussion for notification
                    Id = job.Chatting.DiscussionId,
                    CreatedBy = new ApplicationUser
                    {
                        Id = job.Chatting.ChatFromId,
                        ProfilePic = job.Chatting.ChatFromImg,
                        Email = job.Chatting.ChatFromEmail,
                        DisplayUserName = job.Chatting.ChatFromName
                    },
                    NotifiedUser = new ApplicationUser { UserName = chatTo },
                    Event = job.Chatting.Event
                };

                SendBroadcastNotification(notify);
            });

        }


        #endregion

        #region Update order processing to send SignalR notification after invoice created

        //public async Task<bool> SignalRB2COrderProcess(TradeOrder tradeOrder, NotificationEventEnum notificationEvent)
        //{
        //    return await ExcecuteJobAsync(job, job.EndPointName);
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradeOrder"></param>
        /// <param name="createdById">if is not null, then do not send notification to the user</param>
        /// <param name="notificationEvent"></param>
        /// <returns></returns>
        //public async Task SignalRB2COrderProcessAsync(TradeOrder tradeOrder, string createdById, NotificationEventEnum notificationEvent)
        //{
        //    await Task.Run(() =>
        //    {
        //        SignalRB2COrderProcess(tradeOrder, createdById, notificationEvent);
        //    });
        //}
        /// <summary>
        /// When the order is processed
        /// when the invoice creation is completed
        /// on the backend send a(SignalR) notification to the members of the B2C Qbicle associated with the order(particularly the customer)
        /// that the invoice has been created so the customer order page can refresh to display the payments options.
        /// or order begin processed, order processed
        /// </summary>
        /// <param name="activityNotification"></param>
        public void SignalRB2COrderProcess(TradeOrder tradeOrder, string createdById, NotificationEventEnum notificationEvent)
        {
            try
            {
                var startDate = DateTime.UtcNow;
                var userReceived = tradeOrder.Customer;

                if (userReceived == null)
                {
                    return;
                }

                var orderStatus = new
                {
                    tradeOrder.Id,
                    label = tradeOrder.GetDescription(),
                    color = tradeOrder.GetClassColor()
                };
                //For B2B order, customer is member of Consumer Domain. So only members of Consumer Domain Administrators and those users in the role Business User.
                var notify = new Notification
                {
                    CreatedBy = tradeOrder.CreatedBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = notificationEvent,
                    NotifiedUser = userReceived,
                    AssociatedUser = userReceived,
                    SentMethod = userReceived.ChosenNotificationMethod,
                    IsRead = false,
                    AppendToPageName = ApplicationPageName.B2COrder,
                    AssociatedTradeOrder = tradeOrder,
                    IsCreatorTheCustomer = false,
                    //IsShowAlert = true,
                };
                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                dbContext.SaveChanges();

                SendBroadcastNotification(notify, orderStatus);
                // if payment approved ony send notification to the customer, to update ui
                //if (notificationEvent == NotificationEventEnum.B2COrderPaymentApproved)
                //   return;
                var domain = tradeOrder.SellingDomain;



                //only be visible to Domain Administrators and those users in the role Business User...
                //var isB2CConnectionAllowed = false;
                var b2cQbicleMemIds = dbContext.B2CQbicles.Where(p => p.Business.Id == domain.Id && !p.IsHidden).SelectMany(x => x.Members).Where(e => e.Id != createdById).Select(x => x.Id).Distinct().ToList();
                //A user who is a Relationship Manager in a B2C Qbicle should also have access to the B2B menu item.
                var b2cQbicleMembers = dbContext.QbicleUser.Where(p => b2cQbicleMemIds.Contains(p.Id)).ToList();

                b2cQbicleMembers.ForEach(user =>
                {
                    var isDomainAdmin = domain.Administrators.Any(p => p.Id == user.Id);
                    var rightApps = new AppRightRules(dbContext).UserRoleRights(user.Id, domain.Id);
                    var visibleMenu = isDomainAdmin || rightApps.Any(r => r == RightPermissions.QbiclesBusinessAccess);
                    if (!visibleMenu) return;

                    notify = new Notification
                    {
                        CreatedBy = tradeOrder.CreatedBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = notificationEvent,
                        NotifiedUser = user,
                        AssociatedUser = user,
                        SentMethod = user.ChosenNotificationMethod,
                        IsRead = false,
                        AppendToPageName = ApplicationPageName.B2COrder,
                        AssociatedTradeOrder = tradeOrder,
                        IsCreatorTheCustomer = false,
                        //IsShowAlert = true,
                    };
                    dbContext.Notifications.Add(notify);
                    dbContext.Entry(notify).State = EntityState.Added;
                    dbContext.SaveChanges();

                    SendBroadcastNotification(notify, orderStatus);
                });

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }
        #endregion

        #region SingalIRB2BOrder
        public void SignalRB2BOrderProcess(TradeOrder tradeOrder, string createdById, NotificationEventEnum notificationEvent)
        {
            try
            {
                var startDate = DateTime.UtcNow;
                var userReceived = tradeOrder.Customer;

                if (userReceived == null)
                {
                    return;
                }

                var orderStatus = new
                {
                    tradeOrder.Id,
                    label = tradeOrder.GetDescription(),
                    color = tradeOrder.GetClassColor()
                };
                //For B2B order, customer is member of Consumer Domain. So only members of Consumer Domain Administrators and those users in the role Business User.
                var notify = new Notification
                {
                    CreatedBy = tradeOrder.CreatedBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = notificationEvent,
                    NotifiedUser = userReceived,
                    AssociatedUser = userReceived,
                    SentMethod = userReceived.ChosenNotificationMethod,
                    IsRead = false,
                    AppendToPageName = ApplicationPageName.B2BOrder,
                    AssociatedTradeOrder = tradeOrder,
                    IsCreatorTheCustomer = false,
                    //IsShowAlert = true,
                };
                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                dbContext.SaveChanges();

                SendBroadcastNotification(notify, orderStatus);
                // if payment approved ony send notification to the customer, to update ui
                //if (notificationEvent == NotificationEventEnum.B2COrderPaymentApproved)
                //   return;
                var domain = tradeOrder.SellingDomain;



                //only be visible to Domain Administrators and those users in the role Business User...
                //var isB2CConnectionAllowed = false;
                var b2cQbicleMemIds = dbContext.B2CQbicles.Where(p => p.Business.Id == domain.Id && !p.IsHidden).SelectMany(x => x.Members).Where(e => e.Id != createdById).Select(x => x.Id).Distinct().ToList();
                //A user who is a Relationship Manager in a B2C Qbicle should also have access to the B2B menu item.
                var b2cQbicleMembers = dbContext.QbicleUser.Where(p => b2cQbicleMemIds.Contains(p.Id)).ToList();

                b2cQbicleMembers.ForEach(user =>
                {
                    var isDomainAdmin = domain.Administrators.Any(p => p.Id == user.Id);
                    var rightApps = new AppRightRules(dbContext).UserRoleRights(user.Id, domain.Id);
                    var visibleMenu = isDomainAdmin || rightApps.Any(r => r == RightPermissions.QbiclesBusinessAccess);
                    if (!visibleMenu) return;

                    notify = new Notification
                    {
                        CreatedBy = tradeOrder.CreatedBy,
                        CreatedDate = startDate,
                        SentDate = startDate,
                        Event = notificationEvent,
                        NotifiedUser = user,
                        AssociatedUser = user,
                        SentMethod = user.ChosenNotificationMethod,
                        IsRead = false,
                        AppendToPageName = ApplicationPageName.B2BOrder,
                        AssociatedTradeOrder = tradeOrder,
                        IsCreatorTheCustomer = true,
                        //IsShowAlert = true,
                    };
                    dbContext.Notifications.Add(notify);
                    dbContext.Entry(notify).State = EntityState.Added;
                    dbContext.SaveChanges();

                    SendBroadcastNotification(notify, orderStatus);
                });

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }

        }
        #endregion

        #region Notification for Domain Subscription Events
        // Hangfire calls
        public void NotifyDomainAdminOnTrialTimeEnd(ActivityNotification activityNotification)
        {
            try
            {
                var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Id == activityNotification.Id);
                var job = new QbicleJobParameter
                {
                    EndPointName = "notifysubscriptiontrialend",
                    ActivityNotification = activityNotification,
                    JobExecuteTime = DateTime.UtcNow.AddDays((double)(subscription.Plan?.Level?.NumberOfFreeTrialDays ?? 0)).AddHours(-48)
                };
                //execute SignalR2Activity
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public void NotifyDomainAdminOnPaymentDate(ActivityNotification activityNotification)
        {
            try
            {
                var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Id == activityNotification.Id);
                var job = new QbicleJobParameter
                {
                    EndPointName = "notifynextsubscriptionpaymentdate",
                    ActivityNotification = activityNotification,
                    JobExecuteTime = DateTime.UtcNow.AddDays((double)(subscription.Plan?.Level?.NumberOfFreeTrialDays ?? 0)).AddHours(-48)
                };
                //execute SignalR2Activity
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }
        // END: Hangfire calls

        public void CreateNotificationOnSubscriptionTrialEnd(QbicleJobParameter job)
        {
            try
            {
                var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Id == job.ActivityNotification.Id);

                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = null,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CurrentQbicleId = 0,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    AppendToPageId = job.ActivityNotification.AppendToPageId,
                    HasActionToHandle = false,
                    ObjectById = job.ActivityNotification.ObjectById,
                    Domain = subscription.Plan.Domain
                };


                var notifications = CreateNotificationsDomainSubscription(parameter);
                foreach (var notify in notifications)
                {
                    notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                    notify.OriginatingCreationId = parameter.OriginatingCreationId;
                    SendBroadcastNotification(notify);
                }

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }
        }

        public void CreateNotificationOnSubscriptionNextPaymentDate(QbicleJobParameter job)
        {
            try
            {
                var subscription = dbContext.DomainSubscriptions.FirstOrDefault(p => p.Id == job.ActivityNotification.Id);

                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = null,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CurrentQbicleId = 0,
                    CreatedByName = job.ActivityNotification.CreatedByName,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    AppendToPageId = job.ActivityNotification.AppendToPageId,
                    HasActionToHandle = false,
                    ObjectById = job.ActivityNotification.ObjectById,
                    Domain = subscription.Plan.Domain
                };


                var notifications = CreateNotificationsDomainSubscription(parameter);
                foreach (var notify in notifications)
                {
                    notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                    notify.OriginatingCreationId = parameter.OriginatingCreationId;
                    SendBroadcastNotification(notify);
                }

            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }
        }
        #endregion

        private NotificationModel GetNotificationModel(string timezone, int id)
        {
            var noti = dbContext.Notifications
                    .Where(n => n.Id == id)
                    .OrderByDescending(x => x.CreatedDate).ToList()
                    .Select(s => new NotificationModel
                    {
                        Id = s.Id,
                        CreatedBy = s.CreatedBy,
                        Activity = s.AssociatedAcitvity,
                        CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timezone),
                        AssociatedDomain = s.AssociatedDomain,
                        AssociatedQbicle = s.AssociatedQbicle,
                        Event = s.Event,
                        AssociatedPost = s.AssociatedPost,
                        AssociatedUser = s.AssociatedUser,
                        AssociateInvitation = s.AssociateInvitation,
                        AssociatedHighlight = s.AssociatedHighlight,
                        DomainRequested = s.AssociatedDomainRequest?.DomainRequestJSON,
                        ExtensionRequest = s.AssociatedExtensionRequest,
                        IsRead = s.IsRead,
                        AssociatedTradeOrder = s.AssociatedTradeOrder,
                        AssociatedWaitList = s.AssociatedWaitList,
                        IsCreatorTheCustomer = s.IsCreatorTheCustomer
                    }).FirstOrDefault();
            return noti;
        }

        public object GetAlertNotification(string timezone, int id, string currentUserId, ref string notifyCircleStyle)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAlertsNotification", null, id);

                var noti = GetNotificationModel(timezone, id);
                var activity = noti.Activity;
                if (noti.CreatedBy.Id == currentUserId && activity?.StartedBy.Id == currentUserId)
                {
                    return new { styleClass = notifyCircleStyle, data = "" };
                }

                //activity.UpdateReason
                //Data HTML can re-use from ShowNotificationsModal method
                var isCreatorTheCustomer = noti.IsCreatorTheCustomer;

                var trClass = isCreatorTheCustomer ? "chatuser" : "chatbusiness";

                var creatorTheQbcile = noti.AssociatedQbicle.GetCreatorTheQbcile();

                var createdBy = ""; var createdByImg = ""; var domainId = 0; var createdTime = noti.CreatedDate.GetTimeRelative();

                //var createdByUser = noti.Activity == null ? noti.CreatedBy : noti.Activity.StartedBy;
                var createdByUser = noti.CreatedBy;


                createdBy = createdByUser.GetFullName();
                createdByImg = createdByUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");
                //if bussiness
                if (isCreatorTheCustomer == false)
                {
                    if (creatorTheQbcile == QbicleType.B2CQbicle)
                    {
                        domainId = (noti.AssociatedQbicle as B2CQbicle).Business.Id;
                        var b2BProfiles = dbContext.B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault() ?? new B2BProfile();
                        createdBy = b2BProfiles.BusinessName;
                        createdByImg = b2BProfiles.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                    }
                }
                string wNotification = "";
                if (noti.AssociatedWaitList != null)
                {
                    var wUser = noti.AssociatedWaitList.ReviewedBy;
                    switch (noti.Event)
                    {
                        case NotificationEventEnum.JoinToWaitlist:
                            wUser = noti.AssociatedWaitList.User;
                            break;
                        case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                            wNotification = "Congratulations! You have been approved from the Waitlist, and can now begin adding Subscription Domains and Custom Domains.";
                            break;
                        case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                            wNotification = "Congratulations! You have been approved from the Waitlist, and can now begin adding Subscription Domains.";
                            break;
                        case NotificationEventEnum.ApprovalCustomWaitlist:
                            wNotification = "Congratulations! You have been approved from the Waitlist, and can now begin adding Custom Domains.";
                            break;
                        case NotificationEventEnum.RejectWaitlist:
                            wNotification = "We regret to inform you that your application for access to Subscription Domain creation has been unsuccessful, and you are no longer on the Waitlist. You are welcome to retry, or contact support if you’re stuck.".TruncateForDisplay(150);
                            break;
                    }
                    createdBy = wUser.GetFullName();
                    createdByImg = wUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");
                }
                bool dispayAlert = true;

                var trHtml = $"<tr class='{trClass}' id='alertnotificationid'>"; //add class cusomer or businesss
                //avatar
                trHtml += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{createdByImg}');\"></div></td>";
                switch (noti.Event)
                {

                    case NotificationEventEnum.PostCreation:
                    case NotificationEventEnum.ActivityComment:
                        var postCreation = noti.AssociatedPost;
                        var postMessage = postCreation.Message.TruncateForDisplay(100);
                        trHtml += $"<td>" + $"<p style='font-size: 11.5px; font-weight: 400; margin-bottom: 0;'>{postMessage}</p>" + $"<small>{createdBy}</small>" + $"</td>";


                        //if (postCreation != null && postCreation.Topic != null && activity == null)
                        //{
                        //    trHtml += $"<td><div class=\"avatar-circle sm\" style=\"background-image: url('{createdByImg}');\"></div></td>";
                        //    trHtml += $"<td><small><strong>{createdBy}</strong> added a post</small>" +
                        //        $"<p style=\"font-size: 11.5px; font-weight: 400; margin-bottom: 0;\">{postMessage}</p><small>{createdTime}</small></td>";
                        //}
                        //else
                        //{
                        //    trHtml += $"<td><div class=\"avatar-circle sm\" style=\"background-image: url('{createdByImg}');\"></div></td>";
                        //    trHtml += $"<td><small><strong>{createdBy}</strong> added a comment to {activity?.Name}</small>" +
                        //        $"<p style=\"font-size: 11.5px; font-weight: 400; margin-bottom: 0;\">{postMessage}</p><small>{createdTime}</small></td>";

                        //}
                        break;

                    case NotificationEventEnum.DiscussionCreation:
                        if (activity is QbicleDiscussion)
                        {
                            var ds1 = (QbicleDiscussion)activity;

                            trHtml += $"<td><p><strong>{createdBy}</strong> {GetDiscussionType(ds1.DiscussionType, "has created")} {activity?.Name}</p>" +
                                $"<small>{createdTime}</small></td>";
                        }

                        break;
                    case NotificationEventEnum.DiscussionUpdate:
                        if (activity is QbicleDiscussion)
                        {
                            var ds2 = (QbicleDiscussion)activity;

                            trHtml += $"<td><p><strong>{createdBy}</strong> {GetDiscussionType(ds2.DiscussionType, "has updated")} {activity?.Name}</p>" +
                                $"<small>{createdTime}</small></td>";
                        }
                        else if (activity is QbicleMedia)
                        {

                            trHtml += $"<td><p><strong>{createdBy}</strong> has added a new media {activity?.Name} into B2C Order</p>" +
                                $"<small>{createdTime}</small></td>";
                        }
                        break;

                    case NotificationEventEnum.AddUserParticipants:
                    case NotificationEventEnum.RemoveUserParticipants:
                        var ds3 = (QbicleDiscussion)activity;
                        var actionType = noti.Event == NotificationEventEnum.AddUserParticipants ? "added user into participants" : "remove user from participants";

                        trHtml += $"<td><p><strong>{createdBy}</strong> {GetDiscussionType(ds3.DiscussionType, actionType)} {activity?.Name}</p>" +
                            $"<small>{createdTime}</small></td>";
                        break;

                    case NotificationEventEnum.ApprovalCreation:

                        trHtml += $"<td><p><strong>{createdBy}</strong> created the Approval Request " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";

                        break;
                    case NotificationEventEnum.ApprovalUpdate:
                        dispayAlert = false;
                        //
                        //trHtml += $"<td><p><strong>{createdBy}</strong> updated the Approval Request " +
                        //    $"{activity?.Name}></p><small>{createdTime}</small></td>";

                        break;

                    case NotificationEventEnum.MediaCreation:

                        trHtml += $"<td><p><strong>{createdBy}</strong> uploaded the Media " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";

                        break;

                    case NotificationEventEnum.MediaUpdate:

                        trHtml += $"<td><p><strong>{createdBy}</strong> updated the Media " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";

                        break;

                    case NotificationEventEnum.MediaRemoveVersion:

                        trHtml += $"<td><p><strong>{createdBy}</strong> removed version in the Media " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";

                        break;
                    case NotificationEventEnum.MediaAddVersion:

                        trHtml += $"<td><p><strong>{createdBy}</strong> added version in to the Media " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";

                        break;
                    case NotificationEventEnum.TaskCompletion:

                        trHtml += $"<td><p><strong>{createdBy}</strong> completed the Task " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";
                        break;
                    case NotificationEventEnum.TaskCreation:

                        trHtml += $"<td><p><strong>{createdBy}</strong> created the Task " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";
                        break;
                    case NotificationEventEnum.TaskUpdate:
                        //dispayAlert = false;
                        //
                        trHtml += $"<td><p><strong>{createdBy}</strong> updated the Task " +
                            $"{activity?.Name}</p><small>{noti.CreatedDate.GetTimeRelative()}</small></td>";
                        break;

                    case NotificationEventEnum.ApprovalApproved:

                        trHtml += $"<td><p><strong>{createdBy}</strong>  approved the Approval Request " +
                            $"{activity?.Name}</p><small>{createdTime}</small></td>";
                        break;
                    case NotificationEventEnum.AssignTask:
                        dispayAlert = false;
                        //
                        //trHtml += $"<td><p><strong>{createdBy}</strong> assigned you to the Task " +
                        //    $"{activity?.Name}</p><small>{createdTime}</small></td>";
                        break;
                    case NotificationEventEnum.B2COrderUpdated:
                        if (activity is QbicleDiscussion)
                        {
                            var ds = (QbicleDiscussion)activity;

                            trHtml += $"<td><p><strong>{createdBy}</strong> {GetDiscussionType(ds.DiscussionType, "has updated")} {activity?.Name}</p>" +
                                $"<small>{createdTime}</small></td>";
                        }
                        break;
                    case NotificationEventEnum.B2COrderInvoiceCreationCompleted:
                        trHtml += $"<td><p><strong>{NotificationEventEnum.B2COrderInvoiceCreationCompleted.GetDescription()}</strong></td>";
                        break;
                    case NotificationEventEnum.B2COrderBeginProcess:
                        trHtml += $"<td><p><strong>{NotificationEventEnum.B2COrderBeginProcess.GetDescription()}</strong></td>";
                        break;
                    case NotificationEventEnum.B2COrderCompleted:
                        trHtml += $"<td><p><strong>{NotificationEventEnum.B2COrderCompleted.GetDescription()}</strong></td>";
                        break;
                    case NotificationEventEnum.B2BOrderCompleted:
                        trHtml += $"<td><p><strong>{NotificationEventEnum.B2BOrderCompleted.GetDescription()}</strong></td>";
                        break;
                    case NotificationEventEnum.B2COrderPaymentApproved:
                        trHtml += $"<td><p><strong>{NotificationEventEnum.B2COrderPaymentApproved.GetDescription()}</strong></td>";
                        break;
                    case NotificationEventEnum.EventNotificationPoints:
                        trHtml += $"<td><p>Reminder - <strong>{activity?.Name}</strong> starts tomorrow, and you’re marked to attend</td>";
                        break;
                    case NotificationEventEnum.TaskNotificationPoints:
                        trHtml += $"<td><p>Reminder - Your task, <strong>{activity?.Name}</strong> is due to begin soon</td>";
                        break;
                    case NotificationEventEnum.TaskComplete:
                    case NotificationEventEnum.TaskStart:
                        var peopleAssigneeFullName = dbContext.People.Where(s => s.AssociatedSet.Id == activity.AssociatedSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee).FirstOrDefault()?.User.GetFullName();
                        if (noti.Event == Notification.NotificationEventEnum.TaskStart)
                            trHtml += $"<td><p><strong>{peopleAssigneeFullName}</strong> has begun work on <strong>{activity?.Name}</strong></p></td>";
                        else if (noti.Event == Notification.NotificationEventEnum.TaskComplete)
                            trHtml += $"<td><p><strong>{peopleAssigneeFullName}</strong> has completed their task: <strong>{activity?.Name}</strong></td>";
                        break;
                    case NotificationEventEnum.JoinToWaitlist:
                        trHtml += $"<td><strong>{createdBy}</strong><p> has joined the Qbicles waitlist </p><small>{createdTime}</small></p></td>";
                        break;
                    case NotificationEventEnum.TaskWorkLog:
                    case NotificationEventEnum.DomainSubTrialEnd:
                    case NotificationEventEnum.DomainSubNextPaymentDate:
                        break;
                    case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                    case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                    case NotificationEventEnum.ApprovalCustomWaitlist:
                    case NotificationEventEnum.RejectWaitlist:
                        trHtml += $"<td>" + $"<p style='font-size: 11.5px; font-weight: 400; margin-bottom: 0;'>{wNotification}</p>" + $"<small>{createdBy}</small></td>";
                        break;
                    case NotificationEventEnum.QbicleCreation:
                    case NotificationEventEnum.QbicleUpdate:
                    case NotificationEventEnum.TypingChat:
                    case NotificationEventEnum.EndTypingChat:
                    case NotificationEventEnum.CreateRequest:
                    case NotificationEventEnum.AlertCreation:
                    case NotificationEventEnum.EventCreation:
                    case NotificationEventEnum.EventWithdrawl:
                    case NotificationEventEnum.CreateMember:
                    case NotificationEventEnum.InvitedMember:
                    case NotificationEventEnum.AlertUpdate:
                    case NotificationEventEnum.EventUpdate:
                    case NotificationEventEnum.TopicPost:
                    case NotificationEventEnum.ApprovalReviewed:
                    case NotificationEventEnum.ApprovalDenied:
                    case NotificationEventEnum.JournalPost:
                    case NotificationEventEnum.TransactionPost:
                    case NotificationEventEnum.LinkCreation:
                    case NotificationEventEnum.RemoveUserOutOfDomain:
                    case NotificationEventEnum.ReminderCampaignPost:
                    case NotificationEventEnum.QbicleInvited:
                    case NotificationEventEnum.C2CConnectionIssued:
                    case NotificationEventEnum.C2CConnectionAccepted:
                    case NotificationEventEnum.B2CConnectionCreated:
                    case NotificationEventEnum.LinkUpdate:
                    case NotificationEventEnum.ListingInterested:
                    case NotificationEventEnum.RemoveUserOutOfQbicle:
                    case NotificationEventEnum.AddUserToQbicle:
                    case NotificationEventEnum.ProcessDomainRequest:
                    case NotificationEventEnum.ProcessExtensionRequest:
                    case NotificationEventEnum.PostEdit:
                    case NotificationEventEnum.RemoveQueue:
                    default:
                        dispayAlert = false;
                        break;
                }
                trHtml += "<td style='width: 1px;'><button onclick='closeAlert(this)' class='btn btn-soft' style='background-color: transparent;padding-right: initial;padding-top: initial;'><i class='fa fa-close'></i></button></td>";

                trHtml += "</tr>";
                notifyCircleStyle = "";
                if (!dispayAlert)
                    return new { styleClass = notifyCircleStyle, data = "" };

                notifyCircleStyle = isCreatorTheCustomer ? "info" : "success";

                return new { styleClass = notifyCircleStyle, data = trHtml };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new { styleClass = "", data = "" };
                //return "<tr><td colspan=\"4\">Can't load the messages!</td></tr>";
            }
        }

        public NotificationAlertModel GetAlertNotificationMicro(string timezone, int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAlertNotificationMicro", null, id);

                var noti = GetNotificationModel(timezone, id);

                {
                    var activity = noti.Activity;
                    //activity.UpdateReason
                    //Data HTML can re-use from ShowNotificationsModal method
                    var isCreatorTheCustomer = noti.IsCreatorTheCustomer;

                    var trClass = isCreatorTheCustomer ? "chatuser" : "chatbusiness";

                    var creatorTheQbcile = noti.AssociatedQbicle.GetCreatorTheQbcile();

                    var createdBy = ""; var createdByImg = ""; var domainId = 0; var createdTime = noti.CreatedDate.GetTimeRelative();

                    //var createdByUser = noti.Activity == null ? noti.CreatedBy : noti.Activity.StartedBy;
                    var createdByUser = noti.CreatedBy;
                    createdBy = createdByUser.GetFullName();
                    createdByImg = createdByUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");
                    //if bussiness
                    if (!isCreatorTheCustomer)
                    {
                        if (creatorTheQbcile == QbicleType.B2CQbicle)
                        {
                            domainId = (noti.AssociatedQbicle as B2CQbicle).Business.Id;
                            var b2BProfiles = dbContext.B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault() ?? new B2BProfile();
                            createdBy = b2BProfiles.BusinessName;
                            createdByImg = b2BProfiles.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                        }
                    }

                    bool dispayAlert = true;

                    var alertModel = new NotificationAlertModel
                    {
                        IsCreatorTheCustomer = isCreatorTheCustomer,
                        Avatar = createdByImg,
                        Created = createdTime
                    };
                    switch (noti.Event)
                    {

                        case NotificationEventEnum.PostCreation:
                        case NotificationEventEnum.ActivityComment:
                            var postCreation = noti.AssociatedPost;
                            var postMessage = postCreation.Message.TruncateForDisplay(100);

                            alertModel.Message = postMessage;
                            alertModel.Created = createdBy;

                            break;

                        case NotificationEventEnum.DiscussionCreation:
                            if (activity is QbicleDiscussion)
                            {
                                var ds = (QbicleDiscussion)activity;
                                alertModel.Message = $"{createdBy} {GetDiscussionType(ds.DiscussionType, "has created")} {activity?.Name}";
                            }

                            break;
                        case NotificationEventEnum.DiscussionUpdate:
                            if (activity is QbicleDiscussion)
                            {
                                var ds = (QbicleDiscussion)activity;
                                alertModel.Message = $"{createdBy} {GetDiscussionType(ds.DiscussionType, "has updated")} {activity?.Name}";
                            }
                            else if (activity is QbicleMedia)
                            {
                                alertModel.Message = $"{createdBy} has added a new media {activity?.Name} into B2C Order";
                            }
                            //dispayAlert = false;
                            break;
                        case NotificationEventEnum.AddUserParticipants:
                        case NotificationEventEnum.RemoveUserParticipants:
                            var dsParticipants = (QbicleDiscussion)activity;
                            var actionType = noti.Event == NotificationEventEnum.AddUserParticipants ? "added user into participants" : "remove user from participants";
                            alertModel.Message = $"{createdBy} {GetDiscussionType(dsParticipants.DiscussionType, actionType)} {activity?.Name}";
                            break;

                        case NotificationEventEnum.ApprovalCreation:
                            alertModel.Message = $"{createdBy} created the Approval Request {activity?.Name}";

                            break;
                        case NotificationEventEnum.ApprovalUpdate:
                            dispayAlert = false;
                            break;

                        case NotificationEventEnum.MediaCreation:
                            alertModel.Message = $"{createdBy} uploaded the Media {activity?.Name}";
                            break;

                        case NotificationEventEnum.MediaUpdate:
                            alertModel.Message = $"{createdBy} updated the Media {activity?.Name}";
                            break;

                        case NotificationEventEnum.MediaRemoveVersion:
                            alertModel.Message = $"{createdBy} removed version in the Media {activity?.Name}";
                            break;
                        case NotificationEventEnum.MediaAddVersion:
                            alertModel.Message = $"{createdBy} added version in to the Media {activity?.Name}";
                            break;
                        case NotificationEventEnum.TaskCompletion:
                            alertModel.Message = $"{createdBy}</strong> completed the Task {activity?.Name}";
                            break;
                        case NotificationEventEnum.TaskCreation:
                            alertModel.Message = $"{createdBy}</strong> created the Task {activity?.Name}";
                            break;
                        case NotificationEventEnum.TaskUpdate:
                            //dispayAlert = false;
                            alertModel.Message = $"{createdBy}</strong> updated the Task {activity?.Name}";
                            break;

                        case NotificationEventEnum.ApprovalApproved:
                            alertModel.Message = $"{createdBy} approved the Approval Request {activity?.Name}";
                            break;
                        case NotificationEventEnum.AssignTask:
                            dispayAlert = false;
                            break;
                        case NotificationEventEnum.B2COrderUpdated:
                            if (activity is QbicleDiscussion)
                            {
                                var ds = (QbicleDiscussion)activity;
                                alertModel.Message = $"{createdBy} {GetDiscussionType(ds.DiscussionType, "has updated")} {activity?.Name}";
                            }
                            break;
                        case NotificationEventEnum.B2COrderInvoiceCreationCompleted:
                            alertModel.Message = $"{NotificationEventEnum.B2COrderInvoiceCreationCompleted.GetDescription()}";
                            break;
                        case NotificationEventEnum.B2COrderBeginProcess:
                            alertModel.Message = $"{NotificationEventEnum.B2COrderBeginProcess.GetDescription()}";
                            break;
                        case NotificationEventEnum.B2COrderCompleted:
                            alertModel.Message = $"{NotificationEventEnum.B2COrderCompleted.GetDescription()}";
                            break;
                        case NotificationEventEnum.B2BOrderCompleted:
                            alertModel.Message = $"{NotificationEventEnum.B2BOrderCompleted.GetDescription()}";
                            break;
                        case NotificationEventEnum.B2COrderPaymentApproved:
                            alertModel.Message = $"{NotificationEventEnum.B2COrderPaymentApproved.GetDescription()}";
                            break;
                        case NotificationEventEnum.EventNotificationPoints:
                            alertModel.Message = $"Reminder - {activity?.Name} starts tomorrow, and you’re marked to attend";
                            break;
                        case NotificationEventEnum.TaskNotificationPoints:
                            alertModel.Message = $"Reminder - Your task, {activity?.Name} is due to begin soon";
                            break;
                        case NotificationEventEnum.TaskComplete:
                        case NotificationEventEnum.TaskStart:
                            var peopleAssigneeFullName = dbContext.People.Where(s => s.AssociatedSet.Id == activity.AssociatedSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee).FirstOrDefault()?.User.GetFullName();
                            if (noti.Event == Notification.NotificationEventEnum.TaskStart)
                                alertModel.Message = $"{peopleAssigneeFullName} has begun work on {activity?.Name}";
                            else if (noti.Event == Notification.NotificationEventEnum.TaskComplete)
                                alertModel.Message = $"{peopleAssigneeFullName} has completed their task: {activity?.Name}";
                            break;
                        case NotificationEventEnum.QbicleCreation:
                        case NotificationEventEnum.QbicleUpdate:
                        case NotificationEventEnum.TypingChat:
                        case NotificationEventEnum.EndTypingChat:
                        case NotificationEventEnum.CreateRequest:
                        case NotificationEventEnum.AlertCreation:
                        case NotificationEventEnum.EventCreation:
                        case NotificationEventEnum.EventWithdrawl:
                        case NotificationEventEnum.CreateMember:
                        case NotificationEventEnum.InvitedMember:
                        case NotificationEventEnum.AlertUpdate:
                        case NotificationEventEnum.EventUpdate:
                        case NotificationEventEnum.TopicPost:
                        case NotificationEventEnum.ApprovalReviewed:
                        case NotificationEventEnum.ApprovalDenied:
                        case NotificationEventEnum.JournalPost:
                        case NotificationEventEnum.TransactionPost:
                        case NotificationEventEnum.LinkCreation:
                        case NotificationEventEnum.RemoveUserOutOfDomain:
                        case NotificationEventEnum.ReminderCampaignPost:
                        case NotificationEventEnum.QbicleInvited:
                        case NotificationEventEnum.C2CConnectionIssued:
                        case NotificationEventEnum.C2CConnectionAccepted:
                        case NotificationEventEnum.B2CConnectionCreated:
                        case NotificationEventEnum.LinkUpdate:
                        case NotificationEventEnum.ListingInterested:
                        case NotificationEventEnum.RemoveUserOutOfQbicle:
                        case NotificationEventEnum.AddUserToQbicle:
                        case NotificationEventEnum.ProcessDomainRequest:
                        case NotificationEventEnum.ProcessExtensionRequest:
                        case NotificationEventEnum.PostEdit:
                        case NotificationEventEnum.RemoveQueue:
                        default:
                            dispayAlert = false;
                            break;
                    }

                    if (!dispayAlert)
                        return new NotificationAlertModel { };
                    return alertModel;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new NotificationAlertModel { };
            }
        }


        private List<NotificationModel> GetNotificationsModel(PaginationRequest pagination, string timezone,
            AlertNotificationModel alertNotification, ref PaginationResponseAlert paginationResponse)
        {
            //var order = int.Parse(pagination.orderBy);
            var query = dbContext.Notifications
                .Where(n => alertNotification.Ids.Contains(n.Id));


            if (!alertNotification.IsShowAlertBusiness && !alertNotification.IsShowAlertCustomer)
                return new List<NotificationModel>();
            else if (alertNotification.IsShowAlertBusiness == true && alertNotification.IsShowAlertCustomer == false)
                query = query.Where(e => e.IsCreatorTheCustomer == false);
            else if (alertNotification.IsShowAlertBusiness == false && alertNotification.IsShowAlertCustomer == true)
                query = query.Where(e => e.IsCreatorTheCustomer == true);

            paginationResponse.totalNumber = query.Count();
            paginationResponse.totalPage = ((paginationResponse.totalNumber % pagination.pageSize) == 0) ? (paginationResponse.totalNumber / pagination.pageSize) : (paginationResponse.totalNumber / pagination.pageSize) + 1;


            var notifications = query
                //.Where(e => e.IsShowAlert)
                .OrderByDescending(x => x.CreatedDate).Skip((pagination.pageNumber - 1) * pagination.pageSize).Take(pagination.pageSize).ToList()
                .Select(s => new NotificationModel
                {
                    Id = s.Id,
                    CreatedBy = s.CreatedBy,
                    Activity = s.AssociatedAcitvity,
                    CreatedDate = s.CreatedDate.ConvertTimeFromUtc(timezone),
                    AssociatedDomain = s.AssociatedDomain,
                    AssociatedQbicle = s.AssociatedQbicle,
                    AssociatedTradeOrder = s.AssociatedTradeOrder,
                    Event = s.Event,
                    AssociatedPost = s.AssociatedPost,
                    AssociatedUser = s.AssociatedUser,
                    AssociateInvitation = s.AssociateInvitation,
                    AssociatedHighlight = s.AssociatedHighlight,
                    DomainRequested = s.AssociatedDomainRequest?.DomainRequestJSON,
                    ExtensionRequest = s.AssociatedExtensionRequest,
                    IsRead = s.IsRead,
                    IsCreatorTheCustomer = s.IsCreatorTheCustomer,
                    AssociatedWaitList = s.AssociatedWaitList,
                });
            return notifications.ToList();
        }
        public PaginationResponseAlert GetListAlertNotification(PaginationRequest pagination, string timezone, AlertNotificationModel alertNotification)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetAlertsNotification", null, alertNotification);
                var paginationResponse = new PaginationResponseAlert();
                var notifications = GetNotificationsModel(pagination, timezone, alertNotification, ref paginationResponse);
                var items = new List<string>();

                if (notifications != null && notifications.Any())
                {
                    var approvalType = "";
                    var approvalName = "";
                    foreach (var noti in notifications)
                    {
                        var activity = noti.Activity;
                        var qbicleName = activity?.Qbicle != null ? " / " + activity?.Qbicle.Name : "";
                        switch (noti.Event)
                        {
                            case NotificationEventEnum.ApprovalCreation:
                            case NotificationEventEnum.ApprovalUpdate:
                            case NotificationEventEnum.ApprovalReviewed:
                            case NotificationEventEnum.ApprovalApproved:
                            case NotificationEventEnum.ApprovalDenied:
                                var app = (ApprovalReq)activity;
                                approvalName = app?.Name;
                                switch (app?.ActivityType)
                                {
                                    case QbicleActivity.ActivityTypeEnum.ApprovalRequest:
                                        if (app?.Sale?.Count > 0)
                                        {
                                            approvalType = "/TraderSales/SaleReview?key=" + app?.Sale?.FirstOrDefault()?.Key;
                                        }
                                        if (app?.Purchase?.Count > 0)
                                        {
                                            approvalType = "/TraderPurchases/PurchaseReview?id=" + app?.Purchase?.FirstOrDefault()?.Id;
                                        }
                                        if (app?.TraderContact?.Count > 0)
                                        {
                                            approvalType = "/TraderContact/ContactReview?id=" + app?.TraderContact?.FirstOrDefault()?.Id;
                                        }
                                        if (app?.Transfer?.Count > 0)
                                        {
                                            approvalType = "/TraderTransfers/TransferReview?key=" + app?.Transfer?.FirstOrDefault()?.Key;
                                        }
                                        if (app?.Invoice?.Count > 0)
                                        {
                                            approvalType = "/TraderInvoices/InvoiceReview?key=" + app?.Invoice?.FirstOrDefault()?.Key;
                                        }
                                        break;
                                    case QbicleActivity.ActivityTypeEnum.ApprovalRequestApp:
                                        if (app?.JournalEntries != null && app?.JournalEntries?.Count > 0)
                                            approvalType = "journal";
                                        else
                                            approvalType = "approval";
                                        break;
                                }

                                break;
                        }

                        var isCreatorTheCustomer = noti.IsCreatorTheCustomer;

                        var trClass = isCreatorTheCustomer ? "chatuser" : "chatbusiness";

                        var creatorTheQbcile = noti.AssociatedQbicle.GetCreatorTheQbcile();

                        var createdBy = ""; var createdByImg = ""; var domainId = 0; var createdTime = noti.CreatedDate.GetTimeRelative();

                        //var createdByUser = noti.Activity == null ? noti.CreatedBy : noti.Activity.StartedBy;
                        var createdByUser = noti.CreatedBy;

                        createdBy = createdByUser.GetFullName();
                        createdByImg = createdByUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");

                        //if bussiness
                        if (!isCreatorTheCustomer)
                        {
                            if (creatorTheQbcile == QbicleType.B2CQbicle)
                            {
                                domainId = (noti.AssociatedQbicle as B2CQbicle).Business.Id;
                                var b2BProfiles = dbContext.B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault() ?? new B2BProfile();
                                createdBy = b2BProfiles.BusinessName;
                                createdByImg = b2BProfiles.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                            }
                        }
                        var createdDetail = createdTime + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName;

                        if (noti.AssociatedWaitList != null)
                        {
                            var wUser = noti.AssociatedWaitList.ReviewedBy ?? noti.AssociatedWaitList.User;
                            createdBy = wUser.GetFullName();
                            createdByImg = wUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");
                        }

                        var arlertContent = $"<tr class='{trClass}' id=\"notification-id-" + noti.Id + "\">"; //add class cusomer or businesss
                        arlertContent += $"<td style=\"width:55px;\"><div class=\"avatar-circle sm\" style=\"background-image: url('{createdByImg}');\"></div></td>";
                        switch (noti.Event)
                        {

                            case NotificationEventEnum.ApprovalCreation:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> created the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}', 'false','{approvalType}');\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;
                            case NotificationEventEnum.ApprovalUpdate:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> updated the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}', 'false','{approvalType}');\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;

                            case NotificationEventEnum.MediaCreation:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> uploaded the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;

                            case NotificationEventEnum.MediaUpdate:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> updated the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;
                            case NotificationEventEnum.MediaRemoveVersion:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> removed version from the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;
                            case NotificationEventEnum.MediaAddVersion:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> added version in to the Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;
                            case NotificationEventEnum.PostCreation:
                            case NotificationEventEnum.ActivityComment:
                                var postCreation = noti.AssociatedPost;
                                //isCreatorTheCustomer = noti.AssociatedPost.IsCreatorTheCustomer;
                                creatorTheQbcile = postCreation.Topic.Qbicle.GetCreatorTheQbcile();

                                // post for dashboard - topic_id != null
                                // Post for activity - qbicleactivity_id != null
                                if (postCreation != null && postCreation.Topic != null && noti.AssociatedTradeOrder == null)
                                {
                                    var _contentHtml = "<p><strong>" + createdBy + "</strong> added a post to <a href=\"javascript:void(0)\"  onclick=\"postSelelected('" + (noti.AssociatedQbicle != null ? noti.AssociatedQbicle.Id.Encrypt() : "") + "', '" + postCreation?.Id.Encrypt() + "', '" + Enums.QbicleModule.Dashboard + "'); \"> " + postCreation?.Topic?.Qbicle?.Name + "</a></p>";


                                    arlertContent += $"<td class=\"notification-detail\">{_contentHtml}<small>{createdDetail}</small></td>";
                                }
                                else if (noti.AssociatedTradeOrder != null)
                                {
                                    if (activity is QbicleDiscussion)
                                    {
                                        var ds = (QbicleDiscussion)activity;
                                        var discussionUrl = GetDiscussionLink(ds);

                                        arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> added a post to <a href=\"{discussionUrl}\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                    }
                                }
                                else
                                {
                                    var functionshow = "";
                                    if (activity != null)
                                    {
                                        if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp)
                                        {
                                            functionshow = $"ShowApprovalPage('{activity.Key}', false,'{approvalType}');";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity)
                                        {
                                            functionshow = "ShowMediaPage('" + activity.Key + "', false);";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity)
                                        {
                                            functionshow = "ShowTaskPage('" + activity.Key + "', false); ";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.DiscussionActivity)
                                        {
                                            var discussion = activity as B2COrderCreation;
                                            if (discussion == null)
                                                functionshow = "ShowDiscussionPage('" + activity.Key + "', false); ";
                                            else
                                                functionshow = "ShowDiscussionB2CPage('" + activity.Key + "', false); ";
                                        }
                                    }


                                    if (noti.Event == NotificationEventEnum.PostEdit)
                                        arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> updated a comment to <a href=\"javascript:void(0)\"  onclick=\"{functionshow}\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                    else
                                        arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> added a comment to <a href=\"javascript:void(0)\"  onclick=\"{functionshow}\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                }
                                break;
                            case NotificationEventEnum.TaskCompletion:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> completed the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.TaskCreation:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> created the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.TaskUpdate:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> updated the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.DiscussionCreation:
                                if (activity is QbicleDiscussion)
                                {
                                    var ds1 = (QbicleDiscussion)activity;
                                    var discussionUrl1 = GetDiscussionLink(ds1);

                                    arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> {GetDiscussionType(ds1.DiscussionType, "has created")} <a href=\"{discussionUrl1}\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                }

                                break;
                            case NotificationEventEnum.DiscussionUpdate:
                                if (activity is QbicleDiscussion)
                                {
                                    var ds2 = (QbicleDiscussion)activity;
                                    var discussionUrl2 = GetDiscussionLink(ds2);

                                    arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> {GetDiscussionType(ds2.DiscussionType, "has updated")} <a href=\"{discussionUrl2}\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                }
                                else if (activity is QbicleMedia)
                                {
                                    arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> has added new Media <a href=\"javascript:void(0)\" onclick=\"ShowMediaPage('{activity?.Key}', false);\">{activity?.Name} into B2C order</a></p><small>{createdDetail}</small></td>";
                                }
                                break;
                            case NotificationEventEnum.AddUserParticipants:
                            case NotificationEventEnum.RemoveUserParticipants:
                                var ds3 = (QbicleDiscussion)activity;
                                var discussionUrl3 = GetDiscussionLink(ds3);
                                var actionType = noti.Event == NotificationEventEnum.AddUserParticipants ? "added user into participants" : "remove user from participants";

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> {GetDiscussionType(ds3.DiscussionType, actionType)} <a href=\"{discussionUrl3}\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";

                                break;

                            case NotificationEventEnum.ApprovalApproved:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong>  approved the Approval Request <a href=\"javascript:void(0)\" onclick=\"ShowApprovalPage('{activity?.Key}','false','{approvalType}');\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.AssignTask:

                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> assigned you to the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}</a></p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.TypingChat:
                            case NotificationEventEnum.EndTypingChat:
                            case NotificationEventEnum.CreateRequest:
                                break;
                            case NotificationEventEnum.B2COrderUpdated:
                                break;
                            case NotificationEventEnum.B2COrderInvoiceCreationCompleted:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{NotificationEventEnum.B2COrderInvoiceCreationCompleted.GetDescription()}</strong></td>";
                                break;
                            case NotificationEventEnum.B2COrderBeginProcess:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{NotificationEventEnum.B2COrderBeginProcess.GetDescription()}</strong></td>";
                                break;
                            case NotificationEventEnum.B2COrderCompleted:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{NotificationEventEnum.B2COrderCompleted.GetDescription()}</strong></td>";
                                break;
                            case NotificationEventEnum.B2BOrderCompleted:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{NotificationEventEnum.B2BOrderCompleted.GetDescription()}</strong></td>";
                                break;
                            case NotificationEventEnum.B2COrderPaymentApproved:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{NotificationEventEnum.B2COrderPaymentApproved.GetDescription()}</strong></td>";
                                break;
                            case NotificationEventEnum.DomainSubTrialEnd:
                                var associatedDomain = noti.AssociatedDomain;
                                arlertContent += $"<td class=\"notification-detail\"><p>Your trial will expire soon!</p><small>Your free trial period for <strong>{associatedDomain.Name}</strong> expires soon, after which you will be automatically billed the first month's subscription fee. Manage this in <a href=\"/Administration/AdminPermissions\">Domain Adminstration</a>.</small></td>";
                                break;
                            case NotificationEventEnum.DomainSubNextPaymentDate:
                                arlertContent += $"<td class=\"notification-detail\"><p>Reminder of subscription payment</p><small>Your monthly subscription charge for <strong data-renderer-mark=\"true\">{noti.AssociatedDomain.Name}</strong> is due to be paid soon. Track and manage this in <a href=\"/Administration/AdminPermissions\">Domain Adminstration</a>.</small></td>";
                                break;
                            case NotificationEventEnum.EventNotificationPoints:
                                arlertContent += $"<td class=\"notification-detail\"><p>Reminder - <strong>{activity?.Name}</strong> starts tomorrow, and you’re marked to attend</td>";
                                break;
                            case NotificationEventEnum.TaskNotificationPoints:
                                arlertContent += $"<td class=\"notification-detail\"><p>Reminder - Your task, <strong>{activity?.Name}</strong> is due to begin soon</td>";
                                break;
                            case NotificationEventEnum.TaskComplete:
                            case NotificationEventEnum.TaskStart:
                                var peopleAssigneeFullName = dbContext.People.Where(s => s.AssociatedSet.Id == activity.AssociatedSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee).FirstOrDefault()?.User.GetFullName();
                                if (noti.Event == Notification.NotificationEventEnum.TaskStart)
                                    arlertContent += $"<td class=\"notification-detail\"><p><strong>{peopleAssigneeFullName}</strong> has begun work on <strong>{activity?.Name}</strong></td>";
                                else if (noti.Event == Notification.NotificationEventEnum.TaskComplete)
                                    arlertContent += $"<td class=\"notification-detail\"><p><strong>{peopleAssigneeFullName}</strong> has completed their task: <strong>{activity?.Name}</strong></td>";
                                break;

                            case NotificationEventEnum.JoinToWaitlist:
                                arlertContent += $"<td class=\"notification-detail\"><p><strong>{createdBy}</strong> has send request join to <a href=\"/Administration/AdminSysManage?tabActive=WaitlistRequest\">waitlist</a></p><small>{createdDetail}</small></td>";
                                break;

                            case NotificationEventEnum.ApprovalSubscriptionAndCustomWaitlist:
                                arlertContent += $"<td class=\"notification-detail\"><p>Congratulations! You have been approved from the Waitlist, and can now begin adding Subscription Domains and Custom Domains.</p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.ApprovalSubscriptionWaitlist:
                                arlertContent += $"<td class=\"notification-detail\"><p>Congratulations! You have been approved from the Waitlist, and can now begin adding Subscription Domains.</p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.ApprovalCustomWaitlist:

                                arlertContent += $"<td class=\"notification-detail\"><strong>{createdBy}</strong><p> has approved your request join to waitlist</p><small>{createdDetail}</small></td>";
                                break;

                            case NotificationEventEnum.RejectWaitlist:
                                arlertContent += $"<td class=\"notification-detail\"><p>We regret to inform you that your application for access to Subscription Domain creation has been unsuccessful, and you are no longer on the Waitlist. You are welcome to retry, or contact support if you’re stuck.</p><small>{createdDetail}</small></td>";
                                break;
                            case NotificationEventEnum.QbicleCreation:
                            case NotificationEventEnum.QbicleUpdate:
                            case NotificationEventEnum.AlertCreation:
                            case NotificationEventEnum.EventCreation:
                            case NotificationEventEnum.EventWithdrawl:
                            case NotificationEventEnum.CreateMember:
                            case NotificationEventEnum.InvitedMember:
                            case NotificationEventEnum.AlertUpdate:
                            case NotificationEventEnum.EventUpdate:
                            case NotificationEventEnum.TopicPost:
                            case NotificationEventEnum.ApprovalReviewed:
                            case NotificationEventEnum.ApprovalDenied:
                            case NotificationEventEnum.JournalPost:
                            case NotificationEventEnum.TransactionPost:
                            case NotificationEventEnum.LinkCreation:
                            case NotificationEventEnum.RemoveUserOutOfDomain:
                            case NotificationEventEnum.ReminderCampaignPost:
                            case NotificationEventEnum.QbicleInvited:
                            case NotificationEventEnum.C2CConnectionIssued:
                            case NotificationEventEnum.C2CConnectionAccepted:
                            case NotificationEventEnum.B2CConnectionCreated:
                            case NotificationEventEnum.LinkUpdate:
                            case NotificationEventEnum.ListingInterested:
                            case NotificationEventEnum.RemoveUserOutOfQbicle:
                            case NotificationEventEnum.AddUserToQbicle:
                            case NotificationEventEnum.ProcessDomainRequest:
                            case NotificationEventEnum.ProcessExtensionRequest:
                            case NotificationEventEnum.PostEdit:
                            case NotificationEventEnum.RemoveQueue:
                            default:
                                break;
                        }
                        arlertContent += "</tr>";
                        items.Add(arlertContent);
                        //items.Add(new AlertNotificationDetail
                        //{
                        //    Content = trHtml,
                        //    Type = AlertFilterType.All
                        //});
                    }

                }
                else
                {
                    //items.Add(new AlertNotificationDetail
                    //{
                    //    Content = "<tr><td colspan=\"4\">You have no new messages!</td></tr>",
                    //    Type = AlertFilterType.All
                    //});
                }
                paginationResponse.items = items;
                return paginationResponse;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertNotification);

                //var items = new List<AlertNotificationDetail>();
                //items.Add(new AlertNotificationDetail
                //{
                //    Content = "<tr><td colspan=\"4\">Can't load the messages!</td></tr>",
                //    Type = AlertFilterType.All
                //});
                return new PaginationResponseAlert { totalNumber = 0, items = new List<string>() };
                //return "<tr><td colspan=\"4\">Can't load the messages!</td></tr>";
            }
        }

        public PaginationResponse GetListAlertNotificationMicro(PaginationRequest pagination, string timezone, AlertNotificationModel alertNotification)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetListAlertNotificationMicro", null, alertNotification);
                var paginationResponse = new PaginationResponseAlert();
                var notifications = GetNotificationsModel(pagination, timezone, alertNotification, ref paginationResponse);
                var items = new List<NotificationAlertModel>();

                if (notifications != null && notifications.Any())
                {
                    var approvalType = "";
                    //var approvalName = "";
                    foreach (var noti in notifications)
                    {
                        var activity = noti.Activity;
                        var qbicleName = activity?.Qbicle != null ? " / " + activity?.Qbicle.Name : "";
                        //switch (noti.Event)
                        //{
                        //    case NotificationEventEnum.ApprovalCreation:
                        //    case NotificationEventEnum.ApprovalUpdate:
                        //    case NotificationEventEnum.ApprovalReviewed:
                        //    case NotificationEventEnum.ApprovalApproved:
                        //    case NotificationEventEnum.ApprovalDenied:
                        //        var app = (ApprovalReq)activity;
                        //        approvalName = app?.Name;
                        //        switch (app?.ActivityType)
                        //        {
                        //            case QbicleActivity.ActivityTypeEnum.ApprovalRequest:
                        //                if (app?.Sale?.Count > 0)
                        //                {
                        //                    approvalType = "/TraderSales/SaleReview?key=" + app?.Sale?.FirstOrDefault()?.Key;
                        //                }
                        //                if (app?.Purchase?.Count > 0)
                        //                {
                        //                    approvalType = "/TraderPurchases/PurchaseReview?id=" + app?.Purchase?.FirstOrDefault()?.Id;
                        //                }
                        //                if (app?.TraderContact?.Count > 0)
                        //                {
                        //                    approvalType = "/TraderContact/ContactReview?id=" + app?.TraderContact?.FirstOrDefault()?.Id;
                        //                }
                        //                if (app?.Transfer?.Count > 0)
                        //                {
                        //                    approvalType = "/TraderTransfers/TransferReview?key=" + app?.Transfer?.FirstOrDefault()?.Key;
                        //                }
                        //                if (app?.Invoice?.Count > 0)
                        //                {
                        //                    approvalType = "/TraderInvoices/InvoiceReview?key=" + app?.Invoice?.FirstOrDefault()?.Key;
                        //                }
                        //                break;
                        //            case QbicleActivity.ActivityTypeEnum.ApprovalRequestApp:
                        //                if (app?.JournalEntries != null && app?.JournalEntries?.Count > 0)
                        //                    approvalType = "journal";
                        //                else
                        //                    approvalType = "approval";
                        //                break;
                        //        }

                        //        break;
                        //}

                        var isCreatorTheCustomer = noti.IsCreatorTheCustomer;

                        //var trClass = isCreatorTheCustomer ? "chatuser" : "chatbusiness";

                        var creatorTheQbcile = noti.AssociatedQbicle.GetCreatorTheQbcile();

                        var createdBy = ""; var createdByImg = ""; var domainId = 0; var createdTime = noti.CreatedDate.GetTimeRelative();

                        //var createdByUser = noti.Activity == null ? noti.CreatedBy : noti.Activity.StartedBy;
                        var createdByUser = noti.CreatedBy;
                        createdBy = createdByUser.GetFullName();
                        createdByImg = createdByUser.ProfilePic.ToUriString(Enums.FileTypeEnum.Image, "T");

                        //if bussiness
                        if (!isCreatorTheCustomer)
                        {
                            if (creatorTheQbcile == QbicleType.B2CQbicle)
                            {
                                domainId = (noti.AssociatedQbicle as B2CQbicle).Business.Id;
                                var b2BProfiles = dbContext.B2BProfiles.AsNoTracking().Where(s => s.Domain.Id == domainId).FirstOrDefault() ?? new B2BProfile();
                                createdBy = b2BProfiles.BusinessName;
                                createdByImg = b2BProfiles.LogoUri.ToUriString(Enums.FileTypeEnum.Image, "T");
                            }
                        }
                        var createdDetail = createdTime + (noti.AssociatedDomain != null ? (" in " + noti.AssociatedDomain.Name) : "") + qbicleName;

                        var alertModel = new NotificationAlertModel
                        {
                            NotificationId = noti.Id,
                            IsCreatorTheCustomer = isCreatorTheCustomer,
                            Avatar = createdByImg,
                            Created = createdDetail,
                            ActivityKey = activity?.Key,
                            ActivityType = approvalType,
                            NavigateId = activity?.Id
                        };

                        switch (noti.Event)
                        {

                            case NotificationEventEnum.ApprovalCreation:
                                alertModel.NavigateTo = NavigateEnum.Approval;
                                alertModel.Message = $"{createdBy} created the Approval Request {activity?.Name}";
                                break;
                            case NotificationEventEnum.ApprovalUpdate:
                                alertModel.NavigateTo = NavigateEnum.Approval;
                                alertModel.Message = $"{createdBy} updated the Approval Request {activity?.Name}";
                                break;

                            case NotificationEventEnum.MediaCreation:
                                alertModel.NavigateTo = NavigateEnum.Media;
                                alertModel.Message = $"{createdBy} uploaded the Media {activity?.Name}";
                                break;

                            case NotificationEventEnum.MediaUpdate:
                                alertModel.NavigateTo = NavigateEnum.Media;
                                alertModel.Message = $"{createdBy} updated the Media {activity?.Name}";
                                break;
                            case NotificationEventEnum.MediaRemoveVersion:
                                alertModel.NavigateTo = NavigateEnum.Media;
                                alertModel.Message = $"{createdBy} removed version from the Media {activity?.Name}";
                                break;
                            case NotificationEventEnum.MediaAddVersion:
                                alertModel.NavigateTo = NavigateEnum.Media;
                                alertModel.Message = $"{createdBy} added version in to the Media {activity?.Name}";
                                break;
                            case NotificationEventEnum.PostCreation:
                            case NotificationEventEnum.ActivityComment:
                                var postCreation = noti.AssociatedPost;
                                //isCreatorTheCustomer = noti.AssociatedPost.IsCreatorTheCustomer;
                                creatorTheQbcile = postCreation.Topic.Qbicle.GetCreatorTheQbcile();

                                // post for dashboard - topic_id != null
                                // Post for activity - qbicleactivity_id != null
                                if (postCreation != null && postCreation.Topic != null && noti.AssociatedTradeOrder == null)
                                {
                                    alertModel.Message = $"{postCreation.Message}\n{createdBy} added a post to " + postCreation?.Topic?.Qbicle?.Name;
                                }
                                else if (noti.AssociatedTradeOrder != null)
                                {
                                    if (activity is QbicleDiscussion)
                                    {
                                        var ds = (QbicleDiscussion)activity;
                                        var discussionUrl = GetDiscussionLink(ds);
                                        alertModel.NavigateTo = GetDiscussionNavigate(ds);
                                        alertModel.Message = $"{createdBy} added a post to {activity?.Name}";
                                    }
                                }
                                else
                                {
                                    var functionshow = "";
                                    if (activity != null)
                                    {
                                        if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.ApprovalRequestApp)
                                        {
                                            functionshow = $"ShowApprovalPage('{activity.Key}', false,'{approvalType}');";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.MediaActivity)
                                        {
                                            alertModel.NavigateTo = NavigateEnum.Media;
                                            functionshow = "ShowMediaPage('" + activity.Key + "', false);";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.TaskActivity)
                                        {
                                            alertModel.NavigateTo = NavigateEnum.Task;
                                            functionshow = "ShowTaskPage('" + activity.Key + "', false); ";
                                        }
                                        else if (activity.ActivityType == QbicleActivity.ActivityTypeEnum.DiscussionActivity)
                                        {
                                            var discussion = activity as B2COrderCreation;
                                            if (discussion == null)
                                            {
                                                functionshow = "ShowDiscussionPage('" + activity.Key + "', false); ";
                                                alertModel.NavigateTo = NavigateEnum.Discussion;
                                            }
                                            else
                                            {
                                                functionshow = "ShowDiscussionB2CPage('" + activity.Key + "', false); ";
                                                alertModel.NavigateTo = NavigateEnum.B2COrder;
                                            }
                                        }
                                    }


                                    if (noti.Event == NotificationEventEnum.PostEdit)
                                        alertModel.Message = $"{createdBy} updated a comment to {activity?.Name}";
                                    else
                                        alertModel.Message = $"{createdBy} added a comment to {activity?.Name}";

                                }
                                break;
                            case NotificationEventEnum.TaskCompletion:
                                alertModel.NavigateTo = NavigateEnum.Task;
                                alertModel.Message = $"{createdBy} completed the Task {activity?.Name}";
                                break;
                            case NotificationEventEnum.TaskCreation:
                                alertModel.NavigateTo = NavigateEnum.Task;
                                alertModel.Message = $"{createdBy} created the Task {activity?.Name}";
                                break;
                            case NotificationEventEnum.TaskUpdate:
                                //
                                //alertModel.Message = $"{createdBy} updated the Task <a href=\"javascript:void(0)\" onclick=\"ShowTaskPage('{activity?.Key}', false);\">{activity?.Name}";
                                break;
                            case NotificationEventEnum.DiscussionCreation:
                                if (activity is QbicleDiscussion)
                                {
                                    var ds1 = (QbicleDiscussion)activity;
                                    var discussionUrl1 = GetDiscussionLink(ds1);
                                    alertModel.NavigateTo = GetDiscussionNavigate(ds1);
                                    alertModel.Message = $"{createdBy} {GetDiscussionType(ds1.DiscussionType, "has created")} {activity?.Name}";
                                }

                                break;
                            case NotificationEventEnum.DiscussionUpdate:
                                if (activity is QbicleDiscussion)
                                {
                                    var ds2 = (QbicleDiscussion)activity;
                                    var discussionUrl2 = GetDiscussionLink(ds2);

                                    alertModel.NavigateTo = GetDiscussionNavigate(ds2);
                                    alertModel.Message = $"{createdBy} {GetDiscussionType(ds2.DiscussionType, "has updated")} {activity?.Name}";
                                }
                                else if (activity is QbicleMedia)
                                {
                                    alertModel.NavigateTo = NavigateEnum.Media;
                                    alertModel.Message = $"{createdBy} has added a new media {activity?.Name} into B2C Order";
                                }
                                break;
                            case NotificationEventEnum.AddUserParticipants:
                            case NotificationEventEnum.RemoveUserParticipants:
                                var ds3 = (QbicleDiscussion)activity;
                                var discussionUrl3 = GetDiscussionLink(ds3);
                                var actionType = noti.Event == NotificationEventEnum.AddUserParticipants ? "added user into participants" : "remove user from participants";

                                alertModel.NavigateTo = GetDiscussionNavigate(ds3);
                                alertModel.Message = $"{createdBy} {GetDiscussionType(ds3.DiscussionType, actionType)} {activity?.Name}";
                                break;
                            case NotificationEventEnum.ApprovalApproved:
                                alertModel.NavigateTo = NavigateEnum.Approval;
                                alertModel.Message = $"{createdBy}  approved the Approval Request {activity?.Name}";
                                break;
                            case NotificationEventEnum.AssignTask:
                                alertModel.NavigateTo = NavigateEnum.Task;
                                alertModel.Message = $"{createdBy} assigned you to the Task {activity?.Name}";
                                break;
                            case NotificationEventEnum.TypingChat:
                            case NotificationEventEnum.EndTypingChat:
                            case NotificationEventEnum.CreateRequest:
                                break;
                            case NotificationEventEnum.B2COrderUpdated:
                                break;
                            case NotificationEventEnum.B2COrderInvoiceCreationCompleted:
                                alertModel.NavigateTo = NavigateEnum.Invoice;
                                alertModel.Message = $"{NotificationEventEnum.B2COrderInvoiceCreationCompleted.GetDescription()}";
                                break;
                            case NotificationEventEnum.B2COrderBeginProcess:
                                alertModel.NavigateTo = NavigateEnum.B2COrder;
                                alertModel.Message = $"{NotificationEventEnum.B2COrderBeginProcess.GetDescription()}";
                                break;
                            case NotificationEventEnum.B2COrderCompleted:
                                alertModel.NavigateTo = NavigateEnum.B2COrder;
                                alertModel.Message = $"{NotificationEventEnum.B2COrderCompleted.GetDescription()}";
                                break;
                            case NotificationEventEnum.B2BOrderCompleted:
                                alertModel.NavigateTo = NavigateEnum.B2BOrder;
                                alertModel.Message = $"{NotificationEventEnum.B2BOrderCompleted.GetDescription()}";
                                break;
                            case NotificationEventEnum.B2COrderPaymentApproved:
                                alertModel.NavigateTo = NavigateEnum.Payment;
                                alertModel.Message = $"{NotificationEventEnum.B2COrderPaymentApproved.GetDescription()}";
                                break;
                            case NotificationEventEnum.EventNotificationPoints:
                                alertModel.NavigateTo = NavigateEnum.Event;
                                alertModel.Message = $"Reminder - {activity?.Name} starts tomorrow, and you’re marked to attend";
                                break;
                            case NotificationEventEnum.TaskNotificationPoints:
                                alertModel.Message = $"Reminder - Your task, {activity?.Name} is due to begin soon";
                                break;
                            case NotificationEventEnum.TaskComplete:
                            case NotificationEventEnum.TaskStart:
                                alertModel.NavigateTo = NavigateEnum.Task;
                                var peopleAssigneeFullName = dbContext.People.Where(s => s.AssociatedSet.Id == activity.AssociatedSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee).FirstOrDefault()?.User.GetFullName();
                                if (noti.Event == Notification.NotificationEventEnum.TaskStart)
                                    alertModel.Message = $"{peopleAssigneeFullName} has begun work on {activity?.Name}";
                                else if (noti.Event == Notification.NotificationEventEnum.TaskComplete)
                                    alertModel.Message = $"{peopleAssigneeFullName} has completed their task: {activity?.Name}";
                                break;
                            case NotificationEventEnum.QbicleCreation:
                            case NotificationEventEnum.QbicleUpdate:
                            case NotificationEventEnum.AlertCreation:
                            case NotificationEventEnum.EventCreation:
                            case NotificationEventEnum.EventWithdrawl:
                            case NotificationEventEnum.CreateMember:
                            case NotificationEventEnum.InvitedMember:
                            case NotificationEventEnum.AlertUpdate:
                            case NotificationEventEnum.EventUpdate:
                            case NotificationEventEnum.TopicPost:
                            case NotificationEventEnum.ApprovalReviewed:
                            case NotificationEventEnum.ApprovalDenied:
                            case NotificationEventEnum.JournalPost:
                            case NotificationEventEnum.TransactionPost:
                            case NotificationEventEnum.LinkCreation:
                            case NotificationEventEnum.RemoveUserOutOfDomain:
                            case NotificationEventEnum.ReminderCampaignPost:
                            case NotificationEventEnum.QbicleInvited:
                            case NotificationEventEnum.C2CConnectionIssued:
                            case NotificationEventEnum.C2CConnectionAccepted:
                            case NotificationEventEnum.B2CConnectionCreated:
                            case NotificationEventEnum.LinkUpdate:
                            case NotificationEventEnum.ListingInterested:
                            case NotificationEventEnum.RemoveUserOutOfQbicle:
                            case NotificationEventEnum.AddUserToQbicle:
                            case NotificationEventEnum.ProcessDomainRequest:
                            case NotificationEventEnum.ProcessExtensionRequest:
                            case NotificationEventEnum.PostEdit:
                            case NotificationEventEnum.RemoveQueue:
                            default:
                                break;
                        }

                        items.Add(alertModel);
                    }
                }
                else
                {

                }
                paginationResponse.items = items;
                return new PaginationResponse { items = paginationResponse.items, totalNumber = paginationResponse.totalNumber, totalPage = paginationResponse.totalPage };

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertNotification);

                return new PaginationResponse { totalNumber = 0, totalPage = 0, items = new List<NotificationAlertModel>() };
            }
        }

        #region Event & Task notification points

        public void Notification2EventTaskPoints(ActivityNotification activityNotification, QbicleActivity activity)
        {
            try
            {
                var endpointName = "scheduletasknotificationpoints";
                if (activityNotification.EventNotify == NotificationEventEnum.EventNotificationPoints)
                    endpointName = "scheduleeventnotificationpoints";

                var job = new QbicleJobParameter
                {
                    EndPointName = endpointName,
                    ActivityNotification = activityNotification,
                    ReminderMinutes = activityNotification.ReminderMinutes,
                };

                Task tskHangfire = new Task(async () =>
                {
                    var j = await new QbiclesJob().HangFireExcecuteAsync(job);
                    activity.JobId = j.JobId;
                    dbContext.SaveChanges();
                });
                tskHangfire.Start();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public void SignalREmailEventTaskNotificationPoints(QbicleJobParameter job)
        {
            try
            {
                /*
                 * If within 24 hours of expected task start datetime
                 * If task has not been started
                    Issue notification to task assignee:
                    ”Reminder - Your task, <Task title> is due to begin soon”
                    Else
                    Do nothing. The user can be assumed to be aware of the task status.
                 */
                if (job.ActivityNotification.EventNotify == NotificationEventEnum.TaskNotificationPoints)
                {
                    var taskStarted = dbContext.QbicleTasks.Any(e => e.Id == job.ActivityNotification.Id && e.ActualStart.HasValue);
                    if (taskStarted)
                        return;
                }


                var activity = dbContext.Activities.Find(job.ActivityNotification.Id);
                var parameter = new SignalRParameter
                {
                    OriginatingConnectionId = job.ActivityNotification.OriginatingConnectionId,
                    OriginatingCreationId = job.ActivityNotification.OriginatingCreationId,
                    Activity = activity,
                    ActivityBroadcast = job.ActivityNotification.ActivityBroadcast,
                    EventNotify = job.ActivityNotification.EventNotify,
                    CreatedById = job.ActivityNotification.CreatedById,
                    CurrentQbicleId = activity.Qbicle?.Id ?? 0,
                    AppendToPageName = job.ActivityNotification.AppendToPageName,
                    AppendToPageId = job.ActivityNotification.AppendToPageId,
                    ObjectById = job.ActivityNotification.ObjectById
                };
                Notify2EventTaskNotificationPoints(parameter);
            }
            catch (Exception e)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), e);
            }
        }

        private void Notify2EventTaskNotificationPoints(SignalRParameter parameter)
        {
            var notifications = CreateEventTaskNotificationPoints(parameter, parameter.Activity);
            var reasonSent = MapReasonSent(parameter.EventNotify);
            foreach (var notify in notifications)
            {
                //notify.OriginatingConnectionId = parameter.OriginatingConnectionId;
                //notify.OriginatingCreationId = parameter.OriginatingCreationId;
                SendBroadcastNotification(notify);
                SendEmailEventTaskNotificationPoints(notify, reasonSent);
            }
        }

        private List<Notification> CreateEventTaskNotificationPoints(SignalRParameter parameter, QbicleActivity activity)
        {
            var notifications = new List<Notification>();
            var domain = new QbicleDomain();
            var notification2Users = new List<ApplicationUser>();
            var createdBy = dbContext.QbicleUser.Find(parameter.CreatedById);
            var startDate = DateTime.UtcNow;

            switch (parameter.EventNotify)
            {
                case NotificationEventEnum.EventNotificationPoints:
                    var qbEvent = (QbicleEvent)activity;
                    notification2Users = qbEvent.ActivityMembers.Where(p => p.IsEnabled == true).Distinct().ToList();
                    break;
                case NotificationEventEnum.TaskNotificationPoints:
                case NotificationEventEnum.TaskStart:
                case NotificationEventEnum.TaskComplete:
                    var qbTask = (QbicleTask)activity;

                    var qbicleSet = qbTask.AssociatedSet;

                    var peopleAssignee = dbContext.People.Where(s => s.AssociatedSet.Id == qbicleSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Assignee).FirstOrDefault();

                    notification2Users.Add(peopleAssignee.User);

                    if (parameter.EventNotify != NotificationEventEnum.TaskNotificationPoints)
                    {
                        var peoplesWatches = dbContext.People.Where(s => s.AssociatedSet.Id == qbicleSet.Id && s.Type == QbiclePeople.PeopleTypeEnum.Watcher).ToList();

                        notification2Users.AddRange(peoplesWatches.Select(e => e.User));
                        if (parameter.EventNotify == NotificationEventEnum.TaskStart || parameter.EventNotify == NotificationEventEnum.TaskComplete)
                        {
                            notification2Users.Add(qbTask.StartedBy);
                        }
                    }

                    notification2Users.Add(qbTask.StartedBy);
                    break;
            }

            notification2Users = (from d in notification2Users select d).Distinct().ToList();
            if (notification2Users.Count == 0)
                return notifications;

            /*
             Only issue if task creator !== task assignee
             Otherwise we’d be notifying ourselves of our own progress
             */
            foreach (var user in notification2Users)//.Where(e => e.Id != parameter.CreatedById))
            {
                var notify = new Notification
                {
                    AssociatedDomain = activity.Qbicle.Domain,
                    AssociatedQbicle = activity.Qbicle,
                    AssociatedTradeOrder = null,
                    AssociatedPost = null,
                    AssociatedAcitvity = activity,
                    CreatedBy = createdBy,
                    CreatedDate = startDate,
                    SentDate = startDate,
                    Event = parameter.EventNotify,
                    NotifiedUser = user,
                    SentMethod = user.ChosenNotificationMethod,
                    IsRead = user.Id == createdBy.Id,
                    AppendToPageName = parameter.AppendToPageName,
                    IsCreatorTheCustomer = activity.IsCreatorTheCustomer,
                    IsAlertDisplay = true,
                };

                dbContext.Notifications.Add(notify);
                dbContext.Entry(notify).State = EntityState.Added;
                //if (notify.IsRead == false)
                notifications.Add(notify);
            }

            dbContext.SaveChanges();
            return notifications;

        }

        private void SendEmailEventTaskNotificationPoints(Notification notification, ReasonSent sentReason)
        {
            if (notification.CreatedBy.Id == notification.NotifiedUser.Id) return;
            var el = new EmailRules(dbContext);
            var emailLog = el.SendEmailEventTaskNotificationPoints(notification, sentReason);
            if (emailLog != null)
            {
                notification.EmailSent = emailLog;
                notification.SentDate = DateTime.UtcNow;
            }

            dbContext.Notifications.Attach(notification);
            dbContext.Entry(notification).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
        #endregion

        public void CancelHangfireJob(string hangfireJobId)
        {
            try
            {
                var job = new QbicleJobParameter
                {
                    EndPointName = "deletehangfirejobstate",
                    JobId = hangfireJobId
                };
                //execute SignalR2Activity
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }

        public void CancelHangfireRecurringJob(string hangfireJobId)
        {
            try
            {
                var job = new QbicleJobParameter
                {
                    EndPointName = "deletehangfirerecurringjobstate",
                    JobId = hangfireJobId
                };
                //execute SignalR2Activity
                Task tskHangfire = new Task(async () =>
                {
                    await new QbiclesJob().HangFireExcecuteAsync(job);
                });
                tskHangfire.Start();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }            
        public void ReloadMemberDataTable(Notification notification)
        {
            SendBroadcastNotification(notification);
        }
    }
}