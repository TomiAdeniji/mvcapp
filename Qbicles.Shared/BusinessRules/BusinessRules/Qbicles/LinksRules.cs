using Qbicles.BusinessRules.Helper;
using Qbicles.BusinessRules.Micro;
using Qbicles.BusinessRules.Model;
using Qbicles.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using static Qbicles.Models.Notification;

namespace Qbicles.BusinessRules
{
    public class LinksRules
    {
        private ApplicationDbContext dbContext;


        public LinksRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public QbicleLink GetLinkById(int linkId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save link", null, null, linkId);

                return dbContext.QbicleLinks.Find(linkId);
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, linkId);
                return null;
            }
        }
        public ReturnJsonModel SaveLink(QbicleLink link, int qbicleId, int topicId, MediaModel media, string userId, string originatingConnectionId = "", AppType appType = AppType.Micro)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save link", null, null, link, qbicleId, topicId, media);
                var result = new ReturnJsonModel { result = true };

                var user = dbContext.QbicleUser.Find(userId);

                var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                var topic = new TopicRules(dbContext).GetTopicById(topicId);

                QbicleMedia eventMedia = null;

                if (!string.IsNullOrEmpty(media.UrlGuid))
                {
                    new Azure.AzureStorageRules(dbContext).ProcessingMediaS3(media.UrlGuid);

                    eventMedia = new QbicleMedia
                    {
                        StartedBy = user,
                        StartedDate = DateTime.UtcNow,
                        Name = media.Name,
                        FileType = media.Type,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = topic,
                        MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, qbicle.Id),
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = user,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    eventMedia.VersionedFiles.Add(versionFile);
                    eventMedia.ActivityMembers.Add(user);
                }




                var eventNotify = NotificationEventEnum.LinkCreation;
                if (link.Id == 0)
                {
                    link.Name = link.Name.Trim('\'');
                    link.StartedBy = dbContext.QbicleUser.Find(userId);
                    link.StartedDate = DateTime.UtcNow;
                    link.State = QbicleActivity.ActivityStateEnum.Open;
                    link.ActivityType = QbicleActivity.ActivityTypeEnum.Link;
                    link.TimeLineDate = DateTime.UtcNow;

                    link.Qbicle = qbicle;
                    link.Topic = topic;
                    if (eventMedia != null)
                        link.FeaturedImage = eventMedia;

                    dbContext.QbicleLinks.Add(link);
                    dbContext.Entry(link).State = EntityState.Added;


                    dbContext.SaveChanges();
                    result.actionVal = link.Id;
                }
                else
                {
                    var dbLink = dbContext.QbicleLinks.Find(link.Id);

                    dbLink.Topic = topic;
                    dbLink.Name = link.Name;
                    dbLink.Description = link.Description;
                    dbLink.TimeLineDate = DateTime.UtcNow;
                    dbLink.URL = link.URL;

                    if (eventMedia != null)
                        dbLink.FeaturedImage = eventMedia;

                    if (dbContext.Entry(dbLink).State == EntityState.Detached)
                        dbContext.QbicleLinks.Attach(dbLink);
                    dbContext.Entry(dbLink).State = EntityState.Modified;
                    dbContext.SaveChanges();

                    result.actionVal = dbLink.Id;
                    eventNotify = NotificationEventEnum.LinkUpdate;
                }
                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = result.actionVal,
                    EventNotify = eventNotify,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = userId,
                    CreatedByName = user.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);

                result.actionVal = link.Id;

                if (!string.IsNullOrEmpty(originatingConnectionId))
                {
                    var notification = new Notification
                    {
                        AssociatedQbicle = link.Qbicle,
                        CreatedBy = link.StartedBy,
                        IsCreatorTheCustomer = link.IsCreatorTheCustomer,
                    };
                    if (appType == AppType.Web)
                        result.msg = new ISignalRNotification().HtmlRender(link, user, ApplicationPageName.Activities, NotificationEventEnum.LinkCreation, notification);
                    else
                        result.Object = MicroStreamRules.GenerateActivity(link, link.StartedDate, null, user.Id, user.DateFormat, user.Timezone, false, NotificationEventEnum.LinkCreation, notification);
                }

                dbContext.Entry(link).State = EntityState.Unchanged;
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, link, qbicleId, topicId, media);
                return new ReturnJsonModel { result = false, msg = ex.Message };
            }
        }
        /// <summary>
        ///     Get list events by qbicle id
        /// </summary>
        /// <param name="cubeId">int Qbicle Id</param>
        /// <returns></returns>
        public List<QbicleLink> GetLinksOrderByQbicleId(int cubeId, Enums.OrderByDate orderBy, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get links order by qbicle id", null, null, cubeId, orderBy, topicId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        if (topicId == 0)
                            return dbContext.QbicleLinks.Where(c => c.Qbicle.Id == cubeId && c.Topic != null)
                                .OrderByDescending(d => d.TimeLineDate).ToList();
                        return dbContext.QbicleLinks.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId)
                            .OrderByDescending(d => d.TimeLineDate).ToList();
                    case Enums.OrderByDate.OldestFirst:
                        if (topicId == 0)
                            return dbContext.QbicleLinks.Where(c => c.Qbicle.Id == cubeId && c.Topic != null)
                                .OrderBy(d => d.TimeLineDate).ToList();
                        return dbContext.QbicleLinks.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId)
                            .OrderBy(d => d.TimeLineDate).ToList();
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, topicId);
            }

            return new List<QbicleLink>();
        }
    }
}
