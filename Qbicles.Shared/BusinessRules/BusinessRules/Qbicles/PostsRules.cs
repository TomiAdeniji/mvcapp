using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using Qbicles.Models.B2C_C2C;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class PostsRules
    {
        ApplicationDbContext dbContext;

        public PostsRules()
        {
        }
        public PostsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }


        /// <summary>
        /// Create a new POST
        /// </summary>
        /// <param name="message"></param>
        /// <param name="userId"></param>
        /// <param name="qbicleId">use for set qbicle.LastUpdated</param>
        /// <returns></returns>
        public QbiclePost SavePost(bool isCreatorTheCustomer, string message, string userId, int qbicleId)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Save post", userId, null, message, qbicleId);
            var user = dbContext.QbicleUser.Find(userId);
            var post = new QbiclePost
            {
                CreatedBy = user,
                Message = message,
                StartedDate = DateTime.UtcNow,
                TimeLineDate = DateTime.UtcNow,
                IsCreatorTheCustomer = isCreatorTheCustomer,
                Topic = new TopicRules(dbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicleId)
            };
            var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
            if (qbicle != null)
            {
                qbicle.LastUpdated = DateTime.UtcNow;
            }

            dbContext.Posts.Add(post);
            dbContext.Entry(post).State = EntityState.Added;
            dbContext.SaveChanges();
            return post;
        }

        /// <summary>
        ///  Save post comment on Dashboard
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <param name="topicName"></param>
        /// <param name="currentUserId"></param>
        /// <param name="qbicleId"></param>
        /// <returns></returns>
        public ReturnJsonModel SavePostTopic(bool isCreatorTheCustomer, string message, int topicId, string currentUserId, int qbicleId, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Post Topic from Stream", currentUserId, null, message, topicId, qbicleId);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                var postDate = DateTime.UtcNow;

                var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                Topic topic = dbContext.Topics.Find(topicId);
                if (topic == null)
                {
                    topic = new TopicRules(dbContext).GetTopicByQbicle(qbicleId).FirstOrDefault(x => x.Name == HelperClass.GeneralName);
                }
                qbicle.LastUpdated = DateTime.UtcNow;
                var post = new QbiclePost
                {
                    Message = message,
                    CreatedBy = currentUser,
                    StartedDate = postDate,
                    Topic = topic,
                    TimeLineDate = postDate,
                    IsCreatorTheCustomer = isCreatorTheCustomer
                };
                //var domain = post.Topic?.Qbicle?.Domain;
                dbContext.Posts.Add(post);
                dbContext.Entry(post).State = EntityState.Added;
                dbContext.SaveChanges();


                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = post.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.PostCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = qbicleId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).NotificationComment2Activity(activityNotification);

                result.actionVal = post.Id;
                result.result = true;
                if (topic != null)
                    result.Object = new { topic = new { topic.Id, topic.Name } };


                if (string.IsNullOrEmpty(originatingConnectionId)) return result;

                var notification = new Notification
                {
                    AssociatedQbicle = post.Topic.Qbicle,
                    CreatedBy = post.CreatedBy,
                    IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                };
                if (appType == AppType.Web)
                    result.msg = new ISignalRNotification().HtmlRender(post, currentUser, ApplicationPageName.Activities, NotificationEventEnum.PostCreation, notification);
                else
                    result.Object = MicroStreamRules.GenerateActivity(post, postDate, null, currentUserId, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.PostCreation,notification);
                dbContext.Entry(post).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, message, topicId, qbicleId);
                result.result = false;
                result.msg = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Add post from web or micro app Stream of Qbicles, B2C, B2B, Community
        /// </summary>
        /// <param name="message"></param>
        /// <param name="topicName"></param>
        /// <param name="currentUserId"></param>
        /// <param name="qbicleId"></param>
        /// <param name="originatingConnectionId"></param>
        /// <returns></returns>
        public ReturnJsonModel SavePostTopic(bool isCreatorTheCustomer, string message, string topicName, string currentUserId, int qbicleId, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            var result = new ReturnJsonModel();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Post Topic from Stream", currentUserId, null, message, topicName, qbicleId);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);

                var postDate = DateTime.UtcNow;

                var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                Topic topic;
                if (string.IsNullOrEmpty(topicName))
                {
                    topic = new TopicRules(dbContext).GetTopicByQbicle(qbicleId).FirstOrDefault(x => x.Name == HelperClass.GeneralName);
                }
                else
                {
                    topic = new TopicRules(dbContext).GetTopicByName(topicName, qbicleId) ?? new TopicRules(dbContext).SaveTopic(qbicleId, topicName);
                }
                qbicle.LastUpdated = DateTime.UtcNow;
                if (qbicle is B2CQbicle)
                {
                    if ((qbicle as B2CQbicle).Customer.Id == currentUserId)
                    {
                        (qbicle as B2CQbicle).BusinessViewed = false;
                    }
                    else
                    {
                        (qbicle as B2CQbicle).CustomerViewed = false;
                        (qbicle as B2CQbicle).Customer.RemovedQbicle.Remove(qbicle);
                        qbicle.RemovedForUsers.Remove((qbicle as B2CQbicle).Customer);
                    }
                }
                if (qbicle is C2CQbicle)
                {
                    var unseenCustomer = (qbicle as C2CQbicle).Customers.FirstOrDefault(p => p.Id != currentUserId);
                    if ((qbicle as C2CQbicle).NotViewedBy == null)
                        (qbicle as C2CQbicle).NotViewedBy = new List<ApplicationUser>();
                    (qbicle as C2CQbicle).NotViewedBy.Add(unseenCustomer);
                    if (unseenCustomer.NotViewedQbicle == null)
                        unseenCustomer.NotViewedQbicle = new List<C2CQbicle>();
                    unseenCustomer.NotViewedQbicle.Add(qbicle as C2CQbicle);

                    unseenCustomer.RemovedQbicle.Remove(qbicle);
                    qbicle.RemovedForUsers.Remove(unseenCustomer);
                }

                var post = new QbiclePost
                {
                    Message = message,
                    CreatedBy = currentUser,
                    StartedDate = postDate,
                    Topic = topic,
                    TimeLineDate = postDate,
                    IsCreatorTheCustomer = isCreatorTheCustomer
                };
                //var domain = post.Topic?.Qbicle?.Domain;
                dbContext.Posts.Add(post);
                dbContext.Entry(post).State = EntityState.Added;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);
                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = post.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.PostCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = qbicleId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);

                result.actionVal = post.Id;
                result.result = true;
                //if (topic != null) result.Object = new { topic = new { topic.Id, topic.Name } };


                if (string.IsNullOrEmpty(originatingConnectionId)) return result;


                var notification = new Notification
                {
                    AssociatedQbicle = post.Topic.Qbicle,
                    CreatedBy = post.CreatedBy,
                    IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                };
                if (appType == AppType.Web)
                    result.msg = new ISignalRNotification().HtmlRender(post, currentUser, ApplicationPageName.Activities, NotificationEventEnum.PostCreation, notification);
                else
                    result.Object = MicroStreamRules.GenerateActivity(post, postDate, null, currentUserId, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.PostCreation, notification);
                dbContext.Entry(post).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, message, topicName, qbicleId);
                result.result = false;
                result.msg = ex.Message;
                return result;
            }
        }
        public QbiclePost GetPostById(int id)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get post by id", null, id);

                return dbContext.Posts.Find(id);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return new QbiclePost();
            }
        }

        public List<QbiclePost> GetPosts(int qbicleId, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get posts", null, null, qbicleId, topicId);

                if (topicId == 0)
                    return dbContext.Posts.Where(tp => tp.Topic.Qbicle.Id == qbicleId && tp.Topic != null
                    && tp.JournalEntry == null && tp.BKTransaction == null
                    ).ToList();
                else
                    return dbContext.Posts.Where(tp => tp.Topic.Qbicle.Id == qbicleId && tp.Topic.Id == topicId
                    && tp.JournalEntry == null && tp.BKTransaction == null).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, topicId);
                return new List<QbiclePost>();
            }
        }

        public int GetPostsToday(int qbicleId, string timeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get posts", null, null, qbicleId, timeZone);

                var posts = dbContext.Posts.Where(tp => tp.Topic.Qbicle.Id == qbicleId
                && tp.Topic != null).ToList();
                var p2 = posts.Count(tp => tp.StartedDate.ConvertTimeFromUtc(timeZone).Date == DateTime.UtcNow.Date);
                return p2;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleId, timeZone);
                return 0;
            }
        }

        public List<QbiclePost> GetActivityPosts(int activityId, int pageIndex, out bool endOfOlder)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get posts", null, null, activityId);



                var posts = dbContext.Activities.Find(activityId)?.Posts;


                //var posts = new PostsRules(dbContext).GetActivityPosts(activityId);
                var totalSize = posts.Count;
                posts = posts.OrderByDescending(d => d.TimeLineDate).Skip(pageIndex).Take(HelperClass.activitiesPageSize).ToList();

                endOfOlder = false;
                if (totalSize <= (pageIndex + HelperClass.activitiesPageSize))
                    endOfOlder = true;

                return posts;// dbContext.Activities.Find(activityId)?.Posts ?? new List<QbiclePost>();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, activityId);
                endOfOlder = false;
                return new List<QbiclePost>();
            }
        }
        public List<QbiclePost> GetJournalEntryPosts(int journalEntryId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "GetJournalEntryPosts", null, null, journalEntryId);

                return dbContext.Posts.Where(p => p.JournalEntry.Id == journalEntryId).ToList();


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, journalEntryId);
                return new List<QbiclePost>();
            }


        }
        public bool SavePostJournal(string message, string userId, int id, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Post Journal", null, null, message, id);

                var user = dbContext.QbicleUser.Find(userId);

                var journalEntry = new JournalEntryRules(dbContext).GetById(id);

                var qbicle = journalEntry.WorkGroup?.Qbicle;
                var topic = journalEntry.WorkGroup?.Topic;

                if (qbicle == null)
                {
                    var bkAppsetting = dbContext.BKAppSettings.Where(q => q.Domain.Id == journalEntry.Domain.Id).FirstOrDefault();
                    topic = bkAppsetting.AttachmentDefaultTopic;
                    qbicle = bkAppsetting.AttachmentQbicle;
                }


                var post = new QbiclePost
                {
                    CreatedBy = user,
                    Message = message,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Topic = topic,
                    JournalEntry = journalEntry
                };

                qbicle.LastUpdated = DateTime.UtcNow;
                journalEntry.Posts.Add(post);


                dbContext.JournalEntrys.Attach(journalEntry);
                dbContext.Entry(journalEntry).State = EntityState.Modified;
                dbContext.SaveChanges();


                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = journalEntry.Approval?.Id ?? journalEntry.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.JournalPost,
                    AppendToPageName = ApplicationPageName.bookkeeping,
                    AppendToPageId = id,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);
                return true;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, message, id);
                return false;
            }


        }

        public bool SavePostTransaction(string message, string userId, int id, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Post Transaction", null, null, message, id);


                var user = dbContext.QbicleUser.Find(userId);
                var bkTransaction = new BKTransactionRules(dbContext).GetById(id);

                var qbicle = bkTransaction.JournalEntry.WorkGroup?.Qbicle;
                var topic = bkTransaction.JournalEntry.WorkGroup?.Topic;

                if (qbicle == null)
                {
                    var bkAppsetting = dbContext.BKAppSettings.Where(q => q.Domain.Id == bkTransaction.JournalEntry.Domain.Id).FirstOrDefault();
                    topic = bkAppsetting.AttachmentDefaultTopic;
                    qbicle = bkAppsetting.AttachmentQbicle;
                }

                var post = new QbiclePost
                {
                    CreatedBy = user,
                    Message = message,
                    StartedDate = DateTime.UtcNow,
                    TimeLineDate = DateTime.UtcNow,
                    Topic = topic,
                    BKTransaction = bkTransaction
                };

                qbicle.LastUpdated = DateTime.UtcNow;
                bkTransaction.Posts.Add(post);

                dbContext.BKTransactions.Attach(bkTransaction);
                dbContext.Entry(bkTransaction).State = EntityState.Modified;
                dbContext.SaveChanges();


                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = bkTransaction.JournalEntry.Approval?.Id ?? bkTransaction.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.TransactionPost,
                    AppendToPageName = ApplicationPageName.bookkeeping,
                    AppendToPageId = bkTransaction.JournalEntry.Id,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);
                return true;


            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, id);
                return false;
            }


        }
        public ReturnJsonModel DeletePost(int postId, string currentUserId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete Post from Stream", null, postId);
                var post = dbContext.Posts.Find(postId);
                if (post != null)
                {
                    if (post.CreatedBy.Id != currentUserId)
                    {
                        returnJson.result = false;
                        returnJson.msg = "ERROR_MSG_28";
                    }
                    var notifications = dbContext.Notifications.Where(s => s.AssociatedPost.Id == postId).ToList();
                    if (notifications != null && notifications.Any())
                        dbContext.Notifications.RemoveRange(notifications);
                    post.Media.Clear();
                    post.Folders.Clear();
                    dbContext.Posts.Remove(post);
                    returnJson.result = dbContext.SaveChanges() > 0;
                }
                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, postId);
                returnJson.result = false;
                returnJson.msg = "ERROR_MSG_EXCEPTION_SYSTEM";
                return returnJson;
            }
        }
        public ReturnJsonModel ForwardPost(int postId, int qbicleId, string currentUserId)
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Delete Post from Stream", null, postId, qbicleId, currentUserId);
                var post = dbContext.Posts.Find(postId);
                if (post != null)
                {
                    var clonePost = new QbiclePost
                    {
                        Message = post.Message,
                        Topic = new TopicRules(dbContext).GetTopicByName(HelperClass.GeneralName, qbicleId),
                        StartedDate = DateTime.UtcNow
                    };
                    clonePost.TimeLineDate = clonePost.StartedDate;
                    clonePost.CreatedBy = dbContext.QbicleUser.Find(currentUserId);
                    clonePost.Set = post.Set;
                    dbContext.Posts.Add(clonePost);
                    returnJson.result = dbContext.SaveChanges() > 0;
                }
                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, postId, qbicleId, currentUserId);
                returnJson.result = false;
                returnJson.msg = "ERROR_MSG_EXCEPTION_SYSTEM";
                return returnJson;
            }
        }
        public ReturnJsonModel UpdatePost(int postId, string message, int topicId, string currentUserId, string originatingConnectionId = "")
        {
            var returnJson = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update Post from Stream", null, message, topicId, currentUserId);
                var post = dbContext.Posts.Find(postId);
                if (post != null)
                {
                    post.Message = message;
                    post.Topic = dbContext.Topics.Find(topicId);
                    dbContext.SaveChanges();
                }
                returnJson.result = true;

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = post.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.PostEdit,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = post.Topic.Qbicle.Id,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                new NotificationRules(dbContext).NotificationComment2Activity(activityNotification);

                return returnJson;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, message, topicId, currentUserId);
                returnJson.result = false;
                returnJson.msg = "ERROR_MSG_EXCEPTION_SYSTEM";
                return returnJson;
            }
        }

        public void SavePostPromotionStartEnd(string message, List<string> claimedUsers, string currentUserId, int currentDomainId, string originatingConnectionId = "")
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Save Post Topic from Stream", currentUserId, null, message);

            if (claimedUsers.Count == 0) return;

            var currentUser = dbContext.QbicleUser.Find(currentUserId);

            var b2cQbicles = (from b2c in dbContext.B2CQbicles
                              where !b2c.IsHidden
                              && b2c.Business.Id == currentDomainId
                              && claimedUsers.Contains(b2c.Customer.Id)
                              select b2c).ToList();

            b2cQbicles.ForEach(b2cQbicle =>
            {
                var qbicle = new QbicleRules(dbContext).GetQbicleById(b2cQbicle.Id);
                if (qbicle == null) return;
                qbicle.LastUpdated = DateTime.UtcNow;
                var topic = new TopicRules(dbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id) ?? new TopicRules(dbContext).SaveTopic(qbicle.Id, HelperClass.GeneralName);

                qbicle.LastUpdated = DateTime.UtcNow;

                if ((qbicle as B2CQbicle).Customer.Id == currentUserId)
                {
                    (qbicle as B2CQbicle).BusinessViewed = false;
                }
                else
                {
                    (qbicle as B2CQbicle).CustomerViewed = false;
                    (qbicle as B2CQbicle).Customer.RemovedQbicle.Remove(qbicle);
                    qbicle.RemovedForUsers.Remove((qbicle as B2CQbicle).Customer);
                }
                var post = new QbiclePost
                {
                    Message = message,
                    CreatedBy = currentUser,
                    StartedDate = DateTime.UtcNow,
                    Topic = topic,
                    TimeLineDate = DateTime.UtcNow
                };

                dbContext.Posts.Add(post);
                dbContext.Entry(post).State = EntityState.Added;
                dbContext.SaveChanges();

                var nRule = new NotificationRules(dbContext);
                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = post.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.PostCreation,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = qbicle.Id,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);
            });
        }
    }
}
