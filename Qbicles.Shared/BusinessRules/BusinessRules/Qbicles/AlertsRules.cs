using Qbicles.BusinessRules.Helper;
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
    public class AlertsRules
    {

        ApplicationDbContext dbContext;
        public AlertsRules()
        {

        }
        public AlertsRules(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ApplicationUser GetUser(String userId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get User", null, null, userId);

                var ur = new UserRules(dbContext);
                return ur.GetUser(userId, 0);
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, userId);
                return null;
            }
            
        }

        /// <summary>
        /// Get list alerts by qbicle id
        /// </summary>
        /// <param name="cubeId">int Qbicle Id</param>
        /// <param name="orderBy"></param>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public List<QbicleAlert> GetAlertsByQbicleId(int cubeId, Enums.OrderByDate orderBy, int topicId = 0)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get Alerts by qbicle id", null, null, cubeId, orderBy, topicId);

                switch (orderBy)
                {
                    case Enums.OrderByDate.NewestFirst:
                        if (topicId == 0)
                            return dbContext.Alerts.Where(c => c.Qbicle.Id == cubeId && c.Topic != null).OrderByDescending(d => d.TimeLineDate).ToList();
                        else
                            return dbContext.Alerts.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId).OrderByDescending(d => d.TimeLineDate).ToList();
                    case Enums.OrderByDate.OldestFirst:
                        if (topicId == 0)
                            return dbContext.Alerts.Where(c => c.Qbicle.Id == cubeId && c.Topic != null).OrderBy(d => d.TimeLineDate).ToList();
                        return dbContext.Alerts.Where(c => c.Qbicle.Id == cubeId && c.Topic.Id == topicId).OrderBy(d => d.TimeLineDate).ToList();
                }

            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, orderBy, topicId);
            }
            return new List<QbicleAlert>();

        }

        public bool DuplicateAlertNameCheck(int cubeId, int alertId, string alertName)
        {
            bool exists;
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Check duplicate alert name", null, null, cubeId, alertId, alertName);

                if (alertId > 0)
                {
                    exists = dbContext.Activities.Any(x => x.Id != alertId && x.Qbicle.Id == cubeId && x.Name == alertName);
                }
                else
                {
                    exists = dbContext.Activities.Any(x => x.Qbicle.Id == cubeId && x.Name == alertName);
                }
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, alertId, alertName);
                exists = false;
            }

            return exists;
        }

        public ReturnJsonModel SaveAlert(QbicleAlert alert, string[] linkAlertTo, int qbicleId,
           MediaModel media, string currentUserId,string topicName, string originatingConnectionId = "")
        {
            var result = new ReturnJsonModel()
            {
                result = false
            };
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Save alert", currentUserId, null, alert, linkAlertTo, qbicleId, media, topicName);

                var currentUser = dbContext.QbicleUser.Find(currentUserId);
                var qbicle = new QbicleRules(dbContext).GetQbicleById(qbicleId);
                var tRule = new TopicRules(dbContext);
                var topic = tRule.GetTopicByName(topicName, qbicleId);
                if (topic == null)
                {
                    topic = tRule.SaveTopic(qbicleId, topicName);
                }
                alert.Topic = topic;
                //var uRules = new UserRules(dbContext);
                alert.StartedBy = currentUser;
                alert.StartedDate = DateTime.UtcNow;
                alert.State = QbicleActivity.ActivityStateEnum.Open;
                qbicle.LastUpdated = DateTime.UtcNow;
                alert.Qbicle = qbicle;
                alert.ActivityType = QbicleActivity.ActivityTypeEnum.AlertActivity;
                alert.TimeLineDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(media.Name))
                {
                    //Media attach
                    var m = new QbicleMedia
                    {
                        StartedBy = currentUser,
                        StartedDate = DateTime.UtcNow,
                        FileType = media.Type,
                        Description = alert.Content,
                        Name = alert.Name,
                        Qbicle = qbicle,
                        TimeLineDate = DateTime.UtcNow,
                        Topic = alert.Topic,
                        MediaFolder = new MediaFolderRules(dbContext).GetMediaFolderByName(HelperClass.GeneralName, qbicleId),
                        IsVisibleInQbicleDashboard = false
                    };
                    var versionFile = new VersionedFile
                    {
                        IsDeleted = false,
                        FileSize = media.Size,
                        UploadedBy = currentUser,
                        UploadedDate = DateTime.UtcNow,
                        Uri = media.UrlGuid,
                        FileType = media.Type
                    };
                    m.VersionedFiles.Add(versionFile);
                    m.ActivityMembers.Add(currentUser);
                    alert.SubActivities.Add(m);
                }
                if (linkAlertTo != null)
                {
                    foreach (var userId in linkAlertTo)
                    {
                        var member = GetUser(userId);
                        alert.ActivityMembers.Add(member);
                    }
                    if (!linkAlertTo.Contains(currentUser.Id))
                        alert.ActivityMembers.Add(currentUser);//user create
                }

                var eventNotify = NotificationEventEnum.AlertCreation;
                if (alert.Id == 0)
                {
                    dbContext.Alerts.Add(alert);
                    dbContext.Entry(alert).State = EntityState.Added;
                }
                else
                {
                    if (dbContext.Entry(alert).State == EntityState.Detached)
                        dbContext.Alerts.Attach(alert);
                    dbContext.Entry(alert).State = EntityState.Modified;
                    eventNotify = NotificationEventEnum.AlertUpdate;
                }

                dbContext.SaveChanges();
                result.result = true;
                result.Object = new { topic = new { topic.Id, topic.Name } };
                var nRule = new NotificationRules(dbContext);
                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = alert.Id,
                    EventNotify = eventNotify,
                    AppendToPageName = ApplicationPageName.Activities,
                    AppendToPageId = 0,
                    CreatedById = currentUser.Id,
                    CreatedByName = currentUser.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.Notification2Activity(activityNotification);
                
                return result;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, currentUserId, alert, linkAlertTo, qbicleId, media,  topicName);
                result.msg = ex.Message;
                result.result = false;
                return result;
            }
        }

        public IEnumerable<DateTime> LoadMoreAlerts(int cubeId, int size,
                            ref List<QbicleAlert> alerts, ref int acivitiesDateCount, string currentTimeZone)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Load more alerts", null, null, cubeId, size, alerts, acivitiesDateCount, currentTimeZone);

                IEnumerable<DateTime> activitiesDate;

                var qbiclealerts = this.GetAlertsByQbicleId(cubeId).BusinessMapping(currentTimeZone);
                alerts = qbiclealerts;
                var taskDates = from t in qbiclealerts select t.TimeLineDate.Date;

                var disDates = taskDates;
                acivitiesDateCount = disDates.Count();

                disDates = disDates.Distinct().OrderByDescending(d => d.Date.Date);
                activitiesDate = disDates.OrderByDescending(d => d.Date).Skip(size).Take(HelperClass.qbiclePageSize);

                alerts = qbiclealerts.Where(o => activitiesDate.Contains(o.TimeLineDate.Date)).ToList();
                return activitiesDate;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId, size, alerts, acivitiesDateCount, currentTimeZone);
                return new List<DateTime>();
            }
            
        }

        public List<QbicleAlert> GetAlertsByQbicleId(int cubeId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get alert by qbicle id", null, null, cubeId);

                var qbicleAlert = dbContext.Alerts.Where(c => c.Qbicle.Id == cubeId).ToList();
                return qbicleAlert;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, cubeId);
                return new List<QbicleAlert>();
            }
        }

        public QbicleAlert UpdateAlert(QbicleAlert alert)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Update alert", null, null, alert);

                var alertUpdate = dbContext.Alerts.Find(alert.Id);
                alertUpdate.Content = alert.Content;
                alertUpdate.Priority = alert.Priority;
                alertUpdate.Name = alert.Name;
                dbContext.Entry(alertUpdate).State = EntityState.Modified;
                dbContext.SaveChanges();
                return alertUpdate;
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alert);
                return null;
            }
            
        }

        public QbicleAlert GetAlertById(int alertId)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get alert by id", null, null, alertId);

                return dbContext.Alerts.Find(alertId) ?? new QbicleAlert();
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertId);
                return new QbicleAlert();
            }
        }

        /// <summary>
        /// get list date of Alerts
        /// </summary>
        /// <param name="qbicleAlert">List<QbicleAlert></param>
        /// <returns></returns>
        public IEnumerable<DateTime> GetAlertsDate(List<QbicleAlert> qbicleAlert)
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Get alerts date", null, null, qbicleAlert);

                var eventDates = from t in qbicleAlert select t.TimeLineDate.Date;
                return eventDates.Distinct();
            }
            catch(Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, qbicleAlert);
                return new List<DateTime>();
            }
           
        }


        public bool AddPostToAlert(int alertId, QbiclePost post, string originatingConnectionId = "")
        {
            try
            {
                if (ConfigManager.LoggingDebugSet)
                    LogManager.Debug(MethodBase.GetCurrentMethod(), "Add post to alert", null, null, alertId, post);

                var alert = GetAlertById(alertId);
                alert.TimeLineDate = DateTime.UtcNow;
                alert.Posts.Add(post);
                alert.UpdateReason = QbicleActivity.ActivityUpdateReasonEnum.NewComments;
                alert.Qbicle.LastUpdated = DateTime.UtcNow;
                //var startedBy = alert.StartedBy;
                dbContext.Alerts.Attach(alert);
                dbContext.Entry(alert).State = EntityState.Modified;
                dbContext.SaveChanges();


                var nRule = new NotificationRules(dbContext);

                var activityNotification = new ActivityNotification
                {
                    OriginatingConnectionId = originatingConnectionId,
                    Id = alert.Id,
                    PostId = post.Id,
                    EventNotify = NotificationEventEnum.PostCreation,
                    AppendToPageName = ApplicationPageName.Alert,
                    AppendToPageId = alertId,
                    CreatedById = post.CreatedBy.Id,
                    CreatedByName = post.CreatedBy.GetFullName(),
                    ReminderMinutes = 0
                };
                nRule.NotificationComment2Activity(activityNotification);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Error(MethodBase.GetCurrentMethod(), ex, null, alertId, post);
                return false;
            }

        }

    }
}
