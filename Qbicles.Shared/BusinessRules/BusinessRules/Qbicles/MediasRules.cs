using Newtonsoft.Json;
using Qbicles.BusinessRules.AWS;
using Qbicles.BusinessRules.Azure;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.Bookkeeping;
using Qbicles.Models.FileStorage;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class MediasRules
    {
        public MediasRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ApplicationDbContext dbContext;

        public bool DuplicateMediaNameCheck(int mediaId, string mediaName, int qbicleId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate name", null, null, mediaId, mediaName);

                if (mediaId > 0)
                    return dbContext.Medias.Any(x => x.Id != mediaId && x.Name == mediaName && x.Qbicle.Id == qbicleId);

                return dbContext.Activities.Any(x => x.Name == mediaName && x.Qbicle.Id == qbicleId);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, mediaId, mediaName);
                return false;
            }
        }

        public ReturnJsonModel UpdateMedia(QbicleMedia media, bool isCreatorTheCustomer, UserSetting user, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate name", null, null, media);

                var qbicleMedia = dbContext.Medias.FirstOrDefault(e => e.Id == media.Id);
                if (qbicleMedia == null) return new ReturnJsonModel { actionVal = 2, result = false, msg = "Media is null" };
                qbicleMedia.Description = media.Description;
                qbicleMedia.Name = media.Name;
                qbicleMedia.Topic = media.Topic;
                qbicleMedia.IsCreatorTheCustomer = isCreatorTheCustomer;
                dbContext.Entry(qbicleMedia).State = EntityState.Modified;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = media.Id,
                    EventNotify = NotificationEventEnum.MediaUpdate,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedByName = user.DisplayName,
                    CreatedById = user.Id,
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);

                return new ReturnJsonModel { actionVal = media.Id, result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, media);

                return new ReturnJsonModel { actionVal = 2, result = false, msg = ex.Message };
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="media"></param>
        /// <param name="isCreatorTheCustomer"></param>
        /// <param name="currentUserId"></param>
        /// <param name="isMediaOntab">if add from media tab in Qbicle dashboard - need return difference style</param>
        /// <param name="currentDiscussionId"></param>
        /// <param name="currentTaskId"></param>
        /// <param name="currentEventId"></param>
        /// <param name="currentAlertId"></param>
        /// <param name="currentApprovalId"></param>
        /// <param name="currentLinkId"></param>
        /// <param name="currentMediaId"></param>
        /// <param name="topicName"></param>
        /// <param name="versionFile"></param>
        /// <param name="mFolderId"></param>
        /// <param name="bkAccount"></param>
        /// <param name="bkTransaction"></param>
        /// <param name="jEntry"></param>
        /// <param name="cashAccountTransaction"></param>
        /// <param name="invoice"></param>
        /// <param name="originatingConnectionId"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveMedia(QbicleMedia media, bool isCreatorTheCustomer,
            string currentUserId, bool isMediaOntab,
            int currentDiscussionId = 0,
            int currentTaskId = 0,
            int currentEventId = 0,
            int currentAlertId = 0,
            int currentApprovalId = 0,
            int currentLinkId = 0,
            int currentMediaId = 0,
            string topicName = "",
            VersionedFile versionFile = null, int mFolderId = 0,
            BKAccount bkAccount = null, BKTransaction bkTransaction = null, JournalEntry jEntry = null,
            CashAccountTransaction cashAccountTransaction = null, Invoice invoice = null, string originatingConnectionId = "", AppType appType = AppType.Micro
            )
        {
            var result = new ReturnJsonModel();

            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save media", currentUserId, null, media, null, currentDiscussionId, currentTaskId,
                        currentEventId, currentAlertId, currentApprovalId, currentLinkId, currentMediaId, topicName, versionFile, mFolderId, bkAccount, bkTransaction, jEntry,
                        cashAccountTransaction, invoice);

                if (!string.IsNullOrEmpty(versionFile.Uri))
                    new AzureStorageRules(dbContext).ProcessingMediaS3(versionFile.Uri);

                var isMediaInActivity = false;

                var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == currentUserId);

                var notifiRules = new NotificationRules(dbContext);

                var tRule = new TopicRules(dbContext);
                if (media.Topic == null)
                {
                    var topic = tRule.GetTopicByName(topicName, media.Qbicle.Id);
                    if (topic == null)
                    {
                        topic = tRule.SaveTopic(media.Qbicle.Id, topicName);
                    }
                    media.Topic = topic;
                }
                else
                {
                    media.Topic = dbContext.Topics.FirstOrDefault(e => e.Id == media.Topic.Id);
                }

                #region B2C UserView and BusinessView / B2B ConsumerView and Provider View
                var discussion = new DiscussionsRules(dbContext).GetB2CDiscussionOrderByDiscussionId(currentDiscussionId);
                var discussionB2B = new DiscussionsRules(dbContext).GetB2BDiscussionOrderByDiscussionId(currentDiscussionId);
                bool isCustomerView = false;
                if (discussion != null)
                {
                    var b2cqbicle = discussion.Qbicle as B2CQbicle;
                    var isDomainAdmin = b2cqbicle.Business.Administrators.Any(p => p.Id == currentUser.Id);
                    var isMemberOfDomain = b2cqbicle.Business.Users.Any(p => p.Id == currentUser.Id);
                    var isMemberOfQbicle = b2cqbicle.Members.Any(p => p.Id == currentUser.Id);
                    var isCustomerOfBusiness = b2cqbicle.Customer.Id == currentUser.Id;
                    if (isCustomerOfBusiness && isCreatorTheCustomer)
                    {
                        isCustomerView = true;
                    }
                    else if ((isDomainAdmin || (isMemberOfDomain && isMemberOfQbicle)) &&
                        (isCustomerOfBusiness || isCreatorTheCustomer ))
                    {
                        isCustomerView = false;
                    }
                    else isCustomerView = true;
                }
                else if (discussionB2B != null)
                {
                    // Provider and Consumers. 
                    // isCustomerFromB2Bqbicles : User submit B2Border 
                    var b2bqbicle = discussionB2B.Qbicle as B2BQbicle;
                    var isMemberOfB2bqbicle = b2bqbicle.Members.Any(p => p.Id == currentUser.Id);
                    var isCustomerFromB2bqbicle = b2bqbicle.Members.Any(p => p.Id == discussionB2B.TradeOrder.Customer.Id);
                    
                    if(isMemberOfB2bqbicle && isCustomerFromB2bqbicle)
                    {
                        isCustomerView = true;
                    }else if (isMemberOfB2bqbicle && !isCustomerFromB2bqbicle && !isCreatorTheCustomer)
                    {
                        isCustomerView = false;
                    }
                }
                #endregion
                media.IsVisibleInQbicleDashboard = false;
                //Update Viewed Status for Customer in B2CQBicles
                if (media.Qbicle?.Id != 0)
                {
                    var qbicleItem = dbContext.Qbicles.FirstOrDefault(e => e.Id == media.Qbicle.Id);
                    if (qbicleItem != null)
                    {
                        if (qbicleItem is B2CQbicle)
                        {
                            (qbicleItem as B2CQbicle).CustomerViewed = false;
                            (qbicleItem as B2CQbicle).Customer.RemovedQbicle.Remove(qbicleItem);
                            qbicleItem.RemovedForUsers.Remove((qbicleItem as B2CQbicle).Customer);
                        }
                        else if (qbicleItem is C2CQbicle)
                        {
                            var unseenUser = (qbicleItem as C2CQbicle).Customers.FirstOrDefault(p => p.Id != currentUserId);
                            if ((qbicleItem as C2CQbicle).NotViewedBy == null)
                                (qbicleItem as C2CQbicle).NotViewedBy = new List<ApplicationUser>();
                            (qbicleItem as C2CQbicle).NotViewedBy.Add(unseenUser);
                            if (unseenUser.NotViewedQbicle == null)
                                unseenUser.NotViewedQbicle = new List<C2CQbicle>();
                            unseenUser.NotViewedQbicle.Add(qbicleItem as C2CQbicle);

                            qbicleItem.RemovedForUsers.Remove(unseenUser);
                            unseenUser.RemovedQbicle.Remove(qbicleItem);
                        }
                        else if (qbicleItem is B2BQbicle)
                        {
                         
                        }
                    }
                    media.IsVisibleInQbicleDashboard = true;
                }
                media.IsCreatorTheCustomer = isCreatorTheCustomer;


                if (media.Id == 0)
                {
                    media.StartedBy = currentUser;
                    media.StartedDate = DateTime.UtcNow;
                    media.ActivityType = QbicleActivity.ActivityTypeEnum.MediaActivity;
                    media.TimeLineDate = DateTime.UtcNow;

                    media.IsVisibleInQbicleDashboard = false;

                    if (versionFile != null)
                    {
                        versionFile.IsDeleted = false;
                        versionFile.FileSize = versionFile.FileSize;
                        versionFile.UploadedBy = currentUser;
                        versionFile.UploadedDate = DateTime.UtcNow;
                        versionFile.Uri = versionFile.Uri;
                        versionFile.FileType = media.FileType;

                        media.VersionedFiles.Add(versionFile);
                    }
                    if (mFolderId > 0)
                    {
                        var generalFolder = new MediaFolderRules(dbContext).GetMediaFolderById(mFolderId, media.Qbicle.Id);
                        media.MediaFolder = generalFolder;
                    }
                    else
                    {
                        var generalFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, media.Qbicle.Id, currentUserId);
                        media.MediaFolder = generalFolder;
                    }

                    if (bkAccount == null)
                    {
                        media.Qbicle = media.Qbicle;
                        media.Qbicle.LastUpdated = DateTime.UtcNow;
                    }
                    else
                    {
                        media.Qbicle.LastUpdated = DateTime.UtcNow;
                    }

                    if (currentTaskId > 0)// add media to Task
                    {
                        var task = new TasksRules(dbContext).GetTaskById(currentTaskId);
                        task.TimeLineDate = DateTime.UtcNow;
                        task.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        task.SubActivities.Add(media);
                        task.Qbicle.LastUpdated = DateTime.UtcNow;
                        task.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;
                        MediaUploadNotification(versionFile.Uri, task.Id, media.Id, currentUser, NotificationEventEnum.TaskUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if (currentAlertId > 0)// add media to Alert
                    {
                        var alert = new AlertsRules(dbContext).GetAlertById(currentAlertId);
                        alert.TimeLineDate = DateTime.UtcNow;
                        alert.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        alert.SubActivities.Add(media);
                        alert.Qbicle.LastUpdated = DateTime.UtcNow;
                        alert.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, alert.Id, media.Id, currentUser, NotificationEventEnum.AlertUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if (currentEventId > 0)// add media to Event
                    {
                        var qevent = new EventsRules(dbContext).GetEventById(currentEventId);
                        qevent.TimeLineDate = DateTime.UtcNow;
                        qevent.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        qevent.SubActivities.Add(media);
                        qevent.Qbicle.LastUpdated = DateTime.UtcNow;
                        qevent.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, qevent.Id, media.Id, currentUser, NotificationEventEnum.EventUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if (currentLinkId > 0)// add media to Event
                    {
                        var qlink = new LinksRules(dbContext).GetLinkById(currentLinkId);
                        //var startedBy = qlink.StartedBy;
                        qlink.TimeLineDate = DateTime.UtcNow;
                        qlink.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        qlink.SubActivities.Add(media);
                        qlink.Qbicle.LastUpdated = DateTime.UtcNow;
                        qlink.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, qlink.Id, media.Id, currentUser, NotificationEventEnum.LinkUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if (currentDiscussionId > 0 && discussion != null)// add media to Event
                    {
                        var qdiscussion = new DiscussionsRules(dbContext).GetDiscussionById(currentDiscussionId);
                        qdiscussion.TimeLineDate = DateTime.UtcNow;
                        qdiscussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        qdiscussion.SubActivities.Add(media);
                        qdiscussion.Qbicle.LastUpdated = DateTime.UtcNow;
                        qdiscussion.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, qdiscussion.Id, media.Id, currentUser, NotificationEventEnum.DiscussionUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if(currentDiscussionId > 0 && discussionB2B != null)
                    {
                        var qdiscussion = new DiscussionsRules(dbContext).GetB2BDiscussionOrderByDiscussionId(currentDiscussionId);
                        qdiscussion.TimeLineDate = DateTime.UtcNow;
                        qdiscussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        qdiscussion.SubActivities.Add(media);
                        qdiscussion.Qbicle.LastUpdated = DateTime.UtcNow;
                        qdiscussion.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, qdiscussion.Id, media.Id, currentUser, NotificationEventEnum.DiscussionUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if (currentMediaId > 0)// add media to sub media
                    {
                        var qsubMedia = GetMediaById(currentMediaId);
                        qsubMedia.TimeLineDate = DateTime.UtcNow;
                        qsubMedia.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        qsubMedia.SubActivities.Add(media);
                        qsubMedia.Qbicle.LastUpdated = DateTime.UtcNow;
                        qsubMedia.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, qsubMedia.Id, media.Id, currentUser, NotificationEventEnum.MediaUpdate, ApplicationPageName.Activity, originatingConnectionId);

                    }
                    else if (currentApprovalId > 0)// add media to Event
                    {
                        var app = new ApprovalsRules(dbContext).GetApprovalById(currentApprovalId);
                        app.TimeLineDate = DateTime.UtcNow;
                        app.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewFiles;
                        app.SubActivities.Add(media);
                        app.Qbicle.LastUpdated = DateTime.UtcNow;
                        app.IsCreatorTheCustomer = isCreatorTheCustomer;
                        dbContext.SaveChanges();
                        isMediaInActivity = true;

                        MediaUploadNotification(versionFile.Uri, app.Id, media.Id, currentUser, NotificationEventEnum.ApprovalUpdate, ApplicationPageName.Activity, originatingConnectionId);
                    }
                    else if (bkAccount != null)
                    {
                        media.Description = "Attachment of the Account: " + bkAccount.Name;
                        bkAccount.AssociatedFiles.Add(media);
                        dbContext.SaveChanges();
                    }
                    else if (bkTransaction != null)
                    {
                        media.Description = "Attachment of the Transaction: " + bkTransaction.Account.Name;
                        bkTransaction.AssociatedFiles.Add(media);

                        dbContext.SaveChanges();
                    }
                    else if (jEntry != null)
                    {
                        media.Description = "Attachment of the Journal Entry number: " + jEntry.Number;
                        media.JournalEntry = jEntry;
                        jEntry.AssociatedFiles.Add(media);
                        dbContext.SaveChanges();
                    }
                    else if (cashAccountTransaction != null)
                    {
                        media.Description = "Attachment of the Cash AccountTransaction number: " + cashAccountTransaction.Id;
                        media.CashAccountTransaction = cashAccountTransaction;
                        cashAccountTransaction.AssociatedFiles.Add(media);
                        dbContext.SaveChanges();
                    }
                    else if (invoice != null)
                    {
                        media.Description = "Attachment of the bill invoice number: " + invoice.Id;
                        media.Invoice = invoice;
                        invoice.AssociatedFiles.Add(media);

                        dbContext.SaveChanges();
                    }
                    else
                    {
                        media.IsVisibleInQbicleDashboard = true;
                        dbContext.Medias.Add(media);
                        dbContext.Entry(media).State = EntityState.Added;
                        dbContext.SaveChanges();
                        isMediaInActivity = false;
                        MediaUploadNotification(versionFile.Uri, media.Id, media.Id, currentUser, NotificationEventEnum.MediaCreation, ApplicationPageName.Activities, originatingConnectionId);
                    }

                }
                else
                {
                    if (dbContext.Entry(media).State == EntityState.Detached)
                        dbContext.Medias.Attach(media);
                    dbContext.Entry(media).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    MediaUploadNotification(versionFile.Uri, media.Id, media.Id, currentUser, NotificationEventEnum.MediaUpdate, ApplicationPageName.Activity, originatingConnectionId);
                }
                //var nRule = new NotificationRules(dbContext);

                //var activityNotification = new ActivityNotification
                //{
                //    OriginatingConnectionId = originatingConnectionId,
                //    Id = media.Id,
                //    EventNotify = NotificationEventEnum.MediaCreation,
                //    AppendToPageName = ApplicationPageName.Activities,
                //    AppendToPageId = 0,
                //    CreatedById = currentUser.Id,
                //    CreatedByName = currentUser.GetFullName(),
                //    ReminderMinutes = 0
                //};
                //nRule.Notification2Activity(activityNotification);

                string lastUpdate, createDate;

                var startDate = media.StartedDate;
                var lastVersion = media.VersionedFiles.Where(e => !e.IsDeleted)
                    .OrderByDescending(x => x.UploadedDate).First();

                if (startDate.Date == DateTime.UtcNow.Date)
                    createDate = "Today, " + startDate.ToString("hh:mmtt");
                else
                    createDate = startDate.ToString("dd/MM/yyyy hh:mmtt");
                if (lastVersion.UploadedDate.Date == DateTime.UtcNow.Date)
                    lastUpdate = "Today, " + startDate.ToString("hh:mmtt");
                else
                    lastUpdate = startDate.ToString("dd/MM/yyyy hh:mmtt");

                var imgPath = versionFile.FileType.Type == "Image File"
                    ? versionFile.Uri.ToUriString()
                    : versionFile.FileType.ImgPath;
                if (isMediaInActivity)
                    result.Object2 = new
                    {
                        CreatedByName = HelperClass.GetFullNameOfUser(media.StartedBy, currentUser.Id),
                        CreatedAvatar = media.StartedBy.ProfilePic.ToUriString(),
                        topic = new Topic
                        {
                            Id = media.Topic.Id,
                            Name = media.Topic.Name
                        },
                        media.Key,
                        media.Id,
                        FolderId = media.MediaFolder.Id,
                        ImgPath = imgPath,
                        media.Name,
                        media.FileType.Extension,
                        media.FileType.Type,
                        media.Description,
                        CreatedDate = createDate,
                        LastUpdate = lastUpdate
                    };

                result.actionVal = media.Id;



                if (!string.IsNullOrEmpty(originatingConnectionId))
                {
                    var notification = new Notification
                    {
                        AssociatedQbicle = media.Qbicle,
                        CreatedBy = media.StartedBy,
                        IsCreatorTheCustomer = media.IsCreatorTheCustomer,
                    };
                    if (appType == AppType.Web)
                        if (!isMediaOntab)
                            result.msg = new ISignalRNotification().HtmlRender(media, currentUser, ApplicationPageName.Activities, NotificationEventEnum.DiscussionCreation, notification, isCustomerView);
                        else
                            result.msg = new ISignalRNotification().HtmlRender(media, currentUser, ApplicationPageName.Activities, NotificationEventEnum.MediaTabCreation, notification, isCustomerView);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(media, media.StartedDate, null, currentUser.Id, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.DiscussionCreation, notification);
                }

                dbContext.Entry(media).State = EntityState.Unchanged;
                result.result = true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, media, null, currentDiscussionId, currentTaskId,
                        currentEventId, currentAlertId, currentApprovalId, currentLinkId, currentMediaId, topicName, versionFile, mFolderId, bkAccount, bkTransaction, jEntry,
                        cashAccountTransaction, invoice);

                result.result = false;
            }
            return result;

        }

        public ReturnJsonModel AddPostToMedia(int mediaId, QbiclePost post, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add post to media", null, null, mediaId, post);

                var media = GetMediaById(mediaId);
                media.TimeLineDate = DateTime.UtcNow;
                media.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                media.Posts.Add(post);
                media.IsCreatorTheCustomer = post.IsCreatorTheCustomer;
                media.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.Medias.Attach(media);
                dbContext.Entry(media).State = EntityState.Modified;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = mediaId,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.MediaUpdate,
                    AppendToPageName = ApplicationPageName.Media,
                    AppendToPageId = mediaId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);

                var result = new ReturnJsonModel { result = true };


                if (!string.IsNullOrEmpty(originatingConnectionId))
                {

                    var notification = new Notification
                    {
                        AssociatedQbicle = post.Topic.Qbicle,
                        CreatedBy = post.CreatedBy,
                        IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                    };
                    if (appType == AppType.Web)
                        result.msg = new ISignalRNotification().HtmlRender(post, post.CreatedBy, ApplicationPageName.Media, NotificationEventEnum.MediaUpdate, notification);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(post, post.StartedDate, null, post.CreatedBy.Id, post.CreatedBy.DateFormat, post.CreatedBy.Timezone, false, NotificationEventEnum.MediaUpdate, notification);
                }

                dbContext.Entry(post).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, mediaId, post);
                return new ReturnJsonModel { result = false };
            }
        }

        private void MediaUploadNotification(string objectKey, int activityId, int mediaId, ApplicationUser created, NotificationEventEnum eventNotify, ApplicationPageName appendToPageName, string originatingConnectionId = "")
        {
            var activityNotify = new ActivityNotification
            {
                OriginatingConnectionId = originatingConnectionId,
                Id = activityId,
                ObjectById = mediaId.ToString(),
                CreatedByName = HelperClass.GetFullNameOfUser(created),
                CreatedById = created.Id,
                EventNotify = eventNotify,
                ReminderMinutes = 0,
                S3ObjectUploadedItem = new AwsS3ObjectItem
                {
                    ObjectKey = objectKey,
                    IsPublic = false
                },
                AppendToPageName = appendToPageName
            };
            new NotificationRules(dbContext).Notification2MediaUpload(activityNotify);
        }

        /// <summary>
        /// Get list Medias by qbicle id
        /// </summary>
        /// <param name="cubeId">int Qbicle Id</param>
        /// <param name="orderBy"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public List<QbicleMedia> GetMediasByQbicleId(int cubeId, Enums.OrderByDate orderBy, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get medias by qbicle id", null, null, cubeId, orderBy, topicId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        if (topicId == 0)
                            return dbContext.Medias.Where(c => c.Qbicle.Id == cubeId && c.Topic != null
                            && c.IsVisibleInQbicleDashboard).OrderByDescending(d => d.TimeLineDate).ToList();
                        return dbContext.Medias.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId
                        && c.IsVisibleInQbicleDashboard).OrderByDescending(d => d.TimeLineDate).ToList();
                    case Enums.OrderByDate.OldestFirst:
                        if (topicId == 0)
                            return dbContext.Medias.Where(c => c.Qbicle.Id == cubeId && c.Topic != null
                            && c.IsVisibleInQbicleDashboard).OrderBy(d => d.TimeLineDate).ToList();
                        return dbContext.Medias.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId
                        && c.IsVisibleInQbicleDashboard).OrderBy(d => d.TimeLineDate).ToList();
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, topicId);
            }
            return new List<QbicleMedia>();

        }

        public List<QbicleMedia> GetMediaByListId(List<int> listMedias)
        {
            var resutl = new List<QbicleMedia>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get media by list id", null, null, listMedias);

                resutl = dbContext.Medias.Where(x => listMedias.Contains(x.Id)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, listMedias);
            }
            return resutl;
        }

        public IEnumerable<DateTime> LoadMoreMedias(int cubeId, int size,
            ref List<QbicleMedia> medias, ref int acivitiesDateCount, string currentTimeZone)
        {
            IEnumerable<DateTime> activitiesDate = null;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more medias", null, null, cubeId, size, medias, acivitiesDateCount, currentTimeZone);


                var qbicleMedias = this.GetMediasByQbicleId(cubeId).BusinessMapping(currentTimeZone);

                medias = qbicleMedias;
                var taskDates = from t in qbicleMedias select t.TimeLineDate.Date;

                var disDates = taskDates;
                acivitiesDateCount = disDates.Count();

                disDates = disDates.Distinct().OrderByDescending(d => d.Date.Date);
                activitiesDate = disDates.OrderByDescending(d => d.Date).Skip(size).Take(HelperClass.qbiclePageSize);

                medias = qbicleMedias.Where(o => activitiesDate.Contains(o.TimeLineDate.Date)).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, medias, acivitiesDateCount, currentTimeZone);
            }

            return activitiesDate;
        }

        public List<QbicleMedia> GetMediasByQbicleId(int cubeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get medias by qbicle id", null, null, cubeId);

                var qbicleMedia = dbContext.Medias.Where(c => c.Qbicle.Id == cubeId).ToList();
                return qbicleMedia;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId);
                return new List<QbicleMedia>();
            }
        }


        public QbicleMedia GetMediaById(int mediaId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get media by id", null, null, mediaId);

                return dbContext.Medias.FirstOrDefault(e => e.Id == mediaId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, mediaId);
                return new QbicleMedia();
            }
        }

        public List<QbicleActivity> GetActivityMedias(int activityId, int pageIndex, out bool endOfOlder)
        {
            endOfOlder = false;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get activity medias", null, null, activityId);

                var medias = dbContext.Activities.FirstOrDefault(e => e.Id == activityId)?.SubActivities;
                var totalSize = medias.Count;
                medias = medias.OrderByDescending(d => d.TimeLineDate).Skip(pageIndex).Take(HelperClass.activitiesPageSize).ToList();

                if (totalSize < pageIndex)
                    endOfOlder = true;

                return medias;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId);
                return new List<QbicleActivity>();
            }

        }
        public List<QbicleMedia> GetJournalEntryMedias(int journalEntryId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Medias of Journal Entry", null, null, journalEntryId);

                return dbContext.Medias.Where(j => j.JournalEntry.Id == journalEntryId).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalEntryId);
                return new List<QbicleMedia>();
            }

        }
        public IEnumerable<DateTime> GetMediasDate(List<QbicleMedia> qbicleEvent)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Medias by date", null, null, qbicleEvent);

                var eventDates = from t in qbicleEvent select t.TimeLineDate.Date;
                return eventDates.Distinct();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleEvent);
                return new List<DateTime>();
            }

        }

        public VersionedFileDisplay SaveVersionFile(bool isCreatorTheCustomer, int mediaId, string userId, string currentTimeZone, MediaModel mediaModel, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save version file", userId, null, mediaId, currentTimeZone, mediaModel);

                if (!string.IsNullOrEmpty(mediaModel.UrlGuid))
                {
                    var s3Rules = new Azure.AzureStorageRules(dbContext);

                    s3Rules.ProcessingMediaS3(mediaModel.UrlGuid);

                }
                var notifiRules = new NotificationRules(dbContext);
                var media = GetMediaById(mediaId);
                media.TimeLineDate = DateTime.UtcNow;
                media.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.AddMediaVersion;
                media.IsCreatorTheCustomer = isCreatorTheCustomer;
                media.FileType = mediaModel.Type;
                var verFile = new VersionedFile
                {
                    IsDeleted = false,
                    FileSize = mediaModel.Size,
                    UploadedBy = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId),
                    UploadedDate = DateTime.UtcNow,
                    Uri = mediaModel.UrlGuid,
                    Media = media,
                    FileType = mediaModel.Type
                };

                dbContext.VersionedFiles.Add(verFile);
                dbContext.Entry(verFile).State = EntityState.Added;
                media.Qbicle.LastUpdated = DateTime.UtcNow;
                dbContext.SaveChanges();


                MediaUploadNotification(verFile.Uri, media.Id, media.Id, verFile.UploadedBy, NotificationEventEnum.MediaAddVersion, ApplicationPageName.Activity, originatingConnectionId);

                //var activityNotify = new ActivityNotification
                //{
                //    OriginatingConnectionId = originatingConnectionId,
                //    AppendToPageName = ApplicationPageName.Activities,
                //    CreatedById = userId,
                //    ReminderMinutes = 0,
                //    Id = mediaId,
                //    CreatedByName = verFile.UploadedBy.GetFullName(),
                //    EventNotify = NotificationEventEnum.MediaUpdate
                //};
                //notifiRules.Notification2MediaUpload(activityNotify);

                var fileDisplay = new VersionedFileDisplay
                {
                    FileSize = mediaModel.Size,
                    Uri = mediaModel.UrlGuid,
                    Id = verFile.Id,
                    UploadedDate = verFile.UploadedDate.ConvertTimeFromUtc(currentTimeZone).ToString("MMMM dd,yyyy HH:mm")
                };
                return fileDisplay;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, mediaId, currentTimeZone, mediaModel);
                return null;
            }
        }

        public ReturnJsonModel DeleteVersionFile(bool isCreatorTheCustomer, int versionFileId, string userId, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete version file", userId, null, versionFileId);


                var versionDel = dbContext.VersionedFiles.FirstOrDefault(e => e.Id == versionFileId);

                if (versionDel?.Media.VersionedFiles.Count(v => !v.IsDeleted) == 1)
                    return new ReturnJsonModel { result = false, msg = ResourcesManager._L("ERROR_MSG_320") };


                var versionCurr = dbContext.VersionedFiles.Where(e => e.Id != versionFileId && !e.IsDeleted).OrderByDescending(d => d.UploadedDate).FirstOrDefault();
                var media = versionDel?.Media;
                if (media == null) return new ReturnJsonModel { result = false, msg = "Version not found" };
                media.TimeLineDate = DateTime.UtcNow;
                media.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.DelMediaVersion;
                media.Qbicle.LastUpdated = DateTime.UtcNow;
                media.FileType = versionCurr?.FileType;
                media.IsCreatorTheCustomer = isCreatorTheCustomer;
                //var startby = media.StartedBy;
                versionDel.IsDeleted = true;
                if (dbContext.Entry(versionDel).State == EntityState.Detached)
                    dbContext.VersionedFiles.Attach(versionDel);
                dbContext.Entry(versionDel).State = EntityState.Modified;
                dbContext.SaveChanges();
                var activityNotify = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    AppendToPageName = ApplicationPageName.Activities,
                    CreatedById = userId,
                    ReminderMinutes = 0,
                    Id = media.Id,
                    CreatedByName = HelperClass.GetFullNameOfUser(media.StartedBy),
                    EventNotify = NotificationEventEnum.MediaRemoveVersion,
                    ObjectById = media.Id.ToString(),
                };
                new NotificationRules(dbContext).Notification2MediaUpload(activityNotify);

                //MediaUploadNotification(verFile.Uri, media.Id, media.Id, verFile.UploadedBy, NotificationEventEnum.MediaUpdate, originatingConnectionId);

                return new ReturnJsonModel { result = true };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, versionFileId);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }

        public VersionedFile GetVersionedFileById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get versioned file by id", null, null, id);

                return dbContext.VersionedFiles.FirstOrDefault(e => e.Id == id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return null;
            }

        }

        /// <summary>
        /// change folder
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="nFolderId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel MediaMoveFolderById(int mediaId, int nFolderId, string userId, bool isCreatorTheCustomer)
        {
            ReturnJsonModel refModel = new ReturnJsonModel { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MediaMoveFolderById", null, null, mediaId, nFolderId);

                var media = dbContext.Medias.FirstOrDefault(e => e.Id == mediaId);
                if (media != null)
                {
                    var newFolder = dbContext.MediaFolders.FirstOrDefault(e => e.Id == nFolderId);
                    if (newFolder != null)
                    {
                        media.IsCreatorTheCustomer = isCreatorTheCustomer;
                        media.MediaFolder = newFolder;
                        dbContext.SaveChanges();
                    }
                    refModel.result = true;
                }
                else
                {
                    refModel.msg = JsonConvert.SerializeObject(new ErrorMessageModel("ERROR_MSG_677", null));
                }
                var versionFile = media.VersionedFiles.OrderByDescending(e => e.UploadedDate).FirstOrDefault();
                var currentUser = dbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);
                MediaUploadNotification(versionFile.Uri, media.Id, media.Id, currentUser, NotificationEventEnum.MediaUpdate, ApplicationPageName.Activity);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, mediaId, nFolderId);
                refModel.msg = "An error occurred while move the folder";
            }


            return refModel;
        }
        public StorageFile GetFileInfoByURI(string uri)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "MediaMoveFolderById", null, null, uri);

                return dbContext.StorageFiles.FirstOrDefault(e => e.Id == uri);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, uri);
                return null;
            }
        }

        public void SetMediaIsPublish(QbicleMedia media)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Set media into public", null, null, media);

                var vsFile = media.VersionedFiles.OrderByDescending(s => s.UploadedDate).FirstOrDefault();
                if (vsFile != null)
                {
                    var stFile = dbContext.StorageFiles.FirstOrDefault(e => e.Id == vsFile.Uri);
                    if (stFile != null)
                    {
                        stFile.IsPublic = true;
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region Bill attachments
        public void SaveNewAttachmentsBill(int billId, List<MediaModel> traderBillAttachments, string userId)
        {
            var bill = new TraderInvoicesRules(dbContext).GetById(billId);
            // add new file
            if (bill.AssociatedFiles == null)
                bill.AssociatedFiles = new List<QbicleMedia>();

            var mediaRules = new MediasRules(dbContext);
            traderBillAttachments.ForEach(file =>
            {
                try
                {
                    var extention = string.IsNullOrEmpty(file.Extension) ? Path.GetExtension(file.Name) : file.Extension;
                    var media = new QbicleMedia
                    {
                        Topic = bill.Workgroup.Topic,
                        Qbicle = bill.Workgroup.Qbicle,
                        FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extention),
                        Name = file.Name
                    };
                    var versionFile = new VersionedFile()
                    {
                        Uri = file.Id,
                        FileSize = HelperClass.FileSize(int.Parse(file.Size))
                    };

                    SaveMedia(media, false, userId, false, 0, 0, 0, 0, 0, 0, 0, media.Topic.Name, versionFile, 0, null, null, null, null, bill);
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            });

        }

        public void UpdateAttachmentsBill(Invoice bill, int billId)
        {
            try
            {
                var traderBill = new TraderInvoicesRules(dbContext).GetById(billId);
                traderBill.AssociatedFiles.Clear();
                dbContext.SaveChanges();

                foreach (var item in bill.AssociatedFiles)
                {
                    var qbMedia = dbContext.Medias.FirstOrDefault(e => e.Id == item.Id);

                    if (qbMedia != null)
                    {
                        qbMedia.Name = item.Name;
                        traderBill.AssociatedFiles.Add(qbMedia);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }
        #endregion

        #region Invoice attachments
        public void SaveNewAttachmentsInvoice(CashAccountTransaction payment, List<MediaModel> traderBillAttachments, string userId)
        {
            //var bill = new TraderInvoicesRules(dbContext).GetById(billId);
            // add new file
            if (payment.AssociatedFiles == null)
                payment.AssociatedFiles = new List<QbicleMedia>();

            var mediaRules = new MediasRules(dbContext);
            traderBillAttachments.ForEach(file =>
            {
                try
                {
                    var extention = string.IsNullOrEmpty(file.Extension) ? Path.GetExtension(file.Name) : file.Extension;
                    var media = new QbicleMedia
                    {
                        Topic = payment.Workgroup.Topic,
                        Qbicle = payment.Workgroup.Qbicle,
                        FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extention),
                        Name = file.Name
                    };
                    var versionFile = new VersionedFile()
                    {
                        Uri = file.Id,
                        FileSize = HelperClass.FileSize(int.Parse(file.Size))
                    };

                    mediaRules.SaveMedia(media, false, userId, false, 0, 0, 0, 0, 0, 0, 0, media.Topic.Name, versionFile, 0, null, null, null, payment);

                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            });

        }

        public void UpdateAttachmentsInvoice(CashAccountTransaction paymentAttachments, CashAccountTransaction payment)
        {
            try
            {

                payment.AssociatedFiles.Clear();
                dbContext.SaveChanges();

                foreach (var item in paymentAttachments.AssociatedFiles)
                {
                    var qbMedia = dbContext.Medias.FirstOrDefault(e => e.Id == item.Id);

                    if (qbMedia != null)
                    {
                        qbMedia.Name = item.Name;
                        payment.AssociatedFiles.Add(qbMedia);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }
        #endregion

        #region BK Account attachments
        public void SaveNewAttachmentsBkAccount(BKAccount account, List<MediaModel> bkAccountAttachments, string userId)
        {
            bkAccountAttachments.ForEach(file =>
            {
                try
                {
                    var extention = string.IsNullOrEmpty(file.Extension) ? Path.GetExtension(file.Name) : file.Extension;
                    var media = new QbicleMedia
                    {
                        Topic = account.WorkGroup?.Topic,
                        Qbicle = account.WorkGroup?.Qbicle,
                        FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extention),
                        Name = file.Name
                    };
                    var versionFile = new VersionedFile()
                    {
                        Uri = file.Id,
                        FileSize = HelperClass.FileSize(int.Parse(file.Size))
                    };

                    SaveMedia(media, false, userId, false, 0, 0, 0, 0, 0, 0, 0, media.Topic.Name ?? "", versionFile, 0, account);
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            });

        }

        public void UpdateAttachmentsBkAccount(BKAccount bkAcount)
        {
            try
            {
                foreach (var item in bkAcount.AssociatedFiles)
                {
                    var qbMedia = dbContext.Medias.FirstOrDefault(q => q.Id == item.Id);
                    if (qbMedia != null)
                    {
                        qbMedia.Name = item.Name;
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }
        #endregion

        #region CashAccount Transaction attachments
        public void SaveNewAttachmentsCashAccountTransaction(CashAccountTransaction cashAccountTransaction, List<MediaModel> traderCashBankAttachments, string userId)
        {
            // add new file
            if (cashAccountTransaction.AssociatedFiles == null)
                cashAccountTransaction.AssociatedFiles = new List<QbicleMedia>();

            var mediaRules = new MediasRules(dbContext);
            traderCashBankAttachments.ForEach(file =>
            {
                try
                {
                    var extention = string.IsNullOrEmpty(file.Extension) ? Path.GetExtension(file.Name) : file.Extension;
                    var media = new QbicleMedia
                    {
                        Topic = cashAccountTransaction.Workgroup.Topic,
                        Qbicle = cashAccountTransaction.Workgroup.Qbicle,
                        FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extention),
                        Name = file.Name
                    };
                    var versionFile = new VersionedFile()
                    {
                        Uri = file.Id,
                        FileSize = HelperClass.FileSize(int.Parse(file.Size))
                    };
                    SaveMedia(media, false, userId, false, 0, 0, 0, 0, 0, 0, 0, media.Topic.Name, versionFile, 0, null, null, null, cashAccountTransaction);
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            });

        }

        public void UpdateAttachmentsCashAccountTransaction(CashAccountTransaction traderCashBankAssociatedFiles, CashAccountTransaction cashAccountTransaction)
        {
            try
            {
                cashAccountTransaction.AssociatedFiles.Clear();
                dbContext.SaveChanges();

                foreach (var item in traderCashBankAssociatedFiles.AssociatedFiles)
                {
                    var qbMedia = dbContext.Medias.FirstOrDefault(e => e.Id == item.Id);

                    if (qbMedia != null)
                    {
                        qbMedia.Name = item.Name;
                        cashAccountTransaction.AssociatedFiles.Add(qbMedia);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }
        #endregion

        #region On review Journal Entry, can upload media to transactions row
        public void SaveOnReviewNewAttachmentsBKTransaction(BKTransaction bkTransaction, List<MediaModel> mediaAttachments, string userId)
        {
            //var bill = new TraderInvoicesRules(dbContext).GetById(billId);
            // add new file
            if (bkTransaction.AssociatedFiles == null)
                bkTransaction.AssociatedFiles = new List<QbicleMedia>();

            var mediaRules = new MediasRules(dbContext);
            mediaAttachments.ForEach(file =>
            {
                try
                {
                    var extention = string.IsNullOrEmpty(file.Extension) ? Path.GetExtension(file.Name) : file.Extension;
                    var qbicle = bkTransaction.JournalEntry.WorkGroup?.Qbicle;
                    var topic = bkTransaction.JournalEntry.WorkGroup?.Topic;

                    if (qbicle == null)
                    {
                        var bkAppsetting = dbContext.BKAppSettings.Where(q => q.Domain.Id == bkTransaction.JournalEntry.Domain.Id).FirstOrDefault();
                        topic = bkAppsetting.AttachmentDefaultTopic;
                        qbicle = bkAppsetting.AttachmentQbicle;
                    }
                    var media = new QbicleMedia
                    {
                        Topic = topic,
                        Qbicle = qbicle,
                        FileType = new FileTypeRules(dbContext).GetFileTypeByExtension(extention),
                        Name = file.Name
                    };
                    var versionFile = new VersionedFile()
                    {
                        Uri = file.Id.ToString(),
                        FileSize = HelperClass.FileSize(int.Parse(file.Size))
                    };

                    SaveMedia(media, false, userId, false, 0, 0, 0, 0, 0, 0, 0, media.Topic?.Name ?? "", versionFile, 0, null, bkTransaction);

                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                }
            });

        }

        public void UpdateOnReviewAttachmentsBkTransaction(BKTransaction bkAttachments, BKTransaction bKTransaction)
        {
            try
            {

                bKTransaction.AssociatedFiles.Clear();
                dbContext.SaveChanges();

                foreach (var item in bkAttachments.AssociatedFiles)
                {
                    var qbMedia = dbContext.Medias.FirstOrDefault(e => e.Id == item.Id);

                    if (qbMedia != null)
                    {
                        qbMedia.Name = item.Name;
                        bKTransaction.AssociatedFiles.Add(qbMedia);
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }

        public void UpdateAttachmentsBkTransaction(List<MediaModel> mediaAttachments)
        {
            try
            {
                foreach (var item in mediaAttachments)
                {
                    var id = int.Parse(item.Id);
                    var qbMedia = dbContext.Medias.FirstOrDefault(e => e.Id == id);

                    if (qbMedia != null)
                    {
                        qbMedia.Name = item.Name;
                        dbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
            }

        }
        #endregion

    }
}
