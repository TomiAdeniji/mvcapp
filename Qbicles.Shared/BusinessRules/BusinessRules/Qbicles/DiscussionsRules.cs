using DocumentFormat.OpenXml.EMMA;
using Qbicles.BusinessRules.BusinessRules.Commerce;
using Qbicles.BusinessRules.BusinessRules.PayStack;
using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.BusinessRules.Operator;
using Qbicles.BusinessRules.Trader;
using Qbicles.Models;
using Qbicles.Models.B2B;
using Qbicles.Models.B2C_C2C;
using Qbicles.Models.SystemDomain;
using Qbicles.Models.Trader;
using Qbicles.Models.Trader.SalesChannel;
using Qbicles.Models.TraderApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Mvc;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class DiscussionsRules
    {
        ApplicationDbContext _db;

        public DiscussionsRules(ApplicationDbContext context)
        {
            _db = context;
        }
        public ApplicationDbContext DbContext
        {
            get
            {
                return _db ?? new ApplicationDbContext();
            }
            private set
            {
                _db = value;
            }
        }

        public ApplicationUser GetUser(String userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user by id", null, null, userId);

                UserRules ur = new UserRules(DbContext);
                return ur.GetUser(userId, 0);

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }
        }
        /// <summary>
        /// get list the Discussions by qbicle id
        /// </summary>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <returns>List<QbicleDiscussion></returns>
        public List<QbicleDiscussion> GetDiscussionsByQbicleId(int cubeId, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get user by id", null, null, cubeId, topicId);

                if (topicId == 0)
                    return DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId).ToList();
                return DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, topicId);
                return new List<QbicleDiscussion>();
            }
        }
        /// <summary>
        /// Get list the Discussions order by and filter by Qbicle Id
        /// </summary>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="topicId">int: topicId</param>
        /// <returns>List<QbicleDiscussion> order by</returns>
        public List<QbicleDiscussion> GetDiscussionsOrderByQbicleId(int cubeId, Enums.OrderByDate orderBy, int topicId = 0)
        {
            List<QbicleDiscussion> discussions = new List<QbicleDiscussion>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions order by qbicle id", null, null, cubeId, orderBy, topicId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        if (topicId == 0)
                            discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId).OrderByDescending(d => d.TimeLineDate).ToList();
                        else
                            discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId).OrderByDescending(d => d.TimeLineDate).ToList();
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        if (topicId == 0)
                            discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId).OrderBy(d => d.TimeLineDate).ToList();
                        else
                            discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId).OrderBy(d => d.TimeLineDate).ToList();
                        break;
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, topicId);
            }
            return discussions;
        }
        /// <summary>
        /// Get the Discussions count by Qbicle Id
        /// </summary>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <returns>discussion count</returns>
        public int GetDiscussionsCountByQbicleId(int cubeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count discussions by qbicle id", null, null, cubeId);

                return DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId).Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId);
                return 0;
            }
        }
        /// <summary>
        /// Get Discussions by Qbicle Id between dates
        /// </summary>
        /// <param name="fromDates">datetime: date from</param>
        /// <param name="toDate">datetime: date to</param>
        /// <param name="cubeId">int: Qbicle Id</param>
        /// <returns>List<QbicleDiscussion></returns>
        public List<QbicleDiscussion> GetDiscussionsBetweenDateByQbicleId(DateTime fromDates, DateTime toDate, int cubeId)
        {
            var tasks = new List<QbicleDiscussion>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions between date by qbicle id", null, null, fromDates, toDate, cubeId);

                tasks = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId).ToList()
                    .Where(s => s.TimeLineDate.Date >= fromDates && s.TimeLineDate <= toDate).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, fromDates, toDate, cubeId);
            }
            return tasks;
        }
        /// <summary>
        /// Get the Discussions by Qbicle Id and order by and take records
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="takeSize">int: Take size</param>
        /// <returns>IQueryable<QbicleDiscussion></returns>
        public IQueryable<QbicleDiscussion> GetDicussionsOrderByTakeQbicleId(int cubeId, Enums.OrderByDate orderBy,
            int takeSize)
        {
            IQueryable<QbicleDiscussion> discussions;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions (after order by and take) by qbicle id", null, null, cubeId, orderBy, takeSize);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId)
                                                .OrderByDescending(d => d.TimeLineDate)
                                                .Take(takeSize);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId)
                                                .OrderBy(d => d.TimeLineDate)
                                                .Take(takeSize);
                        break;
                    default:
                        discussions = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, takeSize);
                discussions = null;
            }
            return discussions;
        }
        /// <summary>
        /// Get the Discussions by Qbicle Id and order by and skip and take records
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="takeSize">int: Take size</param>
        /// <returns>IQueryable<QbicleDiscussion></returns>
        public IQueryable<QbicleDiscussion> GetDicussionsOrderBySkipTakeQbicleId(int cubeId, Enums.OrderByDate orderBy,
            int skipSize, int takeSize)
        {
            IQueryable<QbicleDiscussion> discussions;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions (after order by and take, skip) by qbicle id", null, null, cubeId, orderBy, skipSize, takeSize);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId)
                                                .OrderByDescending(d => d.TimeLineDate)
                                                .Skip(skipSize)
                                                .Take(takeSize);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        discussions = DbContext.Discussions.Where(c => c.Qbicle.Id == cubeId)
                                                .OrderBy(d => d.TimeLineDate)
                                                .Skip(skipSize)
                                                .Take(takeSize);
                        break;
                    default:
                        discussions = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, skipSize, takeSize);
                discussions = null;
            }
            return discussions;
        }
        /// <summary>
        /// Get the Discussions by Qbicle Id and order by and take records has been assigned to the current user
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="takeSize">int: Take size</param>
        /// <returns>IQueryable<QbicleDiscussion></returns>
        public IQueryable<QbicleDiscussion> GetDicussionsOrderByTakeQbicleUserId(int cubeId, Enums.OrderByDate orderBy,
            int takeSize, string curentUserId)
        {
            IQueryable<QbicleDiscussion> discussions;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions (after order by and take) by qbicle id and user id", curentUserId, null, cubeId, orderBy, takeSize, curentUserId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        discussions = DbContext.Discussions.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                                        .OrderByDescending(d => d.TimeLineDate)
                                        .Take(takeSize)
                                        .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        discussions = DbContext.Discussions.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                                        .OrderBy(d => d.TimeLineDate)
                                        .Take(takeSize)
                                        .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    default:
                        discussions = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, cubeId, orderBy, takeSize, curentUserId);
                discussions = null;
            }
            return discussions;
        }
        /// <summary>
        /// Get the Discussions by Qbicle Id and order by and skip and take records has been assigned to the current user
        /// </summary>
        /// <param name="cubeId">int: QbicleId</param>
        /// <param name="orderBy">Enums.OrderByDate</param>
        /// <param name="takeSize">int: Take size</param>
        /// <returns>IQueryable<QbicleDiscussion></returns>
        public IQueryable<QbicleDiscussion> GetDicussionsOrderBySkipTakeQbicleUserId(int cubeId, Enums.OrderByDate orderBy,
            int skipSize, int takeSize, string curentUserId)
        {
            IQueryable<QbicleDiscussion> discussions;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions (after order by and take, skip) by qbicle id and user id", curentUserId, null, cubeId, orderBy, skipSize, takeSize, curentUserId);


                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:

                        discussions = DbContext.Discussions.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                                        .OrderByDescending(d => d.TimeLineDate)
                                        .Skip(skipSize)
                                        .Take(takeSize)
                                        .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    case Enums.OrderByDate.OldestFirst:
                        discussions = DbContext.Discussions.Where(t => t.ActivityMembers.Any(am => am.Id == curentUserId))
                                        .OrderBy(d => d.TimeLineDate)
                                        .Skip(skipSize)
                                        .Take(takeSize)
                                        .Where(t => t.Qbicle.Id == cubeId);
                        break;
                    default:
                        discussions = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, cubeId, orderBy, skipSize, takeSize, curentUserId);
                discussions = null;
            }
            return discussions;
        }


        /// <summary>
        /// Get Discussions rules
        /// </summary>
        /// <param name="filterType"></param>
        /// <param name="orderDate"></param>
        /// <param name="size"></param>
        /// <param name="typeGet">1: filter,2: Load next</param>
        /// <returns></returns>
        public IQueryable<QbicleDiscussion> GetDiscussions(int cubeId, Enums.DiscussionStatus filterType, Enums.OrderByDate orderDate,
            int size, Enums.TypeGetData typeGet, string curentUserId)
        {
            IQueryable<QbicleDiscussion> discussions = null;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions", curentUserId, null, cubeId, filterType, orderDate, typeGet, curentUserId);

                switch (filterType)
                {
                    case Enums.DiscussionStatus.AllDiscussions:
                        switch (typeGet)
                        {
                            case Enums.TypeGetData.LoadNext:
                                discussions = this.GetDicussionsOrderBySkipTakeQbicleId(cubeId, orderDate, size, HelperClass.qbiclePageSize);
                                break;
                            case Enums.TypeGetData.Filter:
                                discussions = this.GetDicussionsOrderByTakeQbicleId(cubeId, orderDate, size);
                                break;
                        }
                        break;
                    case Enums.DiscussionStatus.MyDiscussions:
                        switch (typeGet)
                        {
                            case Enums.TypeGetData.LoadNext:
                                discussions = this.GetDicussionsOrderBySkipTakeQbicleUserId(cubeId, orderDate, size, HelperClass.qbiclePageSize, curentUserId);
                                break;
                            case Enums.TypeGetData.Filter:
                                discussions = this.GetDicussionsOrderByTakeQbicleUserId(cubeId, orderDate, size, curentUserId);
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, curentUserId, cubeId, filterType, orderDate, typeGet, curentUserId);
            }

            return discussions;
        }

        /// <summary>
        /// Get count of Discussions
        /// </summary>
        /// <param name="discussions">List<QbicleDiscussion></param>
        /// <returns></returns>
        public int GetDiscussionCountInTheDiscussions(List<QbicleDiscussion> discussions)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Count discussions", null, null, discussions);

                return discussions.Count();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussions);
            }
            return 0;
        }
        /// <summary>
        /// Get task whit discussions size from the discussions list
        /// </summary>
        /// <param name="discussions">List<QbicleTask></param>
        /// <returns></returns>
        public List<QbicleDiscussion> GetDiscussionTakeSizeInTheDiscussions(List<QbicleDiscussion> discussions)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussions", null, null, discussions);

                return discussions.Take(HelperClass.qbiclePageSize).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussions);
            }
            return new List<QbicleDiscussion>();
        }

        /// <summary>
        /// validate duplicate discussion name 
        /// But Qbicle another have same task name
        /// </summary>
        /// <param name="cubeId"></param>
        /// <param name="discussionId"></param>
        /// <param name="discussionName"></param>
        /// <returns></returns>
        public bool DuplicateDiscussionNameCheck(int cubeId, int discussionId, string discussionName)
        {
            bool exist;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate discussion name", null, null, cubeId, discussionId, discussionName);

                if (discussionId > 0)
                    exist = DbContext.Discussions.Any(x => (x.Id != discussionId && x.Name == discussionName && x.Qbicle.Id == cubeId));
                else
                    exist = DbContext.Discussions.Any(x => (x.Name == discussionName && x.Qbicle.Id == cubeId));

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, discussionId, discussionName);
                exist = false;
            }
            return exist;
        }

        public ReturnJsonModel SaveDiscussionForIdea(DiscussionIdeaCustomeModel model, int domainId, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for idea", userSetting.Id, null, model, domainId);

                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(domainId);
                if (setting == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_153");
                    return returnModel;
                }
                if (model.id > 0)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_308");
                    return returnModel;
                }
                var dbidea = DbContext.SMIdeaThemes.FirstOrDefault(e => e.Id == model.ideaId);
                if (dbidea == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_309");
                    return returnModel;
                }
                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_310");
                    return returnModel;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);
                QbicleDiscussion discussion = new QbicleDiscussion();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                if (!string.IsNullOrEmpty(model.expirydate) && model.isexpiry)
                {
                    var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.expirydate.ConvertDateFormat("dd/MM/yyyy HH:mm"), tz);
                    if (expiryDate < DateTime.UtcNow)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_311!");
                        return returnModel;
                    }
                    discussion.ExpiryDate = expiryDate;
                }
                discussion.StartedBy = user;
                discussion.StartedDate = DateTime.UtcNow;
                discussion.State = QbicleActivity.ActivityStateEnum.Open;
                discussion.Qbicle = qbicle;
                discussion.TimeLineDate = DateTime.UtcNow;
                discussion.Name = dbidea.Name;
                discussion.Summary = dbidea.Explanation;
                discussion.FeaturedImageUri = dbidea.FeaturedImageUri;
                discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.IdeaDiscussion;
                discussion.App = QbicleActivity.ActivityApp.SalesAndMarketing;
                discussion.ActivityMembers.AddRange(qbicle.Members);

                var topic = new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);

                discussion.Topic = topic;
                #region Add First Comment
                if (!string.IsNullOrEmpty(model.openingmessage))
                {
                    var post = new QbiclePost
                    {
                        CreatedBy = user,
                        Message = model.openingmessage,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic
                    };

                    DbContext.Posts.Add(post);
                    DbContext.Entry(post).State = EntityState.Added;
                    discussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                    discussion.Posts.Add(post);
                    discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                }
                #endregion
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                dbidea.Discussion = discussion;
                if (DbContext.Entry(dbidea).State == EntityState.Detached)
                    DbContext.SMIdeaThemes.Attach(dbidea);
                DbContext.Entry(dbidea).State = EntityState.Modified;

                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = new { discussion.Id };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userSetting.Id, model, domainId);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }

        public ReturnJsonModel SaveDiscussionForPlace(DiscussionPlaceModel model, int domainId, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for place", userSetting.Id, null, model, domainId);

                var setting = new SocialWorkgroupRules(DbContext).getSettingByDomainId(domainId);
                if (setting == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_153");
                    return returnModel;
                }
                if (model.id > 0)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_308");
                    return returnModel;
                }
                var place = DbContext.SMPlaces.FirstOrDefault(e => e.Id == model.placeId);
                if (place == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_312");
                    return returnModel;
                }
                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_310");
                    return returnModel;
                }

                QbicleDiscussion discussion = new QbicleDiscussion();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                if (!string.IsNullOrEmpty(model.expirydate) && model.isexpiry)
                {
                    var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.expirydate.ConvertDateFormat("dd/MM/yyyy HH:mm"), tz);
                    if (expiryDate < DateTime.UtcNow)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_311!");
                        return returnModel;
                    }
                    discussion.ExpiryDate = expiryDate;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);
                discussion.StartedBy = user;
                discussion.StartedDate = DateTime.UtcNow;
                discussion.State = QbicleActivity.ActivityStateEnum.Open;
                discussion.Qbicle = qbicle;
                discussion.TimeLineDate = DateTime.UtcNow;
                discussion.Name = place.Name;
                discussion.Summary = place.Summary;
                discussion.FeaturedImageUri = place.FeaturedImageUri;
                discussion.App = QbicleActivity.ActivityApp.SalesAndMarketing;
                discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.PlaceDiscussion;
                discussion.ActivityMembers.AddRange(qbicle.Members);

                var topic = new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);

                discussion.Topic = topic;
                #region Add First Comment
                if (!string.IsNullOrEmpty(model.openingmessage))
                {
                    var post = new QbiclePost
                    {
                        CreatedBy = user,
                        Message = model.openingmessage,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic
                    };

                    DbContext.Posts.Add(post);
                    DbContext.Entry(post).State = EntityState.Added;
                    discussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                    discussion.Posts.Add(post);
                    discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                }
                #endregion
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                place.Discussion = discussion;
                if (DbContext.Entry(place).State == EntityState.Detached)
                    DbContext.SMPlaces.Attach(place);
                DbContext.Entry(place).State = EntityState.Modified;

                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = new { discussion.Id };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userSetting.Id, model, domainId);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public ReturnJsonModel SaveDiscussionForQbicle(DiscussionQbicleModel model, UserSetting userSetting, bool isCreatorTheCustomer, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for qbicle", userSetting.Id, null, model);
                if (!string.IsNullOrEmpty(model.UploadKey))
                    new Azure.AzureStorageRules(DbContext).ProcessingMediaS3(model.UploadKey);

                string dateFormat = string.IsNullOrEmpty(userSetting.DateFormat) ? "dd/MM/yyyy" : userSetting.DateFormat;
                dateFormat += (" " + (string.IsNullOrEmpty(userSetting.TimeFormat) ? "HH:mm" : userSetting.TimeFormat));
                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == model.QbicleId);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }

                if (qbicle is B2CQbicle)
                {
                    (qbicle as B2CQbicle).CustomerViewed = false;
                    (qbicle as B2CQbicle).Customer.RemovedQbicle.Remove(qbicle);
                    qbicle.RemovedForUsers.Remove((qbicle as B2CQbicle).Customer);
                }
                else if (qbicle is C2CQbicle)
                {
                    var unseenUser = (qbicle as C2CQbicle).Customers.FirstOrDefault(p => p.Id != userSetting.Id);
                    if ((qbicle as C2CQbicle).NotViewedBy == null)
                        (qbicle as C2CQbicle).NotViewedBy = new List<ApplicationUser>();
                    (qbicle as C2CQbicle).NotViewedBy.Add(unseenUser);
                    if (unseenUser.NotViewedQbicle == null)
                        unseenUser.NotViewedQbicle = new List<C2CQbicle>();
                    unseenUser.NotViewedQbicle.Add(qbicle as C2CQbicle);

                    qbicle.RemovedForUsers.Remove(unseenUser);
                    unseenUser.RemovedQbicle.Remove(qbicle);
                }

                var currentUser = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);

                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                QbicleDiscussion discussion = null;
                if (model.Id != 0)
                {
                    discussion = DbContext.Discussions.FirstOrDefault(e => e.Id == model.Id);
                    discussion.IsCreatorTheCustomer = isCreatorTheCustomer;
                    if (!model.IsExpiry)
                        discussion.ExpiryDate = null;
                    else
                    {
                        if (!string.IsNullOrEmpty(model.ExpiryDate))
                        {
                            var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.ExpiryDate.ConvertDateFormat(dateFormat), tz);
                            if (expiryDate < DateTime.UtcNow)
                            {
                                returnModel.msg = ResourcesManager._L("ERROR_MSG_311");
                                return returnModel;
                            }
                            discussion.ExpiryDate = expiryDate;
                        }
                        else if (model.ExpireDate != null)
                        {
                            if (model.ExpireDate.ConvertTimeToUtc(userSetting.Timezone) < DateTime.UtcNow)
                            {
                                returnModel.msg = ResourcesManager._L("ERROR_MSG_311");
                                return returnModel;
                            }
                            discussion.ExpiryDate = model.ExpireDate.ConvertTimeToUtc(userSetting.Timezone);
                        }
                    }

                    discussion.Name = model.Title;
                    discussion.Summary = model.Summary;
                    discussion.TimeLineDate = DateTime.UtcNow;
                    discussion.App = QbicleActivity.ActivityApp.Qbicles;
                    if (model.Media.UrlGuid != null)
                    {
                        discussion.FeaturedImageUri = model.Media.UrlGuid;
                    }
                    discussion.ActivityMembers.Clear();
                    discussion.ActivityMembers.Add(currentUser);
                    if (model.Assignee != null)
                    {
                        foreach (var item in model.Assignee)
                        {
                            if (item != userSetting.Id)
                            {
                                var user = new UserRules(DbContext).GetUser(item, 0);
                                if (user != null)
                                    discussion.ActivityMembers.Add(user);
                            }
                        }
                    }

                    var topic = new TopicRules(DbContext).GetTopicById(model.Topic);
                    if (topic != null)
                    {
                        discussion.Topic = topic;
                    }
                    if (DbContext.Entry(discussion).State == EntityState.Detached)
                        DbContext.Discussions.Attach(discussion);
                    DbContext.Entry(discussion).State = EntityState.Modified;
                }
                else
                {
                    discussion = new QbicleDiscussion();
                    discussion.IsCreatorTheCustomer = isCreatorTheCustomer;
                    if (model.ExpireDate != null && model.IsExpiry)
                    {
                        discussion.ExpiryDate = model.ExpireDate?.ConvertTimeToUtc(tz);
                    }
                    else if (!string.IsNullOrEmpty(model.ExpiryDate) && model.IsExpiry)
                    {
                        var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.ExpiryDate.ConvertDateFormat(dateFormat), tz);
                        if (expiryDate < DateTime.UtcNow)
                        {
                            returnModel.msg = ResourcesManager._L("ERROR_MSG_311");
                            return returnModel;
                        }
                        discussion.ExpiryDate = expiryDate;
                    }
                    else
                    {
                        discussion.ExpiryDate = null;
                    }
                    discussion.StartedBy = currentUser;
                    discussion.StartedDate = DateTime.UtcNow;
                    discussion.State = QbicleActivity.ActivityStateEnum.Open;
                    discussion.Qbicle = qbicle;
                    discussion.TimeLineDate = DateTime.UtcNow;
                    discussion.Name = model.Title;
                    discussion.Summary = model.Summary;


                    var topic = new TopicRules(DbContext).GetTopicById(model.Topic);
                    if (topic == null)
                        topic = new TopicRules(DbContext).SaveTopic(model.QbicleId, HelperClass.GeneralName);

                    discussion.Topic = topic;

                    #region Add First Comment
                    if (!string.IsNullOrEmpty(model.Summary))
                    {
                        var post = new QbiclePost
                        {
                            CreatedBy = currentUser,
                            Message = model.Summary,
                            StartedDate = DateTime.UtcNow,
                            TimeLineDate = DateTime.UtcNow,
                            IsCreatorTheCustomer = isCreatorTheCustomer,
                            Topic = topic,
                        };

                        DbContext.Posts.Add(post);
                        DbContext.Entry(post).State = EntityState.Added;
                        discussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                        discussion.Posts.Add(post);
                        discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                    }
                    #endregion
                    discussion.FeaturedImageUri = model.Media.UrlGuid;
                    discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.QbicleDiscussion;
                    discussion.ActivityMembers.Clear();
                    discussion.ActivityMembers.Add(currentUser);
                    if (model.Assignee != null)
                    {
                        foreach (var item in model.Assignee)
                        {
                            if (item != userSetting.Id)
                            {
                                var user = new UserRules(DbContext).GetUser(item, 0);
                                if (user != null)
                                    discussion.ActivityMembers.Add(user);
                            }
                        }
                    }


                    DbContext.Discussions.Add(discussion);
                    DbContext.Entry(discussion).State = EntityState.Added;
                }
                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = model.Id == 0 ? NotificationEventEnum.DiscussionCreation : NotificationEventEnum.DiscussionUpdate,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = new { discussion.Id };

                    returnModel.actionVal = discussion.Id;
                    if (!string.IsNullOrEmpty(originatingConnectionId))
                    {
                        var notification = new Notification
                        {
                            AssociatedQbicle = discussion.Qbicle,
                            CreatedBy = discussion.StartedBy,
                            IsCreatorTheCustomer = discussion.IsCreatorTheCustomer,
                        };
                        if (appType == AppType.Web)
                            returnModel.msg = new ISignalRNotification().HtmlRender(discussion, currentUser, ApplicationPageName.Activities, NotificationEventEnum.DiscussionCreation, notification);
                        else
                            returnModel.Object = MicroStreamRules.GenerateActivity(discussion, discussion.StartedDate, null, currentUser.Id, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.DiscussionCreation, notification);
                    }
                    DbContext.Entry(discussion).State = EntityState.Unchanged;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userSetting.Id, model);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public ReturnJsonModel SaveDiscussionForGoal(DiscussionGoalModel model, int domainId, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for goal", userSetting.Id, null, model, domainId);

                var setting = new OperatorConfigRules(DbContext).getSettingByDomainId(domainId);
                if (setting == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_153");
                    return returnModel;
                }
                if (model.id > 0)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_308");
                    return returnModel;
                }
                var goal = DbContext.OperatorGoals.FirstOrDefault(e => e.Id == model.goalId);
                if (goal == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_813");
                    return returnModel;
                }
                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_310");
                    return returnModel;
                }

                QbicleDiscussion discussion = new QbicleDiscussion();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                if (!string.IsNullOrEmpty(model.expirydate) && model.isexpiry)
                {
                    var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.expirydate.ConvertDateFormat("dd/MM/yyyy HH:mm"), tz);
                    if (expiryDate < DateTime.UtcNow)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_311");
                        return returnModel;
                    }
                    discussion.ExpiryDate = expiryDate;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);
                discussion.StartedBy = user;
                discussion.StartedDate = DateTime.UtcNow;
                discussion.State = QbicleActivity.ActivityStateEnum.Open;
                discussion.Qbicle = qbicle;
                discussion.TimeLineDate = DateTime.UtcNow;
                discussion.Name = goal.Name;
                discussion.Summary = goal.Summary;
                //discussion.FeaturedImageUri = goal.FeaturedImageUri;
                discussion.App = QbicleActivity.ActivityApp.Operator;
                discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.GoalDiscussion;
                discussion.ActivityMembers.AddRange(qbicle.Members);

                var topic = new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);
                discussion.Topic = topic;

                #region Add First Comment
                if (!string.IsNullOrEmpty(model.openingmessage))
                {
                    var post = new QbiclePost
                    {
                        CreatedBy = user,
                        Message = model.openingmessage,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,
                    };

                    DbContext.Posts.Add(post);
                    DbContext.Entry(post).State = EntityState.Added;
                    discussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                    discussion.Posts.Add(post);
                    discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                }
                #endregion
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                goal.Discussion = discussion;
                if (DbContext.Entry(goal).State == EntityState.Detached)
                    DbContext.OperatorGoals.Attach(goal);
                DbContext.Entry(goal).State = EntityState.Modified;

                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = new { discussion.Id };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userSetting.Id, model, domainId);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }

        public ReturnJsonModel SaveDiscussionForPerformance(DiscussionPerfomanceModel model, int domainId, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for performance", userSetting.Id, null, model, domainId);

                var setting = new OperatorConfigRules(DbContext).getSettingByDomainId(domainId);

                if (setting == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_153");
                    return returnModel;
                }
                if (model.id > 0)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_308");
                    return returnModel;
                }
                var performance = DbContext.OperatorPerformanceTrackings.FirstOrDefault(e => e.Id == model.perfomanceId);
                if (performance == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_814");
                    return returnModel;
                }
                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_310");
                    return returnModel;
                }

                QbicleDiscussion discussion = new QbicleDiscussion();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                if (!string.IsNullOrEmpty(model.expirydate) && model.isexpiry)
                {
                    var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.expirydate.ConvertDateFormat("dd/MM/yyyy HH:mm"), tz);
                    if (expiryDate < DateTime.UtcNow)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_311");
                        return returnModel;
                    }
                    discussion.ExpiryDate = expiryDate;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);
                discussion.StartedBy = user;
                discussion.StartedDate = DateTime.UtcNow;
                discussion.State = QbicleActivity.ActivityStateEnum.Open;
                discussion.Qbicle = qbicle;
                discussion.TimeLineDate = DateTime.UtcNow;
                discussion.Name = HelperClass.GetFullNameOfUser(performance.Team.User, user.Id);
                discussion.Summary = performance.Description;
                discussion.FeaturedImageUri = performance.Team.User.ProfilePic;
                discussion.App = QbicleActivity.ActivityApp.Operator;
                discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.PerformanceDiscussion;
                discussion.ActivityMembers.AddRange(qbicle.Members);

                var topic = new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);
                discussion.Topic = topic;

                #region Add First Comment
                if (!string.IsNullOrEmpty(model.openingmessage))
                {
                    var post = new QbiclePost
                    {
                        CreatedBy = user,
                        Message = model.openingmessage,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic
                    };

                    DbContext.Posts.Add(post);
                    DbContext.Entry(post).State = EntityState.Added;
                    discussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                    discussion.Posts.Add(post);
                    discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                }
                #endregion
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                performance.Discussion = discussion;
                if (DbContext.Entry(performance).State == EntityState.Detached)
                    DbContext.OperatorPerformanceTrackings.Attach(performance);
                DbContext.Entry(performance).State = EntityState.Modified;

                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = new { discussion.Id };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userSetting.Id, model, domainId);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }

        public ReturnJsonModel SaveDiscussionForComplianceTask(DiscussionComplianceTaskModel model, int domainId, UserSetting userSetting, string originatingConnectionId = "")
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                var setting = new OperatorConfigRules(DbContext).getSettingByDomainId(domainId);
                if (setting == null)
                {
                    returnModel.msg = "Please configure \"Source Qbicle\" and \"Default Topic\" settings!";
                    return returnModel;
                }
                if (model.id > 0)
                {
                    returnModel.msg = "This discussion already exists!";
                    return returnModel;
                }
                var complianceTask = DbContext.OperatorComplianceTasks.FirstOrDefault(e => e.Id == model.taskId);
                if (complianceTask == null)
                {
                    returnModel.msg = "This performance tracking was not found!";
                    return returnModel;
                }
                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == setting.SourceQbicle.Id);
                if (qbicle == null)
                {
                    returnModel.msg = "Error not finding the current Qbicle!";
                    return returnModel;
                }
                QbicleDiscussion discussion = new QbicleDiscussion();
                var tz = TimeZoneInfo.FindSystemTimeZoneById(userSetting.Timezone);
                if (!string.IsNullOrEmpty(model.expirydate) && model.isexpiry)
                {
                    var expiryDate = TimeZoneInfo.ConvertTimeToUtc(model.expirydate.ConvertDateFormat("dd/MM/yyyy HH:mm"), tz);
                    if (expiryDate < DateTime.UtcNow)
                    {
                        returnModel.msg = "The expiry date must be greater than the current time!";
                        return returnModel;
                    }
                    discussion.ExpiryDate = expiryDate;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userSetting.Id);
                discussion.StartedBy = user;
                discussion.StartedDate = DateTime.UtcNow;
                discussion.State = QbicleActivity.ActivityStateEnum.Open;
                discussion.Qbicle = qbicle;
                discussion.TimeLineDate = DateTime.UtcNow;
                discussion.Name = complianceTask.Name;
                discussion.Summary = complianceTask.Description;
                discussion.FeaturedImageUri = null;
                discussion.App = QbicleActivity.ActivityApp.Operator;
                discussion.DiscussionType = QbicleDiscussion.DiscussionTypeEnum.ComplianceTaskDiscussion;
                discussion.ActivityMembers.AddRange(qbicle.Members);

                var topic = new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);
                discussion.Topic = topic;

                #region Add First Comment
                if (!string.IsNullOrEmpty(model.openingmessage))
                {
                    var post = new QbiclePost
                    {
                        CreatedBy = user,
                        Message = model.openingmessage,
                        StartedDate = DateTime.UtcNow,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic
                    };

                    DbContext.Posts.Add(post);
                    DbContext.Entry(post).State = EntityState.Added;
                    discussion.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                    discussion.Posts.Add(post);
                    discussion.Qbicle.LastUpdated = DateTime.UtcNow;
                }
                #endregion
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                complianceTask.Discussion = discussion;
                if (DbContext.Entry(complianceTask).State == EntityState.Detached)
                    DbContext.OperatorComplianceTasks.Attach(complianceTask);
                DbContext.Entry(complianceTask).State = EntityState.Modified;

                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = new { discussion.Id };
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }

        /// <summary>
        ///  Create new promotecatalogue
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <param name="originatingConnectionId"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveDiscussionForProductMenu(B2CProductMenuDiscussionModel model, string userId,
            string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for B2C qbicle", userId, model);


                var qbicle = DbContext.Qbicles.FirstOrDefault(e => e.Id == model.QbicleId);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }
                var menu = DbContext.PosMenus.FirstOrDefault(e => e.Id == model.MenuId);
                if (menu == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "Catalog");
                    return returnModel;
                }
                var currentUser = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);
                var discussion = new B2CProductMenuDiscussion
                {
                    ProductMenu = menu,
                    StartedBy = currentUser,
                    StartedDate = DateTime.UtcNow,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    Qbicle = qbicle,
                    TimeLineDate = DateTime.UtcNow,
                    Name = menu.Name,
                    Summary = model.OpeningComment,
                    FeaturedImageUri = null
                };
                if (qbicle is B2CQbicle)
                {
                    (qbicle as B2CQbicle).CustomerViewed = false;
                    (qbicle as B2CQbicle).Customer.RemovedQbicle.Remove(qbicle);
                    qbicle.RemovedForUsers.Remove((qbicle as B2CQbicle).Customer);
                }
                discussion.ActivityMembers.AddRange(qbicle.Members);
                discussion.Topic = new TopicRules(DbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id);
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                var rs = DbContext.SaveChanges();
                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userId,
                        CreatedByName = discussion.StartedBy.GetFullName(),
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);


                    var notification = new Notification
                    {
                        AssociatedQbicle = discussion.Qbicle,
                        CreatedBy = discussion.StartedBy,
                        IsCreatorTheCustomer = discussion.IsCreatorTheCustomer,
                    };
                    var activity = DbContext.Activities.FirstOrDefault(e => e.Id == discussion.Id);
                    if (appType == AppType.Web)
                        returnModel.msg = new ISignalRNotification().HtmlRender(activity, currentUser, ApplicationPageName.Activities, NotificationEventEnum.DiscussionCreation, notification);
                    else
                        returnModel.Object = MicroStreamRules.GenerateActivity(activity, activity.StartedDate, null, userId, currentUser.DateFormat, currentUser.Timezone, false, NotificationEventEnum.DiscussionCreation, notification);

                    DbContext.Entry(activity).State = EntityState.Unchanged;


                    returnModel.result = true;

                    // returnModel.Object = new { discussion.Id };

                    returnModel.Object2 = discussion.Key;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }
        public ReturnJsonModel SaveB2BCatalogDiscussion(B2BCatalogDiscussionModel model, int currentDomainId, string userId, string originatingConnectionId = "")
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for B2B qbicle", userId, model);


                var qbicle = DbContext.B2BQbicles.FirstOrDefault(e => e.Id == model.QbicleId);
                if (qbicle == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                    return returnModel;
                }
                var catalog = DbContext.PosMenus.FirstOrDefault(e => e.Id == model.CatalogId);
                if (catalog == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "Catalog");
                    return returnModel;
                }
                var sharedByDomain = DbContext.Domains.FirstOrDefault(p => p.Id == currentDomainId);
                if (sharedByDomain == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "The domain which share the catalog");
                    return returnModel;
                }
                var sharedWithDomain = qbicle.Domains.FirstOrDefault(p => p.Id != sharedByDomain.Id);
                if (sharedWithDomain == null)
                {
                    returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "The domain which the catalog is shared with");
                    return returnModel;
                }
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);
                var discussion = new B2BCatalogDiscussion
                {
                    AssociatedCatalog = catalog,
                    StartedBy = user,
                    StartedDate = DateTime.UtcNow,
                    State = QbicleActivity.ActivityStateEnum.Open,
                    Qbicle = qbicle,
                    TimeLineDate = DateTime.UtcNow,
                    Name = catalog.Name,
                    Summary = model.CoveringNote,
                    FeaturedImageUri = null,
                    SharedByDomain = sharedByDomain,
                    SharedWithDomain = sharedWithDomain,
                    ActivityType = QbicleActivity.ActivityTypeEnum.DiscussionActivity
                };
                discussion.ActivityMembers.AddRange(qbicle.Members);
                discussion.Topic = new TopicRules(DbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id);
                DbContext.Discussions.Add(discussion);
                DbContext.Entry(discussion).State = EntityState.Added;
                var rs = DbContext.SaveChanges();

                if (rs > 0)
                {
                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionCreation,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userId,
                        CreatedByName = user.GetFullName(),
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);
                    returnModel.result = true;
                    returnModel.Object = discussion.Id;
                    //returnModel.Object2 = discussion;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                returnModel.msg = ex.Message;
            }
            return returnModel;
        }

        /// <summary>
        /// Create a new B2C Order discussion from B2C Manager , IsCreatorTheCustomer = False
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns>ReturnJsonModel object=new of discussionId</returns>
        public ReturnJsonModel SaveDiscussionForOrderCreation(B2OrderCreationDiscussionModel model, string userId, bool isCreatorTheCustomer, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            ReturnJsonModel returnModel = new ReturnJsonModel() { result = false };
            using (var dbTransaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for qbicle", userId, model);


                    var qbicle = DbContext.B2CQbicles.FirstOrDefault(e => e.Id == model.QbicleId);
                    if (qbicle == null)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_188");
                        return returnModel;
                    }
                    var menu = DbContext.PosMenus.FirstOrDefault(e => e.Id == model.MenuId);
                    if (menu == null)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "Product Menu");
                        return returnModel;
                    }
                    var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);

                    var order = new TradeOrder
                    {
                        SellingDomain = qbicle.Business,
                        SalesChannel = SalesChannelEnum.B2C,
                        OrderStatus = TradeOrderStatusEnum.Draft,
                        Customer = qbicle.Customer,
                        CreatedBy = user,
                        CreateDate = DateTime.UtcNow,
                        Location = menu.Location,
                        ProductMenu = menu,
                        OrderReference = DbContext.TraderReferences.FirstOrDefault(e => e.Id == model.OrderReferenceId),
                        //OrderJson = oJsonInitial,
                        //OrderJsonOrig = oJsonInitial
                    };
                    var discussion = new B2COrderCreation
                    {
                        TradeOrder = order,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Name = $"Order #{order.OrderReference.FullRef}",
                        Summary = model.OpeningComment,
                        FeaturedImageUri = null,
                        Interaction = OrderDiscussionInteraction.Interaction,
                        IsCreatorTheCustomer = isCreatorTheCustomer
                    };
                    qbicle.CustomerViewed = false;
                    qbicle.Customer.RemovedQbicle.Remove(qbicle);
                    qbicle.RemovedForUsers.Remove(qbicle.Customer);

                    discussion.ActivityMembers.AddRange(qbicle.Members);
                    discussion.Topic = new TopicRules(DbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id);
                    DbContext.Discussions.Add(discussion);
                    DbContext.Entry(discussion).State = EntityState.Added;
                    var rs = DbContext.SaveChanges();
                    dbTransaction.Commit();
                    if (rs > 0)
                    {
                        var oJsonInitial = JsonHelper.ToJson(new Order
                        {
                            TradeOrderId = order.Id,
                            TraderId = discussion.Id,
                            SalesChannel = SalesChannelEnum.B2C,
                            Items = new List<Item>()
                        });
                        order.OrderJson = oJsonInitial;
                        order.OrderJsonOrig = oJsonInitial;

                        DbContext.SaveChanges();
                        var nRule = new NotificationRules(DbContext);

                        var activityNotification = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            Id = discussion.Id,
                            EventNotify = NotificationEventEnum.DiscussionCreation,
                            AppendToPageName = ApplicationPageName.Activities,
                            AppendToPageId = 0,
                            CreatedById = userId,
                            CreatedByName = HelperClass.GetFullNameOfUser(discussion.StartedBy),
                            ReminderMinutes = 0
                        };
                        nRule.Notification2Activity(activityNotification);


                        returnModel.result = true;
                        returnModel.Object2 = new { discussion.Id };

                        if (string.IsNullOrEmpty(originatingConnectionId))
                            return returnModel;


                        var notification = new Notification
                        {
                            AssociatedQbicle = discussion.Qbicle,
                            CreatedBy = discussion.StartedBy,
                            IsCreatorTheCustomer = discussion.IsCreatorTheCustomer,
                        };

                        var activity = DbContext.Activities.FirstOrDefault(e => e.Id == discussion.Id);
                        if (appType == AppType.Web)
                            returnModel.msg = new ISignalRNotification().HtmlRender(activity, user, ApplicationPageName.Activities, NotificationEventEnum.DiscussionCreation, notification);
                        else
                            returnModel.Object = MicroStreamRules.GenerateActivity(activity, activity.StartedDate, null, user.Id, user.DateFormat, user.Timezone, false, NotificationEventEnum.DiscussionCreation, notification);

                        DbContext.Entry(activity).State = EntityState.Unchanged;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, model);
                    returnModel.msg = ex.Message;
                    dbTransaction.Rollback();
                }
            }

            return returnModel;
        }
        /// <summary>
        /// Create a new B2C Order when selected a catalog Id
        /// </summary>
        /// <param name="businessDomainId"></param>
        /// <param name="catalogId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveB2CDiscussionForStore(int businessDomainId, int catalogId, string userId, bool isCreatorTheCustomer, string originatingConnectionId = "")
        {
            var returnModel = new ReturnJsonModel() { result = false };
            using (var dbTransaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for qbicle", userId, businessDomainId, catalogId);

                    var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == userId);
                    var b2bProfile = DbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == businessDomainId);

                    var menu = DbContext.PosMenus.FirstOrDefault(e => e.Id == catalogId);
                    if (menu == null)
                    {
                        dbTransaction.Rollback();
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "Product Menu");
                        return returnModel;
                    }

                    // Check the ability to create a b2c order with the given catalog
                    var isB2COrderAllowed = false;
                    if (b2bProfile != null
                        && b2bProfile.IsDisplayedInB2CListings
                        && b2bProfile.DefaultB2CRelationshipManagers.Any()
                        && b2bProfile.Domain.Status != QbicleDomain.DomainStatusEnum.Closed
                        && menu.IsPublished
                        && !menu.IsDeleted
                        )
                    {
                        isB2COrderAllowed = true;
                    }
                    if (!isB2COrderAllowed)
                    {
                        dbTransaction.Rollback();
                        returnModel.msg = "B2C Orders with this catalog are not allowed to created. Please contact the administrator for more information";
                        return returnModel;
                    }

                    var qbicle = DbContext.B2CQbicles.FirstOrDefault(s => s.Business.Id == businessDomainId && s.Customer.Id == userId && !s.IsHidden);
                    if (qbicle == null)
                    {
                        // Need to add a Trader Contact here if possible
                        new OrderProcessingHelper(DbContext).GetCreateTraderContactFromUserInfo(user, b2bProfile.Domain, SalesChannelEnum.B2C);
                        qbicle = new B2CQbicle
                        {
                            Status = CommsStatus.Approved,
                            Domain = DbContext.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2CUSTOMER && s.Type == SystemDomainType.B2C),
                            Business = b2bProfile.Domain,
                            Customer = user,
                            Name = $"{b2bProfile.BusinessName} & {HelperClass.GetFullNameOfUser(user)}",
                            Description = $"{SystemDomainConst.BUSINESS2CUSTOMER} - {b2bProfile.BusinessName} & {HelperClass.GetFullNameOfUser(user)}",
                            LogoUri = HelperClass.QbicleLogoDefault,
                            IsHidden = false,
                            OwnedBy = user,
                            StartedBy = user,
                            Manager = user,
                            StartedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow
                        };

                        if (b2bProfile.DefaultB2CRelationshipManagers.Any())
                            qbicle.Members.AddRange(b2bProfile.DefaultB2CRelationshipManagers);
                        if (!qbicle.Members.Any(s => s.Id == userId))
                            qbicle.Members.Add(user);

                        DbContext.B2CQbicles.Add(qbicle);
                        DbContext.Entry(qbicle).State = EntityState.Added;
                        returnModel.result = DbContext.SaveChanges() > 0 ? true : false;
                        returnModel.Object = qbicle.Id;
                        var relationshipLog = new CustomerRelationshipLog
                        {
                            QbicleId = qbicle.Id,
                            Status = CommsStatus.Pending,
                            UserId = userId,
                            CreatedDate = DateTime.UtcNow
                        };
                        DbContext.CustomerRelationshipLogs.Add(relationshipLog);
                        DbContext.Entry(relationshipLog).State = EntityState.Added;
                        DbContext.SaveChanges();

                        new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);
                    }
                    
                    var order = new TradeOrder
                    {
                        SellingDomain = qbicle.Business,
                        SalesChannel = SalesChannelEnum.B2C,
                        OrderStatus = TradeOrderStatusEnum.Draft,
                        Customer = qbicle.Customer,
                        CreatedBy = user,
                        CreateDate = DateTime.UtcNow,
                        Location = menu.Location,
                        ProductMenu = menu,
                        OrderReference = new TraderReferenceRules(DbContext).GetNewReference(businessDomainId, TraderReferenceType.Order)
                    };
                    if (order.OrderReference == null)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_CANTORDERREF");
                        dbTransaction.Rollback();
                        return returnModel;
                    }
                    var discussion = new B2COrderCreation
                    {
                        TradeOrder = order,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Name = $"Order #{order.OrderReference.FullRef}",
                        Summary = $"Shop with {b2bProfile.BusinessName}",
                        FeaturedImageUri = null,
                        IsCreatorTheCustomer = isCreatorTheCustomer
                    };
                    discussion.ActivityMembers.AddRange(qbicle.Members);
                    discussion.Topic = new TopicRules(DbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id);
                    if (discussion.Topic == null)
                    {
                        discussion.Topic = new TopicRules(DbContext).SaveTopic(qbicle.Id, HelperClass.GeneralName);
                    }
                    DbContext.Discussions.Add(discussion);
                    DbContext.Entry(discussion).State = EntityState.Added;
                    var rs = DbContext.SaveChanges();
                    dbTransaction.Commit();

                    var orderJson = JsonHelper.ToJson(new Order
                    {
                        SalesChannel = SalesChannelEnum.B2C,
                        TradeOrderId = order.Id,
                        TraderId = discussion.Id,
                        Items = new List<Item>()
                    });
                    order.OrderJson = orderJson;
                    order.OrderJsonOrig = orderJson;
                    DbContext.SaveChanges();

                    if (rs > 0)
                    {
                        var nRule = new NotificationRules(DbContext);

                        var activityNotification = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            Id = discussion.Id,
                            EventNotify = NotificationEventEnum.DiscussionCreation,
                            AppendToPageName = ApplicationPageName.Activities,
                            AppendToPageId = 0,
                            CreatedById = user.Id,
                            CreatedByName = user.GetFullName(),
                            ReminderMinutes = 0
                        };
                        nRule.Notification2Activity(activityNotification);
                        returnModel.result = true;
                        returnModel.Object = discussion.Id;
                        returnModel.msgId = discussion.Key;
                        returnModel.Object2 = discussion;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, userId, businessDomainId, catalogId);
                    returnModel.msg = ex.Message;
                    dbTransaction.Rollback();
                }
            }

            return returnModel;
        }
        /// <summary>
        /// Create new B2B discussion
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ReturnJsonModel SaveB2BDiscussionForOrderCreation(B2BOrderCreationDiscussionModel model, string originatingConnectionId = "")
        {
            var returnModel = new ReturnJsonModel() { result = false };
            using (var dbTransaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for qbicle", model.CurrentUserId, model);

                    var partnership = DbContext.B2BPurchaseSalesPartnerships.FirstOrDefault(e => e.Id == model.PartnershipId);
                    if (partnership == null)
                        return returnModel;
                    var menu = DbContext.PosMenus.FirstOrDefault(e => e.Id == model.CatalogId);
                    if (menu == null)
                    {
                        returnModel.msg = ResourcesManager._L("ERROR_MSG_815", "Catalogue");
                        return returnModel;
                    }
                    var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == model.CurrentUserId);
                    var order = new TradeOrderB2B
                    {
                        SellingDomain = partnership.ProviderDomain,
                        SalesChannel = SalesChannelEnum.B2B,
                        OrderStatus = TradeOrderStatusEnum.Draft,
                        BuyingDomain = partnership.ConsumerDomain,
                        TraderContact = (
                                          partnership.ParentRelationship.Domain1.Id == partnership.ConsumerDomain.Id ?     // If Domain1 is the consumer Domain => Domain1 is the Purchaser
                                          partnership.ParentRelationship.Domain2TraderContactForDomain1 :                  // the contact is the TraderContact in Domain2 for Domain1 
                                          partnership.ParentRelationship.Domain1TraderContactForDomain2                    // the contact is the TraderContact in Domain1 for Domain2 
                                          ),
                        VendorTraderContact = (
                                          partnership.ParentRelationship.Domain1.Id == partnership.ProviderDomain.Id ?     // If Domain1 is the Provider Domain => Domain1 is the Vendor
                                          partnership.ParentRelationship.Domain2TraderContactForDomain1 :                  // the contact is the TraderContact in Domain2 for Domain1 
                                          partnership.ParentRelationship.Domain1TraderContactForDomain2),
                        CreatedBy = user,
                        CreateDate = DateTime.UtcNow,
                        Location = menu.Location,
                        ProductMenu = menu,
                        OrderReference = DbContext.TraderReferences.FirstOrDefault(e => e.Id == model.OrderReferenceId),
                        OrderJson = JsonHelper.ToJson(new Order { SalesChannel = SalesChannelEnum.B2B, Items = new List<Item>() })
                    };
                    var discussion = new B2BOrderCreation
                    {
                        TradeOrder = order,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Qbicle = partnership.CommunicationQbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Name = $"Order #{order.OrderReference.FullRef}",
                        Summary = model.OrderNote,
                        FeaturedImageUri = null
                    };
                    discussion.ActivityMembers.AddRange(partnership.CommunicationQbicle.Members);
                    discussion.Topic = new TopicRules(DbContext).GetTopicByName(HelperClass.GeneralName, partnership.CommunicationQbicle.Id);
                    DbContext.Discussions.Add(discussion);
                    DbContext.Entry(discussion).State = EntityState.Added;
                    var rs = DbContext.SaveChanges();
                    //Create a Currency conversion
                    new ExchangeRateRules(DbContext).CreateExchangeRateByOrder(order);
                    dbTransaction.Commit();
                    if (rs > 0)
                    {
                        var nRule = new NotificationRules(DbContext);

                        var activityNotification = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            Id = discussion.Id,
                            EventNotify = NotificationEventEnum.DiscussionCreation,
                            AppendToPageName = ApplicationPageName.Activities,
                            AppendToPageId = 0,
                            CreatedById = model.CurrentUserId,
                            CreatedByName = HelperClass.GetFullNameOfUser(discussion.StartedBy),
                            ReminderMinutes = 0
                        };
                        nRule.Notification2Activity(activityNotification);
                        returnModel.result = true;
                        returnModel.Object = new { discussion.Id };

                        if (string.IsNullOrEmpty(originatingConnectionId)) return returnModel;


                        var notification = new Notification
                        {
                            AssociatedQbicle = discussion.Qbicle,
                            CreatedBy = discussion.StartedBy,
                            IsCreatorTheCustomer = discussion.IsCreatorTheCustomer,
                        };

                        var activity = DbContext.Activities.FirstOrDefault(e => e.Id == discussion.Id);
                        returnModel.msg = new ISignalRNotification().HtmlRender(activity, user, ApplicationPageName.Activities, NotificationEventEnum.DiscussionCreation, notification);
                        DbContext.Entry(activity).State = EntityState.Unchanged;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, model.CurrentUserId, model);
                    returnModel.msg = ex.Message;
                    dbTransaction.Rollback();
                }
            }

            return returnModel;
        }

        /// <summary>
        /// Create B2COrderCreation, B2CQbicle for POS order
        /// </summary>
        /// <param name="businessDomainId"></param>
        /// <param name="catalogId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ReturnJsonModel CreateB2CDiscussionForPos(TradeOrder tradeOrder, string originatingConnectionId = "")
        {
            var returnModel = new ReturnJsonModel() { result = false };
            using (var dbTransaction = DbContext.Database.BeginTransaction())
            {
                try
                {
                    if (ConfigManager.LoggingDebugSet)
                        LogManager.Debug(MethodBase.GetCurrentMethod(), "Save discussion for qbicle", tradeOrder.CreatedBy.Id, tradeOrder);

                    var user = tradeOrder.Customer;

                    int businessDomainId = tradeOrder.SellingDomain.Id;

                    var b2bProfile = DbContext.B2BProfiles.FirstOrDefault(s => s.Domain.Id == businessDomainId);
                    var qbicle = DbContext.B2CQbicles.FirstOrDefault(s => s.Business.Id == businessDomainId && s.Customer.Id == user.Id && !s.IsHidden);
                    if (qbicle == null)
                    {
                        // Need to add a Trader Contact here if possible
                        new OrderProcessingHelper(DbContext).GetCreateTraderContactFromUserInfo(user, b2bProfile.Domain, SalesChannelEnum.POS);
                        qbicle = new B2CQbicle
                        {
                            Status = CommsStatus.Approved,
                            Domain = DbContext.SystemDomains.FirstOrDefault(s => s.Name == SystemDomainConst.BUSINESS2CUSTOMER && s.Type == SystemDomainType.B2C),
                            Business = b2bProfile.Domain,
                            Customer = user,
                            Name = $"{b2bProfile.BusinessName} & {user.GetFullName()}",
                            Description = $"{SystemDomainConst.BUSINESS2CUSTOMER} - {b2bProfile.BusinessName} & {user.GetFullName()}",
                            LogoUri = HelperClass.QbicleLogoDefault,
                            IsHidden = false,
                            OwnedBy = user,
                            StartedBy = user,
                            Manager = user,
                            StartedDate = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow
                        };

                        if (b2bProfile.DefaultB2CRelationshipManagers.Any())
                            qbicle.Members.AddRange(b2bProfile.DefaultB2CRelationshipManagers);
                        if (!qbicle.Members.Any(s => s.Id == user.Id))
                            qbicle.Members.Add(user);

                        DbContext.B2CQbicles.Add(qbicle);
                        DbContext.Entry(qbicle).State = EntityState.Added;
                        returnModel.result = DbContext.SaveChanges() > 0;
                        returnModel.Object = qbicle.Id;
                        var relationshipLog = new CustomerRelationshipLog
                        {
                            QbicleId = qbicle.Id,
                            Status = CommsStatus.Pending,
                            UserId = user.Id,
                            CreatedDate = DateTime.UtcNow
                        };
                        DbContext.CustomerRelationshipLogs.Add(relationshipLog);
                        DbContext.Entry(relationshipLog).State = EntityState.Added;
                        DbContext.SaveChanges();

                        new TopicRules(DbContext).GetCreateTopicByName(HelperClass.GeneralName, qbicle.Id);
                    }

                    var discussion = new B2COrderCreation
                    {
                        TradeOrder = tradeOrder,
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        State = QbicleActivity.ActivityStateEnum.Open,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Name = $"Order #{tradeOrder.OrderReference.FullRef}",
                        Summary = $"Shop with {b2bProfile.BusinessName}",
                        FeaturedImageUri = null,
                    };
                    discussion.ActivityMembers.AddRange(qbicle.Members);
                    discussion.Topic = new TopicRules(DbContext).GetTopicByName(HelperClass.GeneralName, qbicle.Id);
                    DbContext.Discussions.Add(discussion);
                    DbContext.Entry(discussion).State = EntityState.Added;
                    var rs = DbContext.SaveChanges();
                    dbTransaction.Commit();

                    DbContext.SaveChanges();

                    if (rs > 0)
                    {
                        var nRule = new NotificationRules(DbContext);

                        var activityNotification = new ActivityNotification
                        {
                            OriginatingConnectionId = originatingConnectionId,
                            Id = discussion.Id,
                            EventNotify = NotificationEventEnum.DiscussionCreation,
                            AppendToPageName = ApplicationPageName.Activities,
                            AppendToPageId = 0,
                            CreatedById = user.Id,
                            CreatedByName = user.GetFullName(),
                            ReminderMinutes = 0
                        };
                        nRule.Notification2Activity(activityNotification);
                        returnModel.result = true;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Error(MethodBase.GetCurrentMethod(), ex, tradeOrder.CreatedBy.Id, tradeOrder);
                    returnModel.msg = ex.Message;
                    dbTransaction.Rollback();
                }
            }

            return returnModel;
        }

        public ReturnJsonModel ActiveInteraction(int discussionId)
        {
            var returnModel = new ReturnJsonModel() { result = false };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Active Interaction", null, null, discussionId);
                var discussion = DbContext.B2COrderCreations.FirstOrDefault(e => e.Id == discussionId);
                if (discussion != null)
                {
                    discussion.Interaction = OrderDiscussionInteraction.Interaction;
                    returnModel.result = DbContext.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
            }
            return returnModel;
        }
        /// <summary>
        /// Get list dates Discussion for Qbicle view on the tab Discussions panel
        /// </summary>
        /// <param name="qbicleDiscussions"></param>
        /// <returns></returns>
        public IEnumerable<DateTime> GetDiscussionsDate(List<QbicleDiscussion> qbicleDiscussions)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussion for date", null, null, qbicleDiscussions);

                var discussionDates = from d in qbicleDiscussions select d.TimeLineDate.Date;
                var disDates = discussionDates.Distinct();
                return disDates;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleDiscussions);
            }
            return null;

        }

        public IEnumerable<DateTime> LoadMoreDiscussions(int cubeId, int size,
           ref List<QbicleDiscussion> discussions, ref int AcivitiesDateCount, string CurrentTimeZone)
        {
            IEnumerable<DateTime> activitiesDate = null;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more discussions", null, null, cubeId, size, discussions, AcivitiesDateCount, CurrentTimeZone);


                var qbicleDiscussions = new List<QbicleDiscussion>();

                qbicleDiscussions = this.GetDiscussionsByQbicleId(cubeId).BusinessMapping(CurrentTimeZone);

                var discussionDates = from d in qbicleDiscussions select d.TimeLineDate.Date;

                var disDates = discussionDates.Distinct();
                AcivitiesDateCount = disDates.Count();

                disDates = disDates.OrderByDescending(d => d.Date.Date);
                activitiesDate = disDates.OrderByDescending(d => d.Date).Skip(size).Take(HelperClass.qbiclePageSize);

                discussions = qbicleDiscussions.Where(o => activitiesDate.Contains(o.TimeLineDate.Date)).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, discussions, AcivitiesDateCount, CurrentTimeZone);
            }
            return activitiesDate;
        }
        /// <summary>
        /// get a discussion by discussion Id
        /// </summary>
        /// <param name="discussionId"></param>
        /// <returns></returns>
        public QbicleDiscussion GetDiscussionById(int discussionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more discussions", null, null, discussionId);

                return DbContext.Discussions.FirstOrDefault(e => e.Id == discussionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
                return new QbicleDiscussion();
            }
        }

        /// <summary>
        /// Get posts, sub activities of the a discusson
        /// </summary>
        /// <param name="discussion">QbicleDiscussion</param>
        /// <param name="subActivities">ref List<object></param>
        /// <returns>IEnumerable<DateTime></returns>
        public IEnumerable<DateTime> GetSubActivities(QbicleDiscussion discussion,
            ref List<dynamic> subActivities)
        {
            var dates = new List<DateTime>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get sub activities", null, null, discussion, subActivities);

                if (discussion == null)
                    return dates;
                foreach (var p in discussion.Posts)
                {
                    dates.Add(p.StartedDate.Date);
                }
                foreach (var s in discussion.SubActivities)
                {
                    dates.Add(s.TimeLineDate.Date);
                }
                dates = dates.OrderByDescending(d => d.Date).Distinct().ToList();

                var post = discussion.Posts.Where(d => dates.Contains(d.StartedDate.Date)).ToList();
                var activity = discussion.SubActivities.Where(d => dates.Contains(d.TimeLineDate.Date)).ToList();
                subActivities = new List<IEnumerable<dynamic>> { post, activity }
                  .SelectMany(x => x)
                  .OrderBy(x => x.TimeLineDate).ToList();

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, subActivities);
            }
            return dates;
        }

        public IEnumerable<DateTime> LoadMoreSubActivities(QbicleDiscussion discussion, int size,
         ref List<object> subActivities, ref int AcivitiesDateCount, string CurrentTimeZone)
        {
            var activitiesDate = new List<DateTime>();
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more sub activities", null, null, discussion, size, subActivities, AcivitiesDateCount, CurrentTimeZone);

                var disMap = discussion.BusinessMapping(CurrentTimeZone);
                var postMap = discussion.Posts.BusinessMapping(CurrentTimeZone);
                var subActivityMap = discussion.SubActivities.BusinessMapping(CurrentTimeZone);

                foreach (var p in postMap)
                {
                    activitiesDate.Add(p.StartedDate.Date);
                }
                foreach (var s in subActivityMap)
                {
                    activitiesDate.Add(s.TimeLineDate.Date);
                }
                activitiesDate = activitiesDate.OrderByDescending(d => d.Date).Distinct().Skip(size).Take(HelperClass.qbiclePageSize).ToList();

                var post = postMap.BusinessMapping(CurrentTimeZone).Where(d => activitiesDate.Contains(d.StartedDate.Date)).ToList();
                var activity = subActivityMap.Where(d => activitiesDate.Contains(d.TimeLineDate.Date)).ToList();
                subActivities = post.Cast<object>().Concat(activity).ToList();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussion, size, subActivities, AcivitiesDateCount, CurrentTimeZone);
            }
            return activitiesDate;
        }



        public List<QbicleDiscussion> GetDiscussionByUserId(string userId, int folderId, int skip)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get discussion by user id", null, null, userId, folderId, skip);

                var qbicleDiscussion = new List<QbicleDiscussion>();
                qbicleDiscussion = DbContext.Discussions.Where(c => c.ActivityMembers.Any(x => x.Id == userId)).ToList();
                if (folderId != 0)
                {
                    qbicleDiscussion = qbicleDiscussion.Where(x => x.Folders.Any(y => y.Id == folderId)).OrderByDescending(x => x.TimeLineDate).Skip(skip).Take(HelperClass.myDeskPageSize).ToList();
                }
                else
                {
                    qbicleDiscussion = qbicleDiscussion.OrderByDescending(x => x.TimeLineDate).Skip(skip).Take(HelperClass.myDeskPageSize).ToList();
                }
                return qbicleDiscussion;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId, folderId, skip);
                return new List<QbicleDiscussion>();
            }
        }

        /// <summary>
        /// Create a guest and invite to the Discussion
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="discussionId"></param>
        /// <param name="email"></param>
        /// <param name="messageRef"></param>
        /// <returns>ApplicationUser: if exist user name with email then return NULL. else Reurn Guest</returns>
        public ApplicationUser CreateNewGuestToDiscussion(int discussionId, string email, UserSetting userSetting, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create new guest to discussion", null, null, discussionId, email);

                var userRule = new UserRules(DbContext);
                //Checks to see if the email address exists in the application
                var guest = userRule.GetUserByEmail(email);
                // If the application finds that the email address does NOT exist in the system
                if (guest == null)
                {
                    //adds the user
                    guest = userRule.CreateUserInvitedByEmail(email);

                    var discussion = this.GetDiscussionById(discussionId);

                    DbContext.SaveChanges();

                    var nRule = new NotificationRules(DbContext);

                    var activityNotification = new ActivityNotification
                    {
                        OriginatingConnectionId = originatingConnectionId,
                        Id = discussion.Id,
                        EventNotify = NotificationEventEnum.DiscussionUpdate,
                        AppendToPageName = ApplicationPageName.Activities,
                        AppendToPageId = 0,
                        CreatedById = userSetting.Id,
                        CreatedByName = userSetting.DisplayName,
                        ReminderMinutes = 0
                    };
                    nRule.Notification2Activity(activityNotification);

                    return guest;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId, email);
                return null;
            }
        }

        /// <summary>
        /// Add new participant of the Domain to the discussion member
        /// </summary>
        /// <param name="usersDomainAssign"></param>
        /// <param name="domain"></param>
        /// <param name="discussionId"></param>
        /// <returns></returns>
        public bool CreateNewParticipant(string[] usersDomainAssign, QbicleDomain domain, int discussionId, UserSetting userSetting, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Create new participant", null, null, usersDomainAssign, domain, discussionId);

                var discussion = this.GetDiscussionById(discussionId);
                var users = from u in domain.Users
                            where usersDomainAssign.Contains(u.Id)
                            select u;
                discussion.ActivityMembers.AddRange(users);
                DbContext.SaveChanges();
                
                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = discussion.Id,
                    EventNotify = NotificationEventEnum.AddUserParticipants,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userSetting.Id,
                    CreatedByName = userSetting.DisplayName,
                    ReminderMinutes = 0
                };
                new NotificationRules(DbContext).Notification2Activity(activityNotification);

                return true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, usersDomainAssign, domain, discussionId);
                return false;
            }
        }
        /// <summary>
        /// remove participant user from discussion
        /// </summary>
        /// <param name="disId"></param>
        /// <param name="uId"></param>
        /// <param name="currentUId"></param>
        /// <returns></returns>
        public bool RemoveContactDiscussion(int disId, string uId, UserSetting currentUser, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Remove contact discussion", null, null, disId, uId);

                var discussion = DbContext.Discussions.FirstOrDefault(e => e.Id == disId);
                var user = discussion.ActivityMembers.FirstOrDefault(s => s.Id == uId);
                if (discussion == null || user == null || user.Id == currentUser.Id)
                    return false;

                discussion.ActivityMembers.Remove(user);


                DbContext.SaveChanges();


                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = discussion.Id,
                    EventNotify = NotificationEventEnum.RemoveUserParticipants,
                    AppendToPageName = ApplicationPageName.Discussion,
                    CreatedById = currentUser.Id,
                    CreatedByName = currentUser.DisplayName,
                    ReminderMinutes = 0,
                    HasActionToHandle = false,
                    ObjectById = uId,
                };

                new NotificationRules(DbContext).NotificationDiscussionParticipants(activityNotification);

                return true;

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, disId, uId);
                return false;
            }
        }
        /// <summary>
        /// add participant user to discussion
        /// </summary>
        /// <param name="disId"></param>
        /// <param name="uId"></param>
        /// <param name="currentUId"></param>
        /// <returns></returns>
        public bool AddContactDiscussion(int disId, string uId, UserSetting currentUser, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add contact discussion", null, null, disId, uId);

                var discussion = DbContext.Discussions.FirstOrDefault(e => e.Id == disId);
                var user = DbContext.QbicleUser.FirstOrDefault(e => e.Id == uId);
                if (discussion == null || user == null || user.Id == currentUser.Id)
                    return true;

                discussion.ActivityMembers.Add(user);
                DbContext.SaveChanges();

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = discussion.Id,
                    EventNotify = NotificationEventEnum.AddUserParticipants,
                    AppendToPageName = ApplicationPageName.Discussion,
                    CreatedById = currentUser.Id,
                    CreatedByName = currentUser.DisplayName,
                    ReminderMinutes = 0,
                    HasActionToHandle = false,
                    ObjectById = uId,
                };

                new NotificationRules(DbContext).NotificationDiscussionParticipants(activityNotification);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, disId, uId);
                return false;
            }
        }

        public List<ApplicationUser> SearchQbicleUsersInviteDiscussion(int disId, string keyword, int qId, QbicleDiscussion discussion)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Search qbicle users invite discussion", null, null, disId, keyword, qId, discussion);

                keyword = keyword.ToLower();
                var currentqbicle = new QbicleRules(DbContext).GetQbicleById(qId);

                var members = currentqbicle.Members != null ? currentqbicle.Members.Where(s => s.Surname.ToLower().Contains(keyword) || s.Forename.ToLower().Contains(keyword)).ToList() : new List<ApplicationUser>();
                if (members != null && members.Any())
                {
                    foreach (var item in discussion.ActivityMembers)
                    {
                        members.Remove(item);
                    }
                }
                return members;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, disId, keyword, qId, discussion);
                return new List<ApplicationUser>();
            }
        }
        /// <summary>
        /// add comment
        /// </summary>
        /// <param name="disKey"></param>
        /// <param name="post"></param>
        /// <param name="originatingCreationId">connection id, using for case a user log in to multi app</param>
        /// <param name="pageName"></param>
        /// <param name="hasAction"></param>
        /// <param name="sendToQueue"></param>
        /// <param name="originatingConnectionId"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public ReturnJsonModel AddComment(bool isCreatorTheCustomer, string disKey, QbiclePost post, string originatingCreationId, ApplicationPageName pageName = ApplicationPageName.Discussion,
            bool hasAction = false, bool sendToQueue = true, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Add posts to social post discussion", post.CreatedBy.Id, null, disKey, post);

            var activityId = string.IsNullOrEmpty(disKey) ? 0 : int.Parse(disKey.Decrypt());

            var activity = DbContext.Activities.FirstOrDefault(e => e.Id == activityId);
            if (activity == null)
                return new ReturnJsonModel { result = false };
            activity.IsCreatorTheCustomer = isCreatorTheCustomer;
            DbContext.SaveChanges();
            return AddCommentActivity(activity, post, originatingCreationId, pageName, hasAction, sendToQueue, originatingConnectionId, appType);

        }

        public async Task AddCommentAsync(bool isCreatorTheCustomer, string disKey, QbiclePost post, string originatingCreationId, ApplicationPageName pageName = ApplicationPageName.Discussion,
            bool hasAction = false, bool sendToQueue = true, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            await Task.Run(() =>
            {
                AddComment(isCreatorTheCustomer, disKey, post, originatingCreationId, pageName, hasAction, sendToQueue, originatingConnectionId, appType);
            });
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="post"></param>
        /// <param name="originatingCreationId"> connection id,(user created notification) using for case a user log in to multi app
        /// for check on signalR if current connection != originaling creation id then reload
        /// </param>
        /// <param name="pageName"></param>
        /// <param name="hasAction"></param>
        /// <param name="sendToQueue"></param>
        /// <param name="originatingConnectionId"></param>
        /// <param name="appType"></param>
        /// <returns></returns>
        public ReturnJsonModel AddCommentActivity(QbicleActivity activity, QbiclePost post, string originatingCreationId, ApplicationPageName pageName = ApplicationPageName.Discussion,
            bool hasAction = false, bool sendToQueue = true, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            if (ConfigManager.LoggingDebugSet)
                LogManager.Debug(MethodBase.GetCurrentMethod(), "Add posts to social post discussion", post.CreatedBy.Id, null, activity.Id, post);

            activity.TimeLineDate = DateTime.UtcNow;
            activity.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
            if (activity.AssociatedSet != null)
                post.Set = activity.AssociatedSet;

            activity.Posts.Add(post);
            activity.Qbicle.LastUpdated = DateTime.UtcNow;
            DbContext.Activities.Attach(activity);
            DbContext.Entry(activity).State = EntityState.Modified;
            DbContext.SaveChanges();

            var activityNotification = new ActivityNotification
            {
                OriginatingConnectionId = originatingConnectionId,
                Id = activity.Id,
                PostId = post.Id,
                EventNotify = NotificationEventEnum.DiscussionUpdate,
                AppendToPageName = pageName,
                AppendToPageId = activity.Id,
                CreatedById = post.CreatedBy.Id,
                CreatedByName = post.CreatedBy.GetFullName(),
                ReminderMinutes = 0,
                HasActionToHandle = hasAction,
                OriginatingCreationId = originatingCreationId,
            };
            new NotificationRules(DbContext).NotificationComment2Activity(activityNotification, sendToQueue: sendToQueue);

            var result = new ReturnJsonModel { result = true };


            if (!string.IsNullOrEmpty(originatingConnectionId))
            {
                var notification = new Notification
                {
                    AssociatedQbicle = activity.Qbicle,
                    CreatedBy = post.CreatedBy,
                    IsCreatorTheCustomer = post.IsCreatorTheCustomer,
                };
                if (appType == AppType.Web)
                    result.msg = new ISignalRNotification().HtmlRender(post, post.CreatedBy, pageName, NotificationEventEnum.DiscussionUpdate, notification);
                else
                    result.Object = MicroStreamRules.GenerateActivity(post, post.StartedDate, null, post.CreatedBy.Id, post.CreatedBy.DateFormat, post.CreatedBy.Timezone, false, NotificationEventEnum.DiscussionUpdate, notification);
            }
            DbContext.Entry(post).State = EntityState.Unchanged;

            return result;
        }


        public B2CProductMenuDiscussion GetDiscussionProductMenuById(int discussionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load B2C Discussion product menu", null, null, discussionId);

                return DbContext.B2CProductMenuDiscussions.FirstOrDefault(e => e.Id == discussionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
                return new B2CProductMenuDiscussion();
            }
        }
        /// <summary>
        /// Get B2COrderCreations (Discussion) by discutionId
        /// </summary>
        /// <param name="discussionId"></param>
        /// <returns></returns>
        public B2COrderCreation GetB2CDiscussionOrderByDiscussionId(int discussionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load B2C Discussion product menu", null, null, discussionId);
                return DbContext.B2COrderCreations.FirstOrDefault(e => e.Id == discussionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
                return new B2COrderCreation();
            }
        }
        /// <summary>
        /// Get B2COrderCreation (Discussion) by tradeOrderId
        /// </summary>
        /// <returns>B2COrderCreation</returns>
        public B2COrderCreation GetB2CDiscussionOrderByTradeorderId(int traderOrderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get B2COrderCreation by tradeOrderId", null, null, traderOrderId);
                var order = DbContext.B2COrderCreations.Include(s => s.Qbicle).FirstOrDefault(s => s.TradeOrder.Id == traderOrderId);
                return order;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderOrderId);
                return null;
            }
        }
        public B2BOrderCreation GetB2BDiscussionOrderByDiscussionId(int discussionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load B2B Discussion product menu", null, null, discussionId);
                return DbContext.B2BOrderCreations.FirstOrDefault(e => e.Id == discussionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
                return new B2BOrderCreation();
            }
        }
        public B2BOrderCreation GetB2BDiscussionOrderByTradeorderId(int traderOrderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get B2COrderCreation by tradeOrderId", null, null, traderOrderId);
                var order = DbContext.B2BOrderCreations.Include(s => s.Qbicle).FirstOrDefault(s => s.TradeOrder.Id == traderOrderId);
                return order;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, traderOrderId);
                return null;
            }
        }
        public B2BPartnershipDiscussion GetDiscussionByB2BRelationshipId(int relationshipId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get B2BDiscussion by relationshipId", null, null, relationshipId);
                return DbContext.B2BPartnershipDiscussions.FirstOrDefault(s => s.Relationship.Id == relationshipId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, relationshipId);
                return null;
            }
        }
        public B2BOrderCreation GetDiscussionByB2BOrderById(int discussionId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load B2C Discussion product menu", null, null, discussionId);
                return DbContext.B2BOrderCreations.FirstOrDefault(e => e.Id == discussionId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, discussionId);
                return new B2BOrderCreation();
            }
        }
        public async Task<ReturnJsonModel> GetB2COrerPaymentPaystackPopupData(int tradeOrderId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), null, null, null, tradeOrderId);

                var tradeOrder = DbContext.TradeOrders.FirstOrDefault(p => p.Id == tradeOrderId);
                if (tradeOrder == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find any associated Trade Order"
                    };
                }

                var domainObj = tradeOrder?.Location?.Domain;
                if (domainObj == null)
                {
                    return new ReturnJsonModel
                    {
                        result = false,
                        msg = "Cannot find any associated Domain"
                    };
                }

                var currentDomainPlan = DbContext.DomainPlans.FirstOrDefault(p => p.Domain.Id == domainObj.Id && p.IsArchived == false);
                if (currentDomainPlan == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find any domain plan associated with the domain of the Catalogue"
                    };
                }

                var currentSubscription = DbContext.DomainSubscriptions.FirstOrDefault(p => p.Plan.Id == currentDomainPlan.Id);
                if (currentSubscription == null)
                {
                    return new ReturnJsonModel()
                    {
                        result = false,
                        msg = "Cannot find any subscription associated with the domain of the Catalogue"
                    };
                }

                // Get remain amount needed to be paid
                var associatedPayments = tradeOrder.Payments.Where(p => p.Status == TraderPaymentStatusEnum.PaymentApproved).ToList();
                var orderAmount = tradeOrder.Invoice?.TotalInvoiceAmount ?? 0;
                var amount = orderAmount - associatedPayments.Sum(p => p.Amount);

                // Get SubAccount data from Paystack
                var paystackSubAccount = await new PayStackRules(DbContext).GetSubAccountInformation(domainObj.SubAccountCode);

                var percentageCharge = paystackSubAccount.percentage_charge;

                // Check flat fee
                var subAccSettings = DbContext.B2COrderPaymentCharges.FirstOrDefault();
                var charge_fee = (percentageCharge / 100) * amount;
                var isFlatFeeNeeded = charge_fee > subAccSettings.QbiclesFlatFee;
                decimal chargeAmount = 0;
                if (isFlatFeeNeeded)
                    chargeAmount = subAccSettings.QbiclesFlatFee;

                return new ReturnJsonModel()
                {
                    result = true,
                    msg = "",
                    Object = new
                    {
                        subAccCode = domainObj.SubAccountCode,
                        userEmail = tradeOrder.Customer?.Email ?? "",
                        publicKey = ConfigurationManager.AppSettings["PayStackPublicKey"],
                        chargeAmount = chargeAmount * 100,
                        amount = amount
                    }
                };
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, tradeOrderId);
                return new ReturnJsonModel()
                {
                    result = false,
                    msg = ResourcesManager._L("ERROR_MSG_5")
                };
            }
        }
    }
}
